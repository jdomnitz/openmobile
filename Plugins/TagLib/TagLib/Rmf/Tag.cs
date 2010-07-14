//
// Tag.cs:
//
// Author:
//   Justin Domnitz (jdomnitz@gmail.com)
//
// Original Source:
//   Id3v1/Tag.cs from TagLib-sharp
//
// Copyright (C) 2010 Justin Domnitz (Original Implementation)
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
using System.Collections.Generic;

namespace TagLib.Rmf
{

    /// <summary>
    ///    This class extends <see cref="Tag" /> to provide support for
    ///    reading tags stored in the RMF Metadata format.
    /// </summary>
    public class Tag : TagLib.Tag
    {


        private string title;
        private string description;
        private uint duration;
        private uint bitrate;

        #region Constructors

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> with no contents.
        /// </summary>
        public Tag()
        {
            Clear();
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> by reading the contents from a specified
        ///    position in a specified file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="File" /> object containing the file from
        ///    which the contents of the new instance is to be read.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value specify at what position to
        ///    read the tag.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="position" /> is less than zero or greater
        ///    than the size of the file.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The file does not contain <see cref="FileIdentifier" />
        ///    at the given position.
        /// </exception>
        public Tag(File file, long position)
        {
            // TODO: can we read from file
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> by reading the contents from a specified
        ///    <see cref="ByteVector" /> object.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object to read the tag from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> is less than 128 bytes or does
        ///    not start with <see cref="FileIdentifier" />.
        /// </exception>
        public Tag(ByteVector data)
        {

            if (data == null)
                throw new ArgumentNullException("data");

            Clear();
            Parse(data);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///    Populates the current instance by parsing the contents of
        ///    a raw AudibleMetadata tag.
        /// </summary>
        /// <param name="data">
        ///    	A <see cref="ByteVector" /> object containing the whole tag
        /// 	object
        /// </param>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> is less than 128 bytes or does
        ///    not start with <see cref="FileIdentifier" />.
        /// </exception>
        private void Parse(ByteVector data)
        {
            uint chunkLength;
            string chunkType;
            ByteVector chunkData;
            try
            {
                do
                {
                    chunkType = data.Mid(0, 4).ToString();
                    chunkLength = data.Mid(4, 4).ToUInt();
                    if (chunkType == "DATA")
                        return; //End of the Header Data
                    chunkData = data.Mid(8, (int)(chunkLength - 8));
                    data.RemoveRange(0, (int)chunkLength);
                    parseChunk(chunkType, chunkData);
                } while (chunkType != "DATA");
            }
            catch (Exception)
            {
                throw new CorruptFileException();
            }
        }

        private void parseChunk(string chunkType, ByteVector chunkData)
        {
            ushort len;
            int start=2;
            switch (chunkType)
            {
                case "PROP":
                    bitrate = chunkData.Mid(6, 4).ToUInt();
                    duration = chunkData.Mid(22, 4).ToUInt();
                    break;
                case "CONT":
                    len = chunkData.Mid(start, 2).ToUShort();
                    title = chunkData.Mid(start+2, len).ToString();
                    start = start + len + 2;
                    len = chunkData.Mid(start, 2).ToUShort();
                    description = chunkData.Mid(start+2, len).ToString();
                    //start = start + len + 2;
                    //len = chunkData.Mid(start, 2).ToUShort();
                    //copyright = chunkData.Mid(start+ 2, len).ToString();
                    //start = start + len + 2;
                    //len = chunkData.Mid(start, 2).ToUShort();
                    //comment = chunkData.Mid(start + 2, len).ToString();
                    break;
            }
        }
        #endregion

        #region TagLib.Tag

        /// <summary>
        ///    Gets the tag types contained in the current instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TagTypes.AudibleMetadata" />.
        /// </value>
        public override TagTypes TagTypes
        {
            get { return TagTypes.RMFMetadata; }
        }

        /// <summary>
        ///    Gets the title for the media described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the title for
        ///    the media described by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        public override string Title
        {
            get
            {
                return title;
            }
        }

        /// <summary>
        ///    Gets the album artist for the media described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    	A <see cref="string[]" /> object containing a single 
        /// 	artist described by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        public override string[] AlbumArtists
        {
            get
            {
                return  new string[] { description };
            }
        }

        /// <summary>
        ///    Clears the values stored in the current instance.
        /// </summary>
        public override void Clear()
        {
            title = null;
            description = null;
            bitrate = 0;
            duration = 0;
        }

        #endregion
    }
}