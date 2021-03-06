using System;
using System.Collections.Generic;
using System.Composition.Convention.UnitTests;
using System.Linq;
using System.Text;
using System.Reflection;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace System.Composition.Convention
{
    public interface IContract1 { }

    public interface IContract2 { }

    public class ClassWithLifetimeConcerns : IContract1, IContract2, IDisposable
    {
        public void Dispose()
        {
        }

        public void OnImportsSatisfied()
        {
        }
    }

    static class DELETE_ME_TESTER
    {
        public static PartConventionBuilder ExportInterfaces(this PartConventionBuilder pb) { return null; }
    }

    [TestClass]
    public class ExportInterfacesContractExclusionTests
    {
        static readonly Type[] ContractInterfaces = new[] { typeof(IContract1), typeof(IContract2) };

        [TestMethod]
        public void WhenExportingInterfaces_NoPredicate_OnlyContractInterfacesAreExported()
        {
            var builder = new ConventionBuilder();
            builder.ForType<ClassWithLifetimeConcerns>().ExportInterfaces();

            var attributes = GetExportAttributes(builder, typeof(ClassWithLifetimeConcerns));
            var exportedContracts = attributes.Select(e => e.ContractType).ToArray();
            CollectionAssert.AreEquivalent(ContractInterfaces, exportedContracts);
        }

        [TestMethod]
        public void WhenExportingInterfaces_PredicateSpecified_OnlyContractInterfacesAreSeenByThePredicate()
        {
            var seenInterfaces = new List<Type>();

            var builder = new ConventionBuilder();
            builder.ForType<ClassWithLifetimeConcerns>().ExportInterfaces(i => { seenInterfaces.Add(i); return true; });

            var attributes = GetExportAttributes(builder, typeof(ClassWithLifetimeConcerns));
            CollectionAssert.AreEquivalent(ContractInterfaces, seenInterfaces);
        }

        private static IEnumerable<ExportAttribute> GetExportAttributes(ConventionBuilder builder, Type type)
        {
            var list = builder.GetDeclaredAttributes(type, type.GetTypeInfo());
            return list.Cast <ExportAttribute>();
        }
    }
}
