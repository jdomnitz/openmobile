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
    /// Plays audio and/or video
    /// </summary>
    public interface IAVPlayer:IPlayer
    {
        /// <summary>
        /// Play the current media
        /// </summary>
        /// <returns></returns>
        bool play(int instance);
        /// <summary>
        /// Pause the current media
        /// </summary>
        /// <returns></returns>
        bool pause(int instance);
        /// <summary>
        /// Stop the currently playing media
        /// </summary>
        /// <returns></returns>
        bool stop(int stop);
        /// <summary>
        /// The the media playback position (in seconds)
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool setPosition(int instance,float seconds);
        /// <summary>
        /// Set the media playback speed
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool setPlaybackSpeed(int instance,float speed);
        /// <summary>
        /// Play the given media
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool play(int instance,string url,eMediaType type);
        /// <summary>
        /// Return the current playback position (in seconds)
        /// </summary>
        /// <returns></returns>
        float getCurrentPosition(int instance);
        /// <summary>
        /// Get the status of the player
        /// </summary>
        /// <returns></returns>
        ePlayerStatus getPlayerStatus(int instance);
        /// <summary>
        /// Returns the media players playback speed
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        float getPlaybackSpeed(int instance);
    }
}
