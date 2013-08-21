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
using OpenMobile;

namespace OpenMobile.Controls
{
    public class OMCube : OMControl
    {
        /// <summary>
        /// The images to use for the cube 
        /// </summary>
        public imageItem[] Images
        {
            get
            {
                return this._ImageItems;
            }
            set
            {
                if (value == null)
                {
                    _ImageItems = null;
                    _Images = null;
                    return;
                }

                if (value.Length > 6 || value.Length == 0)
                {
                    if (this._ImageItems != value)
                    {
                        this._ImageItems = value;
                        _Images = new OImage[_ImageItems.Length];
                        for (int i = 0; i < _ImageItems.Length; i++)
                            _Images[i] = _ImageItems[i].image;
                    }
                }
                else
                {
                    throw new Exception("Invalid array length! Length must in the range 1 to 6 items");
                }
            }
        }

        /// <summary>
        /// Sets or gets the single image to use for all sides of the cube
        /// </summary>
        public imageItem Image
        {
            get
            {
                return this._ImageItems[0];
            }
            set
            {
                for (int i = 0; i < _ImageItems.Length; i++)
                {
                    _ImageItems[i] = value;
                    _Images[i] = value.image;
                }
            }
        }

        private imageItem[] _ImageItems = new imageItem[6];
        private OImage[] _Images = new OImage[6];

        /// <summary>
        /// Sets or gets the image for side 1 (top)
        /// </summary>
        public imageItem Image1
        {
            get
            {
                return this._ImageItems[0];
            }
            set
            {
                if (this._ImageItems[0] != value)
                {
                    this._ImageItems[0] = value;
                    this._Images[0] = value.image;
                }
            }
        }

        /// <summary>
        /// Sets or gets the image for side 2 (bottom)
        /// </summary>
        public imageItem Image2
        {
            get
            {
                return this._ImageItems[1];
            }
            set
            {
                if (this._ImageItems[1] != value)
                {
                    this._ImageItems[1] = value;
                    this._Images[1] = value.image;
                }
            }
        }

        /// <summary>
        /// Sets or gets the image for side 3 (front)
        /// </summary>
        public imageItem Image3
        {
            get
            {
                return this._ImageItems[2];
            }
            set
            {
                if (this._ImageItems[2] != value)
                {
                    this._ImageItems[2] = value;
                    this._Images[2] = value.image;
                }
            }
        }

        /// <summary>
        /// Sets or gets the image for side 4 (back)
        /// </summary>
        public imageItem Image4
        {
            get
            {
                return this._ImageItems[3];
            }
            set
            {
                if (this._ImageItems[3] != value)
                {
                    this._ImageItems[3] = value;
                    this._Images[3] = value.image;
                }
            }
        }

        /// <summary>
        /// Sets or gets the image for side 5 (right)
        /// </summary>
        public imageItem Image5
        {
            get
            {
                return this._ImageItems[4];
            }
            set
            {
                if (this._ImageItems[4] != value)
                {
                    this._ImageItems[4] = value;
                    this._Images[4] = value.image;
                }
            }
        }

        /// <summary>
        /// Sets or gets the image for side 6 (left)
        /// </summary>
        public imageItem Image6
        {
            get
            {
                return this._ImageItems[5];
            }
            set
            {
                if (this._ImageItems[5] != value)
                {
                    this._ImageItems[5] = value;
                    this._Images[5] = value.image;
                }
            }
        }

        /// <summary>
        /// Rotation of the object
        /// </summary>
        public Math.Vector3 Rotation
        {
            get
            {
                return this._Rotation;
            }
            set
            {
                if (this._Rotation != value)
                {
                    this._Rotation = value;
                    Refresh();
                }
            }
        }
        private Math.Vector3 _Rotation;

        /// <summary>
        /// Creates a new cube style control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMCube(string name, int left, int top, int width, int height)
            : base(name, left, top, width, height)
        {
        }

        /// <summary>
        /// Creates a new cube style control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="image"></param>
        public OMCube(string name, int left, int top, int width, int height, imageItem image)
            : base(name, left, top, width, height)
        {
            this.Image = image;
        }

        /// <summary>
        /// Creates a new cube style control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="images"></param>
        public OMCube(string name, int left, int top, int width, int height, imageItem[] images)
            : base(name, left, top, width, height)
        {
            this.Images = images;
        }


        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            g.DrawCube(_Images, left, top, 0, 300, 300, 300, _Rotation);

            base.RenderFinish(g, e);
        }

        public override object Clone(OMPanel parent)
        {
            OMCube newObject = (OMCube)this.MemberwiseClone();
            for (int i = 0; i < _ImageItems.Length; i++)
            {
                newObject._ImageItems[i] = _ImageItems[i];
                newObject._Images[i] = _Images[i];
            }
            return newObject;
        } 
    }
}
