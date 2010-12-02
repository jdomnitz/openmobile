//
// Tag.cs: This abstract class provides generic access to standard tag
// features. All tag types will extend this class.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   tag.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 Scott Wheeler
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

namespace TagLib {
	/// <summary>
	///    Indicates the tag types used by a file.
	/// </summary>
	[Flags]
	public enum TagTypes : uint
	{
		/// <summary>
		///    No tag types.
		/// </summary>
		None         = 0x00000000,
		
		/// <summary>
		///    Xiph's Vorbis Comment
		/// </summary>
		Xiph         = 0x00000001,
		
		/// <summary>
		///    ID3v1 Tag
		/// </summary>
		Id3v1        = 0x00000002,
		
		/// <summary>
		///    ID3v2 Tag
		/// </summary>
		Id3v2        = 0x00000004,
		
		/// <summary>
		///    APE Tag
		/// </summary>
		Ape          = 0x00000008,
		
		/// <summary>
		///    Apple's ILST Tag Format
		/// </summary>
		Apple        = 0x00000010,
		
		/// <summary>
		///    ASF Tag
		/// </summary>
		Asf          = 0x00000020,
		
		/// <summary>
		///    Standard RIFF INFO List Tag
		/// </summary>
		RiffInfo     = 0x00000040,
		
		/// <summary>
		///    RIFF Movie ID List Tag
		/// </summary>
		MovieId      = 0x00000080,
		
		/// <summary>
		///    DivX Tag
		/// </summary>
		DivX         = 0x00000100,
		
		/// <summary>
		///    FLAC Metadata Blocks Tag
		/// </summary>
		FlacMetadata = 0x00000200,
		
        /// <summary>
		///    Audible Metadata Blocks Tag
		/// </summary>
		AudibleMetadata = 0x00000400,

        /// <summary>
        /// RMF Metadata
        /// </summary>
        RMFMetadata  = 0x00000800,

        QuicktimeAtoms=0x00001000,

        XMTags=0x00002000,

		/// <summary>
		///    All tag types.
		/// </summary>
		AllTags      = 0xFFFFFFFF
	}
	
	/// <summary>
	///    This abstract class provides generic access to standard tag
	///    features. All tag types will extend this class.
	/// </summary>
	/// <remarks>
	///    Because not every tag type supports the same features, it may be
	///    useful to check that the value is stored by re-reading the
	///    property after it is stored.
	/// </remarks>
	public abstract class Tag
	{
		/// <summary>
		///    Gets the tag types contained in the current instance.
		/// </summary>
		/// <value>
		///    A bitwise combined <see cref="TagLib.TagTypes" />
		///    containing the tag types contained in the current
		///    instance.
		/// </value>
		/// <remarks>
		///    For a standard tag, the value should be intuitive. For
		///    example, <see cref="TagLib.Id3v2.Tag" /> objects have a
		///    value of <see cref="TagLib.TagTypes.Id3v2" />. However,
		///    for tags of type <see cref="TagLib.CombinedTag" /> may
		///    contain multiple or no types.
		/// </remarks>
		public abstract TagTypes TagTypes {get;}
		
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
		///    The title is most commonly the name of the song or
		///    episode or a movie title. For example, "Daydream
		///    Believer" (a song by the Monkies), "Space Seed" (an
		///    episode of Star Trek), or "Harold and Kumar Go To White
		///    Castle" (a movie).
		/// </remarks>
		public virtual string Title {
			get {return null;}
			set {}
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
		///    <para>This field is most commonly called "Artists" in
		///    media applications and should be used to represent each
		///    of the artists appearing in the media. It can be simple
		///    in theform of "The Beatles", or more complicated in the
		///    form of "John Lennon, Paul McCartney, George Harrison,
		///    Pete Best", depending on the preferences of the listener
		///    and the degree to which they organize their media
		///    collection.</para>
		///    <para>As the preference of the user may vary,
		///    applications should not try to limit the user in what
		///    choice they may make.</para>
		/// </remarks>
		public virtual string [] Performers {
			get {return new string [] {};}
			set {}
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
		///    <para>This field is typically optional but aids in the
		///    sorting of compilations or albums with multiple artists.
		///    For example, if an album has several artists, sorting by
		///    artist will split up the album and sorting by album will
		///    split up albums by the same artist. Having a single album
		///    artist for an entire album will solve this
		///    problem.</para>
		///    <para>As this value is to be used as a sorting key, it
		///    should be used with less variation than <see
		///    cref="Performers" />. Where performers can be broken into
		///    muliple artist it is best to stick with a single band
		///    name. For example, "The Beatles".</para>
		/// </remarks>
		public virtual string [] AlbumArtists {
			get {return new string [] {};}
			set {}
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
		///    <para>This field represents the composers, song writers,
		///    script writers, or persons who claim authorship of the
		///    media.</para>
		/// </remarks>
		public virtual string [] Composers {
			get {return new string [] {};}
			set {}
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
		///    <para>This field represents the name of the album the
		///    media belongs to. In the case of a boxed set, it should
		///    be the name of the entire set rather than the individual
		///    disc.</para>
		///    <para>For example, "Rubber Soul" (an album by the
		///    Beatles), "The Sopranos: Complete First Season" (a boxed
		///    set of TV episodes), or "Back To The Future Trilogy" (a 
		///    boxed set of movies).</para>
		/// </remarks>
		public virtual string Album {
			get {return null;}
			set {}
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
		///    <para>This field represents genres that apply to the song
		///    or album. This is often used for filtering media.</para>
		///    <para>A list of common audio genres as popularized by
		///    ID3v1, are stored in Genres.Audio.
		///    Additionally, Genres.Video contains video
		///    genres as used by DivX.</para>
		/// </remarks>
		public virtual string [] Genres {
			get {return new string [] {};}
			set {}
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
		///    <para>Years greater than 9999 cannot be stored by most
		///    tagging formats and will be cleared if a higher value is
		///    set.</para>
		///    <para>Some tagging formats store higher precision dates
		///    which will be truncated when this property is set. Format
		///    specific implementations are necessary access the higher
		///    precision values.</para>
		/// </remarks>
		public virtual uint Year {
			get {return 0;}
			set {}
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
		///    <para>This value should be the same as is listed on the
		///    album cover and no more than <see cref="TrackCount"
		///    /> if <see cref="TrackCount" /> is non-zero.</para>
		/// </remarks>
		public virtual uint Track {
			get {return 0;}
			set {}
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
		///    <para>This field contains a plain text representation of
		///    the lyrics or scripts with line breaks and whitespace
		///    being the only formatting marks.</para>
		///    <para>Some formats support more advances lyrics, like
		///    synchronized lyrics, but those must be accessed using
		///    format specific implementations.</para>
		/// </remarks>
		public virtual string Lyrics {
			get {return null;}
			set {}
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
		///    <para>This field is most useful for organizing classical
		///    music and movies.</para>
		/// </remarks>
		public virtual string Conductor {
			get {return null;}
			set {}
		}
		
		/// <summary>
		///    Gets and sets the MusicBrainz Artist ID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz ArtistID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the MusicBrainz ArtistID, and is used
		///    to uniquely identify a particular Artist of the track.</para>
		/// </remarks>
		public virtual string MusicBrainzArtistId {
			get { return null; }
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Release ID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz ReleaseID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the MusicBrainz ReleaseID, and is used
		///    to uniquely identify a particular Release to which this track belongs.</para>
		/// </remarks>
		public virtual string MusicBrainzReleaseId {
			get { return null; }
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Track ID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz TrackID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the MusicBrainz TrackID, and is used
		///    to uniquely identify a particular track.</para>
		/// </remarks>
		public virtual string MusicBrainzTrackId {
			get { return null; }
		}

		/// <summary>
		///    Gets and sets the MusicBrainz Disc ID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicBrainz DiscID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the MusicBrainz DiscID, and is used
		///    to uniquely identify the particular Released Media associated with
		///    this track.</para>
		/// </remarks>
		public virtual string MusicBrainzDiscId {
			get { return null; }
		}

		/// <summary>
		///    Gets and sets the MusicIP PUID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the MusicIP PUID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the MusicIP PUID, and is an acoustic
		///    fingerprint identifier.  It Identifies what this track "Sounds Like".</para>
		/// </remarks>
		public virtual string MusicIpId {
			get { return null; }
			set {}
		}

		/// <summary>
		///    Gets and sets the Amazon ID of the media represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> containing the AmazonID of the
		///    media represented by the current instance or an empty
		///    array if no value is present.
		/// </value>
		/// <remarks>
		///    <para>This field represents the AmazonID, and is used
		///    to identify the particular track or album in the Amazon Catalog.</para>
		/// </remarks>
		public virtual string AmazonId {
			get { return null; }
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
		///    <para>Typically, this value is used to store an album
		///    cover or icon to use for the file, but it is capable of
		///    holding any type of image, including pictures of the
		///    band, the recording studio, the concert, etc.</para>
		/// </remarks>
		public virtual IPicture [] Pictures {
			get {return new Picture [] {};}
			set {}
		}
		
		/// <summary>
		///    Gets the first value contained in <see
		///    cref="AlbumArtists" />.
		/// </summary>
		/// <value>
		///    The first <see cref="string" /> object in <see
		///    cref="AlbumArtists" />, or <see langword="null" /> is it
		///    contains no values.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="AlbumArtists" /> to set the value.
		/// </remarks>
		public string FirstAlbumArtist {
			get {return FirstInGroup(AlbumArtists);}
		}
		
		/// <summary>
		///    Gets the first value contained in <see
		///    cref="Performers" />.
		/// </summary>
		/// <value>
		///    The first <see cref="string" /> object in <see
		///    cref="Performers" />, or <see langword="null" /> is it
		///    contains no values.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Performers" /> to set the value.
		/// </remarks>
		public string FirstPerformer {
			get {return FirstInGroup(Performers);}
		}
		
		/// <summary>
		///    Gets the first value contained in <see
		///    cref="Composers" />.
		/// </summary>
		/// <value>
		///    The first <see cref="string" /> object in <see
		///    cref="Composers" />, or <see langword="null" /> is it
		///    contains no values.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Composers" /> to set the value.
		/// </remarks>
		public string FirstComposer {
			get {return FirstInGroup(Composers);}
		}
		
		/// <summary>
		///    Gets the first value contained in <see cref="Genres" />.
		/// </summary>
		/// <value>
		///    The first <see cref="string" /> object in <see
		///    cref="Genres" />, or <see langword="null" /> is it
		///    contains no values.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Genres" /> to set the value.
		/// </remarks>
		public string FirstGenre {
			get {return FirstInGroup(Genres);}
		}
		
		/// <summary>
		///    Gets a semicolon separated string containing the values
		///    in <see cref="AlbumArtists" />.
		/// </summary>
		/// <value>
		///    A semicolon separated <see cref="string" /> object
		///    containing the values in <see cref="AlbumArtists" />.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="AlbumArtists" /> to set the value.
		/// </remarks>
		public string JoinedAlbumArtists {
			get {return JoinGroup(AlbumArtists);}
		}
		
		/// <summary>
		///    Gets a semicolon separated string containing the values
		///    in <see cref="Performers" />.
		/// </summary>
		/// <value>
		///    A semicolon separated <see cref="string" /> object
		///    containing the values in <see cref="Performers" />.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Performers" /> to set the value.
		/// </remarks>
		public string JoinedPerformers {
			get {return JoinGroup(Performers);}
		}
		
		/// <summary>
		///    Gets a semicolon separated string containing the values
		///    in <see cref="Composers" />.
		/// </summary>
		/// <value>
		///    A semicolon separated <see cref="string" /> object
		///    containing the values in <see cref="Composers" />.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Composers" /> to set the value.
		/// </remarks>
		public string JoinedComposers {
			get {return JoinGroup(Composers);}
		}
		
		/// <summary>
		///    Gets a semicolon separated string containing the values
		///    in <see cref="Genres" />.
		/// </summary>
		/// <value>
		///    A semicolon separated <see cref="string" /> object
		///    containing the values in <see cref="Genres" />.
		/// </value>
		/// <remarks>
		///    This property is provided for convenience. Use <see
		///    cref="Genres" /> to set the value.
		/// </remarks>
		public string JoinedGenres {
			get {return JoinGroup(Genres);}
		}
		
		/// <summary>
		///    Gets the first string in an array.
		/// </summary>
		/// <param name="group">
		///    A string[] to get the first string from.
		/// </param>
		/// <returns>
		///    The first <see cref="string" /> object contained in
		///    <paramref name="group" />, or <see langword="null" /> if
		///    the array is <see langword="null" /> or empty.
		/// </returns>
		private static string FirstInGroup(string [] group)
		{
			return group == null || group.Length == 0 ?
				null : group [0];
		}
		
		/// <summary>
		///    Joins a array of strings into a single, semicolon
		///    separated, string.
		/// </summary>
		/// <param name="group">
		///    A string[] containing values to combine.
		/// </param>
		/// <returns>
		///    A semicolon separated <see cref="string" /> object
		///    containing the values from <paramref name="group" />.
		/// </returns>
		private static string JoinGroup (string [] group)
		{
			if (group == null)
				return null;
			
			return string.Join ("; ", group);
		}

		/// <summary>
		///    Gets whether or not the current instance is empty.
		/// </summary>
		/// <value>
		///    <see langword="true" /> if the current instance does not
		///    any values. Otherwise <see langword="false" />.
		/// </value>
		/// <remarks>
		///    In the default implementation, this checks the values
		///    supported by <see cref="Tag" />, but it may be extended
		///    by child classes to support other values.
		/// </remarks>
		public virtual bool IsEmpty {
			get {
				return IsNullOrLikeEmpty (Title) &&
				IsNullOrLikeEmpty (AlbumArtists) &&
				IsNullOrLikeEmpty (Performers) &&
				IsNullOrLikeEmpty (Composers) &&
				IsNullOrLikeEmpty (Conductor) &&
				IsNullOrLikeEmpty (Album) &&
				IsNullOrLikeEmpty (Genres) &&
				Year == 0 &&
				Track == 0;
			}
		}
		
		/// <summary>
		///    Clears the values stored in the current instance.
		/// </summary>
		/// <remarks>
		///    The clearing procedure is format specific and should
		///    clear all values.
		/// </remarks>
		public abstract void Clear ();
		
		/// <summary>
		///    Copies the values from the current instance to another
		///    <see cref="TagLib.Tag" />, optionally overwriting
		///    existing values.
		/// </summary>
		/// <param name="target">
		///    A <see cref="Tag" /> object containing the target tag to
		///    copy values to.
		/// </param>
		/// <param name="overwrite">
		///    A <see cref="bool" /> specifying whether or not to copy
		///    values over existing one.
		/// </param>
		/// <remarks>
		///    <para>This method only copies the most basic values when
		///    copying between different tag formats, however, if
		///    <paramref name="target" /> is of the same type as the
		///    current instance, more advanced copying may be done.
		///    For example, <see cref="TagLib.Id3v2.Tag" /> will copy
		///    all of its frames to another tag.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="target" /> is <see langword="null" />.
		/// </exception>
		public virtual void CopyTo (Tag target, bool overwrite)
		{
			if (target == null)
				throw new ArgumentNullException ("target");
			
			if (overwrite || IsNullOrLikeEmpty (target.Title))
				target.Title = Title;
			
			if (overwrite || IsNullOrLikeEmpty (target.AlbumArtists))
				target.AlbumArtists = AlbumArtists;
			
			if (overwrite || IsNullOrLikeEmpty (target.Performers))
				target.Performers = Performers;
			
			if (overwrite || IsNullOrLikeEmpty (target.Composers))
				target.Composers = Composers;
			
			if (overwrite || IsNullOrLikeEmpty (target.Album))
				target.Album = Album;
			
			if (overwrite || IsNullOrLikeEmpty (target.Genres))
				target.Genres = Genres;
			
			if (overwrite || target.Year == 0)
				target.Year = Year;
			
			if (overwrite || target.Track == 0)
				target.Track = Track;
			
			if (overwrite || IsNullOrLikeEmpty (target.Conductor))
				target.Conductor = Conductor;
		}
		
		/// <summary>
		///    Checks if a <see cref="string" /> is <see langword="null"
		///    /> or contains only whitespace characters.
		/// </summary>
		/// <param name="value">
		///    A <see cref="string" /> object to check.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if the string is <see
		///    langword="null" /> or contains only whitespace
		///    characters. Otherwise <see langword="false" />.
		/// </returns>
		private static bool IsNullOrLikeEmpty (string value)
		{
			return value == null || value.Trim ().Length == 0;
		}
		
		/// <summary>
		///    Checks if all the strings in the array return <see
		///    langword="true" /> with <see
		///    cref="IsNullOrLikeEmpty(string)" /> or if the array is
		///    <see langword="null" /> or is empty.
		/// </summary>
		/// <param name="value">
		///    A string[] to check the contents of.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if the array is <see
		///    langword="null" /> or empty, or all elements return <see
		///    langword="true" /> for <see
		///    cref="IsNullOrLikeEmpty(string)" />. Otherwise <see
		///    langword="false" />.
		/// </returns>
		private static bool IsNullOrLikeEmpty (string [] value)
		{
			if (value == null)
				return true;
			
			foreach (string s in value)
				if (!IsNullOrLikeEmpty (s))
					return false;
			
			return true;
		}
	}
}
