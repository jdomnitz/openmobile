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
using System.ComponentModel;
using OpenMobile.Graphics;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;

namespace OpenMobile
{
    /// <summary>
    /// An Open Mobile representation of an image
    /// </summary>
    [Serializable]
    public struct imageItem : ICloneable
    {
        /// <summary>
        /// The region this image covers
        /// </summary>
        public Rectangle Region 
        {
            get
            {
                if (image != null)
                    return image.Region;
                else
                    return new Rectangle();
            }
        }
        /// <summary>
        /// The image
        /// </summary>
        public OImage image;
        /// <summary>
        /// The image name in the skin folder
        /// </summary>
        public string name;
        /// <summary>
        /// Construct an image item from an image
        /// </summary>
        /// <param name="i"></param>
        public imageItem(OImage i)
        {
            this.name = "Unknown";
            this.image = i;
        }
        public imageItem(Color color, int width, int height)
        {
            this.name = "Unknown";
            this.image = new OImage(color, width, height);
        }
        /// <summary>
        /// Construct an image item from fully qualified path 
        /// </summary>
        /// <param name="imageName"></param>
        public imageItem(string imagePath)
        {
            this.name = imagePath;
            if (imagePath == "")
            {
                this.image = null;
                return;
            }
            this.image = OImage.FromFile(imagePath);
        }
        /// <summary>
        /// Construct an image item from an image
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Name"></param>
        public imageItem(OImage i, string Name)
        {
            this.name = Name;
            this.image = i;
        }

        /// <summary>
        /// Provides the name of an image
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (image == null)
                return String.Empty;
            else
                return string.Format("ImageItem (Name/Path:{0}, Hash:{1}",name,this.GetHashCode());
        }
        /// <summary>
        /// Value Comparison
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator ==(imageItem item1, imageItem item2)
        {
            if (item1.name != item2.name)
                return false;
            if (item1.image != item2.image)
                return false;
            return true;
        }
        /// <summary>
        /// Value comparison
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator !=(imageItem item1, imageItem item2)
        {
            if (item1.name != item2.name)
                return true;
            if (item1.image != item2.image)
                return true;
            return false;
        }
        /// <summary>
        /// Value comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is imageItem)
                return (this == (imageItem)obj);
            return false;
        }
        /// <summary>
        /// Serves as a hash function for a particular type
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return image.GetHashCode();
        }
        /// <summary>
        /// Represents an image that the framework can't find
        /// </summary>
        public static imageItem MISSING = new imageItem("") { name = "MISSING" };
        /// <summary>
        /// Represents an empty image item
        /// </summary>
        public static imageItem NONE = new imageItem();

        public void Refresh()
        {
            if (image != null)
                image.Refresh();
        }

        #region ICloneable Members

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            imageItem returnData = (imageItem)this.MemberwiseClone();
            if (this.image != null)
                returnData.image = (OImage)this.image.Clone();
            returnData.name = this.name;
            return returnData;
        }

        /// <summary>
        /// Copies this object as a new imageItem
        /// </summary>
        /// <returns></returns>
        public imageItem Copy()
        {
            return (imageItem)this.Clone();
        }

        #endregion
    }
}
