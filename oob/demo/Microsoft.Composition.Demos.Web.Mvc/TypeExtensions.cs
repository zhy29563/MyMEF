﻿// -----------------------------------------------------------------------
// Copyright © Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Composition.Demos.Web.Mvc
{
    static class TypeExtensions
    {
        public static bool IsInNamespace(this Type type, string namespaceFragment)
        {
            return type.Namespace != null &&
                  (type.Namespace.EndsWith("." + namespaceFragment) || type.Namespace.Contains("." + namespaceFragment + "."));
        }
    }
}
