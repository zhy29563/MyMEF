// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.UnitTesting;
using System.UnitTesting;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.ComponentModel.Composition.ReflectionModel;
using Microsoft.Internal;

namespace Tests.Integration
{
    [TestClass]
    public class DelayLoadingTests
    {
        [TestMethod]
        public void PartTypeLoadedLazily()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(ExportingPart));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IExporter> lazyContract = container.GetExport<IExporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IExporter value = lazyContract.Value;

            catalog.AssertLoaded(typeof(ExportingPart));
            Assert.AreEqual(1, catalog.LoadedTypes.Count());
        }

        [TestMethod]
        public void PartTypeLoadedLazilyEagerDependeciesLoadEagerly()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(ExportingPart), typeof(PartImportingEagerly));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            catalog.AssertNotLoaded(typeof(PartImportingEagerly));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IImporter> lazyContract = container.GetExport<IImporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(PartImportingEagerly));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IImporter value = lazyContract.Value;
            catalog.AssertLoaded(typeof(PartImportingEagerly));
            catalog.AssertLoaded(typeof(ExportingPart));
            Assert.AreEqual(2, catalog.LoadedTypes.Count());
        }

        [TestMethod]
        public void PartTypeLoadedLazilyLazyDependeciesLoadLazily()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(ExportingPart), typeof(PartImportingLazily));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            catalog.AssertNotLoaded(typeof(PartImportingLazily));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IImporter> lazyContract = container.GetExport<IImporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(PartImportingLazily));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IImporter value = lazyContract.Value;
            catalog.AssertLoaded(typeof(PartImportingLazily));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(1, catalog.LoadedTypes.Count());
        }

        [TestMethod]
        public void PartTypeLoadedLazilyEagerCollectionDependeciesLoadEagerly()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(ExportingPart), typeof(PartImportingCollectionEagerly));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            catalog.AssertNotLoaded(typeof(PartImportingCollectionEagerly));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IImporter> lazyContract = container.GetExport<IImporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(PartImportingCollectionEagerly));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IImporter value = lazyContract.Value;
            catalog.AssertLoaded(typeof(PartImportingCollectionEagerly));
            catalog.AssertLoaded(typeof(ExportingPart));
            Assert.AreEqual(2, catalog.LoadedTypes.Count());
        }

        [TestMethod]
        public void PartTypeLoadedLazilyLazyCollectionDependeciesLoadLazily()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(ExportingPart), typeof(PartImportingCollectionLazily));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(ExportingPart));
            catalog.AssertNotLoaded(typeof(PartImportingCollectionLazily));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IImporter> lazyContract = container.GetExport<IImporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(PartImportingCollectionLazily));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IImporter value = lazyContract.Value;
            catalog.AssertLoaded(typeof(PartImportingCollectionLazily));
            catalog.AssertNotLoaded(typeof(ExportingPart));
            Assert.AreEqual(1, catalog.LoadedTypes.Count());
        }


        [TestMethod]
        public void PartTypeLoadedLazilyLazyLoopLoadsLazily()
        {
            var catalog = new TypeLoadNotifyingCatalog(typeof(LazyLoopImporter), typeof(LazyLoopExporter));
            var container = new CompositionContainer(catalog);
            catalog.AssertNotLoaded(typeof(LazyLoopImporter));
            catalog.AssertNotLoaded(typeof(LazyLoopExporter));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            Lazy<IImporter> lazyContract = container.GetExport<IImporter>();
            Assert.IsNotNull(lazyContract);
            catalog.AssertNotLoaded(typeof(LazyLoopImporter));
            catalog.AssertNotLoaded(typeof(LazyLoopExporter));
            Assert.AreEqual(0, catalog.LoadedTypes.Count());

            IImporter value = lazyContract.Value;
            catalog.AssertLoaded(typeof(LazyLoopImporter));
            catalog.AssertNotLoaded(typeof(LazyLoopExporter));
            Assert.AreEqual(1, catalog.LoadedTypes.Count());
        }

        public class IExporter
        {
        }

        public class IImporter
        {
        }

        [Export(typeof(IExporter))]
        public class ExportingPart : IExporter
        {
        }

        [Export(typeof(IImporter))]
        public class PartImportingLazily : IImporter
        {
            [Import]
            public Lazy<IExporter> Exporter { get; set; }
        }

        [Export(typeof(IImporter))]
        public class PartImportingCollectionLazily : IImporter
        {
            [ImportMany]
            public IEnumerable<Lazy<IExporter>> Exporters { get; set; }
        }

        [Export(typeof(IImporter))]
        public class PartImportingEagerly : IImporter
        {
            [Import]
            public IExporter Exporter { get; set; }
        }

        [Export(typeof(IImporter))]
        public class PartImportingCollectionEagerly : IImporter
        {
            [ImportMany]
            public IEnumerable<IExporter> Exporters { get; set; }
        }


        [Export(typeof(IImporter))]
        public class LazyLoopImporter : IImporter
        {
            [Import]
            public Lazy<IExporter> Exporter { get; set; }
        }

        [Export(typeof(IExporter))]
        public class LazyLoopExporter : IExporter
        {
            [Import]
            public Lazy<IImporter> Importer { get; set; }
        }
       

        private class TypeLoadNotifyingCatalog : ComposablePartCatalog
        {
            ComposablePartDefinition[] _definitions;
            public HashSet<Type> LoadedTypes { get; private set; }

            public TypeLoadNotifyingCatalog(params Type[] types)
            {
                this._definitions = types.Select(type => this.CreatePartDefinition(type)).ToArray();
                this.LoadedTypes = new HashSet<Type>();
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { return this._definitions.AsQueryable(); }
            }

            private ComposablePartDefinition CreatePartDefinition(Type type)
            {
                ComposablePartDefinition partDefinition = AttributedModelServices.CreatePartDefinition(type, null);
                return this.CreateWrapped(partDefinition, type);
            }

            private ComposablePartDefinition CreateWrapped(ComposablePartDefinition partDefinition, Type type)
            {
                IEnumerable<ExportDefinition> exports = partDefinition.ExportDefinitions.Select(e => this.CreateWrapped(e, type)).ToArray();
                IEnumerable<ImportDefinition> imports = partDefinition.ImportDefinitions.Cast<ContractBasedImportDefinition>().Select(i => this.CreateWrapped(i, type)).ToArray();

                return ReflectionModelServices.CreatePartDefinition(
                    this.CreateWrapped(ReflectionModelServices.GetPartType(partDefinition), type),
                    ReflectionModelServices.IsDisposalRequired(partDefinition),
                    imports.AsLazy(),
                    exports.AsLazy(),
                    partDefinition.Metadata.AsLazy(),
                    null);
            }

            private Lazy<T> CreateWrapped<T>(Lazy<T> lazy, Type type)
            {
                return new Lazy<T>(
                    () => { this.OnTypeLoaded(type); return lazy.Value; });
            }

            private LazyMemberInfo CreateWrapped(LazyMemberInfo lazyMember, Type type)
            {
                return new LazyMemberInfo(
                    lazyMember.MemberType,
                    () => { this.OnTypeLoaded(type); return lazyMember.GetAccessors(); });
            }

            private ExportDefinition CreateWrapped(ExportDefinition export, Type type)
            {
                return ReflectionModelServices.CreateExportDefinition(
                    this.CreateWrapped(ReflectionModelServices.GetExportingMember(export), type),
                    export.ContractName,
                    export.Metadata.AsLazy(),
                    null);
            }

            private ImportDefinition CreateWrapped(ContractBasedImportDefinition import, Type type)
            {
                if (ReflectionModelServices.IsImportingParameter(import))
                {
                    return ReflectionModelServices.CreateImportDefinition(
                        this.CreateWrapped(ReflectionModelServices.GetImportingParameter(import), type),
                        import.ContractName,
                        import.RequiredTypeIdentity,
                        import.RequiredMetadata,
                        import.Cardinality,
                        import.RequiredCreationPolicy,
                        null);
                }
                else
                {
                    return ReflectionModelServices.CreateImportDefinition(
                        this.CreateWrapped(ReflectionModelServices.GetImportingMember(import), type),
                        import.ContractName,
                        import.RequiredTypeIdentity,
                        import.RequiredMetadata,
                        import.Cardinality,
                        import.IsRecomposable,
                        import.RequiredCreationPolicy,
                        null);
                }
            }



            private void OnTypeLoaded(Type type)
            {
                this.LoadedTypes.Add(type);
            }

            public void AssertLoaded(Type type)
            {
                Assert.IsTrue(this.LoadedTypes.Contains(type));
            }

            public void AssertNotLoaded(Type type)
            {
                Assert.IsFalse(this.LoadedTypes.Contains(type));
            }

        }
    }
}
