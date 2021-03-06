﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace System.Composition.UnitTests
{
    [TestClass]
    public class OptionalImportTests : ContainerTests
    {
        class Missing { }

        [Export]
        class Supplied { }

        [Export]
        class HasOptionalConstructorParameter
        {
            public Missing Missing { get; set; }
            public Supplied Supplied { get; set; }

            [ImportingConstructor]
            public HasOptionalConstructorParameter([Import(AllowDefault = true)] Missing missing, Supplied supplied)
            {
                Missing = missing;
                Supplied = supplied;
            }
        }

        [Export]
        class HasOptionalProperty
        {
            Missing _missing;

            public bool WasMissingSetterCalled { get; set; }

            [Import(AllowDefault=true)]
            public Missing Missing
            {
                get { return _missing; }
                set
                { 
                    WasMissingSetterCalled = true;
                    _missing = value;
                }
            }

            [Import]
            public Supplied Supplied { get; set; }
        }

        [TestMethod]
        public void MissingOptionalConstructorParametersAreSuppliedTheirDefaultValue()
        {
            var cc = CreateContainer(typeof(Supplied), typeof(HasOptionalConstructorParameter));
            var ocp = cc.GetExport<HasOptionalConstructorParameter>();
            Assert.IsNull(ocp.Missing);
            Assert.IsNotNull(ocp.Supplied);
        }

        [TestMethod]
        public void MissingOptionalPropertyImportsAreIgnored()
        {
            var cc = CreateContainer(typeof(Supplied), typeof(HasOptionalProperty));
            var op = cc.GetExport<HasOptionalProperty>();
            Assert.IsNotNull(op.Supplied);
            Assert.IsNull(op.Missing);
            Assert.IsFalse(op.WasMissingSetterCalled);
        }
    }
}
