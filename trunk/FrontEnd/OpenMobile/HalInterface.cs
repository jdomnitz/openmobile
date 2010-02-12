using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace OpenMobile
{
    public sealed class HalInterface
        {
            System.Net.Sockets.UdpClient receive;
            System.Net.Sockets.UdpClient send;

            public HalInterface()
            {
                OpenMobile.Framework.OSSpecific.runManagedProcess("OMHal.exe", "", false);
                receive = new System.Net.Sockets.UdpClient(8550);
                receive.BeginReceive(recv, null);
                send = new System.Net.Sockets.UdpClient("127.0.0.1", 8549);
            }
            public void snd(string text)
            {
                send.Send(ASCIIEncoding.ASCII.GetBytes(text), text.Length);
            }
            void parse(string message)
            {
                string[] parts=message.Split(new char[]{'|'});
                int i = int.Parse(parts[0]);
                string arg1="",arg2="",arg3="";
                if (parts.Length>3)
                    arg3=parts[3];
                if (parts.Length>2)
                    arg2=parts[2];
                if (parts.Length>1)
                    arg1=parts[1];
                if (i >= 0)
                    Core.theHost.raiseSystemEvent((eFunction)Enum.Parse(typeof(eFunction), parts[0]), arg1, arg2, arg3);
                else if (i == -3)
                    Core.theHost.RaiseStorageEvent((eMediaType)Enum.Parse(typeof(eMediaType), arg1), arg2);
            }
            void recv(IAsyncResult res)
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 8549);
                parse(ASCIIEncoding.ASCII.GetString(receive.EndReceive(res, ref remote)));
                receive.BeginReceive(recv, null);
            }
        }
    }
