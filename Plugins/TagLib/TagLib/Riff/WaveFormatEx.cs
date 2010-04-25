//
// WaveFormatEx.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2007 Brian Nickel
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

namespace TagLib.Riff
{
    /// <summary>
    ///    This structure provides a representation of a Microsoft
    ///    WaveFormatEx structure.
    /// </summary>
    public struct WaveFormatEx : IAudioCodec, ILosslessAudioCodec
    {
        #region Private Fields

        /// <summary>
        ///    Contains the format tag of the audio.
        /// </summary>
        ushort format_tag;

        /// <summary>
        ///    Contains the number of audio channels.
        /// </summary>
        ushort channels;

        /// <summary>
        ///    Contains the number of samples per second.
        /// </summary>
        uint samples_per_second;

        /// <summary>
        ///    Contains the average number of bytes per second.
        /// </summary>
        uint average_bytes_per_second;

        /// <summary>
        ///    Contains the number of bits per sample.
        /// </summary>
        ushort bits_per_sample;

        #endregion



        #region Constructorss

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="WaveFormatEx" /> by reading the raw structure from
        ///    a specified position in a <see cref="ByteVector" />
        ///    object.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    data structure.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="int" /> value specifying the index in
        ///    <paramref name="data"/> at which the structure begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="offset" /> is less than zero.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> contains less than 16 bytes at
        ///    <paramref name="offset" />.
        /// </exception>
        public WaveFormatEx(ByteVector data, int offset)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(
                    "offset");

            if (offset + 16 > data.Count)
                throw new CorruptFileException(
                    "Expected 16 bytes.");

            format_tag = data.Mid(offset, 2).ToUShort(false);
            channels = data.Mid(offset + 2, 2).ToUShort(false);
            samples_per_second = data.Mid(offset + 4, 4)
                .ToUInt(false);
            average_bytes_per_second = data.Mid(offset + 8, 4)
                .ToUInt(false);
            bits_per_sample = data.Mid(offset + 14, 2)
                .ToUShort(false);
        }

        #endregion



        #region Public Properties

        /// <summary>
        ///    Gets the format tag of the audio described by the
        ///    current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ushort" /> value containing the format tag
        ///    of the audio.
        /// </returns>
        /// <remarks>
        ///    Format tags indicate the codec of the audio contained in
        ///    the file and are contained in a Microsoft registry. For
        ///    a description of the format, use <see cref="Description"
        ///    />.
        /// </remarks>
        public ushort FormatTag
        {
            get { return format_tag; }
        }

        /// <summary>
        ///    Gets the average bytes per second of the audio described
        ///    by the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ushort" /> value containing the average
        ///    bytes per second of the audio.
        /// </returns>
        public uint AverageBytesPerSecond
        {
            get { return average_bytes_per_second; }
        }

        /// <summary>
        ///    Gets the bits per sample of the audio described by the
        ///    current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ushort" /> value containing the bits per
        ///    sample of the audio.
        /// </returns>
        public ushort BitsPerSample
        {
            get { return bits_per_sample; }
        }

        #endregion

        #region ILosslessAudioCodec

        int ILosslessAudioCodec.BitsPerSample
        {
            get { return bits_per_sample; }
        }

        #endregion

        #region IAudioCodec

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
                return (int)Math.Round(
                    average_bytes_per_second * 8d / 1000d);
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
            get { return (int)samples_per_second; }
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
            get { return channels; }
        }

        /// <summary>
        ///    Gets the types of media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="MediaTypes.Audio" />.
        /// </value>
        public MediaTypes MediaTypes
        {
            get { return MediaTypes.Audio; }
        }

        /// <summary>
        ///    Gets the duration of the media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TimeSpan.Zero" />.
        /// </value>
        public TimeSpan Duration
        {
            get { return TimeSpan.Zero; }
        }

        /// <summary>
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public string Description
        {
            get
            {
                return ""; //More bloat removed
            }
        }

        #endregion



        #region IEquatable

        /// <summary>
        ///    Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="int" /> value containing the hash code for
        ///    the current instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(format_tag ^ channels ^
                    samples_per_second ^
                    average_bytes_per_second ^
                    bits_per_sample);
            }
        }

        /// <summary>
        ///    Checks whether or not the current instance is equal to
        ///    another object.
        /// </summary>
        /// <param name="other">
        ///    A <see cref="object" /> to compare to the current
        ///    instance.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    current instance is equal to <paramref name="other" />.
        /// </returns>
        /// <seealso cref="M:System.IEquatable`1.Equals" />
        public override bool Equals(object other)
        {
            if (!(other is WaveFormatEx))
                return false;

            return Equals((WaveFormatEx)other);
        }

        /// <summary>
        ///    Checks whether or not the current instance is equal to
        ///    another instance of <see cref="WaveFormatEx" />.
        /// </summary>
        /// <param name="other">
        ///    A <see cref="WaveFormatEx" /> object to compare to the
        ///    current instance.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    current instance is equal to <paramref name="other" />.
        /// </returns>
        /// <seealso cref="M:System.IEquatable`1.Equals" />
        public bool Equals(WaveFormatEx other)
        {
            return format_tag == other.format_tag &&
                channels == other.channels &&
                samples_per_second == other.samples_per_second &&
                average_bytes_per_second == other.average_bytes_per_second &&
                bits_per_sample == other.bits_per_sample;
        }

        /// <summary>
        ///    Gets whether or not two instances of <see
        ///    cref="WaveFormatEx" /> are equal to eachother.
        /// </summary>
        /// <param name="first">
        ///    A <see cref="WaveFormatEx" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    A <see cref="WaveFormatEx" /> object to compare.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if <paramref name="first" /> is
        ///    equal to <paramref name="second" />. Otherwise, <see
        ///    langword="false" />.
        /// </returns>
        public static bool operator ==(WaveFormatEx first,
                                        WaveFormatEx second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///    Gets whether or not two instances of <see
        ///    cref="WaveFormatEx" /> differ.
        /// </summary>
        /// <param name="first">
        ///    A <see cref="WaveFormatEx" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    A <see cref="WaveFormatEx" /> object to compare.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if <paramref name="first" /> is
        ///    unequal to <paramref name="second" />. Otherwise, <see
        ///    langword="false" />.
        /// </returns>
        public static bool operator !=(WaveFormatEx first,
                                        WaveFormatEx second)
        {
            return !first.Equals(second);
        }
        #endregion
    }
}