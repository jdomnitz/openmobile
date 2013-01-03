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
#if DEBUG
using System.Diagnostics;
#endif
using System.Collections.Generic;
using OpenMobile.Threading;

namespace OpenMobile
{
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class SandboxedThread
    {
        public static void Asynchronous(Function function)
        {
            SafeThread.Asynchronous(function, Core.theHost);
        }
        public static void Handle(Exception e)
        {
            //string message = e.GetType().ToString() + " (" + e.Message + ")\r\n" + e.StackTrace + "\r\n********";
//            string message = spewException(e);

//            BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, e.Source, message);
//#if DEBUG
//            Debug.Print(message);
//#endif
            // Log exception
            BuiltInComponents.Host.DebugMsg("SandboxedThread exception", e);
            if (e.Source == "OpenMobile")
                return;
            int index = Core.pluginCollection.FindIndex(p => ((p != null) && (p.pluginName == e.Source)));
            IBasePlugin CrashedPlugin = null;
            if (index > -1)
                CrashedPlugin = Core.pluginCollection[index];
            if (CrashedPlugin != null)
            {
                if (Core.status != null)
                    Core.pluginCollection[index] = null;
                else if (Core.RenderingWindows[0] != null)
                    Core.pluginCollection.Remove(CrashedPlugin);

                BuiltInComponents.Host.UIHandler.AddNotification(new Notification(Notification.Styles.Warning, CrashedPlugin, String.Empty, CrashedPlugin.pluginIcon.image, String.Format("Plugin {0} crashed", CrashedPlugin.pluginName), String.Format("Plugin {0} ({1}) by {2} was unloaded.", CrashedPlugin.pluginName, CrashedPlugin.pluginDescription, CrashedPlugin.authorName)));
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "SandboxedThread", String.Format("Plugin {0} crashed! Plugin {1} ({2}) by {3} was unloaded.", CrashedPlugin.pluginName, CrashedPlugin.pluginName, CrashedPlugin.pluginDescription, CrashedPlugin.authorName));
                CrashedPlugin.Dispose();
            }
        }

        private static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }

    }
}
