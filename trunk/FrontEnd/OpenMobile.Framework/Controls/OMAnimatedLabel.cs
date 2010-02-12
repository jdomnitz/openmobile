using System;
using System.ComponentModel;
using System.Drawing;
using System.Timers;

namespace OpenMobile.Controls
{
    /// <summary>
    /// The various effects the control is capable of rendering
    /// </summary>
    public enum eAnimation
    {
        /// <summary>
        /// No Animation
        /// </summary>
        None=0,
        /// <summary>
        /// Scolling left to right and then restarting
        /// </summary>
        Scroll=1,
        /// <summary>
        /// Scrolling left to right and then going right to left
        /// </summary>
        BounceScroll=2,
        /// <summary>
        /// A single character turning outline color and effect font...scrolling left to right
        /// </summary>
        Pulse=3,
        /// <summary>
        /// A single character glowing outline color...scrolling left to right
        /// </summary>
        GlowPulse=4,
        /// <summary>
        /// Text is unveiled starting from the far left and working right
        /// </summary>
        UnveilRight=5,
        /// <summary>
        /// Text is unveiled starting from the far right and working left
        /// </summary>
        UnveilLeft=6
    }
    /// <summary>
    /// A label used for rendering various text effects
    /// </summary>
    public class OMAnimatedLabel:OMLabel
    {
        System.Timers.Timer t;
        private int currentLetter;
        bool singleAnimation;
        private Font effectFont;
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        public OMAnimatedLabel()
        {
            Text = "";
            init();
        }
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMAnimatedLabel(int x, int y, int w, int h)
        {
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
            Text = "";
            init();
        }
        /// <summary>
        /// Create a deep copy of the control
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            OMAnimatedLabel l=(OMAnimatedLabel)this.MemberwiseClone();
            l.init();
            l.TickSpeed = this.TickSpeed;
            return l;
        }

        private void init()
        {
            effectFont = Font;
            t = new System.Timers.Timer();
            t.BeginInit();
            t.EndInit();
            t.Interval = 150;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
        }
        private void endSingleAnimation()
        {
            singleAnimation = false;
            t.Enabled = false;
            animation = eAnimation.None;
        }

        private bool directionReverse;
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.hooked()==false)
                t.Enabled = false;
            if (animation == eAnimation.Scroll)
            {
                scrollPos++;
                if (scrollPos + numChars - 2 > Text.Length){
                    if (singleAnimation == true)
                        endSingleAnimation();
                     scrollPos = 0;
                }
            }
            else if (animation == eAnimation.BounceScroll)
            {
                if (directionReverse==true)
                    scrollPos--;
                 else
                    scrollPos++;
                if (scrollPos + numChars > Text.Length)
                {
                    scrollPos = Text.Length - numChars - 1;
                    directionReverse = true;
                }
                if (scrollPos == -1)
                {
                    if (singleAnimation == true)
                        endSingleAnimation();
                    scrollPos = 0;
                    directionReverse = false;
                }

            }
            else if ((animation == eAnimation.Pulse) || (animation == eAnimation.GlowPulse))
            {
                currentLetter++;
                if (currentLetter >= Text.Length)
                {
                    if (singleAnimation == true)
                        endSingleAnimation();
                    currentLetter = 0;
                }
            }
            else if (animation == eAnimation.UnveilLeft)
            {
                veilLeft-=5;
                if (veilLeft<=0){
                    if (singleAnimation==true)
                        endSingleAnimation();
                    veilLeft=Width;
                }
            }
            else if(animation == eAnimation.UnveilRight)
            {
                veilRight=veilRight- 5;
                if (veilRight<=0){
                    if (singleAnimation==true)
                        endSingleAnimation();
                    veilRight=Width;
                }
            }
            if (animation!=eAnimation.None)
                this.refreshMe(this.toRegion());
        }

        /// <summary>
        /// The speed of the effect (step speed in ms)
        /// </summary>
        [Category("Animation"),Description("The speed of the effect (step speed in ms)")]
        public int TickSpeed
        {
            get
            {
                return (int)t.Interval;
            }
            set
            {
                t.Interval = value;
            }
        }

        private eAnimation animation;
        /// <summary>
        /// The effect to loop continuously
        /// </summary>
        [Category("Animation"),Description("The effect to loop continuously")]
        public eAnimation ContiuousAnimation
        {
            get
            {
                return animation;
            }
            set
            {
                if (animation == value)
                    return;
                animation = value;
                this.refreshMe(this.toRegion());
            }
        }
        private Color outlineColor=Color.Black;
        /// <summary>
        /// Effect Specific Second Color
        /// </summary>
        [DisplayName("Effect Color"),Description("Effect Specific Second Color")]
        [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
        public override Color OutlineColor
        {
            get
            {
                return base.OutlineColor;
            }
            set
            {
                base.OutlineColor = value;
            }
        }

        /// <summary>
        /// Effect specific-second font
        /// </summary>
        [Description("Effect specific-second font"),Category("Text")]
        public Font EffectFont
        {
            get
            {
                return effectFont;
            }
            set
            {
                if (effectFont == value)
                    return;
                effectFont = value;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Renders the specified effect for a single iteration
        /// </summary>
        /// <param name="effect"></param>
        public void animateNow(eAnimation effect)
        {
            singleAnimation = true;
            animation = effect;
            refreshMe(this.toRegion());
        }

        int scrollPos = 0;
        int numChars = 1;
        int veilRight = 0;
        int veilLeft = 0;
        /// <summary>
        /// The type of control
        /// </summary>
        public static new string TypeName
        {
            get
            {
                return "Animated Label";
            }
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void  Render(System.Drawing.Graphics g,renderingParams e)
        {
            float tmp = 1;
            t.Enabled = this.hooked();
            if (Text.Length == 0)
                return;
            if (this.Mode == modeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (this.Mode == modeType.transitioningOut)
                tmp = e.globalTransitionOut;
            switch (animation)
            {
                case eAnimation.Scroll:
                case eAnimation.BounceScroll:
                case eAnimation.None:
                case eAnimation.UnveilLeft:
                case eAnimation.UnveilRight:
                    string text;
                    if ((animation == eAnimation.Scroll) || (animation == eAnimation.BounceScroll))
                    {
                        int width = (g.MeasureString(Text, this.Font).ToSize().Width + 1) / Text.Length;
                        numChars = (this.Width / width);//(Text.Length * width) / this.Width;
                        if (numChars >= this.Text.Length)
                        {
                            animation = eAnimation.None;
                            text = this.Text;
                        }
                        else
                        {
                            text = Text.Substring(scrollPos, numChars);
                        }
                    }
                    else
                    {
                        text = this.Text;
                    }
                    if ((animation==eAnimation.UnveilRight)||(animation==eAnimation.UnveilLeft)){
                        StringFormat format=new StringFormat(StringFormatFlags.NoWrap);
                        g.DrawString(text, Font, new SolidBrush(Color.FromArgb((int)(tmp * 255), Color)), new RectangleF(Left + veilLeft, Top, Width - veilRight + 1 - veilLeft, Height), format);
                    }else
                        g.DrawString(text, Font, new SolidBrush(Color.FromArgb((int)(tmp * 255), Color)), new RectangleF(Left, Top, Width, Height));
                    return;
                case eAnimation.GlowPulse:
                case eAnimation.Pulse:
                    int x2;
                    for (int i = 0; i < Text.Length; i++)
                    {
                        if (currentLetter != i)
                        {
                            x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, i), Font);
                            g.DrawString(Text[i].ToString(), this.Font, new SolidBrush(Color.FromArgb((int)(tmp * 255), this.Color)), new Point(x2, this.Top));
                        }
                    }
                    x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, currentLetter), Font);
                    if (animation == eAnimation.Pulse)
                        g.DrawString(Text[currentLetter].ToString(), effectFont, new SolidBrush(Color.FromArgb((int)(tmp * 255), this.OutlineColor)), new Point(x2, this.Top - (int)(EffectFont.Size - Font.Size) - 2));
                    else
                    {
                        for (int x = -3; x < 3; x++)
                            for (int j = -3; j < 3; j++)
                                g.DrawString(Text[currentLetter].ToString(), effectFont, new SolidBrush(Color.FromArgb((int)(tmp * 255) / (3 * (Math.Abs(x) + Math.Abs(j) + 1)), this.OutlineColor)), new Point(x2 + x, Top + j));
                        g.DrawString(Text[currentLetter].ToString(), effectFont, new SolidBrush(Color.FromArgb((int)(tmp * 255), this.Color)), new Point(x2, this.Top));
                    }
                    return;
            }
        }
        
        private int MeasureDisplayStringWidth(Graphics graphics, string text,
                                            Font font)
        {
            if (text == "")
                return 0;
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0,
                                                                          1000, 1000);
            System.Drawing.CharacterRange[] ranges = 
                                       { new System.Drawing.CharacterRange(0, 
                                                               text.Length) };
            System.Drawing.Region[] regions = new System.Drawing.Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right - (Font.Size / 4.5));
        }
    }
}
