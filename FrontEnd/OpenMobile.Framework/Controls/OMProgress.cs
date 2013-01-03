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
using System.ComponentModel;
using System.Drawing.Drawing2D;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A progress bar control
    /// </summary>
    [System.Serializable]
    public class OMProgress : OMLabel
    {
        #region Preconfigured controls

        /// <summary>
        /// Returns a preconfigured OMProgress control with a triangle layout (via ShapeData), layout is growing to the right
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="BorderColor"></param>
        /// <param name="IndicatorColor"></param>
        /// <returns></returns>
        public static OMProgress PreConfigLayout_Triangle(string name, int left, int top, int width, int height, Color BorderColor, Color IndicatorColor)
        {
            OMProgress progress = new OMProgress(name, left, top, width, height);
            progress.BackgroundColor = Color.Transparent;
            progress.Minimum = 0;
            progress.Maximum = 100;
            progress.Value = 0;
            progress.Style = OMProgress.Styles.shape;
            progress.ShowProgressBarValue = true;
            progress.FontSize = 10;
            progress.TextOffset = new Point(0, 5);

            // Create shapedata for progressbar background
            ShapeData ShapeData = new ShapeData(shapes.Polygon,
                Color.Transparent,
                Color.FromArgb(50, BorderColor),
                2,
                new Point[] 
                { 
                    new Point(0,progress.Region.Height-2), // Slighty rounded corner A
                    new Point(0,progress.Region.Height-8), // Slighty rounded corner B
                    new Point(2,progress.Region.Height-10),// Slighty rounded corner B
                    new Point(progress.Region.Width-3,0),  // Slighty rounded corner C
                    new Point(progress.Region.Width,3),    // Slighty rounded corner C
                    new Point(progress.Region.Width-0,progress.Region.Height-3),  // Slighty rounded corner D
                    new Point(progress.Region.Width-3,progress.Region.Height-0),  // Slighty rounded corner D
                    new Point(2,progress.Region.Height-0)  // Slighty rounded corner A
                });
            progress.ShapeData = ShapeData;

            // Create shapedata for progressbar bar
            ShapeData BarShapeData = new ShapeData(shapes.Polygon,
                IndicatorColor,
                IndicatorColor,
                1,
                new Point[] 
                { 
                new Point(5,progress.Region.Height-5),
                new Point(progress.Region.Width-5,5),
                new Point(progress.Region.Width-5,progress.Region.Height-5),
                new Point(5,progress.Region.Height-5)
                });
            progress.ProgressBarShapeData = BarShapeData;

            return progress;
        }

        #endregion

        #region Properties

        public enum Styles { simple, shape, image }

        /// <summary>
        /// The graphic style of this progress bar
        /// </summary>
        public Styles Style
        {
            get
            {
                return this._Style;
            }
            set
            {
                if (this._Style != value)
                {
                    this._Style = value;
                }
            }
        }
        private Styles _Style;

        /// <summary>
        /// progress value
        /// </summary>
        protected int value = 0;
        /// <summary>
        /// minimum value
        /// </summary>
        protected int minimum = 0;
        /// <summary>
        /// maximum value
        /// </summary>
        protected int maximum = 100;
        /// <summary>
        /// vertical progress bar
        /// </summary>
        protected bool vertical;
        /// <summary>
        /// Represents the first of two colors for the progress bars gradient
        /// </summary>
        protected Color firstColor = Color.DarkRed;
        /// <summary>
        /// Represents the second of two colors for the progress bars gradient
        /// </summary>
        protected Color secondColor = Color.Red;
        /// <summary>
        /// Background color
        /// </summary>
        protected Color backColor = Color.FromArgb(180, Color.White);

        /// <summary>
        /// Display the progress bar vertically instead
        /// </summary>
        public bool Vertical
        {
            get
            {
                return vertical;
            }
            set
            {
                vertical = value;
            }
        }

        /// <summary>
        /// A number between the minimum and the maximum value
        /// </summary>
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if ((value >= this.minimum) && (value <= this.maximum))
                {
                    SetValue(value, value.ToString());
                }
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 100%
        /// </summary>
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (maximum == value)
                    return;
                if (Value > value)
                    Value = value;

                // Reset overlay image
                if (ImageProgressBar.image != null)
                    _ImageProgressBarOverlay = null;

                maximum = value;

                Refresh();
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 0%
        /// </summary>
        public int Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;

                // Reset overlay image
                if (ImageProgressBar.image != null)
                    _ImageProgressBarOverlay = null;

                Refresh();
            }
        }

        /// <summary>
        /// Controls where the text is drawn on the control relative to the control itself
        /// </summary>
        public Point TextOffset
        {
            get
            {
                return this._TextOffset;
            }
            set
            {
                if (this._TextOffset != value)
                {
                    this._TextOffset = value;
                }
            }
        }
        private Point _TextOffset;

        /// <summary>
        /// True = Renders the progressbar value 
        /// </summary>
        public bool ShowProgressBarValue
        {
            get
            {
                return this._ShowProgressBarValue;
            }
            set
            {
                if (this._ShowProgressBarValue != value)
                {
                    this._ShowProgressBarValue = value;
                }
            }
        }
        private bool _ShowProgressBarValue;

        /// <summary>
        /// Background image to use
        /// </summary>
        public imageItem ImageBackground
        {
            get
            {
                return this._ImageBackground;
            }
            set
            {
                if (this._ImageBackground != value)
                {
                    this._ImageBackground = value;
                    Refresh();
                }
            }
        }
        private imageItem _ImageBackground;

        /// <summary>
        /// Image for the actual bar 
        /// </summary>
        public imageItem ImageProgressBar
        {
            get
            {
                return this._ImageProgressBar;
            }
            set
            {
                if (this._ImageProgressBar != value)
                {
                    this._ImageProgressBar = value;
                    //UpdateImageProgressBarOverlay();
                    Refresh();
                }
            }
        }
        private imageItem _ImageProgressBar;
        private OImage _ImageProgressBarOverlay;

        /// <summary>
        /// Properties for the progressbarshape
        /// </summary>
        public ShapeData ProgressBarShapeData
        {
            get
            {
                return this._ProgressBarShapeData;
            }
            set
            {
                this._ProgressBarShapeData = value;
            }
        }
        private ShapeData _ProgressBarShapeData = new ShapeData();

        #endregion

        #region methods

        private void SetValue(int value, string text)
        {
            if ((value >= this.minimum) && (value <= this.maximum))
            {
                this.value = value;

                // Reset overlay image
                //UpdateImageProgressBarOverlay();

                // Update progressbar text
                if (_ShowProgressBarValue)
                    this.Text = text;

                Refresh();
            }
        }

        #endregion

        #region Constructors

        private void Init()
        {
        }

        /// <summary>
        /// Create the control with the default size and location
        /// </summary>
        [Obsolete("Use OMProgress(string name, int x, int y, int w, int h) instead")]
        public OMProgress()
            : base("", 0, 0, 200, 200)
        {
            Init();
        }
        /// <summary>
        /// Create a new progress bar
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        [Obsolete("Use OMProgress(string name, int x, int y, int w, int h) instead")]
        public OMProgress(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
            Init();
        }
        /// <summary>
        /// Create a new progress bar
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMProgress(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            Init();
        }

        #endregion

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            // Render background (if any)
            if (background != Color.Transparent)
                using (Brush Fill = new Brush(Color.FromArgb((int)(this.GetAlphaValue255(background.A)), background)))
                    g.FillRectangle(Fill, left, top, width, height);

            switch (_Style)
            {
                case Styles.image:
                    {
                        // Draw background image
                        g.DrawImage(ImageBackground.image, left, top, width, height, base._RenderingValue_Alpha, eAngle.Normal);

                        // Draw progress bar with color overlay
                        g.DrawImage(_ImageProgressBarOverlay, left, top, width, height, base._RenderingValue_Alpha, eAngle.Normal);
                    }
                    break;
                case Styles.shape:
                    {
                        Rectangle rect = this.Region;
                        rect = new Rectangle(left, top, (int)(width * ((float)value / maximum)), height);
                        rect.Inflate(-1, -1, true);

                        if (ImageBackground.image == null)
                            ImageBackground = new imageItem(new OImage(base.DrawShapeToBMP(_ShapeData, this.Region)));
                        if (ImageProgressBar.image == null)
                            ImageProgressBar = new imageItem(new OImage(base.DrawShapeToBMP(_ProgressBarShapeData, this.Region)));

                        // Draw background image
                        g.DrawImage(ImageBackground.image, left, top, width, height, base._RenderingValue_Alpha);

                        // Draw progress bar with color overlay
                        float Percentage = ((float)value / (float)maximum);
                        g.DrawImage(_ImageProgressBar.image, new Rectangle(left, top, (int)(width * Percentage), height), 0, 0, (int)(width * Percentage), height, base._RenderingValue_Alpha);
                    }
                    break;
                case Styles.simple:
                    {
                        using (Brush b = new Brush(Color.FromArgb((int)this.GetAlphaValue255(backColor.A), backColor)))
                            g.FillRectangle(b, left, top, width, height);
                        if (vertical == false)
                        {
                            using (Brush b = new Brush(Color.FromArgb((int)this.GetAlphaValue255(firstColor.A), firstColor), Color.FromArgb((int)this.GetAlphaValue255(secondColor.A), secondColor), Gradient.Horizontal))
                                g.FillRectangle(b, left, top, (int)(width * ((float)value / maximum)), height);
                        }
                        else
                        {
                            using (Brush b = new Brush(Color.FromArgb((int)this.GetAlphaValue255(firstColor.A), firstColor), Color.FromArgb((int)this.GetAlphaValue255(secondColor.A), secondColor), Gradient.Vertical))
                                g.FillRectangle(b, left, top + height - (int)(height * ((float)value / maximum)), width, (int)(height * ((float)value / maximum)));
                        }
                    }
                    break;
                default:
                    break;
            }

            // Draw text (if any)
            if (_text != "")
            {
                if (_RefreshGraphic)
                    textTexture = g.GenerateTextTexture(textTexture, left, top, width, height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                g.DrawImage(textTexture, (left + _TextOffset.X), (top + _TextOffset.Y), width, height, _RenderingValue_Alpha);
            }

            base.RenderFinish(g, e);
        }

        #region Datasource

        internal override void DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            try
            {
                SetValue(Convert.ToInt32(dataSource.Value), dataSource.FormatedValue);
            }
            catch { }
            Refresh();
        }

        internal override void DataSource_Missing()
        {
            SetValue(0, "ERR");
        }

        #endregion

    }
}