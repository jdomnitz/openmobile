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
using System.Runtime.CompilerServices;
using System.Threading;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Input;
using OpenMobile.Plugin;
using System.Diagnostics;
using OpenMobile.Framework;
using OpenMobile.helperFunctions.Graphics;
using System.Linq;
using OpenTK;

namespace OpenMobile
{
    public class RenderingWindow : OpenTK.GameWindow, iRenderingWindow
    {

        /// <summary>
        /// Current _Screen dimming value
        /// </summary>
        private int dimmer;

        bool Identify;
        float IdentifyOpacity = 1f;
        public Graphics.Graphics g;
        MouseData _MouseData = new MouseData();
        float CursorDistance = 0f;
        Point CursorDistanceXYTotal = new Point();
        Point CursorDistanceXYRelative = new Point();
        PointF CursorSpeed = new PointF();
        Stopwatch swCursorSpeedTiming = new Stopwatch();
        PointF CursorSpeedScaling = new PointF(1, 1);
        bool BlockRendering = false;
        bool _RenderingReset = false;
        bool _ResizeRequired = false;

        double _FPS;
        double _FPS_Max = double.MinValue;
        double _FPS_Min = double.MaxValue;
        bool _Refresh = true;
        bool _StopRendering = false;
        double _SecondsSinceLastRender = 0;
        double _SecondsBetweenMinRenderFrame = 1;
        int _FPS_SleepTime = 1;
        bool _StartupControl = true;

        /// <summary>
        /// The graphic device
        /// </summary>
        public Graphics.Graphics graphics
        {
            get
            {
                return g;
            }
        }

        /// <summary>
        /// Startup info text
        /// </summary>
        public string StartupInfoText
        {
            get
            {
                return this._StartupInfoText;
            }
            set
            {
                if (this._StartupInfoText != value)
                {
                    this._StartupInfoText = value;

                    // Update label text
                    if (panelStartUp != null)
                    {
                        OMLabel lbl = ((OMLabel)panelStartUp["lblStartUpInfo"]);
                        if (lbl != null)
                            lbl.Text = this._StartupInfoText;
                    }
                }

            }
        }
        private string _StartupInfoText = "Initializing...";        

        /// <summary>
        /// Contains the currently focused control's parent object (if any)
        /// <para>The control is only set if a control implements certain interfaces (like iContainer)</para>
        /// </summary>
        OMControl FocusedControlParent = null;
        OMControl FocusedControl = null;
        OMControl MouseOverControl = null;
        private List<Point> currentGesture = new List<Point>();
        private bool ThrowActive;        

        Timer tmrClickHold = new Timer(500);
        Stopwatch swClickTiming = new Stopwatch();

        ReDrawTrigger ReDrawPanel;

        private Point MouseMoveStartPoint = new Point();

        public Rectangle ApplicationArea = new Rectangle(0, 0, 1000, 600);

        /// <summary>
        /// List of panels to be rendered, the list is organized in accordance with panel priority (lowest priority at the start of the queue)
        /// </summary>
        private List<OMPanel> RenderingQueue = new List<OMPanel>();

        /// <summary>
        /// Rendering parameters passed on to each panel
        /// </summary>
        private renderingParams RenderingParam = new renderingParams();

        private renderingParams TransitionEffectParam_In = new renderingParams();
        private renderingParams TransitionEffectParam_Out = new renderingParams();


        /// <summary>
        /// Render lock, IS THIS NEEDED?
        /// </summary>
        object painting = new object();

        /// <summary>
        /// [REPLACE WITH TOPMOST CONTROLS INSTEAD] Blocks the functionality of TransitionOutEverything
        /// </summary>
        public bool blockHome { get; set; }

        /// <summary>
        /// Form title string
        /// </summary>
        public string FormTitle { get; internal set; }


        /// <summary>
        /// Screen number
        /// </summary>
        public int Screen
        {
            get
            {
                return this._Screen;
            }
            set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                }
            }
        }
        private int _Screen;        

        private PointF _ScaleFactors = new PointF(1,1);
        /// <summary>
        /// The scale factors for the _Screen
        /// </summary>
        public PointF ScaleFactors
        {
            get
            {
                return _ScaleFactors;
            }
            private set
            {
                if (_ScaleFactors == value)
                    return;

                _ScaleFactors = value;
                g.SetScaleFactors(_ScaleFactors);
                RefreshAllControls();
            }
        }

        /// <summary>
        /// Requests a refresh of all control graphics
        /// </summary>
        private void RefreshAllControls()
        {
            foreach (OMPanel panel in RenderingQueue)
            {
                foreach (OMControl control in panel.Controls)
                {
                    if (typeof(IContainer2).IsInstanceOfType(control) && control.IsControlRenderable())
                    {
                        foreach (OMControl containerControl in ((IContainer2)control).Controls)
                            containerControl.RefreshGraphic();
                    }
                    else
                        control.RefreshGraphic();
                }
            }
        }

        /// <summary>
        /// The aspect ratio of the _Screen
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return (float)System.Math.Sqrt(System.Math.Pow(_ScaleFactors.Y, 2) + System.Math.Pow(_ScaleFactors.X, 2));
            }
        }

        public RenderingWindow(int s, Size initalScreenSize, OpenTK.GameWindowFlags flags)
            : base(initalScreenSize.Width, initalScreenSize.Height, OpenTK.Graphics.GraphicsMode.Default, "OpenMobile", flags)
        {
            g = new OpenMobile.Graphics.Graphics(s);
            this._Screen = s;

            // Register redraw method
            ReDrawPanel = new ReDrawTrigger(Invalidate);
        }

        public bool IsDisposed = false;
        public override void Dispose()
        {
            IsDisposed = true;
            if (tmrClickHold != null)
                tmrClickHold.Dispose();
            base.Dispose();
                    }
        protected override void Dispose(bool manual)
        {
            if (tmrClickHold != null)
                tmrClickHold.Dispose();
            base.Dispose(manual);
        }

        private string GetProductNameAndVersion()
        {
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location);
            return String.Format("OpenMobile [{0}.{1}.{2}.{3}]", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InitializeRendering()
        {
            base.MakeCurrent();

            if (BuiltInComponents.SystemSettings.OpenGLVSync)
                base.VSync = OpenTK.VSyncMode.On;
            else
                base.VSync = OpenTK.VSyncMode.Off;

            // Check for specific startup _Screen
            int StartupScreen = Core.theHost.StartupScreen;

            // Set bounds
            if (_Screen <= DisplayDevice.AvailableDisplays.Count - 1)
                this.Bounds = new System.Drawing.Rectangle(DisplayDevice.AvailableDisplays[StartupScreen > 0 ? StartupScreen : _Screen].Bounds.Location, this.Size);

            FormTitle = String.Format("{0} ({1}) Screen {2}", GetProductNameAndVersion(), OpenMobile.Framework.OSSpecific.getOSVersion(), _Screen);
            this.Title = FormTitle;
            
            // Connect events
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommonResources));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("Icon")));
            //this.MouseLeave += new System.EventHandler<System.EventArgs>(this.RenderingWindow_MouseLeave);
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(this.RenderingWindow_FormClosing);
            this.Resize += new EventHandler<EventArgs>(this.RenderingWindow_Resize);
            this.Move += new EventHandler<EventArgs>(this.RenderingWindow_Resize);
            //this.Gesture += new EventHandler<OpenMobile.Graphics.TouchEventArgs>(RenderingWindow_Gesture);
            //this.ResolutionChange += new EventHandler<OpenMobile.Graphics.ResolutionChange>(RenderingWindow_ResolutionChange);
            tmrClickHold.Elapsed += new System.Timers.ElapsedEventHandler(tmrClickHold_Elapsed);

            //// Start input router
            //if (_Screen == 0)
            //    InputRouter.Initialize();
        }

        void tmrClickHold_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Disable timer to prevent multiple hits
            tmrClickHold.Enabled = false;
            
            // Exit if no control is focused
            if (FocusedControl == null)
                return;

            // Exit if gestures is active
            if (Gesture_Active)
                return;

            // Activate hold click
            ActivateClick(FocusedControl, ClickTypes.Hold, (tmrClickHold.Tag is MouseButtonEventArgs ? tmrClickHold.Tag : null) as MouseButtonEventArgs);
        }

        protected override void OnLoad(EventArgs e)
        {
            InitializeRendering();
            g.Initialize(this, _MouseData);
            if (_Screen == 0)
            {
                if ((Graphics.Graphics.Renderer == "GDI Generic") || (Graphics.Graphics.Renderer == "Software Rasterizer"))
                    Application.ShowError(base.WindowInfo.Handle, "This application has been forced to use software rendering.  Performance will be horrible until you install proper graphics drivers!!", "Performance Warning");
            }
            base.OnLoad(e);

            // Init startup logo
            Initialize_StartUpPanel();
        }

        public new void Run()
        {
            try
            {
                Run(0);
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg(String.Format("RenderingWindow[{0}].Run Exception", _Screen), e);
            }
        }

        public void RunAsync()
        {
            Thread t = new Thread(delegate()
            {
                Run();
            });
            t.TrySetApartmentState(ApartmentState.STA);
            t.Name = String.Format("RenderingWindow_{0}.RunAsync", _Screen);
            t.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _SecondsSinceLastRender += e.Time;
            // Wait for a refresh
            if (!_Refresh & (_SecondsSinceLastRender < _SecondsBetweenMinRenderFrame))
            {
                for (int i = 0; i < _FPS_SleepTime; i++)
                {
                    if (_Refresh)
                        break;
                    Thread.Sleep(1);
                }
                
                if (!_Refresh)
                    return;
            }

            if (_StopRendering)
                return;

            _FPS = 1 / _SecondsSinceLastRender;
            _SecondsSinceLastRender = 0;

            if (_Refresh)
            {
                _FPS_Max = System.Math.Max(_FPS_Max, _FPS);
                if (_FPS_Max > 15)
                    _FPS_Min = System.Math.Min(_FPS_Min, _FPS);
            }

            _Refresh = false;

            if (_ResizeRequired)
            {
                g.Resize(Width, Height);
                _ResizeRequired = false;
            }

            // Remove startup panel if present when additional panels are shown
            if (_StartupControl)
            {
                _StartupControl = false;
                if (RenderingQueue.Contains(panelStartUp) && RenderingQueue.Count > 1)
                    RenderingQueue.Remove(panelStartUp);
            }

            g.Clear(Color.Black);
            g.ResetClip();

            // Inform graphics that rendering begins
            g.Begin();

            RenderPanels();

            // Render gestures
            RenderGesture();

            // Render cursors
            RenderCursor();

            // Render identity (if needed)
            RenderIndentity();

            // Render debug info (if needed)
            RenderDebugInfo();
            //ShowDebugInfoTitle();

            // Render a "dimmer" overlay to reduce _Screen brightness
            RenderDimmer();

            SwapBuffers(); //show the new image before potentially lagging

            // Inform graphics that rendering ends
            g.End();
        }

        #region Local renders

        private OMImage imgStartUpLogo;
        private OMPanel panelStartUp;
        private StringWrapper startUpString = new StringWrapper();
        private OImage startUpStringTexture = null;

        private void Initialize_StartUpPanel()
        {
            panelStartUp = new OMPanel("panelStartUp");
            panelStartUp.FastRendering = true;

            imageItem img = OM.Host.getSkinImage("Icons|Icon-OM_Large");
            OImage oImg = img.image.Copy();
            oImg.Overlay(BuiltInComponents.SystemSettings.SkinFocusColor);
            oImg.Glow(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor));
            oImg.ShaderEffect = OMShaders.Radar;
            img = new imageItem(oImg);

            imgStartUpLogo = new OMImage("imgStartUpLogo", 0, 0, img);
            imgStartUpLogo.Left = (base.ClientRectangle.Width / 2) - (imgStartUpLogo.Region.Center.X);
            imgStartUpLogo.Top = (base.ClientRectangle.Height / 2) - (imgStartUpLogo.Region.Center.Y);
            panelStartUp.addControl(imgStartUpLogo);

            OMLabel lblStartUpProductText = new OMLabel("lblStartUpProductText", 0, imgStartUpLogo.Region.Bottom, base.ClientRectangle.Width, 40);
            lblStartUpProductText.Text = GetProductNameAndVersion();
            lblStartUpProductText.TextAlignment = Alignment.CenterCenter;
            panelStartUp.addControl(lblStartUpProductText);

            OMLabel lblStartUpInfo = new OMLabel("lblStartUpInfo", 0, lblStartUpProductText.Region.Bottom, base.ClientRectangle.Width, 40);
            lblStartUpInfo.Text = _StartupInfoText;
            lblStartUpInfo.TextAlignment = Alignment.CenterCenter;
            panelStartUp.addControl(lblStartUpInfo, ControlDirections.Down);

            this.TransitionInPanel(panelStartUp);
            this.ExecuteTransition(eGlobalTransition.CollapseGrowCenter.ToString(), 1.0f);

            //panelStartUp.Render(g, RenderingParam);
            //_Refresh = true;

            //startUpString.Text = String.Format("Startup string");
            //if (startUpString.Changed)
            //    startUpStringTexture = g.GenerateTextTexture(startUpStringTexture, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height, startUpString.Text, new Font(Font.Arial, 48), eTextFormat.GlowBoldBig, Alignment.TopLeft, Color.Blue, Color.Blue);
            //g.DrawImage(startUpStringTexture, 0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);

        }

        private void RenderDimmer()
        {
            if (dimmer > 0)
                g.FillRectangle(new Brush(Color.FromArgb(dimmer, Color.Black)), 0, 0, 1000, 600);
        }

        private StringWrapper DebugString = new StringWrapper();
        private OImage RenderDebugInfoTexture = null;
        private string RenderUpdateMarker = "-";
        private void RenderDebugInfo()
        {
            if (BuiltInComponents.Host.ShowDebugInfo)
            {
                lock (painting)
                {
                    switch (RenderUpdateMarker)
                    {
                        case @"-":
                            RenderUpdateMarker = @"\";
                            break;
                        case @"\":
                            RenderUpdateMarker = @"|";
                            break;
                        case @"|":
                            RenderUpdateMarker = @"/";
                            break;
                        case @"/":
                            RenderUpdateMarker = @"-";
                            break;
                        default:
                            RenderUpdateMarker = @"-";
                            break;
                    }
                    DebugString.Text = String.Format("OpenGL version {10} / OMEngine: {11} / Renderer: {12}\nScreen: {0} {1}\nFPS: {2:0}/{3:0}/{4:0}\nFocus: {5}.{6}\nFocusParent: {7}\nUnderMouse: {8}.{9}", _Screen, RenderUpdateMarker, _FPS_Min, _FPS, _FPS_Max, (FocusedControl != null && FocusedControl.Parent != null ? FocusedControl.Parent.Name : ""), (FocusedControl != null ? FocusedControl.Name : ""), (FocusedControlParent != null ? FocusedControlParent.Name : ""), (MouseOverControl != null && MouseOverControl.Parent != null ? MouseOverControl.Parent.Name : ""), (MouseOverControl != null ? MouseOverControl.Name : ""), Graphics.Graphics.Version, Graphics.Graphics.GraphicsEngine, Graphics.Graphics.Renderer);
                    if (DebugString.Changed)
                        RenderDebugInfoTexture = g.GenerateTextTexture(RenderDebugInfoTexture, 0, 0, 1000, 300, DebugString.Text, new Font(Font.Arial, 12), eTextFormat.Normal, Alignment.TopLeft, Color.Yellow, Color.Yellow);
                    g.DrawImage(RenderDebugInfoTexture, 0, 0, 1000, 300);
                }
            }
        }

        private OImage identity = new OImage();
        private void RenderIndentity()
        {
            if (Identify)
            {
                lock (painting)
                {
                    if (identity.TextureGenerationRequired(_Screen))
                        identity = g.GenerateTextTexture(identity, 0, 0, 1000, 600, _Screen.ToString(), new Font(Font.GenericSansSerif, 400F), eTextFormat.Outline, Alignment.CenterCenter, Color.White, Color.Black);
                    g.DrawImage(identity, 0, 0, 1000, 600, IdentifyOpacity);
                }
            }
        }

        private void RenderCursor()
        {
            if (BuiltInComponents.Host.ShowCursors)
            {
                lock (painting)
                {
                    g.DrawLine(new Pen(Color.Red, 3F), _MouseData.CursorPosition.X, _MouseData.CursorPosition.Y, _MouseData.CursorPosition.X + 5, _MouseData.CursorPosition.Y);
                    g.DrawLine(new Pen(Color.Red, 3F), _MouseData.CursorPosition.X, _MouseData.CursorPosition.Y, _MouseData.CursorPosition.X, _MouseData.CursorPosition.Y + 5);
                    g.DrawLine(new Pen(Color.Red, 3F), _MouseData.CursorPosition.X, _MouseData.CursorPosition.Y, _MouseData.CursorPosition.X + 12, _MouseData.CursorPosition.Y + 12);
                }
            }
        }

        private void TransitionEffect_ConfigureRenderingParams(renderingParams e)
        {
            // Reset transformation data
            g.ResetTransform();
            RenderingParam.Alpha = 1.0f;
            RenderingParam.TransitionActive = false;

            // Exit after resetting transition effects?
            if (e == null)
                return;

            // Apply any offset data
            g.Translate(e.Offset.X, e.Offset.Y);

            // Apply any rotation data
            if (e.Rotation.Length != 0)
                g.Rotate(new Math.Vector3((float)e.Rotation.X, (float)e.Rotation.Y, (float)e.Rotation.Z));

            // Apply any scale data
            if (e.Scale.X != 1 | e.Scale.Y != 1 | e.Scale.Y != 1)
                g.Scale(new Math.Vector3((float)e.Scale.X, (float)e.Scale.Y, (float)e.Scale.Z));

            // Apply transparency values (this is done via rendering parameters passed along to each control)
            RenderingParam.Alpha = e.Alpha;

            // Transition active?
            RenderingParam.TransitionActive = e.TransitionActive;
        }

        private bool RenderingError = false;
        protected void RenderPanels()
        {
            if (BlockRendering)
                return;

            //lock (painting)
            {
                // Reset rendering parameters 
                g.ResetTransform();

                try
                {
                    // Get a filtered list of panels to render
                    List<OMPanel> FilteredRenderingQueue = RenderingQueue.FindAll(x => (x.Mode != eModeType.Loaded && x.Mode != eModeType.Unloaded));

                    for (int i = 0; i < FilteredRenderingQueue.Count; i++)
                    {
                        // Configure rendering params based on panel mode
                        if (FilteredRenderingQueue[i].Mode == eModeType.transitioningIn)
                            // Render parameters for transition effect in
                            TransitionEffect_ConfigureRenderingParams(TransitionEffectParam_In);
                        else if (FilteredRenderingQueue[i].Mode == eModeType.transitioningOut)
                            // Render parameters for transition effect out
                            TransitionEffect_ConfigureRenderingParams(TransitionEffectParam_Out);
                        else
                            TransitionEffect_ConfigureRenderingParams(null);
                        //if (_RenderingReset)
                        //{
                        //    // Reset any transition effects
                        //    TransitionEffect_ConfigureRenderingParams(null);
                        //    _RenderingReset = false;
                        //}

                        if (FilteredRenderingQueue[i].Mode != eModeType.Loaded && FilteredRenderingQueue[i].Mode != eModeType.Unloaded)
                        {
                            FilteredRenderingQueue[i].Render(g, RenderingParam);

                            // Should we redraw the screen as fast as possible?
                            if (FilteredRenderingQueue[i].FastRendering)
                                _Refresh = true;
                        }
                    }

                    // Render any modal panel a second time on top of all others
                    OMPanel ModalPanel = GetModalPanel();
                    if (ModalPanel != null && ModalPanel.Mode != eModeType.Loaded && ModalPanel.Mode != eModeType.Unloaded)
                    {
                        ModalPanel.Render(g, RenderingParam);

                        // Should we redraw the screen as fast as possible?
                        if (ModalPanel.FastRendering)
                            _Refresh = true;
                    }

                    RenderingError = false;
                }
                catch (Exception e)
                {
                    if (!RenderingError)
                    {
                        RenderingError = true;
                        BuiltInComponents.Host.DebugMsg(String.Format("RenderingWindow.RenderPanels (Screen {0}) Exception:", _Screen), e);
                    }
                }
            }
        }

        private void RenderGesture()
        {
            if (BuiltInComponents.SystemSettings.ShowGestures)
            {
                if ((currentGesture != null) && (currentGesture.Count > 0))
                {
                    lock (painting)
                    {
                        Point[] GesturePoints = currentGesture.ToArray();
                        for (int i = 0; i < GesturePoints.Length; i++)
                            g.FillEllipse(new Brush(Color.Red), new Rectangle((GesturePoints[i].X - 5), (GesturePoints[i].Y - 5), 10, 10));
                        if (GesturePoints.Length > 1)
                            g.DrawLine(new Pen(Color.Red, 10F), GesturePoints);
                    }
                }
            }
        }

        #endregion

        private void RenderingWindow_Resize(object sender, EventArgs e)
        {
            raiseResizeEvent();

            // Also make other windows follow the state of the main window (maximize and minimize)
            if (this.Screen == 0)
                Core.theHost.SetAllWindowState(this.WindowState);
        }
        protected override void OnResize(EventArgs e)
        {
            ScaleFactors = new PointF((this.ClientRectangle.Width / 1000F), (this.ClientRectangle.Height / 600F));
            _ResizeRequired = true;
            RenderingWindow_Resize(null, e);
            base.OnResize(e);
        }
        protected override void OnWindowStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Fullscreen;
            if ((this.WindowState == WindowState.Fullscreen)) // && (!defaultMouse))
            {
                base.CursorVisible = false;
            }
            else
            {
                base.CursorVisible = true;
            }
            base.OnWindowStateChanged(e);

            // Stop rendering if window is minimized
            if (this.WindowState == WindowState.Minimized)
                this._StopRendering = true;
            else
            {
                this._StopRendering = false;
            }

            RenderingWindow_Resize(null, e);
        }

        void RenderingWindow_Gesture(object sender, OpenMobile.Graphics.TouchEventArgs e)
        {
            string gesture = String.Empty;
            if (e.GestureComplete)
                gesture = "End";
            gesture += e.Name + "|";
            if (e.Name == "Rotate")
                gesture += e.Arg1.ToString() + "|";
            else
                gesture = (e.Arg1 * AspectRatio).ToString("0.00") + "|";
            gesture += ((e.Position.X - this.X) * _ScaleFactors.X).ToString() + ",";
            gesture += ((e.Position.Y - this.Y) * _ScaleFactors.Y).ToString();
            Core.theHost.execute(eFunction.multiTouchGesture, _Screen.ToString(), gesture);
        }

        //void RenderingWindow_ResolutionChange(object sender, OpenMobile.Graphics.ResolutionChange e)
        //{
        //    try
        //    {
        //        DisplayDevice dev = DisplayDevice.AvailableDisplays[_Screen];
        //        if (e.Landscape != dev.Landscape)
        //            Core.theHost.raiseSystemEvent(eFunction.screenOrientationChanged, _Screen.ToString(), e.Landscape ? "Landscape" : "Portrait", String.Empty);
        //    }
        //    catch (Exception ex)
        //    {
        //        BuiltInComponents.Host.DebugMsg("RenderingWindow_ResolutionChange Exception", ex);
        //    }
        //}

        private void RenderingWindow_FormClosing(object sender, CancelEventArgs e)
        {
            try
            {
                if (_Screen == 0)
                    Core.theHost.execute(eFunction.closeProgram);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Indicates gesture recognition is active
        /// </summary>
        private bool Gesture_Active
        {
            get
            {
                return (currentGesture.Count > 0);
            }
        }

        private Point ScalePointToScreen(Point p)
        {
            p.Scale(_ScaleFactors.X, _ScaleFactors.Y);
            return p;
        }
        
        internal void RenderingWindow_MouseMove(object sender, MouseMoveEventArgs e)
        {
            if ((int)sender != _Screen)
                return;

            // Scale mouse data
            MouseMoveEventArgs eScaled = new MouseMoveEventArgs(e);
            MouseMoveEventArgs.Scale(eScaled, _ScaleFactors);

            // Save current cursor position
            _MouseData.CursorPosition = eScaled.Location;

            // Calculate mouse move distances
            CursorDistance = (MouseMoveStartPoint.ToVector2() - _MouseData.CursorPosition.ToVector2()).Length;
            CursorDistanceXYTotal = _MouseData.CursorPosition - MouseMoveStartPoint;
            CursorDistanceXYRelative = _MouseData.CursorPosition - CursorDistanceXYRelative;

            // Calculate mouse move speed (pixels since last execution)
            swCursorSpeedTiming.Stop();
            CursorSpeed = new PointF(
                (CursorDistanceXYRelative.X / (float)swCursorSpeedTiming.Elapsed.TotalMilliseconds) * CursorSpeedScaling.X,
                (CursorDistanceXYRelative.Y / (float)swCursorSpeedTiming.Elapsed.TotalMilliseconds) * CursorSpeedScaling.Y);
            swCursorSpeedTiming.Reset();
            swCursorSpeedTiming.Start();

            OMControl control = null;

            if (e.Buttons == MouseButton.Left)
            {   // Mouse moved with button pressed, this indicates a gesture or a throw

                bool EnableGesture = true;
                
                // Disable gesture if this control uses iThrow interface
                if ((FocusedControl != null && typeof(IThrow).IsInstanceOfType(FocusedControl)) || (FocusedControlParent != null && typeof(IThrow).IsInstanceOfType(FocusedControlParent)))
                    EnableGesture = false;

                // Disable gesture if this control uses iMouse interface
                if ((FocusedControl != null && typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControlParent != null && typeof(IMouse).IsInstanceOfType(FocusedControlParent)))
                    EnableGesture = false;

                // Ensure mouse is moved a certain distance before we start to gesture
                if (EnableGesture)
                    if (Gesture_Active || CursorDistance > 50)
                    {
                        // Add mouse position to gesture array
                        currentGesture.Add(_MouseData.CursorPosition);
                    }

                // Send iThrow interface data
                if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IThrow).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
                {   // Use focused control parent
                    if (FocusedControlParent != null && typeof(IThrow).IsInstanceOfType(FocusedControlParent) == true)
                    {
                        if (!ThrowActive)
                        {   // Start new throw
                            if (CursorDistance > 5)
                            {
                                bool cancel = false;
                                ((IThrow)FocusedControlParent).MouseThrowStart(_Screen, MouseMoveStartPoint, CursorSpeed, _ScaleFactors, ref cancel);
                                ThrowActive = !cancel;
                                UpdateControlFocus(FocusedControlParent, null, false);
                            }
                        }
                        else
                        {   // Throw started, update data for throw interface
                            ((IThrow)FocusedControlParent).MouseThrow(_Screen, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative, CursorSpeed);
                        }
                    }
                }
                else
                {   // Use focused control
                    if (FocusedControl != null && typeof(IThrow).IsInstanceOfType(FocusedControl) == true)
                    {
                        if (!ThrowActive)
                        {   // Start new throw
                            if (CursorDistance > 5)
                            {
                                bool cancel = false;
                                ((IThrow)FocusedControl).MouseThrowStart(_Screen, MouseMoveStartPoint, CursorSpeed, _ScaleFactors, ref cancel);
                                ThrowActive = !cancel;
                            }
                        }
                        else
                        {   // Throw started, update data for throw interface
                            ((IThrow)FocusedControl).MouseThrow(_Screen, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative, CursorSpeed);
                        }
                    }
                }
            }
            else
            {   // Regular mouse move action

                OMControl ParentControl = null;

                // Find control under mouse
                control = FindControlAtLocation(_MouseData.CursorPosition, out ParentControl);

                // Highlight/unhighlight the control
                UpdateControlFocus(control, ParentControl, true);
            }

            // Send mouse preview interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMousePreview).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControlParent))
                    {
                        ((IMousePreview)FocusedControlParent).MousePreviewMove(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
                    }
            }
            else
            {   // Send event to focused control
                // Mouse preview interface
                if (FocusedControl != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControl))
                        ((IMousePreview)FocusedControl).MousePreviewMove(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
            }

            // Send event data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                    {
                        ((IMouse)FocusedControlParent).MouseMove(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
                    }
            }
            else
            {   // Send event to focused control
                // Mouse interface
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                        ((IMouse)FocusedControl).MouseMove(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
            }

            // Update relative distance data
            CursorDistanceXYRelative = _MouseData.CursorPosition;
            
            // Redraw _Screen
            Invalidate();
        }

        internal void RenderingWindow_MouseDown(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != _Screen)
                return;

            // Scale mouse data
            MouseButtonEventArgs eScaled = new MouseButtonEventArgs(e);
            MouseButtonEventArgs.Scale(eScaled, _ScaleFactors);

            // Save mouse start point
            MouseMoveStartPoint = eScaled.Location;

            // Default value for mouse moved distances
            CursorDistance = 0;
            CursorDistanceXYTotal.X = 0;
            CursorDistanceXYTotal.Y = 0;
            CursorDistanceXYRelative = MouseMoveStartPoint;

            OMControl ParentControl = null;
            OMControl control = FocusedControl;

            // Verify that control under mouse is still the same
            OMControl controlAtMouseLocation = FindControlAtLocation(MouseMoveStartPoint, out ParentControl);

            // If nothing is selected then try to select something
            if (FocusedControl == null || controlAtMouseLocation != control)
            {
                // Find control under mouse
                control = FindControlAtLocation(MouseMoveStartPoint, out ParentControl);

                // Highlight/unhighlight the control
                UpdateControlFocus(control, ParentControl, true);
            }

            // Send Mouse preview interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMousePreview).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControlParent))
                    {
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMousePreview)FocusedControlParent).MousePreviewDown(_Screen, eScaled, MouseMoveStartPoint);
                            });
                    }
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControl))
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMousePreview)FocusedControl).MousePreviewDown(_Screen, eScaled, MouseMoveStartPoint);
                            });
            }

            // No use in doing anything if nothing is focused
            if (FocusedControl == null)
                return;

            // Set current control as clicked
            FocusedControl.Mode = eModeType.Clicked;

            // Can this control accept clicks?
            if (IsControlClickable(FocusedControl))
            {   // Reset and start click timing 
                swClickTiming.Reset();
                swClickTiming.Start();

                // Enable hold detection
                tmrClickHold.Enabled = true;
                tmrClickHold.Tag = eScaled;
            }

            // Send Mouse interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                    {
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMouse)FocusedControlParent).MouseDown(_Screen, eScaled, MouseMoveStartPoint);
                            });
                    }
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMouse)FocusedControl).MouseDown(_Screen, eScaled, MouseMoveStartPoint);
                            });
            }

            // Redraw
            Invalidate();
        }

        internal void RenderingWindow_MouseUp(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != _Screen)
            {
                return;
            }

            // Scale mouse data
            MouseButtonEventArgs eScaled = new MouseButtonEventArgs(e);
            MouseButtonEventArgs.Scale(eScaled, _ScaleFactors);

            // Stop click timing
            swClickTiming.Stop();
            tmrClickHold.Enabled = false;

            // Handle gesture
            bool GestureHandled = HandleGesture();
            
            // No use in checking click if no control is focused
            if (FocusedControl != null)
            {
                // Reset clicked state on focused control
                FocusedControl.Mode = eModeType.Highlighted;
            }
            if (FocusedControlParent != null)
            {
                // Reset clicked state on focused control
                FocusedControlParent.Mode = eModeType.Highlighted;
            }
            
            // Redraw
            Invalidate();

            // Check click type
            ClickTypes clickType = ClickTypes.Normal;
            if (swClickTiming.ElapsedMilliseconds > 1)
            {
                // Determine type of click (click or long click)
                if (swClickTiming.ElapsedMilliseconds < 500)
                {   // Normal click
                    clickType = ClickTypes.Normal;
                }
                else
                {   // Long click
                    clickType = ClickTypes.Long;
                }
            }

            // Send Mouse preview interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMousePreview).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControlParent))
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMousePreview)FocusedControlParent).MousePreviewUp(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, clickType);
                            });
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMousePreview).IsInstanceOfType(FocusedControl))
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMousePreview)FocusedControl).MousePreviewUp(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, clickType);
                            });
            }

            // Return if gesture is handled or no control has focus
            if (GestureHandled || FocusedControl == null)
            {
                // Find control under mouse (if any)
                OMControl ParentControl = null;
                OMControl control = FindControlAtLocation(_MouseData.CursorPosition, out ParentControl);

                // Highlight/unhighlight the control
                UpdateControlFocus(control, ParentControl, true);

                // Redraw
                Invalidate();

                return;
            }

            // Click filter time
            if (swClickTiming.ElapsedMilliseconds > 1)
            {
                // Determine type of click (click or long click)
                if (swClickTiming.ElapsedMilliseconds < 500)
                {   // Normal click
                    ActivateClick(FocusedControl, ClickTypes.Normal, e);
                }
                else
                {   // Long click
                    ActivateClick(FocusedControl, ClickTypes.Long, e);
                }
            }

            // Send Mouse interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                        OpenMobile.Threading.SafeThread.Asynchronous(() =>
                            {
                                ((IMouse)FocusedControlParent).MouseUp(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal);
                            });
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                              OpenMobile.Threading.SafeThread.Asynchronous(() =>
                                 {
                                    ((IMouse)FocusedControl).MouseUp(_Screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal);
                                 });
            }

            // Send Throw interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IThrow).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (ThrowActive)
                {
                    if (FocusedControlParent != null)
                        if (typeof(IThrow).IsInstanceOfType(FocusedControlParent))
                        {
                            OpenMobile.Threading.SafeThread.Asynchronous(() =>
                                {
                                    ((IThrow)FocusedControlParent).MouseThrowEnd(_Screen, MouseMoveStartPoint, CursorDistanceXYTotal, eScaled.Location, CursorSpeed);
                                }
                            );
                        }
                }
            }
            else
            {   // Send event to focused control 
                if (ThrowActive)
                {
                    if (FocusedControl != null)
                        if (typeof(IThrow).IsInstanceOfType(FocusedControl))
                        {
                             OpenMobile.Threading.SafeThread.Asynchronous(() =>
                                 {
                                    ((IThrow)FocusedControl).MouseThrowEnd(_Screen, MouseMoveStartPoint, CursorDistanceXYTotal, eScaled.Location, CursorSpeed);
                                 }
                             );
                       }
                }
            }

            // Redraw
            Invalidate();

            // Default value for mouse moved distances
            CursorDistance = 0;
            CursorDistanceXYTotal.X = 0;
            CursorDistanceXYTotal.Y = 0;
            CursorDistanceXYRelative = MouseMoveStartPoint;
            ThrowActive = false;
            swCursorSpeedTiming.Stop();
            swCursorSpeedTiming.Reset();
        }

        private void RenderingWindow_MouseLeave(object sender, EventArgs e)
        {
            UpdateControlFocus(null, null, true);
        }

        public void RenderingWindow_KeyDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (e.Screen != Screen)
                return;

            // iKey interface (if key is handled by controls then it will not be handled by the rendering window
            OMControl iKeyControl = FocusedControl;
            if (iKeyControl != null)
                if (typeof(IKey).IsInstanceOfType(iKeyControl))
                    if (((IKey)iKeyControl).KeyDown_BeforeUI(_Screen, e, _ScaleFactors))
                    {
                        Invalidate();
                        return;
                    }

            // Handle arrow keys to move focus around
            if ((e.Key == Key.Left) || (e.Key == Key.Right) || (e.Key == Key.Up) || (e.Key == Key.Down))
            {
                if (FocusedControl == null)
                {   // Select the first available control that can receive focus
                    OMControl control = FindFirstFocusableControl();
                    UpdateControlFocus(control, null, true);
                    Invalidate();
                }
                else
                {   // Goto to next control in the requested direction
                    OMControl control = null;
                    if (e.Key == Key.Right)
                        control = FindFirstFocusableControlInDirection(SearchDirections.Right, 0.1F);
                    else if (e.Key == Key.Left)
                        control = FindFirstFocusableControlInDirection(SearchDirections.Left, 0.1F);
                    else if (e.Key == Key.Up)
                        control = FindFirstFocusableControlInDirection(SearchDirections.Up, 0.1F);
                    else if (e.Key == Key.Down)
                        control = FindFirstFocusableControlInDirection(SearchDirections.Down, 0.1F);
                    if (control != null)
                        UpdateControlFocus(control, null, true);
                }
            }

            // iKey interface
            if (iKeyControl != null)
                if (typeof(IKey).IsInstanceOfType(iKeyControl))
                    ((IKey)iKeyControl).KeyDown_AfterUI(_Screen, e, _ScaleFactors);

            Invalidate();
        }

        public void RenderingWindow_KeyUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (e.Screen != Screen)
                return;

            // iKey interface (if key is handled by controls then it will not be handled by the rendering window
            OMControl iKeyControl = FocusedControl;
            if (iKeyControl != null)
                if (typeof(IKey).IsInstanceOfType(iKeyControl))
                    if (((IKey)iKeyControl).KeyUp_BeforeUI(_Screen, e, _ScaleFactors))
                    {
                        Invalidate();
                        return;
                    }

            // Exit fullscreen or close program
            if (e.Key == Key.Escape)
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                {
                    if (_Screen == 0)
                        Core.theHost.execute(eFunction.closeProgram);
                    else
                        CloseMe();
                }
            }
            else if (e.Key == Key.Enter)
            {
                // Only active if we have a focused control
                if (FocusedControl != null)
                {
                    if (IsControlClickable(FocusedControl))
                    {
                        // Check for normal enter (click), shift enter (long click) or ctrl enter (hold click)
                        if (e.Shift == true && e.Control == false)
                        {   // Long click
                            ActivateClick(FocusedControl, ClickTypes.Long, null);
                        }
                        else if (e.Shift == true && e.Control == true)
                        {   // Hold click
                            ActivateClick(FocusedControl, ClickTypes.Hold, null);
                        }
                        else
                        {   // Normal click
                            ActivateClick(FocusedControl, ClickTypes.Normal, null);
                        }
                    }
                }
            }

            // iKey interface
            if (iKeyControl != null)
                if (typeof(IKey).IsInstanceOfType(iKeyControl))
                    ((IKey)iKeyControl).KeyUp_AfterUI(_Screen, e, _ScaleFactors);

            Invalidate();
        }

        private OMPanel lastPanelTransition = null;

        public void ExecuteTransition(string transType, float transSpeed)
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                List<OMPanel> panels = RenderingQueue.FindAll(x => ((x.Mode == eModeType.Loaded) || (x.Mode == eModeType.transitioningIn) || (x.Mode == eModeType.transitioningOut)));

                // If only one panel is transitioned in then we can use the transition effect for the panel (of any) if not use the requested effect
                if (panels.Count == 1)
                {
                    if (panels[0].Mode == eModeType.transitioningIn || panels[0].Mode == eModeType.Loaded)
                        transType = panels[0].TransitionEffect_Show.ToString();
                    if (panels[0].Mode == eModeType.transitioningOut || panels[0].Mode == eModeType.Unloaded)
                        transType = panels[0].TransitionEffect_Hide.ToString();
                }

                // Reset transition effects parameters
                TransitionEffectParam_In = new renderingParams();
                TransitionEffectParam_Out = new renderingParams();

                // Get transition effect
                iPanelTransitionEffect TransitionEffect = PanelTransitionEffectHandler.GetEffect(transType);

                // Get initial transition effect values
                TransitionEffect.SetInitialTransitionEffects(TransitionEffectParam_In, TransitionEffectParam_Out);
                
                // Ensure panels are transitioned in if setting the mode was forgotten in the effect
                foreach (OMPanel panel in panels)
                {
                    if (panel.Mode == eModeType.Loaded)
                    {   // Panel is loaded, set to transition in state
                        panel.Mode = eModeType.transitioningIn;
                    }
                }

                TransitionEffectParam_In.TransitionActive = true;
                TransitionEffectParam_Out.TransitionActive = true;

                // Execute effect
                TransitionEffect.Run(
                    TransitionEffectParam_In, 
                    TransitionEffectParam_Out, 
                    ReDrawPanel, 
                    (transSpeed > 0 ? transSpeed : BuiltInComponents.SystemSettings.TransitionSpeed));

                TransitionEffectParam_In.TransitionActive = false;
                TransitionEffectParam_Out.TransitionActive = false;
                _RenderingReset = true;

                // Set all panels that's transitioned out as unloaded
                List<OMPanel> panelsTransOut = RenderingQueue.FindAll(x => (x.Mode == eModeType.transitioningOut));
                panelsTransOut.ForEach(delegate(OMPanel panel) { panel.Mode = eModeType.Unloaded; });            

                // Go trough each panel to set correct modes after transition effects
                lock (RenderingQueue)
                {
                    panels = panels.OrderByDescending(x => x.Mode).ToList();
                    foreach (OMPanel panel in panels)
                    {
                        // Stop rendering in each control in the panel (if applicable)
                        for (int i = 0; i < panel.Controls.Count; i++)
                            panel.Controls[i].RenderStop(g,RenderingParam);

                        if (panel.Mode == eModeType.transitioningIn)
                        {   // Panel is transitioning in, set to normal state
                            panel.Mode = eModeType.Normal;

                            // Raise event for entering panel
                            ((iPanelEvents)panel).RaiseEvent(_Screen, eEventType.Entering);

                            // Show infobar text (if present)
                            if (!String.IsNullOrEmpty(panel.Header))
                            {
                                OM.Host.UIHandler.InfoBar_Show(_Screen, new InfoBar(panel.Header, panel.Icon));
                                lastPanelTransition = panel;
                            }
                        }
                        else if (panel.Mode == eModeType.transitioningOut | panel.Mode == eModeType.Unloaded)
                        {   // Panel is transitioning out, remove from rendering queue
                            RenderingQueue.Remove(panel);
                            // Unhook refresh event
                            panel.UpdateThisControl -= UpdateThisControl;

                            // Raise event for leaving panel
                            ((iPanelEvents)panel).RaiseEvent(_Screen, eEventType.Leaving);

                            // Remove infobar text
                            if (!String.IsNullOrEmpty(panel.Header) && (lastPanelTransition == panel))
                                OM.Host.UIHandler.InfoBar_Hide(_Screen);
                        }
                    }
                }
                ReDrawPanel();
            }
        }

        public bool TransitionInPanel(OMPanel newP)
        {
            lock (this) // Lock to prevent multiple transitions at the same time
            {
                // Remove startup panel
                if (RenderingQueue.Contains(panelStartUp))
                    RenderingQueue.Remove(panelStartUp);

                OMPanel ExistingPanel = RenderingQueue.Find(x => x == newP);
                if (ExistingPanel == null)
                {
                    // Attach _Screen update event to new panel
                    newP.UpdateThisControl += UpdateThisControl;

                    // Unfocus currently focused control
                    UpdateControlFocus(null, null, true);

                    // Mark this panel as transitionin in
                    newP.Mode = eModeType.Loaded;

                    // Add new panel
                    insertPanel(newP);

                    // Raise panel event
                    ((iPanelEvents)newP).RaiseEvent(_Screen, eEventType.Loaded);

                    return true;
                }
                else
                {   // Reset existing panel to normal mode if it's already loaded
                    ExistingPanel.Mode = eModeType.Normal;
                    return false;
                }
            }
        }

        public bool TransitionOutPanel(OMPanel oldP)
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                // Is this panel loaded? If not cancel request
                if (!RenderingQueue.Contains(oldP))
                    return false;

                // Unfocus currently focused control
                UpdateControlFocus(null, null, true);

                // Mark this panel as transitioning out
                oldP.Mode = eModeType.transitioningOut;

                // Raise panel event
                ((iPanelEvents)oldP).RaiseEvent(_Screen, eEventType.Unloaded);

                return true;
            }
        }

        public bool TransitionOutEverything()
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                if (blockHome)
                    return false;

                // Unfocus currently focused control
                UpdateControlFocus(null, null, true);

                for (int i = RenderingQueue.Count - 1; i >= 0; i--)
                {
                    if ((RenderingQueue[i].Mode == eModeType.transitioningIn) || (RenderingQueue[i].UIPanel))
                        RenderingQueue[i].Mode = eModeType.Normal;
                    else
                    {
                        RenderingQueue[i].Mode = eModeType.transitioningOut;

                        // Raise panel event
                        ((iPanelEvents)RenderingQueue[i]).RaiseEvent(_Screen, eEventType.Unloaded);
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Closes this _Screen with animation
        /// </summary>
        public void CloseMe()
        {
            //SandboxedThread.Asynchronous(delegate() { tmrClosing_Tick(null, null); });
            this.Exit();
        }

        /// <summary>
        /// [RENAME TO CloseAll] Closes all screens with animation
        /// </summary>
        public static void CloseRenderer()
        {
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
                Core.RenderingWindows[i].CloseMe();
        }

        /// <summary>
        /// Fades the _Screen to black
        /// </summary>
        public void FadeOut()
        {
            for (dimmer = 1; dimmer < 250; dimmer += 5)
            {
                Invalidate();
                Thread.Sleep(30);
            }
            dimmer = 255;
            Invalidate();
            SandboxedThread.Asynchronous(delegate() { Thread.Sleep(1000); dimmer = 0; });
        }

        /// <summary>
        /// Requests a redraw of the _Screen
        /// </summary>
        private void Invalidate()
        {
            _Refresh = true;
        }

        /// <summary>
        /// Hookable method to request a redraw of the _Screen
        /// </summary>
        /// <param name="resetHighlighted"></param>
        public void UpdateThisControl(bool resetHighlighted)
        {
            Invalidate();
            //if (resetHighlighted)
            //    MouseMove();
        }

        #region Paint Identity

        public void PaintIdentity()
        {
            Identify = true;
            IdentifyOpacity = 255;
            Invalidate();
            Thread.Sleep(1500);
            Identify = false;
            Invalidate();
        }
        public void PaintIdentity(bool Show)
        {
            Identify = Show;
            IdentifyOpacity = 255;
            Invalidate();
        }
        public void PaintIdentity(bool Show, float Opacity)
        {
            Identify = Show;
            IdentifyOpacity = Opacity;
            Invalidate();
        }

        #endregion


        /// <summary>
        /// Raises the resize event to the framework
        /// </summary>
        private void raiseResizeEvent()
        {
            SandboxedThread.Asynchronous(delegate()
                {
                    Core.theHost.raiseSystemEvent(eFunction.RenderingWindowResized, _Screen.ToString(), this.Location.ToOpenMobilePoint(), this.Size.ToOpenMobileSize(), this.ScaleFactors);
                }
            );
        }

        /// <summary>
        /// Inserts a panel into the rendering queue according to panel priority
        /// </summary>
        /// <param name="newP"></param>
        private void insertPanel(OMPanel newP)
        {
            for (int i = RenderingQueue.Count - 1; i >= 0; i--)
            {
                if (RenderingQueue[i].Priority <= newP.Priority)
                {
                    if (newP.Priority == ePriority.UI)
                    {
                        if (RenderingQueue[i].Priority == ePriority.UI)
                            RenderingQueue.Insert(i, newP);
                        else
                            RenderingQueue.Insert(i + 1, newP);
                        return;
                    }
                    else if (newP.PanelType == OMPanel.PanelTypes.Modal)
                    {
                        if (RenderingQueue[i].PanelType == OMPanel.PanelTypes.Modal || RenderingQueue[i].Priority == ePriority.UI)
                            RenderingQueue.Insert(i, newP);
                        else
                            RenderingQueue.Insert(i + 1, newP);
                        return;
                    }

                    else
                    {
                        RenderingQueue.Insert(i + 1, newP);
                        return;
                    }
                }
            }
            RenderingQueue.Insert(0, newP);
        }

        /// <summary>
        /// Sets or unsets the currently focused control
        /// </summary>
        /// <param name="control"></param>
        private void UpdateControlFocus(OMControl control, OMControl controlParent, bool Reset)
        {
            // Reset any click actions
            if (Reset)
                ResetForm();

            // Remove highlight from previously highlighted control
            if (FocusedControl != null && FocusedControl != control)
            {
                FocusedControl.Mode = eModeType.Normal;
                FocusedControl = null;
            }
            if (FocusedControlParent != null && FocusedControlParent != control)
            {
                FocusedControlParent.Mode = eModeType.Normal;
                FocusedControlParent = null;
            }
            
            // Remove any debug info
            if (MouseOverControl != null && MouseOverControl != control)
            {
                MouseOverControl.SkinDebug = false;
                MouseOverControl = null;
            }

            if (control != null)
            {
                FocusedControlParent = controlParent;

                // Only set focus to highlightable controls
                if (IsControlFocusable(control))
                {
                    // highlight this control
                    FocusedControl = control;

                    // Set focused control's mode as highlighted
                    if (FocusedControl != null)
                        FocusedControl.Mode = eModeType.Highlighted;
                }

                // Only set focus to highlightable controls
                if (IsControlFocusable(FocusedControlParent))
                {
                    // Set focused control's mode as highlighted
                    if (FocusedControlParent != null)
                        FocusedControlParent.Mode = eModeType.Highlighted;
                }
            }
            else
            {
                FocusedControl = null;
                FocusedControlParent = null;
            }

            // Additional actions on control
            if (control != null)
            {
                // Show debug info?
                if (BuiltInComponents.Host.ShowDebugInfo)
                {
                    MouseOverControl = control;
                    MouseOverControl.SkinDebug = true;
                }
            }
        }

        private void ResetControl(OMControl control)
        {
            if (control != null)
            {
                control.Mode = eModeType.Normal;
                control = null;
            }
        }

        /// <summary>
        /// Checks if a control in this panel has a hit at the given location
        /// </summary>
        /// <param name="Location"></param>
        /// <returns></returns>
        private OMControl PanelHitTest(OMPanel panel, Point Location, out OMControl ParentControl)
        {
            // Check this panel for hit
            OMControl control = null;
            ParentControl = null;
            for (int i = panel.controlCount - 1; i >= 0; i--)
            {
                control = panel.Controls[i];
                if (typeof(IContainer2).IsInstanceOfType(control) && control.IsControlRenderable())
                {
                    OMControl ContainerControl = null;
                    for (int i2 = ((IContainer2)control).Controls.Count - 1; i2 >= 0; i2--)
                    {
                        ContainerControl = ((IContainer2)control).Controls[i2];
                        if (control.Region.Contains(Location))
                        {
                            if (ControlHitTest(ContainerControl, Location))
                            {
                                ParentControl = control;
                                return ContainerControl;
                            }
                        }
                    }
                    if (ControlHitTest(control, Location))
                        return control;
                }
                else
                {
                    if (ControlHitTest(control, Location))
                        return control;
                }
            }

            // No hit on this panel, return null
            return null;
        }

        /// <summary>
        /// Checks if this control has a hit at the given location
        /// </summary>
        /// <param name="Location"></param>
        /// <returns></returns>
        public virtual bool ControlHitTest(OMControl control, Point Location)
        {
            // Make sure this control is inside the application area
            if (!ApplicationArea.ToSystemRectangle().IntersectsWith(control.Region.ToSystemRectangle()))
                return false;

            // Make sure this control is visible
            if (!control.IsControlRenderable())
                return false;

            // Ensure control allows user interaction
            if (control.NoUserInteraction)
                return false;

            // Return false if this is not inside this controls region
            if (!control.Region.Contains(Location))
                return false;

            // Check if control overrides the hit test 
            if (typeof(INotClickable).IsInstanceOfType(control))
                if (((INotClickable)control).IsPointClickable(Location.X, Location.Y))
                    return true;
                else
                    return false;

            // This is inside this controls region
            return true;
        }

        /// <summary>
        /// Returns a OMPanel if a modal panel is loaded, if not returns null
        /// </summary>
        /// <returns></returns>
        private OMPanel GetModalPanel()
        {
            return RenderingQueue.Find(x => x.PanelType == OMPanel.PanelTypes.Modal);
        }

        /// <summary>
        /// Tries to find a control at the given location
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="ParentControl">Contains null if the current control has no parent control, otherwise holds a referance to the parent control</param>
        /// <returns>The control if found, null if no control is found</returns>
        private OMControl FindControlAtLocation(Point Location, out OMControl ParentControl)
        {
            // Loop trough controls checking for hit test, starting at the end of the rendering queue (topmost item)
            OMControl control = null;
            ParentControl = null;

            // Check for any modal panels present, if so check only this
            OMPanel ModalPanel = GetModalPanel();

            if (ModalPanel == null)
            {   // Check all panels
                for (int i = RenderingQueue.Count - 1; i >= 0; i--)
                {
                    control = PanelHitTest(RenderingQueue[i], Location, out ParentControl);
                    // Did we hit something?
                    if (control != null)
                    {   // Yes
                        return control;
                    }
                }
            }
            else
            {   // Check the modal panel only
                control = PanelHitTest(ModalPanel, Location, out ParentControl);
                // Did we hit something?
                if (control != null)
                {   // Yes
                    return control;
                }
            }

            return null;
        }


        /// <summary>
        /// Checks if the control is fully or partly covered by other controls
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        private bool IsControlCoveredByOthers(OMControl control)
        {
            // Find panel that contains the control and use this as a starting point
            int panelIndex = -1;
            for (int i = RenderingQueue.Count - 1; i >= 0; i--)
            {
                if (RenderingQueue[i].contains(control))
                {
                    panelIndex = i;
                    break;
                }
            }

            // Loop trough rendering queue, starting from the control going outwards
            System.Drawing.Rectangle controlRect = control.Region.ToSystemRectangle();
            for (int i = panelIndex; i < RenderingQueue.Count; i++)
            {
                for (int i2 = 0; i2 < RenderingQueue[i].Controls.Count; i2++)
                {
                    if (RenderingQueue[i].Controls[i2] != control)
                    {
                        if (RenderingQueue[i].Controls[i2].IsControlRenderable(false))
                        {
                            // Check if control in panel intersects with control from input param
                            if (RenderingQueue[i].Controls[i2].Region.ToSystemRectangle().IntersectsWith(controlRect))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private bool IsControlFocusable(OMControl control)
        {
            if (control == null)
                return false;

            return (control.IsControlRenderable() && !control.NoUserInteraction && typeof(IHighlightable).IsInstanceOfType(control) && ApplicationArea.Contains(control.Region));
        }

        private bool IsControlClickable(OMControl control)
        {
            return typeof(IClickable).IsInstanceOfType(control) | typeof(IClickableAdvanced).IsInstanceOfType(control);
        }

        /// <summary>
        /// Finds the upper left control that can receive focus
        /// </summary>
        /// <returns></returns>
        private OMControl FindFirstFocusableControl()
        {
            // Loop trough controls checking against a search region that starts small in the upper left corner then increases if no control is found
            System.Drawing.Rectangle SearchRect = new System.Drawing.Rectangle(0,0,50,30);
            for (int SearchStep = 1; SearchStep < 21; SearchStep++)
            {
                SearchRect.Width = SearchStep * 50;
                SearchRect.Height = SearchStep * 30;

                OMControl control = null;

                // Do we have a modal panel?
                OMPanel ModalPanel = GetModalPanel();

                // Loop trough panels
                if (ModalPanel == null)
                {   // Check all panels
                    for (int i = RenderingQueue.Count - 1; i >= 0; i--)
                    {   // Loop trough controls in the panel
                        for (int i2 = RenderingQueue[i].controlCount - 1; i2 >= 0; i2--)
                        {
                            control = RenderingQueue[i][i2];
                            if (IsControlFocusable(control))
                            {
                                System.Drawing.Rectangle region = control.Region.ToSystemRectangle();
                                if (region.IntersectsWith(SearchRect))
                                    return control;
                            }
                        }
                    }
                }
                else
                {   // Loop trough controls in the panel
                    for (int i2 = ModalPanel.controlCount - 1; i2 >= 0; i2--)
                    {
                        control = ModalPanel[i2];
                        if (IsControlFocusable(control))
                        {
                            System.Drawing.Rectangle region = control.Region.ToSystemRectangle();
                            if (region.IntersectsWith(SearchRect))
                                return control;
                        }
                    }
                }
            }
            return null;
        }


        private enum SearchDirections { Left, Right, Up, Down }
        private OMControl FindFirstFocusableControlInDirection(SearchDirections SearchDirection, float Tolerance)
        {
            int ToleranceX = (int)(ApplicationArea.Width * Tolerance);
            int ToleranceY = (int)(ApplicationArea.Height * Tolerance);
            OMControl bestControl = null;
            for (int SearchMode = 0; SearchMode < 2; SearchMode++)
            {
                OMControl control = null;
                float BestDistance = float.MaxValue;
                OpenTK.Vector2 FocusedControlLocation = FocusedControl.Region.Center.ToVector2();

                // Do we have a modal panel?
                OMPanel ModalPanel = GetModalPanel();
                OMPanel PanelToCheck = null;

                for (int i = RenderingQueue.Count - 1; i >= 0; i--)
                {
                    // Set panel to check
                    if (ModalPanel == null)
                        PanelToCheck = RenderingQueue[i];
                    else
                        PanelToCheck = ModalPanel;

                    for (int i2 = PanelToCheck.controlCount - 1; i2 >= 0; i2--)
                    {
                        // Find base control
                        control = PanelToCheck[i2];

                        // Create a control to list to use for checking
                        List<OMControl> Controls = new List<OMControl>();
                        Controls.Add(control);

                        // Add sub controls to list to check
                        if (typeof(IContainer2).IsInstanceOfType(control) && control.IsControlRenderable())
                            for (int i3 = ((IContainer2)control).Controls.Count - 1; i3 >= 0; i3--)
                                Controls.Add(((IContainer2)control).Controls[i3]);

                        // loop trough controls
                        for (int i3 = 0; i3 < Controls.Count; i3++)
                        {
                            control = Controls[i3];

                            // no use in testing against ourself
                            if (control == FocusedControl)
                                continue;

                            // Ensure we move in the requested direction
                            switch (SearchDirection)
                            {
                                case SearchDirections.Left:
                                    if (control.Region.Center.X >= FocusedControl.Region.Center.X)
                                        continue;
                                    break;
                                case SearchDirections.Right:
                                    if (control.Region.Center.X <= FocusedControl.Region.Center.X)
                                        continue;
                                    break;
                                case SearchDirections.Up:
                                    if (control.Region.Center.Y >= FocusedControl.Region.Center.Y)
                                        continue;
                                    break;
                                case SearchDirections.Down:
                                    if (control.Region.Center.Y <= FocusedControl.Region.Center.Y)
                                        continue;
                                    break;
                                default:
                                    break;
                            }

                            switch (SearchMode)
                            {
                                // Limit search to controls on the same height
                                case 0:
                                    {
                                        switch (SearchDirection)
                                        {
                                            case SearchDirections.Left:
                                            case SearchDirections.Right:
                                                if (control.Region.Center.Y < (FocusedControl.Region.Center.Y - ToleranceY))
                                                    continue;
                                                if (control.Region.Center.Y > (FocusedControl.Region.Center.Y + ToleranceY))
                                                    continue;
                                                break;
                                            case SearchDirections.Up:
                                            case SearchDirections.Down:
                                                if (control.Region.Center.X < (FocusedControl.Region.Center.X - ToleranceX))
                                                    continue;
                                                if (control.Region.Center.X > (FocusedControl.Region.Center.X + ToleranceX))
                                                    continue;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }

                            // Only check against controls that can receive focus
                            if (!IsControlFocusable(control))
                                continue;

                            // Calculate distance between the focused control and the next one
                            float Distance = (FocusedControlLocation - control.Region.Center.ToVector2()).Length;
                            if (Distance < BestDistance)
                            {
                                BestDistance = Distance;
                                bestControl = control;
                            }
                        }
                    }

                    // Exit if we only has a modal panel
                    if (ModalPanel != null)
                        break;
                }

                if (bestControl != null)
                    break;
            }
            return bestControl;
        }

        private void ActivateClick(OMControl control, ClickTypes ClickType, MouseButtonEventArgs e)
        {
            // Cancel if no control is provided
            if (control == null)
                return;

            // If no mouse data is available we'll simulate it (as indicated by the negative mouse location)
            if (e == null)
                e = new MouseButtonEventArgs(-1, -1, MouseButton.Left, true);

            // Scale mouse data
            MouseButtonEventArgs eScaled = new MouseButtonEventArgs(e);
            //MouseButtonEventArgs.Scale(eScaled, _ScaleFactors);

            // Lock the currently focused control
            lock (FocusedControl)
            {
                // Cancel if control is not clickable
                if (!typeof(IClickable).IsInstanceOfType(control) && !typeof(IClickableAdvanced).IsInstanceOfType(control))
                    return;

                Debug.WriteLine(string.Format("Click {0} on {1} activated", ClickType, control));

                switch (ClickType)
                {
                    case ClickTypes.None:
                        break;
                    case ClickTypes.Normal:
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                if (typeof(IClickableAdvanced).IsInstanceOfType(control))
                                    ((IClickableAdvanced)control).clickMe(_Screen, eScaled);
                                else
                                    ((IClickable)control).clickMe(_Screen);
                            });
                        }
                        break;
                    case ClickTypes.Long:
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                if (typeof(IClickableAdvanced).IsInstanceOfType(control))
                                    ((IClickableAdvanced)control).longClickMe(_Screen, eScaled);
                                else
                                    ((IClickable)control).longClickMe(_Screen);
                            });
                        }
                        break;
                    case ClickTypes.Hold:
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                if (typeof(IClickableAdvanced).IsInstanceOfType(control))
                                    ((IClickableAdvanced)control).holdClickMe(_Screen, eScaled);
                                else
                                    ((IClickable)control).holdClickMe(_Screen);
                            });
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        private void ResetForm()
        {
            // Stop click timing
            swClickTiming.Stop();
            tmrClickHold.Enabled = false;

            // Reset clicked state on focused control
            if (FocusedControl != null)
                FocusedControl.Mode = eModeType.Highlighted;

            // Clear any gestures
            currentGesture.Clear();

            // Reset mouse move points
            //MouseMoveStartPoint = new Point();

            // Default value for mouse moved distances
            CursorDistance = 0;
            CursorDistanceXYTotal.X = 0;
            CursorDistanceXYTotal.Y = 0;
            CursorDistanceXYRelative = new Point();
            ThrowActive = false;
            swCursorSpeedTiming.Stop();
            swCursorSpeedTiming.Reset();

            // Redraw
            Invalidate();
        }

        private bool HandleGesture()
        {
            // Handle gesture
            if (currentGesture.Count > 0)
            {
                Recognizer rec = new Recognizer();
                rec.Initialize();
                foreach (Point p in currentGesture)
                    rec.AddPoint(p, false);
                string s = rec.Recognize();
                if (!string.IsNullOrEmpty(s))
                    Core.theHost.raiseGestureEvent(_Screen, rec.Recognize(), RenderingQueue[RenderingQueue.Count - 1], FocusedControl);
                currentGesture.Clear();
                return true;
            }
            return false;
        }

        ///// <summary>
        ///// Returns the control to use for events (Selects between FocusedControl and FocusedControlParent)
        ///// </summary>
        ///// <returns></returns>
        //private OMControl GetControlForEvents()
        //{
        //    if (FocusedControlParent != null)
        //        return FocusedControlParent;
        //    else
        //        return FocusedControl;
        //}

        //private void GetWindowHandle()
        //{
        //    //IWindowInfo ii = ((OpenTK.NativeWindow)this).WindowInfo;
        //    //object inf = ((OpenTK.NativeWindow)this).WindowInfo;
        //    //PropertyInfo pi = (inf.GetType()).GetProperty("WindowHandle");
        //    //IntPtr hnd = ((IntPtr)pi.GetValue(ii, null));
        //}


        #region iRenderingWindow interface

        bool iRenderingWindow.IsControlCoveredByOthers(OMControl Control)
        {
            return IsControlCoveredByOthers(Control);
        }

        #endregion
    }
}
