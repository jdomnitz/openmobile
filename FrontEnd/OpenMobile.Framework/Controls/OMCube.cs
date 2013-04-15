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
        /// The image to use for the cube
        /// </summary>
        public imageItem Image
        {
            get
            {
                return this._Image;
            }
            set
            {
                if (this._Image != value)
                {
                    this._Image = value;
                }
            }
        }
        private imageItem _Image;

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

        public OMCube(string name, int left, int top, int width, int height, imageItem image)
            : base(name, left, top, width, height)
        {
            this.Image = image;
        }


        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            g.DrawCube(_Image.image, left, top, 0, 300, 300, 300, _Rotation);

            base.RenderFinish(g, e);
        }
    }
}
