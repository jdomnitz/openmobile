using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OpenMobile.Media
{
    public static class MediaLoader
    {
        public static bool loadArtists(IPluginHost host, ref OpenMobile.Controls.IList list)
        {
            PluginSettings ps = new PluginSettings();
            string dbname = ps.getSetting("Default.MusicDatabase");
            ps.Dispose();
            if (dbname == "")
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            list.Clear();
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetArtists(false);
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    list.Add(info.Artist);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return false;
            
        }
    }
}
