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
using OpenMobile.Controls;
using System.Windows.Forms;

namespace OpenMobile.Controls
{
    internal class VolumeBar:OMProgress, IMouse,IThrow
    {
        public userInteraction OnSliderMoved;

        private void raiseSliderMoved(int screen)
        {
            if (OnSliderMoved != null)
                OnSliderMoved(this, screen);
        }

        public VolumeBar()
        {
            this.Top = 20;
            this.Left = 20;
            this.Width = 100;
            this.Height = 25;
        }
        public VolumeBar(int x, int y, int w, int h)
        {
            this.top = y;
            this.left = x;
            this.width = w;
            this.height = h;
            this.vertical = true;
        }

        public void MouseMove(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale)
        {
            if (e.Button == MouseButtons.Left)
                MouseDown(screen, e, WidthScale, HeightScale);
        }

        public void MouseDown(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale)
        {
            this.Value = (int)(((top+height-(e.Y * HeightScale)) / height)*(maximum-minimum))+minimum;
            raiseSliderMoved(screen);
        }

        public void MouseUp(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale)
        {
            //
        }

        #region IThrow Members
        //Implemented to prevent gestures
        public void MouseThrow(int screen, System.Drawing.Point TotalDistance, System.Drawing.Point RelativeDistance)
        {
            //
        }

        public void MouseThrowStart(int screen, System.Drawing.Point StartLocation, System.Drawing.PointF scaleFactors, ref bool Cancel)
        {
            Cancel = true;
        }

        public void MouseThrowEnd(int screen, System.Drawing.Point EndLocation)
        {
            //
        }

        #endregion
    }
}
