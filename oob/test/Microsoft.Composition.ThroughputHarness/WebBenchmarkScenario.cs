﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompositionThroughput.Web
{
    static class Boundaries
    {
        public const string Web = "Web";
    }

    [Export, Shared]
    public class GlobalA
    {
    }

    [Export, Shared]
    public class GlobalB
    {
    }

    [Export, Shared(Boundaries.Web)]
    public class A
    {
        GlobalA _ga;

        [ImportingConstructor]
        public A(GlobalA ga) { _ga = ga; }

        public GlobalA GA { get { return _ga; } }
    }

    [Export, Shared(Boundaries.Web)]
    public class B
    {
        GlobalB _gb;

        [ImportingConstructor]
        public B(GlobalB gb) { _gb = gb; }

        public GlobalB GB { get { return _gb; } }
    }

    [Export]
    public class Transient
    {
    }

    [Export, Shared(Boundaries.Web)]
    public class Wide
    {
        A _a1, _a2; B _b; Transient _t;

        [ImportingConstructor]
        public Wide(A a1, A a2, B b, Transient t) { _a1 = a1; _a2 = a2; _b = b; _t = t; }

        public A A1 { get { return _a1; } }

        public A A2 { get { return _a2; } }

        public B B { get { return _b; } }

        public Transient T { get { return _t; } }
    }

    public class Disposable : IDisposable
    {
        public bool IsDisposed { get; set; }
        public void Dispose() { IsDisposed = true; }
    }

    [Export, Shared(Boundaries.Web)]
    public class TailA : Disposable
    {
        TailB _b;

        [ImportingConstructor]
        public TailA(TailB b) { _b = b; }

        public TailB TailB { get { return _b; } }
    }

    [Export, Shared(Boundaries.Web)]
    public class TailB : Disposable
    {
        TailC _c;

        [ImportingConstructor]
        public TailB(TailC c) { _c = c; }

        public TailC TailC { get { return _c; } }
    }

    [Export, Shared(Boundaries.Web)]
    public class TailC : Disposable
    {
    }

    [Export, Shared(Boundaries.Web)]
    public class Long
    {
        TailA _a;

        [ImportingConstructor]
        public Long(TailA a) { _a = a; }

        public TailA TailA { get { return _a; } }
    }

    [Export, Shared(Boundaries.Web)]
    public class OperationRoot
    {
        Wide _w; Long _l;

        [ImportingConstructor]
        public OperationRoot(Wide w, Long l) { _w = w; _l = l; }

        public Long Long { get { return _l; } }

        public Wide Wide { get { return _w; } }
    }
}

