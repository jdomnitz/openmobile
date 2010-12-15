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
        public bool VideoPlaying;
        OMControl varHighlighted;
        private renderingParams rParam = new renderingParams();
        object painting = new object();
        private Point ThrowStart = new Point(-1, -1);
        OMButton lastClick;
        List<OMPanel> backgroundQueue = new List<OMPanel>();
        float heightScale = 1F;
        float widthScale = 1F;
        Point ofsetIn = new Point(0, 0);
        Point ofsetOut = new Point(0, 0);
        private eGlobalTransition currentTransition;
        private bool transitioning = false;
        private bool keyboardActive;
        public MouseDevice currentMouse;
        private List<Point> currentGesture;
        // Throw started (will be reset when throw starts) for thrown interface
        private bool ThrowStarted = false;
        // Relative mouse moved distance for thrown interface
        private Point ThrowRelativeDistance = new Point(-1, -1);
        private int tick = 0;
        public PointF ScaleFactors
        {
            get
            {
                return new PointF(widthScale, heightScale);
            }
        }
        internal OMControl highlighted
        {
            get
            {
                return varHighlighted;
            }
            set
            {
                if (varHighlighted == value)
                    return;
                if ((varHighlighted != null) && (varHighlighted.Mode == eModeType.Highlighted))
                    varHighlighted.Mode = eModeType.Normal;
                varHighlighted = value;
                InputRouter.raiseHighlightChanged(varHighlighted, screen);
            }
        }

        public void RunAsync(GameWindowFlags flags)
        {
            Thread t = new Thread(delegate()
            {
                Run(flags);
            });
            t.TrySetApartmentState(ApartmentState.STA);
            t.Start();
        }
        public void Run(GameWindowFlags flags)
        {
            NativeInitialize(flags);
            InitializeRendering();
            Run(1.0, 0.0);
        }
        Graphics.Graphics g;
        public RenderingWindow(int s)
        {
            g = new OpenMobile.Graphics.Graphics(s);
            this.screen = s;
            this.tmrClick = new System.Timers.Timer(20);
            this.tmrLongClick = new System.Timers.Timer(500);
            this.tmrClick.Elapsed += new System.Timers.ElapsedEventHandler(this.tmrClick_Tick);
            this.tmrLongClick.Elapsed += new System.Timers.ElapsedEventHandler(this.tmrLongClick_Tick);
        }
        public void InitializeRendering()
        {
            if ((this.WindowState == WindowState.Fullscreen)&&(screen==0))
                Mouse.Location = this.Location;
            if (screen <= DisplayDevice.AvailableDisplays.Count - 1)
                this.Bounds = new Rectangle(DisplayDevice.AvailableDisplays[screen].Bounds.Location, this.Size);
            this.Title = "openMobile v" + Assembly.GetCallingAssembly().GetName().Version + " (" + OpenMobile.Framework.OSSpecific.getOSVersion() + ") Screen " + (screen + 1).ToString();
            this.Mouse.Instance = (screen * -1) - 1;
            this.Mouse.SetBounds(DisplayDevice.AvailableDisplays[screen].Width, DisplayDevice.AvailableDisplays[screen].Height);
            InitializeComponent();
            if (screen == 0)
                InputRouter.Initialize();
            if (Configuration.RunningOnWindows)
                if (options == GameWindowFlags.Fullscreen)
                    OnWindowStateChanged(EventArgs.Empty);
        }
        bool Identify = false;
        public void PaintIdentity()
        {
            Identify = true;
            Invalidate();
            Thread.Sleep(1500);
            Identify = false;
            Invalidate();
        }

        private void Invalidate()
        {
            refresh = true;
        }
        //Code Added by Borte
        public new int Width
        {
            set
            {
                base.Width = value + (this.Width - ClientSize.Width);
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

        #region ControlManagement
        public void transitionInPanel(OMPanel newP)
        {
            OMControl c;
            bool exists = backgroundQueue.Contains(newP);
            for (int i = 0; i < newP.controlCount; i++)
            {
                c = newP.getControl(i);
                c.UpdateThisControl += UpdateThisControl;
                if ((c.Mode == eModeType.ClickedAndTransitioningOut) || (c.Mode == eModeType.transitioningOut)||(exists))
                    c.Mode = eModeType.transitionLock;
                else
                    c.Mode = eModeType.transitioningIn;
            }
            if (exists)
                newP.Mode = eModeType.transitionLock;
            else if (newP.Mode == eModeType.transitioningOut)
                newP.Mode = eModeType.Normal;
            else
                newP.Mode = eModeType.transitioningIn;
            if (!exists)
                insertPanel(newP);
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }

        private void insertPanel(OMPanel newP)
        {
            for (int i = backgroundQueue.Count - 1; i >= 0; i--)
                if (backgroundQueue[i].Priority <= newP.Priority)
                {
                    backgroundQueue.Insert(i + 1, newP);
                    return;
                }
            backgroundQueue.Insert(0, newP);
        }
        public void UpdateThisControl(bool resetHighlighted)
        {
            Invalidate();
            if (resetHighlighted)
                MouseMove(new MouseMoveEventArgs(currentMouse.X, currentMouse.Y, 0, 0, MouseButton.None));
        }
        public bool blockHome = false;
        public bool transitionOutEverything()
        {
            if (blockHome)
                return false;
            highlighted = null;
            for (int i = backgroundQueue.Count - 1; i >= 0; i--)
                if ((backgroundQueue[i].Mode == eModeType.transitioningIn) || (backgroundQueue[i].UIPanel))
                    backgroundQueue[i].Mode = eModeType.Normal;
                else
                {
                    backgroundQueue[i].Mode = eModeType.transitioningOut;
                    for (int j = backgroundQueue[i].controlCount - 1; j >= 0; j--)
                        backgroundQueue[i][j].Mode = eModeType.transitioningOut;
                }
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
            return true;
        }
        public void transitionOutPanel(OMPanel oldP)
        {
            if (highlighted != null)
                highlighted.Mode = eModeType.Normal;
            highlighted = null;
            for (int i = 0; i < oldP.controlCount; i++)
                if (oldP.getControl(i).Mode != eModeType.transitionLock)
                    oldP[i].Mode = eModeType.transitioningOut;
            if (oldP.Mode == eModeType.transitioningIn)
                oldP.Mode = eModeType.Normal;
            else
                oldP.Mode = eModeType.transitioningOut;
            rParam.globalTransitionIn = 0;
            rParam.globalTransitionOut = 1;
        }
        public void executeTransition(eGlobalTransition transType)
        {
            while (!this.Visible)
                Thread.Sleep(10);
            if (transType != eGlobalTransition.None)
            {
                currentTransition = transType;
                transitioning = true;
                while (transitioning == true)
                {
                    transition_Tick();
                    Thread.Sleep(25);
                }
                Invalidate();
            }
            lock (painting)
            {
                tmrClick.Enabled = false;
                rParam.transparency = 1;
                rParam.transitionTop = 0;
                rParam.globalTransitionIn = 1;
                rParam.globalTransitionOut = 0;
            }
            foreach (OMPanel panel in backgroundQueue)
            {
                if (panel.Mode == eModeType.transitioningOut)
                    panel.Mode = eModeType.Highlighted;
                for (int i = panel.controlCount - 1; i >= 0; i--)
                {
                    if (panel.Mode == eModeType.transitioningOut)
                        panel[i].UpdateThisControl -= UpdateThisControl;
                    panel[i].Mode = eModeType.Normal;
                }
            }
            backgroundQueue.RemoveAll(q => q.Mode == eModeType.Highlighted);
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
            UpdateThisControl(true);
        }
        #endregion

        #region Overrides
        OImage identity;
        protected override void OnRenderFrame(EventArgs e)
        {
            //g.ResetClip();
            OnPaint();
            if ((currentGesture != null) && (currentGesture.Count > 0))
                lock (painting)
                    RenderGesture();
            if (Identify)
            {
                lock (painting)
                {
                    if (identity == null)
                        identity = g.GenerateTextTexture(0, 0, 1000, 600, (screen + 1).ToString(), new Font(Font.GenericSansSerif, 400F), eTextFormat.Outline, Alignment.CenterCenter, Color.White, Color.Black);
                    g.DrawImage(identity, 0, 0, 1000, 600);
                }
            }
            SwapBuffers(); //show the new image before potentially lagging
            g.Finish();
        }

        private void RenderGesture()
        {
            foreach (Point p in currentGesture)
                g.FillEllipse(new Brush(Color.Red), new Rectangle((int)((p.X - 10) / widthScale), (int)((p.Y - 10) / heightScale), 20, 20));
        }
        protected override void OnResize(EventArgs e)
        {
            MakeCurrent();
            g.Resize(Width, Height);
            base.OnResize(e);
            MakeCurrent(null);
        }
        protected void OnPaint()
        {
            lock (painting)
            {
                for (int i = 0; i < backgroundQueue.Count; i++)
                {
                    if (backgroundQueue[i].Mode == eModeType.transitioningIn)
                        modifyIn(g);
                    else if (backgroundQueue[i].Mode == eModeType.transitioningOut)
                        modifyOut(g);
                    else
                        g.ResetTransform(); //modify neutral
                    backgroundQueue[i].Render(g, rParam);
                }
            }
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
        private static bool inBounds(Rectangle control, Rectangle bounds)
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
                            lastClick.Mode = eModeType.Highlighted;
                        }
                        else
                        {
                            lastClick.Mode = eModeType.Normal;
                            //Recheck where the mouse is at
                            MouseMove(new OpenMobile.Input.MouseMoveEventArgs(currentMouse.X, currentMouse.Y, 0, 0, MouseButton.None));
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

                    if (keyboardActive == true)
                    {
                        lastClick.Mode = eModeType.Highlighted;
                    }
                    else
                    {
                        //Recheck where the mouse is at
                        MouseMove(new OpenMobile.Input.MouseMoveEventArgs(currentMouse.X, currentMouse.Y, 0, 0, MouseButton.None));
                    }
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
            Invalidate();
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
        internal void RenderingWindow_MouseMove(object sender, MouseMoveEventArgs e)
        {
            int scr = (int)sender;
            if (scr == screen)
            {
                keyboardActive = false;
                MouseMove(e);
            }
        }
        private void MouseMove(MouseMoveEventArgs e)
        {
            if (keyboardActive)
                return;
            bool done = false; //We found something that was selected
            if (rParam.currentMode == eModeType.Scrolling)
            {
                if (e.Buttons==MouseButton.Left)
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
                if ((e.Buttons==MouseButton.Left) && (!ThrowStarted))
                {
                    if (currentGesture == null)
                    {
                        if ((System.Math.Abs(e.X - ThrowStart.X) <= (int)(20 * widthScale)) && (System.Math.Abs(e.Y - ThrowStart.Y) <= (int)(20 * heightScale)))
                            return;
                        currentGesture = new List<Point>();
                        rParam.currentMode = eModeType.gesturing;
                    }
                    currentGesture.Add(e.Location);
                    Invalidate();
                    if (lastClick != null)
                        lastClick.Mode = eModeType.Highlighted;
                }
                else
                {
                    done = checkControl(e);
                    if (highlighted != null)
                    {
                        if (typeof(IMouse).IsInstanceOfType(highlighted) == true)
                            try
                            {
                                ((IMouse)highlighted).MouseMove(screen, e, widthScale, heightScale);
                            }
                            catch (Exception ex) { SandboxedThread.handle(ex); }
                        if (typeof(IThrow).IsInstanceOfType(highlighted) == true)
                            if (ThrowStarted)
                                if (System.Math.Abs(e.X - ThrowStart.X) > 3 || (System.Math.Abs(e.Y - ThrowStart.Y) > 3))
                                {
                                    bool cancel = false;
                                    ((IThrow)highlighted).MouseThrowStart(screen, ThrowStart, new PointF(widthScale, heightScale), ref cancel);
                                    if (cancel == false)
                                        rParam.currentMode = eModeType.Scrolling;
                                }

                        if (done == false)
                        {
                            highlighted = null;
                            Invalidate();
                        }
                    }
                }
            }
        }
        private bool checkControl(OpenMobile.Input.MouseMoveEventArgs e)
        {
            OMControl b;
            for (int i = backgroundQueue.Count - 1; i >= 0; i--)
            {
                for (int j = backgroundQueue[i].controlCount - 1; j >= 0; j--)
                {
                    b = backgroundQueue[i][j];
                    //Note potential drawing error with updated rectangle
                    if ((e.X > (b.Left * widthScale)) && (e.Y > (b.Top * heightScale)) && (e.X < ((b.Left + b.Width) * widthScale)) && (e.Y < ((b.Top + b.Height) * heightScale)))
                    {
                        if (b.Visible == true)
                        {
                            if (typeof(INotClickable).IsInstanceOfType(b))
                                if (!((INotClickable)b).IsPointClickable((int)(e.X / widthScale), (int)(e.Y / widthScale)))
                                    continue;
                            rParam.currentMode = eModeType.Highlighted;
                            if ((b.Mode == eModeType.Normal))
                                b.Mode = eModeType.Highlighted;
                            if (b == highlighted)
                                return true;
                            highlighted = b;
                            Invalidate();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        internal void RenderingWindow_MouseClick(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != screen)
                return;
            if ((e.Buttons == MouseButton.Left) && (highlighted != null))
            {
                if (rParam.currentMode == eModeType.Highlighted)
                {
                    if (typeof(OMButton).IsInstanceOfType(highlighted))
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
                    if (lastClick != null)
                        lastClick.Mode = eModeType.Normal;
                    lastClick = null;
                    if (typeof(IClickable).IsInstanceOfType(highlighted) == true)
                    {
                        SandboxedThread.Asynchronous(delegate() 
                        { 
                            if (highlighted != null) 
                                ((IClickable)highlighted).clickMe(screen); 
                        });
                    }
                }
            }
        }
#if false
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
                            tmrClick.Enabled = true;
                            lastClick.Mode = eModeType.Clicked;
                            SandboxedThread.Asynchronous(delegate() { if (lastClick != null) lastClick.clickMe(screen); });
                        }
                    }
                    else if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted) == true))
                    {
                        SandboxedThread.Asynchronous(delegate() { (highlighted as IClickable).clickMe(screen); });
                    }
                }
            }
        }
#endif
        private void tmrLongClick_Tick(object sender, EventArgs e)
        {
            tmrLongClick.Enabled = false;
            if (rParam.currentMode == eModeType.gesturing)
                return;
            if ((highlighted != null) && (typeof(IClickable).IsInstanceOfType(highlighted)))
            {
                SandboxedThread.Asynchronous(delegate() { ((IClickable)highlighted).longClickMe(screen); });
                if (highlighted != null)
                    highlighted.Mode = eModeType.Highlighted;
            }
            lastClick = null;
        }

        internal void RenderingWindow_MouseDown(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != screen)
                return;
            keyboardActive = false;
            if (highlighted != null)
            {
                if ((rParam.currentMode == eModeType.Highlighted) && (typeof(OMButton).IsInstanceOfType(highlighted)))
                {
                    if (lastClick != null)
                    {
                        lastClick.Mode = eModeType.Normal;
                    }
                    lastClick = (OMButton)highlighted;
                    if (lastClick.Mode == eModeType.transitioningOut)
                        lastClick.Mode = eModeType.ClickedAndTransitioningOut;
                    else
                        lastClick.Mode = eModeType.Clicked;
                    tmrLongClick.Enabled = true;
                    Invalidate();
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
                    ThrowRelativeDistance = new Point(e.X, e.Y);
                }
            } ThrowStart = e.Location; //If we're not throwing something we're gesturing
        }

        internal void RenderingWindow_MouseUp(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != screen)
                return;
            tmrLongClick.Enabled = false;
            OMButton ptrLast = lastClick;
            if ((ptrLast != null) && (ptrLast.DownImage.image != null))
            {
                ptrLast.Mode = eModeType.Highlighted;
                Invalidate();
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
                    rParam.currentMode = eModeType.Highlighted;
                    MouseMove(new OpenMobile.Input.MouseMoveEventArgs(e.X, e.Y, 0, 0, MouseButton.None));
                }
                Invalidate();
            }
            ThrowStart.X = -1;
            ThrowStart.Y = -1;
            ThrowStarted = false;
            currentGesture = null;
        }
        #endregion
        #region OtherUIEvents
        protected override void OnLoad(EventArgs e)
        {
            g.Initialize(screen);
            if (screen == 0)
            {
                if ((Graphics.Graphics.Renderer == "GDI Generic") || (Graphics.Graphics.Renderer == "Software Rasterizer"))
                    Application.ShowError(this.WindowHandle, "This application has been forced to use software rendering.  Performance will be horrible until you install proper graphics drivers!!", "Performance Warning");
            }
            base.OnLoad(e);
        }
        private void RenderingWindow_FormClosing(object sender, CancelEventArgs e)
        {
            try
            {
                if (screen == 0)
                    Core.theHost.execute(eFunction.closeProgram);
            }
            catch (Exception) { }
        }
        public int Screen
        {
            get
            {
                return screen;
            }
        }
        public void RenderingWindow_KeyUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                {
                    if (screen == 0)
                        Core.theHost.execute(eFunction.closeProgram);
                    else
                        CloseMe();
                }
            }
            else if (e.Key == Key.Enter)
            {
                tmrLongClick.Enabled = false;

                if (lastClick != null)
                {
                    keyboardActive = true;
                    tmrClick.Enabled = true;
                    SandboxedThread.Asynchronous(delegate() 
                    {
                        if (lastClick!=null)
                            lastClick.clickMe(screen); 
                    });
                    if ((lastClick!=null)&&(lastClick.FocusImage.image != null))
                    {
                        lastClick.Mode = eModeType.Highlighted;
                        Invalidate();
                    }
                }
            }
            if ((highlighted != null) && (typeof(IKey).IsInstanceOfType(highlighted) == true))
                ((IKey)highlighted).KeyUp(screen, e, widthScale, heightScale);
        }

        public void CloseMe()
        {
            SandboxedThread.Asynchronous(delegate() { tmrClosing_Tick(null, null); });
        }

        void RenderingWindow_Gesture(object sender, OpenMobile.Graphics.TouchEventArgs e)
        {
            string gesture="";
            if (e.GestureComplete)
                gesture = "End";
            gesture+=e.Name+"|";
            if (e.Name=="Rotate")
                gesture+=e.Arg1.ToString()+"|";
            else
                gesture=(e.Arg1*totalScale).ToString("0.00")+"|";
            gesture += ((e.Position.X - this.X) * widthScale).ToString()+",";
            gesture += ((e.Position.Y - this.Y) * heightScale).ToString();
            Core.theHost.execute(eFunction.multiTouchGesture, screen.ToString(), gesture);
        }
        public static void CloseRenderer()
        {
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
                Core.RenderingWindows[i].CloseMe();
        }
        protected override void OnWindowStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Fullscreen;
            if ((this.WindowState == WindowState.Fullscreen) && (!defaultMouse))
            {
                if (screen == 0)
                    Mouse.TrapCursor();
                Mouse.HideCursor(this.WindowInfo);
            }
            else
            {
                if ((screen == 0)&& (!defaultMouse))
                    Mouse.UntrapCursor();
                Mouse.ShowCursor(this.WindowInfo);
            }
            base.OnWindowStateChanged(e);
        }
        public bool defaultMouse = false;
        float totalScale = 1F;
        private void RenderingWindow_Resize(object sender, EventArgs e)
        {
            heightScale = (this.ClientRectangle.Height / 600F);
            widthScale = (this.ClientRectangle.Width / 1000F);
            totalScale = (float)System.Math.Sqrt(System.Math.Pow(heightScale, 2) + System.Math.Pow(widthScale, 2));
            OnRenderFrameInternal();
            SandboxedThread.Asynchronous(raiseResize);
        }
        private void raiseResize()
        {
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
                    rParam.globalTransitionIn += 0.15F;
                    rParam.globalTransitionOut -= 0.15F;
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
                    for (int j = 0; j < backgroundQueue.Count; j++)
                        for (int i = 0; i < backgroundQueue[j].controlCount; i++)
                            //Modified by Borte
                            if (typeof(IHighlightable).IsInstanceOfType(backgroundQueue[j][i]))
                                if ((backgroundQueue[j][i].Left < left) && (backgroundQueue[j][i].Top < top) && (inBounds(backgroundQueue[j][i].toRegion(), OpenMobile.Graphics.Graphics.NoClip) == true))
                                {
                                    b = backgroundQueue[j][i];
                                    top = b.Top;
                                    left = b.Left;
                                }
                    if (b == null)
                        return;
                    b.Mode = eModeType.Highlighted;
                    highlighted = b;
                    Invalidate();
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
                        for (int j = 0; j < backgroundQueue.Count; j++)
                            for (int i = 0; i < backgroundQueue[j].controlCount; i++)
                                if (typeof(IHighlightable).IsInstanceOfType(backgroundQueue[j][i]))
                                    if ((backgroundQueue[j][i].Left < highlighted.Left) && (inBounds(backgroundQueue[j][i].toRegion(), OpenMobile.Graphics.Graphics.NoClip) == true))
                                        if (xdistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion()) < best)
                                        {
                                            if (notCovered(backgroundQueue[j][i]) == true)
                                            {
                                                best = xdistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion());
                                                b = backgroundQueue[j][i];
                                            }
                                        }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardExit(screen);
                        highlighted = b;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardEnter(screen);
                        Invalidate();
                        break;
                    case Key.Right:
                        for (int j = 0; j < backgroundQueue.Count; j++)
                            for (int i = 0; i < backgroundQueue[j].controlCount; i++)
                                if (typeof(IHighlightable).IsInstanceOfType(backgroundQueue[j][i]))
                                    if ((backgroundQueue[j][i].Left > highlighted.Left) && (inBounds(backgroundQueue[j][i].toRegion(), OpenMobile.Graphics.Graphics.NoClip) == true))
                                        if (xdistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion()) < best)
                                        {
                                            if (notCovered(backgroundQueue[j][i]) == true)
                                            {
                                                best = xdistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion());
                                                b = backgroundQueue[j][i];
                                            }
                                        }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardExit(screen);
                        highlighted = b;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardEnter(screen);
                        Invalidate();
                        break;
                    case Key.Up:
                        for (int j = 0; j < backgroundQueue.Count; j++)
                            for (int i = 0; i < backgroundQueue[j].controlCount; i++)
                                if (typeof(IHighlightable).IsInstanceOfType(backgroundQueue[j][i]))
                                    if ((backgroundQueue[j][i].Top < highlighted.Top) && (inBounds(backgroundQueue[j][i].toRegion(), OpenMobile.Graphics.Graphics.NoClip) == true))
                                        if (ydistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion()) < best)
                                        {
                                            if (notCovered(backgroundQueue[j][i]) == true)
                                            {
                                                best = ydistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion());
                                                b = backgroundQueue[j][i];
                                            }
                                        }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardExit(screen);
                        highlighted = b;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardEnter(screen);
                        Invalidate();
                        break;
                    case Key.Down:
                        for (int j = 0; j < backgroundQueue.Count; j++)
                            for (int i = 0; i < backgroundQueue[j].controlCount; i++)
                                if (typeof(IHighlightable).IsInstanceOfType(backgroundQueue[j][i]))
                                    if ((backgroundQueue[j][i].Top > highlighted.Top) && (inBounds(backgroundQueue[j][i].toRegion(), OpenMobile.Graphics.Graphics.NoClip) == true))
                                        if (ydistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion()) < best)
                                        {
                                            if (notCovered(backgroundQueue[j][i]) == true)
                                            {
                                                best = ydistance(highlighted.toRegion(), backgroundQueue[j][i].toRegion());
                                                b = backgroundQueue[j][i];
                                            }
                                        }
                        if (b == null)
                            break;
                        b.Mode = eModeType.Highlighted;
                        highlighted.Mode = eModeType.Normal;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardExit(screen);
                        highlighted = b;
                        if (typeof(IKeyboard).IsInstanceOfType(highlighted))
                            ((IKeyboard)highlighted).KeyboardEnter(screen);
                        Invalidate();
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
                            Invalidate();
                        }
                        else if (typeof(IClickable).IsInstanceOfType(highlighted))
                        {
                            SandboxedThread.Asynchronous(delegate() { ((IClickable)highlighted).clickMe(screen); });
                        }
                        break;
                }
            }
        }

        private bool notCovered(OMControl oMControl)
        {
            for (int h = backgroundQueue.Count - 1; h >= 0; h--)
            {
                for (int i = backgroundQueue[h].controlCount - 1; i >= 0; i--)
                {
                    if (backgroundQueue[h][i].Visible == false)
                        continue;
                    if ((oMControl.Left >= backgroundQueue[h][i].Left) && (oMControl.Top >= backgroundQueue[h][i].Top) && ((oMControl.Left+oMControl.Width) <= (backgroundQueue[h][i].Left + backgroundQueue[h][i].Width)) && ((oMControl.Top+oMControl.Height) <= (backgroundQueue[h][i].Top + backgroundQueue[h][i].Height)))
                    {
                        
                        if (backgroundQueue[h][i] == oMControl)
                            return true;
                        else
                            return false;
                    }
                }
                if (backgroundQueue[h].BackgroundType != backgroundStyle.None)
                    return false;
            }
            return true;
        }

        private static int xdistance(Rectangle r1, Rectangle r2)
        {
            return (int)System.Math.Sqrt(System.Math.Pow((r2.Left + r2.Width / 2) - (r1.Left + r1.Width / 2), 2) + 8 * System.Math.Pow((r2.Top + r2.Height / 2) - (r1.Top + r1.Height / 2), 2));
        }
        private static int ydistance(Rectangle r1, Rectangle r2)
        {
            return (int)System.Math.Sqrt(System.Math.Pow((r2.Left + r2.Width / 2) - (r1.Left + r1.Width / 2), 2) * 8 + System.Math.Pow((r2.Top + r2.Height / 2) - (r1.Top + r1.Height / 2), 2));
        }
        private void RenderingWindow_MouseLeave(object sender, EventArgs e)
        {
            if (rParam.currentMode == eModeType.Scrolling)
            {
                rParam.currentMode = eModeType.Highlighted;
                ThrowStart.Y = -1;
                ThrowStart.X = -1;
                if ((highlighted != null) && typeof(IThrow).IsInstanceOfType(highlighted))
                    ((IThrow)highlighted).MouseThrowEnd(screen, Point.Empty);
            }
            tmrLongClick.Enabled = false;
        }
        void RenderingWindow_ResolutionChange(object sender, OpenMobile.Graphics.ResolutionChange e)
        {
            DisplayDevice dev=DisplayDevice.AvailableDisplays[screen];
            if (e.Landscape != dev.Landscape)
                Core.theHost.raiseSystemEvent(eFunction.screenOrientationChanged, screen.ToString(), e.Landscape ? "Landscape" : "Portrait", "");
        }
        internal void Rollback()
        {
            foreach (OMPanel panel in backgroundQueue)
            {
                if (panel.Mode == eModeType.transitioningOut)
                    panel.Mode = eModeType.Normal;
                for (int i = panel.controlCount - 1; i >= 0; i--)
                {
                    if (panel[i].Mode == eModeType.transitioningIn)
                        panel[i].UpdateThisControl -= UpdateThisControl;
                    panel[i].Mode = eModeType.Normal;
                }
            }
            backgroundQueue.RemoveAll(p => p.Mode == eModeType.transitioningIn);
        }
    }
}
