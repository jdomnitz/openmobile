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
    public enum eAnimation : byte
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
        /// <summary>
        /// Only animate once
        /// </summary>
        protected bool singleAnimation;
        /// <summary>
        /// the font to use for the effects
        /// </summary>
        protected Font effectFont;
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        [System.Obsolete("Use OMAnimatedLabel(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel()
        {
            Text = String.Empty;
            init();
        }
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        [System.Obsolete("Use OMAnimatedLabel(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel(int x, int y, int w, int h)
        {
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
            Text = String.Empty;
            init();
        }
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMAnimatedLabel(string name, int x, int y, int w, int h)
        {
            this.Name = name;
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
            Text = String.Empty;
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
            t.Interval = 250;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
        }
        private void endSingleAnimation()
        {
            singleAnimation = false;
            t.Enabled = false;
            currentAnimation = eAnimation.None;
            _RefreshGraphic = true;
        }

        private bool directionReverse;
        private double oldTick;
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((this.hooked() == false) || (_text == null))
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
                            _RefreshGraphic = true;
                        }
                        raiseUpdate(false);
                        return;
                    }
                }
                if (currentAnimation == eAnimation.Scroll)
                {
                    if (avgChar * _text.Length >= width)
                        scrollPos++;
                    if ((scrollPos * avgChar) + width > (Text.Length * avgChar) + 1)
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
                    if ((scrollPos * avgChar) + width > (Text.Length * avgChar) + 1)
                    {
                        scrollPos = _text.Length - (int)(width / avgChar);
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
                if ((_text != null) && (_text.Length > 0) && (currentAnimation != eAnimation.None))
                    raiseUpdate(false);
            }
        }

        /// <summary>
        /// The speed of the effect (step speed in ms)
        /// </summary>
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
                _RefreshGraphic = true;
            }
        }
        /// <summary>
        /// The text displayed in the label
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value == null)
                    value = String.Empty;
                lock (this)
                {
                    if (value == base._text)
                        return;
                    currentAnimation = animation;
                    charTex = new OImage[value.Length];
                    base._text = value;
                    _RefreshGraphic = true;
                    scrollPos = 0;
                    recalc();
                }
            }
        }

        private void recalc()
        {
            if ((_text != null) && (_text.Length > 0))
                avgChar = (Graphics.Graphics.MeasureString(_text, this.Font, _textFormat).Width / Text.Length);
        }

        /// <summary>
        /// Effect specific-second font
        /// </summary>
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
                _RefreshGraphic = true;
            }
        }
        /// <summary>
        /// Renders the specified effect for a single iteration
        /// </summary>
        /// <param name="effect"></param>
        public void animateNow(eAnimation effect)
        {
            singleAnimation = true;
            _RefreshGraphic = true;
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
            oldText = _text;
            _text = newText;
            veilRight = Width;
            tempTransition = effect;
            currentAnimation = animation;
            scrollPos = 0;
            recalc();
            //textTexture = oldTexture = null;
            _RefreshGraphic = true;
            if (oldTick == 0)
                oldTick = t.Interval;
            t.Interval = tick;
            t.Enabled = true;
        }
        string oldText = null;
        OImage oldTexture;
        eAnimation tempTransition;
        int scrollPos = 0;
        double avgChar = 1;
        int veilRight = 0;
        int veilLeft = 0;
        OImage[] charTex;
        OImage currentLetterTex;

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
            if ((_text != null) && (_text.Length != 0))
            {
                t.Enabled = this.hooked();
                float tmp = OpacityFloat;
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
                        if (_RefreshGraphic)
                            textTexture = g.GenerateTextTexture(textTexture, 0, 0, width, height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        g.DrawImage(textTexture, left, top, width, height, tmp);
                        if (tempTransition != eAnimation.None)
                        {
                            g.SetClipFast(left + (width - veilRight), top, veilRight, height);
                            if (_RefreshGraphic)
                                oldTexture = g.GenerateTextTexture(oldTexture, 0, 0, width, height, oldText, _font, _textFormat, _textAlignment, _color, _outlineColor);
                            g.DrawImage(oldTexture, left, top, width, height, tmp);
                        }
                        g.Clip = old;
                        break;
                    case eAnimation.Scroll:
                    case eAnimation.BounceScroll:
                        old = g.Clip;
                        g.SetClipFast(left, top, width, height);
                        if (_RefreshGraphic)
                            textTexture = g.GenerateTextTexture(textTexture, 0, 0, (int)(avgChar * _text.Length) + 1, height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        g.DrawImage(textTexture, left - (int)(scrollPos * avgChar), top, (int)(avgChar * _text.Length) + 1, height, tmp);
                        g.Clip = old;
                        if (avgChar * _text.Length < width)
                        {
                            currentAnimation = eAnimation.None;
                            scrollPos = 0;
                            _RefreshGraphic = true;
                        }
                        break;
                    case eAnimation.GlowPulse:
                    case eAnimation.Pulse:
                        int x2;
                        for (int i = 0; i < Text.Length; i++)
                        {
                            if (currentLetter != i)
                            {
                                x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, i), Font);
                                if (_RefreshGraphic)
                                    charTex[i] = g.GenerateStringTexture(charTex[i], Text[i].ToString(), this.Font, Color.FromArgb((int)(tmp * 255), this.Color), x2, this.Top, 30, 30, System.Drawing.StringFormat.GenericDefault);
                                g.DrawImage(charTex[i], x2, this.Top, 30, 30, tmp);
                            }
                        }
                        x2 = Left + MeasureDisplayStringWidth(g, Text.Substring(0, currentLetter), Font);
                        if (animation == eAnimation.Pulse)
                        {
                            if (_RefreshGraphic)
                                currentLetterTex = g.GenerateStringTexture(currentLetterTex, Text[currentLetter].ToString(), effectFont, Color.FromArgb((int)(tmp * 255), this.OutlineColor), 0, 0, 30, 30, System.Drawing.StringFormat.GenericDefault);
                            g.DrawImage(currentLetterTex, x2, this.Top - (int)(EffectFont.Size - Font.Size) - 2, 30, 30, tmp);
                        }
                        else
                        {
                            if (_RefreshGraphic)
                                currentLetterTex = g.GenerateTextTexture(currentLetterTex, 0, 0, 30, 30, Text[currentLetter].ToString(), effectFont, Format, Alignment.CenterCenter, _color, _outlineColor);
                            g.DrawImage(currentLetterTex, x2, this.Top, 30, 30, tmp);
                        }
                        break;
                }
            }
            _RefreshGraphic = false;
            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Yellow);
        }

        private int MeasureDisplayStringWidth(Graphics.Graphics graphics, string text,
                                            Font font)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
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
    internal sealed class HomemadeTimer : IDisposable
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
            loop.Name = "HomemadeTimer";
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
