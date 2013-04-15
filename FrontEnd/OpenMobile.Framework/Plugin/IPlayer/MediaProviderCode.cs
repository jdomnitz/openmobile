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
using System.Text;
using OpenMobile;
using OpenMobile.Media;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// A common code base for the IMediaProvider interface
    /// </summary>
    public abstract class MediaProviderCode : BasePluginCode, IMediaProvider
    {
        /// <summary>
        /// Zone specific local data
        /// </summary>
        protected class ZoneSpecificLocalData 
        {
            /// <summary>
            /// Currently active media source
            /// </summary>
            public MediaSource ActiveMediaSource = null;

            /// <summary>
            /// Zone specific plugin data
            /// </summary>
            public ZoneSpecificData Data = null;
        }

        /// <summary>
        /// Zone specific plugin data
        /// </summary>
        public abstract class ZoneSpecificData { }

        /// <summary>
        /// Initializes a MediaProviderCode plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="pluginIcon"></param>
        /// <param name="pluginVersion"></param>
        /// <param name="pluginDescription"></param>
        /// <param name="authorName"></param>
        /// <param name="authorEmail"></param>
        public MediaProviderCode(string pluginName, imageItem pluginIcon, float pluginVersion, string pluginDescription, string authorName, string authorEmail)
            : base(pluginName, pluginIcon, pluginVersion, pluginDescription, authorName, authorEmail)
        {
        }

        /// <summary>
        /// Zone specific data
        /// </summary>
        protected Dictionary<Zone, ZoneSpecificLocalData> _ZoneSpecificData = new Dictionary<Zone, ZoneSpecificLocalData>();
        
        /// <summary>
        /// Media provider type for this plugin
        /// </summary>
        protected MediaProviderTypes _MediaProviderType;

        /// <summary>
        /// Gets the media provider types supported by this plugin
        /// </summary>
        /// <returns></returns>
        public abstract MediaProviderTypes MediaProviderType { get; }

        /// <summary>
        /// Available media sources for this plugin
        /// </summary>
        protected List<MediaSource> _MediaSources = new List<MediaSource>();

        /// <summary>
        /// Gets all media sources supported by this MediaProvider
        /// </summary>
        /// <returns></returns>
        public virtual List<MediaSource> GetMediaSources()
        {
            return _MediaSources;
        }

        /// <summary>
        /// Sets the currently active media source
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="mediaSource"></param>
        /// <returns></returns>
        public virtual bool SetMediaSource(Zone zone, MediaSource mediaSource)
        {
            ZoneSpecificLocalData dta;
            if (!_ZoneSpecificData.TryGetValue(zone, out dta))
            {   // Zone not present, add zone and update data
                dta = new ZoneSpecificLocalData();
                dta.ActiveMediaSource = mediaSource;
                _ZoneSpecificData.Add(zone, dta);
            }
            else
            {   // Zone already present, update data
                dta.ActiveMediaSource = mediaSource;
            }

            //_ZoneSpecificData.
           

            return true;
        }

        /// <summary>
        /// Activate media provider (use this to power on)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual bool ActivateMediaProvider(Zone zone)
        {
            return true;
        }

        /// <summary>
        /// Deactivate media provider (use this to power off)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual bool DeactivateMediaProvider(Zone zone)
        {
            return true;
        }

        /// <summary>
        /// Gets the current information from the media provider
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract MediaProviderInfo GetMediaProviderInfo(Zone zone);

        /// <summary>
        /// Executes the specified command
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="command"></param>
        /// <returns>The returned data from the command</returns>
        public abstract bool ExecuteCommand(Zone zone, MediaProvider_CommandWrapper command);

        /// <summary>
        /// Gets data from the mediaprovider
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract object GetData(Zone zone, MediaProvider_DataWrapper data);
    }
}
