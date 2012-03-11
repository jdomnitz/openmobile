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
using System.Threading;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using OpenMobile.Framework;

namespace OpenMobile.helperFunctions.Graphics
{
    /// <summary>
    /// A smooth animator class
    /// </summary>
    public class SmoothAnimator
    {
        /// <summary>
        /// Delegate for Animator class. Return true when the animation should run, returning false stops the animation.
        /// </summary>
        /// <param name="AnimationStep"></param>
        public delegate bool AnimatorDelegate(int AnimationStep);

        /// <summary>
        /// Speed of animation. 
        /// <para>A value of 1.0F indicates that the AnimationStep will correspond to milliseconds since last call to the animation loop</para>
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Execute animation
        /// </summary>
        public bool Run { get; set; }

        /// <summary>
        /// Execute animation asynchronous
        /// </summary>
        public bool Asynchronous { get; set; }

        /// <summary>
        /// Animation thread delay (A higer delay is better for slower systems but may lead to "choppy" graphics)
        /// </summary>
        public int ThreadDelay { get; set; }

        /// <summary>
        /// Frames pr second to run animation at (default 30fps)
        /// </summary>
        public float FPS { get; set; }

        /// <summary>
        /// Initialize a new smooth animator
        /// </summary>
        /// <param name="Animation_Speed"></param>
        public SmoothAnimator(float Animation_Speed, bool Asynchronous)
            : this(Animation_Speed)
        {
            this.Asynchronous = Asynchronous;
        }
        /// <summary>
        /// Initialize a new smooth animator
        /// </summary>
        /// <param name="Animation_Speed"></param>
        public SmoothAnimator()
            : this(1.0F)
        {            
        }
        /// <summary>
        /// Initialize a new smooth animator
        /// </summary>
        /// <param name="Animation_Speed"></param>
        public SmoothAnimator(float Animation_Speed)
        {
            this.Speed = Animation_Speed;
            this.FPS = 30.0F;
            this.ThreadDelay = 1;
            this.Asynchronous = false;
        }

        /// <summary>
        /// Execute animation. Sample code with anonymous delegate:
        /// <para>Animate(delegate(int AnimationStep)</para>
        /// <para>{</para>
        /// <para>.... Code goes here ....</para>
        /// <para>]);</para>
        /// </summary>
        /// <param name="d"></param>
        public void Animate(AnimatorDelegate d)
        {
            if (this.Asynchronous)
            {
                OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                {
                    RunAnimation(d);
                });
            }
            else
            {
                RunAnimation(d);
            }
        }

        private void RunAnimation(AnimatorDelegate d)
        {
            int Animation_Step;
            float Interval = System.Diagnostics.Stopwatch.Frequency / FPS;
            float currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            float lastUpdateTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            float ticks = 0;
            float ticksMS = 0;

            this.Run = true;
            while (Run)
            {
                currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                ticks = currentTicks - lastUpdateTicks;
                if (ticks >= Interval)
                {
                    lastUpdateTicks = currentTicks;
                    ticksMS = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000;
                    Animation_Step = ((int)(ticksMS * Speed));

                    // Call animation 
                    Run = d(Animation_Step);
                }
                Thread.Sleep(ThreadDelay);
            }
        }
    }

    /// <summary>
    /// A button style image that can be generated to match any size
    /// </summary>
    public static class ButtonGraphic
    {

        /// <summary>
        /// The different data that the menu can return
        /// </summary>
        public enum ImageTypes
        {
            /// <summary>
            /// Returns the button background image
            /// </summary>
            ButtonBackground,

            /// <summary>
            /// Returns the button background image in a focused (highlighted) state
            /// </summary>
            ButtonBackgroundFocused,

            /// <summary>
            /// Returns the button foreground image (icon and text)
            /// </summary>
            ButtonForeground,

            /// <summary>
            /// Returns the button foreground image (icon and text) in a focused (highlighted) state
            /// </summary>
            ButtonForegroundFocused
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                IconLocation = new Point(-1, -1);
                TextLocation = new Point(-1, -1);
                BackgroundFocusSize = 0.5F;

                // Default colors are read from the OM system setting (SkinFocusColor)
                this.BackgroundFocusColor = StoredData.SystemSettings.SkinFocusColor;
                this.ForegroundFocusColor = this.BackgroundFocusColor;
            }
            
            /// <summary>
            /// Height of graphic
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// Width of graphic
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// Generated image
            /// </summary>
            public OImage Image { get; set; }
            /// <summary>
            /// Imagetype to generate
            /// </summary>
            public ImageTypes ImageType { get; set; }
            /// <summary>
            /// Color to use when drawing the focus color on the background
            /// </summary>
            public Color BackgroundFocusColor { get; set; }
            /// <summary>
            /// Size to use when drawing the background focus (size is given in percent of height (0.0F to 1.0F), default is 0.5F (50%))
            /// </summary>
            public float BackgroundFocusSize { get; set; }
            /// <summary>
            /// Color to use when drawing the background color
            /// </summary>
            public Color BackgroundColor { get; set; }
            /// <summary>
            /// Color to use when drawing the focus color on the foreground
            /// </summary>
            public Color ForegroundFocusColor { get; set; }
            /// <summary>
            /// Color to use when drawing border
            /// </summary>
            public Color BorderColor { get; set; }
            /// <summary>
            /// Color to use when drawing the icon 
            /// </summary>
            public Color IconColor { get; set; }
            /// <summary>
            /// Icon string
            /// </summary>
            public string Icon { get; set; }
            /// <summary>
            /// Icon image string (if this is present then the referenced image will be used instead of the icon string)
            /// </summary>
            public string IconImage { get; set; }
            /// <summary>
            /// Font to use for the icon (default is WebDings at size 76)
            /// </summary>
            public Font IconFont { get; set; }
            /// <summary>
            /// Icon placement internally in graphic (default x=0, y=7)
            /// </summary>
            public Point IconLocation { get; set; }
            /// <summary>
            /// Color to use when drawing the text 
            /// </summary>
            public Color TextColor { get; set; }
            /// <summary>
            /// Text string 
            /// </summary>
            public string Text { get; set; }
            /// <summary>
            /// Font to use for the text (default is Arial at size 36)
            /// </summary>
            public Font TextFont { get; set; }
            /// <summary>
            /// Text placement internally in graphic (default x=0, y=7)
            /// </summary>
            public Point TextLocation { get; set; }
        }

        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, ImageTypes ImageType)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.ImageType = ImageType;
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, ImageTypes ImageType, string Icon, string Text)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.ImageType = ImageType;
            gd.Icon = Icon;
            gd.Text = Text;
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the requested handler
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <param name="Handler"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, ImageTypes ImageType, string Handler)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.ImageType = ImageType;
            return GetImage(gd, Handler);
        }
        /// <summary>
        /// Gets a image according to the graphicdata using the default handler (OMGraphics)
        /// </summary>
        /// <param name="gd"></param>
        /// <returns></returns>
        public static OImage GetImage(GraphicData gd)
        {
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image according to the graphicdata using the requested handler
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="Handler"></param>
        /// <returns></returns>
        public static OImage GetImage(GraphicData gd, string Handler)
        {
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.ButtonGraphic", "ButtonGraphic", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get ButtonGraphic, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

    /// <summary>
    /// A rectangle with or without a header text that can be used as background for other objects
    /// </summary>
    public static class PanelOutlineGraphic
    {

        /// <summary>
        /// The different data that the menu can return
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Returns a rounded rectangle style graphic
            /// </summary>
            RoundedRectangle
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                ShowShadow = true;

                // Default colors are read from the OM system setting (SkinFocusColor)
                this.ShadowColor = StoredData.SystemSettings.SkinFocusColor;
            }

            /// <summary>
            /// Height of graphic
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// Width of graphic
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// Generated image
            /// </summary>
            public OImage Image { get; set; }
            /// <summary>
            /// Client area (The available space for client controls relative to the generated image)
            /// </summary>
            public Rectangle ClientArea { get; set; }
            /// <summary>
            /// Header area (The available space for Header controls relative to the generated image)
            /// </summary>
            public Rectangle HeaderArea { get; set; }
            /// <summary>
            /// Imagetype to generate
            /// </summary>
            public Types Type { get; set; }
            /// <summary>
            /// Color to use when drawing the background color
            /// </summary>
            public Color BackgroundColor { get; set; }
            /// <summary>
            /// Color to use when drawing border
            /// </summary>
            public Color BorderColor { get; set; }
            /// <summary>
            /// Color to use when drawing the text 
            /// </summary>
            public Color TextColor { get; set; }
            /// <summary>
            /// Color to use when drawing the shadow
            /// </summary>
            public Color ShadowColor { get; set; }
            /// <summary>
            /// Show shadow
            /// </summary>
            public bool ShowShadow { get; set; }
            /// <summary>
            /// Disable "glossy" header (False = show a color gradient in the header)
            /// </summary>
            public bool DisableGlossyHeader { get; set; }
            /// <summary>
            /// Text string 
            /// </summary>
            public string Text { get; set; }
            /// <summary>
            /// Font to use for the text (default is Arial at size 36)
            /// </summary>
            public Font TextFont { get; set; }
        }

        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, Types Type, string Text)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.Type = Type;
            gd.Text = Text;
            return GetImage(ref gd, "OMGraphics");
        }

        /// <summary>
        /// Gets a image according to the graphicdata using the default handler (OMGraphics)
        /// </summary>
        /// <param name="gd"></param>
        /// <returns></returns>
        public static OImage GetImage(GraphicData gd)
        {
            return GetImage(ref gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image according to the graphicdata using the default handler (OMGraphics)
        /// </summary>
        /// <param name="gd"></param>
        /// <returns></returns>
        public static OImage GetImage(ref GraphicData gd)
        {
            return GetImage(ref gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image according to the graphicdata using the requested handler
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="Handler"></param>
        /// <returns></returns>
        public static OImage GetImage(ref GraphicData gd, string Handler)
        {
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.PanelOutline", "PanelOutline", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get PanelOutline, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

    /// <summary>
    /// A graphic with a fading edge (different shapes available)
    /// </summary>
    public static class FadingEdge
    {

        /// <summary>
        /// The different data that the menu can return
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Returns a rectangle style graphic 
            /// </summary>
            Rectangle,
            /// <summary>
            /// Returns a rounded rectangle style graphic (NB! Rounded rectangle can onl show all sides)
            /// </summary>
            RoundedRectangle,
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                FadeSize = 0.05f;
                Sides = new bool[4] { true, true, true, true };
            }

            /// <summary>
            /// Height of graphic
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// Width of graphic
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// Size in percent for fading area
            /// </summary>
            public float FadeSize { get; set; }
            /// <summary>
            /// Generated image
            /// </summary>
            public OImage Image { get; set; }
            /// <summary>
            /// Imagetype to generate
            /// </summary>
            public Types Type { get; set; }
            /// <summary>
            /// Start color
            /// </summary>
            public Color Color1 { get; set; }
            /// <summary>
            /// End color
            /// </summary>
            public Color Color2 { get; set; }
            /// <summary>
            /// Sides that should be drawn (0 = Top, 1 = Right, 2 = Bottom, 3 = Left)
            /// </summary>
            public bool[] Sides { get; set; }
        }

        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, Types Type)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.Type = Type;
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Type"></param>
        /// <param name="Color1"></param>
        /// <param name="Color2"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, Types Type, Color Color1, Color Color2)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.Type = Type;
            gd.Color1 = Color1;
            gd.Color2 = Color2;
            return GetImage(gd, "OMGraphics");
        }

        /// <summary>
        /// Gets a image according to the graphicdata using the default handler (OMGraphics)
        /// </summary>
        /// <param name="gd"></param>
        /// <returns></returns>
        public static OImage GetImage(GraphicData gd)
        {
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image according to the graphicdata using the requested handler
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="Handler"></param>
        /// <returns></returns>
        public static OImage GetImage(GraphicData gd, string Handler)
        {
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.FadingEdge", "FadingEdge", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get FadingEdge, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

}
