﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Hosting.Providers;
using System.Composition.Convention;
using System.Reflection;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace System.Composition.UnitTests
{
    public class ContainerTests
    {
        protected static CompositionContext CreateContainer(params Type[] types)
        {
            return new ContainerConfiguration()
                .WithParts(types)
                .CreateContainer();
        }

        protected static CompositionContext CreateContainer(ConventionBuilder rb, params Type[] types)
        {
            return new ContainerConfiguration()
                .WithParts(types)
                .WithDefaultConventions(rb)
                .CreateContainer();
        }
    }
}
