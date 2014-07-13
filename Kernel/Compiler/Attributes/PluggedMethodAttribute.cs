﻿#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// <para>
    /// Indicates a method is plugged. 
    /// </para>
    /// <para>
    /// Use the ASMFilePath without any file extensions to specify the path 
    /// to the ASM file that contains the plug. The extension is automatically
    /// added based on the target architecture. E.g. "PlugFileName.x86_32.asm".
    /// See remarks for more info.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute must be applied to a method which is plugged to tell
    /// the compiler where the ASM for the plug can be found.
    /// </para>
    /// <para>
    /// The file name should be a relative path, relative to the build directory.
    /// The file name extension is added automatically so different ASM plugs
    /// can be used for different target architectures.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PluggedMethodAttribute : Attribute
    {
        /// <summary>
        /// The relative path to the ASM plug file, excluding extension(s).
        /// </summary>
        /// <remarks>
        /// Please see class remarks for details.
        /// </remarks>
        public string ASMFilePath
        {
            get;
            set;
        }
    }
}
