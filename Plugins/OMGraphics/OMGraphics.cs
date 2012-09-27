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
using OpenMobile.helperFunctions.Graphics;
using System.Drawing;
using System.Drawing.Drawing2D;
using OpenMobile.Plugin;
using OpenMobile;

namespace OMGraphics
{
    [SkinIcon("*@")]
    [PluginLevel(PluginLevels.System)]
    public class OMGraphics : IBasePlugin
    {
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return null;
        }
        public Settings loadSettings()
        {
            return null;
        }

        #region Attributes
        public string displayName
        {
            get { return "OMGraphics"; }
        }
        public string authorName
        {
            get { return "Bjørn Morten Orderløkken"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMGraphics"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Default style generated graphics"; }
        }
        #endregion

        public bool incomingMessage(string message, string source)
        {
            return false; // No action
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {

            // What type of graphic do we want
            switch (message.ToLower())
            {
                case "buttongraphic":
                    {
                        // Convert input data to local data object (data sent in is a 
                        ButtonGraphic.GraphicData gd = data as ButtonGraphic.GraphicData;

                        switch (gd.Style)
                        {
                            case ButtonGraphic.GraphicStyles.BaseStyle:
                            default:
                                {
                                    #region Create base style button graphic

                                    System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                                    {
                                        switch (gd.ImageType)
                                        {
                                            case ButtonGraphic.ImageTypes.ButtonBackgroundFocused:
                                            case ButtonGraphic.ImageTypes.ButtonBackgroundClicked:
                                            case ButtonGraphic.ImageTypes.ButtonBackground:
                                                {
                                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                                    #region Colors

                                                    Color glowColor;
                                                    if (String.IsNullOrEmpty(gd.BackgroundFocusColor.Name) || gd.BackgroundFocusColor.Name == "0")
                                                        glowColor = System.Drawing.Color.FromArgb(0, 0, 255);
                                                    else
                                                        glowColor = gd.BackgroundFocusColor.ToSystemColor();

                                                    Color backColor;
                                                    if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                                        //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                                        backColor = Color.FromArgb(255, System.Drawing.Color.Black);
                                                    else
                                                        backColor = gd.BackgroundColor.ToSystemColor();

                                                    Color borderColor;
                                                    if (String.IsNullOrEmpty(gd.BorderColor.Name) || gd.BorderColor.Name == "0")
                                                        borderColor = BuiltInComponents.SystemSettings.SkinTextColor.SetAlpha(180).ToSystemColor();//System.Drawing.Color.FromArgb(255, System.Drawing.Color.White);//System.Drawing.Color.LightGray;
                                                    else
                                                        borderColor = gd.BorderColor.ToSystemColor();

                                                    #endregion

                                                    // Create outline path
                                                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1);
                                                    GraphicsPath gp = GetPath_RoundedRectangle(g, rect, 30);
                                                    g.SetClip(gp);

                                                    #region Draw ButtonBackground

                                                    // Draw background
                                                    using (GraphicsPath bb = CreateRoundRectangle(rect, 2))
                                                    {
                                                        using (System.Drawing.Brush br = new System.Drawing.SolidBrush(backColor))
                                                            g.FillPath(br, bb);
                                                    }

                                                    System.Drawing.Rectangle rect2 = rect;
                                                    rect2.Height >>= 1;
                                                    rect2.Height++;
                                                    System.Drawing.Color shineColor = System.Drawing.Color.FromArgb(85, 98, 130); // System.Drawing.Color.White;
                                                    using (GraphicsPath bh = CreateTopRoundRectangle(rect2, 2))
                                                    {
                                                        rect2.Height++;
                                                        int opacity = 0x99;
                                                        using (LinearGradientBrush br = new LinearGradientBrush(rect2, System.Drawing.Color.FromArgb(opacity, shineColor), System.Drawing.Color.FromArgb(opacity / 3, shineColor), LinearGradientMode.Vertical))
                                                            g.FillPath(br, bh);
                                                    }
                                                    rect2.Height -= 2;


                                                    #endregion

                                                    if (gd.ImageType == ButtonGraphic.ImageTypes.ButtonBackgroundFocused || gd.ImageType == ButtonGraphic.ImageTypes.ButtonBackgroundClicked)
                                                    {
                                                        #region Draw inner glow

                                                        System.Drawing.Rectangle rectTop = new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height / 2);
                                                        //System.Drawing.Rectangle rectBottom = new System.Drawing.Rectangle(0, rect.Height / 2, rect.Width, rect.Height / 2);
                                                        System.Drawing.Rectangle rectBottom = new System.Drawing.Rectangle(0, (int)(rect.Height * (1 - gd.BackgroundFocusSize)), rect.Width, (int)(rect.Height * gd.BackgroundFocusSize));

                                                        // Adjust innerglow to show clicked state
                                                        if (gd.ImageType == ButtonGraphic.ImageTypes.ButtonBackgroundClicked)
                                                            rectBottom = new System.Drawing.Rectangle(0, (int)(rect.Height * (1 - gd.BackgroundFocusClickedSize)), rect.Width, (int)(rect.Height * gd.BackgroundFocusClickedSize));

                                                        // Draw inner button glow

                                                        using (GraphicsPath brad = CreateBottomRadialPath(rectBottom))
                                                        {
                                                            using (PathGradientBrush pgr = new PathGradientBrush(brad))
                                                            {
                                                                unchecked
                                                                {
                                                                    int opacity = 255;
                                                                    System.Drawing.RectangleF bounds = brad.GetBounds();
                                                                    pgr.CenterPoint = new System.Drawing.PointF((bounds.Left + bounds.Right) / 2f, (bounds.Top + bounds.Bottom) / 2f);
                                                                    pgr.CenterColor = System.Drawing.Color.FromArgb(opacity, glowColor);
                                                                    pgr.SurroundColors = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(0, glowColor) };
                                                                }
                                                                g.FillPath(pgr, brad);
                                                            }
                                                        }

                                                        #endregion
                                                    }

                                                    g.ResetClip();

                                                    #region Draw outer border

                                                    g.DrawPath(new System.Drawing.Pen(borderColor, 1), gp);

                                                    #endregion
                                                }
                                                break;
                                            case ButtonGraphic.ImageTypes.ButtonForegroundFocused:
                                            case ButtonGraphic.ImageTypes.ButtonForeground:
                                                {

                                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                                    #region Colors

                                                    Color glowColor;
                                                    if (String.IsNullOrEmpty(gd.ForegroundFocusColor.Name) || gd.ForegroundFocusColor.Name == "0")
                                                        glowColor = System.Drawing.Color.FromArgb(0, 0, 255);
                                                    else
                                                        glowColor = gd.ForegroundFocusColor.ToSystemColor();

                                                    Color iconColor;
                                                    if (String.IsNullOrEmpty(gd.IconColor.Name) || gd.IconColor.Name == "0")
                                                        iconColor = System.Drawing.Color.FromArgb(225, 225, 255);
                                                    else
                                                        iconColor = gd.IconColor.ToSystemColor();

                                                    Color textColor;
                                                    if (String.IsNullOrEmpty(gd.TextColor.Name) || gd.TextColor.Name == "0")
                                                        textColor = System.Drawing.Color.LightGray;
                                                    else
                                                        textColor = gd.TextColor.ToSystemColor();

                                                    #endregion

                                                    System.Drawing.Rectangle rect = new Rectangle(0, 0, 0, 0);

                                                    #region Draw Icon

                                                    if ((!String.IsNullOrEmpty(gd.Icon)) && String.IsNullOrEmpty(gd.IconImage))
                                                    {
                                                        // Calculate placement (if no text it's centered in graphic)
                                                        rect = new System.Drawing.Rectangle(gd.IconLocation.X, gd.IconLocation.Y, (int)((bmp.Width - 1) * 0.33F), bmp.Height - 1);
                                                        if (String.IsNullOrEmpty(gd.Text))
                                                            rect = new System.Drawing.Rectangle(gd.IconLocation.X, gd.IconLocation.Y, bmp.Width - 1, bmp.Height - 1);

                                                        // Set font format
                                                        StringFormat sf = new StringFormat();
                                                        sf.Alignment = StringAlignment.Center;
                                                        sf.LineAlignment = StringAlignment.Center;
                                                        System.Drawing.Font f = (gd.IconFont.Name == null ? new System.Drawing.Font(OpenMobile.Graphics.Font.Webdings.Name, 76) : new System.Drawing.Font(gd.IconFont.Name, gd.IconFont.Size));

                                                        SizeF FSize = g.MeasureString(gd.Icon, f, new SizeF(bmp.Width, bmp.Height), sf);
                                                        // Calculate scaling factor
                                                        float Scale = bmp.Height / FSize.Height;

                                                        // Recalculate font based on scaling
                                                        f = new Font(f.Name, f.Size * Scale);

                                                        // Check placement data
                                                        if ((gd.IconLocation.X == -1) && (gd.IconLocation.Y == -1))
                                                            gd.IconLocation = new OpenMobile.Graphics.Point(0, 7);

                                                        // Wanted scale between font and height is 90 / 76 = 1,184210526315789
                                                        GraphicsPath gpTxt = new GraphicsPath();
                                                        gpTxt.AddString(gd.Icon, f.FontFamily, (int)System.Drawing.FontStyle.Regular, f.Size, rect, sf);

                                                        if (gd.ImageType == ButtonGraphic.ImageTypes.ButtonForegroundFocused)
                                                        {
                                                            #region Draw Icon glow

                                                            for (int i = 1; i < 15; ++i)
                                                            {
                                                                //System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32, 0, 128, 192), i);
                                                                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32 - i, glowColor), i);
                                                                pen.LineJoin = LineJoin.Round;
                                                                g.DrawPath(pen, gpTxt);
                                                                pen.Dispose();
                                                            }

                                                            #endregion
                                                        }

                                                        System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(iconColor);
                                                        g.FillPath(brush, gpTxt);
                                                        g.DrawPath(new System.Drawing.Pen(glowColor, 0.5F), gpTxt);
                                                    }

                                                    #endregion

                                                    #region Draw Image Icon

                                                    if (!String.IsNullOrEmpty(gd.IconImage))
                                                    {
                                                        imageItem img = BuiltInComponents.Host.getSkinImage(gd.IconImage);
                                                        if (img.image != null)
                                                            if (img.image.image != null)
                                                            {
                                                                Bitmap image = img.image.image;
                                                                // Calculate aspect
                                                                float Aspect = image.Width / image.Height;

                                                                // Calculate placement (if no text it's centered in graphic)
                                                                rect = new System.Drawing.Rectangle(gd.IconLocation.X, gd.IconLocation.Y, (int)((bmp.Width - 1) * 0.33F), bmp.Height - 1);
                                                                if (String.IsNullOrEmpty(gd.Text))
                                                                    rect = new System.Drawing.Rectangle(gd.IconLocation.X, gd.IconLocation.Y, bmp.Width - 1, bmp.Height - 1);

                                                                Rectangle imgRect = new Rectangle(gd.IconLocation.X, gd.IconLocation.Y, (int)(rect.Width * 0.60f), (int)(rect.Height * 0.60f));

                                                                // Center graphic 
                                                                imgRect.X = (rect.Width - imgRect.Width) / 2;
                                                                imgRect.Y = (rect.Height - imgRect.Height) / 2;

                                                                // Calculate width to draw
                                                                if (image.Width > image.Height)
                                                                {
                                                                    if (image.Width > imgRect.Width)
                                                                        imgRect.Width = imgRect.Width;
                                                                    else
                                                                        imgRect.Width = image.Width;
                                                                    imgRect.Height = (int)(imgRect.Width * Aspect);
                                                                }
                                                                else
                                                                {
                                                                    if (image.Height > imgRect.Height)
                                                                        imgRect.Height = imgRect.Height;
                                                                    else
                                                                        imgRect.Height = image.Height;
                                                                    imgRect.Width = (int)(imgRect.Height * Aspect);
                                                                }

                                                                // Draw image
                                                                g.DrawImage(img.image.image, imgRect);
                                                            }
                                                    }

                                                    #endregion

                                                    #region Draw text

                                                    if (!String.IsNullOrEmpty(gd.Text))
                                                    {
                                                        // Check placement data
                                                        if ((gd.TextLocation.X == -1) && (gd.TextLocation.Y == -1))
                                                            gd.TextLocation = new OpenMobile.Graphics.Point(0, 0);

                                                        // Calculate placement (if no icon it's centered in graphic)
                                                        rect = new System.Drawing.Rectangle(rect.Width + gd.TextLocation.X, gd.TextLocation.Y, (bmp.Width - 1) - rect.Width, bmp.Height - 1);

                                                        // Set style
                                                        StringFormat sf = new StringFormat();
                                                        if (String.IsNullOrEmpty(gd.Icon))
                                                            sf.Alignment = StringAlignment.Center;
                                                        else
                                                            sf.Alignment = StringAlignment.Near;
                                                        sf.LineAlignment = StringAlignment.Center;

                                                        // Get font
                                                        System.Drawing.Font f = (gd.TextFont.Name == null ? new System.Drawing.Font(OpenMobile.Graphics.Font.Arial.Name, 36) : new System.Drawing.Font(gd.TextFont.Name, gd.TextFont.Size));

                                                        // Draw text
                                                        GraphicsPath gpTxt = new GraphicsPath();
                                                        gpTxt.AddString(gd.Text, f.FontFamily, (int)System.Drawing.FontStyle.Regular, f.Size, rect, sf);
                                                        g.FillPath(new System.Drawing.SolidBrush(textColor), gpTxt);
                                                    }

                                                    #endregion
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        g.Flush();
                                    }
                                    gd.Image = new OpenMobile.Graphics.OImage(bmp);

                                    #endregion
                                }
                                break;
                            case ButtonGraphic.GraphicStyles.Style1:
                                {
                                    #region Create button graphic with style 1

                                    System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                                    {
                                        switch (gd.ImageType)
                                        {
                                            case ButtonGraphic.ImageTypes.ButtonBackgroundFocused:
                                            case ButtonGraphic.ImageTypes.ButtonBackground:
                                                {
                                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                                    #region Colors

                                                    Color glowColor;
                                                    if (String.IsNullOrEmpty(gd.BackgroundFocusColor.Name) || gd.BackgroundFocusColor.Name == "0")
                                                        glowColor = System.Drawing.Color.FromArgb(0, 0, 255);
                                                    else
                                                        glowColor = gd.BackgroundFocusColor.ToSystemColor();

                                                    Color backColor;
                                                    if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                                        //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                                        backColor = Color.FromArgb(255, 0x7, 0x7, 0x7);
                                                    else
                                                        backColor = gd.BackgroundColor.ToSystemColor();

                                                    Color borderColor;
                                                    if (String.IsNullOrEmpty(gd.BorderColor.Name) || gd.BorderColor.Name == "0")
                                                        borderColor = System.Drawing.Color.FromArgb(125, System.Drawing.Color.White);
                                                    else
                                                        borderColor = gd.BorderColor.ToSystemColor();

                                                    #endregion

                                                    Rectangle OuterRect = new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1);

                                                    int scalingDividend = Math.Min(OuterRect.Width, OuterRect.Height);
                                                    int rectCornerRadius = Math.Max(1, scalingDividend / 5);
                                                    float rectOutlineWidth = Math.Max(1, scalingDividend / 50);

                                                    Color ColorBackgroundGradientTop = Color.FromArgb(255, 30, 30, 30); //Color.FromArgb(255, 44, 85, 177);
                                                    Color ColorBackgroundGradientBottom = Color.FromArgb(255, 7, 7, 7); //Color.FromArgb(255, 153, 198, 241);

                                                    using (GraphicsPath OuterPath = CreateRoundRectangle(OuterRect, rectCornerRadius))
                                                    {
                                                        g.SetClip(OuterPath);

                                                        // Draw background
                                                        using (LinearGradientBrush OuterBrush = new LinearGradientBrush(OuterRect, ColorBackgroundGradientTop, ColorBackgroundGradientBottom, LinearGradientMode.Vertical))
                                                        {
                                                            g.FillPath(OuterBrush, OuterPath);
                                                        }

                                                        // Draw bevels 
                                                        int BevelHeight = (int)(OuterRect.Height * 1F);
                                                        int BevelWidth = Math.Max(1, scalingDividend / 70);
                                                        int OffsetPlacement = (int)rectOutlineWidth * 2;
                                                        Rectangle LeftBevel = new Rectangle(OuterRect.X + OffsetPlacement, OuterRect.Y, BevelWidth, BevelHeight);
                                                        using (LinearGradientBrush innerBrush = new LinearGradientBrush(LeftBevel, Color.FromArgb(30, Color.White), Color.FromArgb(20, Color.White), LinearGradientMode.Vertical))
                                                            g.FillRectangle(innerBrush, LeftBevel);
                                                        Rectangle RightBevel = new Rectangle(OuterRect.X + OuterRect.Width - OffsetPlacement, OuterRect.Y, BevelWidth, BevelHeight);
                                                        using (LinearGradientBrush innerBrush = new LinearGradientBrush(RightBevel, Color.FromArgb(255, Color.Black), Color.FromArgb(255, Color.Black), LinearGradientMode.Vertical))
                                                            g.FillRectangle(innerBrush, RightBevel);


                                                        // Draw outline
                                                        using (Pen OutlinePen = new Pen(Color.Black, rectOutlineWidth))
                                                        {
                                                            OutlinePen.Alignment = PenAlignment.Inset;
                                                            g.DrawPath(OutlinePen, OuterPath);
                                                        }

                                                        // Draw gloss effect
                                                        Rectangle HighlightRect = new Rectangle(OuterRect.X + OffsetPlacement, OuterRect.Y, OuterRect.Width - (OffsetPlacement * 2), (int)(OuterRect.Height * 0.4F));
                                                        int highlightRectOffset = Math.Max(1, scalingDividend / 40);
                                                        int highlightAlphaTop = 60;
                                                        int highlightAlphaBottom = 0;
                                                        using (GraphicsPath innerPath = RoundedRectangle(HighlightRect, (int)(rectCornerRadius * 0.8F), highlightRectOffset))
                                                        {
                                                            using (LinearGradientBrush innerBrush = new LinearGradientBrush(HighlightRect,
                                                                    Color.FromArgb(highlightAlphaTop, Color.White),
                                                                    Color.FromArgb(highlightAlphaBottom, Color.White), LinearGradientMode.Vertical))
                                                            {
                                                                g.FillPath(innerBrush, innerPath);
                                                            }
                                                        }

                                                        // Draw icon
                                                        GraphicsPath IconPath = new GraphicsPath();


                                                        // Set font format
                                                        StringFormat sf = new StringFormat();
                                                        sf.Alignment = StringAlignment.Center;
                                                        sf.LineAlignment = StringAlignment.Center;
                                                        System.Drawing.Font f = (gd.IconFont.Name == null ? new System.Drawing.Font(OpenMobile.Graphics.Font.Webdings.Name, 76) : new System.Drawing.Font(gd.IconFont.Name, gd.IconFont.Size));
                                                        SizeF FSize = g.MeasureString(":", f, new SizeF(1000, 600), sf);
                                                        float Scale = (OuterRect.Height / FSize.Height) * 1.1F;
                                                        f = new Font(f.Name, f.Size * Scale);

                                                        IconPath.AddString(":", f.FontFamily, (int)System.Drawing.FontStyle.Regular, f.Size, OuterRect, sf);
                                                        g.FillPath(Brushes.White, IconPath);
                                                        using (Pen IconPen = new Pen(Color.Black, 1F))
                                                        {
                                                            IconPen.Alignment = PenAlignment.Inset;
                                                            g.DrawPath(IconPen, IconPath);
                                                        }

                                                        g.SetClip(IconPath);
                                                        /*
                                                        Rectangle IconEffectRect = new Rectangle(OuterRect.X + (int)(OuterRect.Width * 0.3F), OuterRect.Y + (int)(OuterRect.Height * 0.4f), OuterRect.Width, OuterRect.Height);
                                                        using (LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(
                                                           IconEffectRect,
                                                           Color.FromArgb(255, Color.Blue),
                                                           Color.FromArgb(0, Color.Blue),
                                                           LinearGradientMode.Horizontal))
                                                        {
                                                            g.FillEllipse(myLinearGradientBrush, IconEffectRect);
                                                        }
                                                        */

                                                        sf.Dispose();
                                                        IconPath.Dispose();
                                                        f.Dispose();

                                                        g.ResetClip();
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        g.Flush();
                                    }
                                    gd.Image = new OpenMobile.Graphics.OImage(bmp);

                                    #endregion
                                }
                                break;
                        }


                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));
                    }
                    break;

                case "panelpopupoutline":
                    {
                        // Convert input data to local data object (data sent in is a 
                        PanelPopupOutlineGraphic.GraphicData gd = data as PanelPopupOutlineGraphic.GraphicData;

                        #region Create Panel Outline graphic

                        System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        {
                            switch (gd.Type)
                            {
                                case PanelPopupOutlineGraphic.Types.RoundedRectangle:
                                    {
                                        #region RoundedRectangle

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        int HeaderSize = 40;
                                        int ShadowDistance = 14;
                                        if (!gd.ShowShadow)
                                            ShadowDistance = 0;

                                        #region Colors

                                        Color backColor;
                                        if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                            //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                            backColor = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            backColor = gd.BackgroundColor.ToSystemColor();

                                        Color borderColor;
                                        if (String.IsNullOrEmpty(gd.BorderColor.Name) || gd.BorderColor.Name == "0")
                                            borderColor = System.Drawing.Color.LightGray;
                                        else
                                            borderColor = gd.BorderColor.ToSystemColor();

                                        Color textColor;
                                        if (String.IsNullOrEmpty(gd.TextColor.Name) || gd.TextColor.Name == "0")
                                            textColor = System.Drawing.Color.LightGray;
                                        else
                                            textColor = gd.TextColor.ToSystemColor();

                                        Color shadowColor;
                                        if (String.IsNullOrEmpty(gd.ShadowColor.Name) || gd.ShadowColor.Name == "0")
                                            shadowColor = System.Drawing.Color.Blue;
                                        else
                                            shadowColor = gd.ShadowColor.ToSystemColor();

                                        #endregion

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width - ShadowDistance, bmp.Height - ShadowDistance);
                                        GraphicsPath gp = GetPath_RoundedRectangle(g, rect, 30);

                                        // Save client area
                                        gd.ClientArea = new OpenMobile.Graphics.Rectangle(rect.X + 10, rect.Y + HeaderSize + 10, rect.Width - 20, rect.Height - HeaderSize - 20);

                                        // Save client area
                                        gd.HeaderArea = new OpenMobile.Graphics.Rectangle(rect.X, rect.Y, rect.Width, HeaderSize);

                                        #region Draw drop shadow

                                        // Sample code found here: http://www.codeproject.com/Articles/15847/Fuzzy-DropShadows-in-GDI

                                        if (ShadowDistance > 0)
                                        {
                                            // Move shadow off to the side of the object
                                            System.Drawing.Rectangle rectShadow = new System.Drawing.Rectangle(0, 0, bmp.Width - ShadowDistance, bmp.Height - ShadowDistance);
                                            GraphicsPath gpShadow = GetPath_RoundedRectangle(g, rectShadow, 30);
                                            Matrix _Matrix = new Matrix();
                                            _Matrix.Translate(ShadowDistance, ShadowDistance);
                                            gpShadow.Transform(_Matrix);

                                            using (PathGradientBrush _Brush = new PathGradientBrush(gpShadow))
                                            {
                                                // set the wrapmode so that the colors will layer themselves
                                                // from the outer edge in
                                                _Brush.WrapMode = WrapMode.Clamp;

                                                // Create a color blend to manage our colors and positions and
                                                // since we need 3 colors set the default length to 3
                                                ColorBlend _ColorBlend = new ColorBlend(3);

                                                // here is the important part of the shadow making process, remember
                                                // the clamp mode on the colorblend object layers the colors from
                                                // the outside to the center so we want our transparent color first
                                                // followed by the actual shadow color. Set the shadow color to a 
                                                // slightly transparent DimGray, I find that it works best.|
                                                //_ColorBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(180, Color.DimGray), Color.FromArgb(180, Color.DimGray) };
                                                _ColorBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(180, shadowColor), Color.FromArgb(180, shadowColor) };

                                                // our color blend will control the distance of each color layer
                                                // we want to set our transparent color to 0 indicating that the 
                                                // transparent color should be the outer most color drawn, then
                                                // our Dimgray color at about 10% of the distance from the edge
                                                _ColorBlend.Positions = new float[] { 0f, .1f, 1f };

                                                // assign the color blend to the pathgradientbrush
                                                _Brush.InterpolationColors = _ColorBlend;

                                                // fill the shadow with our pathgradientbrush
                                                g.FillPath(_Brush, gpShadow);
                                            }
                                        }

                                        #endregion

                                        g.SetClip(gp);

                                        // Create border pen
                                        Pen BorderPen = new System.Drawing.Pen(borderColor, 3);

                                        #region Draw Background

                                        // Draw background
                                        using (GraphicsPath bb = CreateRoundRectangle(rect, 2))
                                        {
                                            using (System.Drawing.Brush br = new System.Drawing.SolidBrush(backColor))
                                                g.FillPath(br, bb);
                                        }

                                        #endregion

                                        #region Draw Header

                                        if (!String.IsNullOrEmpty(gd.Text))
                                        {
                                            System.Drawing.Rectangle rectHeader = new System.Drawing.Rectangle(0, 0, rect.Width, HeaderSize);

                                            // Draw background
                                            using (GraphicsPath bb = CreateRoundRectangle(rectHeader, 2))
                                            {
                                                using (System.Drawing.Brush br = new System.Drawing.SolidBrush(backColor))
                                                    g.FillPath(br, bb);
                                            }

                                            if (!gd.DisableGlossyHeader)
                                            {
                                                System.Drawing.Rectangle rect2 = rectHeader;
                                                rect2.Height >>= 1;
                                                rect2.Height++;
                                                System.Drawing.Color shineColor = System.Drawing.Color.FromArgb(85, 98, 130); // System.Drawing.Color.White;
                                                using (GraphicsPath bh = CreateTopRoundRectangle(rect2, 2))
                                                {
                                                    rect2.Height++;
                                                    int opacity = 0x99;
                                                    using (LinearGradientBrush br = new LinearGradientBrush(rect2, System.Drawing.Color.FromArgb(opacity, shineColor), System.Drawing.Color.FromArgb(opacity / 3, shineColor), LinearGradientMode.Vertical))
                                                        g.FillPath(br, bh);
                                                }
                                                rect2.Height -= 2;
                                            }
                                            g.DrawLine(BorderPen, new Point(rectHeader.Left, rectHeader.Bottom), new Point(rectHeader.Right, rectHeader.Bottom));

                                            #region Draw text

                                            if (!String.IsNullOrEmpty(gd.Text))
                                            {
                                                // Set style
                                                StringFormat sf = new StringFormat();
                                                sf.Alignment = StringAlignment.Center;
                                                sf.LineAlignment = StringAlignment.Center;

                                                // Get font
                                                System.Drawing.Font f = (gd.TextFont.Name == null ? new System.Drawing.Font(OpenMobile.Graphics.Font.Arial.Name, 36) : new System.Drawing.Font(gd.TextFont.Name, gd.TextFont.Size));

                                                // Draw text
                                                GraphicsPath gpTxt = new GraphicsPath();
                                                gpTxt.AddString(gd.Text, f.FontFamily, (int)System.Drawing.FontStyle.Regular, f.Size, rectHeader, sf);
                                                g.FillPath(new System.Drawing.SolidBrush(textColor), gpTxt);
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        g.ResetClip();

                                        #region Draw outer border

                                        g.DrawPath(BorderPen, gp);

                                        #endregion

                                        #endregion
                                    }
                                    break;
                                default:
                                    break;
                            }
                            g.Flush();
                        }
                        gd.Image = new OpenMobile.Graphics.OImage(bmp);

                        #endregion

                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));
                    }
                    break;

                case "paneloutline":
                    {
                        // Convert input data to local data object (data sent in is a 
                        PanelOutlineGraphic.GraphicData gd = data as PanelOutlineGraphic.GraphicData;

                        #region Create Panel Outline graphic

                        System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        {
                            switch (gd.Type)
                            {
                                case PanelOutlineGraphic.Types.RoundedRectangle:
                                    {
                                        #region RoundedRectangle

                                        bool CustomPath = false;

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        int ShadowDistance = gd.ShadowSize;
                                        if (gd.ShadowType == PanelOutlineGraphic.ShadowTypes.None)
                                            ShadowDistance = 0;

                                        #region Colors

                                        Color backColor;
                                        if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                            //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                            backColor = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            backColor = gd.BackgroundColor.ToSystemColor();

                                        Color borderColor;
                                        if (String.IsNullOrEmpty(gd.BorderColor.Name) || gd.BorderColor.Name == "0")
                                            borderColor = System.Drawing.Color.LightGray;
                                        else
                                            borderColor = gd.BorderColor.ToSystemColor();

                                        Color shadowColor;
                                        if (String.IsNullOrEmpty(gd.ShadowColor.Name) || gd.ShadowColor.Name == "0")
                                            shadowColor = System.Drawing.Color.Blue;
                                        else
                                            shadowColor = gd.ShadowColor.ToSystemColor();

                                        #endregion

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)(ShadowDistance * 1.5f), ShadowDistance, bmp.Width - (ShadowDistance * 3), bmp.Height - (ShadowDistance * 2));
                                        GraphicsPath gp = gd.GraphicPath;
                                        if (gp == null)
                                            gp = GetPath_RoundedRectangle(g, rect, 30);
                                        else
                                            CustomPath = true;

                                        // Save client area
                                        gd.ClientArea = new OpenMobile.Graphics.Rectangle(rect.X + 10, rect.Y + 10, rect.Width - 20, rect.Height - 20);

                                        #region Draw drop shadow

                                        // Sample code found here: http://www.codeproject.com/Articles/15847/Fuzzy-DropShadows-in-GDI

                                        if (ShadowDistance > 0)
                                        {
                                            // Move shadow off to the side of the object
                                            System.Drawing.Rectangle rectShadow = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
                                            //GraphicsPath gpShadow = GetPath_RoundedRectangle(g, rectShadow, 30);
                                            GraphicsPath gpShadow = (GraphicsPath)gp.Clone();
                                            gpShadow.Widen(new Pen(Color.Transparent, ShadowDistance*2));
                                            gpShadow.CloseAllFigures();

                                            using (PathGradientBrush _Brush = new PathGradientBrush(gpShadow))
                                            {
                                                // set the wrapmode so that the colors will layer themselves
                                                // from the outer edge in
                                                _Brush.WrapMode = WrapMode.Clamp;

                                                // Create a color blend to manage our colors and positions and
                                                // since we need 3 colors set the default length to 3
                                                ColorBlend _ColorBlend = new ColorBlend(3);

                                                // here is the important part of the shadow making process, remember
                                                // the clamp mode on the colorblend object layers the colors from
                                                // the outside to the center so we want our transparent color first
                                                // followed by the actual shadow color. Set the shadow color to a 
                                                // slightly transparent DimGray, I find that it works best.|
                                                //_ColorBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(180, Color.DimGray), Color.FromArgb(180, Color.DimGray) };
                                                _ColorBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(180, shadowColor), Color.FromArgb(180, shadowColor) };

                                                // our color blend will control the distance of each color layer
                                                // we want to set our transparent color to 0 indicating that the 
                                                // transparent color should be the outer most color drawn, then
                                                // our Dimgray color at about 10% of the distance from the edge
                                                _ColorBlend.Positions = new float[] { 0f, .1f, 1f };

                                                // assign the color blend to the pathgradientbrush
                                                _Brush.InterpolationColors = _ColorBlend;

                                                // fill the shadow with our pathgradientbrush
                                                g.FillPath(_Brush, gpShadow);
                                            }
                                        }

                                        #endregion

                                        g.SetClip(gp);

                                        // Create border pen
                                        Pen BorderPen = new System.Drawing.Pen(borderColor, gd.BorderThickness);

                                        #region Draw Background

                                        // Draw background
                                        using (System.Drawing.Brush br = new System.Drawing.SolidBrush(backColor))
                                            g.FillPath(br, gp);

                                        #endregion

                                        g.ResetClip();

                                        #region Draw outer border

                                        g.DrawPath(BorderPen, gp);

                                        #endregion

                                        #endregion
                                    }
                                    break;
                                default:
                                    break;
                            }
                            g.Flush();
                        }
                        gd.Image = new OpenMobile.Graphics.OImage(bmp);

                        #endregion

                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));
                    }
                    break;

                case "fadingedge":
                    {
                        // Convert input data to local data object (data sent in is a 
                        FadingEdge.GraphicData gd = data as FadingEdge.GraphicData;

                        #region Create a fading edge style graphic

                        System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        {
                            switch (gd.Type)
                            {
                                case FadingEdge.Types.RoundedRectangle:
                                    {
                                        #region RoundedRectangle

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        #region Colors

                                        Color Color1;
                                        if (String.IsNullOrEmpty(gd.Color1.Name) || gd.Color1.Name == "0")
                                            //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                            Color1 = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            Color1 = gd.Color1.ToSystemColor();

                                        Color Color2;
                                        if (String.IsNullOrEmpty(gd.Color2.Name) || gd.Color2.Name == "0")
                                            Color2 = System.Drawing.Color.Transparent;
                                        else
                                            Color2 = gd.Color2.ToSystemColor();

                                        #endregion

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

                                        System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                                        int d = 30;
                                        gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
                                        gp.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
                                        gp.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
                                        gp.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
                                        gp.AddLine(rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2);

                                        #region Draw inner shadow

                                        using (PathGradientBrush _Brush = new PathGradientBrush(gp))
                                        {
                                            _Brush.WrapMode = WrapMode.Clamp;
                                            ColorBlend _ColorBlend = new ColorBlend(3);
                                            _ColorBlend.Colors = new Color[] { Color1, Color2, Color2 };
                                            _ColorBlend.Positions = new float[] { 0f, gd.FadeSize, 1f };
                                            _Brush.InterpolationColors = _ColorBlend;
                                            g.FillPath(_Brush, gp);
                                        }
                                        #endregion

                                        #endregion
                                    }
                                    break;

                                case FadingEdge.Types.Rectangle:
                                    {
                                        #region Rectangle

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        #region Colors

                                        Color Color1;
                                        if (String.IsNullOrEmpty(gd.Color1.Name) || gd.Color1.Name == "0")
                                            //backColor = Color.FromArgb(0x7f, System.Drawing.Color.Black);
                                            Color1 = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            Color1 = gd.Color1.ToSystemColor();

                                        Color Color2;
                                        if (String.IsNullOrEmpty(gd.Color2.Name) || gd.Color2.Name == "0")
                                            Color2 = System.Drawing.Color.Transparent;
                                        else
                                            Color2 = gd.Color2.ToSystemColor();

                                        #endregion

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

                                        int GradientSize = (int)((bmp.Width / 2) * gd.FadeSize);

                                        // Left side
                                        if ((gd.Sides & FadingEdge.GraphicSides.Left) == FadingEdge.GraphicSides.Left)
                                        {
                                            Rectangle rectGradient = new Rectangle(rect.X, rect.Y, GradientSize, rect.Height);
                                            LinearGradientBrush lgb = new LinearGradientBrush(rectGradient, Color1, Color2, 0f);
                                            // Addjust size of drawn rectangle to mask unwanted graphics
                                            Rectangle rectGradientDraw = rectGradient;
                                            rectGradientDraw.Width = rectGradient.Width - 1;
                                            // Draw gradient
                                            g.FillRectangle(lgb, rectGradientDraw);
                                        }

                                        // Right side
                                        if ((gd.Sides & FadingEdge.GraphicSides.Right) == FadingEdge.GraphicSides.Right)
                                        {
                                            Rectangle rectGradient = new Rectangle(rect.X + rect.Width - GradientSize, rect.Y, GradientSize, rect.Height);
                                            LinearGradientBrush lgb = new LinearGradientBrush(rectGradient, Color1, Color2, 180f);
                                            // Addjust size of drawn rectangle to mask unwanted graphics
                                            Rectangle rectGradientDraw = rectGradient;
                                            rectGradientDraw.Width = rectGradient.Width - 1;
                                            // Draw gradient
                                            g.FillRectangle(lgb, rectGradientDraw);
                                        }

                                        // Top side
                                        if ((gd.Sides & FadingEdge.GraphicSides.Top) == FadingEdge.GraphicSides.Top)
                                        {
                                            Rectangle rectGradient = new Rectangle(rect.X, rect.Y, rect.Width, GradientSize);
                                            LinearGradientBrush lgb = new LinearGradientBrush(rectGradient, Color1, Color2, 90f);
                                            // Addjust size of drawn rectangle to mask unwanted graphics
                                            Rectangle rectGradientDraw = rectGradient;
                                            rectGradientDraw.Y = rectGradient.Y - 1;
                                            rectGradientDraw.Height = rectGradient.Height - 1;
                                            // Draw gradient
                                            g.FillRectangle(lgb, rectGradientDraw);
                                        }

                                        // Bottom side
                                        if ((gd.Sides & FadingEdge.GraphicSides.Bottom) == FadingEdge.GraphicSides.Bottom)
                                        {
                                            Rectangle rectGradient = new Rectangle(rect.X, rect.Y + rect.Height - GradientSize, rect.Width, GradientSize);
                                            LinearGradientBrush lgb = new LinearGradientBrush(rectGradient, Color1, Color2, -90f);
                                            // Addjust size of drawn rectangle to mask unwanted graphics
                                            Rectangle rectGradientDraw = rectGradient;
                                            rectGradientDraw.Y = rectGradient.Y + 1;
                                            rectGradientDraw.Height = rectGradient.Height - 1;
                                            // Draw gradient
                                            g.FillRectangle(lgb, rectGradientDraw);
                                        }

                                        #endregion
                                    }
                                    break;

                                default:
                                    break;
                            }
                            g.Flush();
                        }
                        gd.Image = new OpenMobile.Graphics.OImage(bmp);

                        #endregion

                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));
                    }
                    break;

                case "mediagraphic":
                    {
                        // Convert input data to local data object (data sent in is a 
                        MediaGraphic.GraphicData gd = data as MediaGraphic.GraphicData;

                        switch (gd.Style)
                        {
                            case MediaGraphic.GraphicStyles.NoCover:
                                {
                                    #region NoCover graphics

                                    System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                                    {

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        #region Colors

                                        Color backColor;
                                        if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                            backColor = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            backColor = gd.BackgroundColor.ToSystemColor();

                                        Color foreColor;
                                        if (String.IsNullOrEmpty(gd.ForegroundColor.Name) || gd.ForegroundColor.Name == "0")
                                            foreColor = System.Drawing.Color.FromArgb(125, System.Drawing.Color.White);
                                        else
                                            foreColor = gd.ForegroundColor.ToSystemColor();

                                        #endregion

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(1, 1, bmp.Width - 2, bmp.Height - 2);
                                        GraphicsPath gp = new GraphicsPath();
                                        gp.AddRectangle(rect);

                                        #region Draw background

                                        PathGradientBrush pgb = new PathGradientBrush(gp);

                                        pgb.CenterPoint = new PointF(rect.Width / 2, rect.Height / 2);
                                        pgb.CenterColor = Color.Black;
                                        pgb.SurroundColors = new Color[] { Color.FromArgb(127, foreColor) };

                                        pgb.FocusScales = new PointF(0f, 0f);
                                        Blend blnd = new Blend();
                                        blnd.Positions = new float[] { 0f, 1f };
                                        blnd.Factors = new float[] { 1f, 0f };
                                        pgb.Blend = blnd;

                                        g.FillPath(pgb, gp);

                                        pgb.Dispose();

                                        #endregion

                                        g.SetClip(gp);

                                        #region Draw Icon

                                        string Icon = "¯";

                                        System.Drawing.Rectangle rectIcon = new System.Drawing.Rectangle(0, (int)(rect.Height * 0.1F), rect.Width, rect.Height);

                                        // Set font format
                                        StringFormat sf = new StringFormat();
                                        sf.Alignment = StringAlignment.Center;
                                        sf.LineAlignment = StringAlignment.Center;
                                        System.Drawing.Font f = new System.Drawing.Font(OpenMobile.Graphics.Font.Webdings.Name, 76);

                                        SizeF FSize = g.MeasureString(Icon, f, new SizeF(bmp.Width, bmp.Height), sf);
                                        // Calculate scaling factor
                                        float Scale = bmp.Height / FSize.Height;

                                        // Recalculate font based on scaling
                                        f = new Font(f.Name, f.Size * Scale * 1.5F);

                                        GraphicsPath gpTxt = new GraphicsPath();
                                        gpTxt.AddString(Icon, f.FontFamily, (int)System.Drawing.FontStyle.Regular, f.Size, rectIcon, sf);

                                        #region Draw Icon glow

                                        for (int i = 1; i < 15; ++i)
                                        {
                                            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32 - i, foreColor), i);
                                            pen.LineJoin = LineJoin.Round;
                                            g.DrawPath(pen, gpTxt);
                                            pen.Dispose();
                                        }

                                        #endregion

                                        System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(backColor);
                                        g.FillPath(brush, gpTxt);
                                        g.DrawPath(new System.Drawing.Pen(foreColor, 0.5F), gpTxt);

                                        #endregion

                                        g.ResetClip();

                                        #region Draw outer border

                                        g.DrawPath(new System.Drawing.Pen(foreColor, 2), gp);

                                        #endregion

                                        g.Flush();
                                    }
                                    gd.Image = new OpenMobile.Graphics.OImage(bmp);

                                    #endregion
                                }
                                break;

                            case MediaGraphic.GraphicStyles.OMLogo:
                                {
                                    #region NoCover graphics

                                    System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                                    {

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        #region Colors

                                        Color backColor;
                                        if (String.IsNullOrEmpty(gd.BackgroundColor.Name) || gd.BackgroundColor.Name == "0")
                                            backColor = Color.FromArgb(255, System.Drawing.Color.Black);
                                        else
                                            backColor = gd.BackgroundColor.ToSystemColor();

                                        Color foreColor;
                                        if (String.IsNullOrEmpty(gd.ForegroundColor.Name) || gd.ForegroundColor.Name == "0")
                                            foreColor = System.Drawing.Color.FromArgb(125, System.Drawing.Color.White);
                                        else
                                            foreColor = gd.ForegroundColor.ToSystemColor();

                                        #endregion

                                        // Load source image
                                        imageItem img = BuiltInComponents.Host.getSkinImage("OMIconBlack_Transparent_Clean");

                                        // Get region
                                        //Region rgn = GetRegion(img.image.image, System.Drawing.Color.Black);
                                        //g.FillRegion(Brushes.Blue, rgn);
                                        //GraphicsPath gp = new GraphicsPath();
                                        //gp.AddRectangles(rgn.GetRegionScans(new Matrix()));
                                        //Graphics.DrawPath((Pen), (RegionToPath?)(Form).Region);

                                        GraphicsPath gp = GetGraphicsPathOutline(img.image.image, System.Drawing.Color.Black);

                                        // Create outline path
                                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(1, 1, bmp.Width - 2, bmp.Height - 2);
                                        //GraphicsPath gp = GetGraphicsPath(img.image.image, System.Drawing.Color.Black);
                                        //GraphicsPath gp = GetGraphicsPathFilled(img.image.image, System.Drawing.Color.Black);

                                        #region Draw background

                                        PathGradientBrush pgb = new PathGradientBrush(gp);

                                        pgb.CenterPoint = new PointF(rect.Width / 2, rect.Height / 2);
                                        pgb.CenterColor = Color.Black;
                                        pgb.SurroundColors = new Color[] { Color.FromArgb(127, foreColor) };

                                        pgb.FocusScales = new PointF(0f, 0f);
                                        Blend blnd = new Blend();
                                        blnd.Positions = new float[] { 0f, 1f };
                                        blnd.Factors = new float[] { 1f, 0f };
                                        pgb.Blend = blnd;

                                        g.FillPath(pgb, gp);

                                        pgb.Dispose();

                                        #endregion

                                        //g.SetClip(gp);

                                        #region Draw glow
                                        /*
                                        for (int i = 1; i < 15; ++i)
                                        {
                                            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32 - i, foreColor), i);
                                            pen.LineJoin = LineJoin.Round;
                                            g.DrawPath(pen, gp);
                                            pen.Dispose();
                                        }
                                        */
                                        #endregion

                                        System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(foreColor);
                                        g.FillPath(brush, gp);
                                        //g.DrawPath(new System.Drawing.Pen(foreColor, 0.5F), gp);

                                        g.ResetClip();

                                        #region Draw outer border

                                        g.DrawPath(new System.Drawing.Pen(foreColor, 2), gp);


                                        #endregion

                                        g.Flush();
                                    }
                                    gd.Image = new OpenMobile.Graphics.OImage(bmp);

                                    #endregion
                                }
                                break;

                            default:
                                break;
                        }

                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));
                    }
                    break;

                case "textgraphics":
                    {
                        #region textgraphics

                        // Convert input data to local data object (data sent in is a 
                        TextGraphics.GraphicData gd = data as TextGraphics.GraphicData;
                        System.Drawing.Bitmap bmp = new Bitmap(gd.Width, gd.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        {
                            switch (gd.Style)
                            {
                                case TextGraphics.GraphicStyles.Glow:
                                    {

                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        #region Colors

                                        Color glowColor;
                                        if (String.IsNullOrEmpty(gd.EffectColor.Name) || gd.EffectColor.Name == "0")
                                            glowColor = System.Drawing.Color.FromArgb(0, 0, 255);
                                        else
                                            glowColor = gd.EffectColor.ToSystemColor();

                                        Color textColor;
                                        if (String.IsNullOrEmpty(gd.TextColor.Name) || gd.TextColor.Name == "0")
                                            textColor = System.Drawing.Color.LightGray;
                                        else
                                            textColor = gd.TextColor.ToSystemColor();

                                        #endregion

                                        System.Drawing.Rectangle rect = new Rectangle(0, 0, 0, 0);

                                        // Calculate placement (if no text it's centered in graphic)
                                        rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

                                        // Set font format
                                        StringFormat sf = OpenMobile.Graphics.Font.AlignmentToStringFormat(gd.textAlignment);
                                        System.Drawing.Font f = (gd.TextFont.Name == null ? new System.Drawing.Font(OpenMobile.Graphics.Font.Arial.Name, 18) : new System.Drawing.Font(gd.TextFont.Name, gd.TextFont.Size));

                                        GraphicsPath gpTxt = new GraphicsPath();
                                        gpTxt.AddString(gd.Text, f.FontFamily, (int)OpenMobile.Graphics.Font.FormatToStyle(gd.textFormat), f.Size, rect, sf);

                                        #region Draw glow

                                        for (int i = 1; i < 15; ++i)
                                        {
                                            //System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32, 0, 128, 192), i);
                                            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32 - i, glowColor), i);
                                            pen.LineJoin = LineJoin.Round;
                                            g.DrawPath(pen, gpTxt);
                                            pen.Dispose();
                                        }

                                        #endregion

                                        if (textColor != Color.Transparent)
                                        {
                                            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(textColor);
                                            g.FillPath(brush, gpTxt);
                                        }
                                        //g.DrawPath(new System.Drawing.Pen(glowColor, 0.5F), gpTxt);

                                    }
                                    break;

                                default:
                                    break;
                            }
                            g.Flush();
                        }
                        gd.Image = new OpenMobile.Graphics.OImage(bmp);

                        // Return data
                        data = (T)Convert.ChangeType(gd, typeof(T));

                        #endregion
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            return eLoadStatus.LoadSuccessful;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static void DrawRoundedRectangle(System.Drawing.Graphics g,
                System.Drawing.Rectangle r, int d, System.Drawing.Pen p)
        {

            System.Drawing.Drawing2D.GraphicsPath gp =

                    new System.Drawing.Drawing2D.GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            g.DrawPath(p, gp);
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetPath_RoundedRectangle(System.Drawing.Graphics g, System.Drawing.Rectangle r, int d)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);
            return gp;
        }

        private static GraphicsPath CreateBottomRadialPath(System.Drawing.Rectangle rectangle)
        {
            GraphicsPath path = new GraphicsPath();
            System.Drawing.RectangleF rect = rectangle;
            rect.X -= rect.Width * .35f;
            rect.Y -= rect.Height * .15f;
            rect.Width *= 1.7f;
            rect.Height *= 2.3f;
            path.AddEllipse(rect);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateRoundRectangle(System.Drawing.Rectangle rectangle, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int l = rectangle.Left;
            int t = rectangle.Top;
            int w = rectangle.Width;
            int h = rectangle.Height;
            int d = radius << 1;

            path.AddArc(l, t, d, d, 180, 90); // topleft
            path.AddLine(l + radius, t, l + w - radius, t); // top
            path.AddArc(l + w - d, t, d, d, 270, 90); // topright
            path.AddLine(l + w, t + radius, l + w, t + h - radius); // right
            path.AddArc(l + w - d, t + h - d, d, d, 0, 90); // bottomright
            path.AddLine(l + w - radius, t + h, l + radius, t + h); // bottom
            path.AddArc(l, t + h - d, d, d, 90, 90); // bottomleft
            path.AddLine(l, t + h - radius, l, t + radius); // left
            path.CloseFigure();

            return path;
        }

        private static GraphicsPath CreateTopRoundRectangle(System.Drawing.Rectangle rectangle, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int l = rectangle.Left;
            int t = rectangle.Top;
            int w = rectangle.Width;
            int h = rectangle.Height;
            int d = radius << 1;
            path.AddArc(l, t, d, d, 180, 90); // topleft
            path.AddLine(l + radius, t, l + w - radius, t); // top
            path.AddArc(l + w - d, t, d, d, 270, 90); // topright
            path.AddLine(l + w, t + radius, l + w, t + h); // right
            path.AddLine(l + w, t + h, l, t + h); // bottom
            path.AddLine(l, t + h, l, t + radius); // left
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath RoundedRectangle(Rectangle boundingRect, int cornerRadius, int margin)
        {
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(boundingRect.X + margin, boundingRect.Y + margin, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddArc(boundingRect.X + boundingRect.Width - margin - cornerRadius * 2, boundingRect.Y + margin, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddArc(boundingRect.X + boundingRect.Width - margin - cornerRadius * 2, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddArc(boundingRect.X + margin, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(boundingRect.X + margin, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, boundingRect.X + margin, boundingRect.Y + margin + cornerRadius);
            roundedRect.CloseFigure();
            return roundedRect;
        }

        private static Region GetRegion(Bitmap _img, Color color)
        {
            Color _matchColor = Color.FromArgb(color.R, color.G, color.B);
            System.Drawing.Region rgn = new Region();
            rgn.MakeEmpty();
            Rectangle rc = new Rectangle(0, 0, 0, 0);
            bool inimage = false;
            for (int y = 0; y < _img.Height; y++)
            {
                for (int x = 0; x < _img.Width; x++)
                {
                    if (!inimage)
                    {
                        if (_img.GetPixel(x, y) != _matchColor)
                        {
                            inimage = true;
                            rc.X = x;
                            rc.Y = y;
                            rc.Height = 1;
                        }
                    }
                    else
                    {
                        if (_img.GetPixel(x, y) == _matchColor)
                        {
                            inimage = false;
                            rc.Width = x - rc.X;
                            rgn.Union(rc);
                        }
                    }

                }
                if (inimage)
                {
                    inimage = false;
                    rc.Width = _img.Width - rc.X;
                    rgn.Union(rc);
                }
            }

            return rgn;

        }

        private static GraphicsPath GetGraphicsPathOutline(Bitmap _img, Color color)
        {
            Color _matchColor = Color.FromArgb(color.R, color.G, color.B);
            GraphicsPath path = new GraphicsPath();
            Rectangle rc = new Rectangle(0, 0, 0, 0);
            bool inimage = false;
            rc.Height = 1;
            for (int y = 0; y < _img.Height; y++)
            {
                for (int x = 0; x < _img.Width; x++)
                {
                    if (!inimage)
                    {
                        if (_img.GetPixel(x, y) != _matchColor)
                        {
                            inimage = true;
                            path.AddRectangle(new Rectangle(x, y, 1, 1));
                        }
                        else if (y < _img.Height - 1 && _img.GetPixel(x, y + 1) != _matchColor)
                        {
                            path.AddRectangle(new Rectangle(x, y + 1, 1, 1));
                        }
                    }
                    else
                    {
                        if (_img.GetPixel(x, y) == _matchColor)
                        {
                            inimage = false;
                            path.AddRectangle(new Rectangle(x, y, 1, 1));
                        }
                        else if (y < _img.Height - 1 && _img.GetPixel(x, y + 1) == _matchColor)
                        {
                            path.AddRectangle(new Rectangle(x, y + 1, 1, 1));
                        }
                    }

                }

                if (inimage)
                {
                    inimage = false;

                }
            }

            path.CloseAllFigures();

            return path;
        }

        private static GraphicsPath GetGraphicsPathFilled(Bitmap _img, Color color)
        {
            Color _matchColor = Color.FromArgb(color.R, color.G, color.B);
            GraphicsPath path = new GraphicsPath();
            Rectangle rc = new Rectangle(0, 0, 0, 0);
            bool inimage = false;
            rc.Height = 1;
            for (int y = 0; y < _img.Height; y++)
            {
                for (int x = 0; x < _img.Width; x++)
                {
                    if (!inimage)
                    {
                        Color c = _img.GetPixel(x, y);
                        if (_img.GetPixel(x, y) != _matchColor)
                        {
                            inimage = true;
                            rc.X = x;
                            rc.Y = y;
                            rc.Height = 1;
                        }
                    }
                    else
                    {
                        if (_img.GetPixel(x, y) == _matchColor)
                        {
                            inimage = false;
                            rc.Width = x - rc.X;
                            path.AddRectangle(rc);
                        }
                    }

                }

                if (inimage)
                {
                    inimage = false;
                    rc.Width = _img.Width - rc.X;
                    path.AddRectangle(rc);
                }
            }
            return path;
        }
    }
}
