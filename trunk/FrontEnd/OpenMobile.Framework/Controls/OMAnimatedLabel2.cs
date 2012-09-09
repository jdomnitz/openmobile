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
    public class OMAnimatedLabel2 : OMLabel
    {
        private class RenderData : IDisposable
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

            public bool Visible = true;

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
                Position = new Rectangle(home.left, home.top, home.width, home.height);

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
            public void RefreshTexture()
            {
                // Regeneration of texture required, delete old textures
                if (Texture != null)
                    Texture.Dispose();

                // Generate new texture
                _Texture = Graphics.Graphics.GenerateTextTexture(Texture, 0, Position.Left, Position.Top, Position.Width, Position.Height, _Text, home._font, home._textFormat, home._textAlignment, home._color, home._outlineColor);
            }

            public RenderData(OMAnimatedLabel2 home)
            {
                this.home = home;
            }

            public void SetText(string text, bool SetWidthToTextSize)
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
                    RefreshTexture();
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
            /// A single character turning outline color and effect font...scrolling left to right
            /// </summary>
            Pulse,
            /// <summary>
            /// A single character glowing outline color...scrolling left to right
            /// </summary>
            GlowPulse,
            /// <summary>
            /// Text is unveiled starting from the far left and working right in a smooth manner
            /// </summary>
            UnveilRightSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text up
            /// </summary>
            UnveilUpSmooth,
            /// <summary>
            /// Text is unveiled sliding the new text down
            /// </summary>
            UnveilDownSmooth,
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
            ScrollChar_LRRL
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

        private void ClearRenderObjects(int ObjectsCountCondition)
        {
            if (_RenderObjects.Count > ObjectsCountCondition)
                ClearRenderObjects();
        }

        private SizeF GetStringSize(string Text)
        {
            return Graphics.Graphics.MeasureString(Text, this.Font, _textFormat);
        }

        private void Animation_Execute(OMAnimatedLabel2.eAnimation animationType, string text, string text2, bool ContinousEffect)
        {
            bool CancelAnimation = false;

            // Disable object if no text is specified
            if (text == "")
                animationType = eAnimation.None;

            switch (animationType)
            {
                case eAnimation.None:
                    {
                        #region None

                        // Reset properties to default (no animation)
                        ClearRenderObjects();

                        // Request a redraw
                        Refresh();

                        // Cancel 
                        return;

                        #endregion
                    }
                    break;
                case eAnimation.ScrollSmooth_LR:
                case eAnimation.ScrollChar_LR:
                    {
                        #region ScrollSmooth_LR / ScrollChar_LR

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

                        // Check if activation properties is valid
                        if (_ActivationType == AnimationActivationTypes.TextToLong)
                        {
                            if (_MainObject.TextSize.Width < this.Region.Width)
                            {
                                CancelAnimation = true;
                                break;
                            }
                        }
                        
                        SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                        float AnimationValue = 0;

                        // Calculate stepsize
                        float StepSize = 1F;
                        if (animationType == eAnimation.ScrollChar_LR)
                            StepSize = _MainObject.CharWidth_Avg;

                        // Delay before starting animation
                        Thread.Sleep(1000);

                        // Animation runs until it's canceled or changed 
                        #region Scroll Left

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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

                        // Clear old parameters if required
                        ClearRenderObjects(1);

                        // Set parameters
                        _MainObject.SetText(text, true);
                        _MainObject.ResetClip();

                        // Check if activation properties is valid
                        if (_ActivationType == AnimationActivationTypes.TextToLong)
                        {
                            if (_MainObject.TextSize.Width < this.Region.Width)
                            {
                                CancelAnimation = true;
                                break;
                            }
                        }

                        SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                        float AnimationValue = 0;

                        int EndPos = left + width - _MainObject.Position.Width;
                        if (EndPos < left)
                            EndPos = left;

                        // Calculate stepsize
                        float StepSize = 1F;
                        if (animationType == eAnimation.ScrollChar_LRRL)
                            StepSize = _MainObject.CharWidth_Avg;

                        // Delay before starting animation
                        Thread.Sleep(1000);

                        #region Scroll Right

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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

                        #region Scroll Left

                        // Set final value 
                        EndPos = left + width - _MainObject.Position.Width;
                        if (EndPos > left)
                            EndPos = left;

                        // Delay before starting animation
                        Thread.Sleep(1000);

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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
                case eAnimation.Pulse:
                    break;
                case eAnimation.GlowPulse:
                    break;
                case eAnimation.UnveilRightSmooth:
                case eAnimation.UnveilRightChar:
                    {
                        #region UnveilRightSmooth / UnveilRightChar

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

                        SmoothAnimator Animation = new SmoothAnimator(0.2f * _AnimationSpeed);
                        float AnimationValue = 0;

                        // Calculate stepsize
                        float StepSize = 1F;
                        if (animationType == eAnimation.UnveilRightChar)
                            StepSize = _MainObject.CharWidth_Avg;

                        // Animation runs until it's canceled or changed 
                        #region UnveilRight

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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
                        Thread.Sleep(1000);

                        #endregion
                    }
                    break;
                case eAnimation.UnveilUpSmooth:
                    {
                        #region UnveilUpSmooth

                        // Clear old parameters if required
                        ClearRenderObjects(2);

                        // Set parameters
                        _MainObject.SetText(text, true);
                        _MainObject.ResetClip();
                        _MainObject.Visible = true;
                        _MainObject.Position.Top = this.Region.Bottom;
                        _SecondObject.SetText(text2, true);
                        _SecondObject.ResetClip();
                        _SecondObject.Visible = true;

                        SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                        float AnimationValue = 0;

                        // Calculate stepsize
                        float StepSize = 1F;

                        // Animation runs until it's canceled or changed 
                        #region UnveilUp

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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
                                    return false;
                                }

                                Refresh();
                            }

                            // Continue animation
                            return true;
                        });

                        #endregion

                        // Delay before ending animation
                        Thread.Sleep(1000);

                        #endregion
                    }
                    break;
                case eAnimation.UnveilDownSmooth:
                    {
                        #region UnveilDownSmooth

                        // Clear old parameters if required
                        ClearRenderObjects(2);

                        // Set parameters
                        _MainObject.SetText(text, true);
                        _MainObject.ResetClip();
                        _MainObject.Visible = true;
                        _MainObject.Position.Top = this.Region.Top - _MainObject.Position.Height;
                        _SecondObject.SetText(text2, true);
                        _SecondObject.ResetClip();
                        _SecondObject.Visible = true;

                        SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                        float AnimationValue = 0;

                        // Calculate stepsize
                        float StepSize = 1F;

                        // Animation runs until it's canceled or changed 
                        #region UnveilDown

                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                        {
                            // Cancel animation
                            if (!_Animation_Run || !visible || !hooked())
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
                                    return false;
                                }

                                Refresh();
                            }

                            // Continue animation
                            return true;
                        });

                        #endregion

                        // Delay before ending animation
                        Thread.Sleep(1000);

                        #endregion
                    }
                    break;
                default:
                    break;
            }

            if (ContinousEffect && !CancelAnimation)
            {
                // Allow animation thread to restart itself
                _Animation_Thread_Run.Set();
            }
        }

        private Thread _Animation_Thread = null;
        private bool _Animation_Enable = false;
        private bool _Animation_Run = false;
        private bool _Animation_RunSingle = false;
        private EventWaitHandle _Animation_Thread_Run = new EventWaitHandle(false, EventResetMode.AutoReset);

        private void Animation_Cancel()
        {
            // Cancel thread
            if (_Animation_Thread != null)
            {
                ClearRenderObjects();
                _Animation_Run = false;
                _Animation_Thread.Abort();
                _Animation_Thread = null;
            }

            _Animation_RunSingle = true;
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
                foreach (RenderData rd in _RenderObjects)
                    rd.RefreshTexture();
            }

            // Start animation thread
            if (_Animation_Thread == null)
            {
                if ((_Animation != eAnimation.None) || (_AnimationSingle != eAnimation.None))
                {
                    _Animation_Run = true;
                    _Animation_Thread = new Thread(Animation_TreadCall);
                    _Animation_Thread.Name = String.Format("OMAnimatedLabel2 ({0})", name);
                    _Animation_Thread.IsBackground = true;
                    _Animation_Thread.Start();
                    _Animation_Thread_Run.Set();
                }
            }
        }

        private void Animation_TreadCall()
        {
            while (_Animation_Run)
            {
                if (_Animation_Thread_Run.WaitOne())
                {
                    if (_AnimationSingle != eAnimation.None && _Animation_RunSingle)
                    {
                        _Animation_RunSingle = false;
                        Animation_Execute(_AnimationSingle, _text, _TextPrevious, true);
                    }

                    if (_Animation != eAnimation.None)
                        Animation_Execute(_Animation, _text, _TextPrevious, true);
                }
            }

            // Invalididate thread
            _Animation_Thread = null;
        }

/*
        private SizeF[] _StringSize = new SizeF[2];


        private void Animation_SetParameters(eAnimation animation)
        {
            lock (this)
            {
                _TextCharWidth_Avg = new float[2];
                if (!String.IsNullOrEmpty(_text))
                {
                    // Extract string size
                    _StringSize[0] = GetStringSize(_text);

                    // Calculate average character width
                    if (_StringSize[0].Width > 0)
                        _TextCharWidth_Avg[0] = _StringSize[0].Width / _text.Length;
                }
                if (!String.IsNullOrEmpty(_Text_Previous))
                {
                    // Extract string size
                    _StringSize[1] = GetStringSize(_Text_Previous);

                    // Calculate average character width
                    if (_StringSize[1].Width > 0)
                        _TextCharWidth_Avg[1] = _StringSize[1].Width / _Text_Previous.Length;
                }

                // Regeneration of texture required, delete old textures
                if (textTexture != null)
                    textTexture.Dispose();
                if (_Text_Previous_Texture != null)
                    _Text_Previous_Texture.Dispose();

                // Default clipping data
                _Clip_Current = new Rectangle[2];
                _Clip_Current[0] = new Rectangle(left, top, width, height);
                _Clip_Current[1] = new Rectangle(left, top, width, height);

                switch (animation)
                {
                    case eAnimation.None:
                        #region None

                        _Pos_Current = new Rectangle[1];
                        _Pos_Current[0] = new Rectangle(left, top, width, height);
                        _Pos_Current[0].Width = Width;
                        // Set clipping rectangle
                        _Clip_Current = new Rectangle[1];
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        #endregion
                        break;

                    case eAnimation.ScrollSmooth_LR:
                    case eAnimation.ScrollChar_LR:
                        #region ScrollSmooth_LR / ScrollChar_LR

                        _Pos_Current = new Rectangle[2];
                        _Pos_Current[0] = new Rectangle(left, top, width, height - 1);
                        _Pos_Current[0].Left = left;
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));
                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;
                        _Text_Textures[1] = textTexture;

                        // Set second text string data
                        _Pos_Current[1] = _Pos_Current[0];
                        _Pos_Current[1].Left = this.Region.Right + (_Pos_Current[0].Width >= this.Region.Width ? 100 : 0);

                        // Set text visibility
                        _Pos_Active = new bool[_Pos_Current.Length];
                        _Pos_Active[0] = true;
                        _Pos_Active[1] = false;

                        #endregion
                        break;

                    case eAnimation.ScrollSmooth_LRRL:
                    case eAnimation.ScrollChar_LRRL:
                        #region ScrollSmooth_LRRL / ScrollSmooth_LRRL

                        _Pos_Current = new Rectangle[1];
                        _Pos_Current[0] = new Rectangle(left, top, width, height);
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));
                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        #endregion
                        break;

                    case eAnimation.Pulse:
                        break;

                    case eAnimation.GlowPulse:
                        break;

                    case eAnimation.UnveilRightChar:
                    case eAnimation.UnveilRightSmooth:
                    case eAnimation.UnveilLeftChar:
                    case eAnimation.UnveilLeftSmooth:
                        #region UnveilRightChar / UnveilRightSmooth

                        _Pos_Current = new Rectangle[2];
                        _Pos_Current[0] = new Rectangle(left, top, width, height - 1);
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));

                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, 0, height);
                        _Clip_Current[1] = new Rectangle(left, top, width, height);

                        // Generate text textures
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        // Set second text string data
                        _Pos_Current[1] = _Pos_Current[0];
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[1].Width = (int)(_TextCharWidth_Avg[1] * (_Text_Previous.Length + 1));
                        // Generate text textures
                        _Text_Previous_Texture = Graphics.Graphics.GenerateTextTexture(_Text_Previous_Texture, 0, _Pos_Current[1].Left, _Pos_Current[1].Top, _Pos_Current[1].Width, _Pos_Current[1].Height, _Text_Previous, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[1] = _Text_Previous_Texture;

                        // Set text visibility
                        _Pos_Active = new bool[_Pos_Current.Length];
                        _Pos_Active[0] = true;
                        _Pos_Active[1] = true;

                        #endregion
                        break;
                    default:
                        break;
                }
            }
        }


        private void Animation_Execute(eAnimation animation, bool SingleAnimation)
        {            
            
            // Configure parameters for the requested animation
            Animation_SetParameters(animation);

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

                switch (animation)
                {
                    case eAnimation.None:
                        Animation_Cancel();
                        break;

                    case eAnimation.ScrollSmooth_LR:
                    case eAnimation.ScrollChar_LR:
                        {
                            // Cancel if this is a single animation as this effect is not suitable for single animation
                            if (SingleAnimation)
                                break;

                            #region ScrollSmooth_LR / ScrollChar_LR

                            // Delay before starting animation effect
                            Thread.Sleep(1000);

                            SmoothAnimator Animation = new SmoothAnimator(0.025f * _AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.ScrollChar_LR)
                                StepSize = _TextCharWidth_Avg[0];

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Show text 1?
                                    if (_Pos_Current[1].Left < this.Region.Left && _Pos_Current[1].Right < this.Region.Right)
                                    {   // Yes
                                        _Pos_Active[0] = true;
                                    }

                                    // Move text 1
                                    if (_Pos_Active[0])
                                        _Pos_Current[0].Left -= AnimationStep;
                                    // Reset text 1?
                                    if (_Pos_Current[0].Left < (this.Region.Left - _Pos_Current[0].Width))
                                    {   // Yes, move text 1 to the right side
                                        _Pos_Current[0].Left = this.Region.Right;
                                        _Pos_Active[0] = false;
                                    }

                                    // Show text 2?
                                    if (_Pos_Current[0].Left < this.Region.Left && _Pos_Current[0].Right < this.Region.Right)
                                    {   // Yes
                                        _Pos_Active[1] = true;
                                    }

                                    // Move text 2
                                    if (_Pos_Active[1])
                                        _Pos_Current[1].Left -= AnimationStep;

                                    // Reset text 2?
                                    if (_Pos_Current[1].Left < (this.Region.Left - _Pos_Current[1].Width))
                                    {   // Yes, move text 1 to the right side
                                        _Pos_Current[1].Left = this.Region.Right;
                                        _Pos_Active[1] = false;
                                    }

                                    Refresh();
                                }
                                return true;
                            });

                            // Delay before changing animation
                            Thread.Sleep(500);

                            #endregion
                        }
                        break;

                    case eAnimation.ScrollSmooth_LRRL:
                    case eAnimation.ScrollChar_LRRL:
                        {
                            // Cancel if this is a single animation as this effect is not suitable for single animation
                            if (SingleAnimation)
                                break;
                            
                            #region ScrollSmooth_LRRL / ScrollChar_LRRL

                            SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                            float AnimationValue = 0;

                            int EndPos = left + width - _Pos_Current[0].Width;
                            if (EndPos < left)
                                EndPos = left;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.ScrollChar_LRRL)
                                StepSize = _TextCharWidth_Avg[0];

                            if (_Pos_Current[0].Left < EndPos)
                            {
                                // Delay before starting animation
                                Thread.Sleep(1000);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        _Pos_Current[0].Left += AnimationStep;
                                        Refresh();

                                        // End animation?
                                        if (_Pos_Current[0].Left >= EndPos)
                                        {   // Yes, set final value and exit
                                            _Pos_Current[0].Left = EndPos;
                                            return false;
                                        }
                                    }

                                    // Continue animation
                                    return true;
                                });
                            }
                            else
                            {
                                // Set final value 
                                EndPos = left + width - _Pos_Current[0].Width;
                                if (EndPos > left)
                                    EndPos = left;

                                // Delay before starting animation
                                Thread.Sleep(1000);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        _Pos_Current[0].Left -= AnimationStep;
                                        Refresh();

                                        // End animation?
                                        if (_Pos_Current[0].Left <= EndPos)
                                        {   // Yes, set final value and exit
                                            _Pos_Current[0].Left = EndPos;
                                            return false;
                                        }
                                    }

                                    // Continue animation
                                    return true;
                                });
                            }

                            #endregion
                        }
                        break;

                    case eAnimation.Pulse:
                        break;

                    case eAnimation.GlowPulse:
                        break;

                    case eAnimation.UnveilRightChar:
                    case eAnimation.UnveilRightSmooth:
                    case eAnimation.UnveilLeftChar:
                    case eAnimation.UnveilLeftSmooth:
                        {
                            #region UnveilLeft / UnveilRight

                            SmoothAnimator Animation = new SmoothAnimator(0.4f * _AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.UnveilRightChar)
                                StepSize = _TextCharWidth_Avg[0];

                            if (animation == eAnimation.UnveilRightChar || animation == eAnimation.UnveilRightSmooth)
                            {
                                #region UnveilRightChar / UnveilRightSmooth

                                // Set start values
                                _Clip_Current[0] = new Rectangle(left, top, 0, height);
                                if (_Pos_Active[1])
                                    _Clip_Current[1] = new Rectangle(left, top, width, height);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        // Reveal new text
                                        _Clip_Current[0].Right += AnimationStep;
                                        // Limit clip
                                        if (_Clip_Current[0].Right > this.Region.Right)
                                            _Clip_Current[0].Right = this.Region.Right;

                                        // Hide old text
                                        if (_Clip_Current[1].Right <= this.Region.Right)
                                        {
                                            _Clip_Current[1].Left += AnimationStep;
                                            _Clip_Current[1].Width -= AnimationStep;
                                        }
                                        // Limit clip
                                        if (_Clip_Current[1].Right > this.Region.Right)
                                            _Clip_Current[1].Right = this.Region.Right + 1;

                                        // Animation completed?
                                        if (_Clip_Current[0].Right >= this.Region.Right)
                                        {
                                            _Pos_Active[0] = true;
                                            _Pos_Active[1] = false;
                                            Refresh();
                                            return false;
                                        }
                                    }

                                    Refresh();
                                    return true;
                                });

                                #endregion
                            }
                            else
                            {
                                #region UnveilLeftChar / UnveilLeftSmooth

                                // Set start values
                                _Clip_Current[0] = new Rectangle(this.Region.Right, top, 0, height);
                                if (_Pos_Active[1])
                                    _Clip_Current[1] = new Rectangle(left, top, width, height);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        // Reveal new text
                                        _Clip_Current[0].Left -= AnimationStep;
                                        _Clip_Current[0].Width += AnimationStep;

                                        // Limit clip
                                        if (_Clip_Current[0].Left < this.Region.Left)
                                            _Clip_Current[0].Left = this.Region.Left;
                                        // Limit clip
                                        if (_Clip_Current[0].Width > this.Region.Width)
                                            _Clip_Current[0].Width = this.Region.Width;

                                        // Hide old text
                                        if (_Clip_Current[1].Width > 0)
                                            _Clip_Current[1].Width -= AnimationStep;
                                        // Limit clip
                                        if (_Clip_Current[1].Width < 0)
                                            _Clip_Current[1].Width = 0;

                                        // Animation completed?
                                        if (_Clip_Current[0].Left <= this.Region.Left)
                                        {
                                            _Pos_Active[0] = true;
                                            _Pos_Active[1] = false;
                                            Refresh();
                                            return false;
                                        }
                                    }

                                    Refresh();
                                    return true;
                                });

                                #endregion
                            }

                            // Delay before changing animation
                            Thread.Sleep(500);

                            #endregion
                        }
                        break;

                    default:
                        break;
                }

                // Cancel animation if this is a single effect
                if (SingleAnimation)
                    _Animation_Run = false;

            }
        }
*/

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
                _TextPrevious = _text;
                base.Text = value;
                Animation_Cancel();
                _Animation_RunSingle = true;
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
                        g.DrawImage(rd.Texture, rd.Position.Left, rd.Position.Top, rd.Position.Width + 5, rd.Position.Height, _RenderingValue_Alpha);
                        // Debug: Draw text limits
                        if (this._SkinDebug)
                            using (Pen PenDebug = new Pen(new Brush(Color.Green), 1))
                                g.DrawRectangle(PenDebug, rd.Position);
                    }
                }

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
    }

}

/* ORG CODE
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
    [System.Serializable]
    public class OMAnimatedLabel2 : OMLabel
    {
        //private class RenderData
        //{
        //    private OImage Text_Texture;
        //    private float[] CharWidth_Avg = new float[2];

        //}

        /// <summary>
        /// Texture for text
        /// </summary>
        private OImage _Text_Previous_Texture;
        private string _Text_Previous = "";

        /// <summary>
        /// Rendering textures
        /// </summary>
        private OImage[] _Text_Textures = new OImage[2];
        
        /// <summary>
        /// Calculated average size of one character
        /// </summary>
        private float[] _TextCharWidth_Avg = new float[2];

        /// <summary>
        /// Clipping region to use when drawing 
        /// </summary>
        private Rectangle[] _Clip_Current = new Rectangle[1];

        /// <summary>
        /// Position to use when drawing
        /// </summary>
        private Rectangle[] _Pos_Current = new Rectangle[1];
        private bool[] _Pos_Active = new bool[1];

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
            /// Text is unveiled starting from the far left and working right in a smooth manner
            /// </summary>
            UnveilRightSmooth,
            /// <summary>
            /// Text is unveiled starting from the far left and working right revealing one character at a time
            /// </summary>
            UnveilRightChar,
            /// <summary>
            /// Text is unveiled starting from the far right and working left in a smooth manner
            /// </summary>
            UnveilLeftSmooth,
            /// <summary>
            /// Text is unveiled starting from the far right and working left revealing one character at a time
            /// </summary>
            UnveilLeftChar,
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

        /// <summary>
        /// A label used for rendering various text effects
        /// </summary>
        [System.Obsolete("Use OMAnimatedLabel2(string name, int x, int y, int w, int h) instead")]
        public OMAnimatedLabel2()
            : base("", 0, 0, 200,200)
        {
            SoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
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
            SoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
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
            SoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
        }

        #endregion

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
                Animation_Cancel();
                Animation_SetParameters(_Animation);
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
                Animation_Cancel();
                Animation_SetParameters(_AnimationSingle);
                Refresh();
            }
        }

        private AnimationActivationTypes _ActivationType = AnimationActivationTypes.TextToLong;
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

        private SizeF[] _StringSize = new SizeF[2];

        private SizeF GetStringSize(string Text)
        {
            return Graphics.Graphics.MeasureString(Text, this.Font, _textFormat);
        }

        private void Animation_SetParameters(eAnimation animation)
        {
            lock (this)
            {
                _TextCharWidth_Avg = new float[2];
                if (!String.IsNullOrEmpty(_text))
                {
                    // Extract string size
                    _StringSize[0] = GetStringSize(_text);

                    // Calculate average character width
                    if (_StringSize[0].Width > 0)
                        _TextCharWidth_Avg[0] = _StringSize[0].Width / _text.Length;
                }
                if (!String.IsNullOrEmpty(_Text_Previous))
                {
                    // Extract string size
                    _StringSize[1] = GetStringSize(_Text_Previous);

                    // Calculate average character width
                    if (_StringSize[1].Width > 0)
                        _TextCharWidth_Avg[1] = _StringSize[1].Width / _Text_Previous.Length;
                }

                // Regeneration of texture required, delete old textures
                if (textTexture != null)
                    textTexture.Dispose();
                if (_Text_Previous_Texture != null)
                    _Text_Previous_Texture.Dispose();

                // Default clipping data
                _Clip_Current = new Rectangle[2];
                _Clip_Current[0] = new Rectangle(left, top, width, height);
                _Clip_Current[1] = new Rectangle(left, top, width, height);

                switch (animation)
                {
                    case eAnimation.None:
                        #region None

                        _Pos_Current = new Rectangle[1];
                        _Pos_Current[0] = new Rectangle(left, top, width, height);
                        _Pos_Current[0].Width = Width;
                        // Set clipping rectangle
                        _Clip_Current = new Rectangle[1];
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        #endregion
                        break;

                    case eAnimation.ScrollSmooth_LR:
                    case eAnimation.ScrollChar_LR:
                        #region ScrollSmooth_LR / ScrollChar_LR

                        _Pos_Current = new Rectangle[2];
                        _Pos_Current[0] = new Rectangle(left, top, width, height - 1);
                        _Pos_Current[0].Left = left;
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));
                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;
                        _Text_Textures[1] = textTexture;

                        // Set second text string data
                        _Pos_Current[1] = _Pos_Current[0];
                        _Pos_Current[1].Left = this.Region.Right + (_Pos_Current[0].Width >= this.Region.Width ? 100 : 0);

                        // Set text visibility
                        _Pos_Active = new bool[_Pos_Current.Length];
                        _Pos_Active[0] = true;
                        _Pos_Active[1] = false;

                        #endregion
                        break;

                    case eAnimation.ScrollSmooth_LRRL:
                    case eAnimation.ScrollChar_LRRL:
                        #region ScrollSmooth_LRRL / ScrollSmooth_LRRL

                        _Pos_Current = new Rectangle[1];
                        _Pos_Current[0] = new Rectangle(left, top, width, height);
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));
                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, width, height);
                        // Generate text texture
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        #endregion
                        break;

                    case eAnimation.Pulse:
                        break;

                    case eAnimation.GlowPulse:
                        break;

                    case eAnimation.UnveilRightChar:
                    case eAnimation.UnveilRightSmooth:
                    case eAnimation.UnveilLeftChar:
                    case eAnimation.UnveilLeftSmooth:
                        #region UnveilRightChar / UnveilRightSmooth

                        _Pos_Current = new Rectangle[2];
                        _Pos_Current[0] = new Rectangle(left, top, width, height - 1);
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[0].Width = (int)(_TextCharWidth_Avg[0] * (_text.Length + 1));

                        // Set clipping rectangle
                        _Clip_Current[0] = new Rectangle(left, top, 0, height);
                        _Clip_Current[1] = new Rectangle(left, top, width, height);

                        // Generate text textures
                        textTexture = Graphics.Graphics.GenerateTextTexture(textTexture, 0, _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width, _Pos_Current[0].Height, _text, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[0] = textTexture;

                        // Set second text string data
                        _Pos_Current[1] = _Pos_Current[0];
                        // Limit width of text rectangle to just fit text to remove any text alignment values
                        _Pos_Current[1].Width = (int)(_TextCharWidth_Avg[1] * (_Text_Previous.Length + 1));
                        // Generate text textures
                        _Text_Previous_Texture = Graphics.Graphics.GenerateTextTexture(_Text_Previous_Texture, 0, _Pos_Current[1].Left, _Pos_Current[1].Top, _Pos_Current[1].Width, _Pos_Current[1].Height, _Text_Previous, _font, _textFormat, _textAlignment, _color, _outlineColor);
                        _Text_Textures[1] = _Text_Previous_Texture;

                        // Set text visibility
                        _Pos_Active = new bool[_Pos_Current.Length];
                        _Pos_Active[0] = true;
                        _Pos_Active[1] = true;

                        #endregion
                        break;
                    default:
                        break;
                }
            }
        }

        private Thread _Animation_Thread = null;
        private bool _Animation_Enable = false;
        private bool _Animation_Run = false;
        private bool _Animation_RunSingle = false;

        private void Animation_Cancel()
        {
            // Cancel thread
            if (_Animation_Thread != null)
            {
                _Animation_Run = false;
                _Animation_Thread.Abort();
                _Animation_Thread = null;
            }

            _Animation_RunSingle = true;
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
            {   // Which animation is active?
                if (_AnimationSingle != eAnimation.None && _Animation_RunSingle)
                    Animation_SetParameters(_AnimationSingle);
                else
                    Animation_SetParameters(_Animation);
            }

            // Start animation thread
            if (_Animation_Thread == null)
            {
                if ((_Animation != eAnimation.None && _Animation_Enable) || (_AnimationSingle != eAnimation.None && _Animation_RunSingle))
                {
                    _Animation_Run = true;
                    _Animation_Thread = new Thread(Animation_TreadCall);
                    _Animation_Thread.Name = String.Format("OMAnimatedLabel2 ({0})", name);
                    _Animation_Thread.IsBackground = true;
                    _Animation_Thread.Start();
                }
            }
        }

        private void Animation_TreadCall()
        {
            // Execute and run single animation
            if (_AnimationSingle != eAnimation.None && _Animation_RunSingle)
            {
                _Animation_RunSingle = false;
                Animation_SetParameters(_AnimationSingle);
                Animation_Execute(_AnimationSingle, true);
            }

            if (_Animation_Enable)
            {
                // Execute and run continous animation
                if (_Animation != eAnimation.None)
                {
                    Animation_SetParameters(_Animation);
                    Animation_Execute(_Animation, false);
                }

            }
            // Invalididate thread
            _Animation_Thread = null;
        }

        private void Animation_Execute(eAnimation animation, bool SingleAnimation)
        {            
            
            // Configure parameters for the requested animation
            Animation_SetParameters(animation);

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

                switch (animation)
                {
                    case eAnimation.None:
                        Animation_Cancel();
                        break;

                    case eAnimation.ScrollSmooth_LR:
                    case eAnimation.ScrollChar_LR:
                        {
                            // Cancel if this is a single animation as this effect is not suitable for single animation
                            if (SingleAnimation)
                                break;

                            #region ScrollSmooth_LR / ScrollChar_LR

                            // Delay before starting animation effect
                            Thread.Sleep(1000);

                            SmoothAnimator Animation = new SmoothAnimator(0.025f * _AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.ScrollChar_LR)
                                StepSize = _TextCharWidth_Avg[0];

                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                            {
                                // Cancel animation
                                if (!_Animation_Run || !visible || !hooked())
                                    return false;

                                // Calculate animation value
                                AnimationValue += AnimationStepF;

                                // Animation step large enough?
                                if (AnimationValue > StepSize)
                                {
                                    AnimationStep = (int)AnimationValue;
                                    AnimationValue -= AnimationStep;

                                    // Show text 1?
                                    if (_Pos_Current[1].Left < this.Region.Left && _Pos_Current[1].Right < this.Region.Right)
                                    {   // Yes
                                        _Pos_Active[0] = true;
                                    }

                                    // Move text 1
                                    if (_Pos_Active[0])
                                        _Pos_Current[0].Left -= AnimationStep;
                                    // Reset text 1?
                                    if (_Pos_Current[0].Left < (this.Region.Left - _Pos_Current[0].Width))
                                    {   // Yes, move text 1 to the right side
                                        _Pos_Current[0].Left = this.Region.Right;
                                        _Pos_Active[0] = false;
                                    }

                                    // Show text 2?
                                    if (_Pos_Current[0].Left < this.Region.Left && _Pos_Current[0].Right < this.Region.Right)
                                    {   // Yes
                                        _Pos_Active[1] = true;
                                    }

                                    // Move text 2
                                    if (_Pos_Active[1])
                                        _Pos_Current[1].Left -= AnimationStep;

                                    // Reset text 2?
                                    if (_Pos_Current[1].Left < (this.Region.Left - _Pos_Current[1].Width))
                                    {   // Yes, move text 1 to the right side
                                        _Pos_Current[1].Left = this.Region.Right;
                                        _Pos_Active[1] = false;
                                    }

                                    Refresh();
                                }
                                return true;
                            });

                            // Delay before changing animation
                            Thread.Sleep(500);

                            #endregion
                        }
                        break;

                    case eAnimation.ScrollSmooth_LRRL:
                    case eAnimation.ScrollChar_LRRL:
                        {
                            // Cancel if this is a single animation as this effect is not suitable for single animation
                            if (SingleAnimation)
                                break;
                            
                            #region ScrollSmooth_LRRL / ScrollChar_LRRL

                            SmoothAnimator Animation = new SmoothAnimator(0.05f * _AnimationSpeed);
                            float AnimationValue = 0;

                            int EndPos = left + width - _Pos_Current[0].Width;
                            if (EndPos < left)
                                EndPos = left;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.ScrollChar_LRRL)
                                StepSize = _TextCharWidth_Avg[0];

                            if (_Pos_Current[0].Left < EndPos)
                            {
                                // Delay before starting animation
                                Thread.Sleep(1000);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        _Pos_Current[0].Left += AnimationStep;
                                        Refresh();

                                        // End animation?
                                        if (_Pos_Current[0].Left >= EndPos)
                                        {   // Yes, set final value and exit
                                            _Pos_Current[0].Left = EndPos;
                                            return false;
                                        }
                                    }

                                    // Continue animation
                                    return true;
                                });
                            }
                            else
                            {
                                // Set final value 
                                EndPos = left + width - _Pos_Current[0].Width;
                                if (EndPos > left)
                                    EndPos = left;

                                // Delay before starting animation
                                Thread.Sleep(1000);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        _Pos_Current[0].Left -= AnimationStep;
                                        Refresh();

                                        // End animation?
                                        if (_Pos_Current[0].Left <= EndPos)
                                        {   // Yes, set final value and exit
                                            _Pos_Current[0].Left = EndPos;
                                            return false;
                                        }
                                    }

                                    // Continue animation
                                    return true;
                                });
                            }

                            #endregion
                        }
                        break;

                    case eAnimation.Pulse:
                        break;

                    case eAnimation.GlowPulse:
                        break;

                    case eAnimation.UnveilRightChar:
                    case eAnimation.UnveilRightSmooth:
                    case eAnimation.UnveilLeftChar:
                    case eAnimation.UnveilLeftSmooth:
                        {
                            #region UnveilLeft / UnveilRight

                            SmoothAnimator Animation = new SmoothAnimator(0.4f * _AnimationSpeed);
                            float AnimationValue = 0;

                            // Calculate stepsize
                            float StepSize = 1F;
                            if (animation == eAnimation.UnveilRightChar)
                                StepSize = _TextCharWidth_Avg[0];

                            if (animation == eAnimation.UnveilRightChar || animation == eAnimation.UnveilRightSmooth)
                            {
                                #region UnveilRightChar / UnveilRightSmooth

                                // Set start values
                                _Clip_Current[0] = new Rectangle(left, top, 0, height);
                                if (_Pos_Active[1])
                                    _Clip_Current[1] = new Rectangle(left, top, width, height);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        // Reveal new text
                                        _Clip_Current[0].Right += AnimationStep;
                                        // Limit clip
                                        if (_Clip_Current[0].Right > this.Region.Right)
                                            _Clip_Current[0].Right = this.Region.Right;

                                        // Hide old text
                                        if (_Clip_Current[1].Right <= this.Region.Right)
                                        {
                                            _Clip_Current[1].Left += AnimationStep;
                                            _Clip_Current[1].Width -= AnimationStep;
                                        }
                                        // Limit clip
                                        if (_Clip_Current[1].Right > this.Region.Right)
                                            _Clip_Current[1].Right = this.Region.Right + 1;

                                        // Animation completed?
                                        if (_Clip_Current[0].Right >= this.Region.Right)
                                        {
                                            _Pos_Active[0] = true;
                                            _Pos_Active[1] = false;
                                            Refresh();
                                            return false;
                                        }
                                    }

                                    Refresh();
                                    return true;
                                });

                                #endregion
                            }
                            else
                            {
                                #region UnveilLeftChar / UnveilLeftSmooth

                                // Set start values
                                _Clip_Current[0] = new Rectangle(this.Region.Right, top, 0, height);
                                if (_Pos_Active[1])
                                    _Clip_Current[1] = new Rectangle(left, top, width, height);

                                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                                {
                                    // Cancel animation
                                    if (!_Animation_Run || !visible || !hooked())
                                        return false;

                                    // Calculate animation value
                                    AnimationValue += AnimationStepF;

                                    // Animation step large enough?
                                    if (AnimationValue > StepSize)
                                    {
                                        AnimationStep = (int)AnimationValue;
                                        AnimationValue -= AnimationStep;

                                        // Reveal new text
                                        _Clip_Current[0].Left -= AnimationStep;
                                        _Clip_Current[0].Width += AnimationStep;

                                        // Limit clip
                                        if (_Clip_Current[0].Left < this.Region.Left)
                                            _Clip_Current[0].Left = this.Region.Left;
                                        // Limit clip
                                        if (_Clip_Current[0].Width > this.Region.Width)
                                            _Clip_Current[0].Width = this.Region.Width;

                                        // Hide old text
                                        if (_Clip_Current[1].Width > 0)
                                            _Clip_Current[1].Width -= AnimationStep;
                                        // Limit clip
                                        if (_Clip_Current[1].Width < 0)
                                            _Clip_Current[1].Width = 0;

                                        // Animation completed?
                                        if (_Clip_Current[0].Left <= this.Region.Left)
                                        {
                                            _Pos_Active[0] = true;
                                            _Pos_Active[1] = false;
                                            Refresh();
                                            return false;
                                        }
                                    }

                                    Refresh();
                                    return true;
                                });

                                #endregion
                            }

                            // Delay before changing animation
                            Thread.Sleep(500);

                            #endregion
                        }
                        break;

                    default:
                        break;
                }

                // Cancel animation if this is a single effect
                if (SingleAnimation)
                    _Animation_Run = false;

            }
        }

        #endregion

        #region Parameters

        /// <summary>
        /// Enables a fading effect from the listbackground to the list
        /// </summary>
        public bool SoftEdges { get; set; }
        /// <summary>
        /// Data to use for softedge
        /// </summary>
        public FadingEdge.GraphicData SoftEdgeData { get; set; }
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
                _Text_Previous = _text;
                base.Text = value;
                Animation_Cancel();

                // Check if animation should be activated
                if (_ActivationType == AnimationActivationTypes.TextToLong)
                {   // Activate animation if text doesn't fit
                    if (GetStringSize(value).Width >= this.Region.Width)
                        _Animation_Enable = true;
                    else
                    {
                        _Animation_Enable = false;
                    }
                }
                else
                {   // Always activate animation
                    _Animation_Enable = true;
                }

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
                _Pos_Current[0].Height = value;
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
            if (String.IsNullOrEmpty(_text))
                return;

            // Start animation
            Animation_Start();

            lock (this)
            {
                if (background != Color.Transparent)
                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * background.A), background)), Left + 1, Top + 1, Width - 2, Height - 2);

                // Save current clip and activate new clip region
                _Clip_Stored = g.Clip;

                // Draw text 2
                if (_Pos_Current.Length >= 1)
                {
                    g.SetClipFast(_Clip_Current[1].Left, _Clip_Current[1].Top, _Clip_Current[1].Width, _Clip_Current[1].Height);
                    g.DrawImage(_Text_Textures[1], _Pos_Current[1].Left, _Pos_Current[1].Top, _Pos_Current[1].Width + 5, _Pos_Current[1].Height, _RenderingValue_Alpha);
                    // Debug: Draw text 2 limits
                    if (this._SkinDebug)
                        using (Pen PenDebug = new Pen(new Brush(Color.Green), 1))
                        {
                            g.DrawRectangle(PenDebug, _Pos_Current[1]);
                        }
                }

                // Draw text 1 (default)
                g.SetClipFast(_Clip_Current[0].Left, _Clip_Current[0].Top, _Clip_Current[0].Width, _Clip_Current[0].Height);
                g.DrawImage(_Text_Textures[0], _Pos_Current[0].Left, _Pos_Current[0].Top, _Pos_Current[0].Width + 5, _Pos_Current[0].Height, _RenderingValue_Alpha);
                // Debug: Draw text 1 limits
                if (this._SkinDebug)
                    using (Pen PenDebug = new Pen(new Brush(Color.Red), 1))
                    {
                        g.DrawRectangle(PenDebug, _Pos_Current[0]);
                    }

                // Restore clip region
                g.Clip = _Clip_Stored;

                #region Render soft edges

                // Use soft edges?
                if (SoftEdges)
                {
                    if (Background != Color.Transparent)
                    {
                        Size SoftEdgeSize = new Size(Width + 2, Height + 2);
                        if (imgSoftEdge == null || imgSoftEdge.Width != SoftEdgeSize.Width || imgSoftEdge.Height != SoftEdgeSize.Height)
                        {   // Generate image
                            SoftEdgeData.Sides = FadingEdge.GraphicSides.Left | FadingEdge.GraphicSides.Right;
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
    }

}
*/
