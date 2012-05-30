//****************************************************************************************
// Aqua Gauge Control - A Windows User Control.
// Author  : Ambalavanar Thirugnanam
// Date    : 24th August 2007
// email   : ambalavanar.thiru@gmail.com
// This is control is for free. You can use for any commercial or non-commercial purposes.
// [Please do no remove this header when using this control in your application.]
// Modified for use in OpenMobile 1/25/10
//***************************************************************************************
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A gauge control for openMobile
    /// </summary>
    public class OMGauge : OMControl, ISensorDisplay 
    {
        #region Private Attributes
        private float minValue;
        private float maxValue;
        private float threshold;
        private float currentValue;
        private float recommendedValue;
        private int noOfDivisions;
        private int noOfSubDivisions;
        private string dialText;
        private Color dialColor = Color.Lavender;
        private float glossinessAlpha = 25;
        private int oldWidth, oldHeight;
        int x, y;
        float fromAngle = 135F;
        float toAngle = 405F;
        private bool requiresRedraw;
        private OImage backgroundImg;
        private System.Drawing.Rectangle rectImg;
        private int controlHeight;
        private int controlWidth;
        private int controlTop;
        private int controlLeft;
        #endregion
        /// <summary>
        /// A gauge control for openMobile
        /// </summary>
        public OMGauge()
        {
            x = 5;
            y = 5;
            width = this.Width - 10;
            height = this.Height - 10;
            this.noOfDivisions = 10;
            this.noOfSubDivisions = 3;
            this.requiresRedraw = true;
        }
        /// <summary>
        /// A gauge control for openMobile
        /// </summary>
        public OMGauge(int X, int Y, int W, int H)
        {
            x = 5 + X;
            y = 5 + Y;
            width = W - 10;
            height = H - 10;
            this.noOfDivisions = 10;
            this.noOfSubDivisions = 3;
            this.requiresRedraw = true;
        }

        #region Public Properties
        /// <summary>
        /// Mininum value on the scale
        /// </summary>
        [DefaultValue(0)]
        public float MinValue
        {
            get { return minValue; }
            set
            {
                if (value < maxValue)
                {
                    minValue = value;
                    if (currentValue < minValue)
                        currentValue = minValue;
                    if (recommendedValue < minValue)
                        recommendedValue = minValue;
                    requiresRedraw = true;
                }
            }
        }

        /// <summary>
        /// Maximum value on the scale
        /// </summary>
        [DefaultValue(100)]
        public float MaxValue
        {
            get { return maxValue; }
            set
            {
                if (value > minValue)
                {
                    maxValue = value;
                    if (currentValue > maxValue)
                        currentValue = maxValue;
                    if (recommendedValue > maxValue)
                        recommendedValue = maxValue;
                    requiresRedraw = true;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Threshold area from the Recommended Value. (1-99%)
        /// </summary>
        [DefaultValue(25)]
        public float ThresholdPercent
        {
            get { return threshold; }
            set
            {
                if (value > 0 && value < 100)
                {
                    threshold = value;
                    requiresRedraw = true;
                }
            }
        }

        /// <summary>
        /// Threshold value from which green area will be marked.
        /// </summary>
        [DefaultValue(25)]
        public float RecommendedValue
        {
            get { return recommendedValue; }
            set
            {
                if (value > minValue && value < maxValue)
                {
                    recommendedValue = value;
                    requiresRedraw = true;
                }
            }
        }
        private int bufferSize = 0;
        /// <summary>
        /// The size of a moving average buffer to use (0=disabled)
        /// </summary>
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                if ((buffer == null) || (buffer.Length != value))
                    buffer = new float[value];
                bufferSize = value;
            }
        }

        /// <summary>
        /// Value where the pointer will point to.
        /// </summary>
        [DefaultValue(0)]
        public float Value
        {
            get { return currentValue; }
            set
            {
                if (value >= minValue && value <= maxValue)
                {
                    if (BufferSize > 0)
                        currentValue = addToBuffer(value);
                    else
                        currentValue = value;
                }
            }
        }
        private float[] buffer;
        private int bufPos = 0;
        private float addToBuffer(float value)
        {
            if (bufPos >= bufferSize)
                bufPos = 0;
            buffer[bufPos] = value;
            bufPos++;
            if (bufPos >= bufferSize)
                bufPos = 0;
            double ret = 0;
            for (int i = 0; i < bufferSize; i++)
                ret += buffer[i];
            return (int)(ret / bufferSize);
        }

        /// <summary>
        /// Background color of the dial
        /// </summary>
        public Color DialColor
        {
            get { return dialColor; }
            set
            {
                dialColor = value;
                requiresRedraw = true;
            }
        }

        /// <summary>
        /// Glossiness strength. Range: 0-100
        /// </summary>
        public float Glossiness
        {
            get
            {
                return (glossinessAlpha * 100) / 220;
            }
            set
            {
                float val = value;
                if (val > 100)
                    value = 100;
                if (val < 0)
                    value = 0;
                glossinessAlpha = (value * 220) / 100;
            }
        }

        /// <summary>
        /// Get or Sets the number of Divisions in the dial scale.
        /// </summary>
        public int NoOfDivisions
        {
            get { return this.noOfDivisions; }
            set
            {
                if (value > 1 && value < 25)
                {
                    this.noOfDivisions = value;
                    requiresRedraw = true;
                }
            }
        }
        private Color backColor;
        /// <summary>
        /// The back color of the gauge
        /// </summary>
        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                backColor = value;
                requiresRedraw = true;
            }
        }

        private Color foreColor;
        /// <summary>
        /// The color of the gauge text
        /// </summary>
        public Color ForeColor
        {
            get
            {
                return foreColor;
            }
            set
            {
                foreColor = value;
                requiresRedraw = true;
            }
        }
        Font font = new Font(Font.GenericSansSerif, 13F);
        /// <summary>
        /// The gauge font
        /// </summary>
        public Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
                requiresRedraw = true;
            }
        }

        /// <summary>
        /// Gets or Sets the number of Sub Divisions in the scale per Division.
        /// </summary>
        [DefaultValue(3)]
        public int NoOfSubDivisions
        {
            get { return this.noOfSubDivisions; }
            set
            {
                if (value > 0 && value <= 10)
                {
                    this.noOfSubDivisions = value;
                    requiresRedraw = true;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Text to be displayed in the dial
        /// </summary>
        public string DialText
        {
            get { return this.dialText; }
            set
            {
                this.dialText = value;
                requiresRedraw = true;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Draws the Pointer.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        private void DrawPointer(Graphics.Graphics gr, int cx, int cy)
        {
            /*
            float radius = this.Width / 2 - (this.Width * .12F);
            float val = MaxValue - MinValue;

            System.Drawing.Bitmap img = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            val = (100 * (this.currentValue - MinValue)) / val;
            val = ((toAngle - fromAngle) * val) / 100;
            val += fromAngle;

            float angle = GetRadian(val);
            float gradientAngle = angle;

            PointF[] pts = new PointF[5];

            pts[0].X = (float)(cx + radius * Math.Cos(angle));
            pts[0].Y = (float)(cy + radius * Math.Sin(angle));

            pts[4].X = (float)(cx + radius * Math.Cos(angle - 0.02));
            pts[4].Y = (float)(cy + radius * Math.Sin(angle - 0.02));

            angle = GetRadian((val + 20));
            pts[1].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            pts[1].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            pts[2].X = cx;
            pts[2].Y = cy;

            angle = GetRadian((val - 20));
            pts[3].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            pts[3].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            Brush pointer = new Brush(Color.Black);
            g.FillPolygon(pointer, pts);

            PointF[] shinePts = new PointF[3];
            angle = GetRadian(val);
            shinePts[0].X = (float)(cx + radius * Math.Cos(angle));
            shinePts[0].Y = (float)(cy + radius * Math.Sin(angle));

            angle = GetRadian(val + 20);
            shinePts[1].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            shinePts[1].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            shinePts[2].X = cx;
            shinePts[2].Y = cy;

            LinearGradientBrush gpointer = new LinearGradientBrush(shinePts[0], shinePts[2], Color.SlateGray, Color.Black);
            g.FillPolygon(gpointer, shinePts);

            Rectangle rect = new Rectangle(x, y, width, height);
            DrawCenterPoint(g, rect, ((width) / 2) + x, ((height) / 2) + y);

            DrawGloss(g);

            gr.DrawImage(new OImage(img), Left, Top,width,height); //TODO - Verify correct drawing
        
             */
        }

        /// <summary>
        /// Draws the glossiness.
        /// </summary>
        /// <param name="g"></param>
        private void DrawGloss(System.Drawing.Graphics g)
        {
            System.Drawing.RectangleF glossRect = new System.Drawing.RectangleF(
               x + (float)(width * 0.10),
               y + (float)(height * 0.07),
               (float)(width * 0.80),
               (float)(height * 0.7));
            LinearGradientBrush gradientBrush =
                new LinearGradientBrush(glossRect,
                System.Drawing.Color.FromArgb((int)glossinessAlpha, System.Drawing.Color.White),
                System.Drawing.Color.Transparent,
                LinearGradientMode.Vertical);
            g.FillEllipse(gradientBrush, glossRect);

            glossRect = new System.Drawing.RectangleF(
               x + (float)(width * 0.25),
               y + (float)(height * 0.77),
               (float)(width * 0.50),
               (float)(height * 0.2));
            int gloss = (int)(glossinessAlpha / 3);
            gradientBrush =
                new LinearGradientBrush(glossRect,
                System.Drawing.Color.Transparent, System.Drawing.Color.FromArgb(gloss, BackColor.R, BackColor.G, BackColor.B),
                LinearGradientMode.Vertical);
            g.FillEllipse(gradientBrush, glossRect);
        }

        /// <summary>
        /// Draws the center point.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        private void DrawCenterPoint(System.Drawing.Graphics g, Rectangle rect, int cX, int cY)
        {/*
            float shift = Width / 5;
            System.Drawing.RectangleF rectangle = new System.Drawing.RectangleF(cX - (shift / 2), cY - (shift / 2), shift, shift);
            LinearGradientBrush brush = new LinearGradientBrush(rect, System.Drawing.Color.Black, Color.FromArgb(100, this.dialColor), LinearGradientMode.Vertical);
            g.FillEllipse(brush, rectangle);

            shift = Width / 7;
            rectangle = new System.Drawing.RectangleF(cX - (shift / 2), cY - (shift / 2), shift, shift);
            brush = new LinearGradientBrush(rect, Color.SlateGray, System.Drawing.Color.Black, LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(brush, rectangle);*/
        }

        /// <summary>
        /// Draws the Ruler
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        private void DrawCalibration(System.Drawing.Graphics g, System.Drawing.Rectangle rect, int cX, int cY)
        {
            int noOfParts = this.noOfDivisions + 1;
            int noOfIntermediates = this.noOfSubDivisions;
            float currentAngle = GetRadian(fromAngle);
            int gap = (int)(this.Width * 0.01F);
            float shift = this.Width / 25;
            Rectangle rectangle = new Rectangle(rect.Left + gap, rect.Top + gap, rect.Width - gap, rect.Height - gap);

            float x, y, x1, y1, tx, ty, radius;
            radius = rectangle.Width / 2 - gap * 5;
            float totalAngle = toAngle - fromAngle;
            float incr = GetRadian(((totalAngle) / ((noOfParts - 1) * (noOfIntermediates + 1))));

            System.Drawing.Pen thickPen = new System.Drawing.Pen(System.Drawing.Color.Black, Width / 50);
            System.Drawing.Pen thinPen = new System.Drawing.Pen(System.Drawing.Color.Black, Width / 100);
            float rulerValue = MinValue;
            for (int i = 0; i <= noOfParts; i++)
            {
                //Draw Thick Line
                x = (float)(cX + radius * System.Math.Cos(currentAngle));
                y = (float)(cY + radius * System.Math.Sin(currentAngle));
                x1 = (float)(cX + (radius - Width / 20) * System.Math.Cos(currentAngle));
                y1 = (float)(cY + (radius - Width / 20) * System.Math.Sin(currentAngle));
                g.DrawLine(thickPen, x, y, x1, y1);

                //Draw Strings
                System.Drawing.StringFormat format = new System.Drawing.StringFormat();
                tx = (float)(cX + (radius - Width / 10) * System.Math.Cos(currentAngle));
                ty = (float)(cY - shift + (radius - Width / 10) * System.Math.Sin(currentAngle));
                System.Drawing.Brush stringPen = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(ForeColor.A, ForeColor.R, ForeColor.G, ForeColor.B));
                System.Drawing.StringFormat strFormat = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
                strFormat.Alignment = System.Drawing.StringAlignment.Center;
                System.Drawing.Font f = new System.Drawing.Font(font.Name, (float)(this.Width / 23), (System.Drawing.FontStyle)this.Font.Style);
                g.DrawString(rulerValue.ToString(), f, stringPen, new System.Drawing.PointF(tx, ty), strFormat);
                rulerValue += (float)((MaxValue - MinValue) / (noOfParts - 1));
                rulerValue = (float)System.Math.Round(rulerValue, 2);

                //currentAngle += incr;
                if (i == noOfParts - 1)
                    break;
                for (int j = 0; j <= noOfIntermediates; j++)
                {
                    //Draw thin lines 
                    currentAngle += incr;
                    x = (float)(cX + radius * System.Math.Cos(currentAngle));
                    y = (float)(cY + radius * System.Math.Sin(currentAngle));
                    x1 = (float)(cX + (radius - Width / 50) * System.Math.Cos(currentAngle));
                    y1 = (float)(cY + (radius - Width / 50) * System.Math.Sin(currentAngle));
                    g.DrawLine(thinPen, x, y, x1, y1);
                }
            }
        }

        /// <summary>
        /// Converts the given degree to radian.
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public float GetRadian(float theta)
        {
            return theta * (float)System.Math.PI / 180F;
        }

        /// <summary>
        /// Displays the given number in the 7-Segement format.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="number"></param>
        /// <param name="drect"></param>
        private void DisplayNumber(Graphics.Graphics g, float number, Rectangle drect)
        {
            try
            {
                string num = number.ToString("000.00");
                num.PadLeft(3, '0');
                float shift = 0;
                if (number < 0)
                {
                    shift -= width / 17;
                }
                bool drawDPS = false;
                char[] chars = num.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    if (i < chars.Length - 1 && chars[i + 1] == '.')
                        drawDPS = true;
                    else
                        drawDPS = false;
                    if (c != '.')
                    {
                        if (c == '-')
                        {
                            DrawDigit(g, -1, new Point(drect.X + shift, drect.Y), drawDPS, drect.Height);
                        }
                        else
                        {
                            DrawDigit(g, Int16.Parse(c.ToString()), new Point(drect.X + shift, drect.Y), drawDPS, drect.Height);
                        }
                        shift += 15 * this.width / 250;
                    }
                    else
                    {
                        shift += 2 * this.width / 250;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Draws a digit in 7-Segement format.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="number"></param>
        /// <param name="position"></param>
        /// <param name="dp"></param>
        /// <param name="height"></param>
        private void DrawDigit(Graphics.Graphics g, int number, Point position, bool dp, float height)
        {
            float width;
            width = 10F * height / 13;

            Pen outline = new Pen(Color.FromArgb(40, this.dialColor));
            Pen fillPen = new Pen(Color.Black);

            #region Form Polygon Points
            //Segment A
            Point[] segmentA = new Point[5];
            segmentA[0] = segmentA[4] = new Point(position.X + GetX(2.8F, width), position.Y + GetY(1F, height));
            segmentA[1] = new Point(position.X + GetX(10, width), position.Y + GetY(1F, height));
            segmentA[2] = new Point(position.X + GetX(8.8F, width), position.Y + GetY(2F, height));
            segmentA[3] = new Point(position.X + GetX(3.8F, width), position.Y + GetY(2F, height));

            //Segment B
            Point[] segmentB = new Point[5];
            segmentB[0] = segmentB[4] = new Point(position.X + GetX(10, width), position.Y + GetY(1.4F, height));
            segmentB[1] = new Point(position.X + GetX(9.3F, width), position.Y + GetY(6.8F, height));
            segmentB[2] = new Point(position.X + GetX(8.4F, width), position.Y + GetY(6.4F, height));
            segmentB[3] = new Point(position.X + GetX(9F, width), position.Y + GetY(2.2F, height));

            //Segment C
            Point[] segmentC = new Point[5];
            segmentC[0] = segmentC[4] = new Point(position.X + GetX(9.2F, width), position.Y + GetY(7.2F, height));
            segmentC[1] = new Point(position.X + GetX(8.7F, width), position.Y + GetY(12.7F, height));
            segmentC[2] = new Point(position.X + GetX(7.6F, width), position.Y + GetY(11.9F, height));
            segmentC[3] = new Point(position.X + GetX(8.2F, width), position.Y + GetY(7.7F, height));

            //Segment D
            Point[] segmentD = new Point[5];
            segmentD[0] = segmentD[4] = new Point(position.X + GetX(7.4F, width), position.Y + GetY(12.1F, height));
            segmentD[1] = new Point(position.X + GetX(8.4F, width), position.Y + GetY(13F, height));
            segmentD[2] = new Point(position.X + GetX(1.3F, width), position.Y + GetY(13F, height));
            segmentD[3] = new Point(position.X + GetX(2.2F, width), position.Y + GetY(12.1F, height));

            //Segment E
            Point[] segmentE = new Point[5];
            segmentE[0] = segmentE[4] = new Point(position.X + GetX(2.2F, width), position.Y + GetY(11.8F, height));
            segmentE[1] = new Point(position.X + GetX(1F, width), position.Y + GetY(12.7F, height));
            segmentE[2] = new Point(position.X + GetX(1.7F, width), position.Y + GetY(7.2F, height));
            segmentE[3] = new Point(position.X + GetX(2.8F, width), position.Y + GetY(7.7F, height));

            //Segment F
            Point[] segmentF = new Point[5];
            segmentF[0] = segmentF[4] = new Point(position.X + GetX(3F, width), position.Y + GetY(6.4F, height));
            segmentF[1] = new Point(position.X + GetX(1.8F, width), position.Y + GetY(6.8F, height));
            segmentF[2] = new Point(position.X + GetX(2.6F, width), position.Y + GetY(1.3F, height));
            segmentF[3] = new Point(position.X + GetX(3.6F, width), position.Y + GetY(2.2F, height));

            //Segment G
            Point[] segmentG = new Point[7];
            segmentG[0] = segmentG[6] = new Point(position.X + GetX(2F, width), position.Y + GetY(7F, height));
            segmentG[1] = new Point(position.X + GetX(3.1F, width), position.Y + GetY(6.5F, height));
            segmentG[2] = new Point(position.X + GetX(8.3F, width), position.Y + GetY(6.5F, height));
            segmentG[3] = new Point(position.X + GetX(9F, width), position.Y + GetY(7F, height));
            segmentG[4] = new Point(position.X + GetX(8.2F, width), position.Y + GetY(7.5F, height));
            segmentG[5] = new Point(position.X + GetX(2.9F, width), position.Y + GetY(7.5F, height));

            //Segment DP
            #endregion

            #region Draw Segments Outline
            g.FillPolygon(outline.Brush, segmentA);
            g.FillPolygon(outline.Brush, segmentB);
            g.FillPolygon(outline.Brush, segmentC);
            g.FillPolygon(outline.Brush, segmentD);
            g.FillPolygon(outline.Brush, segmentE);
            g.FillPolygon(outline.Brush, segmentF);
            g.FillPolygon(outline.Brush, segmentG);
            #endregion

            #region Fill Segments
            //Fill SegmentA
            if (IsNumberAvailable(number, 0, 2, 3, 5, 6, 7, 8, 9))
            {
                g.FillPolygon(fillPen.Brush, segmentA);
            }

            //Fill SegmentB
            if (IsNumberAvailable(number, 0, 1, 2, 3, 4, 7, 8, 9))
            {
                g.FillPolygon(fillPen.Brush, segmentB);
            }

            //Fill SegmentC
            if (IsNumberAvailable(number, 0, 1, 3, 4, 5, 6, 7, 8, 9))
            {
                g.FillPolygon(fillPen.Brush, segmentC);
            }

            //Fill SegmentD
            if (IsNumberAvailable(number, 0, 2, 3, 5, 6, 8, 9))
            {
                g.FillPolygon(fillPen.Brush, segmentD);
            }

            //Fill SegmentE
            if (IsNumberAvailable(number, 0, 2, 6, 8))
            {
                g.FillPolygon(fillPen.Brush, segmentE);
            }

            //Fill SegmentF
            if (IsNumberAvailable(number, 0, 4, 5, 6, 7, 8, 9))
            {
                g.FillPolygon(fillPen.Brush, segmentF);
            }

            //Fill SegmentG
            if (IsNumberAvailable(number, 2, 3, 4, 5, 6, 8, 9, -1))
            {
                g.FillPolygon(fillPen.Brush, segmentG);
            }
            #endregion

            //Draw decimal point
            if (dp)
            {
                g.FillEllipse(fillPen.Brush, new Rectangle(
                    (int)(position.X + GetX(10F, width)), (int)(
                    position.Y + GetY(12F, height)),
                    (int)(width / 7),
                    (int)(width / 7)));
            }
        }

        /// <summary>
        /// Gets Relative X for the given width to draw digit
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private float GetX(float x, float width)
        {
            return x * width / 12;
        }

        /// <summary>
        /// Gets relative Y for the given height to draw digit
        /// </summary>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private float GetY(float y, float height)
        {
            return y * height / 15;
        }

        /// <summary>
        /// Returns true if a given number is available in the given list.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="listOfNumbers"></param>
        /// <returns></returns>
        private bool IsNumberAvailable(int number, params int[] listOfNumbers)
        {
            if (listOfNumbers.Length > 0)
            {
                foreach (int i in listOfNumbers)
                {
                    if (i == number)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Restricts the size to make sure the height and width are always same.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AquaGauge_Resize(object sender, EventArgs e)
        {
            if (this.Width < 136)
            {
                this.Width = 136;
            }
            if (oldWidth != this.Width)
            {
                this.Height = this.Width;
                oldHeight = this.Width;
            }
            if (oldHeight != this.Height)
            {
                this.Width = this.Height;
                oldWidth = this.Width;
            }
        }
        #endregion

        /// <summary>
        /// The controls height in pixels
        /// </summary>
        public override int Height
        {
            get
            {
                return controlHeight;
            }
            set
            {
                if (controlHeight == value)
                    return;
                requiresRedraw = true;
                controlHeight = value;
            }
        }

        /// <summary>
        /// The controls width in pixels
        /// </summary>
        public override int Width
        {
            get
            {
                return controlWidth;
            }
            set
            {
                if (value == controlWidth)
                    return;
                requiresRedraw = true;
                controlWidth = value;
            }
        }

        /// <summary>
        /// The distance between the top edge of the control and the top edge of the user interface
        /// </summary>
        public override int Top
        {
            get
            {
                return controlTop;
            }
            set
            {
                if (controlTop == value)
                    return;
                requiresRedraw = true;
                controlTop = value;
            }
        }
        /// <summary>
        /// The distance between the left edge of the control and the left edge of the user interface
        /// </summary>
        public override int Left
        {
            get
            {
                return controlLeft;
            }
            set
            {
                if (controlLeft == value)
                    return;
                requiresRedraw = true;
                controlLeft = value;
            }
        }
        /// <summary>
        /// Redraws the gauge control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            //TODO - Gauge doesn't respect transition data (and opacity level)
            if ((this.Width == 0) || (this.Height == 0))
                return;


            //paint the gauge
            g.FillRectangle(new Brush(Color.Transparent), new Rectangle(Left, Top, Width, Height));
            if (backgroundImg == null || requiresRedraw)
            {
                backgroundImg = new OImage(new System.Drawing.Bitmap(this.Width, this.Height));
                System.Drawing.Graphics bg = System.Drawing.Graphics.FromImage(backgroundImg.image);
                bg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                width = this.Width - x * 2;
                height = this.Height - y * 2;
                rectImg = new System.Drawing.Rectangle(0, 0, width, height);

                //Draw background color
                System.Drawing.Brush backGroundBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(120, System.Drawing.Color.FromArgb(dialColor.A, dialColor.R, dialColor.G, dialColor.B)));
                float gg = width / 60;
                bg.FillEllipse(backGroundBrush, 0, 0, width, height);

                //Draw Rim
                System.Drawing.Pen outline = new System.Drawing.Pen(System.Drawing.Color.FromArgb(100, System.Drawing.Color.SlateGray), (float)(width * .03));
                bg.DrawEllipse(outline, rectImg);
                System.Drawing.Pen darkRim = new System.Drawing.Pen(System.Drawing.Color.SlateGray);
                bg.DrawEllipse(darkRim, 0, 0, width, height);

                //Draw Callibration
                DrawCalibration(bg, rectImg, (int)((width) / 2) + x, (int)((height) / 2) + y);

                //Draw Colored Rim
                System.Drawing.Pen colorPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(190, System.Drawing.Color.Gainsboro), this.Width / 40);
                int gap = (int)(this.Width * 0.03F);
                System.Drawing.Rectangle rectg = new System.Drawing.Rectangle(gap, gap, rectImg.Width - gap * 2, rectImg.Height - gap * 2);
                bg.DrawArc(colorPen, rectg, 135, 270);

                //Draw Threshold
                colorPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(200, System.Drawing.Color.LawnGreen), this.Width / 50);
                rectg = new System.Drawing.Rectangle(gap, gap, rectImg.Width - gap * 2, rectImg.Height - gap * 2);
                float val = MaxValue - MinValue;
                val = (100 * (this.recommendedValue - MinValue)) / val;
                val = ((toAngle - fromAngle) * val) / 100;
                val += fromAngle;
                float stAngle = val - ((270 * threshold) / 200);
                if (stAngle <= 135) stAngle = 135;
                float sweepAngle = ((270 * threshold) / 100);
                if (stAngle + sweepAngle > 405) sweepAngle = 405 - stAngle;
                bg.DrawArc(colorPen, rectg, stAngle, sweepAngle);
                System.Drawing.Font fnt = new System.Drawing.Font(font.Name, font.Size, (System.Drawing.FontStyle)font.Style);
                System.Drawing.SizeF textSize = bg.MeasureString(this.dialText, fnt);
                System.Drawing.RectangleF digiFRectText = new System.Drawing.RectangleF(this.Width / 2 - textSize.Width / 2, (int)(this.height / 1.5), textSize.Width, textSize.Height);
                bg.DrawString(dialText, fnt, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(ForeColor.A, ForeColor.R, ForeColor.G, ForeColor.B)), digiFRectText);
                requiresRedraw = false;
            }
            g.DrawImage(backgroundImg, controlLeft, controlTop, controlWidth, controlHeight);

            //Draw Digital Value
            Rectangle digiFRect = new Rectangle(this.Left + this.Width / 2 - this.width / 7, this.Top + (int)(this.height / 1.18), this.width / 4, this.Height / 12);
            g.FillRectangle(new Brush(Color.FromArgb(30, Color.Gray)), (this.Left + (float)this.Width / 2F - (float)this.width / 5F), (this.Top + (float)this.height / 1.2F), ((float)this.width / 2.5F), ((float)this.Height / 9F));
            DisplayNumber(g, this.currentValue, digiFRect);

            //Paint the needle
            width = this.Width - x * 2;
            height = this.Height - y * 2;
            DrawPointer(g, ((width) / 2) + x, ((height) / 2) + y);
        }
        /// <summary>
        /// Returns the area covered by the gauge control
        /// </summary>
        /// <returns></returns>
        public override Rectangle toRegion()
        {
            return new Rectangle(controlLeft, controlTop, controlWidth, controlHeight);
        }

        /// <summary>
        /// sensor name to be watched
        /// </summary>
        protected string sensor;
        /// <summary>
        /// Sets the sensor to subscribe to
        /// </summary>
        public string sensorName
        {
            get
            {
                return sensor;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                this.sensor = value;

                object o;
                BuiltInComponents.Host.getData(eGetData.GetAvailableSensors, "", out o);
                if (o == null)
                    return;
                System.Collections.Generic.List<OpenMobile.Plugin.Sensor> sensors = (System.Collections.Generic.List<OpenMobile.Plugin.Sensor>)o;

                OpenMobile.Plugin.Sensor sensor = sensors.Find(s => s.Name == this.sensor);
                if (sensor != null)
                    sensor.newSensorDataReceived += new OpenMobile.Plugin.SensorDataReceived(sensor_newSensorDataReceived);
                Value = (float)sensor.Value;
            }
        }

        void sensor_newSensorDataReceived(OpenMobile.Plugin.Sensor sender)
        {
            this.Value = (float)sender.Value;
        }
    }
}
