﻿// -----------------------------------------------------------------------
// Copyright © Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System.Reflection;
using System.Composition.Hosting.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Composition.Demos.ExtendedCollectionImports.Util;
using System.Composition.Hosting;

namespace Microsoft.Composition.Demos.ExtendedCollectionImports.OrderedCollections
{
    public class OrderedImportManyExportDescriptorProvider : ExportDescriptorProvider
    {
        /// <summary>
        /// Identifies the metadata used to order a "many" import.
        /// </summary>
        const string OrderByMetadataImportMetadataConstraintName = "OrderMetadataName";

        static readonly MethodInfo GetImportManyDefinitionMethod = typeof(OrderedImportManyExportDescriptorProvider).GetTypeInfo().GetDeclaredMethod("GetImportManyDescriptor");
        static readonly Type[] SupportedContractTypes = new[] { typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>) };

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(CompositionContract contract, DependencyAccessor definitionAccessor)
        {
            if (!(contract.ContractType.IsArray ||
                  contract.ContractType.IsConstructedGenericType && SupportedContractTypes.Contains(contract.ContractType.GetGenericTypeDefinition())))
                return NoExportDescriptors;

            string keyToOrderBy;
            CompositionContract orderUnwrappedContract;
            if (!contract.TryUnwrapMetadataConstraint(OrderByMetadataImportMetadataConstraintName, out keyToOrderBy, out orderUnwrappedContract))
                return NoExportDescriptors;

            var elementType = contract.ContractType.IsArray ?
                contract.ContractType.GetElementType() :
                contract.ContractType.GenericTypeArguments[0];

            var elementContract = orderUnwrappedContract.ChangeType(elementType);

            var gimd = GetImportManyDefinitionMethod.MakeGenericMethod(elementType);

            return new[] { (ExportDescriptorPromise)gimd.Invoke(null, new object[] { contract, elementContract, definitionAccessor, keyToOrderBy }) };
        }

        static ExportDescriptorPromise GetImportManyDescriptor<TElement>(CompositionContract importManyContract, CompositionContract elementContract, DependencyAccessor definitionAccessor, string keyToOrderBy)
        {
            return new ExportDescriptorPromise(
                importManyContract,
                typeof(TElement[]).Name,
                false,
                () => definitionAccessor.ResolveDependencies("item", elementContract, true),
                d =>
                {
                    var dependentDescriptors = (keyToOrderBy != null) ?
                        OrderDependentDescriptors(d, keyToOrderBy) :
                        d.Select(el => el.Target.GetDescriptor()).ToArray();

                    return ExportDescriptor.Create((c, o) => dependentDescriptors.Select(e => (TElement)e.Activator(c, o)).ToArray(), NoMetadata);
                });
        }

        static IEnumerable<ExportDescriptor> OrderDependentDescriptors(IEnumerable<CompositionDependency> dependentDescriptors, string keyToOrderBy)
        {
            var targets = dependentDescriptors.Select(d => d.Target).ToArray();
            var missing = targets.Where(t => !t.GetDescriptor().Metadata.ContainsKey(keyToOrderBy) ||
                                             t.GetDescriptor().Metadata[keyToOrderBy] == null).ToArray();
            if (missing.Length != 0)
            {
                var origins = Formatters.ReadableQuotedList(missing.Select(m => m.Origin));
                var message = string.Format("The metadata '{0}' cannot be used for ordering because it is missing from exports on part(s) {1}.", keyToOrderBy, origins);
                throw new CompositionFailedException(message);
            }

            return targets.Select(t => t.GetDescriptor()).OrderBy(d => d.Metadata[keyToOrderBy]).ToArray();
        }
    }
}
