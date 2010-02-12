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
using OpenMobile;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// Bluetooth Stack Interface
    /// </summary>
    public interface IBluetooth:IBasePlugin
    {
        /// <summary>
        /// Raised when a wireless event occurs within the plugin
        /// </summary>
        event WirelessEvent OnWirelessEvent;
        /// <summary>
        /// Dial the given number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        bool dial(string number);
        /// <summary>
        /// Answer an incoming call
        /// </summary>
        /// <returns>True if succeeded</returns>
        bool answer();
        /// <summary>
        /// Ignore an incoming call
        /// </summary>
        void ignore();
        /// <summary>
        /// Accepts a pairing request sent by the given device
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool acceptRequest(string deviceName,string password);
        /// <summary>
        /// Search for and list devices in connection range
        /// </summary>
        /// <returns></returns>
        string[] getDevices();
        /// <summary>
        /// Initiate pairing with the given device
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        bool pairWithDevice(string deviceName);
        /// <summary>
        /// If true, transfers audio to the phone.  If false, transfers audio from the phone.
        /// </summary>
        /// <param name="toPhone"></param>
        /// <returns>True if successful</returns>
        bool transferAudio(bool toPhone);
        /// <summary>
        /// Syncs the phonebook with the contacts database
        /// </summary>
        void syncPhonebook();
        //***INCOMPLETE DRAFT***
    }
}
