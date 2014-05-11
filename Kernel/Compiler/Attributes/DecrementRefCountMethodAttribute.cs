﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates the method is the kernel's GC.DecrementRefCount method. 
    /// Note: There should only ever be one of these used!
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public class DecrementRefCountMethodAttribute : Attribute
    {
    }
}