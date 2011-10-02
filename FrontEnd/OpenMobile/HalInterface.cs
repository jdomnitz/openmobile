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
using System.Net;
using System.Text;
using OpenMobile.Input;
using System.Reflection;

namespace OpenMobile
{
    public sealed class HalInterface
    {
        System.Net.Sockets.UdpClient receive;
        System.Net.Sockets.UdpClient send;
        public string[] volume;
        public string[] brightness;
        private bool isDisposed;
        public HalInterface()
        {
#if WINDOWS
            if (Configuration.RunningOnWindows)
            {
                if (!OpenMobile.Framework.OSSpecific.runManagedProcess(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", String.Empty)), "OMHal.exe"), String.Empty, false))
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "HalInterface", "Unable to start HAL!");

            }
#endif
#if LINUX
#if WINDOWS
            else
#endif
                if (Configuration.RunningOnLinux)
                {
                    if (!OpenMobile.Framework.OSSpecific.runManagedProcess(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:", String.Empty)), "OMHal.exe"), String.Empty, false))
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "HalInterface", "Unable to start HAL!");
                }
#endif
            receive = new System.Net.Sockets.UdpClient(8550);
            receive.BeginReceive(recv, null);
            send = new System.Net.Sockets.UdpClient("127.0.0.1", 8549);
        }
        public void snd(string text)
        {
            try
            {
                if (!isDisposed)
                    send.Send(ASCIIEncoding.ASCII.GetBytes(text), text.Length);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "HalInterface", "Exception: " + e.Message);
            }
        }
        void parse(string message)
        {
            string[] parts = message.Split(new char[] { '|' });
            int i = int.Parse(parts[0]);
            string arg1 = String.Empty, arg2 = String.Empty, arg3 = String.Empty;
            if (parts.Length > 3)
                arg3 = parts[3];
            if (parts.Length > 2)
                arg2 = parts[2];
            if (parts.Length > 1)
                arg1 = parts[1];
            if (i >= 0)
            {
                if (i == 1)
                    brightness = new string[] { arg1, arg2 };
                else if (i == 3)
                    volume = new string[] { arg1, arg2 };
                else if (i == 200)
                    Core.theHost.execute(eFunction.navigateToAddress, arg1);
                else if (i == 300)
                    Core.theHost.raiseSystemEvent(eFunction.promptDialNumber, arg1, arg2, arg3);
                else
                    Core.theHost.raiseSystemEvent((eFunction)Enum.Parse(typeof(eFunction), parts[0]), arg1, arg2, arg3);
            }
            else if (i == -1)
                Core.theHost.sendMessage(arg1, "OMHal", arg2);
            else if (i == -2)
                Core.theHost.raisePowerEvent((ePowerEvent)Enum.Parse(typeof(ePowerEvent), arg1));
            else if (i == -3)
                Core.theHost.RaiseStorageEvent((eMediaType)Enum.Parse(typeof(eMediaType), arg1), bool.Parse(arg2), arg3);
            else if (i == -4)
                Core.theHost.raiseKeyPressEvent((eKeypressType)Enum.Parse(typeof(eKeypressType), arg1), new KeyboardKeyEventArgs((Key)Enum.Parse(typeof(Key), arg2)));
            else if (i == -99) // Info message from HAL
            {
                string[] Info = new string[parts.Length-2];
                Array.Copy(parts, 2, Info, 0, Info.Length);
                Core.theHost.DebugMsg(DebugMessageType.Info,"HAL", arg1, Info);
            }
        }
        void recv(IAsyncResult res)
        {
            if (isDisposed)
                return;
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 8549);
            try
            {
                parse(ASCIIEncoding.ASCII.GetString(receive.EndReceive(res, ref remote)));
            }
            catch (System.Net.Sockets.SocketException) { } //unknown xp bug on startup
            finally
            {
                if (!isDisposed)
                    receive.BeginReceive(recv, null);
            }
        }
        internal void close()
        {
            isDisposed = true;
            receive.Close();
        }
    }
}
