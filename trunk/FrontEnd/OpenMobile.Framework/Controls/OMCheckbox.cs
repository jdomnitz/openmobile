﻿using System.Drawing;
using System.ComponentModel;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A checkbox control
    /// </summary>
    public class OMCheckbox : OMLabel, IClickable, IHighlightable
    {
        private bool isChecked;
        private Color highlightColor=Color.Blue;

        /// <summary>
        /// The color the text should turn when the checkbox is highlighted
        /// </summary>
        [Category("Checkbox"), Description("Is the checkbox checked")]
        public Color HighlightColor
        {
            get
            {
                return highlightColor;
            }
            set
            {
                highlightColor = value;
            }
        }

        /// <summary>
        /// The value of the checkbox
        /// </summary>
        [Category("Checkbox"),Description("Is the checkbox checked")]
        public bool Checked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    this.refreshMe(this.toRegion());
                }
            }
        }
        /// <summary>
        /// create a new checkbox
        /// </summary>
        public OMCheckbox()
        {
            //
        }
        /// <summary>
        /// Create a new checkbox
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMCheckbox(int x, int y, int w, int h)
        {
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
        }

        /// <summary>
        /// Click the checkbox (Toggle its checked state)
        /// </summary>
        public void clickMe(int screen)
        {
            if (Checked == true)
                Checked = false;
            else
                Checked = true;
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
        }
        /// <summary>
        /// Fires the OnDoubleClick Event
        /// </summary>
        public void doubleClickMe(int screen)
        {
        }

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static new string TypeName
        {
            get
            {
                return "Checkbox";
            }
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics g,renderingParams e)
        {
            float tmp = 1;
            if (this.Mode == modeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (this.Mode == modeType.transitioningOut)
                tmp = e.globalTransitionOut;
            using (Brush defaultBrush = new SolidBrush(Color.FromArgb((int)tmp * 255, this.OutlineColor)))
            {
                if (Mode==modeType.Highlighted)
                    Renderer.renderText(g, this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, Alignment.CenterLeft, tmp, 0, highlightColor, this.OutlineColor);
                else
                    Renderer.renderText(g, this.Left + this.Height+5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, Alignment.CenterLeft, tmp,0, this.Color, this.OutlineColor);
                
                g.DrawRectangle(new Pen(defaultBrush, 4.0F), new Rectangle(this.Left, this.Top, this.Height, this.Height));
                if (this.isChecked == true)
                {
                    g.DrawLine(new Pen(defaultBrush, 4.0F), new Point(this.Left, this.Top), new Point((this.Left + this.Height), this.Top + this.Height));
                    g.DrawLine(new Pen(defaultBrush, 4.0F), new Point(this.Left, this.Top + this.Height), new Point((this.Left + this.Height), this.Top));
                }
            }
        }
    }
}