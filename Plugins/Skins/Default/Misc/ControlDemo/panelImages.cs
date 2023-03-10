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
using System.Collections.Generic;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.Media;


namespace ControlDemo
{
    public static class panelImages
    {
        public class Bricks : OMImage
        {
            public Bricks()
                : base("imgBricks", OM.Host.ClientArea_Init.Center.X - (75 / 2), OM.Host.ClientArea_Init.Top + 10, OM.Host.getSkinImage("Icons|OMBricks-Bricks_Gray.png"))
            {
            }

            public void ShowOverlay(imageItem image)
            {
                this.Image = new imageItem(Image.Copy().image.AddImageOverlay(0, 0, image.image));
                base.RefreshGraphic();
                base.Refresh();
            }
        }

        public static OMPanel Initialize()
        {
            OMPanel p = new OMPanel("Images");

            //OMImage imgNoCover = new OMImage("imgNoCover", 0, 100, new imageItem(MediaLoader.MissingCoverImage));
            //p.addControl(imgNoCover);

            //OMImage imgOverlayTest = new OMImage("imgOverlayTest", OM.Host.ClientArea_Init.Center.X - (75 / 2), OM.Host.ClientArea_Init.Top + 10, OM.Host.getSkinImage("Icons|OMBricks-Bricks_Gray.png"));
            //imgOverlayTest.Image = new imageItem(imgOverlayTest.Image.Copy().image.AddImageOverlay(0, 0, OM.Host.getSkinImage("Icons|OMBricks-Bricks_OL_Lasers").image));
            //p.addControl(imgOverlayTest);

            Bricks brick = new Bricks();
            p.addControl(brick);

            OM.Host.LoadSkinSprite("maneuvers-2x", 
                new Sprite("TurnSharpLeft", 0, 2, 38, 30),
                new Sprite("TurnLeft", 0, 36, 38, 27),
                new Sprite("UTurnRight", 0, 71, 38, 30)
                );
            OMImage imgIconTest = new OMImage("imgSpriteTest", 0, 100, OM.Host.getSkinImage("maneuvers-2x", "TurnSharpLeft"));
            p.addControl(imgIconTest);
            p.addControl(new OMImage("imgSpriteTest2", 0, 0, OM.Host.getSkinImage("maneuvers-2x", "TurnLeft")), ControlDirections.Down);
            p.addControl(new OMImage("imgSpriteTest3", 0, 0, OM.Host.getSkinImage("maneuvers-2x", "UTurnRight")), ControlDirections.Down);

            OMImage img9PatchProgressbar1 = new OMImage("img9PatchProgressbar1", 0, 0, OM.Host.getSkinImage("9Patch|progress_bar_fill_bg.9", new Size(250, 36)));
            p.addControl(img9PatchProgressbar1, ControlDirections.Down);
            OMImage img9PatchProgressbar2 = new OMImage("img9PatchProgressbar2", img9PatchProgressbar1.Left + 2, img9PatchProgressbar1.Top + 2, OM.Host.getSkinImage("9Patch|red_progress_bar_fill.9", new Size(img9PatchProgressbar1.Width - 40, img9PatchProgressbar1.Height - 4)));
            p.addControl(img9PatchProgressbar2);

            p.addControl(new OMImage("img9PatchTest", 0, 0, OM.Host.getSkinImage("9Patch|box_launcher_top_normal.9", new Size(150, 90))), ControlDirections.Down);
            p.addControl(new OMImage("img9PatchTest3", 0, 0, OM.Host.getSkinImage("9Patch|box_launcher_top_normal.9", new Size(250, 90))), ControlDirections.Down);
            p.addControl(new OMImage("img9PatchTest4", 0, 0, OM.Host.getSkinImage("9Patch|box_launcher_top_normal.9", new Size(250, 60))), ControlDirections.Down);


            imageItem img = OM.Host.getSkinImage("Icons|Icon-OM_Large");
            OImage oImg = img.image.Copy();
            oImg.Overlay(BuiltInComponents.SystemSettings.SkinFocusColor);
            oImg.Glow(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor));
            oImg.ShaderEffect = OMShaders.Radar;
            img = new imageItem(oImg);


            OMImage imgBackground = new OMImage("imgBackground", 330, 100, img);

            //imgBackground.BackgroundColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            //imgBackground.Image.image.ShaderEffect = OMShaders.Radar;
            imgBackground.Left = 500 - (imgBackground.Image.image.Width / 2);
            imgBackground.Top = 300 - (imgBackground.Image.image.Height / 2);
            p.addControl(imgBackground);

            //imageItem img = new imageItem(OM.Host.getSkinImage("Unknown Album.png").image);
            //img.image.ShaderEffect = OMShaders.Negative;
            OMImage imgReflectionTestSource = new OMImage("imgReflectionTestSource", 730, 130, 200, 250, OM.Host.getSkinImage("Unknown Album.png"));
            imgReflectionTestSource.Image.image.ShaderEffect = OMShaders.MouseDot;
            //imgReflectionTestSource.Image.image.ShaderEffect = OMShaders.Radar;
            //imgReflectionTestSource.Rotation = new OpenMobile.Math.Vector3(0, 0, 45);
            //imgReflectionTestSource.OnImageChange += new userInteraction(imgReflectionTestSource_OnImageChange);
            imgReflectionTestSource.ReflectionData = new ReflectionsData(Color.White, Color.Transparent, 0.5f);
            p.addControl(imgReflectionTestSource);

            OMButton btnChangeSource = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("btnChangeSource", 400, 500, 200, 90, "", "Source");
            btnChangeSource.OnClick += new userInteraction(btnChangeSource_OnClick);
            p.addControl(btnChangeSource);




            //OMLabel lblRotationX = new OMLabel("lblRotationX", 525, 388, 200, 40);
            //lblRotationX.Text = "X rotation:";
            //p.addControl(lblRotationX);

            //OMSlider Slider_RotationX = new OMSlider("Slider_RotationX", 700, 400, 250, 25, 12, 40);
            //Slider_RotationX.Slider = Host.getSkinImage("Slider");
            //Slider_RotationX.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationX.Maximum = 180;
            //Slider_RotationX.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationX);

            //OMLabel lblRotationY = new OMLabel("lblRotationY", 525, 438, 200, 40);
            //lblRotationY.Text = "Y rotation:";
            //p.addControl(lblRotationY);

            //OMSlider Slider_RotationY = new OMSlider("Slider_RotationY", 700, 450, 250, 25, 12, 40);
            //Slider_RotationY.Slider = Host.getSkinImage("Slider");
            //Slider_RotationY.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationY.Maximum = 180;
            //Slider_RotationY.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationY);

            //OMLabel lblRotationZ = new OMLabel("lblRotationZ", 525, 488, 200, 40);
            //lblRotationZ.Text = "Z rotation:";
            //p.addControl(lblRotationZ);

            //OMSlider Slider_RotationZ = new OMSlider("Slider_RotationZ", 700, 500, 250, 25, 12, 40);
            //Slider_RotationZ.Slider = Host.getSkinImage("Slider");
            //Slider_RotationZ.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationZ.Maximum = 180;
            //Slider_RotationZ.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationZ);

            //OMImage imgReflectionTestTarget = new OMImage("imgReflectionTestTarget", imgReflectionTestSource.Left, imgReflectionTestSource.Top + imgReflectionTestSource.Height, imgReflectionTestSource.Width, imgReflectionTestSource.Height);
            //if (imgReflectionTestSource.Image != null)
            //    imgReflectionTestTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgReflectionTestSource.Image, 0.7f, true);
            //    //imgReflectionTestTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgReflectionTestSource.Image, true);
            //p.addControl(imgReflectionTestTarget);

            return p;
        }

        static void imgReflectionTestSource_OnImageChange(OMControl sender, int screen)
        {
            OMImage imgTarget = (OMImage)sender.Parent[screen, "imgReflectionTestTarget"];
            OMImage imgSource = (OMImage)sender;
            if (imgSource.Image != null)
                imgTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgSource.Image, 0.7f, true);
        }

        static void btnChangeSource_OnClick(OMControl sender, int screen)
        {

            ((Bricks)sender.Parent[screen, "imgBricks"]).ShowOverlay(OM.Host.getSkinImage("Icons|OMBricks-Bricks_OL_Lasers"));

            //((OMImage)sender.Parent[screen,"imgReflectionTestSource"]).Image = new imageItem(OpenMobile.Net.Network.imageFromURL("http://farm4.staticflickr.com/3273/2996054575_a08a46fdb8.jpg"));

            //OImage img = OpenMobile.Net.Network.imageFromURL("http://upload.wikimedia.org/wikipedia/commons/a/a4/IgM_white_background.png");
            //img.MakeTransparent(Color.White);
            //((OMImage)sender.Parent[screen, "imgReflectionTestSource"]).Image = new imageItem(img);
        }

        static void Slider_Rotation_OnSliderMoved(OMSlider sender, int screen)
        {
            OMImage img = sender.Parent[screen, "imgReflectionTestSource"] as OMImage;
            OMLabel lblX = sender.Parent[screen, "lblRotationX"] as OMLabel;
            OMLabel lblY = sender.Parent[screen, "lblRotationY"] as OMLabel;
            OMLabel lblZ = sender.Parent[screen, "lblRotationZ"] as OMLabel;
            OMSlider sldrX = sender.Parent[screen, "Slider_RotationX"] as OMSlider;
            OMSlider sldrY = sender.Parent[screen, "Slider_RotationY"] as OMSlider;
            OMSlider sldrZ = sender.Parent[screen, "Slider_RotationZ"] as OMSlider;

            int XValue = sldrX.Value;
            int YValue = sldrY.Value;
            int ZValue = sldrZ.Value;

            img.Rotation = new OpenMobile.Math.Vector3(XValue, YValue, ZValue);
            lblX.Text = String.Format("X Rotation: {0}", XValue);
            lblY.Text = String.Format("Y Rotation: {0}", YValue);
            lblZ.Text = String.Format("Z Rotation: {0}", ZValue);
        }

    }
}
