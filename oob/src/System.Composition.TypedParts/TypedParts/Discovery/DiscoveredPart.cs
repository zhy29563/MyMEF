﻿// -----------------------------------------------------------------------
// Copyright © Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Composition.Debugging;
using System.Composition.TypedParts.ActivationFeatures;
using System.Composition.Hosting.Core;
using System.Composition.Runtime;
using System.Composition.Convention;
using System.Composition.Hosting;

namespace System.Composition.TypedParts.Discovery
{
    [DebuggerDisplay("{PartType.Name}")]
    [DebuggerTypeProxy(typeof(DiscoveredPartDebuggerProxy))]
    class DiscoveredPart
    {
        readonly TypeInfo _partType;
        readonly AttributedModelProvider _attributeContext;
        readonly ICollection<DiscoveredExport> _exports = new List<DiscoveredExport>();
        readonly ActivationFeature[] _activationFeatures;
        readonly Lazy<IDictionary<string, object>> _partMetadata;

        // This is unbounded so potentially a source of memory consumption,
        // but in reality unlikely to be a problem.
        readonly IList<Type[]> _appliedArguments = new List<Type[]>();

        // Lazyily initialised among potentially many exports
        ConstructorInfo _constructor;
        CompositeActivator _partActivator;

        static readonly IDictionary<string, object> NoMetadata = new Dictionary<string, object>();
        static readonly MethodInfo ActivatorInvoke = typeof(CompositeActivator).GetTypeInfo().GetDeclaredMethod("Invoke");

        DiscoveredPart(
            TypeInfo partType,
            AttributedModelProvider attributeContext,
            ActivationFeature[] activationFeatures,
            Lazy<IDictionary<string, object>> partMetadata)
        {
            _partType = partType;
            _attributeContext = attributeContext;
            _activationFeatures = activationFeatures;
            _partMetadata = partMetadata;
        }

        public DiscoveredPart(
            TypeInfo partType, 
            AttributedModelProvider attributeContext,
            ActivationFeature[] activationFeatures)
        {
            _partType = partType;
            _attributeContext = attributeContext;
            _activationFeatures = activationFeatures;
            _partMetadata = new Lazy<IDictionary<string, object>>(() => GetPartMetadata(partType));
        }

        public TypeInfo PartType { get { return _partType; } }

        public bool IsShared { get { return ContractHelpers.IsShared(_partMetadata.Value); } }

        public void AddDiscoveredExport(DiscoveredExport export)
        {
            _exports.Add(export);
            export.Part = this;
        }

        public CompositionDependency[] GetDependencies(DependencyAccessor definitionAccessor)
        {
            return GetPartActivatorDependencies(definitionAccessor)
                .Concat(_activationFeatures
                    .SelectMany(feature => feature.GetDependencies(_partType, definitionAccessor)))
                .Where(a => a != null)
                .ToArray();
        }

        IEnumerable<CompositionDependency> GetPartActivatorDependencies(DependencyAccessor definitionAccessor)
        {
            var partTypeAsType = _partType.AsType();

            if (_constructor == null)
            {
                foreach (var c in _partType.DeclaredConstructors.Where(ci => ci.IsPublic && !(ci.IsStatic)))
                {
                    if (_attributeContext.GetDeclaredAttribute<ImportingConstructorAttribute>(partTypeAsType, c) != null)
                    {
                        if (_constructor != null)
                        {
                            var message = string.Format(Properties.Resources.DiscoveredPart_MultipleImportingConstructorsFound, _partType);
                            throw new CompositionFailedException(message);
                        }

                        _constructor = c;
                    }
                }

                if (_constructor == null)
                    _constructor = _partType.DeclaredConstructors
                        .FirstOrDefault(ci => ci.IsPublic && !(ci.IsStatic || ci.GetParameters().Any()));

                if (_constructor == null)
                {
                    var message = string.Format(Properties.Resources.DiscoveredPart_NoImportingConstructorsFound, _partType);
                    throw new CompositionFailedException(message);
                }
            }

            var cps = _constructor.GetParameters();

            for (var i = 0; i < cps.Length; ++i)
            {
                var pi = cps[i];
                var site = new ParameterImportSite(pi);

                var importInfo = ContractHelpers.GetImportInfo(pi.ParameterType, _attributeContext.GetDeclaredAttributes(partTypeAsType, pi), site);
                if (!importInfo.AllowDefault)
                {
                    yield return definitionAccessor.ResolveRequiredDependency(site, importInfo.Contract, true);
                }
                else
                {
                    CompositionDependency optional;
                    if (definitionAccessor.TryResolveOptionalDependency(site, importInfo.Contract, true, out optional))
                        yield return optional;
                }
            }
        }

        public CompositeActivator GetActivator(DependencyAccessor definitionAccessor, IEnumerable<CompositionDependency> dependencies)
        {
            if (_partActivator != null) return _partActivator;

            var contextParam = Expression.Parameter(typeof(LifetimeContext), "cc");
            var operationParm = Expression.Parameter(typeof(CompositionOperation), "op");

            var cps = _constructor.GetParameters();
            Expression[] paramActivatorCalls = new Expression[cps.Length];

            var partActivatorDependencies = dependencies
                .Where(dep => dep.Site is ParameterImportSite)
                .ToDictionary(d => ((ParameterImportSite)d.Site).Parameter);

            for (var i = 0; i < cps.Length; ++i)
            {
                var pi = cps[i];
                CompositionDependency dep;

                if (partActivatorDependencies.TryGetValue(pi, out dep)) 
                {
                    var a = dep.Target.GetDescriptor().Activator;
                    paramActivatorCalls[i] =
                        Expression.Convert(Expression.Call(Expression.Constant(a), ActivatorInvoke, contextParam, operationParm), pi.ParameterType);
                }
                else
                {
                    paramActivatorCalls[i] = Expression.Default(pi.ParameterType);
                }
            }

            Expression body = Expression.Convert(Expression.New(_constructor, paramActivatorCalls), typeof(object));

            var activator = Expression
                .Lambda<CompositeActivator>(body, contextParam, operationParm)
                .Compile();

            foreach (var activationFeature in _activationFeatures)
                activator = activationFeature.RewriteActivator(_partType, activator, _partMetadata.Value, dependencies);

            _partActivator = activator;
            return _partActivator;
        }

        public IDictionary<string, object> GetPartMetadata(TypeInfo partType)
        {
            var partMetadata = new Dictionary<string, object>();
            foreach (var attr in _attributeContext.GetDeclaredAttributes(partType.AsType(), partType))
            {
                if (attr is PartMetadataAttribute)
                {
                    var ma = (PartMetadataAttribute)attr;
                    partMetadata.Add(ma.Name, ma.Value);
                }
            }

            return partMetadata.Count == 0 ? NoMetadata : partMetadata;
        }

        public bool TryCloseGenericPart(Type[] typeArguments, out DiscoveredPart closed)
        {
            if (_appliedArguments.Any(args => Enumerable.SequenceEqual(args, typeArguments)))
            {
                closed = null;
                return false;
            }

            _appliedArguments.Add(typeArguments);

            var closedType = _partType.MakeGenericType(typeArguments).GetTypeInfo();

            var result = new DiscoveredPart(closedType, _attributeContext, _activationFeatures, _partMetadata);

            foreach (var export in _exports)
            {
                var closedExport = export.CloseGenericExport(closedType, typeArguments);
                result.AddDiscoveredExport(closedExport);
            }

            closed = result;
            return true;
        }

        public IEnumerable<DiscoveredExport> DiscoveredExports { get { return _exports; } }
    }
}
