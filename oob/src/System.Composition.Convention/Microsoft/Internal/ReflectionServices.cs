// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Internal
{
    internal static class ReflectionServices
    {
        public static bool IsMemberInfoForType(this MemberInfo memberInfo)
        {
            return memberInfo is TypeInfo;
        }

        public static bool IsMemberInfoForProperty(this MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo;
        }

        public static bool IsMemberInfoForConstructor(this MemberInfo memberInfo)
        {
            return memberInfo is ConstructorInfo;
        }

        public static bool IsMemberInfoForMethod(this MemberInfo memberInfo)
        {
            return memberInfo is MethodInfo;
        }
    }
}
