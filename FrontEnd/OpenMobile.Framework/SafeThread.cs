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
using System.Threading;
using OpenMobile.Plugin;

namespace OpenMobile.Threading
{
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class SafeThread
    {
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Function function,IPluginHost host)
        {
            new Thread(delegate()
                {
                    try
                    {
                        function();
                    }
                    catch (Exception e)
                    {
                        if (host != null)
                            host.sendMessage("SandboxedThread", "SafeThread", "", ref e);
                    }
                }).Start();
        }
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Delegate function,object[] args, IPluginHost host)
        {
            new Thread(delegate()
            {
                try
                {
                    function.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    if (host != null)
                        host.sendMessage("SandboxedThread", "SafeThread", "", ref e);
                }
            }).Start();
        }
    }
}