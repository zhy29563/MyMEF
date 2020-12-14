//------------------------------------------------------------------------------
// <copyright from='1997' to='2001' company='Microsoft Corporation'>           
//    Copyright (c) Microsoft Corporation. All Rights Reserved.                
//    Information Contained Herein is Proprietary and Confidential.            
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Reflection.Context {
    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Resources;
    using System.Text;
    using System.Threading;
    using System.Security.Permissions;

    

    /// <summary>
    ///    AutoGenerated resource class. Usage:
    ///
    ///        string s = SR.GetString(SR.MyIdenfitier);
    /// </summary>
    
    internal sealed class SR {
        internal const string ArgumentNull_GetterOrSetterMustBeSpecified = "ArgumentNull_GetterOrSetterMustBeSpecified";
        internal const string Argument_GetMethNotFnd = "Argument_GetMethNotFnd";
        internal const string Argument_SetMethNotFnd = "Argument_SetMethNotFnd";
        internal const string Argument_PropertyTypeFromDifferentContext = "Argument_PropertyTypeFromDifferentContext";
        internal const string Format_AttributeUsage = "Format_AttributeUsage";
        internal const string InvalidOperation_AddNullProperty = "InvalidOperation_AddNullProperty";
        internal const string InvalidOperation_AddPropertyDifferentContext = "InvalidOperation_AddPropertyDifferentContext";
        internal const string InvalidOperation_AddPropertyDifferentType = "InvalidOperation_AddPropertyDifferentType";
        internal const string InvalidOperation_NullAttribute = "InvalidOperation_NullAttribute";
        internal const string InvalidOperation_InvalidMemberType = "InvalidOperation_InvalidMemberType";
        internal const string InvalidOperation_InvalidMethodType = "InvalidOperation_InvalidMethodType";
        internal const string InvalidOperation_NotGenericMethodDefinition = "InvalidOperation_NotGenericMethodDefinition";
        internal const string InvalidOperation_EnumLitValueNotFound = "InvalidOperation_EnumLitValueNotFound";
        internal const string Target_InstanceMethodRequiresTarget = "Target_InstanceMethodRequiresTarget";
        internal const string Target_ObjectTargetMismatch = "Target_ObjectTargetMismatch";
        internal const string Argument_ObjectArgumentMismatch = "Argument_ObjectArgumentMismatch";

        static SR loader = null;
        ResourceManager resources;

        internal SR() {
            resources = new System.Resources.ResourceManager("System.Reflection.Context.Strings", this.GetType().Assembly);
        }
        
        private static SR GetLoader() {
            if (loader == null) {
                SR sr = new SR();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }
        
        public static ResourceManager Resources {
            get {
                return GetLoader().resources;
            }
        }
        
        public static string GetString(string name, params object[] args) {
            SR sys = GetLoader();
            if (sys == null)
                return null;
            string res = sys.resources.GetString(name, SR.Culture);

            if (args != null && args.Length > 0) {
                for (int i = 0; i < args.Length; i ++) {
                    String value = args[i] as String;
                    if (value != null && value.Length > 1024) {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else {
                return res;
            }
        }

        public static string GetString(string name) {
            SR sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetString(name, SR.Culture);
        }
        
        public static string GetString(string name, out bool usedFallback) {
            // always false for this version of gensr
            usedFallback = false;
            return GetString(name);
        }

        public static object GetObject(string name) {
            SR sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetObject(name, SR.Culture);
        }
    }
}
