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
using OpenMobile.Graphics;
using OpenMobile;
using System.Collections.Generic;
using OpenMobile.Graphics.OpenGL;

namespace OpenMobile.Controls
{
    /// <summary>
    /// An "coverflow" style control for images
    /// </summary>
    public class OMImageFlow : OMLabel, IThrow, IHighlightable, IClickableAdvanced
    {
        // Code is based on the free source code found here http://lxd.bumuckl.com/home.php but modified to fit the OpenMobile project.

        #region Preconfigured layouts

        /// <summary>
        /// Creates a "pyramid" style control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static OMImageFlow PreConfigLayout_Pyramid(string name, int left, int top, int width, int height)
        {
            OMImageFlow imgFlow = new OMImageFlow(name, left, top, width, height);
            imgFlow.Item_DistancePushBack = 0.3f;
            imgFlow.Item_Distance = 0.3f;
            imgFlow.Item_CenterImageScale = 1;
            return imgFlow;
        }

        /// <summary>
        /// Apply the "pyramid" style to a OMImageFlow control
        /// </summary>
        /// <param name="control"></param>
        public static void PreConfigLayout_Pyramid_ApplyToControl(OMImageFlow control)
        {
            control.Item_DistancePushBack = 0.3f;
            control.Item_Distance = 0.3f;
            control.Item_CenterImageScale = 1;
        }

        /// <summary>
        /// Creates a "flat" style control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static OMImageFlow PreConfigLayout_Flat(string name, int left, int top, int width, int height)
        {
            OMImageFlow imgFlow = new OMImageFlow(name, left, top, width, height);
            imgFlow.Item_Rotation = 0;
            imgFlow.Item_DistanceToCenter = 0.1f;
            imgFlow.Item_Distance = 1.1f;
            imgFlow.Item_CenterImagePlacementZ = 0;
            imgFlow.Control_PlacementOffsetY = 0.3f;
            imgFlow.Animation_FadeOutDistance = 3;
            imgFlow._Control_PlacementOffsetY = 0;
            //imgFlow.Item_CenterImageScale = 1;
            return imgFlow;
        }

        /// <summary>
        /// Apply the "flat" style to a OMImageFlow control
        /// </summary>
        /// <param name="control"></param>
        public static void PreConfigLayout_Flat_ApplyToControl(OMImageFlow control)
        {
            control.Item_Rotation = 0;
            control.Item_DistanceToCenter = 0.1f;
            control.Item_Distance = 1.1f;
            control.Item_CenterImagePlacementZ = 0;
            control.Control_PlacementOffsetY = 0.3f;
            control.Animation_FadeOutDistance = 3;
            control._Control_PlacementOffsetY = 0;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new imageflow
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMImageFlow(string name, int left, int top, int width, int height)
            : base(name, left, top, width, height)
        {
            _Step_Backup = _Animation_Step / _Animation_Speedup;
            if (_Step_Backup > _Animation_Step_Max) _Step_Backup = _Animation_Step_Max;

            // Set the default image size
            _ImageSize = new Size(this.height * 0.5f, this.height * 0.5f);

            _Control_PlacementOffsetY = -(this.height * 0.15f);

            _textAlignment = Alignment.BottomCenter;
        }

        ~OMImageFlow()
        {
            if (tmrUpdate != null)
                tmrUpdate.Dispose();
            if (_tmrThrowHandler != null)
                _tmrThrowHandler.Dispose();
        }

        #endregion

        #region private classes

        /// <summary>
        /// Vector data 
        /// </summary>
        protected struct RVect
        {
            /// <summary>
            /// X value
            /// </summary>
            public float x;
            /// <summary>
            /// Y value
            /// </summary>
            public float y;
            /// <summary>
            /// Z Value
            /// </summary>
            public float z;
            /// <summary>
            /// Rotation
            /// </summary>
            public float rot;
            /// <summary>
            /// Scale
            /// </summary>
            public float scale;

            /// <summary>
            /// Creates new vector data
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="rot"></param>
            public RVect(float x, float y, float z, float rot)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.rot = rot;
                this.scale = 1;
            }

            /// <summary>
            /// Creates new vector data
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="rot"></param>
            /// <param name="scale"></param>
            public RVect(float x, float y, float z, float rot, float scale)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.rot = rot;
                this.scale = scale;
            }

            /// <summary>
            /// String representation
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("[{0},{1},{2}:{4}]", x, y, z, rot);
            }
        }

        /// <summary>
        /// Image items
        /// </summary>
        protected class ImageWrapper : ICloneable
        {
            /// <summary>
            /// Current Image
            /// </summary>
            public OImage Image;
            /// <summary>
            /// Width of item
            /// </summary>
            public float Width;
            /// <summary>
            /// Height of item
            /// </summary>
            public float Height;
            /// <summary>
            /// Current placement
            /// </summary>
            public RVect Current;
            /// <summary>
            /// Target placement
            /// </summary>
            public RVect AnimEnd;

            /// <summary>
            /// Creates a new imageWrapper item
            /// </summary>
            public ImageWrapper()
            {
            }
            /// <summary>
            /// Creates a new imageWrapper item
            /// </summary>
            /// <param name="image"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            public ImageWrapper(OImage image, float width, float height)
            {
                this.Image = image;
                this.Width = width;
                this.Height = height;
            }

            /// <summary>
            /// Clones this item
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Enable reflections
        /// </summary>
        public bool ReflectionsEnabled
        {
            get
            {
                return this._ReflectionsEnabled;
            }
            set
            {
                if (this._ReflectionsEnabled != value)
                {
                    this._ReflectionsEnabled = value;
                }
            }
        }
        private bool _ReflectionsEnabled = true;        

        /// <summary>
        /// Sets or gets the speed of the animation 
        /// </summary>
        public float Animation_Step
        {
            get
            {
                return this._Animation_Step;
            }
            set
            {
                if (this._Animation_Step != value)
                {
                    this._Animation_Step = value;
                }
            }
        }
        float _Animation_Step = 0.8f;

        /// <summary>
        /// Sets or gets the speedup of the animation
        /// </summary>
        public float Animation_Speedup
        {
            get
            {
                return this._Animation_Speedup;
            }
            set
            {
                if (this._Animation_Speedup != value)
                {
                    this._Animation_Speedup = value;
                }
            }
        }
        float _Animation_Speedup = 1.5f;

        /// <summary>
        /// Sets or gets the maximum increment size of the animation
        /// </summary>
        public float Animation_Step_Max
        {
            get
            {
                return this._Animation_Step_Max;
            }
            set
            {
                if (this._Animation_Step_Max != value)
                {
                    this._Animation_Step_Max = value;
                }
            }
        }
        float _Animation_Step_Max = 0.1f;

        /// <summary>
        /// Sets or gets the distance of view
        /// </summary>
        public float Animation_FadeOutDistance
        {
            get
            {
                return this._Animation_FadeOutDistance;
            }
            set
            {
                if (this._Animation_FadeOutDistance != value)
                {
                    this._Animation_FadeOutDistance = value;
                }
            }
        }
        float _Animation_FadeOutDistance = 1.75f;

        /// <summary>
        /// Sets or gets the rotation step of the animation
        /// </summary>
        public float Animation_PreRotation
        {
            get
            {
                return this._Animation_PreRotation;
            }
            set
            {
                if (this._Animation_PreRotation != value)
                {
                    this._Animation_PreRotation = value;
                }
            }
        }
        float _Animation_PreRotation = 1.5f;

        /// <summary>
        /// Sets or gets the light strength while rotating the whole control
        /// </summary>
        public float ViewRotate_LightStrenght
        {
            get
            {
                return this._ViewRotate_LightStrenght;
            }
            set
            {
                if (this._ViewRotate_LightStrenght != value)
                {
                    this._ViewRotate_LightStrenght = value;
                }
            }
        }
        float _ViewRotate_LightStrenght = 2.5f;

        //sets the speed of the rotation 
        /// <summary>
        /// Sets or gets the speed of the controls forward rotation 
        /// </summary>
        public float ViewRotate_AnglePercentageIncrement
        {
            get
            {
                return this._ViewRotate_AnglePercentageIncrement;
            }
            set
            {
                if (this._ViewRotate_AnglePercentageIncrement != value)
                {
                    this._ViewRotate_AnglePercentageIncrement = value;
                }
            }
        }
        float _ViewRotate_AnglePercentageIncrement = 0.02f;

        /// <summary>
        /// Sets or gets the speed of the controls reverse rotation 
        /// </summary>
        public float ViewRotate_ReverseSpeed
        {
            get
            {
                return this._ViewRotate_ReverseSpeed;
            }
            set
            {
                if (this._ViewRotate_ReverseSpeed != value)
                {
                    this._ViewRotate_ReverseSpeed = value;
                }
            }
        }
        float _ViewRotate_ReverseSpeed = 0.04f;

        /// <summary>
        /// Maximum allowed view rotation while speeding up in percentage of "ViewRotate_Angle"
        /// </summary>
        public float ViewRotate_AnglePercentageMax
        {
            get
            {
                return this._ViewRotate_AnglePercentageMax;
            }
            set
            {
                if (this._ViewRotate_AnglePercentageMax != value)
                {
                    this._ViewRotate_AnglePercentageMax = value;
                }
            }
        }
        float _ViewRotate_AnglePercentageMax = 0.25f;

        /// <summary>
        /// Maximum allowed view rotation while jumping to a index in percentage of "ViewRotate_Angle"
        /// </summary>
        public float ViewRotate_AnglePercentageMax_JumpTo
        {
            get
            {
                return this._ViewRotate_AnglePercentageMax_JumpTo;
            }
            set
            {
                if (this._ViewRotate_AnglePercentageMax_JumpTo != value)
                {
                    this._ViewRotate_AnglePercentageMax_JumpTo = value;
                }
            }
        }
        float _ViewRotate_AnglePercentageMax_JumpTo = 0.1f;

        /// <summary>
        /// Sets or gets the maximum angle to rotate the view with
        /// </summary>
        public float ViewRotate_Angle
        {
            get
            {
                return this._ViewRotate_Angle;
            }
            set
            {
                if (this._ViewRotate_Angle != value)
                {
                    this._ViewRotate_Angle = value;
                }
            }
        }
        float _ViewRotate_Angle = 0;

        /// <summary>
        /// Sets or gets the currently selected index (the centered image)
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return this._SelectedIndex;
            }
            set
            {
                if (this._SelectedIndex != value)
                {
                    JumpTo(value);
                }
            }
        }
        int _SelectedIndex = 0;

        /// <summary>
        /// The size of the images
        /// </summary>
        public Size ImageSize
        {
            get
            {
                return this._ImageSize;
            }
            set
            {
                if (this._ImageSize != value)
                {
                    this._ImageSize = value;
                }
            }
        }
        private Size _ImageSize;

        /*** Position Configuration ***/
        //the X Position of the Coverflow 
        float _Control_PlacementX = 0;
        //the Y Position of the Coverflow 
        float _Control_PlacementY = 0;
        //the Z Position of the Coverflow 
        float _Control_PlacementZ = -3;

        /// <summary>
        /// Sets or gets the X rotation of the control
        /// </summary>
        public float Control_RotationX
        {
            get
            {
                return this._Control_RotationX;
            }
            set
            {
                if (this._Control_RotationX != value)
                {
                    this._Control_RotationX = value;
                }
            }
        }
        float _Control_RotationX = -15;

        /// <summary>
        /// Sets or gets the Y rotation of the control
        /// </summary>
        public float Control_RotationY
        {
            get
            {
                return this._Control_RotationY;
            }
            set
            {
                if (this._Control_RotationY != value)
                {
                    this._Control_RotationY = value;
                }
            }
        }
        float _Control_RotationY = 0;

        /// <summary>
        /// Sets or gets the Z rotation of the control
        /// </summary>
        public float Control_RotationZ
        {
            get
            {
                return this._Control_RotationZ;
            }
            set
            {
                if (this._Control_RotationZ != value)
                {
                    this._Control_RotationZ = value;
                }
            }
        }
        float _Control_RotationZ = 0;

        /// <summary>
        /// Sets or gets the rotation of each item
        /// </summary>
        public float Item_Rotation
        {
            get
            {
                return this._Item_Rotation;
            }
            set
            {
                if (this._Item_Rotation != value)
                {
                    this._Item_Rotation = value;
                }
            }
        }
        float _Item_Rotation = -50;

        /// <summary>
        /// Sets or gets the distance between each item (value is in the range 0 - 1)
        /// </summary>
        public float Item_Distance
        {
            get
            {
                return this._Item_Distance;
            }
            set
            {
                if (this._Item_Distance != value)
                {
                    this._Item_Distance = value;
                }
            }
        }
        float _Item_Distance = 0.18f;

        /// <summary>
        /// Sets or gets the distance between the items and the centered item (value is in the range 0 - 1)
        /// </summary>
        public float Item_DistanceToCenter
        {
            get
            {
                return this._Item_DistanceToCenter;
            }
            set
            {
                if (this._Item_DistanceToCenter != value)
                {
                    this._Item_DistanceToCenter = value;
                }
            }
        }
        float _Item_DistanceToCenter = 0.75f;

        //sets the pushback amount 
        /// <summary>
        /// Sets or gets the pushback of each item (value is in the range 0 - 1)
        /// </summary>
        public float Item_DistancePushBack
        {
            get
            {
                return this._Item_DistancePushBack;
            }
            set
            {
                if (this._Item_DistancePushBack != value)
                {
                    this._Item_DistancePushBack = value;
                }
            }
        }
        float _Item_DistancePushBack = 0;

        /// <summary>
        /// The x position of the center image
        /// </summary>
        public float Item_CenterImagePlacementX
        {
            get
            {
                return this._Item_Position_Center.x;
            }
            set
            {
                if (this._Item_Position_Center.x != value)
                {
                    this._Item_Position_Center.x = value;
                }
            }
        }

        /// <summary>
        /// The y position of the center image
        /// </summary>
        public float Item_CenterImagePlacementY
        {
            get
            {
                return this._Item_Position_Center.y;
            }
            set
            {
                if (this._Item_Position_Center.y != value)
                {
                    this._Item_Position_Center.y = value;
                }
            }
        }

        /// <summary>
        /// The z position of the center image
        /// </summary>
        public float Item_CenterImagePlacementZ
        {
            get
            {
                return this._Item_Position_Center.z;
            }
            set
            {
                if (this._Item_Position_Center.z != value)
                {
                    this._Item_Position_Center.z = value;
                }
            }
        }

        /// <summary>
        /// The rotation of the center image
        /// </summary>
        public float Item_CenterImageRotation
        {
            get
            {
                return this._Item_Position_Center.rot;
            }
            set
            {
                if (this._Item_Position_Center.rot != value)
                {
                    this._Item_Position_Center.rot = value;
                }
            }
        }

        /// <summary>
        /// The scale of the center image
        /// </summary>
        public float Item_CenterImageScale
        {
            get
            {
                return this._Item_Position_Center.scale;
            }
            set
            {
                if (this._Item_Position_Center.scale != value)
                {
                    this._Item_Position_Center.scale = value;
                }
            }
        }


        //defines the position of the centered cover 
        RVect _Item_Position_Center = new RVect(0, 0, 1, 0, 1.3f);

        /// <summary>
        /// Sets or gets the offset of the control in OM dimension X
        /// </summary>
        public float Control_PlacementOffsetX
        {
            get
            {
                return this._Control_PlacementOffsetX;
            }
            set
            {
                if (this._Control_PlacementOffsetX != value)
                {
                    this._Control_PlacementOffsetX = value;
                }
            }
        }
        float _Control_PlacementOffsetX = 0;

        /// <summary>
        /// Sets or gets the offset of the control in OM dimension Y
        /// </summary>
        public float Control_PlacementOffsetY
        {
            get
            {
                return this._Control_PlacementOffsetY;
            }
            set
            {
                if (this._Control_PlacementOffsetY != value)
                {
                    this._Control_PlacementOffsetY = value;
                }
            }
        }
        float _Control_PlacementOffsetY = 0;

        /// <summary>
        /// Sets or gets the items top shading (value is in the range 0 - 1)
        /// </summary>
        public float Item_ShadingTop
        {
            get
            {
                return this._Item_ShadingTop;
            }
            set
            {
                if (this._Item_ShadingTop != value)
                {
                    this._Item_ShadingTop = value;
                }
            }
        }
        float _Item_ShadingTop = 1;

        /// <summary>
        /// Sets or gets the items bottom shading (value is in the range 0 - 1)
        /// </summary>
        public float Item_ShadingBottom
        {
            get
            {
                return this._Item_ShadingBottom;
            }
            set
            {
                if (this._Item_ShadingBottom != value)
                {
                    this._Item_ShadingBottom = value;
                }
            }
        }
        float _Item_ShadingBottom = 0.02f;

        /// <summary>
        /// Sets or gets the items top reflection strength (value is in the range 0 - 1)
        /// </summary>
        public float Item_ReflectionStrengthUp
        {
            get
            {
                return this._Item_ReflectionStrengthUp;
            }
            set
            {
                if (this._Item_ReflectionStrengthUp != value)
                {
                    this._Item_ReflectionStrengthUp = value;
                }
            }
        }
        float _Item_ReflectionStrengthUp = 0;

        /// <summary>
        /// Sets or gets the items bottom reflection strength (value is in the range 0 - 1)
        /// </summary>
        public float Item_ReflectionStrengthBottom
        {
            get
            {
                return this._Item_ReflectionStrengthBottom;
            }
            set
            {
                if (this._Item_ReflectionStrengthBottom != value)
                {
                    this._Item_ReflectionStrengthBottom = value;
                }
            }
        }
        float _Item_ReflectionStrengthBottom = 0.45f;

        /*** System info ***/
        float _View_rotate = 0;
        bool _View_rotate_active = false;
        float _Step_Backup = 0;

        /// <summary>
        /// Clip the controls rendering to the size of the control
        /// </summary>
        public bool ClipRendering
        {
            get
            {
                return this._ClipRendering;
            }
            set
            {
                if (this._ClipRendering != value)
                {
                    this._ClipRendering = value;
                }
            }
        }
        private bool _ClipRendering = true;

        #endregion

        public OImage overlay = null;

        protected List<ImageWrapper> _Images = new List<ImageWrapper>();
        private Timer tmrUpdate = null;
        int cleanUpDelay = 0;

        #region Animation and placement

        private void CalcPos(ref ImageWrapper cf, int pos)
        {
	        if(pos == 0)
            {
                cf.Current = _Item_Position_Center;
	        }
            else
            {
		        if(pos > 0)
                {
                    cf.Current.x = (_Item_DistanceToCenter) + (_Item_Distance * pos);
                    cf.Current.y = 0;
                    cf.Current.z = _Item_DistancePushBack * pos * -1;
                    cf.Current.rot = _Item_Rotation;
                    cf.Current.scale = 1;
		        }
                else
                {
                    cf.Current.x = (_Item_DistanceToCenter) * -1 + (_Item_Distance * pos);
                    cf.Current.y = 0;
                    cf.Current.z = _Item_DistancePushBack * pos;
                    cf.Current.rot = _Item_Rotation * -1;
                    cf.Current.scale = 1;
                }
	        }
        }

        private void CalcRV(ref RVect rv, int pos)
        {
            ImageWrapper Dummy = new ImageWrapper(); 
            CalcPos(ref Dummy, pos);

            rv.x = Dummy.Current.x;
            rv.y = Dummy.Current.y;
            rv.z = Dummy.Current.z;
            rv.rot = Dummy.Current.rot;
            rv.scale = Dummy.Current.scale;
        }

        private bool Animate(ref RVect current, RVect to)
        {
            bool animate = false;

            // Perform animation? 
            if (System.Math.Abs(to.x - current.x) > 0.001)
                animate = true;
            if (System.Math.Abs(to.y - current.y) > 0.001)
                animate = true;
            if (System.Math.Abs(to.z - current.z) > 0.001)
                animate = true;

	        //calculate and apply positions
            current.x = current.x + (to.x - current.x) * _Animation_Step;
            current.y = current.y + (to.y - current.y) * _Animation_Step;
            current.z = current.z + (to.z - current.z) * _Animation_Step;

            current.scale = current.scale + (to.scale - current.scale) * _Animation_Step;

            if (System.Math.Abs(to.rot - current.rot) > 0.0001)
            {
                animate = true;
                current.rot = current.rot + (to.rot - current.rot) * (_Animation_Step * _Animation_PreRotation);                
	        }

            return animate;
        }

        private void CleanupAnimation()
        {
            _Animation_Step = _Step_Backup;
            _View_rotate_active = false;
        }

        private int _updateCountIdle = 0;
        private int _SelectedIndexStored = -1;
        private void Update()
        {
            lock (_Images)
            {
                bool animationActive = false;

                for (int i = _Images.Count - 1; i >= 0; i--)
                {
                    CalcRV(ref _Images[i].AnimEnd, i - _SelectedIndex);
                    if (Animate(ref _Images[i].Current, _Images[i].AnimEnd))
                        animationActive = true;
                }

                if (!animationActive && _View_rotate_active)
                    cleanUpDelay++;
                else
                    cleanUpDelay = 0;

                if (cleanUpDelay > 1)
                    CleanupAnimation();

                //slowly reset view angle
                if (!_View_rotate_active)
                {
                    _View_rotate += (0 - _View_rotate) * _ViewRotate_ReverseSpeed;
                    if (System.Math.Abs(_View_rotate) < 0.0001)
                        _View_rotate = 0;

                    if (_View_rotate != 0)
                        animationActive = true;
                }

                if (animationActive)
                    _updateCountIdle = 0;

                if (!animationActive && _updateCountIdle < 10)
                {
                    _updateCountIdle++;
                    animationActive = true;
                }

                // Selected index changed?
                if (_SelectedIndexStored != _SelectedIndex)
                {
                    // Override to update text field
                    this.Text = ExtractLabelText();

                    _SelectedIndexStored = _SelectedIndex;
                }

                if (animationActive | _View_rotate_active)
                {
                    Refresh();
                    if (tmrUpdate != null)
                        tmrUpdate.Enabled = true;
                }
                else
                {   // Stop update timer
                    if (tmrUpdate != null)
                        tmrUpdate.Enabled = false;
                }
            }
        }

        private void Animation_Control(bool start)
        {
            if (start)
            {
                if (tmrUpdate != null)
                    tmrUpdate.Enabled = true;
            }
            else
            {
                if (tmrUpdate != null)
                    tmrUpdate.Enabled = false;
            }
        }

        void tmrUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            tmrUpdate.Enabled = false;
            Update();
        }

        /// <summary>
        /// Updates the text displayed in the label field
        /// </summary>
        /// <returns></returns>
        internal virtual string ExtractLabelText()
        {
            return this._text;
        }

        #endregion

        #region List item control

        public void Insert(OImage image)
        {
            Insert(image, _ImageSize.Width, _ImageSize.Height, null);
        }
        public void Insert(OImage image, int index)
        {
            Insert(image, _ImageSize.Width, _ImageSize.Height, index);
        }

        public void Insert(OImage image, float width, float height)
        {
            Insert(image, width, height, null);
        }

        public void Insert(OImage image, float width, float height, int? index)
        {
            // Insert new item
            ImageWrapper img = new ImageWrapper(image, width, height);
            CalcRV(ref img.Current, _Images.Count-1);
            img.Current.x += 1;
            img.Current.rot = 90;

            if (!index.HasValue)
            {
                _Images.Add(img);
            }
            else
            {
                _Images.Insert(index.Value, img);
                //if (index.Value < _Images.Count)
                //{
                //    _Images[index.Value].Image = image;
                //    _Images[index.Value].Width = width;
                //    _Images[index.Value].Height = height;
                //}
            }
            Update();
        }

        public void Clear()
        {
            _Images.Clear();
            Update();
        }

        public void RemoveAt(int index)
        {
            if (index >= _Images.Count || index < 0)
                return;

            _Images.RemoveAt(index);
            Update();
        }

        public void MoveLeft()
        {
            MoveLeft(null);
        }
        public void MoveLeft(int? index)
        {
            if (_SelectedIndex > 0)
            {
                if (index.HasValue)
                {
                    // Calculate rotation based on distance to move
                    _View_rotate = _ViewRotate_AnglePercentageIncrement * System.Math.Abs(_SelectedIndex - index.Value) * -1;
                    // Limit view rotation
                    if (_View_rotate < -_ViewRotate_AnglePercentageMax_JumpTo) _View_rotate = -_ViewRotate_AnglePercentageMax_JumpTo;

                    _SelectedIndex = index.Value;
                }
                else
                {
                    _SelectedIndex--;
                }

                // Limit selected item
                if (_SelectedIndex < 0)
                    _SelectedIndex = 0;

                // Increase animation speed
                _Animation_Step *= _Animation_Speedup;

                // Limit animation speed
                if (_Animation_Step > _Animation_Step_Max) _Animation_Step = _Animation_Step_Max;

                // Incrementally rotate view
                if (_View_rotate_active) _View_rotate -= _ViewRotate_AnglePercentageIncrement;

                // Limit view rotation
                if (_View_rotate < -_ViewRotate_AnglePercentageMax) _View_rotate = -_ViewRotate_AnglePercentageMax;

                // Enable view rotation
                _View_rotate_active = true;
            }
            Animation_Control(true);
        }

        public void MoveRight()
        {
            MoveRight(null);
        }
        public void MoveRight(int? index)
        {
            if (_SelectedIndex < _Images.Count)
            {
                if (index.HasValue)
                {
                    // Calculate rotation based on distance to move
                    _View_rotate = _ViewRotate_AnglePercentageIncrement * System.Math.Abs(_SelectedIndex - index.Value);
                    // Limit view rotation
                    if (_View_rotate > _ViewRotate_AnglePercentageMax_JumpTo) _View_rotate = _ViewRotate_AnglePercentageMax_JumpTo;
                    _SelectedIndex = index.Value;
                }
                else
                {
                    _SelectedIndex++;
                }

                // Limit selected item
                if (_SelectedIndex >= _Images.Count)
                    _SelectedIndex = _Images.Count - 1;

                // Increase animation speed
                _Animation_Step *= _Animation_Speedup;

                // Limit animation speed
                if (_Animation_Step > _Animation_Step_Max) _Animation_Step = _Animation_Step_Max;

                // Incrementally rotate view
                if (_View_rotate_active) _View_rotate += _ViewRotate_AnglePercentageIncrement;

                // Limit view rotation
                if (_View_rotate > _ViewRotate_AnglePercentageMax) _View_rotate = _ViewRotate_AnglePercentageMax;

                // Enable view rotation
                _View_rotate_active = true;
            }
            Animation_Control(true);
        }

        public void JumpTo(int index)
        {
            // Limit index
            if (index < 0) index = 0;
            if (index > _Images.Count-1) index = _Images.Count-1;
            
            // Select direction to move
            if (index > _SelectedIndex)
                MoveRight(index);
            else if (index < _SelectedIndex)
                MoveLeft(index);           
        }

        #endregion

        #region Rendering

        private void Draw(Graphics.Graphics g)
        {
            try
            {
                int CS = _SelectedIndex;
                int count;
                float visibleCount = 40;

                // Translate base point from center to upper left corner of screen
                GL.Translate(-500, -300, 0);

                // Translate graphics to correct placement
                GL.Translate(this.left + (this.width / 2), this.top + (this.height / 2), 0);

                //// Scale graphics to correct size
                //GL.Scale(this.Region.Height / 2, this.Region.Height / 2, 1);

                //// Ensure graphics are centered vertically
                //GL.Translate(0, -(this.height * 0.15), 0);

                //Draw right Covers
                for (count = _Images.Count - 1; count >= 0; count--)
                {
                    if (count > CS)
                    {
                        if (_Images[count].Current.x < (visibleCount * _Item_Distance))
                            DrawCover(g, _Images[count], false);
                    }
                }

                //Draw left Covers
                for (count = 0; count < _Images.Count; count++)
                {
                    if (count < CS)
                    {
                        if (_Images[count].Current.x > (visibleCount * -_Item_Distance))
                            DrawCover(g, _Images[count], false);
                    }
                }

                //Draw Center Cover
                if (CS >= 0 && CS < _Images.Count)
                    DrawCover(g, _Images[CS], true);
            }
            catch
            {
                // Mask errors here
            }
        }

        private void DrawCover(Graphics.Graphics g, ImageWrapper cf, bool enableOverlay)
        {
	        // Error handling
            if (cf.Image == null)
                return;
            
            float w = cf.Width;
	        float h = cf.Height;

	        //fadeout 
            float opacity = 1 - 1 / (_Animation_FadeOutDistance + _ViewRotate_LightStrenght * System.Math.Abs(_View_rotate)) * System.Math.Abs(0 - cf.Current.x);

            //if (System.Math.Abs(opacity) < 0.2f)
            //if (opacity <= 0.0f)
            //    return;

            // Set opacity according to controls values
            //opacity = opacity * _RenderingValue_Alpha;

            // load texture
            uint texture = g.LoadTexture(ref cf.Image);
            if (texture == 0)
                return;

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToBorder);

            double width2 = w / 2.0;
            double height2 = h / 2.0;

            // Save current matrix
            GL.PushMatrix();

            //// Translate base point from center to upper left corner of screen
            //GL.Translate(-500, -300, 0);

            //// Translate graphics to correct placement
            //GL.Translate(this.left + (this.width / 2), this.top + (this.height / 2), 0);

            // Scale cover
            GL.Scale(cf.Current.scale, cf.Current.scale, 1);

            // Offset control
            GL.Translate(_Control_PlacementOffsetX, _Control_PlacementOffsetY, 0);
            
            // Scale graphics to correct dimensions
            GL.Scale(w, h, 1);

            // Place item
            GL.Translate(_Control_PlacementX, _Control_PlacementY, _Control_PlacementZ);
            GL.Rotate(_Control_RotationX, 1, 0, 0);
            GL.Rotate(_View_rotate * _ViewRotate_Angle + _Control_RotationY, 0, 1, 0);
            GL.Rotate(_Control_RotationZ, 0, 0, 1);
            GL.Translate(cf.Current.x, cf.Current.y, cf.Current.z);
            GL.Rotate(cf.Current.rot, 0, 1, 0);

            // Bind texture
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            //calculate shading
            float LShading = ((_Item_Rotation != 0) ? ((cf.Current.rot < 0) ? 1 - 1 / _Item_Rotation * cf.Current.rot : 1) : 1);
            float RShading = ((_Item_Rotation != 0) ? ((cf.Current.rot > 0) ? 1 - 1 / (_Item_Rotation * -1) * cf.Current.rot : 1) : 1);
            float LUP = _Item_ShadingTop + (1 - _Item_ShadingTop) * LShading;
            float LDOWN = _Item_ShadingBottom + (1 - _Item_ShadingBottom) * LShading;
            float RUP = _Item_ShadingTop + (1 - _Item_ShadingTop) * RShading;
            float RDOWN = _Item_ShadingBottom + (1 - _Item_ShadingBottom) * RShading;

            // Normalize rendering output since it's scaled to the correct size instead
            h = w = 1;

            //DrawCover
            GL.Begin(BeginMode.Quads);
            GL.Color4(LDOWN * opacity, LDOWN * opacity, LDOWN * opacity, _RenderingValue_Alpha * (opacity * 2));
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(w / 2 * -1, -0.5, 0);
            GL.Color4(RDOWN * opacity, RDOWN * opacity, RDOWN * opacity, _RenderingValue_Alpha * (opacity * 2));
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(w / 2, -0.5, 0);
            GL.Color4(RUP * opacity, RUP * opacity, RUP * opacity, _RenderingValue_Alpha * (opacity * 2));
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(w / 2, -0.5 + h, 0);
            GL.Color4(LUP * opacity, LUP * opacity, LUP * opacity, _RenderingValue_Alpha * (opacity * 2));
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(w / 2 * -1, -0.5 + h, 0);
            GL.End();

            //Draw reflection
            if (_ReflectionsEnabled)
            {
                GL.Begin(BeginMode.Quads);
                GL.Color4(opacity * _Item_ReflectionStrengthBottom, opacity * _Item_ReflectionStrengthBottom, opacity * _Item_ReflectionStrengthBottom, opacity * _Item_ReflectionStrengthBottom * _RenderingValue_Alpha);
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex3(w / 2 * -1, 0.5, 0);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex3(w / 2, 0.5, 0);
                GL.Color4(LUP * opacity * _Item_ReflectionStrengthUp, LUP * opacity * _Item_ReflectionStrengthUp, LUP * opacity * _Item_ReflectionStrengthUp, opacity * _Item_ReflectionStrengthUp * _RenderingValue_Alpha);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(w / 2, 0.5 + h, 0);
                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex3(w / 2 * -1, 0.5 + h, 0);
                GL.End();
            }

            // Draw overlay
            if (enableOverlay && overlay != null)
            {
                texture = g.LoadTexture(ref overlay);
                if (texture != 0)
                {
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    GL.Begin(BeginMode.Quads);
                    GL.Color4(LDOWN * opacity, LDOWN * opacity, LDOWN * opacity, _RenderingValue_Alpha);
                    GL.TexCoord2(0.0f, 0.0f);
                    GL.Vertex3(w / 2 * -1, -0.5, 0);
                    GL.Color4(RDOWN * opacity, RDOWN * opacity, RDOWN * opacity, _RenderingValue_Alpha);
                    GL.TexCoord2(1.0f, 0.0f);
                    GL.Vertex3(w / 2, -0.5, 0);
                    GL.Color4(RUP * opacity, RUP * opacity, RUP * opacity, _RenderingValue_Alpha);
                    GL.TexCoord2(1.0f, 1.0f);
                    GL.Vertex3(w / 2, -0.5 + h, 0);
                    GL.Color4(LUP * opacity, LUP * opacity, LUP * opacity, _RenderingValue_Alpha);
                    GL.TexCoord2(0.0f, 1.0f);
                    GL.Vertex3(w / 2 * -1, -0.5 + h, 0);
                    GL.End();
                }
            }

            GL.Disable(EnableCap.Texture2D);

            // Restore matrix
            GL.PopMatrix();
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            // Save current matrix
            GL.PushMatrix();

            if (tmrUpdate == null)
            {
                tmrUpdate = new Timer(25);
                tmrUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrUpdate_Elapsed);
                Update();
            }

            base.RenderBegin(g, e);

            // Render controls background
            base.Render_Background(g, e);

            if (_ClipRendering)
                g.SetClipFast(this.left, this.top, this.width, this.height); 

            Draw(g);

            if (_ClipRendering)
                g.ResetClip();

            // Restore matrix
            GL.PopMatrix();

            // Render controls foreground (text label)
            base.Render_Foreground(g, e, _text);

            base.RenderFinish(g, e);

        }

        #endregion

        #region Mouse Throw

        Timer _tmrThrowHandler = null;
        Point _accDistance;
        bool _ThrowActive = false;
        void IThrow.MouseThrow(int screen, Point StartLocation, Point TotalDistance, Point RelativeDistance, PointF CursorSpeed)
        {
            _accDistance += RelativeDistance;
            if (System.Math.Abs(_accDistance.X) > 50)
            {
                _ThrowActive = true;
                if (RelativeDistance.X > 0)
                    SelectedIndex--;
                else if (RelativeDistance.X < 0)
                    SelectedIndex++;
                _accDistance.X = _accDistance.Y = 0;
            }
        }

        void IThrow.MouseThrowStart(int screen, Point StartLocation, PointF CursorSpeed, PointF scaleFactors, ref bool Cancel)
        {
            //_ThrowActive = true;
            if (_tmrThrowHandler != null)
                _tmrThrowHandler.Enabled = false;
        }

        void IThrow.MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation, PointF CursorSpeed)
        {
            _ThrowActive = false;
            
            if (_tmrThrowHandler == null)
            {
                _tmrThrowHandler = new Timer(1);
                _tmrThrowHandler.Elapsed += new System.Timers.ElapsedEventHandler(tmrThrowHandler_Elapsed);
                _tmrThrowHandler.Screen = screen;
            }

            if (System.Math.Abs(CursorSpeed.X) <= 1f)
                return;

            //tmrThrowHandler.Tag = new PointF(TotalDistance.X * System.Math.Abs(CursorSpeed.X * 2), TotalDistance.Y * System.Math.Abs(CursorSpeed.Y * 2));

            PointF existingThrowSpeed = new PointF();
            if (_tmrThrowHandler.Tag is PointF)
                existingThrowSpeed = (PointF)_tmrThrowHandler.Tag;

            //tmrThrowHandler.Tag = new PointF(existingThrowSpeed.X + (TotalDistance.X * System.Math.Abs(CursorSpeed.X)), existingThrowSpeed.Y + (TotalDistance.Y * System.Math.Abs(CursorSpeed.Y)));
            _tmrThrowHandler.Tag = new PointF(existingThrowSpeed.X + (2 * (1+CursorSpeed.X)), existingThrowSpeed.Y + (2 * (1+CursorSpeed.Y)));
            _tmrThrowHandler.Interval = 1;
            _tmrThrowHandler.Enabled = true;

            //SelectedIndex = (int)((_SelectedIndex + 1) * -CursorSpeed.X * (-TotalDistance.X / 100f));

        }

        void tmrThrowHandler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _tmrThrowHandler.Enabled = false;

            PointF throwSpeed = (PointF)_tmrThrowHandler.Tag;
            if (throwSpeed.X > 0)
                SelectedIndex--;
            else if (throwSpeed.X < 0)
                SelectedIndex++;
            else
                return;

            throwSpeed.X *= 0.8f;

            if (System.Math.Abs(throwSpeed.X) > 0)
            {
                _tmrThrowHandler.Interval = 250 / System.Math.Abs(throwSpeed.X);
                if (_tmrThrowHandler.Interval < 350)
                    _tmrThrowHandler.Enabled = true;
            }

            _tmrThrowHandler.Tag = throwSpeed;
        }

        #endregion

        #region Mouse click interface

        void IClickableAdvanced.clickMe(int screen, Input.MouseButtonEventArgs e)
        {
            //if (!IsPointClickable(e.X, e.Y))
            //    return;

            // Cancel if throw is active
            if (_ThrowActive)
                return;

            switch (GetClickHorizontalSection(base.GetLocalControlPoint(e.Location)))
            {
                case 1: // Left side
                    MoveLeft();
                    break;
                case 2: // Center
                    if (OnClick != null)
                        OnClick(this, screen);
                    break;
                case 3: // Right side
                    MoveRight();
                    break;
                default:
                    break;
            }
            Refresh();
        }

        void IClickableAdvanced.longClickMe(int screen, Input.MouseButtonEventArgs e)
        {
            // Cancel if throw is active
            if (_ThrowActive)
                return;

            switch (GetClickHorizontalSection(base.GetLocalControlPoint(e.Location)))
            {
                case 1: // Left side
                    //MoveLeft();
                    break;
                case 2: // Center
                    if (OnLongClick != null)
                        OnLongClick(this, screen);
                    break;
                case 3: // Right side
                    //MoveRight();
                    break;
                default:
                    break;
            }

            Refresh();
        }

        void IClickableAdvanced.holdClickMe(int screen, Input.MouseButtonEventArgs e)
        {
            // Cancel if throw is active
            if (_ThrowActive)
                return;

            switch (GetClickHorizontalSection(base.GetLocalControlPoint(e.Location)))
            {
                case 1: // Left side
                    //MoveLeft();
                    break;
                case 2: // Center
                    if (OnHoldClick != null)
                        OnHoldClick(this, screen);
                    break;
                case 3: // Right side
                    //MoveRight();
                    break;
                default:
                    break;
            }

            Refresh();
        }

        /// <summary>
        /// Occurs when the control is clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Occurs when the control is held
        /// </summary>
        public event userInteraction OnLongClick;
        /// <summary>
        /// Occurs when the control is long clicked
        /// </summary>
        public event userInteraction OnHoldClick;

        private bool IsPointClickable(int x, int y)
        {
            // Return true if click is inside the center item's area
            if (SelectedIndex >= 0)
            {
                if ((x >= (this.Region.Center.X - (_ImageSize.Width / 2))) && (x <= (this.Region.Center.X + (_ImageSize.Width / 2))) &&
                    (y >= (this.Region.Center.Y - (_ImageSize.Height / 2))) && (y <= (this.Region.Center.Y + (_ImageSize.Height / 2))))
                    return true;
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Returns the horizontal region the point is located in (1 = left, 2 = center, 3 = right)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private int GetClickHorizontalSection(Point p)
        {
            int area = this.Region.Width / 3;
            if (p.X <= this.Region.Left + area)
                return 1;
            else if (p.X >= this.Region.Left + area + area)
                return 3;
            else
                return 2;
        }

        #endregion

        #region ICloneable Members

        public override object Clone(OMPanel parent)
        {
            OMImageFlow newObject = (OMImageFlow)this.MemberwiseClone();
            newObject.parent = parent;
            newObject._Images = new List<ImageWrapper>();
            foreach (ImageWrapper iW in _Images)
            {
                newObject._Images.Add((ImageWrapper)iW.Clone());
            }
            return newObject;
        }

        #endregion

    }
}
