//
// CombinedTag.cs: Combines a collection of tags so that they behave as one.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2005-2007 Brian Nickel
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

using System.Collections.Generic;

namespace TagLib {
	/// <summary>
	///    This class combines a collection of tags so that they behave as
	///    one.
	/// </summary>
	public class CombinedTag : Tag
	{
		#region Private Fields
		
		/// <summary>
		///    Contains tags to be combined.
		/// </summary>
		private List<Tag> tags;
		
		#endregion
		
		
		
		#region Constructors
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="CombinedTag" /> with no internal tags.
		/// </summary>
		/// <remarks>
		///    You can set the tags in the new instance later using
		///    <see cref="SetTags" />.
		/// </remarks>
		public CombinedTag ()
		{
			this.tags = new List<Tag> ();
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="CombinedTag" /> with a specified collection of
		///    tags.
		/// </summary>
		/// <param name="tags">
		///    A Tag[] containing a collection of tags to
		///    combine in the new instance.
		/// </param>
		public CombinedTag (params Tag [] tags)
		{
			this.tags = new List<Tag> (tags);
		}
		
		#endregion
		
		
		
		#region Public Properties
		
		/// <summary>
		///    Gets the tags combined in the current instance.
		/// </summary>
		/// <value>
		///    A Tag[] containing the tags combined in
		///    the current instance.
		/// </value>
		public virtual Tag [] Tags {
			get {return tags.ToArray ();}
		}
		
		#endregion
		
		
		
		#region Public Methods
		
		/// <summary>
		///    Sets the child tags to combine in the current instance.
		/// </summary>
		/// <param name="tags">
		///    A Tag[] containing the tags to combine.
		/// </param>
		public void SetTags (params Tag [] tags)
		{
			this.tags.Clear ();
			this.tags.AddRange (tags);
		}
		
		#endregion
		
		
		
		#region Protected Methods
		
		/// <summary>
		///    Inserts a tag into the collection of tags in the current
		///    instance.
		/// </summary>
		/// <param name="index">
		///    A <see cref="int" /> value specifying the index at which
		///    to insert the tag.
		/// </param>
		/// <param name="tag">
		///    A <see cref="Tag" /> object to insert into the collection
		///    of tags.
		/// </param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///    <paramref name="index" /> is less than zero or greater
		///    than the count.
		/// </exception>
		protected void InsertTag (int index, Tag tag)
		{
			this.tags.Insert (index, tag);
		}
		
		/// <summary>
		///    Adds a tag at the end of the collection of tags in the
		///    current instance.
		/// </summary>
		/// <param name="tag">
		///    A <see cref="Tag" /> object to add to the collection of
		///    tags.
		/// </param>
		protected void AddTag (Tag tag)
		{
			this.tags.Add (tag);
		}
		
		/// <summary>
		///    Clears the tag collection in the current instance.
		/// </summary>
		protected void ClearTags ()
		{
			this.tags.Clear ();
		}
		
		#endregion
		
		
		
		#region Overrides
		
		/// <summary>
		///    Gets the tag types contained in the current instance.
		/// </summary>
		/// <value>
		///    A bitwise combined <see cref="TagLib.TagTypes" />
		///    containing the tag types contained in the current
		///    instance.
		/// </value>
		/// <remarks>
		///    This value contains a bitwise combined value from all the
		///    child tags.
		/// </remarks>
		/// <seealso cref="Tag.TagTypes" />
		public override TagTypes TagTypes {
			get {
				TagTypes types = TagTypes.None;
				foreach (Tag tag in tags)
					if (tag != null)
						types |= tag.TagTypes;
				
				return types;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Title" />
		public override string Title {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.Title;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Title = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Performers" />
		public override string [] Performers {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string [] value = tag.Performers;
					
					if (value != null && value.Length > 0)
						return value;
				}
				
				return new string [] {};
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Performers = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.AlbumArtists" />
		public override string [] AlbumArtists {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string [] value = tag.AlbumArtists;
					
					if (value != null && value.Length > 0)
						return value;
				}
				
				return new string [] {};
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.AlbumArtists = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Composers" />
		public override string [] Composers {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string [] value = tag.Composers;
					
					if (value != null && value.Length > 0)
						return value;
				}
				
				return new string [] {};
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Composers = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Album" />
		public override string Album {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.Album;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Album = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Genres" />
		public override string [] Genres {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string [] value = tag.Genres;
					
					if (value != null && value.Length > 0)
						return value;
				}
				
				return new string [] {};
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Genres = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-zero value is
		///    returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Year" />
		public override uint Year {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					uint value = tag.Year;
					
					if (value != 0)
						return value;
				}
				
				return 0;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Year = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-zero value is
		///    returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Track" />
		public override uint Track {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					uint value = tag.Track;
					
					if (value != 0)
						return value;
				}
				
				return 0;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Track = value;
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Lyrics" />
		public override string Lyrics {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.Lyrics;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Lyrics = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Conductor" />
		public override string Conductor {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.Conductor;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.Conductor = value;
			}
		}
		
		/// <summary>
		///    Gets and sets the MusicBrainz Artist ID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz
		///    ArtistID for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.MusicBrainzArtistId" />
		public override string MusicBrainzArtistId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.MusicBrainzArtistId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.MusicBrainzArtistId = value;
			}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Release ID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz
		///    ReleaseID for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.MusicBrainzReleaseId" />
		public override string MusicBrainzReleaseId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.MusicBrainzReleaseId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.MusicBrainzReleaseId = value;
			}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Track ID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz
		///    TrackID for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.MusicBrainzTrackId" />
		public override string MusicBrainzTrackId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.MusicBrainzTrackId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.MusicBrainzTrackId = value;
			}
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Disc ID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz
		///    DiscID for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.MusicBrainzDiscId" />
		public override string MusicBrainzDiscId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.MusicBrainzDiscId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.MusicBrainzDiscId = value;
			}
		}

		/// <summary>
		///    Gets and sets the MusicIP PUID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicIP PUID
		///    for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.MusicIpId" />
		public override string MusicIpId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.MusicIpId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.MusicIpId = value;
			}
		}

		/// <summary>
		///    Gets and sets the Amazon ID.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the Amazon Id
		///    for the media described by the 
		///    current instance or null if no value is present.
		/// </value>
		/// <remarks>
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.AmazonId" />
		public override string AmazonId {
			get {
				foreach (Tag tag in tags) {
					if (tag == null)
						continue;
					
					string value = tag.AmazonId;
					
					if (value != null)
						return value;
				}
				
				return null;
			}
			
			set {
				foreach (Tag tag in tags)
					if (tag != null)
						tag.AmazonId = value;
			}
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
		///    <para>When getting the value, the child tags are looped
		///    through in order and the first non-<see langword="null" />
		///    and non-empty value is returned.</para>
		///    <para>When setting the value, it is stored in each child
		///    tag.</para>
		/// </remarks>
		/// <seealso cref="Tag.Pictures" />
		public override IPicture [] Pictures {
			get {
				foreach(Tag tag in tags) {
					if (tag == null)
						continue;
					
					IPicture [] value = tag.Pictures;
					
					if (value != null && value.Length > 0)
						return value;
				}
				
				return base.Pictures;
			}
			
			set {
				foreach(Tag tag in tags)
					if(tag != null)
						tag.Pictures = value;
			}
		}
		
		/// <summary>
		///    Gets whether or not the current instance is empty.
		/// </summary>
		/// <value>
		///    <see langword="true" /> if all the child tags are empty.
		///    Otherwise <see langword="false" />.
		/// </value>
		/// <seealso cref="Tag.IsEmpty" />
		public override bool IsEmpty {
			get {
				foreach (Tag tag in tags)
					if (tag.IsEmpty)
						return true;
				
				return false;
			}
		}
		
		/// <summary>
		///    Clears all of the child tags.
		/// </summary>
		public override void Clear ()
		{
			foreach (Tag tag in tags)
				tag.Clear ();
		}
		
		#endregion
	}
}
