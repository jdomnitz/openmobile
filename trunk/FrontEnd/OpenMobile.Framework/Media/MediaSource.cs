using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    #region MediaSource types

    /// <summary>
    /// Media source types
    /// </summary>
    public enum MediaSourceTypes
    {
        /// <summary>
        /// A generic unspecified media source
        /// </summary>
        Unspecified,

        /// <summary>
        /// A tuned content media source (like a radio)
        /// </summary>
        TunedContent,

        /// <summary>
        /// Media based on files (like mp3, flac etc)
        /// </summary>
        FileBasedMedia,

        /// <summary>
        /// Disc based media (like CD, DVD, BluRay etc...)
        /// </summary>
        DiscBasedMedia,
    }


    #endregion

    /// <summary>
    /// A command delegate for a media source
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public delegate object MediaSourceCommandDelegate(Zone zone, object[] param);

    /// <summary>
    /// A media source from a media provider. A media source can be CD, DVD, NetworkStream etc...
    /// </summary>
    public class MediaSource
    {
        /// <summary>
        /// Name of MediaSource
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        /// <summary>
        /// Description for MediaSource
        /// </summary>
        public string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                }
            }
        }
        private string _Description;        

        /// <summary>
        /// Icon for the MediaSource
        /// </summary>
        public imageItem Icon
        {
            get
            {
                return this._Icon;
            }
            set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                }
            }
        }
        private imageItem _Icon;

        /// <summary>
        /// A multipurpose object for providing additional data where a data consists of a name (key) and a value
        /// </summary>
        public Dictionary<string, object> AdditionalData
        {
            get
            {
                return this._AdditionalData;
            }
        }
        private Dictionary<string, object> _AdditionalData = new Dictionary<string,object>();

        /// <summary>
        /// Additional command list for the media source. Key is the command name and value is the <paramref name="MediaSourceCommandDelegate"/> to run when the command is executed
        /// the delegate is defined like this: MediaSourceCommandDelegate(Zone zone, object[] param)
        /// </summary>
        public Dictionary<string, MediaSourceCommandDelegate> AdditionalCommands
        {
            get
            {
                return this._AdditionalCommands;
            }
        }
        private Dictionary<string, MediaSourceCommandDelegate> _AdditionalCommands = new Dictionary<string, MediaSourceCommandDelegate>();

        /// <summary>
        /// A reference to the provider of this media source
        /// </summary>
        public IMediaProvider Provider
        {
            get
            {
                return this._Provider;
            }
            set
            {
                if (this._Provider != value)
                {
                    this._Provider = value;
                }
            }
        }
        private IMediaProvider _Provider;

        /// <summary>
        /// Type of media source (Can be one of the types defined in MediaSourceTypes or a custom string identifying a media source)
        /// </summary>
        public string MediaSourceType
        {
            get
            {
                return this._MediaSourceType;
            }
            set
            {
                if (this._MediaSourceType != value)
                {
                    this._MediaSourceType = value;
                }
            }
        }
        private string _MediaSourceType = MediaSourceTypes.Unspecified.ToString();

        /// <summary>
        /// Sets an additional data value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetAdditionalData(string key, object value)
        {
            if (!_AdditionalData.ContainsKey(key))
                _AdditionalData.Add(key, value);
            else
                _AdditionalData[key] = value;
        }

        /// <summary>
        /// Creates a new media source
        /// </summary>
        public MediaSource()
        {
        }

        /// <summary>
        /// Creates a new media source
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="mediaSourceType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        internal MediaSource(IMediaProvider provider, string mediaSourceType, string name, string description, imageItem icon)
        {
            _MediaSourceType = mediaSourceType;
            _Provider = provider;
            _Name = name;
            _Description = description;
            _Icon = icon;
        }

        /// <summary>
        /// A string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}", _Name);
        }

    }

    /// <summary>
    /// A media source comming from a tunable source
    /// </summary>
    public class MediaSource_TunedContent : MediaSource
    {
        /// <summary>
        /// Channel ID (like MHz or channel number, etc...)
        /// </summary>
        public string ChannelID
        {
            get
            {
                return (string)base.AdditionalData["ChannelID"];
            }
            set
            {
                base.AdditionalData["ChannelID"] = value;
            }
        }

        /// <summary>
        /// The long version of the channel name
        /// </summary>
        public string ChannelNameLong
        {
            get
            {
                return (string)base.AdditionalData["ChannelName.Long"];
            }
            set
            {
                base.AdditionalData["ChannelName.Long"] = value;
            }
        }

        /// <summary>
        /// The short version of the channel name
        /// </summary>
        public string ChannelNameShort
        {
            get
            {
                return (string)base.AdditionalData["ChannelName.Short"];
            }
            set
            {
                base.AdditionalData["ChannelName.Short"] = value;
            }
        }

        /// <summary>
        /// Program text for the channel
        /// </summary>
        public string ProgramText
        {
            get
            {
                return (string)base.AdditionalData["ProgramText"];
            }
            set
            {
                base.AdditionalData["ProgramText"] = value;
            }
        }

        /// <summary>
        /// Signal strength (0 - 100%)
        /// </summary>
        public int SignalStrength
        {
            get
            {
                return (int)base.AdditionalData["SignalStrength"];
            }
            set
            {
                base.AdditionalData["SignalStrength"] = value;
            }
        }

        /// <summary>
        /// The bitrate of the current channel (if available)
        /// </summary>
        public string SignalBitRate
        {
            get
            {
                return (string)base.AdditionalData["SignalBitRate"];
            }
            set
            {
                base.AdditionalData["SignalBitRate"] = value;
            }
        }

        /// <summary>
        /// Is channel considered a HD type channel
        /// </summary>
        public bool HD
        {
            get
            {
                return (bool)base.AdditionalData["HD"];
            }
            set
            {
                base.AdditionalData["HD"] = value;
            }
        }

        /// <summary>
        /// Is signal stereo
        /// </summary>
        public bool Stereo
        {
            get
            {
                return (bool)base.AdditionalData["Stereo"];
            }
            set
            {
                base.AdditionalData["Stereo"] = value;
            }
        }

        /// <summary>
        /// The type of the program for this channel
        /// </summary>
        public string ProgramType
        {
            get
            {
                return (string)base.AdditionalData["ProgramType"];
            }
            set
            {
                base.AdditionalData["ProgramType"] = value;
            }
        }

        /// <summary>
        /// The status of the device
        /// </summary>
        public string DeviceStatus
        {
            get
            {
                return (string)base.AdditionalData["DeviceStatus"];
            }
            set
            {
                base.AdditionalData["DeviceStatus"] = value;
            }
        }

        /// <summary>
        /// A list (PlayList) of all available channel presets
        /// </summary>
        public Playlist ChannelsPreset
        {
            get
            {
                return (Playlist)base.AdditionalData["Channels.Preset"];
            }
            set
            {
                base.AdditionalData["Channels.Preset"] = value;
            }
        }

        /// <summary>
        /// A list (PlayList) of all available channel live from the radio (if any)
        /// </summary>
        public Playlist ChannelsLive
        {
            get
            {
                return (Playlist)base.AdditionalData["Channels.Live"];
            }
            set
            {
                base.AdditionalData["Channels.Live"] = value;
            }
        }

        /// <summary>
        /// A reference to which list source is currently active (0 = Live, 1 = Presets. Other sources can also be used but this is dependent on the media source)
        /// </summary>
        public int ListSource
        {
            get
            {
                return (int)base.AdditionalData["ListSource"];
            }
            set
            {
                base.AdditionalData["ListSource"] = value;
            }
        }

        /// <summary>
        /// Creates a new tuned content media source
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        public MediaSource_TunedContent(IMediaProvider provider, string name, string description, imageItem icon)
            : base(provider, MediaSourceTypes.TunedContent.ToString(), name, description, icon)
        {
            // Add predefined data for this media source
            base.AdditionalData.Add("ChannelID", "");
            base.AdditionalData.Add("ChannelName.Long", "");
            base.AdditionalData.Add("ChannelName.Short", "");
            base.AdditionalData.Add("ProgramText", "");
            base.AdditionalData.Add("SignalStrength", 0);
            base.AdditionalData.Add("SignalBitRate", "");
            base.AdditionalData.Add("DeviceStatus", "");
            base.AdditionalData.Add("HD", false);
            base.AdditionalData.Add("Stereo", false);
            base.AdditionalData.Add("ProgramType", "");
            base.AdditionalData.Add("Channels.Preset", null);
            base.AdditionalData.Add("Channels.Live", null);
            base.AdditionalData.Add("ListSource", null);

            // Add predefined commands
            base.AdditionalCommands.Add("Preset.Set", null);
            base.AdditionalCommands.Add("Preset.Remove", null);
            base.AdditionalCommands.Add("Preset.Rename", null);
            base.AdditionalCommands.Add("Preset.SetCoverArt", null);
            base.AdditionalCommands.Add("SetListSource", null);
            base.AdditionalCommands.Add("DirectTune", null);
            base.AdditionalCommands.Add("SearchChannels", null);
        }


        /// <summary>
        /// Raised when a preset is set.
        /// <para>If no parameter is given the current channel is to be set as a preset</para>
        /// <para>If a parameter is given it identifies the channel to set as a preset. Param0 should be ChannelID as string or int or a full mediaInfo object, Param1 is optional preset name</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_PresetSet
        {
            add
            {
                base.AdditionalCommands["Preset.Set"] = value;
            }
            remove
            {
                base.AdditionalCommands["Preset.Set"] = null;
            }
        }

        /// <summary>
        /// Raised when a preset is removed.
        /// <para>Param0 should be ChannelID as string or int or a full mediaInfo object</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_PresetRemove
        {
            add
            {
                base.AdditionalCommands["Preset.Remove"] = value;
            }
            remove
            {
                base.AdditionalCommands["Preset.Remove"] = null;
            }
        }

        /// <summary>
        /// Raised when setting the list source
        /// <para>Requires one parameter which describes the list source. Param0 is integer where 0 = Live, 1 = Presets. Other sources can also be used but this is dependent on the media source</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_SetListSource
        {
            add
            {
                base.AdditionalCommands["SetListSource"] = value;
            }
            remove
            {
                base.AdditionalCommands["SetListSource"] = null;
            }
        }

        /// <summary>
        /// Raised when tuning to a channel
        /// <para>Requires one parameter which describes the channel to tune to. Param0 should be ChannelID as string or int or a full mediaInfo object</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_DirectTune
        {
            add
            {
                base.AdditionalCommands["DirectTune"] = value;
            }
            remove
            {
                base.AdditionalCommands["DirectTune"] = null;
            }
        }

        /// <summary>
        /// Raised when renaming a preset
        /// <para>Requires two parameters where the Param0 is channel data as mediaInfo and Param1 as string for the new name</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_PresetRename
        {
            add
            {
                base.AdditionalCommands["Preset.Rename"] = value;
            }
            remove
            {
                base.AdditionalCommands["Preset.Rename"] = null;
            }
        }

        /// <summary>
        /// Raised when setting the coverart for a preset
        /// <para>Requires two parameters where the Param0 is channel data as mediaInfo and Param1 as OImage for the new cover art</para>
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_PresetSetCoverArt
        {
            add
            {
                base.AdditionalCommands["Preset.SetCoverArt"] = value;
            }
            remove
            {
                base.AdditionalCommands["Preset.SetCoverArt"] = null;
            }
        }

        /// <summary>
        /// Raised when sending command to search channels (this is used to rescan all available channels if supported) 
        /// </summary>
        public event MediaSourceCommandDelegate OnCommand_SearchChannels
        {
            add
            {
                base.AdditionalCommands["SearchChannels"] = value;
            }
            remove
            {
                base.AdditionalCommands["SearchChannels"] = null;
            }
        }

    }

    /// <summary>
    /// A unspecified (generic usage) media source
    /// </summary>
    public class MediaSource_Unspecified : MediaSource
    {
        /// <summary>
        /// Creates a new unspecified media source 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        public MediaSource_Unspecified(IMediaProvider provider, string name, string description, imageItem icon)
            : base(provider, MediaSourceTypes.Unspecified.ToString(), name, description, icon)
        {
        }
    }

    /// <summary>
    /// A media source which is disc based (like CD, DVD etc)
    /// </summary>
    public class MediaSource_DiscBasedMedia : MediaSource
    {
        /// <summary>
        /// Creates a new disc based media source 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        public MediaSource_DiscBasedMedia(IMediaProvider provider, string name, string description, imageItem icon)
            : base(provider, MediaSourceTypes.DiscBasedMedia.ToString(), name, description, icon)
        {
        }
    }

    /// <summary>
    /// A media source which is file based (like MP3, FLAC etc)
    /// </summary>
    public class MediaSource_FileBasedMedia : MediaSource
    {
        /// <summary>
        /// Creates a new file based media source 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        public MediaSource_FileBasedMedia(IMediaProvider provider, string name, string description, imageItem icon)
            : base(provider, MediaSourceTypes.FileBasedMedia.ToString(), name, description, icon)
        {
        }
    }

}
