using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A checkbox control
    /// </summary>
    public class OMCheckbox : OMLabel, IClickable, IHighlightable
    {
        /// <summary>
        /// Occurs when the control is clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Is the checkbox checked
        /// </summary>
        protected bool isChecked;
        /// <summary>
        /// The text color when highlighted
        /// </summary>
        protected Color highlightColor = OpenMobile.helperFunctions.StoredData.SystemSettings.SkinFocusColor;

        /// <summary>
        /// The color the text should turn when the checkbox is highlighted
        /// </summary>
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
        public bool Checked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
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
            Checked = !Checked;
            if (OnClick != null)
                OnClick(this, screen);
            raiseUpdate(false);
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
        }

        private bool genHighlight;
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            float tmp = OpacityFloat;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            using (Brush defaultBrush = new Brush(Color.FromArgb((int)tmp * 255, this.OutlineColor)))
            {
                if ((textTexture == null) || (genHighlight != (Mode == eModeType.Highlighted)))
                {
                    if (Mode == eModeType.Highlighted)
                        textTexture = g.GenerateTextTexture(this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterLeft, highlightColor, this.OutlineColor);
                    else
                        textTexture = g.GenerateTextTexture(this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterLeft, this.Color, this.OutlineColor);
                    genHighlight = (Mode == eModeType.Highlighted);
                }
                g.DrawImage(textTexture, this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, tmp);
                g.DrawRoundRectangle(new Pen(defaultBrush, 3.0F), this.Left, this.Top, this.Height, this.Height, 5);
                if (this.isChecked == true)
                {
                    g.DrawLine(new Pen(defaultBrush, 4.0F), this.Left + 6, this.Top + 6, this.Left + this.Height - 6, this.Top + this.Height - 6);
                    g.DrawLine(new Pen(defaultBrush, 4.0F), this.Left + 6, this.Top + this.Height - 6, this.Left + this.Height - 6, this.Top + 6);
                    g.DrawLine(new Pen(defaultBrush, 2.0F), this.Left + 5, this.Top + 5, this.Left + this.Height - 5, this.Top + this.Height - 5);
                    g.DrawLine(new Pen(defaultBrush, 2.0F), this.Left + 5, this.Top + this.Height - 5, this.Left + this.Height - 5, this.Top + 5);
                }
            }
        }
    }
}
