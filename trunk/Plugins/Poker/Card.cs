using OpenMobile.Controls;
using OpenMobile;

namespace Poker
{
    internal sealed class Card:OMControl,IClickable,IThrow
    {
        public event userInteraction OnClick;
        public event userInteraction OnDrop;
        imageItem _image;
        bool _dragable;

        public Card(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            height = h;
        }

        public imageItem Image
        {
            get { return _image; }
            set
            {
                if (_image == value)
                    return;
                _image = value;
                raiseUpdate(false);
            }
        }
        public bool Dragable
        {
            get { return _dragable; }
            set { _dragable = value; }
        }
        public override void Render(OpenMobile.Graphics.Graphics g, OpenMobile.renderingParams e)
        {
            g.DrawImage(_image.image, left, top, width, height);
        }

        #region IClickable Members

        public void clickMe(int screen)
        {
            if (OnClick != null)
                OnClick(this, screen);
        }

        public void longClickMe(int screen)
        {
            //
        }

        #endregion

        #region IThrow Members

        public void MouseThrow(int screen, OpenMobile.Graphics.Point TotalDistance, OpenMobile.Graphics.Point RelativeDistance)
        {
            if (_dragable)
            {
                this.top += RelativeDistance.Y;
                this.left += RelativeDistance.X;
                raiseUpdate(false);
            }
        }

        public void MouseThrowStart(int screen, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.PointF scaleFactors, ref bool Cancel)
        {
            Cancel = !_dragable;
        }

        public void MouseThrowEnd(int screen, OpenMobile.Graphics.Point EndLocation)
        {
            if (_dragable)
                if (OnDrop != null)
                    OnDrop(this, screen);
        }

        #endregion
    }
}
