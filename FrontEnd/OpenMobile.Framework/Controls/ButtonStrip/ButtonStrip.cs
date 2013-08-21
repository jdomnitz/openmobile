using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A ButtonStrip button
    /// </summary>
    public class Button : ControlGroup, ICloneable
    {
        #region Properties

        /// <summary>
        /// The name of the button
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new button
        /// </summary>
        /// <param name="name"></param>
        public Button(string name)
        {
            this.Name = name;
        }

        #endregion

        #region Static buttons creators

        /// <summary>
        /// Creates a dropdown style button for usage in a OMContainer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_Button_Style1(string name, Size size, int opacity, imageItem icon, string text, userInteraction OnClick, userInteraction OnHoldClick, userInteraction OnLongClick)
        {
            return PreConfigLayout_Button_Style1(name, size, opacity, icon, text, false, OnClick, OnHoldClick, OnLongClick);
        }

        /// <summary>
        /// Creates a dropdown style button for usage in a OMContainer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="dummy"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <returns></returns>
        static private Button PreConfigLayout_Button_Style1(string name, Size size, int opacity, imageItem icon, string text, bool dummy, userInteraction OnClick, userInteraction OnHoldClick, userInteraction OnLongClick)
        {
            Button btn = new Button(name);

            Rectangle BtnSize = new Rectangle(0, 0, size.Width, size.Height);

            // Button function
            OMButton Button_NotifyDropdown_Btn_Settings = new OMButton(String.Format("{0}_{1}", name, "Button"), BtnSize.Left, BtnSize.Top, BtnSize.Width, BtnSize.Height);
            if (!dummy)
            {
                OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
                gd.BackgroundColor1 = Color.Transparent;
                gd.BorderColor = Color.Transparent;
                gd.Width = BtnSize.Width;
                gd.Height = BtnSize.Height;
                gd.CornerRadius = 0;
                gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
                Button_NotifyDropdown_Btn_Settings.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
                gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
                Button_NotifyDropdown_Btn_Settings.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            }
            Button_NotifyDropdown_Btn_Settings.OnClick += OnClick;
            Button_NotifyDropdown_Btn_Settings.OnHoldClick += OnHoldClick;
            Button_NotifyDropdown_Btn_Settings.OnLongClick += OnLongClick;
            //Button_NotifyDropdown_Btn_Settings.SkinDebug = true;
            btn.Add(Button_NotifyDropdown_Btn_Settings);

            // Button icon
            if (icon != null && icon.image != null)
            {
                imageItem BtnIcon = icon;
                Point BtnIconCenter = new Point(BtnIcon.image.Size.Width / 2, BtnIcon.image.Size.Height / 2);
                OMImage Image_NotifyDropdown_Btn_Settings = new OMImage(String.Format("{0}_{1}", name, "Image"), BtnSize.Center.X - BtnIconCenter.X, BtnSize.Top, BtnIcon);
                if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                    Image_NotifyDropdown_Btn_Settings.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                Image_NotifyDropdown_Btn_Settings.Opacity = opacity;
                Image_NotifyDropdown_Btn_Settings.NoUserInteraction = true;
                btn.Add(Image_NotifyDropdown_Btn_Settings);
            }

            // Button label
            OMLabel Label_NotifyDropdown_Btn_Settings = new OMLabel(String.Format("{0}_{1}", name, "Label"), BtnSize.Left, BtnSize.Bottom - 25, BtnSize.Width, 25);
            Label_NotifyDropdown_Btn_Settings.Text = text;
            Label_NotifyDropdown_Btn_Settings.Format = eTextFormat.Normal;
            Label_NotifyDropdown_Btn_Settings.Opacity = opacity;
            Label_NotifyDropdown_Btn_Settings.Font = new Font(Font.Arial, 14);
            Label_NotifyDropdown_Btn_Settings.TextAlignment = Alignment.TopCenter;
            Label_NotifyDropdown_Btn_Settings.NoUserInteraction = true;
            btn.Add(Label_NotifyDropdown_Btn_Settings);

            return btn;
        }
        /// <summary>
        /// Creates an empty button placeholder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_ButtonDummy_Style1(string name, Size size)
        {
            return PreConfigLayout_Button_Style1(name, size, 0, new imageItem(), null, true, null, null, null);
        }

        /// <summary>
        /// Creates a simple icon based button (one icon, no text)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="corners"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_SimpleButton(string name, Size size, int opacity, imageItem icon, GraphicCorners corners, userInteraction OnClick, userInteraction OnHoldClick, userInteraction OnLongClick)
        {
            Button btn = new Button(name);
            
            // Calculate graphics size for icon in button (to ensure it's correctly aligned)
            Size GraphicSize = new Size(size.Width, size.Height);
            if (icon.image != null)
            {
                //GraphicSize = icon.image.Size;
                //if (icon.image.Width > size.Width)
                //    GraphicSize.Width = size.Width;
                //if (icon.image.Height > size.Height)
                //    GraphicSize.Height = size.Height;

                GraphicSize = new Size(icon.image.Width, icon.image.Height);

                float scaleFactor = 1f;
                if (GraphicSize.Width < GraphicSize.Height)
                {   // Width is smallest
                    scaleFactor = (float)GraphicSize.Width / (float)icon.image.Size.Width;
                    GraphicSize.Height = (int)(GraphicSize.Height * scaleFactor);
                }
                else
                {   // Height is smallest
                    scaleFactor = (float)GraphicSize.Height / (float)icon.image.Size.Height;
                    GraphicSize.Width = (int)(GraphicSize.Width * scaleFactor);
                }
            }

            // Button function
            //OMButton Button_Settings = new OMButton(String.Format("{0}_{1}", btn.Name, "Button"), 0, 0, size.Width, size.Height);
            OMButton Button_Settings = OMButton.PreConfigLayout_BasicStyle(String.Format("{0}_{1}", btn.Name, "Button"), -1, 0, size.Width, size.Height, corners);
            //Button_Settings.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            //Button_Settings.GraphicSize = GraphicSize;
            Button_Settings.Opacity = opacity;

            //Button_Settings.FocusImage = new imageItem(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);
            //Button_Settings.DownImage = new imageItem(Color.FromArgb(200, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);

            //OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            //gd.BackgroundColor = Color.Transparent;
            //gd.BorderColor = Color.Transparent;
            //gd.Width = size.Width;
            //gd.Height = size.Height;
            //gd.CornerRadius = 0;
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            //Button_Settings.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            //Button_Settings.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            if (icon.image != null)
            {
                //// Create focus image
                //OImage oImgFocus = (OImage)icon.image.Clone();
                //oImgFocus.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                //Button_Settings.FocusImage = new imageItem(oImgFocus);

                //// Create down image
                //OImage oImgDown = (OImage)icon.image.Clone();
                //oImgDown.SetAlpha(0.5F); // Slightly darken the image
                //oImgDown.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                //Button_Settings.DownImage = new imageItem(oImgDown);

                // Create icon image
                OImage oImgOverlay = (OImage)icon.image.Clone();
                if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                    oImgOverlay.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                Button_Settings.OverlayImage = new imageItem(oImgOverlay);
                Button_Settings.OverlayImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
                Button_Settings.GraphicSize = GraphicSize;

            }

            // Map actions
            Button_Settings.OnClick += OnClick;
            Button_Settings.OnHoldClick += OnHoldClick;
            Button_Settings.OnLongClick += OnLongClick;
            //Button_Settings.SkinDebug = true;

            btn.Add(Button_Settings);

            return btn;
        }

        /// <summary>
        /// Creates a simple button with no background (glowin highlight)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="corners"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_SimpleButton_Style2(string name, Size size, int opacity, imageItem icon, GraphicCorners corners, userInteraction OnClick, userInteraction OnHoldClick, userInteraction OnLongClick)
        {
            Button btn = new Button(name);

            // Calculate graphics size for icon in button (to ensure it's correctly aligned)
            Size GraphicSize = new Size(size.Width, size.Height);
            if (icon.image != null)
            {
                GraphicSize = new Size(icon.image.Width, icon.image.Height);

                float scaleFactor = 1f;
                if (GraphicSize.Width < GraphicSize.Height)
                {   // Width is smallest
                    scaleFactor = (float)GraphicSize.Width / (float)icon.image.Size.Width;
                    GraphicSize.Height = (int)(GraphicSize.Height * scaleFactor);
                }
                else
                {   // Height is smallest
                    scaleFactor = (float)GraphicSize.Height / (float)icon.image.Size.Height;
                    GraphicSize.Width = (int)(GraphicSize.Width * scaleFactor);
                }
            }

            // Button function
            OMButton Button_Settings = new OMButton(String.Format("{0}_{1}", btn.Name, "Button"), 0, 0, size.Width, size.Height);
            Button_Settings.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            Button_Settings.GraphicSize = GraphicSize;
            Button_Settings.Opacity = opacity;

            //Button_Settings.FocusImage = new imageItem(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);
            //Button_Settings.DownImage = new imageItem(Color.FromArgb(200, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);

            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            gd.BackgroundColor1 = Color.Transparent;
            gd.BorderColor = Color.Transparent;
            gd.Width = size.Width;
            gd.Height = size.Height;
            gd.CornerRadius = 0;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            Button_Settings.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            Button_Settings.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            if (icon.image != null)
            {
                // Create focus image
                OImage oImgFocus = (OImage)icon.image.Clone();
                oImgFocus.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                Button_Settings.FocusImage = new imageItem(oImgFocus);

                // Create down image
                OImage oImgDown = (OImage)icon.image.Clone();
                oImgDown.SetAlpha(0.5F); // Slightly darken the image
                oImgDown.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                Button_Settings.DownImage = new imageItem(oImgDown);

                // Create icon image
                OImage oImgOverlay = (OImage)icon.image.Clone();
                if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                    oImgOverlay.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                Button_Settings.OverlayImage = new imageItem(oImgOverlay);
                Button_Settings.OverlayImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
                Button_Settings.GraphicSize = GraphicSize;
            }

            // Map actions
            Button_Settings.OnClick += OnClick;
            Button_Settings.OnHoldClick += OnHoldClick;
            Button_Settings.OnLongClick += OnLongClick;
            //Button_Settings.SkinDebug = true;

            btn.Add(Button_Settings);

            return btn;
        }

        /// <summary>
        /// Creates a button for usage in a menubar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <param name="visibleBackground"></param>
        /// <param name="visibleBorder"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_MenuBarStyle(string name, Size size, int opacity, imageItem icon, userInteraction OnClick = null, userInteraction OnHoldClick = null, userInteraction OnLongClick = null, bool visibleBackground = true, bool visibleBorder = true)
        {
            return PreConfigLayout_MenuBarStyle(name, size, opacity, icon, (object)OnClick, (object)OnHoldClick, (object)OnLongClick, visibleBackground, visibleBorder);
        }

        /// <summary>
        /// Creates a button for usage in a menubar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="OnClick">Command to execute</param>
        /// <param name="OnHoldClick">Command to execute</param>
        /// <param name="OnLongClick">Command to execute</param>
        /// <param name="visibleBackground"></param>
        /// <param name="visibleBorder"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_MenuBarStyle(string name, Size size, int opacity, imageItem icon, string OnClick = null, string OnHoldClick = null, string OnLongClick = null, bool visibleBackground = true, bool visibleBorder = true)
        {
            return PreConfigLayout_MenuBarStyle(name, size, opacity, icon, (object)OnClick, (object)OnHoldClick, (object)OnLongClick, visibleBackground, visibleBorder);
        }


        /// <summary>
        /// Creates a button for usage in a menubar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <param name="visibleBackground"></param>
        /// <param name="visibleBorder"></param>
        /// <returns></returns>
        static public Button PreConfigLayout_MenuBarStyle(string name, Size size, int opacity, imageItem icon, object OnClick = null, object OnHoldClick = null, object OnLongClick = null, bool visibleBackground = true, bool visibleBorder = true)
        {
            Button btn = new Button(name);

            // Calculate graphics size for icon in button (to ensure it's correctly aligned)
            Size GraphicSize = new Size(size.Width, size.Height);
            if (icon.image != null)
            {
                GraphicSize = new Size(icon.image.Width, icon.image.Height);

                float scaleFactor = 1f;
                if (GraphicSize.Width < GraphicSize.Height)
                {   // Width is smallest
                    scaleFactor = (float)GraphicSize.Width / (float)icon.image.Size.Width;
                    GraphicSize.Height = (int)(GraphicSize.Height * scaleFactor);
                }
                else
                {   // Height is smallest
                    scaleFactor = (float)GraphicSize.Height / (float)icon.image.Size.Height;
                    GraphicSize.Width = (int)(GraphicSize.Width * scaleFactor);
                }
            }

            // Button function

            OMButton Button_Settings = null;
            if (visibleBackground)
                Button_Settings = OMButton.PreConfigLayout_BasicStyle(String.Format("{0}_{1}", btn.Name, "Button"), -1, 0, size.Width, size.Height, GraphicCorners.None);
            else
            {
                Color? borderColor = null;
                if (!visibleBorder)
                    borderColor = Color.Transparent;
                Button_Settings = OMButton.PreConfigLayout_BasicStyle_NoBackground(String.Format("{0}_{1}", btn.Name, "Button"), -1, 0, size.Width, size.Height, GraphicCorners.None, borderColor);
            }
            
            Button_Settings.Opacity = opacity;

            if (icon.image != null)
            {
                // Create icon image
                OImage oImgOverlay = (OImage)icon.image.Clone();
                if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                    oImgOverlay.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                Button_Settings.OverlayImage = new imageItem(oImgOverlay);
                Button_Settings.OverlayImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
                Button_Settings.GraphicSize = GraphicSize;
            }

            // Map actions
            if (OnClick is userInteraction)
                Button_Settings.OnClick += (userInteraction)OnClick;
            else if (OnClick is String)
                Button_Settings.Command_Click = OnClick as string;
            if (OnHoldClick is userInteraction)
                Button_Settings.OnHoldClick += (userInteraction)OnHoldClick;
            else if (OnHoldClick is String)
                Button_Settings.Command_HoldClick = OnHoldClick as string;
            if (OnLongClick is userInteraction)
                Button_Settings.OnLongClick += (userInteraction)OnLongClick;
            else if (OnLongClick is String)
                Button_Settings.Command_LongClick = OnLongClick as string;

            btn.Add(Button_Settings);

            return btn;
        }

        /// <summary>
        /// Creates a simple menu item with an icon and a text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="opacity"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="showSeparator"></param>
        /// <param name="OnClick"></param>
        /// <param name="OnHoldClick"></param>
        /// <param name="OnLongClick"></param>
        /// <returns></returns>
        static public Button CreateMenuItem(string name, Size size, int opacity, imageItem icon, string text, bool showSeparator, userInteraction OnClick, userInteraction OnHoldClick, userInteraction OnLongClick)
        {
            Button btn = new Button(name);

            // Calculate graphics size for icon in button (to ensure it's correctly aligned)
            Size GraphicSize = size;
            if (icon.image != null)
            {
                GraphicSize = icon.image.Size;
                if (icon.image.Width > size.Width)
                    GraphicSize.Width = size.Width;
                if (icon.image.Height > size.Height)
                    GraphicSize.Height = size.Height;
            }

            // Button function
            OMButton btnCtrl = new OMButton(String.Format("{0}_{1}", btn.Name, "Button"), 0, 0, size.Width, size.Height);
            btnCtrl.OverlayImageDrawMode = OMButton.DrawModes.FixedSizeLeft;
            btnCtrl.GraphicSize = GraphicSize;
            btnCtrl.Opacity = opacity;
            btnCtrl.Text = text;
            btnCtrl.TextAlignment = Alignment.CenterLeft;
            btnCtrl.OverlayImage = icon;
            if (icon.image != null)
                btnCtrl.TextOffset = new Point(btnCtrl.OverlayImage.image.Width, 0);
            //btnCtrl.DownImage = new imageItem(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);
            //btnCtrl.FocusImage = new imageItem(Color.FromArgb(200, BuiltInComponents.SystemSettings.SkinFocusColor), size.Width, size.Height);

            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            gd.BackgroundColor1 = Color.Transparent;
            gd.BorderColor = Color.Transparent;
            gd.Width = btnCtrl.Width;
            gd.Height = btnCtrl.Height;
            gd.CornerRadius = 0;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            btnCtrl.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            btnCtrl.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));


            // Map actions
            btnCtrl.OnClick += OnClick;
            btnCtrl.OnHoldClick += OnHoldClick;
            btnCtrl.OnLongClick += OnLongClick;

            btn.Add(btnCtrl);

            if (showSeparator)
            {
                // Add separator
                OMBasicShape shpSeparator = new OMBasicShape(String.Format("{0}_{1}", btn.Name, "Shape"), 0, btnCtrl.Height, size.Width, 1,
                    new ShapeData(shapes.Rectangle, Color.FromArgb(50, Color.White)));
                shpSeparator.Opacity = 178;
                btn.Add(shpSeparator);
            }
            return btn;
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns></returns>
        public new object Clone()
        {
            Button newBtn = new Button(this.Name);
            foreach (OMControl control in this)
                newBtn.Add((OMControl)control.Clone());
            return newBtn;
        }

        #endregion
    }

    /// <summary>
    /// A buttonstrip
    /// </summary>
    public class ButtonStrip : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// A buttonStrip item list
        /// </summary>
        public class ButtonStripItemsList : List<Button>
        {
            private ButtonStrip _Parent;

            #region Constructors

            /// <summary>
            /// Initializes a new item list
            /// </summary>
            /// <param name="parent"></param>
            public ButtonStripItemsList(ButtonStrip parent)
            {
                _Parent = parent;
            }

            #endregion

            #region Item control and access

            /// <summary>
            /// Add a new item
            /// </summary>
            /// <param name="item"></param>
            public new void Add(Button item)
            {
                base.Add(item);
                int index = this.IndexOf(item);
                this.Raise_OnItemAdded(_Parent.Screen, index);
            }

            /// <summary>
            /// Add a list of items
            /// </summary>
            /// <param name="buttons"></param>
            public new void AddRange(IEnumerable<Button> buttons)
            {
                base.AddRange(buttons);
                this.Raise_OnItemAdded(_Parent.Screen, - 1);
            }

            /// <summary>
            /// Remove an item 
            /// </summary>
            /// <param name="item"></param>
            public new void Remove(Button item)
            {
                int index = this.IndexOf(item);
                if (index == -1)
                    return;
                RemoveAt(index);
            }

            /// <summary>
            /// Remove an item 
            /// </summary>
            /// <param name="buttonName"></param>
            public new void Remove(string buttonName)
            {
                Button btn = this[buttonName];
                if (btn == null)
                    return;
                Remove(btn);
            }

            /// <summary>
            /// Remove an item at the specified index
            /// </summary>
            /// <param name="index"></param>
            public new void RemoveAt(int index)
            {
                base.RemoveAt(index);
                this.Raise_OnItemRemoved(_Parent.Screen, index);
            }

            /// <summary>
            /// Items contained in this list
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public new Button this[int index]
            {
                get
                {
                    return base[index];
                }
                set
                {
                    if (base[index] != value)
                    {
                        base[index] = value;
                        this.Raise_OnItemChanged(_Parent.Screen, index);
                    }
                }
            }

            /// <summary>
            /// Items contained in this list
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public new Button this[string name]
            {
                get
                {
                    return this.Find(x => x.Name == name);
                }
                set
                {
                    Button btn = this.Find(x => x.Name == name);
                    if (btn != null)
                    {
                        if (btn != value)
                        {
                            int index = this.IndexOf(btn);
                            if (index == -1)
                                return;
                            base[index] = value;
                            this.Raise_OnItemChanged(_Parent.Screen, index);
                        }
                    }
                }
            }

            #endregion

            #region Events

            /// <summary>
            /// Items changed event delegate
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="screen"></param>
            /// <param name="index"></param>
            public delegate void ItemsChangedEventHandler(object sender, int? screen, int index);

            /// <summary>
            /// Raised when an item changes
            /// </summary>
            public event ItemsChangedEventHandler OnItemChanged;
            private void Raise_OnItemChanged(int? screen, int index)
            {
                ItemsChangedEventHandler handler = OnItemChanged;
                if (handler != null)
                    handler(this, screen, index);
            }

            /// <summary>
            /// Raised when an item is added
            /// </summary>
            public event ItemsChangedEventHandler OnItemAdded;
            private void Raise_OnItemAdded(int? screen, int index)
            {
                ItemsChangedEventHandler handler = OnItemAdded;
                if (handler != null)
                    handler(this, screen, index);
            }

            /// <summary>
            /// Raised when an item is removed
            /// </summary>
            public event ItemsChangedEventHandler OnItemRemoved;
            private void Raise_OnItemRemoved(int? screen, int index)
            {
                ItemsChangedEventHandler handler = OnItemRemoved;
                if (handler != null)
                    handler(this, screen, index);
            }

            #endregion
        }

        #region Properties

        /// <summary>
        /// The screen this is assigned to
        /// </summary>
        public int? Screen
        {
            get
            {
                return this._Screen;
            }
            set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                }
            }
        }
        private int? _Screen = null;                

        /// <summary>
        /// The name of the button strip
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        /// <summary>
        /// The contained buttons 
        /// </summary>
        public ButtonStripItemsList Buttons
        {
            get
            {
                return this._Buttons;
            }
            set
            {
                if (this._Buttons != value)
                {
                    this._Buttons = value;
                    this.OnPropertyChanged("Buttons");
                }
            }
        }
        private ButtonStripItemsList _Buttons;

        /// <summary>
        /// The name of the plugin this buttonstrip belongs to
        /// </summary>
        public string PluginName
        {
            get
            {
                return this._PluginName;
            }
            set
            {
                if (this._PluginName != value)
                {
                    this._PluginName = value;
                }
            }
        }
        private string _PluginName;

        /// <summary>
        /// The name of the panel this buttonstrip belongs to
        /// </summary>
        public string PanelName
        {
            get
            {
                return this._PanelName;
            }
            set
            {
                if (this._PanelName != value)
                {
                    this._PanelName = value;
                }
            }
        }
        private string _PanelName;
        

        #endregion

        #region Events

        void Buttons_OnItemChanged(object sender, int? screen, int index)
        {
            // Propagate change further upwards
            this.Raise_OnItemChanged(screen, index);
        }

        void Buttons_OnItemRemoved(object sender, int? screen, int index)
        {
            // Propagate change further upwards
            this.Raise_OnItemRemoved(screen, index);
        }

        void Buttons_OnItemAdded(object sender, int? screen, int index)
        {
            // Propagate change further upwards
            this.Raise_OnItemAdded(screen, index);
        }

        void Buttons_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Propagate change further upwards
            this.OnPropertyChanged(String.Format("Buttons_{0}", e.PropertyName));
        }

        /// <summary>
        /// Item changed eventhandler delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        /// <param name="index"></param>
        public delegate void ItemsChangedEventHandler(object sender, int? screen, int index);

        /// <summary>
        /// Raised when an item changes
        /// </summary>
        public event ItemsChangedEventHandler OnItemChanged;
        private void Raise_OnItemChanged(int? screen, int index)
        {
            ItemsChangedEventHandler handler = OnItemChanged;
            if (handler != null)
                handler(this, screen, index);
        }

        /// <summary>
        /// Raised when an item is added
        /// </summary>
        public event ItemsChangedEventHandler OnItemAdded;
        private void Raise_OnItemAdded(int? screen, int index)
        {
            ItemsChangedEventHandler handler = OnItemAdded;
            if (handler != null)
                handler(this, screen, index);
        }

        /// <summary>
        /// Raised when an item is changed
        /// </summary>
        public event ItemsChangedEventHandler OnItemRemoved;
        private void Raise_OnItemRemoved(int? screen, int index)
        {
            ItemsChangedEventHandler handler = OnItemRemoved;
            if (handler != null)
                handler(this, screen, index);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a ButtonStrip
        /// </summary>
        /// <param name="name"></param>
        public ButtonStrip(string pluginName, string panelName, string name)
        {
            this._Name = name;
            this._PluginName = pluginName;
            this._PanelName = panelName;

            _Buttons = new ButtonStripItemsList(this);

            // Subscribe to changes in the buttons
            Buttons.OnItemAdded += new ButtonStripItemsList.ItemsChangedEventHandler(Buttons_OnItemAdded);
            Buttons.OnItemRemoved += new ButtonStripItemsList.ItemsChangedEventHandler(Buttons_OnItemRemoved);
            Buttons.OnItemChanged += new ButtonStripItemsList.ItemsChangedEventHandler(Buttons_OnItemChanged);
        }

        /// <summary>
        /// Initializes a new instance of a ButtonStrip
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        public ButtonStrip(string pluginName, string panelName, string name, List<Button> buttons)
            : this(pluginName, panelName, name)
        {
            _Buttons.AddRange(buttons);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Propertychanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region iClonable

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            ButtonStrip newBS = new ButtonStrip(this.PluginName, this.PanelName, this.Name);
            for (int i = 0; i < this.Buttons.Count; i++)
                newBS.Buttons.Add((Button)this.Buttons[i].Clone());
            newBS.Screen = this.Screen;
            newBS.OnItemChanged = this.OnItemChanged;
            newBS.OnItemAdded = this.OnItemAdded;
            newBS.OnItemRemoved = this.OnItemRemoved;
            return newBS;
        }

        #endregion
    }

    /// <summary>
    /// A container for buttonstrips
    /// </summary>
    public class ButtonStripContainer
    {
        #region Properties

        /// <summary>
        /// The container for the controls (Add this to your skin)
        /// <para>Set the name, dimensions and placement for the control when adding it to your skin</para>
        /// </summary>
        public OMContainer Container
        {
            get
            {
                return this._Container;
            }
            set
            {
                if (this._Container != value)
                {
                    this._Container = value;
                }
            }
        }
        private OMContainer _Container;

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return _Container.Name;
            }
            set
            {
                _Container.Name = value;
            }
        }

        /// <summary>
        /// Autosize mode of the button strip
        /// </summary>
        public OMContainer.AutoSizeModes AutoSizeMode
        {
            get
            {
                if (_Container == null)
                    return OMContainer.AutoSizeModes.NoAutoSize;
                return _Container.AutoSizeMode;
            }
            set
            {
                if (_Container != null)
                {
                    if (_Container.AutoSizeMode != value)
                    {
                        _Container.AutoSizeMode = value;
                    }
                }
            }
        }

        #endregion

        #region ButtonStrip access

        /// <summary>
        /// Is a buttonstrip available?
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool Available(int screen)
        {
            return _ButtonStrip[screen] != null;
        }

        private ButtonStrip[] _ButtonStrip;
        /// <summary>
        /// Returns the currently active buttonstrip at the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public ButtonStrip GetButtonStrip(int screen)
        {
            return _ButtonStrip[screen];
        }

        /// <summary>
        /// Clears the current buttonstrip
        /// </summary>
        public void ClearButtonStrip()
        {
            SetButtonStrip(null, null);
        }
        /// <summary>
        /// Clears the current buttonstrip at the given screen
        /// </summary>
        /// <param name="screen"></param>
        public void ClearButtonStrip(int? screen)
        {
            SetButtonStrip(screen, null);
        }
        /// <summary>
        /// Sets the currently active buttonstrip at all screens
        /// </summary>
        /// <param name="strip"></param>
        public void SetButtonStrip(ButtonStrip strip)
        {
            SetButtonStrip(null, strip);
        }
        /// <summary>
        /// Sets the currently active buttonstrip at the given screen
        /// <para>Use null to clear the current strip</para>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="strip"></param>
        public void SetButtonStrip(int? screen, ButtonStrip strip)
        {
            if (screen != null)
            {   // Set on a specific screen
                int s = (int)screen;
                if (this._ButtonStrip[s] != strip)
                {
                    // Unsubscribe to events
                    if (_ButtonStrip[s] != null)
                    {
                        _ButtonStrip[s].OnItemAdded -= Event_ButtonStrip_OnItemAdded;
                        _ButtonStrip[s].OnItemChanged -= Event_ButtonStrip_OnItemChanged;
                        _ButtonStrip[s].OnItemRemoved -= Event_ButtonStrip_OnItemRemoved;
                    }

                    _ButtonStrip[s] = strip;

                    // Remove any contained buttons from the container
                    ItemClear_Internal(s);

                    if (_ButtonStrip[s] != null)
                    {
                        // Set current screen
                        _ButtonStrip[s].Screen = s;

                        // Subscribe to events
                        _ButtonStrip[s].OnItemAdded += Event_ButtonStrip_OnItemAdded;
                        _ButtonStrip[s].OnItemChanged += Event_ButtonStrip_OnItemChanged;
                        _ButtonStrip[s].OnItemRemoved += Event_ButtonStrip_OnItemRemoved;

                        // Add buttons to container
                        for (int i = 0; i < _ButtonStrip[s].Buttons.Count; i++)
                            ItemAdd_Internal(screen, i);
                    }

                    // Raise event
                    Raise_OnButtonStripSet(s, _ButtonStrip[s]);
                }
            }
            else
            {   // Set on all screens
                for (int i = 0; i < _ButtonStrip.Length; i++)
                    SetButtonStrip(i, (ButtonStrip)strip.Clone());
            }
        }

        #endregion

        #region Alignments

        /// <summary>
        /// Alignments of toolstrip
        /// </summary>
        public enum Alignments 
        { 
            /// <summary>
            /// Place buttons from the left to the right
            /// </summary>
            Left, 
            /// <summary>
            /// Place buttons in center
            /// </summary>
            CenterLR, 
            /// <summary>
            /// Place buttons from the right to the left
            /// </summary>
            Right, 
            /// <summary>
            /// Place buttons from the bottom going upwards
            /// </summary>
            Up, 
            /// <summary>
            /// Place buttons from the top going downwards
            /// </summary>
            Down 
        }

        /// <summary>
        /// The alignment of the buttons
        /// </summary>
        public Alignments Alignment
        {
            get
            {
                return this._Alignment;
            }
            set
            {
                if (this._Alignment != value)
                {
                    this._Alignment = value;
                }
            }
        }
        private Alignments _Alignment = Alignments.Left;

        private ControlDirections GetOMContainerDirection(Alignments alignment)
        {
            switch (alignment)
            {
                case Alignments.Left:
                    return ControlDirections.Right;
                case Alignments.CenterLR:
                    return ControlDirections.CenterHorizontally;
                case Alignments.Right:
                    return ControlDirections.Left;
                case Alignments.Up:
                    return ControlDirections.Up;
                case Alignments.Down:
                    return ControlDirections.Down;
                default:
                    return ControlDirections.Right;
            }
        }

        #endregion

        #region Button size

        /// <summary>
        /// The suggested size for buttons in this container (based on size of container)
        /// </summary>
        public Size SuggestedButtonSize
        {
            get
            {
                return new Size(_Container.Region.Height, _Container.Region.Height);
            }
        }

        /// <summary>
        /// The size for buttons in this container
        /// </summary>
        public Size ButtonSize
        {
            get
            {
                if (_ButtonSize.IsEmpty)
                    return new Size(_Container.Region.Height, _Container.Region.Height);
                else
                    return _ButtonSize;
            }
            set
            {
                _ButtonSize = value;
            }

        }
        private Size _ButtonSize = new Size();

        #endregion

        #region Item control (Clear, Add, Remove, Change)

        private void ItemClear_Internal(int? screen)
        {
            if (screen == null)
            {
                if (_Container.Parent != null && _Container.Parent.IsClonedForScreens())
                {   // Container is loaded on separate screens
                    BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                    {
                        OMContainer container = _Container.Parent[Screen, _Container.Name] as OMContainer;
                        container.ClearControls();
                    });
                }
                else
                {   // Container not loaded on any screen yet
                    _Container.ClearControls();
                }
            }
            else
            {   // Access a specifc container
                OMContainer container = _Container.Parent[(int)screen, _Container.Name] as OMContainer;
                container.ClearControls();
            }
        }
        private void ItemAdd_Internal(int? screen, int index)
        {
            if (screen == null)
            {
                if (index >= 0)
                {
                    if (_ButtonStrip != null)
                    {

                        if (_Container.Parent != null && _Container.Parent.IsClonedForScreens())
                        {   // Container is loaded on separate screens, add control on each screen
                            BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                            {
                                OMContainer container = _Container.Parent[Screen, _Container.Name] as OMContainer;
                                container.addControl((Button)_ButtonStrip[Screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
                            });
                        }
                        else
                        {   // Container not loaded on any screen yet, just add it
                            _Container.addControl((Button)_ButtonStrip[(int)screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
                        }
                    }
                }
            }
            else
            {   // Access a specifc container
                OMContainer container = _Container.Parent[(int)screen, _Container.Name] as OMContainer;
                container.addControl((Button)_ButtonStrip[(int)screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
            }
        }
        private void ItemRemove_Internal(int? screen, int index)
        {
            if (screen == null)
            {
                if (index >= 0)
                {
                    if (_ButtonStrip != null)
                    {

                        if (_Container.Parent != null && _Container.Parent.IsClonedForScreens())
                        {   // Container is loaded on separate screens, remove control from each screen
                            BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                            {
                                OMContainer container = _Container.Parent[Screen, _Container.Name] as OMContainer;
                                container.RemoveControl(index);
                            });
                        }
                        else
                        {   // Container not loaded on any screen yet, just remove it
                            _Container.RemoveControl(index);
                        }
                    }
                }
            }
            else
            {   // Access a specifc container
                OMContainer container = _Container.Parent[(int)screen, _Container.Name] as OMContainer;
                container.RemoveControl(index);
            }
        }
        private void ItemChange_Internal(int? screen, int index)
        {
            if (screen == null)
            {
                if (index >= 0)
                {
                    if (_ButtonStrip != null)
                    {

                        if (_Container.Parent != null && _Container.Parent.IsClonedForScreens())
                        {   // Container is loaded on separate screens, update control on each screen
                            BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                            {
                                lock (_ButtonStrip[Screen].Buttons)
                                {
                                    lock (_Container)
                                    {
                                        OMContainer container = _Container.Parent[Screen, _Container.Name] as OMContainer;
                                        lock (container)
                                        {
                                            container.ReplaceControl(index, (Button)_ButtonStrip[Screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {   // Container not loaded on any screen yet, just update it
                            lock (_Container)
                            {
                                _Container.ReplaceControl(index, (Button)_ButtonStrip[(int)screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
                            }
                        }
                    }
                }
            }
            else
            {   // Access a specifc container
                OMContainer container = _Container.Parent[(int)screen, _Container.Name] as OMContainer;
                container.ReplaceControl(index, (Button)_ButtonStrip[(int)screen].Buttons[index].Clone(), GetOMContainerDirection(_Alignment));
            }
        }

        #endregion

        #region Events

        ButtonStrip.ItemsChangedEventHandler Event_ButtonStrip_OnItemAdded;
        void _ButtonStrip_OnItemAdded(object sender, int? screen, int index)
        {
            ItemAdd_Internal(screen, index);
        }
        ButtonStrip.ItemsChangedEventHandler Event_ButtonStrip_OnItemRemoved;
        void _ButtonStrip_OnItemRemoved(object sender, int? screen, int index)
        {
            ItemRemove_Internal(screen, index);
        }
        ButtonStrip.ItemsChangedEventHandler Event_ButtonStrip_OnItemChanged;
        void _ButtonStrip_OnItemChanged(object sender, int? screen, int index)
        {
            ItemChange_Internal(screen, index);
        }

        /// <summary>
        /// Buttonstrip changed eventhandler delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        public delegate void ButtonStripChangedEventHandler(object sender, int screen, ButtonStrip buttonStrip);

        /// <summary>
        /// Raised when the buttonstrip is set
        /// </summary>
        public event ButtonStripChangedEventHandler OnButtonStripSet;
        private void Raise_OnButtonStripSet(int screen, ButtonStrip buttonStrip)
        {
            ButtonStripChangedEventHandler handler = OnButtonStripSet;
            if (handler != null)
                handler(this, screen, buttonStrip);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new ButtonStripContainer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ButtonStripContainer(string name, int left, int top, int width, int height)
            : this(name, new Rectangle(left, top, width, height))
        {
        }
        /// <summary>
        /// Initializes a new ButtonStripContainer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="region"></param>
        public ButtonStripContainer(string name, Rectangle region)
            : this()
        {
            _Container.Name = name;
            _Container.Region = region;
        }
        /// <summary>
        /// Initializes a new ButtonStripContainer
        /// </summary>
        public ButtonStripContainer()
        {
            _Container = new OMContainer("", 0, 0, 1000, 600);
            _Container.ScrollBar_Horizontal_Enabled = true;
            _Container.ScrollBar_Vertical_Enabled = true;

            Event_ButtonStrip_OnItemAdded = new ButtonStrip.ItemsChangedEventHandler(_ButtonStrip_OnItemAdded);
            Event_ButtonStrip_OnItemRemoved = new ButtonStrip.ItemsChangedEventHandler(_ButtonStrip_OnItemRemoved);
            Event_ButtonStrip_OnItemChanged = new ButtonStrip.ItemsChangedEventHandler(_ButtonStrip_OnItemChanged);

            _ButtonStrip = new ButtonStrip[BuiltInComponents.Host.ScreenCount];
            //for (int i = 0; i < _ButtonStrip.Length; i++)
            //    _ButtonStrip[i] = new ButtonStrip("");
        }

        #endregion
    }

}
