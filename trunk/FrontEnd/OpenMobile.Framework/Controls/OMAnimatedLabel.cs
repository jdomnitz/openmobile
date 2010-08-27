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
using System.Threading;
using System.Timers;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// The various effects the control is capable of rendering
    /// </summary>
    public enum eAnimation:byte
    {
        /// <summary>
        /// No Animation
        /// </summary>
        None = 0,
        /// <summary>
        /// Scolling left to right and then restarting
        /// </summary>
        Scroll = 1,
        /// <summary>
        /// Scrolling left to right and then going right to left
        /// </summary>
        BounceScroll = 2,
        /// <summary>
        /// A single character turning outline color and effect font...scrolling left to right
        /// </summary>
        Pulse = 3,
        /// <summary>
        /// A single character glowing outline color...scrolling left to right
        /// </summary>
        GlowPulse = 4,
        /// <summary>
        /// Text is unveiled starting from the far left and working right
        /// </summary>
        UnveilRight = 5,
        /// <summary>
        /// Text is unveiled starting from the far right and working left
        /// </summary>
        UnveilLeft = 6
    }
    /// <summary>
    /// A label used for rendering various text effects
    /// </summary>
    public class OMAnimatedLabel : OMLabel
    {
        HomemadeTimer t;
        private int currentLetter;
        protected bool singleAnimation;
        protected Font effectFont;
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
            OMAnimatedLabel l = (OMAnimatedLabel)this.MemberwiseClone();
            l.init();
            l.TickSpeed = this.TickSpeed;
            return l;
        }

        private void init()
        {
            effectFont = Font;
            t = new HomemadeTimer();
            t.Interval = 150;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
        }
        private void endSingleAnimation()
        {
            singleAnimation = false;
            t.Enabled = false;
            currentAnimation = eAnimation.None;
        }

        private bool directionReverse;
        private double oldTick;
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((this.hooked() == false) || (text == null))
            {
                t.Enabled = false;
                return;
            }
            lock (this)
            {
                if (tempTransition != eAnimation.None)
                {
                    if (tempTransition == eAnimation.UnveilRight)
                    {
                        veilRight -= 20;
                        if (veilRight < 0)
                        {
                            veilRight = 0;
                            t.Interval = oldTick;
                            oldTick = 0;
                            tempTransition = eAnimation.None;
                        }
                        raiseUpdate(Rectangle.Empty);
                        return;
                    }
                }
                if (currentAnimation == eAnimation.Scroll)
                {
                    if (avgChar * text.Length >= width)
                        scrollPos++;
                    if ((scrollPos * avgChar) + width > (Text.Length * (avgChar + 0.5)))
                    {
                        Thread.Sleep(500);
                        if (singleAnimation == true)
                            endSingleAnimation();
                        scrollPos = 0;
                        Thread.Sleep(500);
                    }
                }
                else if (currentAnimation == eAnimation.BounceScroll)
                {
                    if (directionReverse == true)
                        scrollPos--;
                    else
                        scrollPos++;
                    if ((scrollPos * avgChar) + width > (Text.Length * (avgChar + 0.5)))
                    {
                        scrollPos = text.Length - (int)(width / avgChar);
                        directionReverse = true;
                        Thread.Sleep(500);
                    }
                    if (scrollPos < 0)
                    {
                        if (singleAnimation == true)
                            endSingleAnimation();
                        scrollPos = 0;
                        directionReverse = false;
                        Thread.Sleep(500);
                    }

                }
                else if ((currentAnimation == eAnimation.Pulse) || (currentAnimation == eAnimation.GlowPulse))
                {
                    currentLetter++;
                    if (currentLetter >= Text.Length)
                    {
                        if (singleAnimation == true)
                            endSingleAnimation();
                        currentLetter = 0;
                    }
                    currentLetterTex = null;
                }
                else if (currentAnimation == eAnimation.UnveilLeft)
                {
                    veilLeft -= 10;
                    if (veilLeft < 0)
                    {
                        if (singleAnimation == true)
                            endSingleAnimation();
                        veilLeft = Width;
                    }
                }
                else if (currentAnimation == eAnimation.UnveilRight)
                {
                    veilRight -= 10;
                    if (veilRight < 0)
                    {
                        if (singleAnimation == true)
                            endSingleAnimation();
                        veilRight = Width;
                    }
                }
                if ((text!=null)&&(text.Length>0)&&(currentAnimation!=eAnimation.None))
                    raiseUpdate(Rectangle.Empty);
            }
        }

        /// <summary>
        /// The speed of the effect (step speed in ms)
        /// </summary>
        [Category("Animation"), Description("The speed of the effect (step speed in ms)")]
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

        private eAnimation currentAnimation;
        private eAnimation animation;
        /// <summary>
        /// The effect to loop continuously
        /// </summary>
        [Category("Animation"), Description("The effect to loop continuously")]
        public eAnimation ContiuousAnimation
        {
            get
            {
                return animation;
            }
            set
            {
                animation = value;
                if (currentAnimation == value)
                    return;
                currentAnimation = value;
                textTexture = null;
            }
        }
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value == null)
                    value = "";
                lock (this)
                {
                    if (value == base.text)
                        return;
                    currentAnimation = animation;
                    charTex = new OImage[value.Length];
                    base.text = value;
                    textTexture = null;
                    scrollPos = 0;
                    recalc();
                }
            }
        }

        private void recalc()
        {
            if ((text!=null)&&(text.Length>0))
                avgChar = (Graphics.Graphics.MeasureString(text, this.Font).Width / Text.Length);
        }

        /// <summary>
        /// Effect specific-second font
        /// </summary>
        [Description("Effect specific-second font"), Category("Text")]
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
                textTexture = null;
            }
        }
        /// <summary>
        /// Renders the specified effect for a single iteration
        /// </summary>
        /// <param name="effect"></param>
        public void animateNow(eAnimation effect)
        {
            singleAnimation = true;
            textTexture = null;
            currentAnimation = effect;
        }
        /// <summary>
        /// Renders the specified effect for a single iteration
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="newText"></param>
        /// <param name="tick"></param>
        public void Transition(eAnimation effect, string newText, int tick)
        {
            oldText = text;
            text = newText;
            veilRight = Width;
            tempTransition = effect;
            currentAnimation = animation;
            textTexture = oldTexture = null;
            if (oldTick == 0)
                oldTick = t.Interval;
            t.Interval = tick;
            t.Enabled = true;
        }
        string oldText = null;
        OImage oldTexture;
        eAnimation tempTransition;
        int scrollPos = 0;
        float avgChar = 1;
        int veilRight = 0;
        int veilLeft = 0;
        OImage[] charTex;
        OImage currentLetterTex;
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
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            if (tempTransition == eAnimation.None)
                draw(g, e, this.currentAnimation);
            else
                draw(g, e, this.tempTransition);
        }
        private void draw(Graphics.Graphics g, renderingParams e, eAnimation animation)
        {
            t.Enabled = this.hooked();
            if ((text == null) || (text.Length == 0))
                return;
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            Rectangle old;
            switch (animation)
            {
                case eAnimation.None:
                case eAnimation.UnveilLeft:
                case eAnimation.UnveilRight:
                    old = g.Clip;
                    g.SetClipFast(left + veilLeft, top, width - veilRight, height);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(0, 0, width, height, text, font, textFormat, textAlignment, color, outlineColor);
                    g.DrawImage(textTexture, left, top, width, height, tmp);
                    if (tempTransition != eAnimation.None)
                    {
                        g.SetClipFast(left + (width - veilRight), top, veilRight, height);
                        if (oldTexture == null)
                            oldTexture = g.GenerateTextTexture(0, 0, width, height, oldText, font, textFormat, textAlignment, color, outlineColor);
                        g.DrawImage(oldTexture, left, top, width, height, tmp);
                    }
                    g.Clip =old;
                    return;
               case eAnimation.Scroll:
               case eAnimation.BounceScroll:
                    old = g.Clip;
                    g.SetClipFast(left, top, width, height);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(0, 0, (int)((avgChar+0.5) * text.Length), height, text, font, textFormat, textAlignment, color, outlineColor);
                    g.DrawImage(textTexture, left - (int)(scrollPos * avgChar), top, (int)((avgChar+0.5)*text.Length), height, tmp);
                    g.Clip=old;
                    if (avgChar * text.Length < width)
                    {
                        currentAnimation = eAnimation.None;
                        scrollPos = 0;
                        textTexture = null;
                    }
                    return;
                case eAnimation.GlowPulse:
                case eAnimation.Pulse:
                    int x2;
                    for (int i = 0; i < Text.Length; i++)
                    {
                        if (currentLetter != i)
                        {
                            x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, i), Font);
                            if (charTex[i] == null)
                                charTex[i] = g.GenerateStringTexture(Text[i].ToString(), this.Font, Color.FromArgb((int)(tmp * 255), this.Color), x2, this.Top, 30, 30, System.Drawing.StringFormat.GenericDefault);
                            g.DrawImage(charTex[i], x2, this.Top, 30, 30, tmp);
                        }
                    }
                    x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, currentLetter), Font);
                    if (animation == eAnimation.Pulse)
                    {
                        if (currentLetterTex == null)
                            currentLetterTex = g.GenerateStringTexture(Text[currentLetter].ToString(), effectFont, Color.FromArgb((int)(tmp * 255), this.OutlineColor), 0, 0, 30, 30, System.Drawing.StringFormat.GenericDefault);
                        g.DrawImage(currentLetterTex, x2, this.Top - (int)(EffectFont.Size - Font.Size) - 2, 30, 30, tmp);
                    }
                    else
                    {
                        if (currentLetterTex == null)
                            currentLetterTex = g.GenerateTextTexture(0, 0, 30, 30, Text[currentLetter].ToString(), effectFont, Format, Alignment.CenterCenter, color, outlineColor);
                        g.DrawImage(currentLetterTex, x2, this.Top, 30, 30, tmp);
                    }
                    return;
            }
        }

        private int MeasureDisplayStringWidth(Graphics.Graphics graphics, string text,
                                            Font font)
        {
            if (text == "")
                return 0;
            int newWidth = (int)Graphics.Graphics.MeasureString(text, font).Width;
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            format.FormatFlags = System.Drawing.StringFormatFlags.MeasureTrailingSpaces;
            Rectangle rect = new Rectangle(0, 0, 1000, 1000);
            System.Drawing.CharacterRange[] ranges = 
                                       { new System.Drawing.CharacterRange(0, 
                                                               text.Length) };
            format.SetMeasurableCharacterRanges(ranges);

            rect = Graphics.Graphics.MeasureCharacterRanges(text, font, rect, format);
            return (int)(rect.Right - (Font.Size / 4.5));
        }
    }
    public sealed class HomemadeTimer : IDisposable
    {
        public event ElapsedEventHandler Elapsed;
        public double Interval;
        private bool disposed;
        private bool enabled;
        private EventWaitHandle handle;
        private Thread loop;
        public HomemadeTimer()
        {
            handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            loop = new Thread(delegate() { DoWork(); });
            loop.Start();
        }
        public bool Enabled
        {
            set
            {
                if (value != enabled)
                {
                    if (value == false)
                        handle.Reset();
                    else
                        handle.Set();
                }
                enabled = value;
            }
            get
            {
                return enabled;
            }
        }
        private void DoWork()
        {
            while (!disposed)
            {
                handle.WaitOne();
                if (Elapsed != null)
                    Elapsed.Invoke(this, null);
                Thread.Sleep((int)Interval);
            }
        }
        public void Dispose()
        {
            disposed = true;
        }
    }

}
