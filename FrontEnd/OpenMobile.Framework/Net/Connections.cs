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
using OpenMobile.Plugin;
using System.Collections.Generic;
using System;

namespace OpenMobile.Net
{
    /// <summary>
    /// Manages network connections
    /// </summary>
    public static class Connections
    {
        private static string lastNetworkID = String.Empty;
        /// <summary>
        /// Connects to a network
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool connect(IPluginHost host)
        {
            object o = new object();
            host.getData(eGetData.GetAvailableNetworks, String.Empty, out o);
            if (o == null)
                return false;
            List<connectionInfo> info = (List<connectionInfo>)o;
            int speed = 0;
            string name = null;
            foreach (connectionInfo i in info)
                if (i.potentialSpeed > speed)
                {
                    name = i.NetworkName;
                    speed = i.potentialSpeed;
                }
            if (name == null)
                return false;
            return connect(host, name);
        }
        /// <summary>
        /// Connects to a network
        /// </summary>
        /// <param name="host"></param>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public static bool connect(IPluginHost host, string connectionID)
        {
            return connect(host, connectionID, null);
        }
        /// <summary>
        /// Connects to a network
        /// </summary>
        /// <param name="host"></param>
        /// <param name="connectionID"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static bool connect(IPluginHost host, string connectionID, string credentials)
        {
            object o = new object();
            host.getData(eGetData.GetPlugins, String.Empty, out o);
            if (o == null)
                return false;
            List<IBasePlugin> l = (List<IBasePlugin>)o;
            connectionInfo info;
            foreach (IBasePlugin b in l.FindAll(p => typeof(INetwork).IsInstanceOfType(p)))
            {
                INetwork n = (INetwork)b;
                info = Array.Find<connectionInfo>(n.getAvailableNetworks(), p => p.UID == connectionID);
                if (info == null)
                    continue;
                info.Credentials = credentials;
                if (info != null)
                {
                    lastNetworkID = connectionID;
                    return n.connect(info);
                }
            }
            return false;
        }

        /// <summary>
        /// Disconnects from the currently connected network
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool disconnect(IPluginHost host)
        {
            return disconnect(host, lastNetworkID);
        }

        /// <summary>
        /// Disconnects from the given network
        /// </summary>
        /// <param name="host"></param>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public static bool disconnect(IPluginHost host, string connectionID)
        {
            object o = new object();
            host.getData(eGetData.GetPlugins, String.Empty, out o);
            List<IBasePlugin> l = (List<IBasePlugin>)o;
            connectionInfo info;
            foreach (IBasePlugin b in l.FindAll(p => typeof(INetwork).IsInstanceOfType(p)))
            {
                INetwork n = (INetwork)b;
                info = Array.Find<connectionInfo>(n.getAvailableNetworks(), p => p.UID == connectionID);
                if (info != null)
                    return n.disconnect(info);
            }
            return false;
        }
    }
}
