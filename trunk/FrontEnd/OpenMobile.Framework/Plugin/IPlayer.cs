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
    /// Base class for all A/V players and receivers
    /// </summary>
    public interface IPlayer : IBasePlugin
    {
        /// <summary>
        /// Set the volume (range 0-100)
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool setVolume(Zone zone, int percent);
        /// <summary>
        /// Get the players volume
        /// </summary>
        /// <returns></returns>
        int getVolume(Zone zone);
        /// <summary>
        /// Gets information on the currently playing media.
        /// </summary>
        /// <returns>Returns null if information is not available.</returns>
        mediaInfo getMediaInfo(Zone zone);
        /// <summary>
        /// A media specific event notification
        /// </summary>
        event MediaEvent OnMediaEvent;
        /// <summary>
        /// Returns a list of possible output devices (NOTE: the index corresponds to instance ID's)
        /// </summary>
        AudioDevice[] OutputDevices { get; }
        /// <summary>
        /// If this plugin supports Advanced Interfaces (aka IEnhancedAVPlayer or IBufferedTunedContent)
        /// </summary>
        bool SupportsAdvancedFeatures { get; }
        /// <summary>
        /// Toggles the visibility of the video window
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        bool SetVideoVisible(Zone zone,bool visible);
    }
}
