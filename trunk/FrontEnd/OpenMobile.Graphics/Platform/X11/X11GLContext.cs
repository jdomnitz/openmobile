#region --- License ---
/* Copyright (c) 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion
#if LINUX
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using OpenMobile.Graphics;

namespace OpenMobile.Platform.X11
{
    /// \internal
    /// <summary>
    /// Provides methods to create and control an opengl context on the X11 platform.
    /// This class supports OpenMobile, and is not intended for use by OpenMobile programs.
    /// </summary>
    internal sealed class X11GLContext : DesktopGraphicsContext
    {
        #region Fields

        // We assume that we cannot move a GL context to a different display connection.
        // For this reason, we'll "lock" onto the display of the window used in the context
        // constructor and we'll throw an exception if the user ever tries to make the context
        // current on window originating from a different display.
        IntPtr display;
        X11WindowInfo currentWindow;
        bool vsync_supported;
        int vsync_interval;
        bool glx_loaded;

        #endregion

        #region --- Constructors ---

        public X11GLContext(GraphicsMode mode, IWindowInfo window, IGraphicsContext shared, bool direct,
            int major, int minor, GraphicsContextFlags flags)
        {
            if (mode == null)
                throw new ArgumentNullException("mode");
            if (window == null)
                throw new ArgumentNullException("window");

            Mode = mode;
            
            currentWindow = (X11WindowInfo)window;
            // Do not move this lower, as almost everything requires the Display
            // property to be correctly set.
            Display = currentWindow.Display;
            
            currentWindow.VisualInfo = SelectVisual(mode, currentWindow);
            
            ContextHandle shareHandle = shared != null ?
                (shared as IGraphicsContextInternal).Context : (ContextHandle)IntPtr.Zero;
            
            Debug.Write("Creating X11GLContext context: ");
            Debug.Write(direct ? "direct, " : "indirect, ");
            Debug.WriteLine(shareHandle.Handle == IntPtr.Zero ? "not shared... " :
                String.Format("shared with ({0})... ", shareHandle));
            
            if (!glx_loaded)
            {
                Debug.WriteLine("Creating temporary context to load GLX extensions.");
                
                // Create a temporary context to obtain the necessary function pointers.
                XVisualInfo visual = currentWindow.VisualInfo;
                IntPtr ctx = IntPtr.Zero;

                using (new XLock(Display))
                {
                    ctx = Glx.CreateContext(Display, ref visual, IntPtr.Zero, true);
                    if (ctx == IntPtr.Zero)
                        ctx = Glx.CreateContext(Display, ref visual, IntPtr.Zero, false);
                }
                
                if (ctx != IntPtr.Zero)
                {
                    new Glx().LoadEntryPoints();
                    using (new XLock(Display))
                    {
                        Glx.MakeCurrent(Display, IntPtr.Zero, IntPtr.Zero);
                        //Glx.DestroyContext(Display, ctx);
                    }
                    glx_loaded = true;
                }
            }
            
            // Try using the new context creation method. If it fails, fall back to the old one.
            // For each of these methods, we try two times to create a context:
            // one with the "direct" flag intact, the other with the flag inversed.
            // HACK: It seems that Catalyst 9.1 - 9.4 on Linux have problems with contexts created through
            // GLX_ARB_create_context, including hideous input lag, no vsync and other. Use legacy context
            // creation if the user doesn't request a 3.0+ context.
            if ((major >= 3) && Glx.Delegates.glXCreateContextAttribsARB != null)
            {
                Debug.Write("Using GLX_ARB_create_context... ");
                
                unsafe
                {
                    // We need the FB config for the current GraphicsMode.
                    int count;
                    IntPtr* fbconfigs = Glx.ChooseFBConfig(Display, currentWindow.Screen,
                        new int[] {
                        (int)GLXAttribute.VISUAL_ID,
                        (int)mode.Index,
                        0
                    }, out count);
                    
                    if (count > 0)
                    {
                        List<int> attributes = new List<int>();
                        attributes.Add((int)ArbCreateContext.MajorVersion);
                        attributes.Add(major);
                        attributes.Add((int)ArbCreateContext.MinorVersion);
                        attributes.Add(minor);
                        if (flags != 0)
                        {
                            attributes.Add((int)ArbCreateContext.Flags);
                            attributes.Add((int)flags);
                        }
                        // According to the docs, " <attribList> specifies a list of attributes for the context.
                        // The list consists of a sequence of <name,value> pairs terminated by the
                        // value 0. [...]"
                        // Is this a single 0, or a <0, 0> pair? (Defensive coding: add two zeroes just in case).
                        attributes.Add(0);
                        attributes.Add(0);

                        using (new XLock(Display))
                        {
                            Handle = new ContextHandle(Glx.Arb.CreateContextAttribs(Display, *fbconfigs,
                                    shareHandle.Handle, direct, attributes.ToArray()));

                            if (Handle == ContextHandle.Zero)
                            {
                                Debug.Write(String.Format("failed. Trying direct: {0}... ", !direct));
                                Handle = new ContextHandle(Glx.Arb.CreateContextAttribs(Display, *fbconfigs,
                                        shareHandle.Handle, !direct, attributes.ToArray()));
                            }
                        }
                        
                        if (Handle == ContextHandle.Zero)
                            Debug.WriteLine("failed.");
                        else
                            Debug.WriteLine("success!");
                        
                        using (new XLock(Display))
                        {
                            Functions.XFree((IntPtr)fbconfigs);
                        }
                    }
                }
            }
            
            if (Handle == ContextHandle.Zero)
            {
                Debug.Write("Using legacy context creation... ");
                
                XVisualInfo info = currentWindow.VisualInfo;
                using (new XLock(Display))
                {
                    // Cannot pass a Property by reference.
                    Handle = new ContextHandle(Glx.CreateContext(Display, ref info, shareHandle.Handle, direct));

                    if (Handle == ContextHandle.Zero)
                    {
                        Debug.WriteLine(String.Format("failed. Trying direct: {0}... ", !direct));
                        Handle = new ContextHandle(Glx.CreateContext(Display, ref info, IntPtr.Zero, !direct));
                    }
                }
            }
            
            if (Handle != ContextHandle.Zero)
                Debug.Print("Context created (id: {0}).", Handle);
            else
                throw new Exception("Failed to create OpenGL context. Glx.CreateContext call returned 0.");

            using (new XLock(Display))
            {
                if (!Glx.IsDirect(Display, Handle.Handle))
                    Debug.Print("Warning: Context is not direct.");
            }
        }

        public X11GLContext(ContextHandle handle, IWindowInfo window, IGraphicsContext shared, bool direct,
            int major, int minor, GraphicsContextFlags flags)
        {
            if (handle == ContextHandle.Zero)
                throw new ArgumentException("handle");
            if (window == null)
                throw new ArgumentNullException("window");

            Handle = handle;
            currentWindow = (X11WindowInfo)window;
            Display = currentWindow.Display;
        }

        #endregion

        #region --- Private Methods ---

        IntPtr Display
        {
            get { return display; }
            set
            {
                if (value == IntPtr.Zero)
                    throw new ArgumentOutOfRangeException();
                if (display != IntPtr.Zero)
                    throw new InvalidOperationException("The display connection may not be changed after being set.");
                display = value;
            }
        }

        #region XVisualInfo SelectVisual(GraphicsMode mode, X11WindowInfo currentWindow)

        XVisualInfo SelectVisual(GraphicsMode mode, X11WindowInfo currentWindow)
        {
            XVisualInfo info = new XVisualInfo();
            info.VisualID = (IntPtr)mode.Index;
            info.Screen = currentWindow.Screen;
            int items;
            
            lock (API.Lock)
            {
                IntPtr vs = Functions.XGetVisualInfo(Display, XVisualInfoMask.ID | XVisualInfoMask.Screen, ref info, out items);
                if (items == 0)
                    throw new Exception(String.Format("Invalid GraphicsMode specified ({0}).", mode));

                info = (XVisualInfo)Marshal.PtrToStructure(vs, typeof(XVisualInfo));
                Functions.XFree(vs);
            }

            return info;
        }

        #endregion

        #endregion

        #region --- IGraphicsContext Members ---

        #region SwapBuffers()

        public override void SwapBuffers()
        {
            if (Display == IntPtr.Zero || currentWindow.WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException(
                    String.Format("Window is invalid. Display ({0}), Handle ({1}).", Display, currentWindow.WindowHandle));
            using (new XLock(Display))
            {
                Glx.SwapBuffers(Display, currentWindow.WindowHandle);
            }
        }

        #endregion

        #region MakeCurrent

        public override void MakeCurrent(IWindowInfo window)
        {
            if (window == currentWindow && (IsCurrent||currentWindow==null))
                return;

            if (window == null)
            {
                using (new XLock(Display))
                {
                    if(Glx.MakeCurrent(Display, IntPtr.Zero, IntPtr.Zero))
                        currentWindow = null;
                }
            }
            else
            {
                X11WindowInfo w = (X11WindowInfo)window;

                if (Display == IntPtr.Zero || w.WindowHandle == IntPtr.Zero || Handle == ContextHandle.Zero)
                    throw new InvalidOperationException("Invalid display, window or context.");

                using (new XLock(Display))
                {
                    if (Glx.MakeCurrent(Display, w.WindowHandle, Handle))
                        currentWindow = w;
                }
            }

            currentWindow = (X11WindowInfo)window;
        }

        #endregion

        #region IsCurrent

        public override bool IsCurrent
        {
            get
            {
                using (new XLock(Display))
                {
                    return Glx.GetCurrentContext() == Handle.Handle;
                }
            }
        }

        #endregion

        #region VSync

        public override bool VSync
        {
            get
            {
                return vsync_supported && vsync_interval != 0;
            }
            set
            {
                if (vsync_supported)
                {
                    ErrorCode error_code = 0;
                    using (new XLock(Display))
                    {
                        error_code = Glx.Sgi.SwapInterval(value ? 1 : 0);
                    }
                    if (error_code != X11.ErrorCode.NO_ERROR)
                        Debug.Print("VSync = {0} failed, error code: {1}.", value, error_code);
                    vsync_interval = value ? 1 : 0;
                }
            }
        }

        #endregion

        #region GetAddress

        public override IntPtr GetAddress(string function)
        {
            using (new XLock(Display))
            {
                return Glx.GetProcAddress(function);
            }
        }

        #endregion

        #region LoadAll

        public override void LoadAll()
        {
            new Glx().LoadEntryPoints();
            vsync_supported = this.GetAddress("glXSwapIntervalSGI") != IntPtr.Zero;
            Debug.Print("Context supports vsync: {0}.", vsync_supported);

            base.LoadAll();
        }

        #endregion

        #endregion

        #region --- IGLContextInternal Members ---

         #region IWindowInfo IGLContextInternal.Info

        //IWindowInfo IGraphicsContextInternal.Info { get { return window; } }

        #endregion

        #endregion

        #region --- IDisposable Members ---

        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool manuallyCalled)
        {
            if (!IsDisposed)
            {
                if (manuallyCalled)
                {
                    IntPtr display = Display;

                    if (IsCurrent)
                    {
                        using (new XLock(display))
                        {
                            Glx.MakeCurrent(display, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    using (new XLock(display))
                    {
                        Glx.DestroyContext(display, Handle);
                    }
                }
            }
            else
            {
                Debug.Print("[Warning] {0} leaked.", this.GetType().Name);
            }
            IsDisposed = true;
        }
        

        ~X11GLContext()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
#endif