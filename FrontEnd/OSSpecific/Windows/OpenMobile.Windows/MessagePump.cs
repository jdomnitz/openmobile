/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMobile;
using OpenMobile.Threading;
using System.Windows.Forms;

namespace OpenMobile.Threading
{
    public static class MessagePump
    {
        /// <summary>
        /// Creates a new thread with a windows message pump running on it in addition to any code specified
        /// </summary>
        /// <param name="threadName"></param>
        /// <param name="function"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        static public Thread CreateMessagePump(string threadName, Function function, bool start = true)
        {
            Thread th = new Thread(() =>
            {
                try
                {
                    function();
                    // Start messagepump
                    Application.Run();
                }
                catch (Exception ex)
                {   // Send error to debug log
                    BuiltInComponents.Host.DebugMsg(Thread.CurrentThread.Name, ex);
                }
            });
            th.Name = String.Format("OpenMobile.Threading.MessagePump:{0}", threadName);
            th.IsBackground = true;
            th.Start();
            return th;
        }
    }
}

