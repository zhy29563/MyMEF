﻿// -----------------------------------------------------------------------
// Copyright © Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Composition.Hosting.Providers.CurrentScope;
using System.Composition.Hosting.Providers.ExportFactory;
using System.Composition.Hosting.Providers.ImportMany;
using System.Composition.Hosting.Providers.Lazy;
using System.Composition.Hosting.Providers.Metadata;
using System.Composition.Runtime;
using System.Linq;

using Microsoft.Internal;

namespace System.Composition.Hosting
{
    /// <summary>
    /// Assembles a lightweight composition container from configured
    /// providers.
    /// </summary>
    public sealed class CompositionHost : CompositionContext, IDisposable
    {
        static readonly string[] NoBoundaries = new string[0];

        readonly LifetimeContext _rootLifetimeContext;

        CompositionHost(LifetimeContext rootLifetimeContext)
        {
            Requires.ArgumentNotNull(rootLifetimeContext, "rootLifetimeContext");

            _rootLifetimeContext = rootLifetimeContext;
        }

        /// <summary>
        /// Create the composition host.
        /// </summary>
        /// <returns>The container as an <see cref="CompositionHost"/>.</returns>
        public static CompositionHost CreateCompositionHost(params ExportDescriptorProvider[] providers)
        {
            return CreateCompositionHost((IEnumerable<ExportDescriptorProvider>)providers);
        }

        /// <summary>
        /// Create the composition host.
        /// </summary>
        /// <returns>The container as an <see cref="CompositionHost"/>.</returns>
        public static CompositionHost CreateCompositionHost(IEnumerable<ExportDescriptorProvider> providers)
        {
            Requires.ArgumentNotNull(providers, "providers");

            var allProviders = new ExportDescriptorProvider[] {
                new LazyExportDescriptorProvider(),
                new ExportFactoryExportDescriptorProvider(),
                new ImportManyExportDescriptorProvider(),
                new LazyWithMetadataExportDescriptorProvider(),
                new CurrentScopeExportDescriptorProvider(),
                new ExportFactoryWithMetadataExportDescriptorProvider()
            }
            .Concat(providers)
            .ToArray();

            var container = new LifetimeContext(new ExportDescriptorRegistry(allProviders), NoBoundaries);
            return new CompositionHost(container);
        }

        /// <summary>
        /// Retrieve the single <paramref name="contract"/> instance from the
        /// <see cref="CompositionContext"/>.
        /// </summary>
        /// <param name="contract">The contract to retrieve.</param>
        /// <returns>An instance of the export.</returns>
        /// <param name="export">The export if available, otherwise, null.</param>
        /// <exception cref="CompositionFailedException" />
        public override bool TryGetExport(CompositionContract contract, out object export)
        {
            return _rootLifetimeContext.TryGetExport(contract, out export);
        }
    
        /// <summary>
        /// Release the host and any globally-shared parts.
        /// </summary>
        public void Dispose()
        {
            _rootLifetimeContext.Dispose();
        }
    }
}
