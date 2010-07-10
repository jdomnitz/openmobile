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
using System;
using System.Net;
using System.IO;
using System.Drawing;
using System.Text;

namespace OpenMobile.Net
{
    /// <summary>
    /// Handles general network/internet information
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// The hostname of the local computer
        /// </summary>
        public static string hostname=System.Net.Dns.GetHostName();
        /// <summary>
        /// The Local IP Address of this computer
        /// </summary>
        public static string ipAddress;

        private static int available;
        /// <summary>
        /// True if a network connection is available
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (available == 0)
                    return (checkForInternet()==connectionStatus.InternetAccess);
                else if (available == 2)
                    return true;
                else
                    return false; //Return no if unsure
            }
        }
        const string allowed = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz:.=_-$(){}~";
        /// <summary>
        /// URL Encode a string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string urlEncode(string source)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < source.Length; i++)
            {
                if (allowed.Contains(source[i].ToString()) == true)
                    ret.Append(source[i]);
                else
                    ret.Append("%" + Convert.ToInt32(source[i]).ToString("X2"));
            }
            return ret.ToString();
        }

        /// <summary>
        /// Downloads the given url and saves it to the path specified
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveToPath">Can be relative or absolute</param>
        /// <returns></returns>
        public static bool downloadFile(string url, string saveToPath)
        {
            try
            {
                using (WebClient client = new WebClient())
                    client.DownloadFile(url, saveToPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Downloads the given url and saves it to the path specified
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveToPath">Can be relative or absolute</param>
        /// <param name="specialHeader"></param>
        /// <returns></returns>
        public static bool downloadFile(string url, string saveToPath,string specialHeader)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(specialHeader);
                    client.DownloadFile(url, saveToPath);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Downloads the given file and returns it as a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns>String</returns>
        public static string getFile(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                    return client.DownloadString(url);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Retrieves an image from a given URL and returns it in an Image Object
        /// Note all functions done in memory for speed. File may still need to be saved to the hard disk
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        /// <exception cref="WebException">WebException</exception>
        public static Image imageFromURL(string URL)
        {
            if ((URL == null)||(URL==""))
                return null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    MemoryStream stream = new MemoryStream(client.DownloadData(URL));
                    return Image.FromStream(stream, false, true);
                }
            }
            catch (WebException) { return null; }
            catch (NotSupportedException) { return null; }
        }
        /// <summary>
        /// Internet Availability on a network connection
        /// </summary>
        public enum connectionStatus
        {
            /// <summary>
            /// No internet access available
            /// </summary>
            NoInternet=0,
            /// <summary>
            /// Internet access is being redirected...Most likely for a wifi login
            /// </summary>
            LoginRequired=1,
            /// <summary>
            /// Internet Access is available
            /// </summary>
            InternetAccess=2
        }
        /// <summary>
        /// Checks the current network internet connection for internet access
        /// </summary>
        /// <returns></returns>
        public static connectionStatus checkForInternet()
        {
            System.Net.WebRequest request = System.Net.HttpWebRequest.Create("http://www.google.com/");
            request.Timeout = 1500; //Timeout after 1.5 seconds
            request.Method = "HEAD";
            System.Net.WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (System.Net.WebException)
            {
                available=1;
                return connectionStatus.NoInternet;
            }
            if (response.Headers["Server"] != "gws")
            {
                response.Close();
                return connectionStatus.LoginRequired;
            }
            else
            {
                response.Close();
                available=2;
                return connectionStatus.InternetAccess;
            }
        }
    }
}
