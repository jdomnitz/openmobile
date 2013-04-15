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
using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Creates a fully valid window handle that can be used as a target for other applications
    /// </summary>
    public class OMTargetWindow : OMControl, IClickable
    {
        /// <summary>
        /// Local wrapper class for gamewindow
        /// </summary>
        private class TargetWindow : GameWindow
        {
            
        }

        /// <summary>
        /// Outputwindow
        /// </summary>
        private TargetWindow outputWindow = null;

        private bool _Render_IgnoreOverlappingObjects = false;
        private bool _Render_TransitionHidesWindow = false;

        #region events

        /// <summary>
        /// Target Window created arguments
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        /// <param name="window"></param>
        /// <param name="handle"></param>
        public delegate void WindowArgs(OMTargetWindow sender, int screen, GameWindow window, IntPtr handle);

        /// <summary>
        /// Raised when the targetwindow has been created
        /// </summary>
        public event WindowArgs OnWindowCreated;

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender"></param>
        private void RaiseOnWindowCreated(OMTargetWindow sender)
        {
            IntPtr handle = (IntPtr)sender.outputWindow.WindowHandle;
            int screen = sender.Parent.ActiveScreen;

            // Log info to debug log
            BuiltInComponents.Host.DebugMsg( DebugMessageType.Info, String.Format("OMTargetWindow({0})",this), String.Format("Windowhandle {0} created on screen {1}", handle, screen));

            if (OnWindowCreated != null && sender.outputWindow != null)
                OnWindowCreated(sender, screen, sender.outputWindow, handle);
        }

        /// <summary>
        /// Raised when the targetwindow is about to be disposed
        /// </summary>
        public event WindowArgs OnWindowDisposed;

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender"></param>
        private void RaiseOnWindowDisposed(OMTargetWindow sender)
        {
            IntPtr handle = (IntPtr)sender.outputWindow.WindowHandle;
            int screen = sender.Parent.ActiveScreen;

            // Log info to debug log
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, String.Format("OMTargetWindow({0})", this), String.Format("Windowhandle {0} disposed on screen {1}", handle, screen));

            if (OnWindowDisposed != null && sender.outputWindow != null)
                OnWindowDisposed(sender, screen, sender.outputWindow, handle);
        }

        /// <summary>
        /// Raised when the targetWindow is covered by other controls
        /// </summary>
        public event ControlEventHandler OnWindowCovered;

        /// <summary>
        /// (Async) Raised when the targetWindow is covered by other controls
        /// </summary>
        public event ControlEventHandler OnWindowCoveredAsync;
        private void RaiseOnWindowCovered(OMTargetWindow sender, bool async)
        {
            int screen = sender.Parent.ActiveScreen;
            if (async)
            {
                if (OnWindowCoveredAsync != null)
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate() { OnWindowCoveredAsync(this, screen); });
            }
            else
            {
                if (OnWindowCovered != null)
                    OnWindowCovered(this, screen);
            }
        }

        /// <summary>
        /// Raised when the targetWindow is uncovered after being covered by other controls
        /// </summary>
        public event ControlEventHandler OnWindowUncovered;
        /// <summary>
        /// (Async) Raised when the targetWindow is uncovered after being covered by other controls
        /// </summary>
        public event ControlEventHandler OnWindowUncoveredAsync;
        private void RaiseOnWindowUnCovered(OMTargetWindow sender, bool async)
        {
            int screen = sender.Parent.ActiveScreen;
            if (async)
            {
                if (OnWindowUncoveredAsync != null)
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate() { OnWindowUncoveredAsync(this, screen); });
            }
            else
            {
                if (OnWindowUncovered != null)
                    OnWindowUncovered(this, screen);
            }

        }

        /// <summary>
        /// Handles when to raise the covered and uncovered events
        /// </summary>
        /// <param name="visibility"></param>
        private void HandleWindowCoveredEvents(bool visibility)
        {
            if (visibility != _visibilityStored)
            {
                _visibilityStored = visibility;
                if (visibility)
                {
                    RaiseOnWindowUnCovered(this, false);
                    RaiseOnWindowUnCovered(this, true);
                }
                else
                {
                    RaiseOnWindowCovered(this, false);
                    RaiseOnWindowCovered(this, true);
                }
            }
        }
        private bool _visibilityStored = false;

        #endregion

        #region Constructor / desctructor

        /// <summary>
        /// Create a new Target window control
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Left"></param>
        /// <param name="Top"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public OMTargetWindow(string Name, int Left, int Top, int Width, int Height)
            : base(Name, Left, Top, Width, Height)
        {
        }

        /// <summary>
        /// Dispose target window
        /// </summary>
        ~OMTargetWindow()
        {
            if (outputWindow != null)
            {
                outputWindow.Dispose();
                outputWindow = null;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Controls visibility
        /// </summary>
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
                if (outputWindow != null)
                    outputWindow.Visible = value;
            }
        }

        /// <summary>
        /// Control's size is changed
        /// </summary>
        protected override void onSizeChanged()
        {
            base.onSizeChanged();
            if (parent != null)
                PlaceWindow(parent, ref outputWindow);
        }

        /// <summary>
        /// Renders this control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.Render(g, e);

            bool Visibility = true;

            if (!_Redirected_IgnoreTransition)
            {
                // Only show window if opacity is above 90%
                if (base.GetAlphaValue1(1) >= 0.9f)
                    Visibility = true;
                else
                    Visibility = false;
            }

            // Check if control is covered by other controls
            if (!_Render_IgnoreOverlappingObjects && Visibility)
            {
                if (BuiltInComponents.Host.RenderingWindowInterface(g.screen).IsControlCoveredByOthers(this))
                {
                    if (!_Redirected_IgnoreIfCovered)
                        Visibility = false;
                    HandleWindowCoveredEvents(false);
                }
                else
                {
                    HandleWindowCoveredEvents(true);
                }
            }

            // Hide window during transitions
            if (e.TransitionActive && !_Redirected_IgnoreTransition && !_Render_TransitionHidesWindow)
            {
                Visibility = false;
                _Render_TransitionHidesWindow = true;
                HandleWindowCoveredEvents(false);
            }

            if (outputWindow.Visible != Visibility)
            {
                outputWindow.Visible = Visibility;

                if (Visibility && _Render_TransitionHidesWindow)
                {
                    HandleWindowCoveredEvents(true);
                    _Render_TransitionHidesWindow = false;
                }
            }
        }

        /// <summary>
        /// Halts rendering
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void RenderStop(Graphics.Graphics g, renderingParams e)
        {
            base.RenderStop(g, e);

            if (!_Redirected_IgnoreTransition)
                outputWindow.Visible = false;
        }

        #endregion

        /// <summary>
        /// Place outputwindow absolutely on screen
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        private void PlaceWindow(OMPanel parent, ref TargetWindow target)
        {
            RenderingWindowData WindowData = BuiltInComponents.Host.GetRenderingWindowData(parent.ActiveScreen);
            if (_Redirected_Placement == Rectangle.Empty)
            {
                Point newLocation = new Point(WindowData.Location.X + (this.left * WindowData.ScaleFactors.X), WindowData.Location.Y + (this.top * WindowData.ScaleFactors.Y));
                target.X = newLocation.X;
                target.Y = newLocation.Y;
                target.Width = (int)(this.width * WindowData.ScaleFactors.X);
                target.Height = (int)(this.height * WindowData.ScaleFactors.Y);
            }
            else
            {
                Point newLocation = new Point(WindowData.Location.X + (_Redirected_Placement.Left * WindowData.ScaleFactors.X), WindowData.Location.Y + (_Redirected_Placement.Top * WindowData.ScaleFactors.Y));
                target.X = newLocation.X;
                target.Y = newLocation.Y;
                target.Width = (int)(_Redirected_Placement.Width * WindowData.ScaleFactors.X);
                target.Height = (int)(_Redirected_Placement.Height * WindowData.ScaleFactors.Y);
            }
        }


        /// <summary>
        /// Clone this control 
        /// <para>This is where the initialization of the target window is done to ensure we get a unique window for each screen</para>
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override object Clone(OMPanel parent)
        {
            // Nothing to clone here, we create new object instead
            OMTargetWindow newObject = (OMTargetWindow)base.Clone(parent);

            // Try to create a separate video output window
            newObject.outputWindow = new TargetWindow();
            newObject.outputWindow.NativeInitialize(GameWindowFlags.Hidden | GameWindowFlags.AlwaysOnTop | GameWindowFlags.BlockAllRendering, Width, Height);
            newObject.outputWindow.StopRendering = true;
            newObject.outputWindow.Visible = this.visible;

            if (newObject.outputWindow.DefaultMouse != null)
            {
                newObject.outputWindow.DefaultMouse.MouseClick += new EventHandler<Input.MouseButtonEventArgs>(newObject.DefaultMouse_MouseClick);

                // Set mouse instance number (negative instance number = default os unit)
                newObject.outputWindow.DefaultMouse.Instance = (parent.ActiveScreen * -1) - 1; // Instance is negative screen number for default units with an offset of one

                // Limit mouse area to the size of the screen
                newObject.outputWindow.DefaultMouse.SetBounds(width, height);
            }

            // Place new window correctly
            PlaceWindow(parent, ref newObject.outputWindow);

            newObject.outputWindow.Load += new EventHandler<EventArgs>(newObject.outputWindow_Load);

            // Spawn thread to keep window alive
            OpenMobile.Threading.SafeThread.Asynchronous(delegate()
            {                
                // Execute rendering code
                newObject.outputWindow.Run(1, 60, false);

                // Dispose window
                newObject.outputWindow.Dispose();
            });

            // Connect event to catch resize events
            BuiltInComponents.Host.OnSystemEvent += new Plugin.SystemEvent(newObject.Host_OnSystemEvent);

            return newObject;
        }

        #region Host and window events

        /// <summary>
        /// On event: window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void outputWindow_Load(object sender, EventArgs e)
        {
            // Hide window as default (it will be shown when this control is requested to render)
            outputWindow.Visible = false;

            // Raise event
            RaiseOnWindowCreated(this);
        }

        /// <summary>
        /// Systemevents from host
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.RenderingWindowResized)
            {
                if (helperFunctions.Params.IsParamsValid(args, 4))
                {
                    int Screen = helperFunctions.Params.GetParam<int>(args, 0);
                    Point Location = helperFunctions.Params.GetParam<Point>(args, 1);
                    Size Size = helperFunctions.Params.GetParam<Size>(args, 2);
                    PointF ScaleFactors = helperFunctions.Params.GetParam<PointF>(args, 3);

                    if (Screen == this.parent.ActiveScreen)
                    {   // Move embedded window to ensure it's placed correctly related to main window
                        Point newLocation = new Point(Location.X + (this.left * ScaleFactors.X), Location.Y + (this.top * ScaleFactors.Y));
                        outputWindow.Location = newLocation;

                        outputWindow.Width = (int)(this.width * ScaleFactors.X);
                        outputWindow.Height = (int)(this.height * ScaleFactors.Y);
                    }
                }
            }
            else if (function == eFunction.closeProgram)
            {
                RaiseOnWindowDisposed(this);
                outputWindow.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Gets the handle to the embedded window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                if (outputWindow != null)
                    return (IntPtr)outputWindow.WindowHandle;
                else
                    return IntPtr.Zero;
            }
        }

        #region Mouse interaction

        void DefaultMouse_MouseClick(object sender, Input.MouseButtonEventArgs e)
        {
            if (e.Button == Input.MouseButton.Left)
            {
                int SenderScreen = ((int)sender + 1) * -1;
                clickMe(SenderScreen);
            }
        }

        public void clickMe(int screen)
        {
            if (OnClick != null)
                OnClick(this, screen);
        }

        public void longClickMe(int screen)
        {
            if (OnLongClick != null)
                OnLongClick(this, screen);
        }

        public void holdClickMe(int screen)
        {
            if (OnHoldClick != null)
                OnHoldClick(this, screen);
        }

        public event userInteraction OnClick;

        public event userInteraction OnLongClick;

        public event userInteraction OnHoldClick;

        #endregion

        /// <summary>
        /// Sets or gets the fullscreen status
        /// </summary>
        public bool Fullscreen
        {
            get
            {
                return this._Fullscreen;
            }
            set
            {
                if (this._Fullscreen != value)
                {
                    this._Fullscreen = value;
                    SetFullScreen(value);
                }
            }
        }
        private bool _Fullscreen;

        private Rectangle _OrignalRegion;
        private void SetFullScreen(bool fullScreen)
        {
            if (fullScreen)
            {
                _OrignalRegion = this.Region;
                _Render_IgnoreOverlappingObjects = true;
                this.Region = BuiltInComponents.Host.ClientFullArea;
            }
            else
            {
                _Render_IgnoreOverlappingObjects = false;
                this.Region = _OrignalRegion;
            }
        }

        /// <summary>
        /// True = Hide this window when covered by other controls
        /// </summary>
        public bool HideWhenCovered
        {
            get
            {
                return !this._Render_IgnoreOverlappingObjects;
            }
            set
            {
                _Render_IgnoreOverlappingObjects = !value;
            }
        }

        private Rectangle _Redirected_Placement = Rectangle.Empty;
        private bool _Redirected_IgnoreIfCovered = false;
        private bool _Redirected_IgnoreTransition = false;


        /// <summary>
        /// Redirects the outputwindow temporary without moving the control
        /// </summary>
        /// <param name="newPlacement"></param>
        /// <param name="ignoreIfCovered"></param>
        /// <param name="ignoreTransition"></param>
        public void RedirectOutputWindow(Rectangle newPlacement, bool ignoreIfCovered, bool ignoreTransition)
        {
            outputWindow.Visible = true;
            _Redirected_Placement = newPlacement;
            _Redirected_IgnoreIfCovered = ignoreIfCovered;
            _Redirected_IgnoreTransition = ignoreTransition;
            PlaceWindow(this.parent, ref outputWindow);
            outputWindow.Visible = true;
        }

        /// <summary>
        /// Resets the temporary redirect
        /// </summary>
        public void RedirectReset()
        {
            _Redirected_Placement = Rectangle.Empty;
            _Redirected_IgnoreIfCovered = false;
            _Redirected_IgnoreTransition = false;
            PlaceWindow(this.parent, ref outputWindow);
        }

    }
}
