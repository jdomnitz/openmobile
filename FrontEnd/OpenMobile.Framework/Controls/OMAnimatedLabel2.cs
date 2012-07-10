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
using OpenMobile.helperFunctions.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A label used for rendering various text effects
    /// </summary>
    public class OMAnimatedLabel2 : OMLabel
    {
        /// <summary>
        /// Calculated average size of one character
        /// </summary>
        private float _TextCharWidth_Avg = 0;

        /// <summary>
        /// Clipping region to use when drawing 
        /// </summary>
        private Rectangle _Clip_Current = new Rectangle(0,0,1000,600);

        /// <summary>
        /// Position to use when drawing
        /// </summary>
        private Rectangle[] _Pos_Current = new Rectangle[1];

        /// <summary>
        /// Stored clip region, moved outside render method due to code speed 
        /// </summary>
        private Rectangle _Clip_Stored = new Rectangle();

        /// <summary>
        /// The various effects the control is capable of rendering
        /// </summary>
        public enum eAnimation : byte
        {
            /// <summary>
            /// No Animation
            /// </summary>
            None,
            /// <summary>
            /// Scolling smoothly left to right and then restarting
            /// </summary>
            ScrollSmooth_LR,
            /// <summary>
            /// Scrolling smoothly left to right and then going right to left
            /// </summary>
            ScrollSmooth_LRRL,
            /// <summary>
            /// A single character turning outline color and effect font...scrolling left to right
            /// </summary>
            Pulse,
            /// <summary>
            /// A single character glowing outline color...scrolling left to right
            /// </summary>
            GlowPulse,
            /// <summary>
            /// Text is unveiled starting from the far left and working right
            /// </summary>
            UnveilRight,
            /// <summary>
            /// Text is unveiled starting from the far right and working left
            /// </summary>
            UnveilLeft,
            /// <summary>
            /// Scolling char by char left to right and then restarting
            /// </summary>
            ScrollChar_LR,
            /// <summary>
            /// Scrolling char by char left to right and then going right to left
            /// </summary>
            ScrollChar_LRRL
        }
 
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        [System.Obsolete("Use OMAnimatedLabel2(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel2()
        {
        }
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        [System.Obsolete("Use OMAnimatedLabel2(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel2(int x, int y, int w, int h)
            : this()
        {
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
        }
        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMAnimatedLabel2(string name, int x, int y, int w, int h)
            : this()
        {
            this.Name = name;
            this.Left = x;
            this.Top = y;
            this.Width = w;
            this.Height = h;
        }


        /// <summary>
        /// The speed of the effect (step speed in ms)
        /// </summary>
        public int TickSpeed
        {
            get
            {
                return 0;
            }
            set
            {
                //
            }
        }

        private OMAnimatedLabel2.eAnimation _Animation = OMAnimatedLabel2.eAnimation.None;
        /// <summary>
        /// The effect to loop continuously
        /// </summary>
        public OMAnimatedLabel2.eAnimation Animation
        {
            get
            {
                return _Animation;
            }
            set
            {
                if (_Animation == value)
                    return;
                _Animation = value;
                _RefreshGraphic = true;
                Animation_SetParameters();
                Refresh();
            }
        }

        private SizeF _StringSize_Stored = new SizeF();
        private SizeF _StringSize = new SizeF();

        private void Animation_SetParameters()
        {
            _TextCharWidth_Avg = 0;
            if (!String.IsNullOrEmpty(_text))
            {
                // Extract string size
                _StringSize = Graphics.Graphics.MeasureString(_text, this.Font, _textFormat);

                // Calculate average character width
                if (_StringSize.Width > 0)
                    _TextCharWidth_Avg = _StringSize.Width / _text.Length;
            }

            if (_RefreshGraphic)
            {   // Regeneration of texture required, delete old texture
                if (textTexture != null)
                    textTexture.Dispose();
            }

            switch (_Animation)
            {
                case eAnimation.None:
                    _Pos_Current = new Rectangle[1];
                    _Pos_Current[0] = new Rectangle(left, top, width, height);
                    _Pos_Current[0].Width = Width;
                    // Set clipping rectangle
                    _Clip_Current = new Rectangle(left, top, width, height);
                    // Generate text texture
                    textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                    break;
                case eAnimation.ScrollSmooth_LR:
                case eAnimation.ScrollChar_LR:
                    _Pos_Current = new Rectangle[1];
                    _Pos_Current[0] = new Rectangle(left, top, width, height);
                    // Limit width of text rectangle to just fit text to remove any text alignment values
                    _Pos_Current[0].Width = (int)(_TextCharWidth_Avg * (_text.Length + 1));
                    // Set clipping rectangle
                    _Clip_Current = new Rectangle(left, top, width, height);
                    // Generate text texture
                    textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                    break;
                case eAnimation.ScrollSmooth_LRRL:
                case eAnimation.ScrollChar_LRRL:
                    _Pos_Current = new Rectangle[1];
                    _Pos_Current[0] = new Rectangle(left, top, width, height);
                    // Limit width of text rectangle to just fit text to remove any text alignment values
                    _Pos_Current[0].Width = (int)(_TextCharWidth_Avg * (_text.Length + 1));
                    // Set clipping rectangle
                    _Clip_Current = new Rectangle(left, top, width, height);
                    // Generate text texture
                    textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                    break;
                case eAnimation.Pulse:
                    break;
                case eAnimation.GlowPulse:
                    break;
                case eAnimation.UnveilRight:
                    break;
                case eAnimation.UnveilLeft:
                    break;
                default:
                    break;
            }
        }

        private Thread _Animation_Thread = null;
        private bool _Animation_Run = false;
        private void Animation_Start()
        {
            // Cancel if no animation is active
            if (Animation == eAnimation.None)
            {   
                // Cancel thread
                if (_Animation_Thread != null)
                {
                    _Animation_Run = false;
                    //_Animation_Thread.Abort();
                    _Animation_Thread = null;
                }
                return;
            }

            // Start animation thread
            if (_Animation_Thread == null)
            {
                _Animation_Run = true;
                _Animation_Thread = new Thread(Animation_Execute);
                _Animation_Thread.Name = String.Format("OMAnimatedLabel2 ({0})", name);
                _Animation_Thread.IsBackground = true;
                _Animation_Thread.Start();
            }
        }

        private void Animation_Execute()
        {
            while (_Animation_Run)
            {
                // Check if control is visible
                if (!visible || !hooked())
                {
                    _Animation_Run = false;
                    break;
                }

                // Yeild to other threads
                Thread.Sleep(0);

                switch (_Animation)
                {
                    case eAnimation.None:
                        break;
                    case eAnimation.ScrollSmooth_LR:
                        {
                            #region ScrollSmooth_LR

                            SmoothAnimator Animation = new SmoothAnimator(0.052f);
                            int EndPos = left + width + _Pos_Current[0].Width;
                            if (EndPos < left)
                                EndPos = left;

                            // Start value / placement
                            _Pos_Current[0].Left = left;
                            int CurrentPos = _Pos_Current[0].Left;

                            Animation.Animate(delegate(int AnimationStep)
                            {
                                if (!_Animation_Run)
                                    return false;
                                
                                CurrentPos += AnimationStep;
                                if (CurrentPos >= EndPos)
                                {
                                    _Pos_Current[0].Left = EndPos;
                                    Refresh();
                                    return false;
                                }
                                else
                                {
                                    _Pos_Current[0].Left = CurrentPos;
                                    Refresh();
                                    return true;
                                }
                            });

                            // Delay before changing animation
                            Thread.Sleep(500);

                            #endregion
                        }
                        break;
                    case eAnimation.ScrollSmooth_LRRL:
                        {
                            #region ScrollSmooth_LRRL

                            SmoothAnimator Animation = new SmoothAnimator(0.052f);
                            int EndPos = left + width - _Pos_Current[0].Width;
                            if (EndPos < left)
                                EndPos = left;
                            int CurrentPos = _Pos_Current[0].Left;

                            if (CurrentPos < EndPos)
                            {
                                Animation.Animate(delegate(int AnimationStep)
                                {
                                    if (!_Animation_Run)
                                        return false;
                                    
                                    CurrentPos += AnimationStep;
                                    if (CurrentPos >= EndPos)
                                    {
                                        _Pos_Current[0].Left = EndPos;
                                        Refresh();
                                        return false;
                                    }
                                    else
                                    {
                                        _Pos_Current[0].Left = CurrentPos;
                                        Refresh();
                                        return true;
                                    }
                                });

                                // Delay before changing animation
                                Thread.Sleep(500);
                            }
                            else
                            {
                                EndPos = left + width - _Pos_Current[0].Width;
                                if (EndPos > left)
                                    EndPos = left;
                                Animation.Animate(delegate(int AnimationStep)
                                {
                                    if (!_Animation_Run)
                                        return false;

                                    CurrentPos -= AnimationStep;
                                    if (CurrentPos <= EndPos)
                                    {
                                        _Pos_Current[0].Left = EndPos;
                                        Refresh();
                                        return false;
                                    }
                                    else
                                    {
                                        _Pos_Current[0].Left = CurrentPos;
                                        Refresh();
                                        return true;
                                    }
                                });

                                // Delay before changing animation
                                Thread.Sleep(500);
                            }

                            #endregion
                        }
                        break;
                    case eAnimation.Pulse:
                        break;
                    case eAnimation.GlowPulse:
                        break;
                    case eAnimation.UnveilRight:
                        break;
                    case eAnimation.UnveilLeft:
                        break;
                    case eAnimation.ScrollChar_LRRL:
                        {
                            #region ScrollChar_LRRL

                            SmoothAnimator Animation = new SmoothAnimator(0.052f);
                            int EndPos = left + width - _Pos_Current[0].Width;
                            if (EndPos < left)
                                EndPos = left;
                            int CurrentPos = _Pos_Current[0].Left;

                            if (CurrentPos < EndPos)
                            {
                                Animation.Animate(delegate(int AnimationStep)
                                {
                                    if (!_Animation_Run)
                                        return false;

                                    CurrentPos += AnimationStep;
                                    if (CurrentPos >= EndPos)
                                    {
                                        _Pos_Current[0].Left = EndPos;
                                        Refresh();
                                        return false;
                                    }
                                    else
                                    {
                                        _Pos_Current[0].Left = (int)(((int)(CurrentPos / _TextCharWidth_Avg)) * _TextCharWidth_Avg);
                                        Refresh();
                                        return true;
                                    }
                                });

                                // Delay before changing animation
                                Thread.Sleep(500);
                            }
                            else
                            {
                                EndPos = left + width - _Pos_Current[0].Width;
                                if (EndPos > left)
                                    EndPos = left;
                                Animation.Animate(delegate(int AnimationStep)
                                {
                                    if (!_Animation_Run)
                                        return false;

                                    CurrentPos -= AnimationStep;
                                    if (CurrentPos <= EndPos)
                                    {
                                        _Pos_Current[0].Left = EndPos;
                                        Refresh();
                                        return false;
                                    }
                                    else
                                    {
                                        _Pos_Current[0].Left = (int)(((int)(CurrentPos / _TextCharWidth_Avg)) * _TextCharWidth_Avg);
                                        Refresh();
                                        return true;
                                    }
                                });

                                // Delay before changing animation
                                Thread.Sleep(500);
                            }

                            #endregion
                        }
                        break;
                    default:
                        break;
                }

            }

            // Invalididate thread
            _Animation_Thread = null;
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
                if (_text == value)
                    return;
                _text = value;
                _RefreshGraphic = true;
                Animation_SetParameters();

                Refresh();
            }
        }

        /// <summary>
        /// Left placement
        /// </summary>
        public override int Left
        {
            get
            {
                return base.Left;
            }
            set
            {
                _Pos_Current[0].Left = value;
                base.Left = value;
            }
        }
        /// <summary>
        /// Top placement
        /// </summary>
        public override int Top
        {
            get
            {
                return base.Top;
            }
            set
            {
                _Pos_Current[0].Top = value;
                base.Top = value;
            }
        }
        /// <summary>
        /// Width of control
        /// </summary>
        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                _Pos_Current[0].Width = value;
                base.Width = value;
                Animation_SetParameters();
            }
        }
        /// <summary>
        /// Height of control
        /// </summary>
        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                _Pos_Current[0].Height = value;
                base.Height = value;
                Animation_SetParameters();
            }
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            // No use in rendering if text is empty
            if (String.IsNullOrEmpty(_text))
                return;

            // Start animation
            Animation_Start();

            float tmp = OpacityFloat;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            else if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            //if (_RefreshGraphic)
            //    textTexture = g.GenerateTextTexture(textTexture, _Pos_Current.Left, _Pos_Current.Top, _Pos_Current.Width + 5, _Pos_Current.Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);

            // Save current clip and activate new clip region
            _Clip_Stored = g.Clip;
            g.SetClipFast(_Clip_Current.Left, _Clip_Current.Top, _Clip_Current.Width, _Clip_Current.Height);
                       
            // Draw text
            g.DrawImage(textTexture, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width + 5, _Pos_Current[0].Height, tmp);

            // Restore clip region
            g.Clip = _Clip_Stored;

            _RefreshGraphic = false;
            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Yellow);
        }
    }

}
