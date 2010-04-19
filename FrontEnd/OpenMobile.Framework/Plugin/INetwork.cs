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

namespace OpenMobile.Plugin
{
    /// <summary>
    /// Represents the status of the network connection
    /// </summary>
    public enum eConnectionStatus
    {
        /// <summary>
        /// No connection is established
        /// </summary>
        Disconnected,
        /// <summary>
        /// A connection has been established
        /// </summary>
        Connected,
        /// <summary>
        /// A connection is in progress
        /// </summary>
        Connecting,
        /// <summary>
        /// Connection failed/Error Occured/Hardware error/Hardware not available/Hardware not responding
        /// </summary>
        Error
    }
    /// <summary>
    /// Interface with dialup and wireless networks
    /// </summary>
    public interface INetwork:IBasePlugin
    {
        /// <summary>
        /// Alerts the plugin host that a wireless event has occured.  On non-wireless connections no events are fired
        /// </summary>
        event WirelessEvent OnWirelessEvent;
        /// <summary>
        /// Returns information about available networks (dial-up connections, wireless networks, etc.)
        /// </summary>
        /// <returns></returns>
        connectionInfo[] getAvailableNetworks();
        /// <summary>
        /// Connect to the specified network
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool connect(connectionInfo connection);
        /// <summary>
        /// Disconnect from the specified network
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool disconnect(connectionInfo connection);
        /// <summary>
        /// Retrieves the status of the active connection (the last one the connect command was called on-otherwise returns 0).  After 5 seconds should default to 3.
        /// </summary>
        /// <returns>Returns 0 for not connected, returns 1 for connected, returns 2 for connecting, returns 3 for error.</returns>
        eConnectionStatus getConnectionStatus();
    }
}
