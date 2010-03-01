/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
// Note: The FreeDB and Ripper classes have been modified from their original versions
// Outdated code was removed and the Track class was modified to seperate artist from title
//*******************************************************************************
using System;
using System.Collections.Generic;
using Freedb;
using OpenMobile;
using Ripper;

namespace OMPlayer
{
    internal static class CDDB
    {
        //Remove this if unused
        public static mediaInfo[] getInfo(string path)
        {
            if ((path == null) || (path.Length < 1))
                return null;
            CDDrive m_drive = new CDDrive();
            if (!m_drive.Open(path[0]))
                return null;
            if (!m_drive.IsCDReady())
                return null;

            m_drive.LockCD();
            m_drive.Refresh();
            m_drive.UnLockCD();
            
            string query;
            try
            {
                query = m_drive.GetCDDBQuery();
            }
            catch (Exception)
            {
                return null;
            }

            QueryResult queryResult;
            List<QueryResult> coll;
            FreedbHelper m_freedb = new FreedbHelper();
            m_freedb.UserName = "openMobile";
			m_freedb.Hostname = "Unknown";
			m_freedb.ClientName = "OMPlayer";
			m_freedb.Version = "1.0";
            m_freedb.SetDefaultSiteAddress("freedb.org");
            string code = string.Empty;
            try
            {
                code = m_freedb.Query(query, out queryResult, out coll);
            }
            catch (Exception)
            {
                return null;
            }

            //inexact match or multiple mathes show selection dialog
            if (code == FreedbHelper.ResponseCodes.CODE_210 ||
                code == FreedbHelper.ResponseCodes.CODE_211)
            {
                queryResult = coll[0];
            }
            else if (code != FreedbHelper.ResponseCodes.CODE_200)
            {
                return null;
            }

            //queryResult now contains the results of our query.

            CDEntry cdEntry;
            code = m_freedb.Read(queryResult, out cdEntry);
            if (code != FreedbHelper.ResponseCodes.CODE_210)
                return null;
            List<mediaInfo> info=new List<mediaInfo>();
            for(int i=0;i<cdEntry.NumberOfTracks;i++)
            {
                mediaInfo inf=new mediaInfo();
                inf.Genre=cdEntry.Genre;
                inf.Album = cdEntry.Title;
                inf.Name = cdEntry.Tracks[i].Title;
                inf.Artist = cdEntry.Tracks[i].Artist;
                inf.TrackNumber = i+1;
                info.Add(inf);
            }
            return info.ToArray();
        }
        //keep this one instead
        public static mediaInfo getSongInfo(string path)
        {
            if ((path == null) || (path.Length < 1))
                return null;
            if (OpenMobile.Net.Network.IsAvailable == false)
                return null;
            CDDrive m_drive = new CDDrive();
            if (!m_drive.Open(path[0]))
                return null;
            if (!m_drive.IsCDReady())
                return null;

            m_drive.LockCD();
            m_drive.Refresh();
            m_drive.UnLockCD();
            
            string query;
            try
            {
                query = m_drive.GetCDDBQuery();
            }
            catch (Exception)
            {
                return null;
            }

            QueryResult queryResult;
            List<QueryResult> coll;
            FreedbHelper m_freedb = new FreedbHelper();
            m_freedb.UserName = "openMobile";
			m_freedb.Hostname = "Unknown";
			m_freedb.ClientName = "OMPlayer";
			m_freedb.Version = "1.0";
            m_freedb.SetDefaultSiteAddress("freedb.org");
            string code = string.Empty;
            try
            {
                code = m_freedb.Query(query, out queryResult, out coll);
            }
            catch (Exception)
            {
                return null;
            }

            //inexact match or multiple mathes show selection dialog
            if (code == FreedbHelper.ResponseCodes.CODE_210 ||
                code == FreedbHelper.ResponseCodes.CODE_211)
            {
                queryResult = coll[0];
            }
            else if (code != FreedbHelper.ResponseCodes.CODE_200)
            {
                return null;
            }

            //queryResult now contains the results of our query.

            CDEntry cdEntry;
            code = m_freedb.Read(queryResult, out cdEntry);
            if (code != FreedbHelper.ResponseCodes.CODE_210)
                return null;
            int trackNum;
            path = Path.GetFileNameWithoutExtension(path);
            if (int.TryParse(path.ToLower().Replace("track", ""), out trackNum) ==false)
                return null;
            mediaInfo info=new mediaInfo();
            info.Genre=cdEntry.Genre;
            info.Album = cdEntry.Title;
            info.Name = cdEntry.Tracks[trackNum-1].Title;
            info.Artist = cdEntry.Tracks[trackNum-1].Artist;
            info.TrackNumber = trackNum;
            return info;
        }
    }
}
