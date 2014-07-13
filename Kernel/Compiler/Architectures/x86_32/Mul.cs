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
    public class Mul : ILOps.Mul
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if either or both values to multiply are floating point values or
        /// if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either or both values to multiply are not 4 or 8 bytes
        /// in size or if the values are of different size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop in reverse order to push
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            else if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Divide floats is unsupported!");
            }
            else
            {
                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    result.AppendLine("pop dword ebx");
                    //Pop item A
                    result.AppendLine("pop dword eax");
                    //Sign extend A to EAX:EDX
                    result.AppendLine("cdq");
                    //Do the division
                    result.AppendLine("imul ebx");
                    //Result stored in eax
                    result.AppendLine("push dword eax");

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4
                    });
                }
                else if ((itemA.sizeOnStackInBytes == 8 &&
                          itemB.sizeOnStackInBytes == 4) || 
                         (itemA.sizeOnStackInBytes == 4 &&
                          itemB.sizeOnStackInBytes == 8))
                {
                    throw new InvalidOperationException("Invalid stack operand sizes! They should be the 32-32 or 64-64.");
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //SUPPORT - Support 64-bit multiplication

                    //A = item a, B = item B
                    //L = low bits, H = high bits
                    // => A = AL + AH, B = BL + BH
                    
                    // A * B = (AL + AH) * (BL + BH)
                    //       = (AL * BL) + (AL * BH) + (AH * BL) (Ignore: + (AH * BH))

                    // AH = [ESP+12]
                    // AL = [ESP+8]
                    // BH = [ESP+4]
                    // BL = [ESP+0]

                    // mov eax, 0        - Zero out registers
                    result.AppendLine("mov eax, 0");
                    // mov ebx, 0
                    result.AppendLine("mov ebx, 0");
                    // mov ecx, 0
                    result.AppendLine("mov ecx, 0");
                    // mov edx, 0
                    result.AppendLine("mov edx, 0");

                    // mov eax, [ESP+0]  - Load BL
                    result.AppendLine("mov eax, [ESP+0]");
                    // mov ebx, [ESP+8] - Load AL
                    result.AppendLine("mov ebx, [ESP+8]");
                    // mul ebx           - BL * AL, result in eax:edx
                    result.AppendLine("mul ebx");
                    // push edx          - Push result keeping high bits
                    result.AppendLine("push edx");
                    // push eax
                    result.AppendLine("push eax");

                    //                   - Add 8 to offsets for result(s)

                    // mov eax, 0        - Zero out registers
                    result.AppendLine("mov eax, 0");
                    // mov edx, 0
                    result.AppendLine("mov edx, 0");
                    // mov eax [ESP+4+8] - Load BH
                    result.AppendLine("mov eax, [ESP+12]");
                    // mul ebx           - BH * AL, result in eax:edx
                    result.AppendLine("mul ebx");
                    // push eax          - Push result truncating high bits
                    result.AppendLine("push eax");

                    //                   - Add 12 to offsets for result(s)

                    // mov eax, 0        - Zero out registers
                    result.AppendLine("mov eax, 0");
                    // mov edx, 0
                    result.AppendLine("mov edx, 0");
                    // mov eax, [ESP+0+12] - Load BL
                    result.AppendLine("mov eax, [ESP+12]");
                    // mov ebx, [ESP+12+12] - Load AH
                    result.AppendLine("mov ebx, [ESP+24]");
                    // mul ebx             - BL * AH, result in eax:edx
                    result.AppendLine("mul ebx");
                    // push eax            - Push result truncating high bits
                    result.AppendLine("push eax");

                    //                     - Add 16 to offsets for result(s)
                    
                    // AL * BL = [ESP+8] , 64 bits
                    // AL * BH = [ESP+4] , 32 bits - high bits
                    // AH * BL = [ESP+0] , 32 bits - high bits
                    
                    // mov eax, [ESP+8]  - Load AL * BL
                    result.AppendLine("mov eax, [ESP+8]");
                    // mov edx, [ESP+12]
                    result.AppendLine("mov edx, [ESP+12]");
                    // mov ebx, 0
                    result.AppendLine("mov ebx, 0");
                    // mov ecx, [ESP+4]   - Load AL * BH
                    result.AppendLine("mov ecx, [ESP+4]");
                    // add edx, ecx       - Add (AL * BL) + (AL * BH), result in eax:edx
                    result.AppendLine("add edx, ecx");
                    // mov ecx, [ESP+0]   - Load AH * BL
                    result.AppendLine("mov ecx, [ESP+0]");
                    // add edx, ecx       - Add ((AL * BL) + (AL * BH)) + (AH * BL), result in eax:edx
                    result.AppendLine("add edx, ecx");
                    
                    // add esp, 16+16     - Remove temp results and input values from stack
                    result.AppendLine("add ESP, 32");

                    // push edx           - Push final result
                    result.AppendLine("push edx");
                    // push eax
                    result.AppendLine("push eax");

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        isNewGCObject = false,
                        sizeOnStackInBytes = 8
                    });

                    //throw new NotSupportedException("64-bit by 64-bit multiplication not supported yet!.");
                }
            }

            return result.ToString().Trim();
        }
    }
}
