﻿#region License
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using OpenMobile.Graphics;
using OpenMobile.Input;
using OpenMobile.Platform;

namespace OpenMobile
{
    /// <summary>
    /// The GameWindow class contains cross-platform methods to create and render on an OpenGL
    /// window, handle input and load resources.
    /// </summary>
    /// <remarks>
    /// GameWindow contains several events you can hook or override to add your custom logic:
    /// <list>
    /// <item>
    /// OnLoad: Occurs after creating the OpenGL context, but before entering the main loop.
    /// Override to load resources.
    /// </item>
    /// <item>
    /// OnUnload: Occurs after exiting the main loop, but before deleting the OpenGL context.
    /// Override to unload resources.
    /// </item>
    /// <item>
    /// OnResize: Occurs whenever GameWindow is resized. You should update the OpenGL Viewport
    /// and Projection Matrix here.
    /// </item>
    /// <item>
    /// OnUpdateFrame: Occurs at the specified logic update rate. Override to add your game
    /// logic.
    /// </item>
    /// <item>
    /// OnRenderFrame: Occurs at the specified frame render rate. Override to add your
    /// rendering code.
    /// </item>
    /// </list>
    /// Call the Run() method to start the application's main loop. Run(double, double) takes two
    /// parameters that
    /// specify the logic update rate, and the render update rate.
    /// </remarks>
    public class GameWindow : NativeWindow, IGameWindow, IDisposable
    {
        #region --- Fields ---

        object exit_lock = new object();

        IGraphicsContext glContext;

        bool isExiting = false;

        double render_period;
        double target_render_period;
        // TODO: Implement these:
        double render_time;
        VSyncMode vsync;

        Stopwatch render_watch = new Stopwatch();
        double next_render = 0.0;
        FrameEventArgs render_args = new FrameEventArgs();

        #endregion

        #region --- Contructors ---

        #region public GameWindow()

        /// <summary>Constructs a new GameWindow with sensible default attributes.</summary>
        public GameWindow()
            : this(640, 480, GraphicsMode.Default, "OpenMobile Game Window", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameWindow(int width, int height)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        public GameWindow(int width, int height)
            : this(width, height, GraphicsMode.Default, "OpenMobile Game Window", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        public GameWindow(int width, int height, GraphicsMode mode)
            : this(width, height, mode, "OpenMobile Game Window", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode, string title)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        /// <param name="title">The title of the GameWindow.</param>
        public GameWindow(int width, int height, GraphicsMode mode, string title)
            : this(width, height, mode, title, 0, DisplayDevice.Default) { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        /// <param name="title">The title of the GameWindow.</param>
        /// <param name="options">GameWindow options regarding window appearance and behavior.</param>
        public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options)
            : this(width, height, mode, title, options, DisplayDevice.Default) { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        /// <param name="title">The title of the GameWindow.</param>
        /// <param name="options">GameWindow options regarding window appearance and behavior.</param>
        /// <param name="device">The OpenMobile.Graphics.DisplayDevice to construct the GameWindow in.</param>
        public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)
            : this(width, height, mode, title, options, device, 1, 0, GraphicsContextFlags.Default)
        { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        /// <param name="title">The title of the GameWindow.</param>
        /// <param name="options">GameWindow options regarding window appearance and behavior.</param>
        /// <param name="device">The OpenMobile.Graphics.DisplayDevice to construct the GameWindow in.</param>
        /// <param name="major">The major version for the OpenGL GraphicsContext.</param>
        /// <param name="minor">The minor version for the OpenGL GraphicsContext.</param>
        /// <param name="flags">The GraphicsContextFlags version for the OpenGL GraphicsContext.</param>
        public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device,
            int major, int minor, GraphicsContextFlags flags)
            : this(width, height, mode, title, options, device, major, minor, flags, null)
        { }

        #endregion

        #region public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags, IGraphicsContext sharedContext)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param name="width">The width of the GameWindow in pixels.</param>
        /// <param name="height">The height of the GameWindow in pixels.</param>
        /// <param name="mode">The OpenMobile.Graphics.GraphicsMode of the GameWindow.</param>
        /// <param name="title">The title of the GameWindow.</param>
        /// <param name="options">GameWindow options regarding window appearance and behavior.</param>
        /// <param name="device">The OpenMobile.Graphics.DisplayDevice to construct the GameWindow in.</param>
        /// <param name="major">The major version for the OpenGL GraphicsContext.</param>
        /// <param name="minor">The minor version for the OpenGL GraphicsContext.</param>
        /// <param name="flags">The GraphicsContextFlags version for the OpenGL GraphicsContext.</param>
        /// <param name="sharedContext">An IGraphicsContext to share resources with.</param>
        public GameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device,
                          int major, int minor, GraphicsContextFlags flags, IGraphicsContext sharedContext)
            : base(width, height, title, options,
                   mode == null ? GraphicsMode.Default : mode,
                   device == null ? DisplayDevice.Default : device)
        {
            // :)
        }

        #endregion
        public void Initialize()
        {
            try
            {
                glContext = new GraphicsContext(GraphicsMode.Default,WindowInfo);
                glContext.MakeCurrent(WindowInfo);
                (glContext as IGraphicsContextInternal).LoadAll();

                VSync = VSyncMode.Adaptive;

                //glWindow.WindowInfoChanged += delegate(object sender, EventArgs e) { OnWindowInfoChangedInternal(e); };
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                base.Dispose();
                throw;
            }
        }
        #endregion

        #region --- Public Members ---

        #region Methods

        #region Dispose

        /// <summary>
        /// Disposes of the GameWindow, releasing all resources consumed by it.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                try
                {
                    if (glContext != null)
                    {
                        glContext.Dispose();
                        glContext = null;
                    }
                }
                finally
                {
                    base.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Exit

        /// <summary>
        /// Closes the GameWindow. Equivalent to <see cref="NativeWindow.Close"/> method.
        /// </summary>
        /// <remarks>
        /// <para>Override if you are not using <see cref="GameWindow.Run()"/>.</para>
        /// <para>If you override this method, place a call to base.Exit(), to ensure proper OpenMobile shutdown.</para>
        /// </remarks>
        public virtual void Exit()
        {
            if (!IsDisposed)
                Close();
        }

        #endregion

        #region MakeCurrent

        /// <summary>
        /// Makes the GraphicsContext current on the calling thread.
        /// </summary>
        public void MakeCurrent()
        {
            EnsureUndisposed();
            Context.MakeCurrent(WindowInfo);
        }
        public void MakeCurrent(IWindowInfo info)
        {
            EnsureUndisposed();
            Context.MakeCurrent(info);
        }
        #endregion

        #region OnClose

        /// <summary>
        /// Called when the NativeWindow is about to close.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs" /> for this event.
        /// Set e.Cancel to true in order to stop the GameWindow from closing.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
            {
                isExiting = true;
                OnUnloadInternal(EventArgs.Empty);
            }
        }


        #endregion

        #region OnLoad

        /// <summary>
        /// Called after an OpenGL context has been established, but before entering the main loop.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnLoad(EventArgs e)
        {
            if (Load != null) Load(this, e);
        }

        #endregion

        #region OnUnload

        /// <summary>
        /// Called after GameWindow.Exit was called, but before destroying the OpenGL context.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnUnload(EventArgs e)
        {
            if (Unload != null) Unload(this, e);
        }

        #endregion

        #region public void Run()

        /// <summary>
        /// Enters the game loop of the GameWindow using the maximum update rate.
        /// </summary>
        /// <seealso cref="Run(double)"/>
        public void Run()
        {
            Run(0.0, 0.0);
        }

        #endregion

        #region public void Run(double updateFrequency)

        /// <summary>
        /// Enters the game loop of the GameWindow using the specified update rate.
        /// maximum possible render frequency.
        /// </summary>

        #endregion

        #region public void Run(double updates_per_second, double frames_per_second)

        /// <summary>
        /// Enters the game loop of the GameWindow updating and rendering at the specified frequency.
        /// </summary>
        /// <remarks>
        /// When overriding the default game loop you should call ProcessEvents()
        /// to ensure that your GameWindow responds to operating system events.
        /// <para>
        /// Once ProcessEvents() returns, it is time to call update and render the next frame.
        /// </para>
        /// </remarks>
        /// <param name="updates_per_second">The frequency of UpdateFrame events.</param>
        /// <param name="frames_per_second">The frequency of RenderFrame events.</param>
        public void Run(double updates_per_second, double frames_per_second)
        {
            EnsureUndisposed();
            Initialize();
            try
            {
                if (frames_per_second < 0.0 || frames_per_second > 200.0)
                    throw new ArgumentOutOfRangeException("frames_per_second", frames_per_second,
                                                          "Parameter should be inside the range [0.0, 200.0]");

                TargetRenderFrequency = 30;//frames_per_second;

                Visible = true;   // Make sure the GameWindow is visible.
                OnLoadInternal(EventArgs.Empty);
                OnResize(EventArgs.Empty);

                // On some platforms, ProcessEvents() does not return while the user is resizing or moving
                // the window. We can avoid this issue by raising UpdateFrame and RenderFrame events
                // whenever we encounter a size or move event.
                // Note: hack disabled. Threaded rendering isprovides a better solution to this issue.
                //Move += DispatchUpdateAndRenderFrame;
                //Resize += DispatchUpdateAndRenderFrame;

                Debug.Print("Entering main loop.");
                render_watch.Start();
                while (true)
                {
                    ProcessEvents();
                    if (Exists && !IsExiting)
                        DispatchUpdateAndRenderFrame(this, EventArgs.Empty);
                    else
                        return;
                    Thread.Sleep(10);
                }
            }
            finally
            {
                Move -= DispatchUpdateAndRenderFrame;
                Resize -= DispatchUpdateAndRenderFrame;

                //if (Exists)
                {
                    // TODO: Should similar behaviour be retained, possibly on native window level?
                    //while (this.Exists)
                    //    ProcessEvents(false);
                }
            }
        }
        void DispatchUpdateAndRenderFrame(object sender, EventArgs e)
        {
            RaiseRenderFrame(render_watch, ref next_render, render_args);
        }

        void RaiseRenderFrame(Stopwatch render_watch, ref double next_render, FrameEventArgs render_args)
        {
            // Cap the maximum time drift to 1 second (e.g. when the process is suspended).
            double time = render_watch.Elapsed.TotalSeconds;
            if (time > 1.0)
                time = 1.0;
            if (time <= 0)
                return;
            double time_left = next_render - time;

            if (time_left <= 0.0)
            {
                // Schedule next render event. The 1 second cap ensures
                // the process does not appear to hang.
                next_render = time_left + TargetRenderPeriod;
                if (next_render < -1.0)
                    next_render = -1.0;

                render_watch.Reset();
                render_watch.Start();

                if (time > 0)
                {
                    // Todo: revisit this code. Maybe check average framerate instead?
                    // Note: VSyncMode.Adaptive enables vsync by default. The code below
                    // is supposed to disable vsync if framerate becomes too low (half of target
                    // framerate in the current approach) and reenable once the framerate
                    // rises again.
                    // Note 2: calling Context.VSync = true repeatedly seems to cause jitter on
                    // some configurations. If possible, we should avoid repeated calls.
                    // Note 3: we may not read/write the VSync property without a current context.
                    // This may come to pass if the user has moved rendering to his own thread.
                    MakeCurrent();
                    if (Context.IsCurrent && VSync == VSyncMode.Adaptive)
                    {
                        // Check if we have enough time for a vsync
                        if (RenderTime > 2.0 * TargetRenderPeriod)
                            Context.VSync = false;
                        else
                            Context.VSync = true;
                    }
                    render_period = render_args.Time = time;
                    OnRenderFrameInternal(render_args);
                    MakeCurrent(null);
                    render_time = render_watch.Elapsed.TotalSeconds;
                }
            }
        }

        #endregion

        #region SwapBuffers

        /// <summary>
        /// Swaps the front and back buffer, presenting the rendered scene to the user.
        /// </summary>
        public void SwapBuffers()
        {
            EnsureUndisposed();
            this.Context.SwapBuffers();
        }

        #endregion

        #endregion

        #region Properties

        #region Context

        /// <summary>
        /// Returns the opengl IGraphicsContext associated with the current GameWindow.
        /// </summary>
        public IGraphicsContext Context
        {
            get
            {
                EnsureUndisposed();
                return glContext;
            }
        }

        #endregion

        #region IsExiting

        /// <summary>
        /// Gets a value indicating whether the shutdown sequence has been initiated
        /// for this window, by calling GameWindow.Exit() or hitting the 'close' button.
        /// If this property is true, it is no longer safe to use any OpenMobile.Input or
        /// OpenMobile.Graphics.OpenGL functions or properties.
        /// </summary>
        public bool IsExiting
        {
            get
            {
                EnsureUndisposed();
                return isExiting;
            }
        }

        #endregion

        #region Joysticks

        /// <summary>
        /// Gets a readonly IList containing all available OpenMobile.Input.JoystickDevices.
        /// </summary>
        public IList<JoystickDevice> Joysticks
        {
            get { return InputDriver.Joysticks; }
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// Gets the primary Keyboard device, or null if no Keyboard exists.
        /// </summary>
        public KeyboardDevice Keyboard
        {
            get { return InputDriver.Keyboard.Count > 0 ? InputDriver.Keyboard[0] : null; }
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Gets the primary Mouse device, or null if no Mouse exists.
        /// </summary>
        public MouseDevice Mouse
        {
            get { return InputDriver.Mouse.Count > 0 ? InputDriver.Mouse[0] : null; }
        }

        #endregion

        #region --- GameWindow Timing ---

        // TODO: Disabled because it is not reliable enough. Use vsync as a workaround.

        //#region public bool AllowSleep

        //public bool AllowSleep
        //{
        //    get { return allow_sleep; }
        //    set { allow_sleep = value; }
        //}

        //#endregion

        #region RenderFrequency

        /// <summary>
        /// Gets a double representing the actual frequency of RenderFrame events, in hertz (i.e. fps or frames per second).
        /// </summary>
        public double RenderFrequency
        {
            get
            {
                EnsureUndisposed();
                if (render_period == 0.0)
                    return 1.0;
                return 1.0 / render_period;
            }
        }

        #endregion

        #region RenderPeriod

        /// <summary>
        /// Gets a double representing the period of RenderFrame events, in seconds.
        /// </summary>
        public double RenderPeriod
        {
            get
            {
                EnsureUndisposed();
                return render_period;
            }
        }

        #endregion

        #region RenderTime

        /// <summary>
        /// Gets a double representing the time spent in the RenderFrame function, in seconds.
        /// </summary>
        public double RenderTime
        {
            get
            {
                EnsureUndisposed();
                return render_time;
            }
            protected set
            {
                EnsureUndisposed();
                render_time = value;
            }
        }

        #endregion

        #region TargetRenderFrequency

        /// <summary>
        /// Gets or sets a double representing the target render frequency, in hertz.
        /// </summary>
        /// <remarks>
        /// <para>A value of 0.0 indicates that RenderFrame events are generated at the maximum possible frequency (i.e. only limited by the hardware's capabilities).</para>
        /// <para>Values lower than 1.0Hz are clamped to 1.0Hz. Values higher than 200.0Hz are clamped to 200.0Hz.</para>
        /// </remarks>
        public double TargetRenderFrequency
        {
            get
            {
                EnsureUndisposed();
                if (TargetRenderPeriod == 0.0)
                    return 0.0;
                return 1.0 / TargetRenderPeriod;
            }
            set
            {
                EnsureUndisposed();
                if (value < 1.0)
                {
                    TargetRenderPeriod = 0.0;
                }
                else if (value <= 200.0)
                {
                    TargetRenderPeriod = 1.0 / value;
                }
                else Debug.Print("Target render frequency clamped to 200.0Hz."); // TODO: Where is it actually performed?
            }
        }

        #endregion

        #region TargetRenderPeriod

        /// <summary>
        /// Gets or sets a double representing the target render period, in seconds.
        /// </summary>
        /// <remarks>
        /// <para>A value of 0.0 indicates that RenderFrame events are generated at the maximum possible frequency (i.e. only limited by the hardware's capabilities).</para>
        /// <para>Values lower than 0.005 seconds (200Hz) are clamped to 0.0. Values higher than 1.0 seconds (1Hz) are clamped to 1.0.</para>
        /// </remarks>
        public double TargetRenderPeriod
        {
            get
            {
                EnsureUndisposed();
                return target_render_period;
            }
            set
            {
                EnsureUndisposed();
                if (value <= 0.005)
                {
                    target_render_period = 0.0;
                }
                else if (value <= 1.0)
                {
                    target_render_period = value;
                }
                else Debug.Print("Target render period clamped to 1.0 seconds.");
            }
        }

        #endregion

        #endregion

        #region VSync

        /// <summary>
        /// Gets or sets the VSyncMode.
        /// </summary>
        public VSyncMode VSync
        {
            get
            {
                EnsureUndisposed();
                GraphicsContext.Assert();
                return vsync;
            }
            set
            {
                EnsureUndisposed();
                GraphicsContext.Assert();
                Context.VSync = (vsync = value) != VSyncMode.Off;
            }
        }

        #endregion

        #region WindowState

        /// <summary>
        /// Gets or states the state of the NativeWindow.
        /// </summary>
        public override WindowState WindowState
        {
            get
            {
                return base.WindowState;
            }
            set
            {
                base.WindowState = value;
                Debug.Print("Updating Context after setting WindowState to {0}", value);

                if (Context != null)
                    Context.Update(WindowInfo);
            }
        }
        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs before the window is displayed for the first time.
        /// </summary>
        public event EventHandler<EventArgs> Load;

        /// <summary>
        /// Occurs when it is time to render a frame.
        /// </summary>
        public event EventHandler<FrameEventArgs> RenderFrame;

        /// <summary>
        /// Occurs before the window is destroyed.
        /// </summary>
        public event EventHandler<EventArgs> Unload;

        #endregion

        #endregion

        #region --- Protected Members ---

        #region Dispose

        /// <summary>
        /// Override to add custom cleanup logic.
        /// </summary>
        /// <param name="manual">True, if this method was called by the application; false if this was called by the finalizer thread.</param>
        protected virtual void Dispose(bool manual) { }

        #endregion

        #region OnRenderFrame

        /// <summary>
        /// Called when the frame is rendered.
        /// </summary>
        /// <param name="e">Contains information necessary for frame rendering.</param>
        /// <remarks>
        /// Subscribe to the <see cref="RenderFrame"/> event instead of overriding this method.
        /// </remarks>
        protected virtual void OnRenderFrame(FrameEventArgs e)
        {
            if (RenderFrame != null) RenderFrame(this, e);
        }

        #endregion

        #region OnWindowInfoChanged

        /// <summary>
        /// Called when the WindowInfo for this GameWindow has changed.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected virtual void OnWindowInfoChanged(EventArgs e) { }

        #endregion

        #region OnResize

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            MakeCurrent();
            glContext.Update(base.WindowInfo);
            MakeCurrent(null);
        }

        #endregion

        #endregion

        #region --- Private Members ---

        #region OnLoadInternal

        private void OnLoadInternal(EventArgs e)
        {
            OnLoad(e);
        }

        #endregion

        #region OnRenderFrameInternal

        private void OnRenderFrameInternal(FrameEventArgs e) { if (Exists && !isExiting) OnRenderFrame(e); }

        #endregion

        #region OnUnloadInternal

        private void OnUnloadInternal(EventArgs e) { OnUnload(e); }

        #endregion

        #region OnWindowInfoChangedInternal

        private void OnWindowInfoChangedInternal(EventArgs e)
        {
            glContext.MakeCurrent(WindowInfo);

            OnWindowInfoChanged(e);
            glContext.MakeCurrent(null);
        }

        #endregion

        #endregion
    }

    #region public enum VSyncMode

    /// <summary>
    /// Enumerates available VSync modes.
    /// </summary>
    public enum VSyncMode
    {
        /// <summary>
        /// Vsync disabled.
        /// </summary>
        Off = 0,
        /// <summary>
        /// VSync enabled.
        /// </summary>
        On,
        /// <summary>
        /// VSync enabled, unless framerate falls below one half of target framerate.
        /// If no target framerate is specified, this behaves exactly like <see cref="VSyncMode.On"/>.
        /// </summary>
        Adaptive,
    }

    #endregion
}