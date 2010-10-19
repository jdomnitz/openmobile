using System;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Input;

namespace UI
{
    internal class VolumeBar:OMProgress,IMouse
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
