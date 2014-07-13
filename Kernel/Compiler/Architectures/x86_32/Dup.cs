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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Dup : ILOps.Dup
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// If either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop item to duplicate
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();

            if(itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Duplicate float vals not suppported yet!");
            }
            
            if(itemA.sizeOnStackInBytes == 4)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("push dword eax");
                result.AppendLine("push dword eax");
            }
            else if (itemA.sizeOnStackInBytes == 8)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("pop dword edx");
                result.AppendLine("push dword edx");
                result.AppendLine("push dword eax");
                result.AppendLine("push dword edx");
                result.AppendLine("push dword eax");
            }
            else
            {
                throw new NotSupportedException("Stack item size not supported by duplicate op!");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes
            });
            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes
            });

            return result.ToString().Trim();
        }
    }
}
