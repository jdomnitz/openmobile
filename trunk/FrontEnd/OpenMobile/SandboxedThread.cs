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
using System.Diagnostics;

namespace OpenMobile
{
    public delegate void Function();
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class SandboxedThread
    {
        public static void Asynchronous(Function function)
        {
            new Thread(()=>
                {
                    try
                    {
                        function();
                    }
                    catch (Exception e)
                    {
                        handle(e);
                    }
                }).Start();
        }

        public static void handle(Exception e)
        {
            string message = e.GetType().ToString() + "(" + e.Message + ")\r\n\r\n" + e.StackTrace + "\r\n********";
            Core.theHost.sendMessage("OMDebug", e.Source, message);
            Debug.Print(message);
            int index = Core.pluginCollection.FindIndex(p => p.pluginName == e.Source);
            IBasePlugin sample=null;
            if (index>-1)
                sample = Core.pluginCollection[index];
            if (sample != null)
            {
                if (Core.status != null)
                    Core.pluginCollection[index] = null;
                else if (Core.RenderingWindows[0]!=null)
                    Core.pluginCollection.Remove(sample);
                Core.theHost.raiseSystemEvent(eFunction.backgroundOperationStatus, sample.pluginName + " CRASHED!", "ERROR", "");
                sample.Dispose();
            }
        }
    }
}
