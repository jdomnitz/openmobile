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
    /// Tune to 
    /// </summary>
    public interface ITunedContent:IPlayer
    {
        /// <summary>
        /// Tune to the supplied station/channel.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="zone"></param>
        /// <returns>Return true if successful.</returns>
        bool tuneTo(Zone zone,string station);
        /// <summary>
        /// Scan complete band for channels with signal.
        /// </summary>
        bool scanBand(Zone zone);
        /// <summary>
        /// Scan forward for a channel with signal. Fires system event radioTuned when found.
        /// </summary>
        bool scanForward(Zone zone);
        /// <summary>
        /// Scan backward for a channel with signal. Fires system event radioTuned when found.
        /// </summary>
        bool scanReverse(Zone zone);
        /// <summary>
        /// Steps forward one station/channel.
        /// </summary>
        /// <returns>Returns false if no more stations/channels in that direction.</returns>
        bool stepForward(Zone zone);
        /// <summary>
        /// Steps backward one station/channel.
        /// </summary>
        /// <returns>Returns false if no more stations/channels in that direction.</returns>
        bool stepBackward(Zone zone);
        /// <summary>
        /// Returns info on the currently tuned station/channel
        /// </summary>
        /// <returns>Returns info on the currently tuned station/channel</returns>
        stationInfo getStationInfo(Zone zone);
        /// <summary>
        /// Gets information on the Tuned Content's medium
        /// </summary>
        /// <returns></returns>
        eTunedContentBand[] getSupportedBands(Zone zone);
        /// <summary>
        /// When powerstate is true, power on the device. When powerState is false, power off the device.
        /// </summary>
        /// <param name="powerState">When powerstate is true, power on the device. When powerState is false, power off the device.</param>
        /// <param name="instance"></param>
        /// <returns>Returns true if successful.</returns>
        bool setPowerState(Zone zone, bool powerState);
        /// <summary>
        /// Returns a list of all stations/channels that can be tuned to
        /// </summary>
        /// <returns></returns>
        stationInfo[] getStationList(Zone zone);
        /// <summary>
        /// Set the band of the tuned content
        /// </summary>
        /// <param name="band">Band to activate</param>
        /// <param name="instance"></param>
        /// <returns>Returns true if successful.</returns>
        bool setBand(Zone zone, eTunedContentBand band);
        /// <summary>
        /// Get the status of the tuned content
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Returns the status of the tuned content.</returns>
        tunedContentInfo getStatus(Zone zone);
        /// <summary>
        /// Returns the current playback position if applicable otherwise -1
        /// </summary>
        int playbackPosition { get; }
    }
}
