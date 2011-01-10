using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Threading;

public sealed class Card : OMControl, IClickable, IThrow
{
    public event userInteraction OnClick;
    public event userInteraction OnDrop;
    imageItem _image;
    bool _dragable;
    Point _anchor;

    public Card(int x, int y, int w, int h)
    {
        left = x;
        top = y;
        _anchor = new Point(x, y);
        width = w;
        height = h;
    }
    public Point Anchor
    {
        get
        {
            return _anchor;
        }
    }
    Card _child;
    public Card DragChild
    {
        set 
        {
            if(_child!=null)
                _child._parent = null;
            _child = value;
            if (value != null)
                _child._parent = this;
        }
        get { return _child; }
    }
    private Card _parent;
    public Card DragParent
    {
        get
        {
            return _parent;
        }
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
        float tmp = 1F;
        if ((mode == eModeType.transitioningOut) || (mode == eModeType.ClickedAndTransitioningOut))
            tmp = e.globalTransitionOut;
        else if (mode == eModeType.transitioningIn)
            tmp = e.globalTransitionIn;
        g.DrawImage(_image.image, left, top, width, height,tmp);
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
            if (_child == null)
                raiseUpdate(false);
            else
                _child.MouseThrow(screen, TotalDistance, RelativeDistance);
        }
    }

    public void MouseThrowStart(int screen, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.PointF scaleFactors, ref bool Cancel)
    {
        Cancel = !_dragable;
        if (!Cancel)
            parent.MoveControlToFront(this);
        _anchor = new Point(left, top);
        if (_child != null)
            _child.MouseThrowStart(screen, StartLocation, scaleFactors, ref Cancel);
    }

    public void MouseThrowEnd(int screen, OpenMobile.Graphics.Point EndLocation)
    {
        if (_dragable)
            if (OnDrop != null)
                OnDrop(this, screen);
        _anchor = new Point(left, top);
        if (_child != null)
            _child.MouseThrowEnd(screen, EndLocation);
    }

    #endregion
}
