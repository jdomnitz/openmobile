using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A checkbox control
    /// </summary>
    [System.Serializable]
    public class OMCheckbox : OMLabel, IClickable, IHighlightable
    {
        /// <summary>
        /// Rendering styles
        /// </summary>
        public enum Styles 
        { 
            /// <summary>
            /// Text placed to the right of the checkbox
            /// </summary>
            TextToTheRight, 

            /// <summary>
            /// Text placed to the left of the checkbox
            /// </summary>
            TextToTheLeft 
        }

        /// <summary>
        /// Rendering style
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
        /// Occurs when the control is clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Occurs when the control is held
        /// </summary>
        public event userInteraction OnHoldClick;
        /// <summary>
        /// Occurs when the control is long clicked
        /// </summary>
        public event userInteraction OnLongClick;
        /// <summary>
        /// Is the checkbox checked
        /// </summary>
        protected bool isChecked;
        /// <summary>
        /// The text color when highlighted
        /// </summary>
        protected Color highlightColor = BuiltInComponents.SystemSettings.SkinFocusColor;
        /// <summary>
        /// Fillcolor of the checkbox when checked
        /// </summary>
        protected Color _CheckedColor = Color.FromArgb(120, BuiltInComponents.SystemSettings.SkinFocusColor);

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
                _RefreshGraphic = true;
            }
        }

        /// <summary>
        /// Fillcolor of the checkbox when checked
        /// </summary>
        public Color CheckedColor
        {
            get
            {
                return _CheckedColor;
            }
            set
            {
                _CheckedColor = value;
                Refresh();
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
                _RefreshGraphic = true;
            }
        }

        private float _GraphicLineThickness = 1.0f;
        /// <summary>
        /// The thickness of the lines that's used for drawing the graphics
        /// </summary>
        public float GraphicLineThickness
        {
            get
            {
                return _GraphicLineThickness;
            }
            set
            {
                _GraphicLineThickness = value;
                Refresh();
            }
        }

        /// <summary>
        /// create a new checkbox
        /// </summary>
        [System.Obsolete("Use OMCheckbox(string name, int x, int y, int w, int h) instead")]
        public OMCheckbox()
            : base("",0,0,200,200)
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
        [System.Obsolete("Use OMCheckbox(string name, int x, int y, int w, int h) instead")]
        public OMCheckbox(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
        }
        /// <summary>
        /// Create a new checkbox
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMCheckbox(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
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
        /// Hold the checkbox 
        /// </summary>
        public void holdClickMe(int screen)
        {
            if (OnHoldClick != null)
                OnHoldClick(this, screen);
            raiseUpdate(false);
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
            if (OnLongClick != null)
                OnLongClick(this, screen);
        }

        private bool genHighlight;
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            using (Brush defaultBrush = new Brush(Color.FromArgb((int)GetAlphaValue255(this.OutlineColor.A), this.OutlineColor)))
            {
                #region Draw label

                if (!string.IsNullOrEmpty(_text))
                {
                    switch (_Style)
                    {
                        case Styles.TextToTheRight:
                            {
                                if ((_RefreshGraphic) || (genHighlight != (Mode == eModeType.Highlighted)))
                                {
                                    if (Mode == eModeType.Highlighted)
                                        textTexture = g.GenerateTextTexture(this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterLeft, highlightColor, this.OutlineColor);
                                    else
                                        textTexture = g.GenerateTextTexture(this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterLeft, this.Color, this.OutlineColor);
                                    genHighlight = (Mode == eModeType.Highlighted);
                                }
                                g.DrawImage(textTexture, this.Left + this.Height + 5, this.Top, this.Width - this.Height, this.Height, _RenderingValue_Alpha);
                            }
                            break;
                        case Styles.TextToTheLeft:
                            {
                                if ((_RefreshGraphic) || (genHighlight != (Mode == eModeType.Highlighted)))
                                {
                                    if (Mode == eModeType.Highlighted)
                                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterRight, highlightColor, this.OutlineColor);
                                    else
                                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width - this.Height, this.Height, this.Text, this.Font, this.Format, OpenMobile.Graphics.Alignment.CenterRight, this.Color, this.OutlineColor);
                                    genHighlight = (Mode == eModeType.Highlighted);
                                }
                                g.DrawImage(textTexture, this.Left, this.Top, this.Width - this.Height, this.Height, _RenderingValue_Alpha);
                            }
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                // Calculate rounded colors
                int CornerRadius = this.Region.Height / 6;

                using (Brush fillBrush = new Brush(Color.FromArgb((int)GetAlphaValue255(this._CheckedColor.A), this._CheckedColor)))
                {
                    // Offset fill rectangle
                    float Offset = (_GraphicLineThickness * 2f);

                    switch (_Style)
                    {
                        case Styles.TextToTheRight:
                            {
                                // Set frame for checkbox
                                Rectangle rectChkBox = new Rectangle(this.Left, this.Top, this.Height, this.Height);

                                if (this.isChecked == true)
                                {
                                    Rectangle rectChkBoxFill = new Rectangle(rectChkBox.Left + Offset, rectChkBox.Top + Offset, rectChkBox.Width - (Offset * 2), rectChkBox.Height - (Offset * 2));
                                    g.FillRoundRectangle(fillBrush, rectChkBoxFill, CornerRadius);
                                    g.DrawLine(new Pen(defaultBrush, 4.0F), this.Left + 6, this.Top + 6, this.Left + this.Height - 6, this.Top + this.Height - 6);
                                    g.DrawLine(new Pen(defaultBrush, 4.0F), this.Left + 6, this.Top + this.Height - 6, this.Left + this.Height - 6, this.Top + 6);
                                    g.DrawLine(new Pen(defaultBrush, 2.0F), this.Left + 5, this.Top + 5, this.Left + this.Height - 5, this.Top + this.Height - 5);
                                    g.DrawLine(new Pen(defaultBrush, 2.0F), this.Left + 5, this.Top + this.Height - 5, this.Left + this.Height - 5, this.Top + 5);
                                }

                                // Draw frame around checkbox
                                using (Pen p = new Pen(defaultBrush, _GraphicLineThickness))
                                    g.DrawRoundRectangle(p, rectChkBox, CornerRadius);
                            }
                            break;
                        case Styles.TextToTheLeft:
                            {
                                // Set frame for checkbox
                                Rectangle rectChkBox = new Rectangle(this.Region.Right - this.Height, this.Top, this.Height, this.Height);

                                if (this.isChecked == true)
                                {
                                    Rectangle rectChkBoxFill = new Rectangle(rectChkBox.Left + Offset, rectChkBox.Top + Offset, rectChkBox.Width - (Offset * 2), rectChkBox.Height - (Offset * 2));
                                    g.FillRoundRectangle(fillBrush, rectChkBoxFill, CornerRadius);
                                    g.DrawLine(new Pen(defaultBrush, 4.0F), rectChkBox.Left + 6, rectChkBox.Top + 6, rectChkBox.Left + rectChkBox.Height - 6, rectChkBox.Top + rectChkBox.Height - 6);
                                    g.DrawLine(new Pen(defaultBrush, 4.0F), rectChkBox.Left + 6, rectChkBox.Top + rectChkBox.Height - 6, rectChkBox.Left + rectChkBox.Height - 6, rectChkBox.Top + 6);
                                    g.DrawLine(new Pen(defaultBrush, 2.0F), rectChkBox.Left + 5, rectChkBox.Top + 5, rectChkBox.Left + rectChkBox.Height - 5, rectChkBox.Top + rectChkBox.Height - 5);
                                    g.DrawLine(new Pen(defaultBrush, 2.0F), rectChkBox.Left + 5, rectChkBox.Top + rectChkBox.Height - 5, rectChkBox.Left + rectChkBox.Height - 5, rectChkBox.Top + 5);
                                }

                                // Draw frame around checkbox
                                using (Pen p = new Pen(defaultBrush, _GraphicLineThickness))
                                    g.DrawRoundRectangle(p, rectChkBox, CornerRadius);
                            }
                            break;
                        default:
                            break;
                    }

                }               
            }

            base.RenderFinish(g, e);
        }
    }
}
