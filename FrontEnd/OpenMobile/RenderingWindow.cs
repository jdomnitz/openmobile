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
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Input;
using OpenMobile.Plugin;

namespace OpenMobile
{
    public partial class RenderingWindow : GameWindow
    {
        private OMPanel p = new OMPanel();
        OMControl varHighlighted;
        int screen = -1;
        private renderingParams rParam = new renderingParams();
        object painting = new object();
        private Point ThrowStart = new Point(-1, -1);
        OMButton lastClick;
        public delegate IntPtr getVal();
        public delegate void voiddel();
        public voiddel hide;
        public voiddel identify;
        public voiddel redraw;
        List<OMPanel> backgroundQueue = new List<OMPanel>();
        float heightScale = 1F;
        float widthScale = 1F;
        Point ofsetIn = new Point(0, 0);
        Point ofsetOut = new Point(0, 0);
        private eGlobalTransition currentTransition;
        private bool transitioning = false;
        private bool keyboardActive;
        public bool fullscreen = false;
        private List<Point> currentGesture;
        // Throw started (will be reset when throw starts) for thrown interface
        private bool ThrowStarted = false;
        // Relative mouse moved distance for thrown interface
        private Point ThrowRelativeDistance = new Point(-1, -1);
        private bool hidden;
        private int tick = 0;

        public int Screen
        {
            get
            {
                return screen;
            }
        }
        public PointF ScaleFactors
        {
            get
            {
                return new PointF(widthScale, heightScale);
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
                if ((varHighlighted != null) && (varHighlighted != value) && (varHighlighted.Mode == eModeType.Highlighted))
                    varHighlighted.Mode = eModeType.Normal;
                varHighlighted = value;
            }
        }

        public IntPtr getHandle()
        {
            return this.WindowHandle;
        }
        Graphics.Graphics g= new OpenMobile.Graphics.Graphics();
        public RenderingWindow(int s)
        {
            if (this.fullscreen)
                Mouse.Location = this.Location;
            this.screen = s;
            if (s <= DisplayDevice.AvailableDisplays.Count - 1)
                this.Bounds=new Rectangle(DisplayDevice.AvailableDisplays[s].Bounds.Location,new Size(720,450));
            InitializeComponent();
            this.Title = "openMobile v" + Assembly.GetCallingAssembly().GetName().Version + " (" + OpenMobile.Framework.OSSpecific.getOSVersion() + ") Screen " + (screen + 1).ToString();
            hide += new voiddel(hideCursor);
            identify += new voiddel(paintIdentity);
            redraw += new voiddel(invokePaint);
        }
        bool Identify = false;
        private void paintIdentity()
        {
            if (this.InvokeRequired)
                this.Invoke(identify);
            else
            {
                Identify = true;
                Invalidate();
                Thread.Sleep(1500);
                Identify = false;
                Invalidate();
            }
        }
        public void invokePaint()
        {
            if (this.InvokeRequired)
                this.Invoke(redraw);
            else
            {
                Invalidate();
                RenderingWindow_MouseMove(null, new OpenMobile.Input.MouseMoveEventArgs(Mouse.X, Mouse.Y, 0, 0, MouseButton.None));
            }
        }

        private void Invalidate()
        {
            if (this.InvokeRequired)
                this.invokePaint();
            else
            {
                g.ResetClip();
                OnRenderFrame(new FrameEventArgs());
            }
        }
        //Code Added by Borte
        public new int Width
        {
            set
            {
                base.Width = value + (this.Width- ClientSize.Width);
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
                base.Height = value + (this.Height - ClientSize.Height);
            }
            get
            {
                return base.Height;
            }
        }
        public new Size Size
        {
            set
            {
                base.Size = new Size(value.Width + (this.Width - ClientSize.Width), value.Height + (this.Height - ClientSize.Height));
            }
            get
            {
                return base.Size;
            }
        }
        // End of code added by Borte
        
        public void hideCursor()
        {
            if (this.InvokeRequired == true)
                this.Invoke(hide);
            else
                if (hidden == false)
                {
                    Mouse.HideCursor(WindowInfo);
                    hidden = true;
                }
                else
                {
                    Mouse.ShowCursor(WindowInfo);
                    hidden = false;
                }
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
                    c.UpdateThisControl += UpdateThisControl;
                    c.Mode = eModeType.transitioningIn;
                    p.addControl(c,false);
                }
                else
                {
                    c.Mode = eModeType.transitionLock;
                }
            }
            newP.Mode = eModeType.transitioningIn;
            if (!backgroundQueue.Contains(newP))
                backgroundQueue.Add(newP);
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void UpdateThisControl(Rectangle region)
        {
            if (region == Rectangle.Empty)
                Invalidate();
            else
                Invalidate(region);
        }

        private void Invalidate(Rectangle region)
        {
            //Unneeded - And causes problems with the double buffer
            //g.SetClip(region);
            //OnUpdateFrame(new FrameEventArgs());
            //g.ResetClip();
        }
        public void transitionOutEverything()
        {
            highlighted = null;
            for (int i = Core.theHost.RenderFirst; i < p.controlCount; i++)
                if (p.getControl(i).Mode != eModeType.transitionLock)
                    p[i].Mode = eModeType.transitioningOut;
            for (int i = backgroundQueue.Count - 1; i > 1; i--)
                backgroundQueue[i].Mode = eModeType.transitioningOut;
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void transitionOutPanel(OMPanel oldP)
        {
            if (highlighted != null)
                highlighted.Mode = eModeType.Normal;
            highlighted = null;
            for (int i = 0; i < oldP.controlCount; i++)
                if (oldP.getControl(i).Mode != eModeType.transitionLock)
                    oldP[i].Mode = eModeType.transitioningOut;
            oldP.Mode = eModeType.transitioningOut;
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
                if ((p.getControl(i).Mode == eModeType.transitioningOut) || (p.getControl(i).Mode == eModeType.ClickedAndTransitioningOut))
                {
                    p[i].UpdateThisControl -= UpdateThisControl;
                    p[i].Mode = eModeType.Normal;
                    p.Remove(p.getControl(i));
                }
                else
                    p.getControl(i).Mode = eModeType.Normal;
            }
            backgroundQueue.RemoveAll(q => q.Mode == eModeType.transitioningOut);
            for (int i = 0; i < backgroundQueue.Count; i++)
                backgroundQueue[i].Mode = eModeType.Normal;
            if (transType > eGlobalTransition.Crossfade)
            {
                tick = 0;
                ofsetIn = new Point(0, 0);
                ofsetOut = new Point(0, 0);
            }
            highlighted = null;
            if (lastClick != null)
                lastClick.Mode = eModeType.Normal;
            lastClick = null;
            RenderingWindow_MouseMove(null,new OpenMobile.Input.MouseMoveEventArgs(Mouse.X,Mouse.Y,0,0,MouseButton.None));
            Invalidate();
        }
        #endregion

        #region Overrides
        OImage identity;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            OnPaint();
            if ((currentGesture!=null)&&(currentGesture.Count > 0))
                RenderGesture();
            if (Identify)
            {
                if (identity == null)
                    identity = g.GenerateTextTexture(0, 0, 1000, 600, (screen + 1).ToString(), new Font(Font.GenericSansSerif, 400F), eTextFormat.Outline, Alignment.CenterCenter, Color.White, Color.Black);
                g.DrawImage(identity,0,0,1000,600);
            }
            g.Finish();
            SwapBuffers();
            base.OnRenderFrame(e);
        }

        private void RenderGesture()
        {
            foreach(Point p in currentGesture)
                g.FillEllipse(new Brush(Color.Red), new Rectangle((int)((p.X - 10)/widthScale),(int)((p.Y - 10)/heightScale), 20, 20));
        }
        protected override void OnResize(EventArgs e)
        {
            g.Resize(Width, Height);
            base.OnResize(e);
        }
        protected void OnPaint()
        {
            if (p.controlCount == 0)
                return;
            lock (painting)
            {
                //Render everything under the UI
                foreach(ePriority priority in Enum.GetValues(typeof(ePriority)))
                {
                    foreach (OMPanel panel in backgroundQueue.FindAll(q => q.Priority == priority))
                    {
                        if (panel.Mode == eModeType.transitioningIn)
                            modifyIn(g);
                        else if (panel.Mode == eModeType.transitioningOut)
                            modifyOut(g);
                        else
                            modifyNeutral(g);
                        panel.Render(g, rParam);
                    }
                }
            }
        }

        private void modifyNeutral(OpenMobile.Graphics.Graphics g)
        {
            g.ResetTransform();
        }
        private void modifyOut(OpenMobile.Graphics.Graphics g)
        {
            //out=-
            g.ResetTransform();
            g.TranslateTransform(ofsetOut.X, ofsetOut.Y);
        }
        private void modifyIn(OpenMobile.Graphics.Graphics g)
        {
            //in=+
            g.ResetTransform();
            g.TranslateTransform(ofsetIn.X, ofsetIn.Y);
        }
        private bool inBounds(Rectangle control, Rectangle bounds)
        {
            return ((control.X < (bounds.X + bounds.Width)) &&
              (bounds.X < (control.X + control.Width)) &&
              (control.Y < (bounds.Y + bounds.Height)) &&
              (bounds.Y < control.Y + control.Height));
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
                    if ((lastClick.Mode == eModeType.Highlighted) || (lastClick.Mode == eModeType.Clicked))
                    {
                        rParam.transparency = 1;
                        rParam.transitionTop = 0;
                        if (keyboardActive == true)
                        {
                            keyboardActive = false;
                            lastClick.Mode = eModeType.Highlighted;
                        }
                        else
                        {
                            lastClick.Mode = eModeType.Normal;
                            //Recheck where the mouse is at
                            RenderingWindow_MouseMove(this, new OpenMobile.Input.MouseMoveEventArgs(Mouse.X, Mouse.Y, 0,0,MouseButton.None));
                        }
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
                if (lastClick.Mode == eModeType.transitioningOut) //<- Unnecessary?
                    lastClick.Mode = eModeType.ClickedAndTransitioningOut;
                if (lastClick.Transition == eButtonTransition.None)
                {
                    rParam.transparency = 1;
                    rParam.transitionTop = 0;
                    if (lastClick.Mode == eModeType.ClickedAndTransitioningOut)
                        lastClick.Mode = eModeType.transitioningOut;
                    else
                        lastClick.Mode = eModeType.Normal;
                    tmrClick.Enabled = false;
                    lastClick = null;
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
                this.Exit();
                return;
            }
            while (this.Opacity >= 0)
            {
                if ((this.Opacity > 0) && (Core.theHost.GraphicsLevel == eGraphicsLevel.Standard))
                    this.Opacity -= 0.04F;
                else
                {
                    this.Exit();
                    return;
                }
                Thread.Sleep(20);
            }
            this.Exit();
        }
        #endregion
        
        #region MouseHandlers
        private void RenderingWindow_MouseMove(object sender, MouseMoveEventArgs e)
        {
            bool done = false; //We found something that was selected
            if (p.controlCount == 0)
                return;
            if (rParam.currentMode == eModeType.Scrolling)
            {
                if (this.Mouse[MouseButton.Left])
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
                            ((IThrow)highlighted).MouseThrow(screen, ThrowTotalDistance, new Point((int)(ThrowRelativeDistance.X / widthScale), (int)(ThrowRelativeDistance.Y / heightScale)));
                            ThrowRelativeDistance = e.Location;
                            tmrLongClick.Enabled = false;
                        }
                        // End of code added by Borte
                    }
                }
            }
            else
            {
                if ((this.Mouse[MouseButton.Left]) & (!ThrowStarted))
                {
                    if (currentGesture == null)
                    {
                        if ((Math.Abs(e.X - ThrowStart.X) <= 10) && (Math.Abs(e.Y - ThrowStart.Y) <= 10))
                            return;
                        currentGesture = new List<Point>();
                        rParam.currentMode = eModeType.gesturing;
                    }
                    currentGesture.Add(e.Location);
                    if (lastClick != null)
                        lastClick.Mode = eModeType.Highlighted;
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
                                    bool cancel=false;
                                    ((IThrow)highlighted).MouseThrowStart(screen, ThrowStart,new PointF(widthScale,heightScale), ref cancel);
                                    if (cancel==false)
                                        rParam.currentMode = eModeType.Scrolling;
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
        private void checkControl(int i, ref bool done, ref OpenMobile.Input.MouseMoveEventArgs e)
        {
            //Note potential drawing error with updated rectangle
            OMControl b = (OMControl)p.getControl(i);
            if (b == null)
                Thread.Sleep(10);
            b = (OMControl)p.getControl(i);
            if (b == null)
                return; //failsafe - occurs when loading UI on very slow computers
            if ((e.X > (b.Left * widthScale)) && (e.Y > (b.Top * heightScale)) && (e.X < ((b.Left + b.Width) * widthScale)) && (e.Y < ((b.Top + b.Height) * heightScale)))
            {
                if (b.Visible == true)
                {
                    rParam.currentMode = eModeType.Highlighted;
                    if ((b.Mode == eModeType.Normal))
                    {
                        if (typeof(IHighlightable).IsInstanceOfType(b) == true)
                            UpdateThisControl(b.toRegion());
                        b.Mode = eModeType.Highlighted;
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
        private void RenderingWindow_MouseClick(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((e.Buttons==MouseButton.Left) && (highlighted != null))
            {
                if (rParam.currentMode == eModeType.Highlighted)
                {
                    if (typeof(OMButton).IsInstanceOfType(highlighted))
                    {
                        if (p.DoubleClickable == false)
                        {
                            tmrLongClick.Enabled = false;
                            if (lastClick != null)
                            {
                                lastClick.Mode = eModeType.Clicked;
                                tmrClick.Enabled = true;
                                SandboxedThread.Asynchronous(delegate() { if (lastClick != null) lastClick.clickMe(screen); });
                            }
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
                        lastClick.Mode = eModeType.Normal;
                    lastClick = null;
                    if (typeof(IClickable).IsInstanceOfType(highlighted) == true)
                    {
                        SandboxedThread.Asynchronous(delegate() { if (highlighted != null) ((IClickable)highlighted).clickMe(screen); });
                    }
                }
            }
        }

        private void RenderingWindow_MouseDoubleClick(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            tmrLongClick.Enabled = false;
            if (highlighted != null)
            {
                if (rParam.currentMode == eModeType.Highlighted)
                {
                    if (typeof(OMButton).IsInstanceOfType(highlighted))
                    {
                        if (lastClick != null)
                        {
                            tmrMouse.Enabled = false;
                            tmrClick.Enabled = true;
                            lastClick.Mode = eModeType.Clicked;
                            if (lastClick.Parent.DoubleClickable == true)
                                SandboxedThread.Asynchronous(delegate() { if (lastClick != null) lastClick.doubleClickMe(screen); lastClick.Mode = eModeType.Highlighted; });
                            else
                                SandboxedThread.Asynchronous(delegate() { if (lastClick != null) lastClick.clickMe(screen); });
                        }
                    }
                    else if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted) == true))
                    {
                        if (p.DoubleClickable == true)
                            SandboxedThread.Asynchronous(delegate() { (highlighted as IClickable).doubleClickMe(screen); });
                        else
                            SandboxedThread.Asynchronous(delegate() { (highlighted as IClickable).clickMe(screen); });
                    }
                }
            }
        }

        private void tmrMouse_Tick(object sender, EventArgs e)
        {
            tmrMouse.Enabled = false;
            SandboxedThread.Asynchronous(delegate() { lastClick.clickMe(screen); });
        }

        private void tmrLongClick_Tick(object sender, EventArgs e)
        {
            tmrLongClick.Enabled = false;
            if (rParam.currentMode == eModeType.gesturing)
                return;
            if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted)))
            {
                SandboxedThread.Asynchronous(delegate() { ((IClickable)highlighted).longClickMe(screen); });
                if (highlighted!=null)
                    highlighted.Mode = eModeType.Highlighted;
            }
            lastClick = null;
        }

        private void RenderingWindow_MouseDown(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if (highlighted != null)
            {
                if ((rParam.currentMode == eModeType.Highlighted) && (typeof(OMButton).IsInstanceOfType(highlighted)))
                {
                    if (lastClick != null)
                    {
                        lastClick.Mode = eModeType.Normal;
                        UpdateThisControl(lastClick.toRegion());
                    }
                    lastClick = (OMButton)highlighted;
                    if (lastClick.Mode == eModeType.transitioningOut)
                        lastClick.Mode = eModeType.ClickedAndTransitioningOut;
                    else
                        lastClick.Mode = eModeType.Clicked;
                    tmrLongClick.Enabled = true;
                    UpdateThisControl(lastClick.toRegion());
                }
                else if (typeof(IClickable).IsInstanceOfType(highlighted))  // Added by Borte to support long click for other controls than a button
                {
                    tmrLongClick.Enabled = true;
                }
                if (typeof(IMouse).IsInstanceOfType(highlighted))
                    ((IMouse)highlighted).MouseDown(screen, e, widthScale, heightScale);
                if (typeof(IThrow).IsInstanceOfType(highlighted) == true)
                {
                    ThrowStarted = true;
                    ThrowRelativeDistance = new Point(e.X,e.Y);
                }
            } ThrowStart = e.Location; //If we're not throwing something we're gesturing
        }

        private void RenderingWindow_MouseUp(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            tmrLongClick.Enabled = false;
            if ((lastClick != null) && (lastClick.DownImage.image != null))
            {
                lastClick.Mode = eModeType.Highlighted;
                UpdateThisControl(lastClick.toRegion());
            }
            if (highlighted != null)
            {
                if ((highlighted.Mode != eModeType.Clicked) && (highlighted.Mode != eModeType.ClickedAndTransitioningOut))
                    highlighted.Mode = eModeType.Highlighted;
                if (typeof(IMouse).IsInstanceOfType(highlighted) == true)
                    ((IMouse)highlighted).MouseUp(screen, e, widthScale, heightScale);
            }
            if (rParam.currentMode == eModeType.Scrolling)
            {
                rParam.currentMode = eModeType.Highlighted;
                if ((highlighted != null) && (typeof(IThrow).IsInstanceOfType(highlighted) == true))
                    ((IThrow)highlighted).MouseThrowEnd(screen, e.Location);
            }
            else if (rParam.currentMode == eModeType.gesturing)
            {
                if (currentGesture.Count > 0)
                {
                    AlphaRecognizer rec = new AlphaRecognizer();
                    rec.Initialize();
                    for (int i = 0; i < currentGesture.Count; i++)
                        rec.AddPoint(currentGesture[i], false);
                    Core.theHost.execute(eFunction.gesture, screen.ToString(), rec.Recognize());
                    currentGesture = null;
                    rParam.currentMode = eModeType.Highlighted;
                    RenderingWindow_MouseMove(sender, new OpenMobile.Input.MouseMoveEventArgs(e.X, e.Y, 0, 0, MouseButton.None));
                    Invalidate();
                }
            }
            ThrowStart.X = -1;
            ThrowStart.Y = -1;
            ThrowStarted = false;
        }
        #endregion
        #region OtherUIEvents
        protected override void OnLoad(EventArgs e)
        {
            g.Initialize(screen);
            base.OnLoad(e);
        }
        private void RenderingWindow_FormClosing(object sender,CancelEventArgs e)
        {
            try
            {
                Core.theHost.execute(eFunction.closeProgram);
            }
            catch (Exception) { }
        }
        void RenderingWindow_Closed(object sender, EventArgs e)
        {
            //
        }
        public void RenderingWindow_KeyUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (this.WindowState == WindowState.Fullscreen)
                {   //Escape full screen
                    fullscreen = false;
                    this.WindowState = WindowState.Normal;
                }
                else
                    Core.theHost.execute(eFunction.closeProgram);
            }
            else if (e.Key == Key.Enter)
            {
                tmrLongClick.Enabled = false;

                if (lastClick != null)
                {
                    keyboardActive = true;
                    tmrClick.Enabled = true;
                    SandboxedThread.Asynchronous(delegate() { lastClick.clickMe(screen); });
                    if(lastClick.DownImage.image != null)
                    {
                        lastClick.Mode = eModeType.Highlighted;
                        UpdateThisControl(lastClick.toRegion());
                    }
                }
            }
            if ((highlighted != null) && (typeof(IKey).IsInstanceOfType(highlighted) == true))
                ((IKey)highlighted).KeyUp(screen, e, widthScale, heightScale);
        }

        public void closeMe()
        {
            new Thread(delegate() { tmrClosing_Tick(null, null); }).Start();
        }

        public static void closeRenderer()
        {
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
                Core.RenderingWindows[i].closeMe();
        }
        private void RenderingWindow_Resize(object sender, EventArgs e)
        {
            heightScale = (this.ClientRectangle.Height / 600F);
            widthScale = (this.ClientRectangle.Width / 1000F);
            if (((this.WindowState == WindowState.Fullscreen)||(this.WindowState == WindowState.Maximized)) && (fullscreen == false))
            {
                fullscreen = true;
                this.WindowState = WindowState.Normal;
                this.WindowState = WindowState.Fullscreen;
                this.WindowBorder = WindowBorder.Hidden;
                fullscreen = false;
            }
            else if ((fullscreen == false)&&(WindowBorder!=WindowBorder.Resizable))
            {
                this.WindowBorder = WindowBorder.Resizable;
                fullscreen = false;
            }
            Invalidate();
            Core.theHost.raiseSystemEvent(eFunction.RenderingWindowResized, screen.ToString(), "", "");
        }
        #endregion
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
                case eGlobalTransition.CrossfadeFast:
                    rParam.globalTransitionIn += 0.2F;
                    rParam.globalTransitionOut -= 0.2F;
                    if (rParam.globalTransitionOut < 0.2F)
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
                        transitioning = false;
                        return;
                    }
                    ofsetOut = new Point(0, -(120 * tick));
                    ofsetIn = new Point(0, 600 - (120 * tick));
                    break;
                case eGlobalTransition.SlideDown:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        transitioning = false;
                        return;
                    }
                    ofsetOut = new Point(0, (120 * tick));
                    ofsetIn = new Point(0, (120 * tick) - 600);
                    break;
                case eGlobalTransition.SlideLeft:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        transitioning = false;
                        return;
                    }
                    ofsetOut = new Point(-200 * tick, 0);
                    ofsetIn = new Point(1000 - (200 * tick), 0);
                    break;
                case eGlobalTransition.SlideRight:
                    rParam.globalTransitionIn = 1;
                    tick++;
                    if (tick == 6)
                    {
                        transitioning = false;
                        return;
                    }
                    ofsetOut = new Point(200 * tick, 0);
                    ofsetIn = new Point((200 * tick) - 1000, 0);
                    break;
            }
            Invalidate();
        }
        public void RenderingWindow_KeyDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (highlighted == null)
            {
                if ((e.Key == Key.Left) || (e.Key == Key.Right) || (e.Key == Key.Up) || (e.Key == Key.Down))
                {
                    int top = 601;
                    int left = 1001;
                    OMControl b = null;
                    for (int i = 0; i < p.controlCount; i++)
                        //Modified by Borte
                        if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                            if ((p[i].Left < left) && (p[i].Top < top) && (inBounds(p[i].toRegion(), this.ClientRectangle) == true))
                            {
                                b = p[i];
                                top = b.Top;
                                left = b.Left;
                            }
                    if (b == null)
                        return;
                    b.Mode = eModeType.Highlighted;
                    highlighted = b;
                    UpdateThisControl(highlighted.toRegion());
                }
            }
            else
            {
                if (typeof(IKey).IsInstanceOfType(highlighted) == true)
                    if (((IKey)highlighted).KeyDown(screen, e, widthScale, heightScale))
                        return;
                int best = 1000;
                OMControl b = null;
                switch (e.Key)
                {
                    case Key.Left:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Left + p[i].Width <= highlighted.Left) && (inBounds(p[i].toRegion(), this.ClientRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        if (notCovered(p[i],'l') == true)
                                        {
                                            best = distance(highlighted.toRegion(), p[i].toRegion());
                                            b = p[i];
                                        }
                                    }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Key.Right:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Left >= highlighted.Left + highlighted.Width) && (inBounds(p[i].toRegion(), this.ClientRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        if (notCovered(p[i],'r') == true)
                                        {
                                            best = distance(highlighted.toRegion(), p[i].toRegion());
                                            b = p[i];
                                        }
                                    }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Key.Up:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Top + p[i].Height <= highlighted.Top) && (inBounds(p[i].toRegion(), this.ClientRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        if (notCovered(p[i],'u') == true)
                                        {
                                            best = distance(highlighted.toRegion(), p[i].toRegion());
                                            b = p[i];
                                        }
                                    }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Key.Down:
                        for (int i = 0; i < p.controlCount; i++)
                            if (typeof(IHighlightable).IsInstanceOfType(p[i]))
                                if ((p[i].Top >= highlighted.Top + highlighted.Height) && (inBounds(p[i].toRegion(), this.ClientRectangle) == true))
                                    if (distance(highlighted.toRegion(), p[i].toRegion()) < best)
                                    {
                                        if (notCovered(p[i],'d') == true)
                                        {
                                            best = distance(highlighted.toRegion(), p[i].toRegion());
                                            b = p[i];
                                        }
                                    }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        UpdateThisControl(highlighted.toRegion());
                        highlighted = b;
                        UpdateThisControl(highlighted.toRegion());
                        break;
                    case Key.Enter:
                        if (typeof(OMButton).IsInstanceOfType(highlighted))
                        {
                            lastClick = (OMButton)highlighted;
                            if (lastClick.Mode == eModeType.transitioningOut)
                                lastClick.Mode = eModeType.ClickedAndTransitioningOut;
                            else
                                lastClick.Mode = eModeType.Clicked;
                            tmrLongClick.Enabled = true;
                        }
                        else if (typeof(IClickable).IsInstanceOfType(highlighted))
                        {
                            SandboxedThread.Asynchronous(delegate() { ((IClickable)highlighted).clickMe(screen); });
                        }
                        break;
                }
            }
        }

        private bool notCovered(OMControl oMControl,char direction)
        {
            Point pnt=Point.Empty;
            switch(direction)
            {
                case 'l':
                    pnt=new Point(oMControl.Left+oMControl.Width,oMControl.Top+(oMControl.Height/2));
                    break;
                case 'r':
                    pnt=new Point(oMControl.Left,oMControl.Top+(oMControl.Height/2));
                    break;
                case 'd':
                    pnt=new Point(oMControl.Left+(oMControl.Width/2),oMControl.Top);
                    break;
                case 'u':
                    pnt=new Point(oMControl.Left+(oMControl.Width/2),oMControl.Top+oMControl.Height);
                    break;
            }
            for (int i = Core.theHost.RenderFirst - 1; i >= 0; i--)
            {
                if (p[i].Visible == false)
                    continue;
                if ((pnt.X >= p[i].Left) && (pnt.Y >= p[i].Top) && (pnt.X <= (p[i].Left + p[i].Width)) && (pnt.Y <= (p[i].Top + p[i].Height)))
                {

                    if (p[i] == oMControl)
                        return true;
                    else
                        return false;
                }
            }
            for (int i = p.controlCount - 1; i >= Core.theHost.RenderFirst; i--)
            {
                if (p[i].Visible == false)
                    continue;
                if ((pnt.X >= p[i].Left) && (pnt.Y >= p[i].Top) && (pnt.X <= (p[i].Left + p[i].Width)) && (pnt.Y <= (p[i].Top + p[i].Height)))
                {

                    if (p[i] == oMControl)
                        return true;
                    else
                        return false;
                }
            }
            return true;
        }

        private int distance(Rectangle r1, Rectangle r2)
        {
            return Math.Max(Math.Abs((r1.Left + r1.Width / 2) - (r2.Left + r2.Width / 2)) - (r1.Width + r2.Width) / 2, 0) + Math.Max(Math.Abs((r1.Top + r1.Height / 2) - (r2.Top + r2.Height / 2)) - (r1.Height + r2.Height) / 2, 0);
        }

        private void RenderingWindow_MouseLeave(object sender, EventArgs e)
        {
            if (rParam.currentMode == eModeType.Scrolling)
            {
                rParam.currentMode = eModeType.Highlighted;
                ThrowStart.Y = -1;
                if ((highlighted != null) && typeof(IThrow).IsInstanceOfType(highlighted))
                    ((IThrow)highlighted).MouseThrowEnd(screen, Point.Empty);
            }
            tmrLongClick.Enabled = false;
        }
    }
}
