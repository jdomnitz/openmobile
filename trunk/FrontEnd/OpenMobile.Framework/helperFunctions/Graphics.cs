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

namespace OpenMobile.Graphics
{
    /// <summary>
    /// enum indicating corners
    /// </summary>
    [Flags]
    public enum GraphicCorners
    {
        /// <summary>
        /// No corners
        /// </summary>
        None = 0,
        /// <summary>
        /// Upper left corner
        /// </summary>
        TopLeft = 1,
        /// <summary>
        /// Upper right corner
        /// </summary>
        TopRight = 2,
        /// <summary>
        /// Lower right corner
        /// </summary>        
        BottomRight = 4,
        /// <summary>
        /// Lower left corner
        /// </summary>
        BottomLeft = 8,
        /// <summary>
        /// All corners
        /// </summary>
        All = 15,
        /// <summary>
        /// Top side
        /// </summary>
        Top = 3,
        /// <summary>
        /// Right side
        /// </summary>
        Right = 6,
        /// <summary>
        /// Bottom side
        /// </summary>
        Bottom = 12,
        /// <summary>
        /// Left side
        /// </summary>
        Left = 9
    }

}

namespace OpenMobile.helperFunctions.Graphics
{

    /// <summary>
    /// A button style image that can be generated to match any size
    /// </summary>
    public static class ButtonGraphic
    {

        /// <summary>
        /// The different types of buttons
        /// </summary>
        public enum GraphicStyles
        {
            /// <summary>
            /// Basic style button style
            /// </summary>
            BaseStyle,

            /// <summary>
            /// Base style 1 button style
            /// </summary>
            Style1,

            /// <summary>
            /// A clean and simple button style
            /// </summary>
            CleanAndSimple
        }

        /// <summary>
        /// The different images that can be returned
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
            /// Returns the button background image in a focused (highlighted) and clicked state
            /// </summary>
            ButtonBackgroundClicked,

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
            /// <summary>
            /// Generates a new graphicdata object
            /// </summary>
            public GraphicData()
            {   // Default values
                IconLocation = new Point(-1, -1);
                TextLocation = new Point(-1, -1);
                BackgroundFocusSize = 0.5F;
                BackgroundFocusClickedSize = 0.75F;

                // Default colors are read from the OM system setting (SkinFocusColor)
                this.BackgroundFocusColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                this.ForegroundFocusColor = this.BackgroundFocusColor;
                this.TextColor = BuiltInComponents.SystemSettings.SkinTextColor;

                this.CornersRadiusAppliesTo = GraphicCorners.All;
            }

            /// <summary>
            /// The general opacity to use for this graphic
            /// </summary>
            public int? Opacity { get; set; }
            /// <summary>
            /// Radius to use for corners
            /// </summary>
            public int? CornerRadius { get; set; }
            /// <summary>
            /// The corners the radius is applied to
            /// </summary>
            public GraphicCorners CornersRadiusAppliesTo { get; set; }
            /// <summary>
            /// Button type to generate
            /// </summary>
            public GraphicStyles Style { get; set; }
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
            /// Size to use when drawing the background focus in a clicked state (size is given in percent of height (0.0F to 1.0F), default is 0.75F (75%))
            /// </summary>
            public float BackgroundFocusClickedSize { get; set; }
            /// <summary>
            /// Color to use when drawing the background color
            /// </summary>
            public Color BackgroundColor1 { get; set; }
            /// <summary>
            /// Color to use when drawing the background color
            /// </summary>
            public Color BackgroundColor2 { get; set; }
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
            ///// <summary>
            ///// Icon image string (if this is present then the referenced image will be used instead of the icon string)
            ///// </summary>
            //public string IconImage { get; set; }

            public OImage IconImage { get; set; }
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
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, ImageTypes ImageType, GraphicStyles Style)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.ImageType = ImageType;
            gd.Style = Style;
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
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, ImageTypes ImageType, string Icon, string Text, GraphicStyles Style)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.ImageType = ImageType;
            gd.Icon = Icon;
            gd.Text = Text;
            gd.Style = Style;
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
    public static class PanelPopupOutlineGraphic
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
                this.ShadowColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                this.TextColor = BuiltInComponents.SystemSettings.SkinTextColor;
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
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.PanelPopupOutline", "PanelPopupOutline", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get PanelPopupOutline, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

    /// <summary>
    /// A rectangle customisable properties that can be used as background for other objects
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
        /// The different shadowtypes
        /// </summary>
        public enum ShadowTypes
        {
            /// <summary>
            /// No shadow
            /// </summary>
            None,

            /// <summary>
            /// Normal drop style shadow
            /// </summary>
            DropShadow,

            /// <summary>
            /// Glows on all edges
            /// </summary>
            GlowOnAllEdges
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                this.ShadowType =  ShadowTypes.GlowOnAllEdges;
                this.ShadowSize = 14;
                this.BorderThickness = 2;

                // Default colors are read from the OM system setting (SkinFocusColor)
                this.ShadowColor = BuiltInComponents.SystemSettings.SkinFocusColor;
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
            /// Color to use when drawing the shadow
            /// </summary>
            public Color ShadowColor { get; set; }
            /// <summary>
            /// Type of shadow to use
            /// </summary>
            public ShadowTypes ShadowType { get; set; }
            /// <summary>
            /// Size of shadow
            /// </summary>
            public int ShadowSize { get; set; }
            /// <summary>
            /// Thickness of border
            /// </summary>
            public int BorderThickness { get; set; }
            /// <summary>
            /// A custom graphicpath to use when generating this graphic
            /// </summary>
            public System.Drawing.Drawing2D.GraphicsPath GraphicPath { get; set; }
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
        /// The sides to generate graphics for
        /// </summary>
        public enum GraphicSides
        {
            None = 0,
            Top = 1,
            Left = 2,
            Right = 4,
            Bottom = 8
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                FadeSize = 0.05f;
                Sides = GraphicSides.Left | GraphicSides.Top | GraphicSides.Right | GraphicSides.Bottom;
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
            /// Sides that should be drawn (Top | Left | Right | Bottom = all sides active)
            /// </summary>
            public GraphicSides Sides { get; set; }

            /// <summary>
            /// Clone this object
            /// </summary>
            /// <returns></returns>
            public GraphicData Clone()
            {
                return (GraphicData)this.MemberwiseClone();
            }
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

    /// <summary>
    /// Media graphics (missing cover etc.)
    /// </summary>
    public static class MediaGraphic
    {

        /// <summary>
        /// The different types of graphics
        /// </summary>
        public enum GraphicStyles
        {
            /// <summary>
            /// NoCover image
            /// </summary>
            NoCover,
            /// <summary>
            /// The OpenMobile logo
            /// </summary>
            OMLogo
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values

                // Default colors are read from the OM system setting (SkinFocusColor)
                this.ForegroundColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            }

            /// <summary>
            /// Button type to generate
            /// </summary>
            public GraphicStyles Style { get; set; }
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
            /// Color to use when drawing the background color
            /// </summary>
            public Color BackgroundColor { get; set; }
            /// <summary>
            /// Color to use when drawing the foreground
            /// </summary>
            public Color ForegroundColor { get; set; }
        }

        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, GraphicStyles Style)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.Style = Style;
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
        public static OImage GetImage(int Width, int Height, GraphicStyles Style, string Handler)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
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
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.MediaGraphic", "MediaGraphic", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get MediaGraphic, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

    /// <summary>
    /// Text graphic effects
    /// </summary>
    public static class TextGraphics
    {
        /// <summary>
        /// The different types of graphics
        /// </summary>
        public enum GraphicStyles
        {
            /// <summary>
            /// Glow style
            /// </summary>
            Glow
        }

        /// <summary>
        /// Data for generated graphics
        /// </summary>
        public class GraphicData
        {
            public GraphicData()
            {   // Default values
                // Default colors are read from the OM system setting (SkinFocusColor)
                this.EffectColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                this.TextColor = BuiltInComponents.SystemSettings.SkinTextColor;
            }

            /// <summary>
            /// Button type to generate
            /// </summary>
            public GraphicStyles Style { get; set; }
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
            /// Color to use when drawing the effect
            /// </summary>
            public Color EffectColor { get; set; }
            /// <summary>
            /// Color to use when drawing the text 
            /// </summary>
            public Color TextColor { get; set; }
            /// <summary>
            /// Text string 
            /// </summary>
            public string Text { get; set; }
            /// <summary>
            /// Font to use for the text
            /// </summary>
            public Font TextFont { get; set; }
            /// <summary>
            /// Format for the text
            /// </summary>
            public eTextFormat textFormat = eTextFormat.Normal;
            /// <summary>
            /// Text alignment
            /// </summary>
            public Alignment textAlignment = Alignment.CenterCenter;

        }

        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the default handler (OMGraphics)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, GraphicStyles Style)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
            gd.Style = Style;
            return GetImage(gd, "OMGraphics");
        }
        /// <summary>
        /// Gets a image of the requested type and the requested width and height using the requested handler
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Style"></param>
        /// <param name="Handler"></param>
        /// <returns></returns>
        public static OImage GetImage(int Width, int Height, GraphicStyles Style, string Handler)
        {
            GraphicData gd = new GraphicData();
            gd.Width = Width;
            gd.Height = Height;
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
            if (!BuiltInComponents.Host.sendMessage<GraphicData>(Handler, "OpenMobile.helperFunctions.TextGraphics", "TextGraphics", ref gd))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get TextGraphics, plugin " + Handler + " not available");
                return null;
            }
            return gd.Image;
        }
    }

    /// <summary>
    /// Image helpers
    /// </summary>
    public static class Images
    {
        /// <summary>
        /// Creates a "mosaic" effect of a list of images
        /// </summary>
        /// <param name="images"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        static public OImage CreateMosaic(List<OImage> images, int width, int height)
        {
            // Create target image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                List<Rectangle> rects = GetRectangles(images.Count, width, height);
                for (int i = 0; i < rects.Count; i++)
			    {
                    if (images.Count >= i)
                        g.DrawImage(images[i].image, rects[i].ToSystemRectangle(), 0, 0, images[i].image.Width, images[i].image.Height, System.Drawing.GraphicsUnit.Pixel);
			    }
            }
            return new OImage(bmp);
        }

        /// <summary>
        /// Gets a "mosaic" effect rectangle list
        /// </summary>
        /// <param name="count"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="sizeLimit"></param>
        /// <returns></returns>
        static public List<Rectangle> GetRectangles(int count, int width, int height, int sizeLimit = 100)
        {
            List<Rectangle> rects = new List<Rectangle>();

            rects.Add(new Rectangle(0, 0, width, height));

            bool direction = true;
            while (rects.Count < count)
            {
                var rectangles = SplitRectangle(rects[rects.Count - 1], direction);
                rects.RemoveAt(rects.Count - 1);
                rects.AddRange(rectangles);
                direction = !direction;
                if (rects.Count >= count)
                    break;
                if (rects[rects.Count - 1].Width < sizeLimit || rects[rects.Count - 1].Height < sizeLimit)
                    break;
            }
            return rects;
        }

        static private List<Rectangle> SplitRectangle(Rectangle rect, bool directionVertical)
        {
            List<Rectangle> rects = new List<Rectangle>();

            if (directionVertical)
            {
                rects.Add(new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height / 2));
                rects.Add(new Rectangle(rect.Left, rect.Top + rect.Height / 2, rect.Width, rect.Height / 2));
            }
            else
            {   // horizontal

                rects.Add(new Rectangle(rect.Left, rect.Top, rect.Width / 2, rect.Height));
                rects.Add(new Rectangle(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height));
            }
            return rects;            
        }
    }

}
