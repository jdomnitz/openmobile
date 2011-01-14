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
    public class GameWindow : NativeWindow, IDisposable
    {
        #region --- Fields ---

        IGraphicsContext glContext;

        bool isExiting;

        const double target_render_period=(1.0/35);
        double render_time;

        Stopwatch render_watch = new Stopwatch();

        #endregion

        #region Contructors
        public void Initialize()
        {
            try
            {
                glContext = new GraphicsContext(GraphicsMode.Default,WindowInfo);
                glContext.MakeCurrent(WindowInfo);
                (glContext as IGraphicsContextInternal).LoadAll();
                Visible = true;
                if (!Platform.Factory.IsEmbedded)
                    OpenMobile.Graphics.OpenGL.Raw.ClearColor(OpenMobile.Graphics.Color.Black);
                SwapBuffers();
                glContext.VSync = true;
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                base.Dispose();
                throw;
            }
        }
        #endregion

        #region Public Members

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
            if ((!Context.IsCurrent) && (info == null))
                return;
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
                isExiting = true;
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
        public bool refresh;
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
            OnLoadInternal(EventArgs.Empty);
            MakeCurrent();
            OnResize(EventArgs.Empty);
            
            // On some platforms, ProcessEvents() does not return while the user is resizing or moving
            // the window. We can avoid this issue by raising UpdateFrame and RenderFrame events
            // whenever we encounter a size or move event.

            Debug.Print("Entering main loop.");
            render_watch.Start();
            while (true)
            {
                for (int i = 0; i < 29; i++)
                {
                    ProcessEvents();
                    if (refresh)
                        break;
                    Thread.Sleep(15);
                }
                refresh = false;
                if (Exists && !isExiting)
                    DispatchRenderFrame();
                else
                    return;
                if (render_time<34)
                    Thread.Sleep((int)(34-render_time));
            }
        }
        public void DispatchRenderFrame()
        {
            RaiseRenderFrame();
        }

        void RaiseRenderFrame()
        {
            if ((target_render_period - render_watch.Elapsed.TotalSeconds) <= 0.0)
            {
                render_watch.Reset();
                render_watch.Start();
                
                OnRenderFrameInternal();
                render_time = render_watch.Elapsed.TotalMilliseconds;
            }
            #if DEBUG
            else
                Debug.Print(DateTime.Now.ToString()+"-Frame Dropped!");
            #endif
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

        #region Keyboard

        /// <summary>
        /// Gets the primary Keyboard device, or null if no Keyboard exists.
        /// </summary>
        public IList<KeyboardDevice> Keyboard
        {
            get { return InputDriver.Keyboard; }
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
        public event EventHandler<EventArgs> RenderFrame;

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
        protected virtual void OnRenderFrame(EventArgs e)
        {
            if (RenderFrame != null) RenderFrame(this, e);
        }

        #endregion

        #region OnResize

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            glContext.Update(base.WindowInfo);
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

        public void OnRenderFrameInternal()
        {
            if (Exists && !isExiting)
            {
                MakeCurrent(); //switch context
                OnRenderFrame(EventArgs.Empty);
                MakeCurrent(null); //release context
            }
        }

        #endregion

        #endregion
    }
}