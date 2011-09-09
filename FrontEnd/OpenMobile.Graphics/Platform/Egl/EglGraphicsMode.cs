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
#if LINUX
using System;
using System.Collections.Generic;
using System.Text;
using OpenMobile.Graphics;

namespace OpenMobile.Platform.Egl
{
    class EglGraphicsMode //: IGraphicsMode
    {
        #region IGraphicsMode Members

        public GraphicsMode SelectGraphicsMode(IntPtr display, ColorFormat color, int depth, int stencil, int samples, ColorFormat accum, int buffers, bool stereo)
        {
            IntPtr[] configs = new IntPtr[1];
            int[] attribList = new int[] 
            { 
                //Egl.SURFACE_TYPE, Egl.WINDOW_BIT,
                Egl.RENDERABLE_TYPE, Egl.OPENGL_ES_BIT,
                Egl.RED_SIZE, color.Red, 
                Egl.GREEN_SIZE, color.Green, 
                Egl.BLUE_SIZE, color.Blue,
                Egl.ALPHA_SIZE, color.Alpha,

                //Egl.DEPTH_SIZE,  0,
                //Egl.STENCIL_SIZE, stencil > 0 ? stencil : 0,

                //Egl.SAMPLE_BUFFERS, samples > 0 ? 1 : 0,
                //Egl.SAMPLES, samples > 0 ? samples : 0,

                Egl.NONE,
            };

            int num_configs;
            if ((!Egl.ChooseConfig(display, attribList, configs, 1, out num_configs)) || (num_configs == 0))
                configs[0] = manuallySelectGraphicsMode(display);
            int r, g, b, a, d;
            // See what we really got
            IntPtr active_config = configs[0];

            Egl.GetConfigAttrib(display, active_config, Egl.RED_SIZE, out r);
            Egl.GetConfigAttrib(display, active_config, Egl.GREEN_SIZE, out g);
            Egl.GetConfigAttrib(display, active_config, Egl.BLUE_SIZE, out b);
            Egl.GetConfigAttrib(display, active_config, Egl.ALPHA_SIZE, out a);
            Egl.GetConfigAttrib(display, active_config, Egl.DEPTH_SIZE, out d);
            int s;
            Egl.GetConfigAttrib(display, active_config, Egl.STENCIL_SIZE, out s);
            int sample_buffers;
            Egl.GetConfigAttrib(display, active_config, Egl.SAMPLES, out sample_buffers);
            Egl.GetConfigAttrib(display, active_config, Egl.SAMPLES, out samples);

            return new GraphicsMode(active_config, new ColorFormat(r, g, b, a), d, s, sample_buffers > 0 ? samples : 0, 0, 2, false);
        }

        private IntPtr manuallySelectGraphicsMode(IntPtr display)
        {
            int num_configs;
            if (!Egl.GetConfigs(display, null, 0, out num_configs)||(num_configs==0))
                throw new Exception(String.Format("Failed to retrieve GraphicsMode configurations, error {0}", Egl.GetError()));
            IntPtr[] tmp = new IntPtr[num_configs];
            IntPtr fallback=IntPtr.Zero;
            Egl.GetConfigs(display, tmp, tmp.Length, out num_configs);
            int r, g, b, a;
            foreach (IntPtr config in tmp)
            {
                Egl.GetConfigAttrib(display, config, Egl.RED_SIZE, out r);
                if (r < 8)
                    continue;
                Egl.GetConfigAttrib(display, config, Egl.GREEN_SIZE, out g);
                if (g < 8)
                    continue;
                Egl.GetConfigAttrib(display, config, Egl.BLUE_SIZE, out b);
                if (b < 8)
                    continue;
                fallback = config;
                Egl.GetConfigAttrib(display, config, Egl.ALPHA_SIZE, out a);
                if (a >= 8)
                    return config;
            }
            if (fallback == IntPtr.Zero)
                return tmp[0];
            else
                return fallback;
        }

        #endregion
    }
}
#endif