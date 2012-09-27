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
using System.Collections.Generic;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A label used for rendering various text effects
    /// </summary>
    [System.Serializable]
    public class OMAnimatedLabel2 : OMLabel, ICloneable
    {
        private class RenderData : IDisposable, ICloneable
        {
            private OMAnimatedLabel2 home = null;

            private OImage _Texture = null;
            public OImage Texture
            {
                get { return _Texture; }
            }

            private float _CharWidth_Avg = 0f;
            public float CharWidth_Avg
            {
                get { return _CharWidth_Avg; }
            }

            public Rectangle Clip = new Rectangle(0, 0, 1000, 600);

            public void LimitClip(Rectangle region)
            {
                if (Clip.Bottom > region.Bottom)
                    Clip.Bottom = region.Bottom;
                if (Clip.Right > region.Right)
                    Clip.Right = region.Right;
                if (Clip.Top < region.Top)
                    Clip.Top = region.Top;
                if (Clip.Left < region.Left)
                    Clip.Left = region.Left;
            }

            public Rectangle Position = new Rectangle();

            /// <summary>
            /// Scale will scale the object around it's center
            /// </summary>
            public PointF Scale = new PointF(1f, 1f);

            public bool Visible = true;

            public float Alpha = 1.0f;

            private SizeF _TextSize = new SizeF();
            public SizeF TextSize
            {
                get { return _TextSize; }
            }

            private string _Text = "";
            public string Text
            {
                get
                {
                    return _Text;
                }
            }

            private Rectangle _LastPosition;
            private bool SetSizeToMatchControl()
            {
                // Default text placement
                Position = home.Region;

                if (Position != _LastPosition)
                {
                    _LastPosition = Position;
                    return true;
                }
                return false;
            }

            private void RefreshTextDimensions()
            {
                if (!String.IsNullOrEmpty(_Text))
                {
                    // Extract string size
                    _TextSize = home.GetStringSize(_Text);

                    // Calculate average character width
                    if (TextSize.Width > 0)
                        _CharWidth_Avg = TextSize.Width / _Text.Length;
                }
            }
            
            /// <summary>
            /// Refresh texture data
            /// </summary>
            public void RefreshTexture(eTextFormat textFormat)
            {
                // Regeneration of texture required, delete old textures
                if (Texture != null)
                    Texture.Dispose();

                // Generate new texture
                _Texture = Graphics.Graphics.GenerateTextTexture(Texture, 0, Position.Left, Position.Top, Position.Width, Position.Height, _Text, home._font, textFormat, home._textAlignment, home._color, home._outlineColor);
            }

            public RenderData(OMAnimatedLabel2 home)
            {
                this.home = home;
            }

            public void SetText(string text, bool SetWidthToTextSize)
            {
                SetText(text, SetWidthToTextSize, home._textFormat);
            }
            public void SetText(string text, bool SetWidthToTextSize, eTextFormat textFormat)
            {
                bool TextureRefreshRequired = _Text != text;

                _Text = text;
                if (SetSizeToMatchControl())
                    TextureRefreshRequired = true;

                RefreshTextDimensions();

                if (SetWidthToTextSize)
                {   // Limit width of text rectangle to just fit text to remove any text alignment values
                    int TextWidth = (int)(CharWidth_Avg * (_Text.Length + 1));
                    if (TextWidth != Position.Width)
                        TextureRefreshRequired = true;
                    Position.Width = TextWidth;
                }

                if (TextureRefreshRequired)
                    RefreshTexture(textFormat);
            }

            public void ResetClip()
            {
                Clip = new Rectangle(home.left, home.top, home.width, home.height);
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (_Texture != null)
                    _Texture.Dispose();
                _Texture = null;
                _Text = String.Empty;
            }

            #endregion

            #region ICloneable Members

            public object Clone()
            {
                RenderData returnData = (RenderData)this.MemberwiseClone();
                if (this._Texture != null)
                    returnData._Texture = (OImage)this._Texture.Clone();
                return returnData;
            }

            #endregion
        }

        private List<RenderData> _RenderObjects = new List<RenderData>();

        private RenderData _MainObject
        {
            get
            {
                if (_RenderObjects.Count >= 1)
                    return _RenderObjects[0];
                return null;
            }
            set
            {
                _RenderObjects[0] = value;
            }
        }
        private RenderData _SecondObject
        {
            get
            {
                // Add object if missing
                if (_RenderObjects.Count == 1)
                    _RenderObjects.Add(new RenderData(this));

                // Return object
                if (_RenderObjects.Count >= 2)
                    return _RenderObjects[1];

                return null;
            }
            set
            {
                if (_RenderObjects.Count >= 2)
                    _RenderObjects[1] = value;
                else if (_RenderObjects.Count == 1)
                    _RenderObjects.Add(value);
            }
        }

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
            /// A slow glowing effect
            /// </summary>
            Glow,
            /// <summary>
            /// Text is unveiled starting from the far left and working right in a smooth manner
            /// </summary>
            UnveilRightSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text up
            /// </summary>
            SlideUpSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text down
            /// </summary>
            SlideDownSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text in from the left towards right
            /// </summary>
            SlideRightSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text in from the right towards left
            /// </summary>
            SlideLeftSmooth,
            /// <summary>
            /// Text is unveiled starting from the far left and working right revealing one character at a time
            /// </summary>
            UnveilRightChar,
            /// <summary>
            /// Scolling char by char left to right and then restarting
            /// </summary>
            ScrollChar_LR,
            /// <summary>
            /// Scrolling char by char left to right and then going right to left
            /// </summary>
            ScrollChar_LRRL,
            /// <summary>
            /// Crossfade texts to reveal the new one
            /// </summary>
            CrossFade,
            /// <summary>
            /// Text will grow and fade out
            /// </summary>
            GrowAndFade,
            /// <summary>
            /// Text will flash
            /// </summary>
            Flash
        }

        /// <summary>
        /// Activation type for animation
        /// </summary>
        public enum AnimationActivationTypes
        {
            /// <summary>
            /// Always run animation
            /// </summary>
            Always,

            /// <summary>
            /// Animation is only activated if text doesn't fit in the control
            /// </summary>
            TextToLong
        }

        #region Constructors 

        private void Init()
        {
            //SoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
            SoftEdgeData.Sides = FadingEdge.GraphicSides.None;

            // Create main object (the one that holds the correct text)
            _RenderObjects.Add(new RenderData(this));
        }

        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        [System.Obsolete("Use OMAnimatedLabel2(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel2()
            : base("", 0, 0, 200,200)
        {
            Init();
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
            : base("", x, y, w, h)
        {
            Init();
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
            : base(name, x, y, w, h)
        {
            Init();
        }

        #endregion

        private void Animation_Execute(OMAnimatedLabel2.eAnimation animationType, string text, string text2, bool ContinousEffect, bool DelayedRun, float AnimationSpeed)
        {
            if (_Animation_Cancel)
                goto end;

            bool CancelAnimation = false;

            lock (_RenderObjects)
            {
                _Animation_Running = true;

                // Disable object if no text is specified
                if (String.IsNullOrEmpty(text) && String.IsNullOrEmpty(text2))
                    animationType = eAnimation.None;

                switch (animationType)
                {
                    case eAnimation.None:
                        {
                            #region None

                            if (_Animation_Cancel)
                                goto end;

                            // Reset properties to default (no animation)
                            ClearRenderObjects();

                            // Request a redraw
                            Refresh();

                            CancelAnimation = true;

                            #endregion
                        }
                        break;
                    case eAnimation.ScrollSmooth_LR:
                    case eAnimation.ScrollChar_LR:
                        {
                            #region ScrollSmooth_LR / ScrollChar_LR

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Visible = true;
                            _SecondObject.SetText(text, true);
                            _SecondObject.ResetClip();
                            _SecondObject.Position.Left = Region.Right; // Position second object just outside the controls region
                            _SecondObject.Visible = false;
                            int ObjectSeparation = (int)(_MainObject.CharWidth_Avg * 10);

                            // Render initial state
                            Refresh();

                            // Check if activation properties is valid
                            if (_ActivationType == AnimationActivationTypes.TextToLong)
                            {
                                if (_MainObject.TextSize.Width < this.Region.Width)
                                {
                                    Refresh();
                                    CancelAnimation = true;
                                    break;
                                }
                            }

                            SmoothAnimator Animation = new SmoothAnimator(0.05f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animationType == eAnimation.ScrollChar_LR)
                                StepSize = _MainObject.CharWidth_Avg;

                            // Delay before starting animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region Scroll Left

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    if (_MainObject.Visible)
                                        _MainObject.Position.Left -= AnimationStep;

                                    if (_MainObject.Position.Right < Region.Left)
                                    {
                                        _MainObject.Visible = false;
                                        _MainObject.Position.Left = Region.Right;
                                    }

                                    if (_MainObject.Position.Left < Region.Left && _MainObject.Position.Right < (Region.Right - ObjectSeparation))
                                        _SecondObject.Visible = true;

                                    if (_SecondObject.Visible)
                                        _SecondObject.Position.Left -= AnimationStep;

                                    if (_SecondObject.Position.Right < Region.Left)
                                    {
                                        _SecondObject.Visible = false;
                                        _SecondObject.Position.Left = Region.Right;
                                    }

                                    if (_SecondObject.Position.Left < Region.Left && _SecondObject.Position.Right < (Region.Right - ObjectSeparation))
                                        _MainObject.Visible = true;

                                    // Exit animation
                                    if (_SecondObject.Position.Left <= Region.Left)
                                    {
                                        _SecondObject.Position.Left = Region.Left;
                                        return false;
                                    }

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            #endregion
                        }
                        break;

                    case eAnimation.ScrollChar_LRRL:
                    case eAnimation.ScrollSmooth_LRRL:
                        {
                            #region ScrollSmooth_LRRL / ScrollChar_LRRL

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(1);

                            // Set parameters
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();

                            // Render initial state
                            Refresh();

                            // Check if activation properties is valid
                            if (_ActivationType == AnimationActivationTypes.TextToLong)
                            {
                                if (_MainObject.TextSize.Width < this.Region.Width)
                                {
                                    Refresh();
                                    CancelAnimation = true;
                                    break;
                                }
                            }

                            SmoothAnimator Animation = new SmoothAnimator(0.05f * AnimationSpeed);
                            float AnimationValue = 0;

                            int EndPos = left + width - _MainObject.Position.Width;
                            if (EndPos < left)
                                EndPos = left;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animationType == eAnimation.ScrollChar_LRRL)
                                StepSize = _MainObject.CharWidth_Avg;

                            // Delay before starting animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            if (_Animation_Cancel)
                                goto end;

                            #region Scroll Right

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    _MainObject.Position.Left += AnimationStep;
                                    Refresh();

                                    // End animation?
                                    if (_MainObject.Position.Left >= EndPos)
                                    {   // Yes, set final value and exit
                                        _MainObject.Position.Left = EndPos;
                                        return false;
                                    }
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before starting animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #region Scroll Left

                            if (_Animation_Cancel)
                                goto end;

                            // Set final value 
                            EndPos = left + width - _MainObject.Position.Width;
                            if (EndPos > left)
                                EndPos = left;

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    _MainObject.Position.Left -= AnimationStep;
                                    Refresh();

                                    // End animation?
                                    if (_MainObject.Position.Left <= EndPos)
                                    {   // Yes, set final value and exit
                                        _MainObject.Position.Left = EndPos;
                                        return false;
                                    }
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            #endregion
                        }
                        break;
                    case eAnimation.UnveilRightSmooth:
                    case eAnimation.UnveilRightChar:
                        {
                            #region UnveilRightSmooth / UnveilRightChar

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.SetText(text, true);
                            _MainObject.Clip = _MainObject.Position;
                            _MainObject.LimitClip(this.Region);
                            _MainObject.Visible = true;
                            _MainObject.Clip.Width = 0;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.Clip = _SecondObject.Position;
                            _SecondObject.LimitClip(this.Region);
                            _SecondObject.Visible = true;
                            int ObjectSeparation = (int)(_MainObject.CharWidth_Avg * 10);

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.2f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animationType == eAnimation.UnveilRightChar)
                                StepSize = _MainObject.CharWidth_Avg;

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region UnveilRight

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Adjust clip of main object (text to unveil)
                                    _MainObject.Clip.Width += AnimationStep;

                                    // Adjust clip of second object (text to hide)
                                    _SecondObject.Clip.Left += AnimationStep;
                                    _SecondObject.Clip.Width -= AnimationStep;

                                    // Exit animation
                                    if (_SecondObject.Clip.Width <= 0)
                                        return false;

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.SlideUpSmooth:
                        {
                            #region SlideUpSmooth

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Position.Top = this.Region.Bottom;
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.07f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region SlideUp

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Adjust position of main object (text to unveil)
                                    _MainObject.Position.Top -= AnimationStep;

                                    // Adjust positon of second object (text to hide)
                                    _SecondObject.Position.Top -= AnimationStep;

                                    // Exit animation
                                    if (_MainObject.Position.Top <= this.Region.Top)
                                    {
                                        _MainObject.Position.Top = this.Region.Top;
                                        _SecondObject.Visible = false;
                                        Refresh();
                                        return false;
                                    }

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.SlideDownSmooth:
                        {
                            #region SlideDownSmooth

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Position.Top = this.Region.Top - _MainObject.Position.Height;
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.07f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region SlideDown

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Adjust position of main object (text to unveil)
                                    _MainObject.Position.Top += AnimationStep;

                                    // Adjust positon of second object (text to hide)
                                    _SecondObject.Position.Top += AnimationStep;

                                    // Exit animation
                                    if (_MainObject.Position.Top >= this.Region.Top)
                                    {
                                        _MainObject.Position.Top = this.Region.Top;
                                        _SecondObject.Visible = false;
                                        Refresh();
                                        return false;
                                    }

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.SlideRightSmooth:
                        {
                            #region SlideRightSmooth

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Position.Left = this.Region.Left - _MainObject.Position.Width;
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.55f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region SlideRight

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Adjust position of main object (text to unveil)
                                    if (_MainObject.Position.Left < this.Region.Left)
                                        _MainObject.Position.Left += AnimationStep;
                                    else
                                        _MainObject.Position.Left = this.Region.Left;

                                    // Adjust positon of second object (text to hide)
                                    _SecondObject.Position.Left += (int)(AnimationStep * 1.2f);

                                    // Set alpha value of object being transitioned out (this is a result of the distance it has moved)
                                    _SecondObject.Alpha = 1.0f - ((float)(_SecondObject.Position.Left - this.Region.Left) / (float)(this.Region.Width / 2));

                                    // Exit animation
                                    if (_MainObject.Position.Left == this.Region.Left && _SecondObject.Alpha <= 0.05f)
                                    {
                                        _MainObject.Position.Left = this.Region.Left;
                                        _SecondObject.Visible = false;
                                        Refresh();
                                        return false;
                                    }

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.SlideLeftSmooth:
                        {
                            #region SlideLeftSmooth

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Position.Left = this.Region.Right;
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.7f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;

                            if (_Animation_Cancel)
                                goto end;

                            // Animation runs until it's canceled or changed 
                            #region SlideLeft

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Adjust position of main object (text to unveil)
                                    if (_MainObject.Position.Left > this.Region.Left)
                                        _MainObject.Position.Left -= AnimationStep;
                                    else
                                        _MainObject.Position.Left = this.Region.Left;

                                    // Adjust positon of second object (text to hide)
                                    _SecondObject.Position.Left -= AnimationStep;

                                    // Set alpha value of object being transitioned out (this is a result of the distance it has moved)
                                    _SecondObject.Alpha = 1.0f - (System.Math.Abs((float)(_SecondObject.Position.Left - this.Region.Left)) / (float)(this.Region.Width / 2));

                                    // Exit animation
                                    if (_MainObject.Position.Left == this.Region.Left && _SecondObject.Alpha <= 0.05f)
                                    {
                                        _MainObject.Position.Left = this.Region.Left;
                                        _SecondObject.Visible = false;
                                        Refresh();
                                        return false;
                                    }

                                    Refresh();
                                }

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.CrossFade:
                        {
                            #region CrossFade

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _MainObject.Alpha = 0f;
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();
                            _SecondObject.Alpha = 1f;

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.003f * AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;

                            if (_Animation_Cancel)
                                goto end;

                            #region CrossFade

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Adjust alpha of first object 
                                _MainObject.Alpha += AnimationStepF;

                                // Adjust alpha of second object 
                                _SecondObject.Alpha -= AnimationStepF;

                                // Exit animation
                                if (_MainObject.Alpha >= 1.0f)
                                {
                                    _MainObject.Alpha = 1.0f;
                                    _SecondObject.Visible = false;
                                    Refresh();
                                    return false;
                                }

                                Refresh();

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;
                    case eAnimation.GrowAndFade:
                        {
                            #region GrowAndFade

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.Alpha = 0f;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            _SecondObject.Visible = true;
                            _SecondObject.SetText(text2, true);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            Rectangle _SecondObjectStartPos = _SecondObject.Position;

                            SmoothAnimator Animation = new SmoothAnimator(0.01f * AnimationSpeed);

                            if (_Animation_Cancel)
                                goto end;

                            #region GrowAndFade

                            float ScaleEndValue = 5.0f;

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Adjust scale of second object 
                                _SecondObject.Scale.X += AnimationStepF;
                                _SecondObject.Scale.Y += AnimationStepF;

                                // Set alpha value of object being transitioned out (this is a result of the scale)
                                _SecondObject.Alpha = 1.0f - ((float)(_SecondObject.Scale.X) / ScaleEndValue);

                                // Set alpha value of object being transitioned in (this is a result of the scale)
                                _MainObject.Alpha = ((float)(_SecondObject.Scale.X) / ScaleEndValue);

                                // Exit animation
                                if (_SecondObject.Scale.X >= ScaleEndValue)
                                {
                                    _SecondObject.Visible = false;
                                    _MainObject.Visible = true;
                                    _MainObject.Alpha = 1f;
                                    Refresh();
                                    return false;
                                }

                                Refresh();

                                // Continue animation
                                return true;
                            });

                            #endregion

                            // Delay before ending animation
                            if (_Animation_Run && DelayedRun)
                                SleepEx(1000, ref _Animation_Cancel);

                            #endregion
                        }
                        break;

                    case eAnimation.Glow:
                        {
                            #region Glow

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(2);

                            eTextFormat GlowTextFormat = eTextFormat.Normal;
                            #region Find GlowTextFormat effect to use

                            switch (_textFormat)
                            {
                                case eTextFormat.Normal:
                                    GlowTextFormat = eTextFormat.GlowBig;
                                    break;
                                case eTextFormat.DropShadow:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                case eTextFormat.Bold:
                                    GlowTextFormat = eTextFormat.GlowBoldBig;
                                    break;
                                case eTextFormat.Italic:
                                    GlowTextFormat = eTextFormat.GlowItalicBig;
                                    break;
                                case eTextFormat.Underline:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                case eTextFormat.BoldShadow:
                                    GlowTextFormat = eTextFormat.GlowBoldBig;
                                    break;
                                case eTextFormat.ItalicShadow:
                                    GlowTextFormat = eTextFormat.GlowItalicBig;
                                    break;
                                case eTextFormat.UnderlineShadow:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                case eTextFormat.Outline:
                                    GlowTextFormat = eTextFormat.GlowBig;
                                    break;
                                case eTextFormat.Glow:
                                case eTextFormat.BoldGlow:
                                case eTextFormat.GlowBig:
                                case eTextFormat.GlowBoldBig:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                case eTextFormat.OutlineNarrow:
                                case eTextFormat.OutlineFat:
                                case eTextFormat.OutlineNoFill:
                                case eTextFormat.OutlineNoFillNarrow:
                                case eTextFormat.OutlineNoFillFat:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                case eTextFormat.OutlineItalic:
                                case eTextFormat.OutlineItalicNarrow:
                                case eTextFormat.OutlineItalicFat:
                                case eTextFormat.OutlineItalicNoFill:
                                case eTextFormat.OutlineItalicNoFillNarrow:
                                case eTextFormat.OutlineItalicNoFillFat:
                                    GlowTextFormat = eTextFormat.GlowItalicBig;
                                    break;
                                case eTextFormat.GlowItalic:
                                case eTextFormat.BoldGlowItalic:
                                case eTextFormat.GlowItalicBig:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                                default:
                                    GlowTextFormat = eTextFormat.Normal;
                                    break;
                            }

                            #endregion

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.Alpha = 1f;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();
                            Refresh();
                            // Cancel animation if no glow effect is possible
                            if (GlowTextFormat == eTextFormat.Normal)
                            {
                                Refresh();
                                CancelAnimation = true;
                                goto end;
                            }

                            _SecondObject.Visible = true;
                            _SecondObject.Alpha = 0f;
                            _SecondObject.SetText(text, true, GlowTextFormat);
                            _SecondObject.ResetClip();

                            // Render initial state
                            Refresh();

                            Rectangle _SecondObjectStartPos = _SecondObject.Position;

                            SmoothAnimator Animation = new SmoothAnimator(0.001f * AnimationSpeed);

                            if (_Animation_Cancel)
                                goto end;

                            #region GrowAndFade

                            bool FadeIn = true;

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Set alpha value of glow effect
                                if (FadeIn)
                                {
                                    _SecondObject.Alpha += AnimationStepF;
                                    if (_SecondObject.Alpha >= 1.0f)
                                        FadeIn = false;
                                }
                                else
                                {
                                    _SecondObject.Alpha -= AnimationStepF;
                                    if (_SecondObject.Alpha <= 0.0f)
                                    {   // End animation
                                        _SecondObject.Visible = false;
                                        _MainObject.Visible = true;
                                        Refresh();
                                        return false;
                                    }
                                }

                                Refresh();

                                // Continue animation
                                return true;
                            });

                            #endregion

                            #endregion
                        }
                        break;

                    case eAnimation.Flash:
                        {
                            #region Flash

                            if (_Animation_Cancel)
                                goto end;

                            // Clear old parameters if required
                            ClearRenderObjects(1);

                            // Set parameters
                            _MainObject.Visible = true;
                            _MainObject.Alpha = 1f;
                            _MainObject.SetText(text, true);
                            _MainObject.ResetClip();

                            // Render initial state
                            Refresh();

                            SmoothAnimator Animation = new SmoothAnimator(0.001f * AnimationSpeed);

                            if (_Animation_Cancel)
                                goto end;

                            #region Flash

                            bool FadeOut = true;

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || _Animation_Cancel || !visible || !hooked())
                                    return false;

                                // Set alpha value of glow effect
                                if (FadeOut)
                                {
                                    _MainObject.Alpha -= AnimationStepF;
                                    if (_MainObject.Alpha <= 0.0f)
                                        FadeOut = false;
                                }
                                else
                                {
                                    _MainObject.Alpha += AnimationStepF;
                                    if (_MainObject.Alpha >= 1.0f)
                                    {   // End animation
                                        _MainObject.Alpha = 1.0f;
                                        _MainObject.Visible = true;
                                        Refresh();
                                        return false;
                                    }
                                }

                                Refresh();

                                // Continue animation
                                return true;
                            });

                            #endregion

                            #endregion
                        }
                        break;

                    default:
                        break;
                }

            }
            if (ContinousEffect && !CancelAnimation && !_Animation_Cancel)
            {
                // Allow animation thread to restart itself
                _Animation_Thread_Run.Set();
            }

        end:
            _Animation_Running = false;
        }

        #region Animation

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
                Refresh();
            }
        }

        private OMAnimatedLabel2.eAnimation _AnimationSingle = OMAnimatedLabel2.eAnimation.None;
        /// <summary>
        /// The effect to loop continuously
        /// </summary>
        public OMAnimatedLabel2.eAnimation AnimationSingle
        {
            get
            {
                return _AnimationSingle;
            }
            set
            {
                if (_AnimationSingle == value)
                    return;
                _AnimationSingle = value;
                Refresh();
            }
        }

        private AnimationActivationTypes _ActivationType = AnimationActivationTypes.TextToLong;

        /// <summary>
        /// Sets how the main animation effect should be activated
        /// </summary>
        public AnimationActivationTypes ActivationType
        {
            get
            {
                return _ActivationType;
            }
            set
            {
                _ActivationType = value;
            }
        }

        private void ClearRenderObjects()
        {
            RenderData mainObject = _MainObject;

            foreach (RenderData rd in _RenderObjects)
                rd.Dispose();

            _RenderObjects.Clear();

            mainObject = new RenderData(this);
            _RenderObjects.Add(mainObject);
        }
        private void ClearRenderObjects(int ClearCountCondition)
        {
            if (_RenderObjects.Count > ClearCountCondition)
                ClearRenderObjects();
        }

        private SizeF GetStringSize(string Text)
        {
            return Graphics.Graphics.MeasureString(Text, this.Font, _textFormat);
        }

        private void SleepEx(int delayMS, ref bool AbortVar)
        {
            SleepEx(delayMS, 1, ref AbortVar);
        }
        private void SleepEx(int delayMS, int resolution, ref bool AbortVar)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (sw.Elapsed.TotalMilliseconds < delayMS)
            {               
                // Abort?
                if (AbortVar)
                    return;
                Thread.Sleep(resolution);
                // Abort?
                if (AbortVar)
                    return;
            }
        }

        private class AnimationData
        {
            public OMAnimatedLabel2.eAnimation animationType;
            public string text;
            public string text2;
            public float AnimationSpeed;
        }
        private void Animation_Execute(AnimationData animationData)
        {
            Animation_Execute(animationData.animationType, animationData.text, animationData.text2, false, false, AnimationSpeed);
        }

        private Thread _Animation_Thread = null;
        private bool _Animation_Run = false;
        private bool _Animation_Cancel = false;
        private bool _Animation_Running = false;
        private EventWaitHandle _Animation_Thread_Run = null;
        private AnimationData _AnimationData_Single_Current = null;
        private AnimationData _AnimationData_Single_Next = null;

        private void Animation_Cancel()
        {
            // Cancel thread
            if (_Animation_Thread != null)
            {
                _Animation_Cancel = true;

                // Wake thread up (in case it's sleeping)
                if (!_Animation_Running)
                    _Animation_Thread_Run.Set();
            }
        }

        private void Animation_Start()
        {
            // Cancel if no animation is active
            if (Animation == eAnimation.None && AnimationSingle == eAnimation.None)
            {   // Cancel thread
                Animation_Cancel();
                return;
            }

            // Reconfigure animation parameters if needed (This method is called each rendering frame)
            if (_RefreshGraphic)
            {   // Refresh rendering objects
                for (int i = 0; i < _RenderObjects.Count; i++)
                    _RenderObjects[i].RefreshTexture(_textFormat);
            }

            // Start animation thread
            if (_Animation_Thread == null)
            {
                if ((_Animation != eAnimation.None) || (_AnimationSingle != eAnimation.None))
                {
                    _Animation_Run = true;
                    _Animation_Thread = new Thread(Animation_TreadCall);
                    _Animation_Thread.Name = String.Format("OMAnimatedLabel2 ({0} - Hash:{1})", name, this.GetHashCode());
                    _Animation_Thread.IsBackground = true;
                    _Animation_Thread.Start();
                    if (_Animation_Thread_Run != null)
                    {
                        _Animation_Thread_Run.Close();
                        _Animation_Thread_Run = null;
                    }
                    _Animation_Thread_Run = new EventWaitHandle(true, EventResetMode.AutoReset);
                }
            }
        }

        private void Animation_TreadCall()
        {
            while (_AnimationSingle != eAnimation.None || _Animation != eAnimation.None)
            {
                try
                {
                    if (_Animation_Thread_Run.WaitOne())
                    {
                        _Animation_Cancel = false;
                        if (_Animation_Run && !_Animation_Cancel)
                        {
                            try
                            {
                                // Execute single animation
                                if (!_Animation_Cancel)
                                {
                                restart:
                                    while (_AnimationData_Single_Next != null)
                                    {
                                        _AnimationData_Single_Current = _AnimationData_Single_Next;
                                        _AnimationData_Single_Next = null;
                                        _Animation_Cancel = false;
                                        Animation_Execute(_AnimationData_Single_Current);
                                    }

                                    // Execute continous animation
                                    if (_Animation != eAnimation.None && !_Animation_Cancel)
                                    {
                                        Animation_Execute(_Animation, _text, _TextPrevious, true, true, _AnimationSpeed);
                                    }

                                    if (_AnimationData_Single_Next != null)
                                        goto restart;
                                }
                            }
                            catch (Exception e)
                            {
                                BuiltInComponents.Host.DebugMsg("OMAnimatedLabel2", e);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            // Invalididate thread
            _Animation_Thread = null;
        }

        /// <summary>
        /// Transition in new text with the specified animation effect
        /// </summary>
        /// <param name="animationEffect"></param>
        /// <param name="newText"></param>
        public void TransitionInText(eAnimation animationEffect, string newText)
        {
            TransitionInText(animationEffect, newText, _AnimationSpeed);
        }
        /// <summary>
        /// Transition in new text with the specified animation effect
        /// </summary>
        /// <param name="animationEffect"></param>
        /// <param name="newText"></param>
        public void TransitionInText(eAnimation animationEffect, string newText, float AnimationSpeed)
        {
            // Handle empty strings
            if (String.IsNullOrEmpty(newText))
                newText = "";

            _TextPrevious = _text;
            _text = newText;

            // Create new single animation
            _AnimationData_Single_Next = new AnimationData() { animationType = animationEffect, text = _text, text2 = _TextPrevious, AnimationSpeed = AnimationSpeed };

            // Cancel any ongoing animations
            Animation_Cancel();
        }

        #endregion

        #region Parameters

        /// <summary>
        /// Active softedges
        /// </summary>
        public FadingEdge.GraphicSides SoftEdges { get; set; }
        
        /// <summary>
        /// Data to use for softedge
        /// </summary>
        private FadingEdge.GraphicData SoftEdgeData { get; set; }
        /// <summary>
        /// SoftEdge image
        /// </summary>
        private OImage imgSoftEdge = null;

        private Color background = Color.Transparent;
        /// <summary>
        /// The background color of the control (Default: Transparent)
        /// </summary>
        public Color Background
        {
            get
            {
                return background;
            }
            set
            {
                background = value;
            }
        }

        private string _TextPrevious = String.Empty;
        
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
                TransitionInText(_AnimationSingle, value);
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
                base.Width = value;
                //Animation_SetParameters();
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
                base.Height = value;
                //Animation_SetParameters();
            }
        }

        private float _AnimationSpeed = 1F;
        /// <summary>
        /// The speed multiplier of the animation (1.0F = run animation at original speed)
        /// </summary>
        public float AnimationSpeed
        {
            get
            {
                return _AnimationSpeed;
            }
            set
            {
                _AnimationSpeed = value;
            }
        }

        #endregion

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            // No use in rendering if text is empty
            //if (String.IsNullOrEmpty(_text))
            //    return;

            // Start animation
            Animation_Start();

            lock (this)
            {
                if (background != Color.Transparent)
                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * background.A), background)), Left + 1, Top + 1, Width - 2, Height - 2);

                // Save current clip and activate new clip region
                _Clip_Stored = g.Clip;

                // Render items 
                for (int i = _RenderObjects.Count - 1; i >= 0; i--)
                {
                    RenderData rd = _RenderObjects[i];
                    if (rd.Visible)
                    {
                        g.SetClipFast(rd.Clip.Left, rd.Clip.Top, rd.Clip.Width, rd.Clip.Height);
                        //g.DrawImage(rd.Texture, rd.Position.Left, rd.Position.Top, rd.Position.Width + 5, rd.Position.Height, this.GetAlphaValue(rd.Alpha));
                        Point Dimensions = new Point((rd.Position.Width + 5) * rd.Scale.X, rd.Position.Height * rd.Scale.Y);
                        Point Offset = new Point((Dimensions.X - (rd.Position.Width + 5)) / 2, (Dimensions.Y - rd.Position.Height) / 2);
                        g.DrawImage(rd.Texture, rd.Position.Left - Offset.X, rd.Position.Top - Offset.Y, Dimensions.X, Dimensions.Y, this.GetAlphaValue(rd.Alpha));
                        // Debug: Draw text limits
                        if (this._SkinDebug)
                            using (Pen PenDebug = new Pen(new Brush(Color.Green), 1))
                                g.DrawRectangle(PenDebug, rd.Position);
                    }
                }

                //g.ResetTransform();

                // Restore clip region
                g.Clip = _Clip_Stored;

                #region Render soft edges

                // Use soft edges?
                if (SoftEdges != FadingEdge.GraphicSides.None)
                {
                    if (Background != Color.Transparent)
                    {
                        Size SoftEdgeSize = new Size(Width + 2, Height + 2);
                        if (imgSoftEdge == null || imgSoftEdge.Width != SoftEdgeSize.Width || imgSoftEdge.Height != SoftEdgeSize.Height)
                        {   // Generate image
                            SoftEdgeData.Sides = SoftEdges;
                            SoftEdgeData.FadeSize = 0.03f;
                            SoftEdgeData.Width = SoftEdgeSize.Width;
                            SoftEdgeData.Height = SoftEdgeSize.Height;
                            if (imgSoftEdge != null)
                                imgSoftEdge.Dispose();
                            imgSoftEdge = FadingEdge.GetImage(SoftEdgeData);
                        }
                        g.DrawImage(imgSoftEdge, Left - 1, Top - 1, imgSoftEdge.Width, imgSoftEdge.Height, _RenderingValue_Alpha);
                    }
                }

                #endregion
            }
            base.RenderFinish(g, e);
        }

        #region ICloneable Members

        public override object Clone()
        {
            OMAnimatedLabel2 newObject = (OMAnimatedLabel2)this.MemberwiseClone();
            //OMContainer newObject = (OMContainer)base.Clone();
            newObject._RenderObjects = new List<RenderData>();
            foreach (RenderData rd in _RenderObjects)
                newObject._RenderObjects.Add((RenderData)rd.Clone());
            return newObject;
        }

        #endregion
    }

}
