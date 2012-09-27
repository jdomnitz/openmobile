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

        double target_render_periodSec = (1.0 / 66);
        double target_render_periodMS = (1000.0 / 66);
        double render_time;

        Stopwatch render_watch = new Stopwatch();
        Stopwatch render_ExecTime = new Stopwatch();
        Stopwatch sw = new Stopwatch(); 

        public double render_ExecTimeMS = 0;
        public double render_ExecTimeMS_Min = Double.MaxValue;
        public double render_ExecTimeMS_Max = 0;
        public double render_ExecTimeMSAvg = 0;
        private double render_ExecTimeMSTotal = 0;
        private int render_ExecTimeMSCount = 0;
        private double render_ExecTimeSec = 0.0;

        public int FPS = 0;
        public int FPS_Max = 0;
        public int FPS_Min = int.MaxValue;
        public bool FPS_MeasureMax = false;

        private bool _StopRendering = false;

        #endregion

        #region Contructors
        public void Initialize(bool VSync)
        {
            try
            {
                glContext = new GraphicsContext(GraphicsMode.Default, WindowInfo);
                glContext.MakeCurrent(WindowInfo);
                (glContext as IGraphicsContextInternal).LoadAll();
                Visible = true;
                if (!Platform.Factory.IsEmbedded)
                    OpenMobile.Graphics.OpenGL.Raw.ClearColor(OpenMobile.Graphics.Color.Black);
                SwapBuffers();
                glContext.VSync = VSync;
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                base.Dispose();
                Application.ShowError(WindowHandle, "OpenGL could not be initialized on this system.  Please ensure you have proper video drivers installed.", "OpenGL Support Failed");
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
        public void Run(double updates_per_second, double frames_per_second, bool VSync)
        {
            EnsureUndisposed();
            Initialize(VSync);
            OnLoadInternal(EventArgs.Empty);
            MakeCurrent();
            OnResize(EventArgs.Empty);

            // On some platforms, ProcessEvents() does not return while the user is resizing or moving
            // the window. We can avoid this issue by raising UpdateFrame and RenderFrame events
            // whenever we encounter a size or move event.

            target_render_periodSec = (1.0 / frames_per_second);
            target_render_periodMS = (target_render_periodSec * 1000);

            int ProcessDelay = 1;
            double MSBetweenRenderings = 1000 / updates_per_second;
            int LoopCount = (int)(MSBetweenRenderings / ProcessDelay);
            render_watch.Start();

            Stopwatch swIdle = new Stopwatch();

            while (true)
            {
                swIdle.Reset();
                swIdle.Start();
                while (true)
                {
                    ProcessEvents();
                    if (refresh || FPS_MeasureMax || swIdle.Elapsed.TotalMilliseconds >= MSBetweenRenderings)
                        break;
                    Thread.Sleep(1);
                }
                swIdle.Stop();
                //for (int i = 0; i < LoopCount; i++)
                //{
                //    ProcessEvents();
                //    if (refresh || FPS_MeasureMax)
                //        break;
                //    Thread.Sleep(ProcessDelay);
                //}
                refresh = false;
                if (Exists && !isExiting)
                {
                    if (Visible && !_StopRendering)
                        DispatchRenderFrame();
                }
                else
                    return;
                //if (render_time < target_render_periodMS)
                //    Thread.Sleep((int)(target_render_periodMS - render_time));
            }
        }
        public void DispatchRenderFrame()
        {
            RaiseRenderFrame();
        }

        public void FPS_Reset()
        {
            FPS_MeasureMax = false;
            accumulator = 0;
            idleCounter = 0;
            FPS = 0;
            FPS_Max = 0;
            FPS_Min = int.MaxValue;
            refresh = true;
        }

        double accumulator = 0;
        int idleCounter = 0;
        private void CalculateFPS()
        {
            sw.Stop();
            double milliseconds = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();

            idleCounter++;
            accumulator += milliseconds;
            if (accumulator > 1000)
            {
                FPS = idleCounter;
                accumulator -= 1000;
                idleCounter = 0; 
            }

            if (FPS > 0)
            {
                if (FPS > FPS_Max)
                    FPS_Max = FPS;
                if (FPS < FPS_Min)
                    FPS_Min = FPS;
            }
        }

        void RaiseRenderFrame()
        {
            //render_ExecTimeSec = target_render_periodSec - render_watch.Elapsed.TotalSeconds;
            //if (render_ExecTimeSec <= 0.0)
            {
                CalculateFPS();

                // Timing
                render_watch.Reset();
                render_watch.Start();
                //render_ExecTime.Reset();
                //render_ExecTime.Start();

                OnRenderFrameInternal();

                // Debug info
//                render_ExecTime.Stop();
//#if DEBUG
//                render_ExecTimeMS = render_ExecTime.Elapsed.TotalMilliseconds;
//                if (render_ExecTimeMS < render_ExecTimeMS_Min)
//                    render_ExecTimeMS_Min = render_ExecTimeMS;
//                if (render_ExecTimeMS > render_ExecTimeMS_Max)
//                    render_ExecTimeMS_Max = render_ExecTimeMS;
//                render_ExecTimeMSCount++;
//                render_ExecTimeMSTotal += render_ExecTimeMS;
//                if (render_ExecTimeMSCount > 100)
//                {
//                    render_ExecTimeMSCount = 0;
//                    render_ExecTimeMSTotal = 0;
//                }
//                render_ExecTimeMSAvg = render_ExecTimeMSTotal / render_ExecTimeMSCount;
//                //Console.WriteLine("Rendering screen {0}, FPS = {1}, RenderExecTime = {2}, RenderExecTimeAvf = {3}", screen, FPS, render_ExecTimeMS.ToString("#.##"), render_ExecTimeMSAvg.ToString("#.##"));
//#endif

                render_time = render_watch.Elapsed.TotalMilliseconds;

            }
#if DEBUG
            //else
            //    Debug.Print(DateTime.Now.ToString() + "-Frame Dropped ({0})!", render_ExecTimeSec);
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

        /// <summary>
        /// Prevents the rendering loop from running when true
        /// </summary>
        public bool StopRendering
        {
            get
            {
                return _StopRendering;
            }
            set
            {
                _StopRendering = value;
            }
        }

        #region Context

        /// <summary>
        /// Returns the opengl IGraphicsContext associated with the current GameWindow.
        /// </summary>
        public IGraphicsContext Context
        {
            get
            {
                return glContext;
            }
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// Gets the primary Keyboard device, or null if no Keyboard exists.
        /// </summary>
        public KeyboardDevice DefaultKeyboard
        {
            get 
            {
                if ((InputDriver == null) || (InputDriver.Keyboard == null))
                    return null;
                return InputDriver.Keyboard.Count > 0 ? InputDriver.Keyboard[0] : null; 
            }
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Gets the primary Mouse device, or null if no Mouse exists.
        /// </summary>
        public MouseDevice DefaultMouse
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
                //Debug.Print("Updating Context after setting WindowState to {0}", value);

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
            if (RenderFrame != null)
                RenderFrame(this, e);
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

        private static object LockObject = new object();
        public void OnRenderFrameInternal()
        {
            if (Exists && !isExiting)
            {
                lock (LockObject)
                {
                    MakeCurrent(); //switch context
                    OnRenderFrame(EventArgs.Empty);
                    MakeCurrent(null); //release context
                }
            }
        }

        #endregion

        #endregion
    }
}