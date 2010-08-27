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
using System.Collections.Generic;
using System.Xml;

namespace OpenMobile.Net
{
    /// <summary>
    /// An RSS Feed Reader
    /// </summary>
    public sealed class RSS
    {
        /// <summary>
        /// Reads the given URL into an rssFeed class
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static rssFeed getItems(string URL)
        {
            XmlDocument reader=new XmlDocument();
            reader.Load(URL);
            rssFeed ret = new rssFeed();
            foreach(XmlNode n in reader.SelectSingleNode("/rss/channel").ChildNodes)
            {
                switch (n.Name)
                {
                    case "title":
                        ret.title = n.InnerText;
                        break;
                    case "link":
                        ret.link = n.InnerText;
                        break;
                    case "description":
                        ret.description = n.InnerText;
                        break;
                    case "item":
                        rssItem item = new rssItem();
                        foreach (XmlNode i in n.ChildNodes)
                        {
                            switch (i.Name)
                            {
                                case "title":
                                    item.title = i.InnerText;
                                    break;
                                case "link":
                                    item.link = i.InnerText;
                                    break;
                                case "description":
                                    item.description = i.InnerText;
                                    break;
                                case "guid":
                                    item.GUID = i.InnerText;
                                    break;
                                case "author":
                                    item.author = i.InnerText;
                                    break;
                                case "enclosure":
                                    long x;
                                    item.contentURL = i.Attributes["url"].Value;
                                    item.contentType = i.Attributes["type"].Value;
                                    if (long.TryParse(i.Attributes["length"].Value,out x) == true)
                                        item.contentSize = x;
                                    break;
                            }
                        }
                        ret.Items.Add(item);
                        break;
                }
            }
            return ret;
        }
    }
    /// <summary>
    /// A class encapsulating the data from an rss feed
    /// </summary>
    public sealed class rssFeed
    {
        /// <summary>
        /// Feed title
        /// </summary>
        public string title;
        /// <summary>
        /// Link to website generating feed
        /// </summary>
        public string link;
        /// <summary>
        /// Feed description
        /// </summary>
        public string description;
        /// <summary>
        /// Feed items
        /// </summary>
        public List<rssItem> Items;
        /// <summary>
        /// Create a new RSS feed
        /// </summary>
        public rssFeed()
        {
            Items = new List<rssItem>();
        }
    }
    /// <summary>
    /// An RSS Feed Item
    /// </summary>
    public struct rssItem
    {
        /// <summary>
        /// Item Title
        /// </summary>
        public string title;
        /// <summary>
        /// Item Link
        /// </summary>
        public string link;
        /// <summary>
        /// Item Description
        /// </summary>
        public string description;
        /// <summary>
        /// Item Globally Unique ID (Website specific)
        /// </summary>
        public string GUID;
        /// <summary>
        /// Item Author
        /// </summary>
        public string author;
        /// <summary>
        /// Length of content (bytes)
        /// </summary>
        public long contentSize;
        /// <summary>
        /// URL to embedded content
        /// </summary>
        public string contentURL;
        /// <summary>
        /// Mime type of embedded content
        /// </summary>
        public string contentType;
    }
}
