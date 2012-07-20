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
using OpenMobile.Input;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    [System.Serializable]
    internal sealed class VolumeBar : OMProgress, IMouse, IThrow, IHighlightable
    {
        public userInteraction OnSliderMoved;

        private void raiseSliderMoved(int screen)
        {
            if (OnSliderMoved != null)
                OnSliderMoved(this, screen);
        }

        [System.Obsolete("Use VolumeBar(string name, int x, int y, int w, int h) instead")]
        public VolumeBar()
        {
            this.Top = 20;
            this.Left = 20;
            this.Width = 100;
            this.Height = 25;
        }
        [System.Obsolete("Use VolumeBar(string name, int x, int y, int w, int h) instead")]
        public VolumeBar(int x, int y, int w, int h)
        {
            this.top = y;
            this.left = x;
            this.width = w;
            this.height = h;
            this.vertical = true;
        }
        public VolumeBar(string name, int x, int y, int w, int h)
        {
            this.Name = name;
            this.top = y;
            this.left = x;
            this.width = w;
            this.height = h;
            this.vertical = true;
        }

        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            if (e.Buttons == MouseButton.Left)
            {
                //this.Value = (int)(((top + height - (e.Y / HeightScale)) / height) * (maximum - minimum)) + minimum;
                //this.Value = (int)(((top + height - e.Y) / height) * (maximum - minimum)) + minimum;
                this.Value = (int)((((float)(top + height - e.Y)) / height) * (maximum - minimum)) + minimum;
                raiseSliderMoved(screen);
            }
        }

        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation)
        {
            //this.Value = (int)(((top+height-(e.Y / HeightScale)) / height)*(maximum-minimum))+minimum;
            this.Value = (int)((((float)(top + height - e.Y)) / height) * (maximum - minimum)) + minimum;
            raiseSliderMoved(screen);
        }

        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation, Point TotalDistance)
        {
            //
        }

        #region IThrow Members
        //Implemented to prevent gestures
        public void MouseThrow(int screen, Point TotalDistance, Point RelativeDistance)
        {
            //
        }

        public void MouseThrowStart(int screen, Point StartLocation, PointF scaleFactors, ref bool Cancel)
        {
            Cancel = true;
        }

        public void MouseThrowEnd(int screen, Point EndLocation)
        {
            //
        }

        #endregion
    }
}
