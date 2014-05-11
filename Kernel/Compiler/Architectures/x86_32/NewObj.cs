﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Newobj : ILOps.Newobj
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            MethodBase constructorMethod = anILOpInfo.MethodToCall;
            Type objectType = constructorMethod.DeclaringType;
            
            //New obj must:
            // - Allocate memory on the heap for the object
            //          - If no memory is left, throw a panic attack because we're out of memory...
            // - Call the specified constructor

            //The label to jump to if allocated memory isn't null
            //i.e. not out of memory.
            string NotNullLabel = string.Format("{0}.IL_{1}_NotNullMem",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    anILOpInfo.Position);
            

            //Attempt to allocate memory on the heap for the new object
            //This involves:
            // - Pushing the type reference onto the stack
            // - Calling GC NewObj method
            // - Check the pointer == 0, if so, out of memory

            //Push type reference
            string typeIdStr = aScannerState.GetTypeIdString(aScannerState.GetTypeID(objectType));
            result.AppendLine(string.Format("push dword {0}", typeIdStr));
            //Push a dword for return value (i.e. new object pointer)
            result.AppendLine("push dword 0");
            //Get the GC.NewObj method ID (i.e. ASM label)
            string methodLabel = aScannerState.GetMethodID(aScannerState.NewObjMethod);
            //Call GC.NewObj
            result.AppendLine(string.Format("call {0}", methodLabel));
            //Pop the return value (i.e. new object pointer)
            result.AppendLine("pop dword eax");
            //Remove arg 0 from stack
            result.AppendLine("add esp, 4");
            //Check if pointer == 0?
            result.AppendLine("cmp eax, 0");
            //If it isn't 0, not out of memory so continue execution
            result.AppendLine(string.Format("jnz {0}", NotNullLabel));
            //If we are out of memory, we have a massive problem
            //Because it means we don't have space to create a new exception object
            //So ultimately we just have to throw a kernel panic
            //Throw a panic attack... ( :/ ) by calling kernel Halt()
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.HaltMethod)));
            //Insert the not null label
            result.AppendLine(NotNullLabel + ":");

            //Call the specified constructor
            //This involves:
            // - Push empty dword onto stack
            // - Move all args down by one dword
            // - Move object reference into dword as first arg
            // - Call constructor
            result.AppendLine("push dword 0");
            int sizeOfArgs = 0;
            ParameterInfo[] allParams = constructorMethod.GetParameters();
            foreach(ParameterInfo aParam in allParams)
            {
                sizeOfArgs += Utils.GetNumBytesForType(aParam.ParameterType);
                aScannerState.CurrentStackFrame.Stack.Pop();
            }
            result.AppendLine("mov dword ebx, esp");
            if (sizeOfArgs > 0)
            {
                if (sizeOfArgs % 4 != 0)
                {
                    throw new InvalidOperationException("sizeOfArgs not exact multiple of 4!");
                }

                result.AppendLine(string.Format("mov dword ecx, {0}", sizeOfArgs / 4));
                string ShiftArgsLoopLabel = string.Format("{0}.IL_{1}_ShiftArgsLoop",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                result.AppendLine(ShiftArgsLoopLabel + ":");
                result.AppendLine("mov dword edx, [ebx+4]");
                result.AppendLine("mov dword [ebx], edx");
                result.AppendLine("add ebx, 4");
                result.AppendLine(string.Format("loop {0}", ShiftArgsLoopLabel));
            }
            result.AppendLine("mov dword [ebx], eax");
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(constructorMethod)));
            //Only remove args from stack - we want the object pointer to remain on the stack
            result.AppendLine(string.Format("add esp, {0}", sizeOfArgs));

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isNewGCObject = true
            });

            return result.ToString().Trim();
        }
    }
}