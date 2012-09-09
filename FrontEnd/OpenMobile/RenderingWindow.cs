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

namespace OpenMobile
{
    public class RenderingWindow : GameWindow
    {

        /// <summary>
        /// Current screen dimming value
        /// </summary>
        private int dimmer;

        bool Identify;
        float IdentifyOpacity = 1f;
        Graphics.Graphics g;
        Point CursorPosition = new Point();
        float CursorDistance = 0f;
        Point CursorDistanceXYTotal = new Point();
        Point CursorDistanceXYRelative = new Point();
        bool keyboardActive;
        
        /// <summary>
        /// Contains the currently focused control's parent object (if any)
        /// <para>The control is only set if a control implements certain interfaces (like iContainer)</para>
        /// </summary>
        OMControl FocusedControlParent = null;
        OMControl FocusedControl = null;
        OMControl MouseOverControl = null;
        private List<Point> currentGesture = new List<Point>();
        bool ThrowActive = false;

        Timer tmrClickHold = new Timer(500);
        Stopwatch swClickTiming = new Stopwatch();

        Timer tmrMeasureFPS = new Timer(100);
        bool MeasureFPS_Done = false;

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
        /// [REMOVE] Indicates that this screen uses the default mouse
        /// </summary>
        public bool defaultMouse { get; set; }

        /// <summary>
        /// [REMOVE] Indicates that video playback is currently active on this screen
        /// </summary>
        public bool VideoPlaying { get; set; }

        /// <summary>
        /// Currently active mouse device for this screen
        /// </summary>
        public MouseDevice currentMouse { get; set; }

        /// <summary>
        /// [REPLACE WITH TOPMOST CONTROLS INSTEAD] Blocks the functionallity of TransitionOutEverything
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
                return screen;
            }
        }

        private PointF _ScaleFactors = new PointF(1,1);
        /// <summary>
        /// The scale factors for the screen
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
                    if (typeof(IContainer2).IsInstanceOfType(control))
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
        /// The aspect ratio of the screen
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return (float)System.Math.Sqrt(System.Math.Pow(_ScaleFactors.Y, 2) + System.Math.Pow(_ScaleFactors.X, 2));
            }
        }

        public RenderingWindow(int s)
        {
            g = new OpenMobile.Graphics.Graphics(s);
            this.screen = s;

            tmrMeasureFPS.Elapsed += new System.Timers.ElapsedEventHandler(tmrMeasureFPS_Elapsed);

            // Register redraw method
            ReDrawPanel = new ReDrawTrigger(Invalidate);
        }

        void tmrMeasureFPS_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {   // Measure max fps and log it to the debug log
            tmrMeasureFPS.Enabled = false;
            MeasureFPS_Done = true;

            FPS_Reset();
            Stopwatch sw = new Stopwatch();
            bool Measure = true;
            sw.Reset();
            sw.Start();
            while (Measure)
            {
                if (sw.ElapsedMilliseconds > 1500)
                    Measure = false;
                Thread.Sleep(0);
                refresh = true;
            }
            sw.Stop();
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Graphics", String.Format("Screen {0}: FPS Max (0ms) {1}", screen, FPS_Max));
            FPS_Reset();
            Measure = true;
            sw.Reset();
            sw.Start();
            while (Measure)
            {
                if (sw.ElapsedMilliseconds > 1500)
                    Measure = false;
                Thread.Sleep(1);
                refresh = true;
            }
            sw.Stop();
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Graphics", String.Format("Screen {0}: FPS Max (1ms) {1}", screen, FPS_Max));
            FPS_Reset();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InitializeRendering()
        {
            // Check for specific startup screen
            int StartupScreen = Core.theHost.StartupScreen;

            // Set bounds
            if (screen <= DisplayDevice.AvailableDisplays.Count - 1)
                this.Bounds = new Rectangle(DisplayDevice.AvailableDisplays[StartupScreen > 0 ? StartupScreen : screen].Bounds.Location, this.Size);

            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location);
            FormTitle = "openMobile v" + string.Format("{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart) + " (" + OpenMobile.Framework.OSSpecific.getOSVersion() + ") Screen " + screen.ToString();
            this.Title = FormTitle;
            
            // Connect events
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommonResources));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("Icon")));
            this.MouseLeave += new System.EventHandler<System.EventArgs>(this.RenderingWindow_MouseLeave);
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(this.RenderingWindow_FormClosing);
            this.Resize += new EventHandler<EventArgs>(this.RenderingWindow_Resize);
            this.Gesture += new EventHandler<OpenMobile.Graphics.TouchEventArgs>(RenderingWindow_Gesture);
            this.ResolutionChange += new EventHandler<OpenMobile.Graphics.ResolutionChange>(RenderingWindow_ResolutionChange);
            tmrClickHold.Elapsed += new System.Timers.ElapsedEventHandler(tmrClickLong_Elapsed);

            // Start input router
            if (screen == 0)
                InputRouter.Initialize();

            // Set window size
            if (Configuration.RunningOnWindows)
                if (options == GameWindowFlags.Fullscreen)
                    OnWindowStateChanged(EventArgs.Empty);

            // Set mouse startup location
            if ((this.WindowState == WindowState.Fullscreen) && (screen == 0))
                DefaultMouse.Location = this.Location;             
        }

        void tmrClickLong_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
            ActivateClick(FocusedControl, ClickTypes.Hold);
        }

        protected override void OnLoad(EventArgs e)
        {
            g.Initialize();
            if (screen == 0)
            {
                if ((Graphics.Graphics.Renderer == "GDI Generic") || (Graphics.Graphics.Renderer == "Software Rasterizer"))
                    Application.ShowError(this.WindowHandle, "This application has been forced to use software rendering.  Performance will be horrible until you install proper graphics drivers!!", "Performance Warning");
            }
            base.OnLoad(e);
        }

        public void Run(GameWindowFlags flags, Size initalScreenSize)
        {
            //NativeInitialize(flags, 800, 480);
            //NativeInitialize(flags, 1000, 600);
            NativeInitialize(flags, initalScreenSize.Width, initalScreenSize.Height);
            InitializeRendering();
            try
            {
                Run(1.0, 60.0, BuiltInComponents.SystemSettings.OpenGLVSync);
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg("RenderingWindow.Run Exception", e);
            }
        }

        public void RunAsync(GameWindowFlags flags, Size initalScreenSize)
        {
            Thread t = new Thread(delegate()
            {
                Run(flags, initalScreenSize);
            });
            t.TrySetApartmentState(ApartmentState.STA);
            t.Name = String.Format("RenderingWindow_{0}.RunAsync", screen);
            t.Start();
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            if (!MeasureFPS_Done)
                tmrMeasureFPS.Enabled = true;

            g.Clear(Color.Black);
            g.ResetClip();

            RenderPanels();

            // Render gestures
            RenderGesture();

            // Render cursors
            RenderCursor();

            // Render identity (if needed)
            RenderIndentity();

            // Render debuginfo (if needed)
            RenderDebugInfo();
            //ShowDebugInfoTitle();

            // Render a "dimmer" overlay to reduce screen brightness
            RenderDimmer();

            SwapBuffers(); //show the new image before potentially lagging

            g.Finish();
        }


        private void ShowDebugInfoTitle()
        {
            if (BuiltInComponents.Host.ShowDebugInfo)
                this.Title = String.Format("{0} (FPS: {1}/{2}/{3})", FormTitle, FPS_Min, FPS, FPS_Max);
        }

        #region Local renderers

        private void RenderDimmer()
        {
            if (dimmer > 0)
                g.FillRectangle(new Brush(Color.FromArgb(dimmer, Color.Black)), 0, 0, 1000, 600);
        }

        private StringWrapper DebugString = new StringWrapper();
        private OImage RenderDebugInfoTexture = null;
        private void RenderDebugInfo()
        {
            if (BuiltInComponents.Host.ShowDebugInfo)
            {
                lock (painting)
                {
                    DebugString.Text = String.Format("Screen: {0}\nFPS: {1}/{2}/{3}\nFocus: {4}.{5}\nFocusParent: {6}\nUnderMouse: {7}.{8}", screen, FPS_Min, FPS, FPS_Max, (FocusedControl != null && FocusedControl.Parent != null ? FocusedControl.Parent.Name : ""), (FocusedControl != null ? FocusedControl.Name : ""), (FocusedControlParent != null ? FocusedControlParent.Name : ""), (MouseOverControl != null && MouseOverControl.Parent != null ? MouseOverControl.Parent.Name : ""), (MouseOverControl != null ? MouseOverControl.Name : ""));
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
                    if (identity.TextureGenerationRequired(screen))
                        identity = g.GenerateTextTexture(identity, 0, 0, 1000, 600, screen.ToString(), new Font(Font.GenericSansSerif, 400F), eTextFormat.Outline, Alignment.CenterCenter, Color.White, Color.Black);
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
                    g.DrawLine(new Pen(Color.Red, 3F), CursorPosition.X, CursorPosition.Y, CursorPosition.X + 5, CursorPosition.Y);
                    g.DrawLine(new Pen(Color.Red, 3F), CursorPosition.X, CursorPosition.Y, CursorPosition.X, CursorPosition.Y + 5);
                    g.DrawLine(new Pen(Color.Red, 3F), CursorPosition.X, CursorPosition.Y, CursorPosition.X + 12, CursorPosition.Y + 12);
                }
            }
        }

        private void TransitionEffect_ConfigureRenderingParams(renderingParams e)
        {
            // Reset transformation data
            g.ResetTransform();
            RenderingParam.Alpha = 1.0f;

            // Exit after resetting transiton effects?
            if (e == null)
                return;

            // Apply any offset data
            g.TranslateTransform(e.Offset.X, e.Offset.Y);

            // Apply any rotation data
            if (e.Rotation.Length != 0)
                g.Rotate(e.Rotation);

            // Apply any scale data
            if (e.Scale.X != 1 | e.Scale.Y != 1 | e.Scale.Y != 1)
                g.Scale(e.Scale);

            // Apply transparency values (this is done via rendering parameters passed along to each control)
            RenderingParam.Alpha = e.Alpha;
        }

        private bool RenderingError = false;
        protected void RenderPanels()
        {
            lock (painting)
            {
                try
                {
                    for (int i = 0; i < RenderingQueue.Count; i++)
                    {
                        // Configure rendering params based on panel mode
                        if (RenderingQueue[i].Mode == eModeType.transitioningIn)
                            // Render parameters for transition effect in
                            TransitionEffect_ConfigureRenderingParams(TransitionEffectParam_In);
                        else if (RenderingQueue[i].Mode == eModeType.transitioningOut)
                            // Render parameters for transition effect out
                            TransitionEffect_ConfigureRenderingParams(TransitionEffectParam_Out);
                        else
                            // Reset any transition effects
                            TransitionEffect_ConfigureRenderingParams(null);

                        RenderingQueue[i].Render(g, RenderingParam);
                    }
                    RenderingError = false;
                }
                catch (Exception e)
                {
                    if (!RenderingError)
                    {
                        RenderingError = true;
                        BuiltInComponents.Host.DebugMsg(String.Format("RenderingWindow.RenderPanels (Screen {0}) Exception:", screen), e);
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
            ScaleFactors = new PointF((this.ClientRectangle.Width / 1000F), (this.ClientRectangle.Height / 600F));
            OnRenderFrameInternal();
            raiseResizeEvent();

            // Also make other windows follow the state of the main window (maximize and minimize)
            Core.theHost.SetAllWindowState(this.WindowState);
        }
        protected override void OnResize(EventArgs e)
        {
            MakeCurrent();
            g.Resize(Width, Height);
            base.OnResize(e);
            MakeCurrent(null);
        }
        protected override void OnWindowStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Fullscreen;
            if ((this.WindowState == WindowState.Fullscreen) && (!defaultMouse))
            {
                if (screen == 0)
                    DefaultMouse.TrapCursor();
                DefaultMouse.HideCursor(this.WindowInfo);
            }
            else
            {
                if ((screen == 0) && (!defaultMouse))
                    DefaultMouse.UntrapCursor();
                DefaultMouse.ShowCursor(this.WindowInfo);
            }
            base.OnWindowStateChanged(e);
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
            Core.theHost.execute(eFunction.multiTouchGesture, screen.ToString(), gesture);
        }

        void RenderingWindow_ResolutionChange(object sender, OpenMobile.Graphics.ResolutionChange e)
        {
            try
            {
                DisplayDevice dev = DisplayDevice.AvailableDisplays[screen];
                if (e.Landscape != dev.Landscape)
                    Core.theHost.raiseSystemEvent(eFunction.screenOrientationChanged, screen.ToString(), e.Landscape ? "Landscape" : "Portrait", String.Empty);
            }
            catch (Exception ex)
            {
                BuiltInComponents.Host.DebugMsg("RenderingWindow_ResolutionChange Exception", ex);
            }
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
            if ((int)sender != screen)
                return;

            // Scale mouse data
            MouseMoveEventArgs eScaled = new MouseMoveEventArgs(e);
            MouseMoveEventArgs.Scale(eScaled, _ScaleFactors);

            // Save current cursor position
            CursorPosition = eScaled.Location;

            // Calculate mouse move distances
            CursorDistance = (MouseMoveStartPoint.ToVector2() - CursorPosition.ToVector2()).Length;
            CursorDistanceXYTotal = CursorPosition - MouseMoveStartPoint;
            CursorDistanceXYRelative = CursorPosition - CursorDistanceXYRelative;

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
                        currentGesture.Add(CursorPosition);
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
                                ((IThrow)FocusedControlParent).MouseThrowStart(screen, MouseMoveStartPoint, _ScaleFactors, ref cancel);
                                ThrowActive = !cancel;
                                UpdateControlFocus(FocusedControlParent, null, false);
                            }
                        }
                        else
                        {   // Throw started, update data for throw interface
                            ((IThrow)FocusedControlParent).MouseThrow(screen, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
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
                                ((IThrow)FocusedControl).MouseThrowStart(screen, MouseMoveStartPoint, _ScaleFactors, ref cancel);
                                ThrowActive = !cancel;
                            }
                        }
                        else
                        {   // Throw started, update data for throw interface
                            ((IThrow)FocusedControl).MouseThrow(screen, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
                        }
                    }
                }
            }
            else
            {   // Regular mouse move action

                OMControl ParentControl = null;

                // Find control under mouse
                control = FindControlAtLocation(CursorPosition, out ParentControl);

                // Highlight/unhighlight the control
                UpdateControlFocus(control, ParentControl, true);
            }

            // Send event data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                    {
                        ((IMouse)FocusedControlParent).MouseMove(screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
                    }
            }
            else
            {   // Send event to focused control
                // Mouse interface
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                        ((IMouse)FocusedControl).MouseMove(screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal, CursorDistanceXYRelative);
            }

            // Update relative distance data
            CursorDistanceXYRelative = CursorPosition;
            
            // Redraw screen
            Invalidate();
        }

        /// <summary>
        /// This event is not used 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void RenderingWindow_MouseClick(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            // Not used
        }

        internal void RenderingWindow_MouseDown(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != screen)
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

            OMControl control = FocusedControl;
            OMControl ParentControl = null;

            // If nothing is selected then try to select something
            if (FocusedControl == null)
            {
                // Find control under mouse
                control = FindControlAtLocation(MouseMoveStartPoint, out ParentControl);

                // Highlight/unhighlight the control
                UpdateControlFocus(control, ParentControl, true);
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
            }

            // Send Mouse interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                    {
                        ((IMouse)FocusedControlParent).MouseDown(screen, eScaled, MouseMoveStartPoint);
                    }
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                        ((IMouse)FocusedControl).MouseDown(screen, eScaled, MouseMoveStartPoint);
            }

            // Redraw
            Invalidate();
        }

        internal void RenderingWindow_MouseUp(object sender, OpenMobile.Input.MouseButtonEventArgs e)
        {
            if ((int)sender != screen)
                return;

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


            // Return if gesture is handled or no control has focus
            if (GestureHandled || FocusedControl == null)
            {
                // Find control under mouse (if any)
                OMControl ParentControl = null;
                OMControl control = FindControlAtLocation(CursorPosition, out ParentControl);

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
                    ActivateClick(FocusedControl, ClickTypes.Normal);
                }
                else
                {   // Long click
                    ActivateClick(FocusedControl, ClickTypes.Long);
                }
            }

            // Send Mouse interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IMouse).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (FocusedControlParent != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControlParent))
                        ((IMouse)FocusedControlParent).MouseUp(screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal);
            }
            else
            {   // Send event to focused control
                if (FocusedControl != null)
                    if (typeof(IMouse).IsInstanceOfType(FocusedControl))
                        ((IMouse)FocusedControl).MouseUp(screen, eScaled, MouseMoveStartPoint, CursorDistanceXYTotal);
            }

            // Send Throw interface data
            if (FocusedControlParent != null && ((FocusedControl != null && !typeof(IThrow).IsInstanceOfType(FocusedControl)) || (FocusedControl == null)))
            {   // Send event to focused control parent
                if (ThrowActive)
                {
                    if (FocusedControlParent != null)
                        if (typeof(IThrow).IsInstanceOfType(FocusedControlParent))
                            ((IThrow)FocusedControlParent).MouseThrowEnd(screen, MouseMoveStartPoint, CursorDistanceXYTotal, eScaled.Location);
                }
            }
            else
            {   // Send event to focused control 
                if (ThrowActive)
                {
                    if (FocusedControl != null)
                        if (typeof(IThrow).IsInstanceOfType(FocusedControl))
                            ((IThrow)FocusedControl).MouseThrowEnd(screen, MouseMoveStartPoint, CursorDistanceXYTotal, eScaled.Location);
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
                    if (((IKey)iKeyControl).KeyDown_BeforeUI(screen, e, _ScaleFactors))
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
                    ((IKey)iKeyControl).KeyDown_AfterUI(screen, e, _ScaleFactors);

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
                    if (((IKey)iKeyControl).KeyUp_BeforeUI(screen, e, _ScaleFactors))
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
                    if (screen == 0)
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
                            ActivateClick(FocusedControl, ClickTypes.Long);
                        }
                        else if (e.Shift == true && e.Control == true)
                        {   // Hold click
                            ActivateClick(FocusedControl, ClickTypes.Hold);
                        }
                        else
                        {   // Normal click
                            ActivateClick(FocusedControl, ClickTypes.Normal);
                        }
                    }
                }
            }

            // iKey interface
            if (iKeyControl != null)
                if (typeof(IKey).IsInstanceOfType(iKeyControl))
                    ((IKey)iKeyControl).KeyUp_AfterUI(screen, e, _ScaleFactors);

            Invalidate();
        }

        public void ExecuteTransition(string transType)
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                List<OMPanel> panels = RenderingQueue.FindAll(x => ((x.Mode == eModeType.transitioningIn) || (x.Mode == eModeType.transitioningOut)));

                // Reset transition effects parameters
                TransitionEffectParam_In = new renderingParams();
                TransitionEffectParam_Out = new renderingParams();
                
                // Execute effect
                PanelTransitionEffectHandler.GetEffect(transType).Run(TransitionEffectParam_In, TransitionEffectParam_Out, ReDrawPanel, BuiltInComponents.SystemSettings.TransitionSpeed);
                
                // Go trough each panel to set correct modes after transition effects
                foreach (OMPanel panel in panels)
                {
                    if (panel.Mode == eModeType.transitioningIn)
                    {   // Panel is transitioning in, set to normal state
                        panel.Mode = eModeType.Normal;

                        // Raise event for entering panel
                        panel.RaiseEvent(screen, eEventType.Entering);
                    }
                    else if (panel.Mode == eModeType.transitioningOut)
                    {   // Panel is transitioning out, remove from rendering queue
                        RenderingQueue.Remove(panel);

                        // Unhook refresh event
                        panel.UpdateThisControl -= UpdateThisControl;

                        // Raise event for leaving panel
                        panel.RaiseEvent(screen, eEventType.Leaving);
                    }
                }
            }

            // Reset transition effects parameters
            TransitionEffectParam_In = new renderingParams();
            TransitionEffectParam_Out = new renderingParams();

            Invalidate();
        }

        public void TransitionInPanel(OMPanel newP)
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                OMPanel ExistingPanel = RenderingQueue.Find(x => x == newP);
                if (ExistingPanel == null)
                {
                    // Attach screen update event to new panel
                    newP.UpdateThisControl += UpdateThisControl;

                    // Unfocus currently focused control
                    UpdateControlFocus(null, null, true);

                    // Mark this panel as transitionin in
                    newP.Mode = eModeType.transitioningIn;

                    // Add new panel
                    insertPanel(newP);

                    // Raise panel event
                    newP.RaiseEvent(screen, eEventType.Loaded);
                }
                else
                {   // Reset existing panel to normal mode if it's already loaded
                    ExistingPanel.Mode = eModeType.Normal;
                }

            }
        }

        public void TransitionOutPanel(OMPanel oldP)
        {
            lock (this) // Lock to prevent multiple transitons at the same time
            {
                // Unfocus currently focused control
                UpdateControlFocus(null, null, true);

                // Mark this panel as transitioning out
                oldP.Mode = eModeType.transitioningOut;

                // Raise panel event
                oldP.RaiseEvent(screen, eEventType.Unloaded);
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
                        RenderingQueue[i].RaiseEvent(screen, eEventType.Unloaded);
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Closes this screen with animation
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
        /// Fades the screen to black
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
        /// Requests a redraw of the screen
        /// </summary>
        private void Invalidate()
        {
            refresh = true;
        }

        /// <summary>
        /// Hookable method to request a redraw of the screen
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
                    Core.theHost.raiseSystemEvent(eFunction.RenderingWindowResized, screen.ToString(), String.Empty, String.Empty);
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
                if (RenderingQueue[i].Priority <= newP.Priority)
                {
                    RenderingQueue.Insert(i + 1, newP);
                    return;
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
                if (typeof(IContainer2).IsInstanceOfType(control))
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
            if (!control.Visible)
                return false;

            // Ensure control allows user interaction
            if (control.NoUserInteraction)
                return false;

            // Return false if this is not inside this controls region
            if (!control.Region.Contains(Location))
                return false;

            // Check if control overrides the hit test 
            if (typeof(INotClickable).IsInstanceOfType(control))
                if (((INotClickable)this).IsPointClickable(Location.X, Location.Y))
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

        private bool IsControlFocusable(OMControl control)
        {
            if (control == null)
                return false;

            return (control.Visible && !control.NoUserInteraction && typeof(IHighlightable).IsInstanceOfType(control) && ApplicationArea.Contains(control.Region));
        }

        private bool IsControlClickable(OMControl control)
        {
            return typeof(IClickable).IsInstanceOfType(control);
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
                OpenMobile.Math.Vector2 FocusedControlLocation = FocusedControl.Region.Center.ToVector2();

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
                        if (typeof(IContainer2).IsInstanceOfType(control))
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

        private enum ClickTypes { None, Normal, Long, Hold }
        private void ActivateClick(OMControl control, ClickTypes ClickType)
        {
            // Cancel if no control is provided
            if (control == null)
                return;

            // Lock the currently focused control
            lock (FocusedControl)
            {
                // Cancel if control is not clickable
                if (!typeof(IClickable).IsInstanceOfType(control))
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
                                ((IClickable)control).clickMe(screen);
                            });
                        }
                        break;
                    case ClickTypes.Long:
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                ((IClickable)control).longClickMe(screen);
                            });
                        }
                        break;
                    case ClickTypes.Hold:
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                ((IClickable)control).holdClickMe(screen);
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
                    Core.theHost.raiseGestureEvent(screen, rec.Recognize(), RenderingQueue[RenderingQueue.Count - 1], FocusedControl);
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

    }
}
