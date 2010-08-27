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

        private readonly GameWindowFlags options;

        private readonly DisplayDevice device;

        private INativeWindow implementation;

        private bool disposed, events;

        #endregion
        public NativeWindow()
        {
            this.options = GameWindowFlags.Default;
            this.device = DisplayDevice.Default;
        }
        #region --- Contructors ---
        public void NativeInitialize()
        {
            implementation = Factory.Default.CreateNativeWindow(device.Bounds.X + (device.Bounds.Width - 720) / 2, device.Bounds.Y + (device.Bounds.Height - 450) / 2, 720, 450, "OpenMobile Native Window", GraphicsMode.Default,GameWindowFlags.Default, DisplayDevice.Default);
            implementation.Visible = false;
            

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                if (Environment.GetCommandLineArgs()[1].ToLower() == "-fullscreen")
                    WindowState = WindowState.Fullscreen;
                else if (Environment.GetCommandLineArgs()[1].ToLower().StartsWith("-size=") == true)
                {
                    string[] part = Environment.GetCommandLineArgs()[1].Substring(6).Split(new char[] { 'x' });
                    try
                    {
                        Size = new Size(int.Parse(part[0]), int.Parse(part[1]));
                    }
                    catch (ArgumentException) {}
                }
            }
            currentThread = Thread.CurrentThread;
        }

        Thread currentThread;
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
            ProcessEvents(false);
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
                EnsureUndisposed();
                return implementation.Bounds;
            }
            set
            {
                EnsureUndisposed();
                implementation.Bounds = value;
            }
        }

        #endregion

        public IntPtr WindowHandle
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
                EnsureUndisposed();
                return implementation.ClientRectangle;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
                return implementation.ClientSize;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
                return implementation.Height;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
                implementation.Icon = value;
            }
        }

        #endregion

        #region InputDriver

        /// <summary>
        /// This property is deprecated.
        /// </summary>
        [Obsolete]
        public IInputDriver InputDriver
        {
            get
            {
                EnsureUndisposed();
                return implementation.InputDriver;
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
                EnsureUndisposed();
                return implementation.Location;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
                return implementation.Size;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
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
                EnsureUndisposed();
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
        public void Invoke(Delegate function)
        {
            queuedInvokes.Add(function);
        }
        public bool InvokeRequired
        {
            get
            {
                return (Thread.CurrentThread != currentThread);
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
                EnsureUndisposed();
                return implementation.Width;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
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
                EnsureUndisposed();
                return implementation.X;
            }
            set
            {
                EnsureUndisposed();
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
                EnsureUndisposed();
                return implementation.Y;
            }
            set
            {
                EnsureUndisposed();
                implementation.Y = value;
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs after the window has closed.
        /// </summary>
        public event EventHandler<EventArgs> Closed;

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;

        /// <summary>
        /// Occurs whenever a character is typed.
        /// </summary>
        public event EventHandler<KeyPressEventArgs> KeyPress;

        /// <summary>
        /// Occurs whenever the mouse cursor enters the window <see cref="Bounds"/>.
        /// </summary>
        public event EventHandler<EventArgs> MouseEnter;
        
        /// <summary>
        /// Occurs whenever the mouse cursor leaves the window <see cref="Bounds"/>.
        /// </summary>
        public event EventHandler<EventArgs> MouseLeave;

        /// <summary>
        /// Occurs whenever the window is resized.
        /// </summary>
        public event EventHandler<EventArgs> Resize;

        /// <summary>
        /// Occurs when the <see cref="WindowBorder"/> property of the window changes.
        /// </summary>
        public event EventHandler<EventArgs> WindowBorderChanged;

        /// <summary>
        /// Occurs when the <see cref="WindowState"/> property of the window changes.
        /// </summary>
        public event EventHandler<EventArgs> WindowStateChanged;

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
                if ((options & GameWindowFlags.Fullscreen) != 0)
                {
                    //if (WindowState == WindowState.Fullscreen) WindowState = WindowState.Normal; // TODO: Revise.
                    device.RestoreResolution();
                }
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

        #region OnClosed

        /// <summary>
        /// Called when the NativeWindow has closed.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null) Closed(this, e);
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

        #region OnKeyPress

        /// <summary>
        /// Called when a character is typed.
        /// </summary>
        /// <param name="e">The <see cref="OpenMobile.KeyPressEventArgs"/> for this event.</param>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (KeyPress != null) KeyPress(this, e);
        }

        #endregion

        #region OnMouseEnter

        /// <summary>
        /// Called whenever the mouse cursor reenters the window <see cref="Bounds"/>.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnMouseEnter(EventArgs e)
        {
            if (MouseEnter != null) MouseEnter(this, e);
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

        #region OnWindowBorderChanged

        /// <summary>
        /// Called when the WindowBorder of this NativeWindow has changed.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnWindowBorderChanged(EventArgs e)
        {
            if (WindowBorderChanged != null) WindowBorderChanged(this, e);
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

        #region ProcessEvents
        public List<Delegate> queuedInvokes = new List<Delegate>();
        /// <summary>
        /// Processes operating system events until the NativeWindow becomes idle.
        /// </summary>
        /// <param name="retainEvents">If true, the state of underlying system event propagation will be preserved, otherwise event propagation will be enabled if it has not been already.</param>
        protected void ProcessEvents(bool retainEvents)
        {
            EnsureUndisposed();
            if (!retainEvents && !events) Events = true;
            implementation.ProcessEvents();
            if (queuedInvokes.Count > 0)
            {
                for (int i = queuedInvokes.Count-1; i >=0; i--)
                {
                    Delegate d = queuedInvokes[i];
                    try
                    {
                        if (d!=null)
                            d.Method.Invoke(d.Target, null);
                    }
                    catch (TargetInvocationException) { }
                    queuedInvokes.RemoveAt(i);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region --- Private Members ---

        #region Methods

        #region OnClosedInternal

        private void OnClosedInternal(object sender, EventArgs e)
        {
            OnClosed(e);
            Events = false;
        }

        #endregion

        #region OnClosingInternal

        private void OnClosingInternal(object sender, CancelEventArgs e) { OnClosing(e); }

        #endregion

        #region OnKeyPressInternal

        private void OnKeyPressInternal(object sender, KeyPressEventArgs e) { OnKeyPress(e); }

        #endregion

        #region OnMouseEnterInternal

        private void OnMouseEnterInternal(object sender, EventArgs e) { OnMouseEnter(e); }

        #endregion

        #region OnMouseLeaveInternal

        private void OnMouseLeaveInternal(object sender, EventArgs e) { OnMouseLeave(e); }

        #endregion

        #region OnResizeInternal

        private void OnResizeInternal(object sender, EventArgs e) { OnResize(e); }

        #endregion

        #region OnWindowBorderChangedInternal

        private void OnWindowBorderChangedInternal(object sender, EventArgs e) { OnWindowBorderChanged(e); }

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
                    implementation.Closed += OnClosedInternal;
                    implementation.Closing += OnClosingInternal;
                    implementation.KeyPress += OnKeyPressInternal;
                    implementation.MouseEnter += OnMouseEnterInternal;
                    implementation.MouseLeave += OnMouseLeaveInternal;
                    implementation.Resize += OnResizeInternal;
                    implementation.WindowBorderChanged += OnWindowBorderChangedInternal;
                    implementation.WindowStateChanged += OnWindowStateChangedInternal;
                    events = true;
                }
                else if (events)
                {
                    implementation.Closed -= OnClosedInternal;
                    implementation.Closing -= OnClosingInternal;
                    implementation.KeyPress -= OnKeyPressInternal;
                    implementation.MouseEnter -= OnMouseEnterInternal;
                    implementation.MouseLeave -= OnMouseLeaveInternal;
                    implementation.Resize -= OnResizeInternal;
                    implementation.WindowBorderChanged -= OnWindowBorderChangedInternal;
                    implementation.WindowStateChanged -= OnWindowStateChangedInternal;
                    events = false;
                }
                else
                {
                    throw new InvalidOperationException("Event propagation is already disabled.");
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }

}
