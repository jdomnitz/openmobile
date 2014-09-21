using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenMobile.Graphics;

namespace OMMediaDB2
{
    public class DB_DataItem
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Location { get; set; }
        public int TrackNumber { get; set; }
        public string Genre { get; set; }
        public string Lyrics { get; set; }
        public int Rating { get; set; }
        public string Type { get; set; }
        public byte[] CoverArt { get; set; }

        public OImage GetCoverArt()
        {
            try
            {
                MemoryStream m = new MemoryStream(CoverArt);
                return OImage.FromStream(m);
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2} [{3}]", Artist, Album, Name, Location);
        }
    }
}
