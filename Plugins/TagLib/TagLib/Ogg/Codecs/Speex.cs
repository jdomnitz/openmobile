//
// Speex.cs:
//
// Author:
//   Justin Domnitz (jdomnitz@gmail.com)
//
// Original Source:
//   Ogg/Codecs/Vorbis.cs from TagLib-sharp
//
// Copyright (C) 2010 Justin Domnitz
//
// This library is free software; you can redistribute it and/or modify
// it  under the terms of the GNU Lesser General Public License version
// 2.1 as published by the Free Software Foundation.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
// USA
//
using System;
namespace TagLib.Ogg.Codecs
{
    /// <summary>
    ///    This class extends <see cref="Codec" /> and implements <see
    ///    cref="IAudioCodec" /> to provide support for processing Ogg
    ///    Spx bitstreams.
    /// </summary>
    public class Spx : Codec, IAudioCodec
    {
        #region Private Static Fields

        /// <summary>
        ///    Contains the file identifier.
        /// </summary>
        private static ByteVector id = "Speex   ";

        #endregion



        #region Private Fields

        /// <summary>
        ///    Contains the header packet.
        /// </summary>
        private HeaderPacket header;

        /// <summary>
        ///    Contains the comment data.
        /// </summary>
        private ByteVector comment_data;

        #endregion



        #region Constructors

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Spx" />.
        /// </summary>
        private Spx()
        {
        }

        #endregion



        #region Public Methods

        /// <summary>
        ///    Reads a Ogg packet that has been encountered in the
        ///    stream.
        /// </summary>
        /// <param name="packet">
        ///    A ByteVector object containing a packet to
        ///    be read by the current instance.
        /// </param>
        /// <param name="index">
        ///    A <see cref="int" /> value containing the index of the
        ///    packet in the stream.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if the codec has read all the
        ///    necessary packets for the stream and does not need to be
        ///    called again, typically once the Xiph comment has been
        ///    found. Otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="packet" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="index" /> is less than zero.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The data does not conform to the specificiation for the
        ///    codec represented by the current instance.
        /// </exception>
        public override bool ReadPacket(ByteVector packet, int index)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    "index must be at least zero.");

            int type = PacketType(packet);
            if (type != 1 && index == 0)
                throw new CorruptFileException(
                    "Stream does not begin with spx header.");

            if (comment_data == null)
            {
                if (type == 1)
                    header = new HeaderPacket(packet);
                else if (type == -1)
                    comment_data = packet;
                else
                    return true;
            }

            return comment_data != null;
        }

        /// <summary>
        ///    Computes the duration of the stream using the first and
        ///    last granular positions of the stream.
        /// </summary>
        /// <param name="firstGranularPosition">
        ///    A <see cref="long" /> value containing the first granular
        ///    position of the stream.
        /// </param>
        /// <param name="lastGranularPosition">
        ///    A <see cref="long" /> value containing the last granular
        ///    position of the stream.
        /// </param>
        /// <returns>
        ///    A <see cref="TimeSpan" /> value containing the duration
        ///    of the stream.
        /// </returns>
        public override TimeSpan GetDuration(long firstGranularPosition,
                                              long lastGranularPosition)
        {
            return header.sample_rate == 0 ? TimeSpan.Zero :
                TimeSpan.FromSeconds((double)
                    (lastGranularPosition -
                        firstGranularPosition) /
                    (double)header.sample_rate);
        }
        #endregion



        #region Public Properties

        /// <summary>
        ///    Gets the bitrate of the audio represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing a bitrate of the
        ///    audio represented by the current instance.
        /// </value>
        public int AudioBitrate
        {
            get
            {
                return (int)((float)header.bitrate_nominal /
                    1000f + 0.5);
            }
        }

        /// <summary>
        ///    Gets the sample rate of the audio represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the sample rate of
        ///    the audio represented by the current instance.
        /// </value>
        public int AudioSampleRate
        {
            get { return (int)header.sample_rate; }
        }

        /// <summary>
        ///    Gets the number of channels in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of
        ///    channels in the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioChannels
        {
            get { return (int)header.channels; }
        }

        /// <summary>
        ///    Gets the types of media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always MediaTypes.Audio.
        /// </value>
        public override MediaTypes MediaTypes
        {
            get { return MediaTypes.Audio; }
        }

        /// <summary>
        ///    Gets the raw Xiph comment data contained in the codec.
        /// </summary>
        /// <value>
        ///    A ByteVector object containing a raw Xiph
        ///    comment or <see langword="null"/> if none was found.
        /// </value>
        public override ByteVector CommentData
        {
            get { return comment_data; }
        }

        /// <summary>
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public override string Description
        {
            get
            {
                return string.Format(
                    "Speex Version {0} Audio",
                    header.spx_version);
            }
        }

        #endregion



        #region Public Static Methods

        /// <summary>
        ///    Implements the CodecProvider delegate to
        ///    provide support for recognizing a Vorbis stream from the
        ///    header packet.
        /// </summary>
        /// <param name="packet">
        ///    A ByteVector object containing the stream
        ///    header packet.
        /// </param>
        /// <returns>
        ///    A <see cref="Codec"/> object containing a codec capable
        ///    of parsing the stream of <see langref="null" /> if the
        ///    stream is not a Vorbis stream.
        /// </returns>
        public static Codec FromPacket(ByteVector packet)
        {
            return (PacketType(packet) == 1) ? new Spx() : null;
        }

        #endregion



        #region Private Static Methods

        /// <summary>
        ///    Gets the packet type for a specified Vorbis packet.
        /// </summary>
        /// <param name="packet">
        ///    A ByteVector object containing a Vorbis
        ///    packet.
        /// </param>
        /// <returns>
        ///    A <see cref="int" /> value containing the packet type or
        ///    -1 if the packet is invalid.
        /// </returns>
        private static int PacketType(ByteVector packet)
        {
            if (packet.Count <= id.Count)
                return -1;
            
            for (int i = 0; i < id.Count; i++)
                if (packet[i] != id[i])
                    return -1;

            return 1;
        }

        #endregion

        /// <summary>
        ///    This structure represents a Speex header packet.
        /// </summary>
        private struct HeaderPacket
        {
            public uint sample_rate;
            public uint channels;
            public uint spx_version;
            public uint bitrate_nominal;
            public bool VBR;

            public HeaderPacket(ByteVector data)
            {
                spx_version = data.Mid(28, 4).ToUInt(false);
                channels = data.Mid(48, 4).ToUInt(false);
                sample_rate = data.Mid(36, 4).ToUInt(false);
                bitrate_nominal = data.Mid(52, 4).ToUInt(false);
                VBR = (data.Mid(60, 4).ToUInt(false)==1);
                if (VBR == true)
                    bitrate_nominal = 0;
            }
        }
    }
}
