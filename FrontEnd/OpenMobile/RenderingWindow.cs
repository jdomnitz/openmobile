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
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OpenMobile
{
    public partial class RenderingWindow : Form
    {
        private OMPanel p = new OMPanel();
        OMControl varHighlighted;
        int screen = -1;
        private renderingParams rParam = new renderingParams();
        object painting = new object();
        private Point ThrowStart = new Point(-1, -1);
        modeType currentMode = modeType.Normal;
        OMButton lastClick;
        public delegate IntPtr getVal();
        public delegate void displayMessage(string message);
        public delegate void voiddel();
        public voiddel hide;
        public voiddel identify;
        public voiddel redraw;
        List<OMPanel> backgroundQueue = new List<OMPanel>();
        float heightScale = 1F;
        float widthScale = 1F;
        Point ofsetIn = new Point(0, 0);
        Point ofsetOut = new Point(0, 0);
        public displayMessage ShowMessage;
        private eGlobalTransition currentTransition;
        public create invokeOnMain;
        private bool transitioning = false;
        public bool fullscreen = false;
        // Start of code added by Borte
        /// <summary>
        /// Relative mouse moved distance for thrown interface
        /// </summary>
        private Point ThrowRelativeDistance = new Point(-1, -1);
        /// <summary>
        /// Throw started (will be reset when throw starts) for thrown interface
        /// </summary>
        private bool ThrowStarted = false;
        float initialHeight = 600F;
        float initialWidth = 1000F;
        //***
        public int Screen
        {
            get
            {
                return screen;
            }
        }
        OMControl highlighted
        {
            get
            {
                return varHighlighted;
            }
            set
            {
                if ((varHighlighted != null) && (varHighlighted != value) && (varHighlighted.Mode == modeType.Highlighted))
                    varHighlighted.Mode = modeType.Normal;
                varHighlighted = value;
            }
        }

        public IntPtr getHandle()
        {
            return this.Handle;
        }
        public RenderingWindow(int s)
        {
            this.screen = s;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            //Code by Borte
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = System.Windows.Forms.Screen.AllScreens[s].Bounds;
            //**
            InitializeComponent();
            this.Text = "Open Mobile v" + Assembly.GetCallingAssembly().GetName().Version + " (" + OpenMobile.Framework.OSSpecific.getOSVersion() + ") Screen " + (screen + 1).ToString();
            ShowMessage += new displayMessage(showMessage);
            invokeOnMain += new create(createNew);
            hide += new voiddel(hideCursor);
            identify += new voiddel(paintIdentity);
            redraw += new voiddel(invokePaint);
        }
        public ISpeech createNew(Type t)
        {
            return (ISpeech)Activator.CreateInstance(t);
        }
        private void paintIdentity()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            Renderer.renderText(g, 0, 0, this.Width, this.Height, (screen+1).ToString(), new Font(FontFamily.GenericSansSerif, 300F), textFormat.Outline, Alignment.CenterCenter, 1F,Color.White,Color.Black);
            Thread.Sleep(1000);
        }
        public void showMessage(string s)
        {
            MessageBox.Show(s);
        }
        public void invokePaint()
        {
            Invalidate();
            UI_MouseMove(null,new MouseEventArgs(MouseButtons.None,0,Cursor.Position.X,Cursor.Position.Y,0));
        }
        //Code Added by Borte
        public new int Width
        {
            set
            {
                initialWidth = value;
                base.Width = value + 16;
            }
            get
            {
                return base.Width;
            }
        }
        public new int Height
        {
            set
            {
                initialHeight = value;
                base.Height = value + 38;
            }
            get
            {
                return base.Height;
            }
        }

        // End of code added by Borte
        public void hideCursor()
        {
            if (this.InvokeRequired == true)
                this.Invoke(hide);
            else
                Cursor.Hide();
        }

        #region ControlManagement
        public void transitionInPanel(OMPanel newP)
        {
            OMControl c;
            for (int i = 0; i < newP.controlCount; i++)
            {
                c = newP.getControl(i);
                if (p.contains(c) == false)
                {
                    c.Mode = modeType.transitioningIn;
                    c.UpdateThisControl += UpdateThisControl;
                    p.addControl(c);
                }
                else
                {
                    c.Mode = modeType.transitionLock;
                }
            }
            p.DoubleClickable |= newP.DoubleClickable;
            backgroundQueue.Add(newP);
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void UpdateThisControl(Rectangle region)
        {
            if (region == Rectangle.Empty)
                Invalidate();
            else
            {
                region.Location = new Point((int)(region.Left * widthScale), (int)(region.Top * heightScale));
                region.Size = new Size((int)(region.Width * widthScale), (int)(region.Height * heightScale));
                Invalidate(region);
            }
        }
        public void transitionOutEverything()
        {
            if (highlighted != null)
                highlighted.Mode = modeType.Normal;
            highlighted = null;
            for (int i = Core.theHost.RenderFirst; i < p.controlCount; i++)
                if (p.getControl(i).Mode != modeType.transitionLock)
                    p[i].Mode = modeType.transitioningOut;
            for (int i = backgroundQueue.Count - 1; i > 0; i--)
                backgroundQueue.RemoveAt(i);
            for (int i = 0; i < backgroundQueue.Count; i++)
                p.DoubleClickable |= backgroundQueue[i].DoubleClickable;
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void transitionOutPanel(OMPanel oldP)
        {
            if (highlighted != null)
                highlighted.Mode = modeType.Normal;
            highlighted = null;
            for (int i = 0; i < oldP.controlCount; i++)
                if (oldP.getControl(i).Mode != modeType.transitionLock)
                    oldP[i].Mode = modeType.transitioningOut;
            backgroundQueue.Remove(oldP);
            for (int i = 0; i < backgroundQueue.Count; i++)
                p.DoubleClickable |= backgroundQueue[i].DoubleClickable;
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void executeTransition(eGlobalTransition transType)
        {
            if (transType != eGlobalTransition.None)
            {
                currentTransition = transType;
                transitioning = true;
                while (transitioning == true)
                {
                    transition_Tick();
                    Thread.Sleep(50);
                }
            }
            lock (painting)
            {
                tmrMouse.Enabled = tmrClick.Enabled = false;
                rParam.transparency = 1;
                rParam.transitionTop = 0;
                rParam.globalTransitionIn = 1;
                rParam.globalTransitionOut = 0;
            }
            for (int i = p.controlCount - 1; i >= 0; i--)
            {
                if ((p.getControl(i).Mode == modeType.transitioningOut) || (p.getControl(i).Mode == modeType.ClickedAndTransitioningOut))
                {
                    p[i].UpdateThisControl -= UpdateThisControl;
                    p[i].Mode = modeType.Normal;
                    p.Remove(p.getControl(i));
                }
                else
                    p.getControl(i).Mode = modeType.Normal;
            }
            highlighted = null;
            if (lastClick != null)
                lastClick.Mode = modeType.Normal;
            lastClick = null;
            UI_MouseMove(null,new MouseEventArgs(MouseButtons.None,0,Cursor.Position.X,Cursor.Position.Y,0));
            Invalidate();
        }
        #endregion

        #region Overrides
        protected override void OnPaint(PaintEventArgs e)
        {
            if (p.controlCount == 0)
                return;
            lock (painting)
            {
                Graphics g = e.Graphics;
                g.ScaleTransform(widthScale, heightScale);
                ImageAnimator.UpdateFrames();
                //Render everything under the UI
                for (int i = Core.theHost.RenderFirst; i < p.controlCount; i++)
                    if (inBounds(p[i].toRegion(), e.ClipRectangle))
                    {
                        if (p[i].Mode == modeType.transitioningIn)
                            modifyIn(g);
                        else if ((p[i].Mode == modeType.transitioningOut) || (p[i].Mode == modeType.ClickedAndTransitioningOut))
                            modifyOut(g);
                        else
                            modifyNeutral(g);
                        if (p[i].Visible)
                            p[i].Render(g, rParam);
                    }
                modifyNeutral(g);
                //Render the UI
                for (int i = 0; i < Core.theHost.RenderFirst; i++)
                    if (inBounds(p[i].toRegion(), e.ClipRectangle))
                        if (p[i].Visible)
                            p[i].Render(g, rParam);
                g = null;
            }
        }

        private void modifyNeutral(Graphics g)
        {
            g.ResetTransform();
            g.ScaleTransform(widthScale, heightScale);
        }
        private void modifyOut(Graphics g)
        {
            //out=-
            g.ResetTransform();
            g.ScaleTransform(widthScale, heightScale);
            g.TranslateTransform(ofsetOut.X - g.Transform.OffsetX, ofsetOut.Y - g.Transform.OffsetY);
        }
        private void modifyIn(Graphics g)
        {
            //in=+
            g.ResetTransform();
            g.ScaleTransform(widthScale, heightScale);
            g.TranslateTransform(ofsetIn.X - g.Transform.OffsetX, ofsetIn.Y - g.Transform.OffsetY);
        }
        private bool inBounds(Rectangle control, Rectangle bounds)
        {
            return ((((control.X * widthScale < (bounds.X + bounds.Width)) &&
              (bounds.X < (control.X + control.Width) * widthScale)) &&
              (control.Y * heightScale < (bounds.Y + bounds.Height))) &&
              (bounds.Y < (control.Y + control.Height) * heightScale));
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //ToDo-Background transitions
            int i;
            for (i = backgroundQueue.Count - 1; i >= 0; i--)
            {
                if (backgroundQueue[i].BackgroundType == backgroundStyle.Image)
                    break;
                if (backgroundQueue[i].BackgroundType == backgroundStyle.None)
                    continue;
                if (backgroundQueue[i].BackgroundColor1.A > 250)
                    break;
            }
            if (i == -1)
                return;
            for (; i < backgroundQueue.Count; i++)
            {
                switch (backgroundQueue[i].BackgroundType)
                {
                    case backgroundStyle.Gradiant:
                        pevent.Graphics.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(pevent.Graphics.RenderingOrigin, new Point(this.Width, this.Height), backgroundQueue[i].BackgroundColor1, backgroundQueue[i].BackgroundColor2), pevent.ClipRectangle.X, pevent.ClipRectangle.Y, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height);
                        break;
                    case backgroundStyle.SolidColor:
                        pevent.Graphics.FillRectangle(new SolidBrush(backgroundQueue[i].BackgroundColor1), pevent.ClipRectangle.X, pevent.ClipRectangle.Y, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height);
                        break;
                    case backgroundStyle.Image:
                        if (backgroundQueue[i].BackgroundImage.image != null)
                            pevent.Graphics.DrawImage(backgroundQueue[i].BackgroundImage.image, new Rectangle(new Point(pevent.ClipRectangle.X, (pevent.ClipRectangle.Y)), pevent.ClipRectangle.Size), pevent.ClipRectangle, GraphicsUnit.Pixel);
                        break;
                }
            }
        }

        #endregion

        #region Timers
        private void tmrClick_Tick(object sender, EventArgs e)
        {
            if (rParam.transparency < 0.15)
            {
                tmrClick.Enabled = false;
                if (lastClick != null)
                {
                    if ((lastClick.Mode == modeType.Highlighted) || (lastClick.Mode == modeType.Clicked))
                    {
                        lastClick.Mode = modeType.Normal;
                        //Recheck where the mouse is at
                        UI_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, Cursor.Position.X, Cursor.Position.Y, 0));
                        rParam.transparency = 1;
                        rParam.transitionTop = 0;
                        Invalidate();
                    }
                }
                else
                {
                    rParam.transparency = 1;
                    rParam.transitionTop = 0;
                    Invalidate();
                }
                lastClick = null;
                return;
            }
            if (lastClick != null)
            {
                if (lastClick.Mode == modeType.transitioningOut)
                    lastClick.Mode = modeType.ClickedAndTransitioningOut;
                if (lastClick.Transition == eButtonTransition.None)
                {
                    rParam.transparency = 1;
                    rParam.transitionTop = 0;
                    if (lastClick.Mode == modeType.ClickedAndTransitioningOut)
                        lastClick.Mode = modeType.transitioningOut;
                    else
                        lastClick.Mode = modeType.Normal;
                    tmrClick.Enabled = false;
                    return;
                }
            }
            if (rParam.transparency >= 0.15F)
            {
                rParam.transparency = rParam.transparency - 0.15F;
                rParam.transitionTop += 7;
            }
            if (lastClick != null)
                UpdateThisControl(new Rectangle(lastClick.Left - rParam.transitionTop, lastClick.Top - rParam.transitionTop, lastClick.Width + (int)(rParam.transitionTop * 2.5), lastClick.Height + (int)(rParam.transitionTop * 2.5)));
        }

        private void tmrClosing_Tick(object sender, EventArgs e)
        {
            if (Core.exitTransition == false)
            {
                Application.Exit();
                return;
            }
            if (this.Opacity > 0.1)
                this.Opacity -= 0.04;
            else
                Application.Exit();
        }
        #endregion
        private List<Point> currentGesture;
        #region MouseHandlers
        private void UI_MouseMove(object sender, MouseEventArgs e)
        {
            bool done = false; //We found something that was selected
            if (p.controlCount == 0)
                return;
            if ((ThrowStart.X != -1) && (typeof(OMSlider).IsInstanceOfType(highlighted) == true))
            {
                if (e.Button == MouseButtons.Left)
                {
                    OMSlider s = (OMSlider)highlighted;
                    s.SliderPosition += ((int)(e.X / widthScale) - ThrowStart.X);
                    ThrowStart.X = (int)(e.X / widthScale);
                    if ((s.SliderPosition + (s.SliderWidth / 2)) < 0)
                        s.SliderPosition = -(s.SliderWidth / 2);
                    if ((s.SliderPosition + (s.SliderWidth / 2)) > s.Width)
                        s.SliderPosition = s.Width - (s.SliderWidth / 2);
                    s.sliderMoved(screen);
                    UpdateThisControl(s.toRegion());
                    return;
                }
            }
            if (currentMode == modeType.Scrolling)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (highlighted != null)
                    {
                        // Added support of IThrow interface 
                        ThrowStarted = false;  // Reset throw data (Added by Borte)
                        if (typeof(IThrow).IsInstanceOfType(highlighted) == true)
                        {
                            Point ThrowTotalDistance = new Point((int)((e.X - ThrowStart.X + 0.5) / widthScale), (int)((e.Y - ThrowStart.Y + 0.5) / heightScale));
                            ThrowRelativeDistance.X = e.X - ThrowRelativeDistance.X;
                            ThrowRelativeDistance.Y = e.Y - ThrowRelativeDistance.Y;
                            ((IThrow)highlighted).MouseThrow(screen, ThrowTotalDistance, new Point((int)(ThrowRelativeDistance.X + 0.5 / widthScale), (int)(ThrowRelativeDistance.Y + 0.5 / heightScale)));
                            ThrowRelativeDistance = e.Location;
                        }
                        else    // End of code added by Borte
                        {
                            OMList l = (OMList)highlighted;
                            l.Ticking = false;
                            l.Thrown = 0;
                            if (ThrowStart.Y != -1)
                            {
                                if (Math.Abs(e.Y - ThrowStart.Y) > 3)
                                    l.Thrown = (int)((e.Y - ThrowStart.Y) / heightScale);
                                l.moveMe((int)((e.Y - ThrowStart.Y) / heightScale));
                                UpdateThisControl(l.toRegion());
                            }
                            ThrowStart.Y = e.Y;
                        }
                    }
                }
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (currentGesture == null)
                    {
                        if ((Math.Abs(e.X - ThrowStart.X) <= 5) && (Math.Abs(e.Y - ThrowStart.Y) <= 5))
                            return;
                        currentGesture = new List<Point>();
                        currentMode = modeType.gesturing;
                    }
                    Graphics g = Graphics.FromHwnd(this.Handle);
                    g.FillEllipse(Brushes.Red, new Rectangle(e.X - 10, e.Y - 10, (int)(20*widthScale), (int)(20*heightScale)));
                    currentGesture.Add(e.Location);
                    if (lastClick != null)
                        lastClick.Mode = modeType.Highlighted;
                }
                else
                {
                    for (int i = Core.theHost.RenderFirst - 1; i >= 0; i--)
                    {
                        checkControl(i, ref done, ref e);
                        if (done == true)
                            break;
                    }
                    if (done == false)
                    {
                        for (int i = p.controlCount - 1; i >= Core.theHost.RenderFirst; i--)
                        {
                            checkControl(i, ref done, ref e);
                            if (done == true)
                                break;
                        }
                    }
                    if (highlighted != null)
                    {
                        if (typeof(IMouse).IsInstanceOfType(highlighted) == true)
                            ((IMouse)highlighted).MouseMove(screen, e, widthScale, heightScale);
                        if (typeof(IThrow).IsInstanceOfType(highlighted) == true)
                            if (ThrowStarted)
                                if (Math.Abs(e.X - ThrowStart.X) > 3 || (Math.Abs(e.Y - ThrowStart.Y) > 3))
                                {
                                    ((IThrow)highlighted).MouseThrowStart(screen, ThrowStart);
                                    currentMode = modeType.Scrolling;
                                }

                        if (done == false)
                        {
                            if (typeof(IHighlightable).IsInstanceOfType(highlighted) == true)
                                UpdateThisControl(highlighted.toRegion());
                            highlighted = null;
                        }
                    }
                }
            }
        }
        private void checkControl(int i, ref bool done, ref MouseEventArgs e)
        {
            //Note potential drawing error with updated rectangle
            OMControl b = (OMControl)p.getControl(i);
            if (b == null)
                Thread.Sleep(5);
            b = (OMControl)p.getControl(i);
            if ((e.X > (b.Left * widthScale)) && (e.Y > (b.Top * heightScale)) && (e.X < ((b.Left + b.Width) * widthScale)) && (e.Y < ((b.Top + b.Height) * heightScale)))
            {
                if (b.Visible == true)
                {
                    currentMode = modeType.Highlighted;
                    if ((b.Mode == modeType.Normal))
                    {
                        if (typeof(IHighlightable).IsInstanceOfType(b) == true)
                            UpdateThisControl(b.toRegion());
                        b.Mode = modeType.Highlighted;
                    }
                    Rectangle r = Rectangle.Empty;
                    if (highlighted != null)
                    {
                        if (typeof(IHighlightable).IsInstanceOfType(highlighted) == true)
                            r = highlighted.toRegion();
                    }
                    highlighted = b;
                    done = true;
                    if (r != Rectangle.Empty)
                        UpdateThisControl(r);
                }
            }
        }
        private void UI_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (highlighted != null))
            {
                if (currentMode == modeType.Highlighted)
                {
                    if (typeof(OMButton).IsInstanceOfType(highlighted))
                    {
                        if (p.DoubleClickable == false)
                        {
                            tmrLongClick.Enabled = false;
                            try
                            {
                                if (lastClick != null)
                                {
                                    lastClick.Mode = modeType.Clicked;
                                    tmrClick.Enabled = true; //ToDo - lastClick should be IClickable not OMButton
                                    new Thread(delegate() { lastClick.clickMe(screen); }).Start();
                                }
                            }
                            catch (Exception) { }
                            return;
                        }
                        if ((tmrMouse.Enabled == false) || (lastClick != (OMButton)highlighted))
                        {
                            tmrMouse.Enabled = false; //faster to just do it then check if we need to
                            lastClick = (OMButton)highlighted;
                            tmrMouse.Enabled = true;
                            return;
                        }
                    }
                    if (lastClick != null)
                        lastClick.Mode = modeType.Normal;
                    lastClick = null;
                    if (typeof(IClickable).IsInstanceOfType(highlighted) == true)
                    {
                        new Thread(delegate() { ((IClickable)highlighted).clickMe(screen); }).Start();
                    }
                }
            }
        }

        private void UI_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tmrLongClick.Enabled = false;
            if (highlighted != null)
            {
                if (currentMode == modeType.Highlighted)
                {
                    if (typeof(OMButton) == highlighted.GetType())
                    {
                        if (lastClick != null)
                        {
                            tmrMouse.Enabled = false;
                            tmrClick.Enabled = true;
                            lastClick.Mode = modeType.Clicked;
                            if (p.DoubleClickable == true)
                                new Thread(delegate() { lastClick.doubleClickMe(screen); lastClick.Mode = modeType.Highlighted; }).Start();
                            else
                                new Thread(delegate() { lastClick.clickMe(screen); }).Start();
                        }
                    }
                    if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted) == true))
                        new Thread(delegate() { ((IClickable)highlighted).doubleClickMe(screen); }).Start();
                }
            }
        }

        private void tmrMouse_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrMouse.Enabled = false;
                new Thread(delegate() { lastClick.clickMe(screen); }).Start();
            }
            catch (Exception) { }
        }

        private void tmrLongClick_Tick(object sender, EventArgs e)
        {
            tmrLongClick.Enabled = false;
            if (currentMode == modeType.gesturing)
                return;
            if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted) == true))
                try
                {
                    new Thread(delegate() { ((IClickable)highlighted).longClickMe(screen); }).Start();
                }
                catch (Exception) { }
        }

        private void UI_MouseDown(object sender, MouseEventArgs e)
        {
            if (highlighted != null)
            {
                if ((currentMode == modeType.Highlighted) && (typeof(OMButton) == highlighted.GetType()))
                {
                    if (lastClick != null)
                    {
                        lastClick.Mode = modeType.Normal;
                        UpdateThisControl(lastClick.toRegion());
                    }
                    lastClick = (OMButton)highlighted;
                    if (lastClick.Mode == modeType.transitioningOut)
                        lastClick.Mode = modeType.ClickedAndTransitioningOut;
                    else
                        lastClick.Mode = modeType.Clicked;
                    tmrLongClick.Enabled = true;
                    UpdateThisControl(lastClick.toRegion());
                }
                else if (typeof(OMList) == highlighted.GetType())
                {
                    OMList l = (OMList)highlighted;
                    l.listThrown(e.Y, heightScale, screen);
                    l.Thrown = 0;
                    l.Ticking = true;
                    currentMode = modeType.Scrolling;
                    ThrowStart.Y = e.Y;
                    UpdateThisControl(l.toRegion());
                    tmrLongClick.Enabled = true;
                }
                else if (typeof(OMSlider) == highlighted.GetType())
                {
                    OMSlider s = (OMSlider)highlighted;
                    s.Mode = modeType.Scrolling;
                    if ((e.X > (s.Left + s.SliderPosition) * widthScale) && (e.X < (s.Left + s.SliderPosition + s.SliderWidth) * widthScale))
                        if ((e.Y > ((s.Top + (s.SliderBarHeight / 2)) - (s.Height / 2)) * heightScale) && (e.Y < ((s.Top + (s.SliderBarHeight / 2)) + (s.Height / 2)) * heightScale))
                        {
                            ThrowStart.X = (int)(e.X / widthScale);
                        }
                    // Start of code added by Borte 
                    // Added support of IMouse interface 
                }
                else if (typeof(IMouse).IsInstanceOfType(highlighted) == true)
                    ((IMouse)highlighted).MouseDown(screen, e, widthScale, heightScale);

                // Added support of IThrow interface 
                if (typeof(IThrow).IsInstanceOfType(highlighted) == true)
                {
                    ThrowStarted = true;
                    ThrowStart = e.Location;
                    ThrowRelativeDistance = ThrowStart;
                    currentMode = modeType.Scrolling;
                    ((IThrow)highlighted).MouseThrowStart(screen, ThrowStart);
                }
                // End of code added by Borte
            } ThrowStart = e.Location;
        }

        private void UI_MouseUp(object sender, MouseEventArgs e)
        {
            tmrLongClick.Enabled = false;
            if ((lastClick != null) && (lastClick.DownImage.image != null))
            {
                lastClick.Mode = modeType.Highlighted;
                UpdateThisControl(lastClick.toRegion());
            }
            if (highlighted != null)
            {
                if ((highlighted.Mode != modeType.Clicked) && (highlighted.Mode != modeType.ClickedAndTransitioningOut))
                    highlighted.Mode = modeType.Highlighted;
                if (typeof(IMouse).IsInstanceOfType(highlighted) == true)
                    ((IMouse)highlighted).MouseUp(screen, e, widthScale, heightScale);
            }
            if (currentMode == modeType.Scrolling)
            {
                currentMode = modeType.Highlighted;
                ThrowStart.Y = -1;
                if ((highlighted != null) && (typeof(IThrow).IsInstanceOfType(highlighted) == true))
                    ((IThrow)highlighted).MouseThrowEnd(screen, e.Location);
                else if ((highlighted != null) && ((OMList)highlighted).Thrown != 0)
                    ((OMList)highlighted).Ticking = true;
            }
            else if (currentMode == modeType.gesturing)
            {
                AlphaRecognizer rec = new AlphaRecognizer();
                rec.Initialize();
                for (int i = 0; i < currentGesture.Count; i++)
                    rec.AddPoint(currentGesture[i], false);
                Core.theHost.execute(eFunction.gesture, screen.ToString(), rec.Recognize());
                currentGesture = null;
                currentMode = modeType.Highlighted;
                UI_MouseMove(sender, new MouseEventArgs(MouseButtons.None, 0, e.X, e.Y, 0));
                Invalidate();
            }
            ThrowStart.X = -1;
            ThrowStarted = false;
        }
        #endregion
        public delegate ISpeech create(Type t);
        #region OtherUIEvents
        private void UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Core.theHost.execute(eFunction.closeProgram);
                e.Cancel = true;
            }
        }

        public void UI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {   //Escape full screen
                    fullscreen = false;
                    this.WindowState = FormWindowState.Normal;
                }
                else
                    tmrClosing.Enabled = true;
            }
            if ((highlighted != null) && (typeof(IKey).IsInstanceOfType(highlighted) == true))
                ((IKey)highlighted).KeyUp(screen, e, widthScale, heightScale);
        }

        public void closeMe()
        {
            if (this.InvokeRequired == true)
                this.Invoke(new MethodInvoker(delegate() { closeMe(); }));
            else
                this.tmrClosing.Enabled = true;
        }

        public static void closeRenderer()
        {
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
                Core.RenderingWindows[i].closeMe();
        }

        private void UI_Resize(object sender, EventArgs e)
        {
            heightScale = (this.ClientRectangle.Height / initialHeight);
            widthScale = (this.ClientRectangle.Width / initialWidth);
            if ((this.WindowState == FormWindowState.Maximized) && (fullscreen == false))
            {
                fullscreen = true;
                this.WindowState = FormWindowState.Normal;
                //this.TopMost = true;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                fullscreen = false;
            }
            else if (fullscreen == false)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                fullscreen = false;
            }
            Invalidate();
        }
        #endregion
        int tick = 0;
        private void transition_Tick()
        {
            switch (currentTransition)
            {
                case eGlobalTransition.Crossfade:
                    rParam.globalTransitionIn += 0.1F;
                    rParam.globalTransitionOut -= 0.1F;
                    if (rParam.globalTransitionOut < 0.1F)
                    {
                        transitioning = false;
                        rParam.globalTransitionIn = 1;
                        rParam.globalTransitionOut = 0;
                        return;
                    }
                    break;
                case eGlobalTransition.SlideUp:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        cleanupTransition();
                        return;
                    }
                    ofsetOut = new Point(0, -(Bounds.Height / 5) * tick);
                    ofsetIn = new Point(0, Bounds.Height - ((Bounds.Height / 5) * tick));
                    break;
                case eGlobalTransition.SlideDown:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        cleanupTransition();
                        return;
                    }
                    ofsetOut = new Point(0, (Bounds.Height / 5) * tick);
                    ofsetIn = new Point(0, ((Bounds.Height / 5) * tick) - Height);
                    break;
                case eGlobalTransition.SlideLeft:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        cleanupTransition();
                        return;
                    }
                    ofsetOut = new Point(-(Bounds.Width / 5) * tick, 0);
                    ofsetIn = new Point(Bounds.Width - ((Bounds.Width / 5) * tick), 0);
                    break;
                case eGlobalTransition.SlideRight:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        cleanupTransition();
                        return;
                    }
                    ofsetOut = new Point((Bounds.Width / 5) * tick, 0);
                    ofsetIn = new Point(((Bounds.Width / 5) * tick) - Width, 0);
                    break;
            }
            Invalidate();
        }
        private void cleanupTransition()
        {
            tick = 0;
            ofsetIn = new Point(0, 0);
            ofsetOut = new Point(0, 0);
            transitioning = false;
        }
        public void UI_KeyDown(object sender, KeyEventArgs e)
        {
            if (highlighted == null)
            {
                if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) || (e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
                {
                    int top = 1000;
                    int left = 1000;
                    OMControl b = null;
                    for (int i = 0; i < p.controlCount; i++)
                        //Modified by Borte
                        if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                            if ((p[i].Left < left) && (p[i].Top < top) && (inBounds(p[i].toRegion(), this.DisplayRectangle) == true))
                            {
                                b = p[i];
                                top = b.Top;
                                left = b.Left;
                            }
                    if (b == null)
                        return;
                    b.Mode = modeType.Highlighted;
                    highlighted = b;
                    UpdateThisControl(highlighted.toRegion());
                }
            }
            else
            {
                if (typeof(IKey).IsInstanceOfType(highlighted) == true)
                    if (((IKey)highlighted).KeyDown(screen, e, widthScale, heightScale) == true)
                        return;
                int best = 1000;
                OMControl b = null;
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Left + p[i].Width <= highlighted.Left) && (inBounds(p[i].toRegion(), this.DisplayRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        best = distance(highlighted.toRegion(), p[i].toRegion());
                                        b = p[i];
                                    }
                        if (b == null)
                            break;
                        b.Mode = modeType.Highlighted;
                        highlighted.Mode = modeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Keys.Right:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Left >= highlighted.Left + highlighted.Width) && (inBounds(p[i].toRegion(), this.DisplayRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        best = distance(highlighted.toRegion(), p[i].toRegion());
                                        b = p[i];
                                    }
                        if (b == null)
                            break;
                        b.Mode = modeType.Highlighted;
                        highlighted.Mode = modeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Keys.Up:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Top + p[i].Height <= highlighted.Top) && (inBounds(p[i].toRegion(), this.DisplayRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        best = distance(highlighted.toRegion(), p[i].toRegion());
                                        b = p[i];
                                    }
                        if (b == null)
                            break;
                        b.Mode = modeType.Highlighted;
                        highlighted.Mode = modeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Keys.Down:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Top >= highlighted.Top + highlighted.Height) && (inBounds(p[i].toRegion(), this.DisplayRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        best = distance(highlighted.toRegion(), p[i].toRegion());
                                        b = p[i];
                                    }
                        if (b == null)
                            break;
                        b.Mode = modeType.Highlighted;
                        highlighted.Mode = modeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Keys.Return:
                        if (highlighted.GetType() == typeof(OMButton))
                        {
                            lastClick = (OMButton)highlighted;
                            lastClick.Mode = modeType.Clicked;
                            tmrClick.Enabled = true;
                            new Thread(delegate() { lastClick.clickMe(screen); }).Start();
                        }
                        else if (typeof(IClickable).IsInstanceOfType(highlighted))
                        {
                            new Thread(delegate() { ((IClickable)highlighted).clickMe(screen); }).Start();
                        }
                        break;
                }
            }
        }

        private int distance(Rectangle r1, Rectangle r2)
        {
            return Math.Max(Math.Abs((r1.Left + r1.Width / 2) - (r2.Left + r2.Width / 2)) - (r1.Width + r2.Width) / 2, 0) + Math.Max(Math.Abs((r1.Top + r1.Height / 2) - (r2.Top + r2.Height / 2)) - (r1.Height + r2.Height) / 2, 0);
        }

        private void UI_MouseLeave(object sender, EventArgs e)
        {
            if (currentMode == modeType.Scrolling)
            {
                currentMode = modeType.Highlighted;
                ThrowStart.Y = -1;
                if ((highlighted != null) && (typeof(OMList) == highlighted.GetType()) && ((OMList)highlighted).Thrown != 0)
                    ((OMList)highlighted).Ticking = true;
            }
            tmrLongClick.Enabled = false;
        }
    }
}
