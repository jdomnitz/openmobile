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

using OpenMobile.Controls;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// Interface with I/O devices and other raw hardware
    /// </summary>
    public interface IRawHardware:IBasePlugin
    {
        /// <summary>
        /// Convert the string name of the value you are trying to read or write to a PID
        /// </summary>
        /// <param name="FunctionName"></param>
        /// <returns>The PID</returns>
        int getPID(string FunctionName);
        /// <summary>
        /// Get the value of the given PID. Return null if incorrect type of PID.
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        int getIntValue(int PID);
        /// <summary>
        /// Get the value of the given PID. Return null if incorrect type of PID.
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        float getFloatValue(int PID);
        /// <summary>
        /// Get the value of the given PID. Return null if incorrect type of PID.
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        string getStringValue(int PID);
        /// <summary>
        /// Get the value of the given PID. Return null if incorrect type of PID.
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        byte getByteValue(int PID);
        /// <summary>
        /// Get the value of the given PID. Return null if incorrect type of PID.
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        bool getBoolValue(int PID);
        /// <summary>
        /// Set the value of the given PID to the given value. Returns true if succeeded.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(int PID, string value);
        /// <summary>
        /// Set the value of the given PID to the given value. Returns true if succeeded.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(int PID, float value);
        /// <summary>
        /// Set the value of the given PID to the given value. Returns true if succeeded.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(int PID, int value);
        /// <summary>
        /// Set the value of the given PID to the given value. Returns true if succeeded.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(int PID, byte value);
        /// <summary>
        /// Set the value of the given PID to the given value. Returns true if succeeded.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(int PID, bool value);
        /// <summary>
        /// Classify's a PID as a certain type of value. Returns true if successful.
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool setPIDType(int PID, string type);
        /// <summary>
        /// Returns the PID type for the specified PID. Plugin specific
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        string getPIDType(int PID);
        /// <summary>
        /// Reset the hardware device.
        /// </summary>
        void resetDevice();
        /// <summary>
        /// Plugin specific (vehicle information, hardware name, etc.)
        /// </summary>
        /// <returns></returns>
        string deviceInfo{get;}
        /// <summary>
        /// Firmware version of currently connected hardware
        /// </summary>
        /// <returns></returns>
        string firmwareVersion{get;}
    }
}
