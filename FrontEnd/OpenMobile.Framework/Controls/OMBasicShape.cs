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
using System.ComponentModel;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Allows drawing of basic shapes
    /// </summary>
    [System.Serializable]
    public class OMBasicShape : OMControlGraphicsBase
    {
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        [System.Obsolete("Use OMBasicShape(string name, int x, int y, int w, int h) instead")]
        public OMBasicShape()
            : base("", 0, 0, 200, 200)
        { }
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        [System.Obsolete("Use OMBasicShape(string name, int x, int y, int w, int h) instead")]
        public OMBasicShape(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
        }
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMBasicShape(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
        }
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMBasicShape(string name, int x, int y, int w, int h, ShapeData shapeData)
            : base(name, x, y, w, h)
        {
            _ShapeData = shapeData;
        }

        /// <summary>
        /// Draws the basic shape
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);
            base.DrawShape(g, e);
            base.RenderFinish(g, e);
        }
    }
}
