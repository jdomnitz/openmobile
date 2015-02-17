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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Media;

namespace OpenMobile.Plugin
{
    public interface IMediaProvider
    {
        IMediaProviderHandler MediaProviderHandler { get; set; }

        string Activate(Zone zone);

        string Deactivate(Zone zone);

        string Playback_Start(Zone zone, object[] param);
        string Playback_Stop(Zone zone, object[] param);
        string Playback_Pause(Zone zone, object[] param);
        string Playback_Next(Zone zone, object[] param);
        string Playback_Previous(Zone zone, object[] param);
        string Playback_SeekForward(Zone zone, object[] param);
        string Playback_SeekBackward(Zone zone, object[] param);

        bool GetShuffle(Zone zone);
        bool SetShuffle(Zone zone, bool state);
        bool GetRepeat(Zone zone);
        bool SetRepeat(Zone zone, bool state);

        Playlist GetPlaylist(Zone zone);
        Playlist SetPlaylist(Zone zone, Playlist playlist);

        MediaProviderAbilities GetMediaSourceAbilities(string mediaSource);
        List<MediaSource> GetMediaSources();
        MediaSource GetMediaSource(Zone zone);
        string ActivateMediaSource(Zone zone, string mediaSourceName);

    }
}
