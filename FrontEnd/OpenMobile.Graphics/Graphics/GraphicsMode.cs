#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace OpenMobile.Graphics
{
    /// <summary>Defines the format for graphics operations.</summary>
    public class GraphicsMode : IEquatable<GraphicsMode>
    {
        ColorFormat color_format, accumulator_format;
        int depth, stencil, buffers, samples;
        bool stereo;
        IntPtr? index = null;  // The id of the pixel format or visual.

        static GraphicsMode defaultMode;
        static IGraphicsMode implementation;
        static readonly object SyncRoot = new object();
                 /// <summary>
	 	         /// Returns the hashcode for this instance.
	 	         /// </summary>
	 	         /// <returns>A <see cref="System.Int32"/> hashcode for this instance.</returns>
	 	         public override int GetHashCode()
	 	         {
	 	             return Index.GetHashCode();
	 	         }
	 	 
	 	         /// <summary>
	 	         /// Indicates whether obj is equal to this instance.
	 	         /// </summary>
	 	         /// <param name="obj">An object instance to compare for equality.</param>
	 	         /// <returns>True, if obj equals this instance; false otherwise.</returns>
	 	         public override bool Equals(object obj)
	 	         {
                     GraphicsMode mode = obj as GraphicsMode;
	 	             if (mode!=null)
	 	             {
	 	                 return Equals(mode);
	 	             }
	 	             return false;
	 	         }
	 	 
	 	         /// <summary>
	 	         /// Indicates whether other represents the same mode as this instance.
	 	         /// </summary>
	 	         /// <param name="other">The GraphicsMode to compare to.</param>
	 	         /// <returns>True, if other is equal to this instance; false otherwise.</returns>
	 	         public bool Equals(GraphicsMode other)
	 	         {
	 	             return Index.HasValue && Index == other.Index;
	 	         }

        #region --- Constructors ---

        #region static GraphicsMode()
        
        static GraphicsMode()
        {
            lock (SyncRoot)
            {
                implementation = Platform.Factory.Default.CreateGraphicsMode();
            }
        }

        #endregion

        #region internal GraphicsMode(IntPtr? index, ColorFormat color, int depth, int stencil, int samples, ColorFormat accum, int buffers, bool stereo)

        internal GraphicsMode(IntPtr? index, ColorFormat color, int depth, int stencil, int samples, ColorFormat accum,
                              int buffers, bool stereo)
        {
            if (depth < 0) throw new ArgumentOutOfRangeException("depth", "Must be greater than, or equal to zero.");
            if (stencil < 0) throw new ArgumentOutOfRangeException("stencil", "Must be greater than, or equal to zero.");
            if (buffers <= 0) throw new ArgumentOutOfRangeException("buffers", "Must be greater than zero.");
            if (samples < 0) throw new ArgumentOutOfRangeException("samples", "Must be greater than, or equal to zero.");

            this.Index = index;
            this.ColorFormat = color;
            this.Depth = depth;
            this.Stencil = stencil;
            this.Samples = samples;
            this.AccumulatorFormat = accum;
            this.Buffers = buffers;
            this.Stereo = stereo;
        }

        #endregion

        #region public GraphicsMode(ColorFormat color, int depth, int stencil, int samples, ColorFormat accum, int buffers, bool stereo)

        /// <summary>Constructs a new GraphicsMode with the specified parameters.</summary>
        /// <param name="color">The ColorFormat of the color buffer.</param>
        /// <param name="depth">The number of bits in the depth buffer.</param>
        /// <param name="stencil">The number of bits in the stencil buffer.</param>
        /// <param name="samples">The number of samples for FSAA.</param>
        /// <param name="accum">The ColorFormat of the accumilliary buffer.</param>
        /// <param name="stereo">Set to true for a GraphicsMode with stereographic capabilities.</param>
        /// <param name="buffers">The number of render buffers. Typical values include one (single-), two (double-) or three (triple-buffering).</param>
        public GraphicsMode(ColorFormat color, int depth, int stencil, int samples, ColorFormat accum, int buffers, bool stereo)
            : this(null, color, depth, stencil, samples, accum, buffers, stereo) { }

        #endregion

        #endregion

        #region --- Public Methods ---

        #region public IntPtr Index

        /// <summary>
        /// Gets a nullable <see cref="System.IntPtr"/> value, indicating the platform-specific index for this GraphicsMode.
        /// </summary>
        public IntPtr? Index
        {
            get
            {
                if (index == null)
                {
                    GraphicsMode mode;
                    mode = implementation.SelectGraphicsMode(ColorFormat, Depth, Stencil, Samples, AccumulatorFormat, Buffers, Stereo);

                    Index = mode.Index;
                    ColorFormat = mode.ColorFormat;
                    Depth = mode.Depth;
                    Stencil = mode.Stencil;
                    Samples = mode.Samples;
                    AccumulatorFormat = mode.AccumulatorFormat;
                    Buffers = mode.Buffers;
                    Stereo = mode.Stereo;
                }

                return index;
            }
            set { index = value; }
        }

        #endregion

        #region public int ColorFormat

        /// <summary>
        /// Gets an OpenMobile.Graphics.ColorFormat that describes the color format for this GraphicsFormat.
        /// </summary>
        public ColorFormat ColorFormat
        {
            get { return color_format; }
            private set { color_format = value; }
        }

        #endregion

        #region public int AccumulatorFormat

        /// <summary>
        /// Gets an OpenMobile.Graphics.ColorFormat that describes the accumulator format for this GraphicsFormat.
        /// </summary>
        public ColorFormat AccumulatorFormat
        {
            get { return accumulator_format; }
            private set { accumulator_format = value; }
        }

        #endregion

        #region public int Depth

        /// <summary>
        /// Gets a System.Int32 that contains the bits per pixel for the depth buffer
        /// for this GraphicsFormat.
        /// </summary>
        public int Depth
        {
            get { return depth; }
            private set { depth = value; }
        }

        #endregion

        #region public int Stencil

        /// <summary>
        /// Gets a System.Int32 that contains the bits per pixel for the stencil buffer
        /// of this GraphicsFormat.
        /// </summary>
        public int Stencil
        {
            get { return stencil; }
            private set { stencil = value; }
        }

        #endregion

        #region public int Samples

        /// <summary>
        /// Gets a System.Int32 that contains the number of FSAA samples per pixel for this GraphicsFormat.
        /// </summary>
        public int Samples
        {
            get { return samples; }
            private set { samples = value; }
        }

        #endregion

        #region public bool Stereo

        /// <summary>
        /// Gets a System.Boolean indicating whether this DisplayMode is stereoscopic.
        /// </summary>
        public bool Stereo
        {
            get { return this.stereo; }
            private set { this.stereo = value; }
        }

        #endregion

        #region public int Buffers

        /// <summary>
        /// Gets a System.Int32 containing the number of buffers associated with this
        /// DisplayMode.
        /// </summary>
        public int Buffers
        {
            get { return this.buffers; }
            private set { this.buffers = value; }
        }

        #endregion

        #region public static GraphicsFormat Default

        /// <summary>Returns an OpenMobile.GraphicsFormat compatible with the underlying platform.</summary>
        public static GraphicsMode Default
        {
            get
            {
                //Console.WriteLine("GraphicsMode.Default.Enter: " + Timing.GetTiming());
                lock (SyncRoot)
                {
                    if (defaultMode == null)
                    {
                        #if DEBUG
                        Debug.Print("Creating default GraphicsMode ({0}, {1}, {2}, {3}, {4}, {5}, {6}).", DisplayDevice.Default.BitsPerPixel,
                                    16, 0, 2, 0, 2, false);
                        #endif
                        //Console.WriteLine("GraphicsMode.Default.CreateGraphicsMode: " + Timing.GetTiming());
                        defaultMode = new GraphicsMode(DisplayDevice.Default.BitsPerPixel, 16, 0, 0, 0, 2, false);
                        //Console.WriteLine("GraphicsMode.Default.End: " + Timing.GetTiming());
                    }
                    //Console.WriteLine("GraphicsMode.Default.Return: " + Timing.GetTiming());
                    return defaultMode;
                }
            }
        }

        #endregion

        #endregion

        #region --- Overrides ---

        /// <summary>Returns a System.String describing the current GraphicsFormat.</summary>
        /// <returns>! System.String describing the current GraphicsFormat.</returns>
        public override string ToString()
        {
            return String.Format("Index: {0}, Color: {1}, Depth: {2}, Stencil: {3}, Samples: {4}, Accum: {5}, Buffers: {6}, Stereo: {7}",
                Index, ColorFormat, Depth, Stencil, Samples, AccumulatorFormat, Buffers, Stereo);
        }

        #endregion
    }
}
