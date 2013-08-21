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
using System.Collections.Generic;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions.Graphics;
using System;
using OpenMobile.Input;
using OpenMobile.Math;
using OpenMobile.Graphics.OpenGL;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A OMControl that supports sub controls
    /// </summary>
    [System.Serializable]
    public class OMContainer : OMControlGraphicsBase, IContainer2, IHighlightable, IThrow, IKey
    {
        /// <summary>
        /// The currently loaded controls
        /// </summary>
        protected List<ControlGroup> _Controls = new List<ControlGroup>();
        Rectangle oldRegion = new Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
        Rectangle ScrollRegion = new Rectangle();
        protected bool _ThrowRun = true;
        SmoothAnimator Animation = new SmoothAnimator();

        private bool _NoOffsetOnNextRender = false;

        /// <summary>
        /// Scrollpoints is an array of points that the controls will attach to
        /// <para>This can be used to position the controls while scrolling</para>
        /// <para>NB! The coordinates are relative to it's parent</para>
        /// </summary>
        public List<Point> ScrollPoints = null;
        public ScrollDirections MainScrollDirection = ScrollDirections.Both;

        /// <summary>
        /// Autosize modes
        /// </summary>
        public enum AutoSizeModes
        {
            /// <summary>
            /// Autosizeing is not activated
            /// </summary>
            NoAutoSize,
            /// <summary>
            /// Container will grow with controls
            /// </summary>
            AutoSize
        }
        /// <summary>
        /// Sets the autosize mode for the container 
        /// <para>NB! The size is only adjusted when controls are added or removed, changing a control won't affect the size of the container</para>
        /// </summary>
        public AutoSizeModes AutoSizeMode
        {
            get
            {
                return this._AutoSizeMode;
            }
            set
            {
                if (this._AutoSizeMode != value)
                {
                    this._AutoSizeMode = value;
                }
            }
        }
        private AutoSizeModes _AutoSizeMode;

        private bool _AdjustControlPlacementAtFirstRender = true;

        /// <summary>
        /// The default direction to place controls in
        /// </summary>
        public ControlDirections DefaultControlDirection
        {
            get
            {
                return this._DefaultControlDirection;
            }
            set
            {
                if (this._DefaultControlDirection != value)
                {
                    this._DefaultControlDirection = value;
                }
            }
        }
        private ControlDirections _DefaultControlDirection = ControlDirections.None;        

        private int ControlCountOld = 0;

        protected List<int> _RenderOrder = null;        
        protected List<int> _RenderOrder_ObjectsInView = null;
        protected List<int> _RenderOrder_ObjectsOutsideView = null;
        /// <summary>
        /// Scroll directions
        /// </summary>
        public enum ScrollDirections
        {
            /// <summary>
            /// Both directions are active
            /// </summary>
            Both,
            /// <summary>
            /// Direction X
            /// </summary>
            X,
            /// <summary>
            /// Direction Y
            /// </summary>
            Y
        }

        /// <summary>
        /// Area the local controls covers
        /// </summary>
        private Rectangle ControlsArea = new Rectangle();

        /// <summary>
        /// The current rendering order of the objects in view
        /// </summary>
        public List<int> RenderOrder
        {
            get
            {
                return _RenderOrder_ObjectsInView;
            }
            set
            {
                _RenderOrder_ObjectsInView = value;
            }
        }

        #region Events


        #region OnControlAdded 

        /// <summary>
        /// Event that's raised after a controlgroup has been added to the container
        /// </summary>
        public event userInteraction OnControlAdded;
        private void Raise_OnControlAdded()
        {
            // Cancel event if no parent is present
            if (this.parent == null) 
                return;
            if (OnControlAdded != null)
                OnControlAdded(this, this.parent.ActiveScreen);
        }

        #endregion

        #region OnControlRemoved

        /// <summary>
        /// Event that's raised after a controlgroup has been removed from the container
        /// </summary>
        public event userInteraction OnControlRemoved;
        private void Raise_OnControlRemoved()
        {
            // Cancel event if no parent is present
            if (this.parent == null)
                return;
            if (OnControlRemoved != null)
                OnControlRemoved(this, this.parent.ActiveScreen);
        }

        #endregion

        #endregion

        #region ScrollBar properties

        private bool _ScrollBar_Vertical_Enabled = true;
        private bool _ScrollBar_Horizontal_Enabled = true;
        private bool _ScrollBar_Vertical_Visible = false;
        private bool _ScrollBar_Horizontal_Visible = false;
        private bool _ScrollBar_Vertical_ScrollInProgress = false;
        private bool _ScrollBar_Horizontal_ScrollInProgress = false;
        private int _ScrollBar_ClearanceToEdge = 3;
        private int _ScrollBar_Thickness = 6;
        private Color _ScrollBar_ColorNormal = Color.FromArgb(70, Color.White);
        private Color _ScrollBar_ColorHighlighted = Color.FromArgb(180, Color.White);

        /// <summary>
        /// Enable the vertical scrollbar
        /// </summary>
        public bool ScrollBar_Vertical_Enabled
        {
            get
            {
                return _ScrollBar_Vertical_Enabled;
            }
            set
            {
                _ScrollBar_Vertical_Enabled = value;
                Refresh();
            }
        }
        /// <summary>
        /// Enable the horizontal scrollbar
        /// </summary>
        public bool ScrollBar_Horizontal_Enabled
        {
            get
            {
                return _ScrollBar_Horizontal_Enabled;
            }
            set
            {
                _ScrollBar_Horizontal_Enabled = value;
                Refresh();
            }
        }

        /// <summary>
        /// The clearance for the scrollbar from the edge of the control
        /// </summary>
        public int ScrollBar_ClearanceToEdge
        {
            get
            {
                return _ScrollBar_ClearanceToEdge;
            }
            set
            {
                _ScrollBar_ClearanceToEdge = value;
                Refresh();
            }
        }

        /// <summary>
        /// The thickness of the scrollbar
        /// </summary>
        public int ScrollBar_Thickness
        {
            get
            {
                return _ScrollBar_Thickness;
            }
            set
            {
                _ScrollBar_Thickness = value;
                Refresh();
            }
        }

        /// <summary>
        /// The color of the scrollbar when rendered in the normal state
        /// </summary>
        public Color ScrollBar_ColorNormal
        {
            get
            {
                return _ScrollBar_ColorNormal;
            }
            set
            {
                _ScrollBar_ColorNormal = value;
                Refresh();
            }
        }

        /// <summary>
        /// The color of the scrollbar when rendered in the highlighted state
        /// </summary>
        public Color ScrollBar_ColorHighlighted
        {
            get
            {
                return _ScrollBar_ColorHighlighted;
            }
            set
            {
                _ScrollBar_ColorHighlighted = value;
                Refresh();
            }
        }

        #endregion

        #region Background

        private imageItem image;
        //private byte transparency = 100;
        ///// <summary>
        ///// Opacity (0-100%)
        ///// </summary>
        //public byte Transparency
        //{
        //    get
        //    {
        //        return transparency;
        //    }
        //    set
        //    {
        //        if (transparency == value)
        //            return;
        //        transparency = value;
        //        opacity = (byte)(255 * (transparency / 100F));
        //        raiseUpdate(false);
        //    }
        //}

        private Color _BackgroundColor = Color.Transparent;

        /// <summary>
        /// The background control for this control
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                if (value == _BackgroundColor)
                    return;

                _BackgroundColor = value;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// The image to be rendered
        /// </summary>
        public imageItem Image
        {
            get
            {
                return image;
            }
            set
            {
                if (image == value)
                    return;

                image = value;
                raiseUpdate(false);
            }
        }

        #endregion

        /// <summary>
        /// 3D control data
        /// </summary>
        public _3D_Control _3D_CameraData
        {
            get
            {
                return this.__3D_CameraData;
            }
            set
            {
                this.__3D_CameraData = value;
            }
        }
        private _3D_Control __3D_CameraData;        

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
        
        #region Local renders

        /// <summary>
        /// Initialize local renders
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_Init(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            // Limit rendering to this controls region
            g.SetClip(Region);
        }

        /// <summary>
        /// Render local background
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_Background(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            #region Draw background

            // Draw solid background (if any)
            if (_BackgroundColor != Color.Transparent)
                g.FillRectangle(new Brush(Color.FromArgb((int)(this.GetAlphaValue255(_BackgroundColor.A)), _BackgroundColor)), new Rectangle(left + 1, top + 1, width - 2, height - 2));

            // Draw any shapes (if active)
            DrawShape(g, e);

            // Draw image (if any)
            if (image.image != null)
                lock (image.image)
                    g.DrawImage(image.image, left, top, width, height, _RenderingValue_Alpha, eAngle.Normal);

            #endregion
        }

        /// <summary>
        /// Render local controls
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_Controls(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            // Find region of contained controls
            ControlsArea = GetControlsArea();

            //// Autosize control
            //bool moved = false;
            //if (ControlCountOld != _Controls.Count)
            //    moved = AutoSizeContainer(g, ControlsArea);
            //ControlCountOld = _Controls.Count;

            // Add containers opacity to the contained controls
            float Alpha = e.Alpha;
            e.Alpha = this.GetAlphaValue1(e.Alpha);

            // Move controls with this control
            if (!_NoOffsetOnNextRender)
            {
                if (oldRegion.Left == int.MinValue && oldRegion.Top == int.MinValue)
                {
                    if (_AdjustControlPlacementAtFirstRender)
                        e.Offset = Region - ControlsArea;
                }
                else
                    e.Offset = Region - oldRegion;
            }
            _NoOffsetOnNextRender = false;
            oldRegion = Region;

            // Filter out changes to width and height from offset data
            e.Offset.Width = 0;
            e.Offset.Height = 0;

            // Pass offset data along with renderingparameters to move contained controls
            e.Offset += ScrollRegion;

            #region Limit offset so we don't move the controls outside the bounds

            // Horizontal offset
            if (ScrollRegion.X != 0)
            {
                if (e.Offset.X > 0)
                {
                    if (System.Math.Abs(e.Offset.X) > System.Math.Abs(Region.Left - ControlsArea.Left))
                    {   // X offset is outside of bounds, limit value
                        e.Offset.X = System.Math.Abs(Region.Left - ControlsArea.Left);
                    }
                }
                else if (e.Offset.X < 0)
                {
                    if (System.Math.Abs(e.Offset.X) > System.Math.Abs(Region.Right - ControlsArea.Right))
                    {   // X offset is outside of bounds, limit value
                        e.Offset.X = -System.Math.Abs(Region.Right - ControlsArea.Right);
                    }
                }
            }

            // Vertical offset
            if (ScrollRegion.Y != 0)
            {
                if (e.Offset.Y > 0)
                {
                    if (System.Math.Abs(e.Offset.Y) > System.Math.Abs(Region.Top - ControlsArea.Top))
                    {   // Y offset is outside of bounds, limit value
                        e.Offset.Y = System.Math.Abs(Region.Top - ControlsArea.Top);
                    }
                    //Console.WriteLine(String.Format("+ {3} / {0} - Reg.Top:{1} Cont.Top:{2})", System.Math.Abs(Region.Top - ControlsArea.Top), Region.Top, ControlsArea.Top, e.Offset.Y));
                }
                else if (e.Offset.Y < 0)
                {
                    if (System.Math.Abs(e.Offset.Y) > System.Math.Abs(Region.Bottom - ControlsArea.Bottom))
                    {   // Y offset is outside of bounds, limit value
                        e.Offset.Y = -System.Math.Abs(Region.Bottom - ControlsArea.Bottom);
                    }
                    //Console.WriteLine(String.Format("- {3} / {0} - Reg.Btn:{1} Cont.Btn:{2})", System.Math.Abs(Region.Bottom - ControlsArea.Bottom), Region.Bottom, ControlsArea.Bottom, e.Offset.Y));
                }
            }

            #endregion

            // Exctract render objects
            if (_RenderOrder != null && _Controls != null)
            {
                if ((!e.Offset.IsEmpty) || (_RenderOrder_ObjectsInView == null || ((_RenderOrder_ObjectsInView.Count + _RenderOrder_ObjectsOutsideView.Count) != _RenderOrder.Count)))
                {
                    lock (_RenderOrder)
                    {
                        lock (_Controls)
                        {
                            RenderOrder_GetObjects(e.Offset);
                            _RenderOrder_ObjectsInView.Sort(); // Reset to default order 
                            RenderOrder_Set(_Controls, e.Offset, ref _RenderOrder_ObjectsInView);
                        }
                    }
                }

                // Render controls within view
                if (_RenderOrder_ObjectsInView != null)
                {
                    for (int i = 0; i < _RenderOrder_ObjectsInView.Count; i++)
                    {
                        int RenderIndex = _RenderOrder_ObjectsInView[i];
                        if (!ScrollRegion.IsEmpty)
                        {
                            SetOffsetToScrollPoints(_Controls[RenderIndex], ref e.Offset);
                            ModifyControlAndOffsetData(_Controls[RenderIndex], ref e.Offset);
                        }
                        for (int i2 = 0; i2 < _Controls[RenderIndex].Count; i2++)
                        {
                            if (_Controls[RenderIndex][i2].IsControlRenderable(true))
                                _Controls[RenderIndex][i2].Render(g, e);
                        }
                    }
                }
            }

            // Render controls outside the view (to ensure we scroll properly)
            if (_RenderOrder_ObjectsOutsideView != null)
            {
                for (int i = 0; i < _RenderOrder_ObjectsOutsideView.Count; i++)
                {
                    int RenderIndex = _RenderOrder_ObjectsOutsideView[i];
                    if (!ScrollRegion.IsEmpty)
                    {
                        SetOffsetToScrollPoints(_Controls[RenderIndex], ref e.Offset);
                        ModifyControlAndOffsetData(_Controls[RenderIndex], ref e.Offset);
                    }
                    for (int i2 = 0; i2 < _Controls[RenderIndex].Count; i2++)
                    {
                        _Controls[RenderIndex][i2].RenderEmpty(g, e);
                    }
                }
            }



            //// Render controls (controls are only rendered if they are within this controls region)
            //for (int i = 0; i < _Controls.Count; i++)
            //{
            //    RenderIndex = _RenderOrder[i];
            //    if (!ScrollRegion.IsEmpty)
            //    {
            //        SetOffsetToScrollPoints(_Controls[RenderIndex], ref e.Offset);
            //        ModifyControlAndOffsetData(_Controls[RenderIndex], ref e.Offset);
            //    }
            //    if (this.Region.ToSystemRectangle().IntersectsWith(_Controls[RenderIndex].Region.ToSystemRectangle()))
            //    {
            //        if (_Controls[RenderIndex].IsControlRenderable(true))
            //            _Controls[RenderIndex].Render(g, e);
            //    }
            //    else
            //    {   // Prosess rendering parameters even if it's not within the rendering range (to ensure we scroll properly)
            //        _Controls[RenderIndex].RenderEmpty(g, e);
            //    }
            //}

            // Reset scroll data so we don't accumulate offset values
            ScrollRegion = new Rectangle();

            // Reset offset data
            e.Offset = new Rectangle();

            // Reset alpha data
            e.Alpha = Alpha;

            // Reset clip
            g.ResetClip();
        }

        internal virtual void ModifyControlAndOffsetData(ControlGroup controlGroup, ref Rectangle Offset)
        {
            // No action
        }

        internal virtual void SetOffsetToScrollPoints(ControlGroup controlGroup, ref Rectangle Offset)
        {
            if (ScrollPoints == null)
                return;

            Rectangle CalcEndRegion = controlGroup.Region;
            CalcEndRegion.Translate(Offset);

            Point TargetPoint = new Point();

            try
            {
                // Find matching offset data
                switch (MainScrollDirection)
                {
                    case ScrollDirections.X:
                        {
                            // Ignore method if target position is outside this controls regions (saves execution time)
                            if ((CalcEndRegion.Right < this.Region.Left) | (CalcEndRegion.Left > this.Region.Right))
                                return;

                            // Convert values from absolute to relative
                            CalcEndRegion.Left -= this.Region.Left;
                            CalcEndRegion.Top -= this.Region.Top;

                            // Find direct match in scroll points
                            TargetPoint = ScrollPoints.Find(x => x.X == CalcEndRegion.Center.X);

                            // TODO: Search for closest match
                            //if (TargetPoint == null)
                            //{
                            //    TargetPoint = ScrollPoints.Find(x => x.X == CalcEndRegion.Center.X);
                            //}
                        }
                        break;
                    case ScrollDirections.Y:
                        {
                            // Ignore method if target position is outside this controls regions (saves execution time)
                            if ((CalcEndRegion.Bottom < this.Region.Top) | (CalcEndRegion.Top > this.Region.Bottom))
                                return;

                            // Convert values from absolute to relative
                            CalcEndRegion.Left -= this.Region.Left;
                            CalcEndRegion.Top -= this.Region.Top;

                            // Find direct match in scroll points
                            TargetPoint = ScrollPoints.Find(x => x.Y == CalcEndRegion.Center.Y);

                            // TODO: Search for closest match
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
            {   // Set offset to none if an error occurs
                TargetPoint = CalcEndRegion.Center;
            }

            // Calculate new offset based on where it should be
            Offset.X += (TargetPoint.X - CalcEndRegion.Center.X);
            Offset.Y += (TargetPoint.Y - CalcEndRegion.Center.Y);
        }

        /// <summary>
        /// Render local scrollbars
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_Scrollbars(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            #region Render scrollbar

            // Find region of contained controls
            ControlsArea = GetControlsArea();

            if (MainScrollDirection == ScrollDirections.Both || MainScrollDirection == ScrollDirections.X)
            {
                _ScrollBar_Horizontal_Visible = ((ControlsArea.Left < Region.Left || ControlsArea.Right > Region.Right) && _ScrollBar_Horizontal_Enabled && _Controls.Count > 0);
                if (_ScrollBar_Horizontal_Visible)
                {   // Horisontal scrollbar

                    Rectangle scrollbarRect = this.Region;

                    // Set scrollbar color
                    Brush ScrollBarBrush;
                    if (_ScrollBar_Horizontal_ScrollInProgress | this.mode == eModeType.Highlighted)
                        ScrollBarBrush = new Brush(_ScrollBar_ColorHighlighted);
                    else
                        ScrollBarBrush = new Brush(_ScrollBar_ColorNormal);

                    // Calculate scrollbar length
                    int ScrollBarSize = (int)(((float)Region.Width) * (((float)Region.Width) / ((float)ControlsArea.Width)));
                    // Set minimum size of scrollbar
                    if (ScrollBarSize <= 0)
                        ScrollBarSize = 5;

                    // Calculate scrollbar location
                    float MaxTravelScrollBar = System.Math.Abs(Region.Width - ScrollBarSize);
                    float MaxTravelControls = System.Math.Abs(ControlsArea.Width - Region.Width);
                    float CurrentTravel = System.Math.Abs(Region.Left - ControlsArea.Left);
                    float TravelFactor = CurrentTravel / MaxTravelControls;
                    int ScrollBarLocation = Region.Left + (int)(MaxTravelScrollBar * TravelFactor);

                    // Render scrollbar
                    //g.FillRectangle(ScrollBarBrush, ScrollBarLocation, Region.Bottom - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarSize, _ScrollBar_Thickness);
                    g.FillRoundRectangle(ScrollBarBrush, ScrollBarLocation, Region.Bottom - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarSize, _ScrollBar_Thickness, _ScrollBar_Thickness / 2);

                    // Release resources
                    ScrollBarBrush.Dispose();
                }
            }

            if (MainScrollDirection == ScrollDirections.Both || MainScrollDirection == ScrollDirections.Y)
            {
                _ScrollBar_Vertical_Visible = ((ControlsArea.Bottom > Region.Bottom || ControlsArea.Top < Region.Top) && _ScrollBar_Vertical_Enabled && _Controls.Count > 0);
                if (_ScrollBar_Vertical_Visible)
                {   // Vertical scrollbar

                    // Set scrollbar color
                    Brush ScrollBarBrush;
                    if (_ScrollBar_Vertical_ScrollInProgress | this.mode == eModeType.Highlighted)
                        ScrollBarBrush = new Brush(_ScrollBar_ColorHighlighted);
                    else
                        ScrollBarBrush = new Brush(_ScrollBar_ColorNormal);

                    // Calculate scrollbar length
                    int ScrollBarSize = (int)(((float)Region.Height) * (((float)Region.Height) / ((float)ControlsArea.Height)));
                    // Set minimum size of scrollbar
                    if (ScrollBarSize <= 0)
                        ScrollBarSize = 5;

                    // Calculate scrollbar location
                    float MaxTravelScrollBar = System.Math.Abs(Region.Height - ScrollBarSize);
                    float MaxTravelControls = System.Math.Abs(ControlsArea.Height - Region.Height);
                    float CurrentTravel = System.Math.Abs(Region.Top - ControlsArea.Top);
                    float TravelFactor = CurrentTravel / MaxTravelControls;
                    int ScrollBarLocation = Region.Top + (int)(MaxTravelScrollBar * TravelFactor);

                    // Render scrollbar
                    //g.FillRectangle(ScrollBarBrush, Region.Right - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarLocation, _ScrollBar_Thickness, ScrollBarSize);
                    g.FillRoundRectangle(ScrollBarBrush, Region.Right - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarLocation, _ScrollBar_Thickness, ScrollBarSize, _ScrollBar_Thickness / 2);

                    // Release resources
                    ScrollBarBrush.Dispose();
                }
            }

            #endregion
        }

        /// <summary>
        /// Render local softedges
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_SoftEdges(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            #region Render soft edges

            // Use soft edges?
            if (SoftEdges != FadingEdge.GraphicSides.None)
            {
                if (_BackgroundColor != Color.Transparent)
                {
                    Size SoftEdgeSize = new Size(Width + 2, Height + 2);
                    if (imgSoftEdge == null || imgSoftEdge.Width != SoftEdgeSize.Width || imgSoftEdge.Height != SoftEdgeSize.Height)
                    {   // Generate image
                        SoftEdgeData.Sides = SoftEdges;
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

        /// <summary>
        /// Render local softedges
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void RenderLocal_ScrollPoints(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            if (ScrollPoints == null)
                return;

            if (this._SkinDebug)
            {
                List<Point> points = new List<Point>(ScrollPoints);
                // Convert relative coordinates to absolute
                for (int i = 0; i < points.Count; i++)
                    points[i].Translate(this.left, this.top);

                using (Pen p = new Pen(Color.Yellow, 2))
                    g.DrawLine(p, points.ToArray());
            }
        }

        #endregion

        /// <summary>
        /// Renders this control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            // Save current 3D data
            g._3D_ModelView_Push();

            // Zoom 
            if (__3D_CameraData.CameraZoom != 0)
            {
                g.Scale(__3D_CameraData.CameraZoom, __3D_CameraData.CameraZoom, 1);
            }

            // Offset camera
            if (__3D_CameraData.CameraOffset != Vector3d.Zero)
            {
                g.Translate(__3D_CameraData.CameraOffset.X, __3D_CameraData.CameraOffset.Y, __3D_CameraData.CameraOffset.Z);
            }

            // Rotate camera
            if (__3D_CameraData.CameraRotation != Vector3d.Zero)
            {
                g.Rotate(__3D_CameraData.CameraRotation);
            }

            // Rotate control?
            if (__3D_CameraData.ControlRotation != Vector3d.Zero)
            {
                // Calculate center point in absolute screen coordinates
                Point center = g.GetScreenAbsPoint(Region.Center);

                if (__3D_CameraData.ControlRotationPoint != Vector3d.Zero)
                {
                    // Move control to center of screen (to rotate around the center of the control)
                    g.Translate(__3D_CameraData.ControlRotationPoint.X, __3D_CameraData.ControlRotationPoint.Y);

                    // Apply rotation
                    g.Rotate(__3D_CameraData.ControlRotation);

                    // Move control back to original placement
                    g.Translate(-__3D_CameraData.ControlRotationPoint.X, -__3D_CameraData.ControlRotationPoint.Y);
                }
                else
                {
                    // Move control to center of screen (to rotate around the center of the control)
                    g.Translate(center.X, center.Y);

                    // Apply rotation
                    g.Rotate(__3D_CameraData.ControlRotation);

                    // Move control back to original placement
                    g.Translate(-center.X, -center.Y);
                }

            }

            base.RenderBegin(g, e);
            try
            {
                RenderLocal_Init(g, e);
                RenderLocal_Background(g, e);
                RenderLocal_Controls(g, e);
                RenderLocal_SoftEdges(g, e);
                RenderLocal_Scrollbars(g, e);
                RenderLocal_ScrollPoints(g, e);
            }
            // We have to catch index errors as we can't lock the control list while rendering since that would block adding controls to the control most of the time
            catch (ArgumentOutOfRangeException)
            {
            }

            base.RenderFinish(g, e);

            // Restore 3D data
            g._3D_ModelView_Pop();
        }

        /// <summary>
        /// Returns the total area covered by the currently loaded controls
        /// </summary>
        /// <returns></returns>
        public Rectangle GetControlsArea()
        {
            if (_Controls.Count == 0)
                return new Rectangle(this.Region.Left, this.Region.Top, 0, 0);

            Rectangle TotalRegion = _Controls[0].Region;
            for (int i = 1; i < _Controls.Count; i++)
                TotalRegion.Union(_Controls[i].Region);
            return TotalRegion;
        }

        private void Control_Configure(ControlGroup controlGroup)
        {
            foreach (OMControl control in controlGroup)
            {
                // Connect controls parent
                control.Parent = this.parent;

                // Connect refresh requestor to control
                control.UpdateThisControl += this.parent.raiseUpdate;
            }
        }
        private void Controls_PlaceAndConfigure()//ControlGroup ControlToAdd, ControlGroup ControlToRemove)
        {
            foreach (ControlGroup cg in _Controls)
            {
                //Control_PlaceAndConfigure(cg, ControlToAdd, ControlToRemove);
                Control_Configure(cg);

                Control_PlaceOnScrollPoints(cg);
            }
        }

        private void Control_PlaceRelative(ControlGroup controlGroup)
        {
            // Change position of this control from relative to this container to absolute on the panel
            foreach (OMControl control in controlGroup)
            {
                control.Left = this.Left + control.Left;
                control.Top = this.Top + control.Top;
            }
        }
        private void Controls_PlaceRelative()
        {
            foreach (ControlGroup cg in _Controls)
                Control_PlaceRelative(cg);
        }

        private void Control_PlaceOnScrollPoints(ControlGroup controlGroup)
        {
            Rectangle Offset = new Rectangle();
            SetOffsetToScrollPoints(controlGroup, ref Offset);
            foreach (OMControl control in controlGroup)
            {
                control.Left += Offset.Left;
                control.Top += Offset.Top;
            }
        }

        protected virtual void RenderOrder_Set(List<ControlGroup> controlGroups, Rectangle Offset, ref List<int> renderOrder)
        {
            // No action
        }

        private void RenderOrder_GetObjects(Rectangle offset)
        {
            // Exctract render objects
            if (_Controls != null && _RenderOrder != null)
            {
                lock (_RenderOrder)
                {
                    lock (_Controls)
                    {
                        // Get controls based on the position they will be in AFTER offset is added
                        _RenderOrder_ObjectsInView = _RenderOrder.FindAll(x => this.Region.ToSystemRectangle().IntersectsWith(_Controls[x].Region.TranslateToNew(offset).ToSystemRectangle()));
                        _RenderOrder_ObjectsOutsideView = _RenderOrder.FindAll(x => !_RenderOrder_ObjectsInView.Contains(x));
                    }
                }
            }
        }

        #region ScrollToControl

        public void ScrollToControl(string name)
        {
            ScrollToControl(name, false, 1.0f);
        }
        public void ScrollToControl(string name, bool animate)
        {
            ScrollToControl(name, animate, 1.0f);
        }
        public void ScrollToControl(OMControl control)
        {
            ScrollToControl(control.Name, false, 1.0f);
        }
        public void ScrollToControl(OMControl control, bool animate)
        {
            ScrollToControl(control.Name, animate, 1.0f);
        }
        /// <summary>
        /// Scrolls to the specified control
        /// </summary>
        /// <param name="control"></param>
        public void ScrollToControl(OMControl control, bool animate, float animationSpeed)
        {
            ScrollToControl(control.Name, animate, animationSpeed);
        }
        /// <summary>
        /// Scrolls to the specified control
        /// </summary>
        /// <param name="name"></param>
        public void ScrollToControl(string name, bool animate, float animationSpeed)
        {
            ControlGroup cg = _Controls.Find(x => x.Contains(name));
            ScrollToControl(cg,animate,animationSpeed);
        }

        public void ScrollToControl(ControlGroup cg, bool animate, float animationSpeed)
        {
            // Cancel any ongoing scrolling
            _ThrowRun = false;

            if (animationSpeed == 0)
                animationSpeed = 1.0f;

            ControlGroup controlFound;
            lock (_Controls)
            {
                controlFound = _Controls.Find(x => x == cg);
            }
            if (controlFound != null)
            {
                if (animate)
                {
                    #region Animate scrolling

                    lock (Animation)
                    {
                        float StepSize = 1;
                        // Calculate distance from current location to control's location
                        Point Distance_Start = (this.Region.Center - controlFound.Region.Center);
                        Point Distance_Current = Distance_Start;

                        // Calculatate animation speed, this is based on the total distance to move but is limited to a max time and a min time
                        float maxTimeMS = 1000F;
                        float minTimeMS = 500F;
                        float TravelDistance = System.Math.Max(System.Math.Abs(Distance_Current.X), System.Math.Abs(Distance_Current.Y));
                        float TravelSpeedMS = System.Math.Abs(TravelDistance) / animationSpeed;
                        if (TravelSpeedMS > maxTimeMS)
                            animationSpeed = TravelSpeedMS / maxTimeMS;
                        if (TravelSpeedMS < minTimeMS)
                            animationSpeed = TravelSpeedMS / minTimeMS;

                        Animation.Speed = 1f * animationSpeed;
                        float AnimationValue = 0;
                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                        {
                            // Cancel animation
                            if (Distance_Current.IsEmpty)
                                return false;

                            // Calculate animation value
                            AnimationValue += AnimationStepF;

                            // Animation step large enough?
                            if (AnimationValue > StepSize)
                            {
                                AnimationStep = (int)AnimationValue;
                                AnimationValue -= AnimationStep;

                                if (Distance_Current.X > 0)
                                {
                                    ScrollRegion.X = AnimationStep;
                                    Distance_Current.X -= AnimationStep;
                                    if (Distance_Current.X < 0)
                                    {
                                        ScrollRegion.X -= Distance_Current.X;
                                        Distance_Current.X = 0;
                                    }
                                }
                                else if (Distance_Current.X < 0)
                                {
                                    ScrollRegion.X = -AnimationStep;
                                    Distance_Current.X += AnimationStep;
                                    if (Distance_Current.X > 0)
                                    {
                                        ScrollRegion.X += Distance_Current.X;
                                        Distance_Current.X = 0;
                                    }
                                }

                                if (Distance_Current.Y > 0)
                                {
                                    ScrollRegion.Y = AnimationStep;
                                    Distance_Current.Y -= AnimationStep;
                                    if (Distance_Current.Y < 0)
                                    {
                                        ScrollRegion.Y -= Distance_Current.Y;
                                        Distance_Current.Y = 0;
                                    }
                                }
                                else if (Distance_Current.Y < 0)
                                {
                                    ScrollRegion.Y = -AnimationStep;
                                    Distance_Current.Y += AnimationStep;
                                    if (Distance_Current.Y > 0)
                                    {
                                        ScrollRegion.Y += Distance_Current.Y;
                                        Distance_Current.Y = 0;
                                    }
                                }
                            }

                            Refresh();

                            //System.Threading.Thread.Sleep(500);
                            return true;
                        });

                    }
                    Refresh();

                    #endregion
                }
                else
                {
                    // Calculate distance from current location to control's location
                    Point Distance = (this.Region.Center - controlFound.Region.Center);
                    ScrollRegion.X = Distance.X;
                    ScrollRegion.Y = Distance.Y;
                    Refresh();
                }
            }

        }

        #endregion

        /// <summary>
        /// Creates a new instance of the control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMContainer(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            // Initialize softedge data
            SoftEdgeData = new FadingEdge.GraphicData();
            SoftEdgeData.Sides = FadingEdge.GraphicSides.None;
        }

        /// <summary>
        /// This controls parent
        /// </summary>
        public override OMPanel Parent
        {
            get
            {
                return base.Parent;
            }
            internal set
            {
                if (value == null)
                    return;
                bool Configure = (base.parent == null);
                base.Parent = value;
                if (value != null && Configure)
                    Controls_PlaceAndConfigure();
            }
        }

        /// <summary>
        /// Gets/Sets the given control
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ControlGroup this[int index]
        {
            get
            {
                return _Controls[index];
            }
            set
            {
                _Controls[index] = value;
            }
        }
        /// <summary>
        /// Gets a control by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControlGroup this[string name]
        {
            get
            {
                return _Controls.Find(p => p.Contains(name));
            }
        }

        /// <summary>
        /// The controlgroups contained in this container
        /// </summary>
        public List<ControlGroup> ControlGroups
        {
            get
            {
                return _Controls;
            }
        }

        #region IContainer2 Members

        // Only the controls that's in view will be returned in this list
        public List<OMControl> Controls
        {
            get 
            {
                // Exit if nothing is visible
                if (_RenderOrder_ObjectsInView == null)
                    return new List<OMControl>();

                lock (_RenderOrder_ObjectsInView)
                {
                    // Return a flatten list of all controls in the controlgroups
                    List<OMControl> controls = new List<OMControl>();
                    for (int i = 0; i < _RenderOrder_ObjectsInView.Count; i++)
                    {
                        if (_RenderOrder_ObjectsInView[i] < _Controls.Count)
                        {
                            for (int i2 = 0; i2 < _Controls[_RenderOrder_ObjectsInView[i]].Count; i2++)
                                controls.Add(_Controls[_RenderOrder_ObjectsInView[i]][i2]);
                        }
                    }
                    return controls;
                }
            }
        }

        /// <summary>
        /// Adds a control with relative placement to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControlRelative(OMControl control)
        {
            return addControl(control, ControlDirections.RelativeToParent);
        }
        /// <summary>
        /// Adds a control with absolute placement to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControlAbsolute(OMControl control)
        {
            return addControl(control, ControlDirections.Absolute);
        }
        
        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControl(OMControl control)
        {
            return addControl(new ControlGroup(control), _DefaultControlDirection);
        }
        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="control"></param>
        /// <param name="direction">The direction to add the new controlgroup</param>
        /// <returns></returns>
        public bool addControl(OMControl control, ControlDirections direction)
        {
            return addControl(new ControlGroup(control), direction);
        }

        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="cg"></param>
        /// <returns></returns>
        public bool addControl(ControlGroup cg)
        {
            return addControl(cg, DefaultControlDirection);
        }

        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool addControl(ControlGroup cg, ControlDirections direction)
        {
            if (cg == null)
                return false;

            // Save reference to direction
            cg.PlacementDirection = direction;

            // Shift all other controls to accomodate the inserted control
            switch (direction)
            {
                case ControlDirections.None:
                    {
                    }
                    break;
                case ControlDirections.Down:
                    #region Down
                    {
                        // Place new control
                        if (_Controls.Count > 0)
                        {
                            cg.X = _Controls[_Controls.Count - 1].Region.Left;
                            cg.Y = _Controls[_Controls.Count - 1].Region.Bottom;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;
                case ControlDirections.Up:
                    #region Up
                    {
                        // Place new control
                        if (_Controls.Count > 0)
                        {
                            cg.X = _Controls[_Controls.Count - 1].Region.Left;
                            cg.Y = _Controls[_Controls.Count - 1].Region.Top - cg.Region.Height;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top - cg.Region.Height;
                        }
                    }
                    #endregion
                    break;
                case ControlDirections.CenterHorizontally:
                case ControlDirections.Right:
                    #region Right
                    {
                        // Place new control
                        if (_Controls.Count > 0)
                        {
                            cg.X = _Controls[_Controls.Count - 1].Region.Right;
                            cg.Y = _Controls[_Controls.Count - 1].Region.Top;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;
                case ControlDirections.Left:
                    #region Left
                    {
                        // Place new control
                        if (_Controls.Count > 0)
                        {
                            cg.X = _Controls[_Controls.Count - 1].Region.Left - cg.Region.Width;
                            cg.Y = _Controls[_Controls.Count - 1].Region.Top;
                        }
                        else
                        {
                            cg.X = this.Region.Right - cg.Region.Width;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;

                case ControlDirections.RelativeToParent:
                    {
                        // Place controls relatively to this control
                        cg.Translate(this.Region);
                    }
                    break;

                default:
                    break;
            }

            // Insert new control
            _Controls.Add(cg);

            // Center controls 
            if (direction == ControlDirections.CenterHorizontally)
            {
                Controls_CenterHorizontally();

                // Disable control placement if we center controls and we haven't rendered yet.
                if (!_ControlRendered)
                    _AdjustControlPlacementAtFirstRender = false;
            }

            Controls_PlaceAndConfigure();

            RenderOrder_Reset();

            AutoSizeContainer();

            base.Refresh();

            Raise_OnControlAdded();
            return true;


            //// Configure direction data
            //cg.PlacementDirection = direction;

            //if (direction == ControlDirections.CenterHorizontally)
            //    _NoOffsetOnNextRender = true;

            //// Add control
            //_Controls.Add(cg);
            //RenderOrder_Reset();

            //// Add this control to the panel
            //if (this.parent != null)
            //    Control_PlaceAndConfigure(cg, null, null);

            //AutoSizeContainer();

            //Raise_OnControlAdded();

            //return true;
        }

        /// <summary>
        /// Adds a control to this container at a specific index
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="Relative">Indicates that the placement of this control is releative to the container</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool addControl(int index, ControlGroup cg, bool relative, ControlDirections direction)
        {
            if (cg == null)
                return false;

            // Save reference to direction
            cg.PlacementDirection = direction;

            // Shift all other controls to accomodate the inserted control
            switch (direction)
            {
                case ControlDirections.None:
                    break;
                case ControlDirections.Down:
                    #region Down
                    {
                        for (int i = index; i < _Controls.Count; i++)
                            _Controls[i].Translate(0, cg.Region.Height);

                        // Place new control
                        if (index > 0)
                        {
                            cg.X = _Controls[index-1].Region.Left;
                            cg.Y = _Controls[index-1].Region.Bottom;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;
                case ControlDirections.Up:
                    #region Up
                    {
                        for (int i = index; i < _Controls.Count; i++)
                            _Controls[i].Translate(0, -cg.Region.Height);

                        // Place new control
                        if (index > 0)
                        {
                            cg.X = _Controls[index-1].Region.Left;
                            cg.Y = _Controls[index-1].Region.Top - cg.Region.Height;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top - cg.Region.Height;
                        }
                    }
                    #endregion
                   break;
                case ControlDirections.CenterHorizontally:
                case ControlDirections.Right:
                    #region Right
                    {
                        for (int i = index; i < _Controls.Count; i++)
                            _Controls[i].Translate(cg.Region.Width, 0);

                        // Place new control
                        if (index > 0)
                        {
                            cg.X = _Controls[index-1].Region.Right;
                            cg.Y = _Controls[index-1].Region.Top;
                        }
                        else
                        {
                            cg.X = this.Region.Left;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;
                case ControlDirections.Left:
                    #region Left
                    {
                        for (int i = index; i < _Controls.Count; i++)
                            _Controls[i].Translate(-cg.Region.Width, 0);

                        // Place new control
                        if (index > 0)
                        {
                            cg.X = _Controls[index - 1].Region.Left - cg.Region.Width;
                            cg.Y = _Controls[index - 1].Region.Top;
                        }
                        else
                        {
                            cg.X = this.Region.Right - cg.Region.Width;
                            cg.Y = this.Region.Top;
                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }

            // Insert new control
            _Controls.Insert(index, cg);

            // Center controls 
            if (direction == ControlDirections.CenterHorizontally)
            {
                Controls_CenterHorizontally();

                // Disable control placement if we center controls and we haven't rendered yet.
                if (!_ControlRendered)
                    _AdjustControlPlacementAtFirstRender = false;
            }

            RenderOrder_Reset();

            AutoSizeContainer();

            base.Refresh();
            
            Raise_OnControlAdded();
            return true;









            //if (cg == null)
            //    return false;

            //// Configure direction data
            //cg.PlacementDirection = direction;

            //if (direction == ControlDirections.CenterHorizontally)
            //    _NoOffsetOnNextRender = true;

            //// Add control
            //_Controls.Insert(index, cg);
            //RenderOrder_Reset();

            //// Add this control to the panel
            //if (this.parent != null)
            //{
            //    Control_PlaceAndConfigure(cg, null, null);

            //    // Move all other controls as well to accomodate the inserted control
            //    for (int i = index+1; i < _Controls.Count; i++)
            //        Control_PlaceAndConfigure(_Controls[i], cg, null);
            //}

            //AutoSizeContainer();

            //Raise_OnControlAdded();
            //return true;
        }

        private void Controls_CenterHorizontally()
        {
            // Get center position of current items
            int CenterMissmatch = this.Region.Center.X - this.GetControlsArea().Center.X;

            // Offset all controls according to missmatch
            foreach (ControlGroup cg in _Controls)
                cg.Translate(CenterMissmatch, 0);
        }

        private void AutoSizeContainer()
        {
            if (_AutoSizeMode == AutoSizeModes.NoAutoSize)
                return;

            lock (this)
            {
                Rectangle ControlsArea = GetControlsArea();

                // Set width and height
                this.Width = ControlsArea.Width;
                this.Height = ControlsArea.Height;
            }
        }

        private void Control_PlaceAndConfigure(ControlGroup cg, ControlGroup ControlToInsert, ControlGroup ControlToRemove)
        {
            // Configure control
            Control_Configure(cg);

            // Place control
            Control_SetPlacement(cg, cg.PlacementDirection, ControlToInsert, ControlToRemove);
                //if (!UseAbsolutePlacement)
                //    cg.PlacementRelative = true;
            //if (cg.PlacementRelative && ControlToRemove == null && !UseAbsolutePlacement)
            //    Control_PlaceRelative(cg);

            // Place control on scroll points
            Control_PlaceOnScrollPoints(cg);

            // Block controls automatic rendering
            foreach (OMControl control in cg)
                control.ManualRendering = true;
        }

        /// <summary>
        /// Removes a controlgroup based on name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveControl(string name)
        {
            bool result = false;

            ControlGroup cg = _Controls.Find(x => x.Contains(name));
            if (cg != null)
                result = RemoveControl(cg);

            return result;

        }

        /// <summary>
        /// Removes a controlgroup based on tag
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveControlByKey(string key)
        {
            bool result = false;

            ControlGroup cg = _Controls.Find(x => x.Key == key);
            if (cg != null)
                result = RemoveControl(cg);

            return result;

        }

        /// <summary>
        /// Removes a controlgroup based on index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveControl(int index)
        {
            bool result = false;

            ControlGroup cg = _Controls[index];
            if (cg != null)
                result = RemoveControl(cg);

            return result;
        }

        /// <summary>
        /// Removes a controlgroup
        /// </summary>
        /// <param name="cg"></param>
        /// <returns></returns>
        public bool RemoveControl(ControlGroup cg)
        {
            // Get index
            int index = _Controls.IndexOf(cg);
            if (index == -1)
                return false;

            // Remove control
            bool result = _Controls.Remove(cg);
            RenderOrder_Reset();

            // Renable controls automatic rendering
            foreach (OMControl control in cg)
                control.ManualRendering = false;

            // Move all other controls to accomodate the removed control
            for (int i = index; i < _Controls.Count; i++)
                Control_PlaceAndConfigure(_Controls[i], null, cg);

            AutoSizeContainer();

            Raise_OnControlRemoved();

            return result;
        }

        public bool ReplaceControl(int index, ControlGroup cg, ControlDirections direction)
        {
            cg.PlacementDirection = direction;
            
            // Get area of control to replace
            Rectangle oldRegion = _Controls[index].Region;
            Rectangle newRegion = cg.Region;

            // Calculate offset and placement for new control
            Point offset = new Point();
            switch (cg.PlacementDirection)
            {
                case ControlDirections.None:
                    break;
                case ControlDirections.Down:
                     {
                        cg.X = oldRegion.X;
                        cg.Y = oldRegion.Y;
                        offset.Y = newRegion.Height - oldRegion.Height;
                    }
                   break;
                case ControlDirections.Up:
                    {
                        offset.Y = -(newRegion.Height - oldRegion.Height);
                        cg.X = oldRegion.X;
                        cg.Y = oldRegion.Y - offset.Y;
                    }
                    break;
                case ControlDirections.RelativeToParent:
                case ControlDirections.Absolute:
                case ControlDirections.CenterHorizontally:
                case ControlDirections.Right:
                    {
                        cg.X = oldRegion.X;
                        cg.Y = oldRegion.Y;
                        offset.X = newRegion.Width - oldRegion.Width;
                    }
                    break;
                case ControlDirections.Left:
                    {
                        offset.X = -(newRegion.Width - oldRegion.Width);
                        cg.X = oldRegion.X - offset.X;
                        cg.Y = oldRegion.Y;
                    }
                    break;
                default:
                    break;
            }

            // Shift controls to support the new size
            if (index < _Controls.Count - 1)
            {
                for (int i = index + 1; i < _Controls.Count; i++)
                    _Controls[i].Translate(offset.X, offset.Y);
            }

            _Controls[index] = cg;

            // Center controls 
            if (cg.PlacementDirection == ControlDirections.CenterHorizontally)
            {
                Controls_CenterHorizontally();

                // Disable control placement if we center controls and we haven't rendered yet.
                if (!_ControlRendered)
                    _AdjustControlPlacementAtFirstRender = false;
            }

            RenderOrder_Reset();

            AutoSizeContainer();

            base.Refresh();

            Raise_OnControlAdded();
            return true;

        }

        protected virtual void Control_SetPlacement(ControlGroup ControlToPlace, ControlDirections direction, ControlGroup ControlToInsert, ControlGroup ControlToRemove)
        {
            // Note about coordinates: ControlToPlace will always be relative, 
            //                         ControlToInsert will always be absolute
            //                         ControlToRemove will always be absolute

            // Note about controls: ControlToAdd is added to _Controls list BEFORE entering this method,
            //                      ControToRemove is removed from _Controls list BEFORE entering this method

            int ReferenceValue;
            int Index_ControlToPlace = _Controls.IndexOf(ControlToPlace);

            // Swap direction if we're removing a control
            if (ControlToRemove != null)
                direction = (ControlDirections)(-((int)direction));

            switch (direction)
            {
                case ControlDirections.None:
                    {
                        // Place controls from relative to absolute
                        foreach (OMControl control in ControlToPlace)
                        {
                            // Place control
                            control.Top = this.Region.Top + control.Top;
                            control.Left = this.Region.Left + control.Left;
                        }
                    }
                    break;
                case ControlDirections.CenterHorizontally:
                    {   // Center all controls (this means that we have to reposition all controls for each control being added or removed)

                        // Place control to add
                        if (ControlToInsert == null && ControlToRemove == null)
                            Control_SetPlacement(ControlToPlace, ControlDirections.Right, ControlToInsert, ControlToRemove);

                        // Get center position of current items
                        int CenterMissmatch = this.Region.Center.X - this.GetControlsArea().Center.X;

                        // Offset all controls according to missmatch
                        foreach (ControlGroup cg in _Controls)
                        {
                            foreach (OMControl control in cg)
                            {   // Place control
                                control.Left += CenterMissmatch;
                            }
                        }
                        
                    }
                    break;
                case ControlDirections.Down:
                    {
                        if (ControlToInsert == null && ControlToRemove == null)
                        {   // A new control is being added; move control relative
                            // Default reference value
                            ReferenceValue = this.Region.Top;

                            // Calculate reference values
                            if (Index_ControlToPlace > 0 && _Controls.Count > 1)
                                // Reference is the previous control in the list
                                ReferenceValue = _Controls[Index_ControlToPlace - 1].Region.Bottom;

                            // Place controls from relative to absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = ReferenceValue + control.Top;
                                control.Left = this.Region.Left + control.Left;
                            }
                        }
                        else if (ControlToRemove != null)
                        {   // An existing control is being removed; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = control.Top + ControlToRemove.Region.Height;
                            }
                        }
                        else if (ControlToInsert != null)
                        {   // An existing control is being inserted; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = control.Top + ControlToInsert.Region.Height;
                            }
                        }
                    }
                    break;
                case ControlDirections.Up:
                    {
                        if (ControlToInsert == null && ControlToRemove == null)
                        {   // A new control is being added; move control relative
                            // Default reference value
                            ReferenceValue = this.Region.Bottom - ControlToPlace.Region.Height;

                            // Calculate reference values
                            if (Index_ControlToPlace > 0 && _Controls.Count > 1)
                                // Reference is the previous control in the list
                                ReferenceValue = _Controls[Index_ControlToPlace - 1].Region.Top - ControlToPlace.Region.Height;

                            // Place controls from relative to absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = ReferenceValue + control.Top;
                                control.Left = this.Region.Left + control.Left;
                            }
                        }
                        else if (ControlToRemove != null)
                        {   // An existing control is being removed; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = control.Top - ControlToRemove.Region.Height;
                            }
                        }
                        else if (ControlToInsert != null)
                        {   // An existing control is being inserted; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Top = control.Top - ControlToInsert.Region.Height;
                            }
                        }
                    }
                    break;
                case ControlDirections.Right:
                    {
                        if (ControlToInsert == null && ControlToRemove == null)
                        {   // A new control is being added; move control relative
                            // Default reference value
                            ReferenceValue = this.Region.Left;

                            // Calculate reference values
                            if (Index_ControlToPlace > 0 && _Controls.Count > 1)
                                // Reference is the previous control in the list
                                ReferenceValue = _Controls[Index_ControlToPlace - 1].Region.Right;

                            // Place controls from relative to absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = ReferenceValue + control.Left;
                                control.Top = this.Region.Top + control.Top;
                            }
                        }
                        else if (ControlToRemove != null)
                        {   // An existing control is being removed; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = control.Left + ControlToRemove.Region.Width;
                            }
                        }
                        else if (ControlToInsert != null)
                        {   // An existing control is being inserted; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = control.Left + ControlToInsert.Region.Width;
                            }
                        }
                    }
                    break;
                case ControlDirections.Left:
                    {
                        if (ControlToInsert == null && ControlToRemove == null)
                        {   // A new control is being added; move control relative
                            // Default reference value
                            ReferenceValue = this.Region.Right - ControlToPlace.Region.Width;

                            // Calculate reference values
                            if (Index_ControlToPlace > 0 && _Controls.Count > 1)
                                // Reference is the previous control in the list
                                ReferenceValue = _Controls[Index_ControlToPlace - 1].Region.Left - ControlToPlace.Region.Width;

                            // Place controls from relative to absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = ReferenceValue + control.Left;
                                control.Top = this.Region.Top + control.Top;
                            }
                        }
                        else if (ControlToRemove != null)
                        {   // An existing control is being removed; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = control.Left - ControlToRemove.Region.Width;
                            }
                        }
                        else if (ControlToInsert != null)
                        {   // An existing control is being inserted; move control absolute
                            foreach (OMControl control in ControlToPlace)
                            {
                                // Place control
                                control.Left = control.Left - ControlToInsert.Region.Width;
                            }
                        }

                    }
                    break;
            }




            
            /*
            Rectangle ControlPreviousArea = new Rectangle();
            // Get placement of current last control (the control we're trying to place has already been added so whe have to skip this one)
            int index = _Controls.IndexOf(ControlToPlace);
            if (ControlToRemove == null)
            {
                if (_Controls.Count > 1)
                {
                    if (index > 0)
                        ControlPreviousArea = _Controls[index - 1].Region;
                }
            }
            else
            {
                if (_Controls.Count >= 1)
                {
                    if (index > 0)
                        ControlPreviousArea = _Controls[index].Region;
                }
            }
                // Try to find index of current item
                //if (index >= 0)
                //{
                //    if (index > 0)
                //    {
                //        ControlPreviousArea = _Controls[index - 1].Region;
                //    }
                //}
                //else
                //{
                //    ControlPreviousArea = _Controls[_Controls.Count - 2].Region;
                //}

                //if (index >= 0)
                //{
                //    switch (direction)
                //    {
                //        case Directions.Down:
                //        case Directions.Right:
                //            {
                //                if (index > 0 && _Controls.Count > 1)
                //                    ControlPreviousArea = _Controls[index - 1].Region;
                //            }
                //            break;
                //        case Directions.Up:
                //        case Directions.Left:
                //            {
                //                if (_Controls.Count > index + 1)
                //                    ControlPreviousArea = _Controls[index + 1].Region;
                //            }
                //            break;
                //    }
                //}
            //}
            
            Rectangle ControlAreaToRemove = new Rectangle();
            if (ControlToRemove != null)
            {
                ControlAreaToRemove = ControlToRemove.Region;
                direction = (Directions)(-((int)direction));
                index = _Controls.IndexOf(ControlToPlace);
                if (index >= 0)
                {
                    //ControlPreviousArea = _Controls[index].Region;
                }
            }

            // Set placement 
            switch (direction)
            {
                case Directions.Up:
                    {
                        if (ControlPreviousArea.IsEmpty)
                            ControlPreviousArea.Top = this.Region.Bottom;
                        foreach (OMControl control in ControlToPlace)
                        {
                            if (!UseAbsolutePlacement)
                                control.Top = (ControlPreviousArea.Top - this.Region.Top) + control.Top;
                            else
                            {
                                if (ControlAreaToRemove.IsEmpty)
                                    control.Top = ControlPreviousArea.Top;
                                else
                                    control.Top = (ControlPreviousArea.Top - this.Region.Top) + control.Top - ControlAreaToRemove.Height;
                            }
                        }
                    }
                    break;
                case Directions.Down:
                    {
                        if (ControlPreviousArea.IsEmpty)
                            ControlPreviousArea.Bottom = this.Region.Top;
                        foreach (OMControl control in ControlToPlace)
                        {
                            if (!UseAbsolutePlacement)
                                control.Top = (ControlPreviousArea.Bottom - this.Region.Top) + control.Top;
                            else
                            {
                                if (ControlAreaToRemove.IsEmpty)
                                    control.Top = ControlPreviousArea.Bottom;
                                else
                                    control.Top = (ControlPreviousArea.Bottom - this.Region.Top) + control.Top - ControlAreaToRemove.Height;
                            }
                        }
                    }
                    break;
                case Directions.Right:
                    {
                        if (ControlPreviousArea.IsEmpty)
                            ControlPreviousArea.Left = this.Region.Left;
                        foreach (OMControl control in ControlToPlace)
                        {
                            if (!UseAbsolutePlacement)
                                control.Left = (ControlPreviousArea.Right - this.Region.Left) + control.Left;
                            else
                            {
                                if (ControlAreaToRemove.IsEmpty)
                                    control.Left = ControlPreviousArea.Right;
                                else
                                    control.Left = (ControlPreviousArea.Right - this.Region.Left) + control.Left - ControlAreaToRemove.Width;
                            }
                        }
                    }
                    break;
                case Directions.Left:
                    {
                        if (ControlPreviousArea.IsEmpty)
                            ControlPreviousArea.Left = this.Region.Right;
                        foreach (OMControl control in ControlToPlace)
                        {
                            if (!UseAbsolutePlacement)
                                control.Left = (ControlPreviousArea.Left - this.Region.Left) - control.Left - control.Width;
                            else
                            {
                                if (ControlAreaToRemove.IsEmpty)
                                    control.Left = ControlPreviousArea.Left - cg.Region.Width;
                                else
                                    control.Left = control.Left - ControlAreaToRemove.Width;
                            }
                        }

                    }
                    break;
                default:
                    break;
            }
            */
        }

        private void RenderOrder_Reset()
        {
            if ((_RenderOrder == null) || (_RenderOrder.Count != _Controls.Count))
                _RenderOrder = new List<int>();
            // Reset render order to default
            _RenderOrder.Clear();
            for (int i = 0; i < _Controls.Count; i++)
                _RenderOrder.Add(i);
        }

        /// <summary>
        /// Removes all controls
        /// </summary>
        public void ClearControls()
        {
            _Controls.Clear();
            RenderOrder_Reset();
            Refresh();
        }

        #endregion

        #region ICloneable Members

        public override object Clone(OMPanel parent)
        {
            OMContainer newObject = (OMContainer)this.MemberwiseClone();
            newObject.parent = parent;
            //OMContainer newObject = (OMContainer)base.Clone();
            newObject._Controls = new List<ControlGroup>();
            foreach (ControlGroup cg in _Controls)
            {
                newObject._Controls.Add(cg.Clone(parent));
                //ControlGroup newCG = new ControlGroup();
                //foreach (OMControl control in cg)
                //    newCG.Add((OMControl)control.Clone());
                //newObject._Controls.Add(newCG);
            }
            return newObject;
        }

        #endregion

        private void Scroll(Point RelativeDistance)
        {
            // Find region of contained controls
            Rectangle ControlsArea = GetControlsArea();

            if (_ScrollBar_Horizontal_Visible)
            {
                // Control motion vs limits
                if (RelativeDistance.X > 0)
                {
                    if (Region.Left > ControlsArea.Left)
                    {
                        _ScrollBar_Horizontal_ScrollInProgress = true;
                        ScrollRegion.Left += RelativeDistance.X;
                    }
                }
                else if (RelativeDistance.X < 0)
                {
                    if (Region.Right < ControlsArea.Right)
                    {
                        _ScrollBar_Horizontal_ScrollInProgress = true;
                        ScrollRegion.Left += RelativeDistance.X;
                    }
                }
            }

            if (_ScrollBar_Vertical_Visible)
            {
                // Control motion vs limits
                if (RelativeDistance.Y > 0)
                {
                    if (Region.Top > ControlsArea.Top)
                    {
                        _ScrollBar_Vertical_ScrollInProgress = true;
                        ScrollRegion.Top += RelativeDistance.Y;
                    }
                }
                else if (RelativeDistance.Y < 0)
                {
                    if (Region.Bottom < ControlsArea.Bottom)
                    {
                        _ScrollBar_Vertical_ScrollInProgress = true;
                        ScrollRegion.Top += RelativeDistance.Y;
                    }
                }
            }
        }

        #region IThrow Members

        public virtual void MouseThrow(int screen, Point StartLocation, Point TotalDistance, Point RelativeDistance, PointF CursorSpeed)
        {
            _ThrowRun = false;
            Scroll(RelativeDistance);
        }

        public virtual void MouseThrowStart(int screen, Point StartLocation, PointF CursorSpeed, PointF scaleFactors, ref bool Cancel)
        {
        }

        public virtual void MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation, PointF CursorSpeed)
        {
            // Continue motion when user end's the throw 
            _ThrowRun = true;
            int LoopSpeedMS = 12;
            PointF DecelerationFactor = new PointF(0.003f, 0.003f);
            PointF ThrowSpeed = new PointF(System.Math.Abs(CursorSpeed.X), System.Math.Abs(CursorSpeed.Y));
            Point ScrollDistance = new Point();
            PointF Deceleration = new PointF(ThrowSpeed.X * DecelerationFactor.X, ThrowSpeed.Y * DecelerationFactor.Y);

            while (_ThrowRun)
            {
                if (!_ThrowRun)
                    break;

                if (ThrowSpeed.X > 0)
                    ThrowSpeed.X -= Deceleration.X;
                else
                    ThrowSpeed.X = 0;

                if (ThrowSpeed.Y > 0)
                    ThrowSpeed.Y -= Deceleration.Y;
                else
                    ThrowSpeed.Y = 0;


                if (ThrowSpeed.X > 0)
                {
                    if (CursorSpeed.X > 0)
                    {
                        ScrollDistance.X = (int)System.Math.Round((ThrowSpeed.X * LoopSpeedMS), 0);
                    }
                    else if (CursorSpeed.X < 0)
                    {
                        ScrollDistance.X = (int)-System.Math.Round((ThrowSpeed.X * LoopSpeedMS), 0);
                    }
                }

                if (ThrowSpeed.Y > 0)
                {
                    if (CursorSpeed.Y > 0)
                    {
                        ScrollDistance.Y = (int)System.Math.Round((ThrowSpeed.Y * LoopSpeedMS), 0);
                    }
                    else if (CursorSpeed.Y < 0)
                    {
                        ScrollDistance.Y = (int)-System.Math.Round((ThrowSpeed.Y * LoopSpeedMS), 0);
                    }
                }

                // End throw
                if (ThrowSpeed.X <= 0 & ThrowSpeed.Y <= 0)
                    _ThrowRun = false;

                if (!_ThrowRun)
                    break;

                Scroll(ScrollDistance);
                Refresh();

                System.Threading.Thread.Sleep(LoopSpeedMS);
            }            
            
            _ScrollBar_Vertical_ScrollInProgress = false;
            _ScrollBar_Horizontal_ScrollInProgress = false;
        }

        #endregion

        #region IKey Members

        public bool KeyDown_BeforeUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            return false;
        }

        public void KeyDown_AfterUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right) || (e.Key == Key.Up) || (e.Key == Key.Down))
            {
                _Controls.ForEach(delegate(ControlGroup cg)
                {
                    if (cg.IsHighlighted())
                        ScrollToControl(cg, false, 1.0f);
                });
            }
        }

        public bool KeyUp_BeforeUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            return false;
        }

        public void KeyUp_AfterUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
        }

        #endregion
    }
}
