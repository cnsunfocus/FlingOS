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
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Call : ILOps.Call
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if any argument or the return value is a floating point number.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            MethodBase methodToCall = anILOpInfo.MethodToCall;
            //The method to call is a method base
            //A method base can be either a method info i.e. a normal method
            //or a constructor method. The two types are treated separately.
            if(methodToCall is MethodInfo)
            {
                //Allocate space on the stack for the return value as necessary
                Type retType = ((MethodInfo)methodToCall).ReturnType;
                StackItem returnItem = new StackItem()
                {
                    isFloat = Utils.IsFloat(retType),
                    sizeOnStackInBytes = Utils.GetNumBytesForType(retType)
                };
                //We do not push the return value onto the stack unless it has size > 0
                //We do not push the return value onto our stack at this point - it is pushed after the call is done

                if (returnItem.sizeOnStackInBytes != 0)
                {
                    if (returnItem.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Cannot handle float return values!");
                    }
                    else if (returnItem.sizeOnStackInBytes == 4)
                    {
                        result.AppendLine("push dword 0");
                    }
                    else if (returnItem.sizeOnStackInBytes == 8)
                    {
                        result.AppendLine("push dword 0");
                        result.AppendLine("push dword 0");
                    }
                    else
                    {
                        throw new NotSupportedException("Invalid return stack operand size!");
                    }
                }

                //Get the ID of method to call as it will be labelled in the output ASM.
                string methodID = aScannerState.GetMethodID(methodToCall);
                //Append the actual call
                result.AppendLine(string.Format("call {0}", methodID));

                //After a call, we need to remove the return value and parameters from the stack
                //This is most easily done by just adding the total number of bytes for params and
                //return value to the stack pointer (ESP register).
                
                //Stores the number of bytes to add
                int bytesToAdd = 0;
                //All the parameters for the method that was called
                List<Type> allParams = ((MethodInfo)methodToCall).GetParameters().Select(x => x.ParameterType).ToList();
                //Go through each one
                if (!methodToCall.IsStatic)
                {
                    allParams.Insert(0, methodToCall.DeclaringType);
                }
                foreach (Type aParam in allParams)
                {
                    //Pop the paramter off our stack 
                    //(Note: Return value was never pushed onto our stack. See above)
                    aScannerState.CurrentStackFrame.Stack.Pop();
                    //Add the size of the paramter to the total number of bytes to pop
                    bytesToAdd += Utils.GetNumBytesForType(aParam);
                }
                //If the number of bytes to add to skip over params is > 0
                if (bytesToAdd > 0)
                {
                    //If there is a return value on the stack
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //We need to store the return value then pop all the params

                        //We now push the return value onto our stack as,
                        //after all is said and done below, it will be the 
                        //top item on the stack
                        aScannerState.CurrentStackFrame.Stack.Push(returnItem);

                        //SUPPORT - floats (with above)

                        //Pop the return value into the eax register
                        //We will push it back on after params are skipped over.
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("pop dword eax");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("pop dword eax");
                            result.AppendLine("pop dword edx");
                        }
                    }   
                    //Skip over the params
                    result.AppendLine(string.Format("add esp, {0}", bytesToAdd));
                    //If necessary, push the return value onto the stack.
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //SUPPORT - floats (with above)

                        //The return value was stored in eax
                        //So push it back onto the stack
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("push dword eax");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("push dword edx");
                            result.AppendLine("push dword eax");
                        }
                    }
                }
                //No params to skip over but we might still need to store return value
                else if (returnItem.sizeOnStackInBytes != 0)
                {
                    //The return value will be the top item on the stack.
                    //So all we need to do is push the return item onto our stack.
                    aScannerState.CurrentStackFrame.Stack.Push(returnItem);
                }
            }
            else if(methodToCall is ConstructorInfo)
            {
                ConstructorInfo aConstructor = (ConstructorInfo)methodToCall;
                if (aConstructor.IsStatic)
                {
                    //Static constructors do not have parameters or return values

                    //Get the ID of method to call as it will be labelled in the output ASM.
                    string methodID = aScannerState.GetMethodID(methodToCall);
                    //Append the actual call
                    result.AppendLine(string.Format("call {0}", methodID));
                }
                else
                {
                    //Get the ID of method to call as it will be labelled in the output ASM.
                    string methodID = aScannerState.GetMethodID(methodToCall);
                    //Append the actual call
                    result.AppendLine(string.Format("call {0}", methodID));

                    //After a call, we need to remove the parameters from the stack
                    //This is most easily done by just adding the total number of bytes for params
                    //to the stack pointer (ESP register).

                    //Stores the number of bytes to add
                    int bytesToAdd = 0;
                    //All the parameters for the method that was called
                    ParameterInfo[] allParams = methodToCall.GetParameters();
                    //Go through each one
                    foreach (ParameterInfo aParam in allParams)
                    {
                        //Pop the paramter off our stack 
                        //(Note: Return value was never pushed onto our stack. See above)
                        aScannerState.CurrentStackFrame.Stack.Pop();
                        //Add the size of the paramter to the total number of bytes to pop
                        bytesToAdd += Utils.GetNumBytesForType(aParam.ParameterType);
                    }
                    //Add 4 bytes for the instance ref
                    bytesToAdd += 4;
                    //If the number of bytes to add to skip over params is > 0
                    if (bytesToAdd > 0)
                    {
                        //Skip over the params
                        result.AppendLine(string.Format("add esp, {0}", bytesToAdd));
                    }
                }
            }
            
            return result.ToString().Trim();
        }
    }
}
