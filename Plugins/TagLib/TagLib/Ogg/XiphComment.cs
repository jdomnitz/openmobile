//
// XiphComment.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   xiphcomment.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 Scott Wheeler (Original Implementation)
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TagLib.Ogg
{
	/// <summary>
	///    This class extends <see cref="TagLib.Tag" /> and implements <see
	///    cref="T:System.Collections.Generic.IEnumerable`1" /> to provide
	///    support for reading and writing Xiph comments.
	/// </summary>
	public class XiphComment : TagLib.Tag, IEnumerable<string>
	{
#region Private Fields
		
		/// <summary>
		///    Contains the comment fields.
		/// </summary>
		private Dictionary<string,string[]> field_list =
			new Dictionary<string,string[]> ();
		
		/// <summary>
		///    Contains the ventor ID.
		/// </summary>
		private string vendor_id;
		
#endregion
		
		
		
#region Constructors
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="XiphComment" /> with no contents.
		/// </summary>
		public XiphComment ()
		{
		}

		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="XiphComment" /> by reading the contents of a raw
		///    Xiph Comment from a ByteVector object.
		/// </summary>
		/// <param name="data">
		///    A ByteVector object containing a raw Xiph
		///    comment.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="data" /> is <see langword="null" />.
		/// </exception>
		public XiphComment (ByteVector data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			Parse (data);
		}
		
#endregion
		
		
		
#region Public Methods
		
		/// <summary>
		///    Gets the field data for a given field identifier.
		/// </summary>
		/// <param name="key">
		///    A <see cref="string"/> object containing the field
		///    identifier.
		/// </param>
		/// <returns>
		///    A string[] containing the field data or an
		///    empty array if the field was not found.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="key" /> is <see langword="null" />.
		/// </exception>
		public string [] GetField (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			key = key.ToUpper (CultureInfo.InvariantCulture);
			
			if (!field_list.ContainsKey (key))
				return new string [0];
			
			return (string []) field_list [key].Clone ();
		}
		
		/// <summary>
		///    Gets the first field for a given field identifier.
		/// </summary>
		/// <param name="key">
		///    A <see cref="string"/> object containing the field
		///    identifier.
		/// </param>
		/// <returns>
		///    A <see cref="string"/> containing the field data or <see
		///    langword="null" /> if the field was not found.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="key" /> is <see langword="null" />.
		/// </exception>
		public string GetFirstField (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			string [] values = GetField (key);
			return (values.Length > 0) ? values [0] : null;
		}
		
		/// <summary>
		///    Sets the contents of a specified field to a number.
		/// </summary>
		/// <param name="key">
		///    A <see cref="string"/> object containing the field
		///    identifier.
		/// </param>
		/// <param name="number">
		///    A <see cref="uint" /> value to set the field to.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="key" /> is <see langword="null" />.
		/// </exception>
		public void SetField (string key, uint number)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			if (number == 0)
				RemoveField (key);
			else
				SetField (key, number.ToString (
					CultureInfo.InvariantCulture));
		}
		
		/// <summary>
		///    Sets the contents of a specified field to the contents of
		///    a string[].
		/// </summary>
		/// <param name="key">
		///    A <see cref="string"/> object containing the field
		///    identifier.
		/// </param>
		/// <param name="values">
		///    A string[] containing the values to store
		///    in the current instance.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="key" /> is <see langword="null" />.
		/// </exception>
		public void SetField (string key, params string [] values)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			key = key.ToUpper (CultureInfo.InvariantCulture);
			
			if (values == null || values.Length == 0) {
				RemoveField (key);
				return;
			}
			
			List <string> result = new List<string> ();
			foreach (string text in values)
				if (text != null && text.Trim ().Length != 0)
					result.Add (text);
			
			if (result.Count == 0)
				RemoveField (key);
			else if (field_list.ContainsKey (key))
				field_list [key] = result.ToArray ();
			else
				field_list.Add (key, result.ToArray ());
		}
		
		/// <summary>
		///    Removes a field and all its values from the current
		///    instance.
		/// </summary>
		/// <param name="key">
		///    A <see cref="string"/> object containing the field
		///    identifier.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="key" /> is <see langword="null" />.
		/// </exception>
		public void RemoveField (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			key = key.ToUpper (CultureInfo.InvariantCulture);
			
			field_list.Remove (key);
		}
		
		/// <summary>
		///    Renders the current instance as a raw Xiph comment,
		///    optionally adding a framing bit.
		/// </summary>
		/// <param name="addFramingBit">
		///    If <see langword="true" />, a framing bit will be added to
		///    the end of the content.
		/// </param>
		/// <returns>
		///    A <see cref="ByteVector"/> object containing the rendered
		///    version of the current instance.
		/// </returns>
		public ByteVector Render (bool addFramingBit)
		{
			ByteVector data = new ByteVector ();

			// Add the vendor ID length and the vendor ID.  It's
			// important to use the length of the data(String::UTF8)
			// rather than the lenght of the the string since this
			// is UTF8 text and there may be more characters in the
			// data than in the UTF16 string.
			
			ByteVector vendor_data = ByteVector.FromString (
				vendor_id, StringType.UTF8);

			data.Add (ByteVector.FromUInt ((uint) vendor_data.Count,
				false));
			data.Add (vendor_data);

			// Add the number of fields.

			data.Add (ByteVector.FromUInt (FieldCount, false));

			foreach (KeyValuePair<string,string[]> entry in field_list) {
				// And now iterate over the values of the
				// current list.

				foreach (string value in entry.Value) {
					ByteVector field_data =
						ByteVector.FromString (
							entry.Key, StringType.UTF8);
					field_data.Add ((byte) '=');
					field_data.Add (ByteVector.FromString (
						value, StringType.UTF8));
					
					data.Add (ByteVector.FromUInt ((uint)
						field_data.Count, false));
					data.Add (field_data);
				}
			}

			// Append the "framing bit".
			if (addFramingBit)
				data.Add ((byte) 1);

			return data;
		}

#endregion
		
		
		
#region Public Properties
		
		/// <summary>
		///    Gets the number of fields contained in the current
		///    instance.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    fields in the current instance.
		/// </value>
		public uint FieldCount {
			get {
				uint count = 0;
				foreach (string [] values in field_list.Values)
					count += (uint) values.Length;
				
				return count;
			}
		}
		
		/// <summary>
		///    Gets the vendor ID for the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the vendor ID
		///    for current instance.
		/// </value>
		public string VendorId {
			get {return vendor_id;}
		}
		
#endregion
		
		
		
#region Protected Methods
		
		/// <summary>
		///    Populates and initializes a new instance of <see
		///    cref="XiphComment" /> by reading the contents of a raw
		///    Xiph Comment from a ByteVector object.
		/// </summary>
		/// <param name="data">
		///    A ByteVector object containing a raw Xiph
		///    comment.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="data" /> is <see langword="null" />.
		/// </exception>
		protected void Parse (ByteVector data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			
			// The first thing in the comment data is the vendor ID
			// length, followed by a UTF8 string with the vendor ID.
			int pos = 0;
			int vendor_length = (int) data.Mid (pos, 4)
				.ToUInt (false);
			pos += 4;

			vendor_id = data.ToString (StringType.UTF8, pos,
				vendor_length);
			pos += vendor_length;

			// Next the number of fields in the comment vector.

			int comment_fields = (int) data.Mid (pos, 4)
				.ToUInt (false);
			pos += 4;

			for(int i = 0; i < comment_fields; i++) {
				// Each comment field is in the format
				// "KEY=value" in a UTF8 string and has 4 bytes
				// before the text starts that gives the length.

				int comment_length = (int) data.Mid (pos, 4)
					.ToUInt (false);
				pos += 4;

				string comment = data.ToString (StringType.UTF8,
					pos, comment_length);
				pos += comment_length;

				int comment_separator_position = comment
					.IndexOf ('=');

				if (comment_separator_position < 0)
					continue;

				string key = comment.Substring (0,
					comment_separator_position)
					.ToUpper (
						CultureInfo.InvariantCulture);
				string value = comment.Substring (
					comment_separator_position + 1);
				string [] values;
				
				if (field_list.TryGetValue (key, out values)) {
					Array.Resize <string> (ref values,
						values.Length + 1);
					values [values.Length - 1] = value;
					field_list [key] = values;
				} else {
					SetField (key, value);
				}
			}
		}
		
#endregion
		
		
		
#region IEnumerable
		
		/// <summary>
		///    Gets an enumerator for enumerating through the the field
		///    identifiers.
		/// </summary>
		/// <returns>
		///    A <see cref="T:System.Collections.IEnumerator`1" /> for
		///    enumerating through the field identifiers.
		/// </returns>
		public IEnumerator<string> GetEnumerator ()
		{
			return field_list.Keys.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return field_list.Keys.GetEnumerator();
		}
		
#endregion
		
		
		
#region TagLib.Tag
		
		/// <summary>
		///    Gets the tag types contained in the current instance.
		/// </summary>
		/// <value>
		///    Always TagTypes.Xiph
		/// </value>
		public override TagTypes TagTypes {
			get {return TagTypes.Xiph;}
		}
		
		/// <summary>
		///    Gets and sets the title for the media described by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the title for
		///    the media described by the current instance or <see
		///    langword="null" /> if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "TITLE" field.
		/// </remarks>
		public override string Title {
			get {return GetFirstField ("TITLE");}
			set {SetField ("TITLE", value);}
		}

		/// <summary>
		///    Gets and sets the performers or artists who performed in
		///    the media described by the current instance.
		/// </summary>
		/// <value>
		///    A string[] containing the performers or
		///    artists who performed in the media described by the
		///    current instance or an empty array if no value is
		///    present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "ARTIST" field.
		/// </remarks>
		public override string [] Performers {
			get {return GetField ("ARTIST");}
			set {SetField ("ARTIST", value);}
		}
		
		/// <summary>
		///    Gets and sets the band or artist who is credited in the
		///    creation of the entire album or collection containing the
		///    media described by the current instance.
		/// </summary>
		/// <value>
		///    A string[] containing the band or artist
		///    who is credited in the creation of the entire album or
		///    collection containing the media described by the current
		///    instance or an empty array if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "ALBUMARTIST"
		///    field.
		/// </remarks>
		public override string [] AlbumArtists {
			get {
				// First try to get AlbumArtist, if that comment is not present try: 
				// ENSEMBLE: set by TAG & RENAME
				// ALBUM ARTIST: set by The GodFather
				string[] value = GetField("ALBUMARTIST");
				if (value != null && value.Length > 0)
				  return value;

				value = GetField("ALBUM ARTIST");
				if (value != null && value.Length > 0)
				  return value;

				return GetField ("ENSEMBLE"); 
			}
			set {SetField ("ALBUMARTIST", value);}
		}
		
		/// <summary>
		///    Gets and sets the composers of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A string[] containing the composers of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "COMPOSER" field.
		/// </remarks>
		public override string [] Composers {
			get {return GetField ("COMPOSER");}
			set {SetField ("COMPOSER", value);}
		}

		/// <summary>
		///    Gets and sets the album of the media represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the album of
		///    the media represented by the current instance or <see
		///    langword="null" /> if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "ALBUM" field.
		/// </remarks>
		public override string Album {
			get {return GetFirstField ("ALBUM");}
			set {SetField ("ALBUM", value);}
		}
		
		/// <summary>
		///    Gets and sets the genres of the media represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A string[] containing the genres of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "GENRE" field.
		/// </remarks>
		public override string [] Genres {
			get {return GetField ("GENRE");}
			set {SetField ("GENRE", value);}
		}
		
		/// <summary>
		///    Gets and sets the year that the media represented by the
		///    current instance was recorded.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> containing the year that the media
		///    represented by the current instance was created or zero
		///    if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "DATE" field. If a
		///    value greater than 9999 is set, this property will be
		///    cleared.
		/// </remarks>
		public override uint Year {
			get {
				string text = GetFirstField ("DATE");
				uint value;
				return (text != null && uint.TryParse (
					text.Length > 4 ? text.Substring (0, 4)
					: text, out value)) ? value : 0;
			}
			set {SetField ("DATE", value);}
		}
		
		/// <summary>
		///    Gets and sets the position of the media represented by
		///    the current instance in its containing album.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> containing the position of the
		///    media represented by the current instance in its
		///    containing album or zero if not specified.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "TRACKNUMER"
		///    field.
		/// </remarks>
		public override uint Track {
			get {
				string text = GetFirstField ("TRACKNUMBER");
				string [] values;
				uint value;
				
				if (text != null && (values = text.Split ('/'))
					.Length > 0 && uint.TryParse (
						values [0], out value))
					return value;
				
				return 0;
			}
			set {
				SetField ("TRACKNUMBER", value);
			}
		}
		
		/// <summary>
		///    Gets and sets the lyrics or script of the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the lyrics or
		///    script of the media represented by the current instance
		///    or <see langword="null" /> if no value is present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "LYRICS" field.
		/// </remarks>
		public override string Lyrics {
			get {return GetFirstField ("LYRICS");}
			set {SetField ("LYRICS", value);}
		}
		
		/// <summary>
		///    Gets and sets the conductor or director of the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the conductor
		///    or director of the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "CONDUCTOR" field.
		/// </remarks>
		public override string Conductor {
			get {return GetFirstField ("CONDUCTOR");}
			set {SetField ("CONDUCTOR", value);}
		}
		
		/// <summary>
		///    Gets and sets the MusicBrainz Artist ID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the MusicBrainz
		///    ArtistID for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "MUSICBRAINZ_ARTISTID" field.
		/// </remarks>
		public override string MusicBrainzArtistId {
			get {return GetFirstField ("MUSICBRAINZ_ARTISTID");}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Release ID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the MusicBrainz
		///    ReleaseID for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "MUSICBRAINZ_ALBUMID" field.
		/// </remarks>
		public override string MusicBrainzReleaseId {
			get {return GetFirstField ("MUSICBRAINZ_ALBUMID");}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Track ID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the MusicBrainz
		///    TrackID for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "MUSICBRAINZ_TRACKID" field.
		/// </remarks>
		public override string MusicBrainzTrackId {
			get {return GetFirstField ("MUSICBRAINZ_TRACKID");}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Disc ID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the MusicBrainz
		///    DiscID for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "MUSICBRAINZ_DISCID" field.
		/// </remarks>
		public override string MusicBrainzDiscId {
			get {return GetFirstField ("MUSICBRAINZ_DISCID");}
		}

		/// <summary>
		///    Gets and sets the MusicIP PUID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the MusicIP PUID
		///    for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "MUSICIP_PUID" field.
		/// </remarks>
		public override string MusicIpId {
			get {return GetFirstField ("MUSICIP_PUID");}
		}

		/// <summary>
		///    Gets and sets the Amazon ID for the media
		///    represented by the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing the AmazonID
		///    for the media represented by the current
		///    instance or <see langword="null" /> if no value present.
		/// </value>
		/// <remarks>
		///    This property is implemented using the "ASIN" field.
		/// </remarks>
		public override string AmazonId {
			get {return GetFirstField ("ASIN");}
		}

		/// <summary>
		///    Gets and sets a collection of pictures associated with
		///    the media represented by the current instance.
		/// </summary>
		/// <value>
		///    A IPicture[] containing a collection of
		///    pictures associated with the media represented by the
		///    current instance or an empty array if none are present.
		/// </value>
		/// <remarks>
		///    <para>This property is implemented using the METADATA_BLOCK_PICTURE
		///    field. However, the old COVERART field is still supported when reading
		///    the tags but will be converted to the METADATA_BLOCK_PICTURE when
		///    saving the tags.</para>
		/// </remarks>
		public override IPicture [] Pictures {
			get {
				string[] covers = GetField("METADATA_BLOCK_PICTURE");
				if (covers.Length > 0)
				{
					// use the new Flac picture types
					IPicture[] pictures = new Flac.Picture[covers.Length];
					for (int ii = 0; ii < covers.Length; ii++)
					{
						ByteVector data = new ByteVector(Convert.FromBase64String(covers[ii]));
						pictures[ii] = new Flac.Picture(data);
					}
					return pictures;
				}
				else
				{
					// try the old coverart
					covers = GetField("COVERART");
					IPicture[] pictures = new Picture[covers.Length];
					for (int ii = 0; ii < covers.Length; ii++)
					{
						ByteVector data = new ByteVector(Convert.FromBase64String(covers[ii]));
						pictures[ii] = new Flac.Picture(new Picture(data));
					}
					return pictures;
				}
			}
		}

        /// <summary>
        ///    Gets and sets whether or not the album described by the
        ///    current instance is a compilation.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    album described by the current instance is a compilation.
        /// </value>
        /// <remarks>
        ///    This property is implemented using the "COMPILATION" field.
        /// </remarks>
        public bool IsCompilation
        {
            get
            {
                string text;
                int value;

                if ((text = GetFirstField("COMPILATION")) !=
                    null && int.TryParse(text, out value))
                {
                    return value == 1;
                }
                return false;
            }
            set
            {
                if (value)
                {
                    SetField("COMPILATION", "1");
                }
                else
                {
                    RemoveField("COMPILATION");
                }
            }
        }

		/// <summary>
		///    Gets whether or not the current instance is empty.
		/// </summary>
		/// <value>
		///    <see langword="true" /> if the current instance does not
		///    any values. Otherwise <see langword="false" />.
		/// </value>
		public override bool IsEmpty {
			get {
				foreach (string [] values in field_list.Values)
					if (values.Length != 0)
						return false;
				
				return true;
			}
		}
		
		/// <summary>
		///    Clears the values stored in the current instance.
		/// </summary>
		public override void Clear ()
		{
			field_list.Clear ();
		}
		
#endregion
	}
}
