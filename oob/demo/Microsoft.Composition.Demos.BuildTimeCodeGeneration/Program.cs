﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildTimeCodeGeneration.Generated;
using BuildTimeCodeGeneration.Parts;
using System.Composition.Hosting;

namespace BuildTimeCodeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cc = CompositionHost.CreateCompositionHost(new BuildTimeCodeGeneration_ExportDescriptorProvider()))
            {
                var rh = cc.GetExport<RequestListener>();
                rh.HandleRequest();
            }

            Console.ReadKey(true);
        }
    }
}
