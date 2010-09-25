using System;
using OpenMobile;
using MusicBrainz;
using System.Xml;
using System.Collections.Generic;
using OpenMobile.Graphics;
using OpenMobile.Media;
namespace OMLinPlayer
{
	public static class CDDBClient
	{
		public static mediaInfo[] getTracks(string path)
		{
			LocalDisc disc=LocalDisc.GetFromDevice(path);
			disc.Init();
			string url="http://musicbrainz.org/ws/1/release/?type=xml&discid="+disc.Id;
			XmlReader reader=XmlReader.Create(url);
			reader.ReadStartElement();
			string albumName="Unknown Album";
			List<mediaInfo> tracks=new List<mediaInfo>();
			mediaInfo currentTrack=null;
			string lastElement="";
			OImage cover=null;
			string albumArtist=null;
			while(reader.Read())
			{
				switch(reader.NodeType)
				{
					case XmlNodeType.Element:
						lastElement=reader.Name;
						if (lastElement=="track")
						{
							if (currentTrack!=null)
							{
								if (string.IsNullOrEmpty(currentTrack.Artist))
									currentTrack.Artist=albumArtist;
								tracks.Add(currentTrack);
							}
							currentTrack=new mediaInfo("cdda://"+(tracks.Count+1).ToString());
							currentTrack.Album=albumName;
							currentTrack.Type=eMediaType.AudioCD;
							currentTrack.coverArt=cover;
						}
						break;
					case XmlNodeType.Text:
						switch(lastElement)
						{
							case "title":
								if (currentTrack==null)
									albumName=reader.Value;
								else
									currentTrack.Name=reader.Value;
								break;
							case "name":
								if (albumArtist==null)
								{
									albumArtist=reader.Value;
									cover=TagReader.getLastFMImage(albumArtist,albumName);
								}
								else if (currentTrack!=null)
									currentTrack.Artist=reader.Value;
								break;
							case "duration":
								currentTrack.Length=(int.Parse(reader.Value)/1000);
								break;
						}
						break;
				}
			}
			if (currentTrack!=null)
			{
				if (string.IsNullOrEmpty(currentTrack.Artist))
					currentTrack.Artist=albumArtist;
				tracks.Add(currentTrack);
			};
			return tracks.ToArray();
		}
	}
}

