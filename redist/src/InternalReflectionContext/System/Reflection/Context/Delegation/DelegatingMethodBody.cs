﻿// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Reflection.Context.Delegation
{
    internal class DelegatingMethodBody : MethodBody
    {
        private readonly MethodBody _body;

        public DelegatingMethodBody(MethodBody body)
        {
            Contract.Requires(null != body);

            _body = body;
        }

        public override IList<ExceptionHandlingClause> ExceptionHandlingClauses
        {
            get { return _body.ExceptionHandlingClauses; }
        }

        public override bool InitLocals 
        {
            get { return _body.InitLocals; }
        }

        public override int LocalSignatureMetadataToken 
        {
            get { return _body.LocalSignatureMetadataToken; }
        }

        public override IList<LocalVariableInfo> LocalVariables
        {
            get { return _body.LocalVariables; }
        }

        public override int MaxStackSize
        {
            get { return _body.MaxStackSize; }
        }

        public override byte[] GetILAsByteArray()
        {
            return _body.GetILAsByteArray();
        }

        public override string ToString()
        {
            return _body.ToString();
        }
    }
}
