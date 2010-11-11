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
using OpenMobile.Graphics;
using OpenMobile.Input;

namespace OpenMobile.Controls
{
    internal sealed class VolumeBar:OMProgress,IMouse
    {
        imageItem bottom;
        imageItem overlay;
        public event userInteraction OnSliderMoved;
        private void raiseSliderMoved(int screen)
        {
            if (OnSliderMoved != null)
                OnSliderMoved(this, screen);
        }
        public VolumeBar(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            height = h;
        }
        public imageItem Top
        {
            set
            {
                overlay = value;
            }
        }
        public imageItem Bottom
        {
            set 
            {
                bottom = value; 
            }
        }
        public override void Render(OpenMobile.Graphics.Graphics g, OpenMobile.renderingParams e)
        {
            g.DrawImage(bottom.image, left, top, width, height);
            Rectangle r = g.Clip;
            int amount=(int)((value/(float)maximum)*height);
            g.SetClipFast(left, top + height - amount, width, amount);
            g.DrawImage(overlay.image, left, top, width, height);
            g.Clip = r;
        }

        #region IMouse Members

        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, float WidthScale, float HeightScale)
        {
            if (e.Buttons == MouseButton.Left)
            {
                this.Value = (int)(((top + height - (e.Y / HeightScale)) / height) * (maximum - minimum)) + minimum;
                raiseSliderMoved(screen);
            }
        }

        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            this.Value = (int)(((top + height - (e.Y / HeightScale)) / height) * (maximum - minimum)) + minimum;
            raiseSliderMoved(screen);
        }

        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            //
        }

        #endregion
    }
}
