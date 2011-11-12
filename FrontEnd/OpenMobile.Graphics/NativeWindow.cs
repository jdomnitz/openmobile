#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile.Input;
using OpenMobile.Platform;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace OpenMobile
{

    /// <summary>
    /// Instances of this class implement the <see cref="OpenMobile.INativeWindow"/> interface on the current platform.
    /// </summary>
    public class NativeWindow : INativeWindow
    {
        #region --- Fields ---

        protected GameWindowFlags options;

        private readonly DisplayDevice device;

        private INativeWindow implementation;

        private bool disposed, events;

        protected int screen = -1;
        #endregion
        public NativeWindow()
        {
            this.device = DisplayDevice.Default;
        }
        #region --- Contructors ---
        public void NativeInitialize(GameWindowFlags flags)
        {
            options = flags;
            // This call takes approx 2000ms
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Start: " + Timing.GetTiming());

            int S = screen;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing1: " + Timing.GetTiming());
            int X = device.Bounds.X + (device.Bounds.Width - 720) / 2;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing2: " + Timing.GetTiming());
            int Y = device.Bounds.Y + (device.Bounds.Height - 450) / 2;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing3: " + Timing.GetTiming());
            int W = 720;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing4: " + Timing.GetTiming());
            int H = 450;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing5: " + Timing.GetTiming());
            string T = "OpenMobile Native Window";
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing6: " + Timing.GetTiming());
            GraphicsMode M = GraphicsMode.Default;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing7: " + Timing.GetTiming());
            GameWindowFlags O = flags;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing8: " + Timing.GetTiming());
            DisplayDevice D = DisplayDevice.Default;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing9: " + Timing.GetTiming());
            implementation = Factory.Default.CreateNativeWindow(S, X, Y, W, H, T, M, O, D);
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").Timing10: " + Timing.GetTiming());

            //implementation = Factory.Default.CreateNativeWindow(screen, device.Bounds.X + (device.Bounds.Width - 720) / 2, device.Bounds.Y + (device.Bounds.Height - 450) / 2, 720, 450, "OpenMobile Native Window", GraphicsMode.Default, flags, DisplayDevice.Default);
            implementation.Visible = false;
            //Console.WriteLine("NativeInitialize(" + screen.ToString() + ").End: " + Timing.GetTiming());

            if (flags != GameWindowFlags.Temporary)
            {
                if (Environment.GetCommandLineArgs().Length > 1)
                {
                    if (Environment.GetCommandLineArgs()[1].ToLower().StartsWith("-size=") == true)
                    {
                        string[] part = Environment.GetCommandLineArgs()[1].Substring(6).Split(new char[] { 'x' });
                        try
                        {
                            Size = new Size(int.Parse(part[0]), int.Parse(part[1]));
                        }
                        catch (ArgumentException) { }
                    }
                }
            }
        }
        #endregion

        #region --- INativeWindow Members ---

        #region Methods

        #region Close

        /// <summary>
        /// Closes the NativeWindow.
        /// </summary>
        public void Close()
        {
            EnsureUndisposed();
            implementation.Close();
        }

        #endregion

        #region ProcessEvents

        /// <summary>
        /// Processes operating system events until the NativeWindow becomes idle.
        /// </summary>
        public void ProcessEvents()
        {
            if (IsDisposed)
                return;
            if (!events)
                Events = true;
            implementation.ProcessEvents();
        }

        #endregion

        #endregion

        #region Properties

        #region Bounds

        /// <summary>
        /// Gets or sets a <see cref="System.Drawing.Rectangle"/> structure that contains the external bounds of this window, in screen coordinates.
        /// External bounds include the title bar, borders and drawing area of the window.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return implementation.Bounds;
            }
            set
            {
                implementation.Bounds = value;
            }
        }

        #endregion

        public object WindowHandle
        {
            get
            {
                return implementation.WindowHandle;
            }
        }

        #region ClientRectangle

        /// <summary>
        /// Gets or sets a <see cref="System.Drawing.Rectangle"/> structure that contains the internal bounds of this window, in client coordinates.
        /// The internal bounds include the drawing area of the window, but exclude the titlebar and window borders.
        /// </summary>
        public Rectangle ClientRectangle
        {
            get
            {
                return implementation.ClientRectangle;
            }
            set
            {
                implementation.ClientRectangle = value;
            }
        }

        #endregion

        #region ClientSize

        /// <summary>
        /// Gets or sets a <see cref="System.Drawing.Size"/> structure that contains the internal size this window.
        /// </summary>
        public Size ClientSize
        {
            get
            {
                return implementation.ClientSize;
            }
            set
            {
                implementation.ClientSize = value;
            }
        }

        #endregion

        #region Exists

        /// <summary>
        /// Gets a value indicating whether a render window exists.
        /// </summary>
        public bool Exists
        {
            get
            {
                return IsDisposed ? false : implementation.Exists;
            }
        }

        #endregion

        #region Height

        /// <summary>
        /// Gets or sets the external height of this window.
        /// </summary>
        public int Height
        {
            get
            {
                return implementation.Height;
            }
            set
            {
                implementation.Height = value;
            }
        }

        #endregion

        #region Icon

        /// <summary>
        /// Gets or sets the System.Drawing.Icon for this GameWindow.
        /// </summary>
        public System.Drawing.Icon Icon
        {
            set
            {
                implementation.Icon = value;
            }
        }

        #endregion

        #region InputDriver

        public IInputDriver InputDriver
        {
            get
            {
                return implementation!=null ? implementation.InputDriver : null;
            }
        }

        #endregion

        #region Location

        /// <summary>
        /// Gets or sets a <see cref="System.Drawing.Point"/> structure that contains the location of this window on the desktop.
        /// </summary>
        public Point Location
        {
            get
            {
                return implementation.Location;
            }
            set
            {
                implementation.Location = value;
            }
        }

        #endregion

        #region Size

        /// <summary>
        /// Gets or sets a <see cref="System.Drawing.Size"/> structure that contains the external size of this window.
        /// </summary>
        public Size Size
        {
            get
            {
                return implementation.Size;
            }
            set
            {
                implementation.Size = value;
            }
        }

        #endregion

        #region Title

        /// <summary>
        /// Gets or sets the NativeWindow title.
        /// </summary>
        public string Title
        {
            set
            {
                implementation.Title = value;
            }
        }

        #endregion

        #region Visible

        /// <summary>
        /// Gets or sets a System.Boolean that indicates whether this NativeWindow is visible.
        /// </summary>
        public bool Visible
        {
            get
            {
                if ((implementation == null)||(IsDisposed))
                    return false;
                return implementation.Visible;
            }
            set
            {
                implementation.Visible = value;
            }
        }

        #endregion
        public float Opacity
        {
            get
            {
                return implementation.Opacity;
            }
            set
            {
                implementation.Opacity=value;
            }
        }
        
        #region Width

        /// <summary>
        /// Gets or sets the external width of this window.
        /// </summary>
        public int Width
        {
            get
            {
                return implementation.Width;
            }
            set
            {
                implementation.Width = value;
            }
        }

        #endregion

        #region WindowBorder

        /// <summary>
        /// Gets or states the border of the NativeWindow.
        /// </summary>
        public WindowBorder WindowBorder
        {
            get
            {
                return implementation.WindowBorder;
            }
            set
            {
                implementation.WindowBorder = value;
            }
        }

        #endregion

        #region WindowInfo

        /// <summary>
        /// Gets the <see cref="OpenMobile.Platform.IWindowInfo"/> of this window.
        /// </summary>
        public IWindowInfo WindowInfo
        {
            get
            {
                return implementation.WindowInfo;
            }
        }

        #endregion

        #region WindowState

        /// <summary>
        /// Gets or states the state of the NativeWindow.
        /// </summary>
        public virtual WindowState WindowState
        {
            get
            {
                if (implementation == null)
                    return WindowState.Normal;
                return implementation.WindowState;
            }
            set
            {
                implementation.WindowState = value;
            }
        }

        #endregion

        #region X

        /// <summary>
        /// Gets or sets the horizontal location of this window on the desktop.
        /// </summary>
        public int X
        {
            get
            {
                return implementation.X;
            }
            set
            {
                implementation.X = value;
            }
        }

        #endregion

        #region Y

        /// <summary>
        /// Gets or sets the vertical location of this window on the desktop.
        /// </summary>
        public int Y
        {
            get
            {
                return implementation.Y;
            }
            set
            {
                implementation.Y = value;
            }
        }

        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;
        
        /// <summary>
        /// Occurs whenever the mouse cursor leaves the window <see cref="Bounds"/>.
        /// </summary>
        public event EventHandler<EventArgs> MouseLeave;

        /// <summary>
        /// Occurs whenever the window is resized.
        /// </summary>
        public event EventHandler<EventArgs> Resize;

        /// <summary>
        /// Occurs when the <see cref="WindowState"/> property of the window changes.
        /// </summary>
        public event EventHandler<EventArgs> WindowStateChanged;

        public event EventHandler<TouchEventArgs> Gesture;

        public event EventHandler<ResolutionChange> ResolutionChange;
        #endregion

        #endregion

        #region --- IDisposable Members ---

        #region Dispose

        /// <summary>
        /// Releases all non-managed resources belonging to this NativeWindow.
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                Events = false;
                //if ((options & GameWindowFlags.Fullscreen) != 0)
                //{
                    //if (WindowState == WindowState.Fullscreen) WindowState = WindowState.Normal; // TODO: Revise.
                    //device.RestoreResolution();
                //}
                implementation.Dispose();
                GC.SuppressFinalize(this);

                IsDisposed = true;
            }
        }

        #endregion

        #endregion

        #region --- Protected Members ---

        #region Methods

        #region EnsureUndisposed

        /// <summary>
        /// Ensures that this NativeWindow has not been disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        /// If this NativeWindow has been disposed.
        /// </exception>
        protected void EnsureUndisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        #endregion

        #region IsDisposed

        /// <summary>
        /// Gets or sets a <see cref="System.Boolean"/>, which indicates whether
        /// this instance has been disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return disposed; }
            set { disposed = value; }
        }

        #endregion

        #region OnClosing

        /// <summary>
        /// Called when the NativeWindow is about to close.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs" /> for this event.
        /// Set e.Cancel to true in order to stop the NativeWindow from closing.</param>
        protected virtual void OnClosing(CancelEventArgs e)
        {
            if (Closing != null) Closing(this, e);
        }

        #endregion

        #region OnMouseLeave

        /// <summary>
        /// Called whenever the mouse cursor leaves the window <see cref="Bounds"/>.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnMouseLeave(EventArgs e)
        {
            if (MouseLeave != null) MouseLeave(this, e);
        }

        #endregion

        #region OnResize

        /// <summary>
        /// Called when the NativeWindow is resized.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnResize(EventArgs e)
        {
            if (Resize != null) Resize(this, e);
        }

        #endregion

        #region OnWindowStateChanged

        /// <summary>
        /// Called when the WindowState of this NativeWindow has changed.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnWindowStateChanged(EventArgs e)
        {
            if (WindowStateChanged != null) WindowStateChanged(this, e);
        }

        #endregion

        #endregion

        #endregion

        #region --- Private Members ---

        #region Methods

        #region OnClosingInternal

        private void OnClosingInternal(object sender, CancelEventArgs e) { OnClosing(e); }

        #endregion

        #region OnMouseLeaveInternal

        private void OnMouseLeaveInternal(object sender, EventArgs e) { OnMouseLeave(e); }

        #endregion

        #region OnResizeInternal

        private void OnResizeInternal(object sender, EventArgs e) { OnResize(e); }

        #endregion

        #region OnWindowStateChangedInternal

        private void OnWindowStateChangedInternal(object sender, EventArgs e) { OnWindowStateChanged(e); }

        #endregion

        #endregion

        #region Properties

        #region Events

        private bool Events
        {
            set
            {
                if (value)
                {
                    if (events)
                    {
                        throw new InvalidOperationException("Event propagation is already enabled.");
                    }
                    implementation.Closing += OnClosingInternal;
                    implementation.MouseLeave += OnMouseLeaveInternal;
                    implementation.Resize += OnResizeInternal;
                    implementation.WindowStateChanged += OnWindowStateChangedInternal;
                    implementation.Gesture += Gesture;
                    implementation.ResolutionChange+=ResolutionChange;
                    events = true;
                }
                else if (events)
                {
                    implementation.Closing -= OnClosingInternal;
                    implementation.MouseLeave -= OnMouseLeaveInternal;
                    implementation.Resize -= OnResizeInternal;
                    implementation.WindowStateChanged -= OnWindowStateChangedInternal;
                    implementation.Gesture -= Gesture;
                    implementation.ResolutionChange -= ResolutionChange;
                    events = false;
                }
            }
        }
        #endregion

        #endregion

        #endregion
    }

}
