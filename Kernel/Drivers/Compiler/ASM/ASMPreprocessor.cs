﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    public static class ASMPreprocessor
    {
        public static CompileResult Preprocess(ASMLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ASMPreprocessed)
            {
                return result;
            }
            TheLibrary.ASMPreprocessed = true;

            foreach (ASMBlock aBlock in TheLibrary.ASMBlocks)
            {
                Preprocess(aBlock);
            }

            return result;
        }

        private static void Preprocess(ASMBlock theBlock)
        {
            // Due to "insert 0", asm ops are constructed in reverse order here

            theBlock.ASMOps.Insert(0, new ASMLabel() { MethodLabel = true });
            theBlock.ASMOps.Insert(0, new ASMGeneric() { Text = "SECTION .text" });
            string currMethodLabel = theBlock.GenerateMethodLabel();
            theBlock.ASMOps.Insert(0, new ASMGlobalLabel() { Label = currMethodLabel });

            foreach (string anExternalLabel in theBlock.ExternalLabels.Distinct())
            {
                if (anExternalLabel != currMethodLabel)
                {
                    theBlock.ASMOps.Insert(0, new ASMExternalLabel() { Label = anExternalLabel });
                }
            }

            theBlock.ASMOps.Insert(0, new ASMGeneric() { Text = "BITS 32" });
        }
    }
}
