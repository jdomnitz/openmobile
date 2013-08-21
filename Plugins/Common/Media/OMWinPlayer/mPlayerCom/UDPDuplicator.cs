/*********************************************************************************
    This file is part of OpenMobile.

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
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace OpenMobile.mPlayer
{
    /// <summary>
    /// A class to read udp packets from one port and then rely them on to several other ports
    /// </summary>
    class UDPDuplicator
    {
        Thread receiveThread = null;        
        UdpClient UDPReceiveClient;
        UdpClient UDPSendClient;
        IPEndPoint[] remoteEndPoint;

        /// <summary>
        /// Amount of ports to relay data to starting from basePort + 1
        /// </summary>
        public int PortCount
        {
            get
            {
                return this._PortCount;
            }
            set
            {
                if (this._PortCount != value)
                {
                    this._PortCount = value;
                }
            }
        }
        private int _PortCount = 1;

        /// <summary>
        /// Port to use for reading packets from
        /// </summary>
        public int BasePort
        {
            get
            {
                return this._BasePort;
            }
            set
            {
                if (this._BasePort != value)
                {
                    this._BasePort = value;
                }
            }
        }
        private int _BasePort = 23867;
        
        /// <summary>
        /// Starts a new UDP duplicator
        /// </summary>
        /// <param name="basePort"></param>
        /// <param name="portCount"></param>
        public UDPDuplicator(int basePort, int portCount)
        {
            _BasePort = basePort;
            _PortCount = portCount;

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        ~UDPDuplicator()
        {
            Dispose();
        }        

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            receiveThread.Abort();
            receiveThread = null;
        }

        /// <summary>
        /// Receive thread
        /// </summary>
        private void ReceiveData()
        {
            UDPReceiveClient = new UdpClient(_BasePort);
            UDPSendClient = new UdpClient();

            string ports = "";
            remoteEndPoint = new IPEndPoint[_PortCount];
            for (int i = 0; i < remoteEndPoint.Length; i++)
            {
                remoteEndPoint[i] = new IPEndPoint(IPAddress.Loopback, _BasePort + i + 1);
                ports += remoteEndPoint[i].Port + ", ";
            }

            System.Diagnostics.Debug.WriteLine(String.Format("UDPDuplicator: Relaying UDP packets from port {0} to the following ports: {1}", _BasePort, ports));
            while (true)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = UDPReceiveClient.Receive(ref anyIP);

                    // Send packets on to other ports
                    for (int i = 0; i < remoteEndPoint.Length; i++)
                        UDPSendClient.Send(data, data.Length, remoteEndPoint[i]);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("UDP Error: ", err));
                }
            }
        }
    }
}
