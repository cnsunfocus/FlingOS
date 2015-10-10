﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
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
using Kernel.Core.Processes;
using Kernel.FOS_System;

namespace Kernel.Core.Pipes.Standard
{
    public class StandardInpoint : BasicInpoint
    {
        protected byte[] ReadBuffer;

        public StandardInpoint(uint aOutProcessId, bool OutputPipe)
            : base(aOutProcessId, PipeClasses.Standard, (OutputPipe ? PipeSubclasses.Standard_Out : PipeSubclasses.Standard_In), 800)
        {
            ReadBuffer = new byte[BufferSize];
        }

        public unsafe FOS_System.String Read()
        {
            int bytesRead = base.Read(ReadBuffer, 0, ReadBuffer.Length);
            if (bytesRead > 0)
            {
                return ByteConverter.GetASCIIStringFromASCII(ReadBuffer, 0, (uint)bytesRead);
            }
            else
            {
                return "";
            }
        }
    }
}
