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
    public static class panel3D
    {
        static OpenMobile.Timer tmrAutoRot;
        static ButtonStrip PopUpMenuStrip;

        public static OMPanel Initialize()
        {
            OMPanel p = new OMPanel("panel3D", "ControlDemo > 3D Rotation test");

            // Configure cube            
            OMCube cubeTest = new OMCube("cubeTest", 500, OM.Host.ClientArea_Init.Top + 100, 200, 200); //, new imageItem(MediaLoader.MissingCoverImage));//OM.Host.getSkinImage("Unknown Album"));
            cubeTest.Image1 = new imageItem(MediaLoader.MissingCoverImage);
            cubeTest.Image2 = new imageItem(MediaLoader.MissingCoverImage);
            cubeTest.Image3 = new imageItem(MediaLoader.MissingCoverImage);
            cubeTest.Image4 = new imageItem(MediaLoader.MissingCoverImage);
            cubeTest.Image5 = new imageItem(MediaLoader.MissingCoverImage);
            cubeTest.Image6 = new imageItem(MediaLoader.MissingCoverImage);
            p.addControl(cubeTest);

            OMImage imgReflectionTestSource = new OMImage("imgReflectionTestSource", 500, OM.Host.ClientArea_Init.Top + 100, 200, 200, new imageItem(MediaLoader.MissingCoverImage));//host.getSkinImage("Unknown Album.png"));
            imgReflectionTestSource.Visible = false;
            p.addControl(imgReflectionTestSource);

            OMButton btnAuto = OMButton.PreConfigLayout_BasicStyle("btnAuto", 25, 200, 200, 50, GraphicCorners.Top, "", "Auto");
            btnAuto.OnClick += new userInteraction(btnAuto_OnClick);
            p.addControl(btnAuto);

            //OMButton btnCamera = OMButton.PreConfigLayout_BasicStyle("btnCamera", 25, 249, 200, 50, GraphicCorners.None, "", "Camera");
            //btnCamera.OnClick += new userInteraction(btnCamera_OnClick);
            //p.addControl(btnCamera);

            OMButton btnReset = OMButton.PreConfigLayout_BasicStyle("btnReset", 25, 249, 200, 50, GraphicCorners.Bottom, "", "Reset");
            btnReset.OnClick += new userInteraction(btnReset_OnClick);
            p.addControl(btnReset);

            OMLabel lblRotationX = new OMLabel("lblRotationX", 025, 388, 200, 40);
            lblRotationX.Text = "X:";
            lblRotationX.TextAlignment = Alignment.CenterLeft;
            p.addControl(lblRotationX);

            OMSlider Slider_RotationX = new OMSlider("Slider_RotationX", 100, 400, 200, 25, 12, 40);
            Slider_RotationX.Slider = OM.Host.getSkinImage("Slider");
            Slider_RotationX.SliderBar = OM.Host.getSkinImage("Slider.Bar");
            Slider_RotationX.Minimum = -180;
            Slider_RotationX.Maximum = 180;
            Slider_RotationX.Value = 0;
            Slider_RotationX.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            p.addControl(Slider_RotationX);

            OMLabel lblRotationY = new OMLabel("lblRotationY", 025, 438, 200, 40);
            lblRotationY.Text = "Y:";
            lblRotationY.TextAlignment = Alignment.CenterLeft;
            p.addControl(lblRotationY);

            OMSlider Slider_RotationY = new OMSlider("Slider_RotationY", 100, 450, 200, 25, 12, 40);
            Slider_RotationY.Slider = OM.Host.getSkinImage("Slider");
            Slider_RotationY.SliderBar = OM.Host.getSkinImage("Slider.Bar");
            Slider_RotationY.Minimum = -180;
            Slider_RotationY.Maximum = 180;
            Slider_RotationY.Value = 0;
            Slider_RotationY.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            p.addControl(Slider_RotationY);

            OMLabel lblRotationZ = new OMLabel("lblRotationZ", 025, 488, 200, 40);
            lblRotationZ.Text = "Z:";
            lblRotationZ.TextAlignment = Alignment.CenterLeft;
            p.addControl(lblRotationZ);

            OMSlider Slider_RotationZ = new OMSlider("Slider_RotationZ", 100, 500, 200, 25, 12, 40);
            Slider_RotationZ.Slider = OM.Host.getSkinImage("Slider");
            Slider_RotationZ.SliderBar = OM.Host.getSkinImage("Slider.Bar");
            Slider_RotationZ.Minimum = -180;
            Slider_RotationZ.Maximum = 180;
            Slider_RotationZ.Value = 0;
            Slider_RotationZ.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            p.addControl(Slider_RotationZ);

            p.Entering += new PanelEvent(p_Entering);

            tmrAutoRot = new Timer(1);
            tmrAutoRot.Elapsed += new System.Timers.ElapsedEventHandler(tmrAutoRot_Elapsed);

            return p;
        }

        static bool _CameraControl = false;
        static void btnCamera_OnClick(OMControl sender, int screen)
        {
            OMButton btn = sender as OMButton;
            btn.Checked = _CameraControl = !_CameraControl;
        }

        static void tmrAutoRot_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OMImage img = ((OpenMobile.Timer)sender).Tag as OMImage;
            if (img.Tag == null)
                img.Tag = 0;

            OMCube cube = img.Parent[((OpenMobile.Timer)sender).Screen, "cubeTest"] as OMCube;

            int rot = (int)img.Tag;
            rot++;
            img.Tag = rot;
            cube.Rotation = img.Rotation = new OpenMobile.Math.Vector3(rot * 1, rot * 0.7f, rot * 0.2f);
        }

        static void btnAuto_OnClick(OMControl sender, int screen)
        {
            OMButton btn = sender as OMButton;

            if (sender.Tag != null)
                sender.Tag = !(bool)sender.Tag;
            else
                sender.Tag = true;

            btn.Checked = tmrAutoRot.Enabled = (bool)sender.Tag;
            tmrAutoRot.Tag = sender.Parent[screen, "imgReflectionTestSource"] as OMImage;
            tmrAutoRot.Screen = screen;

        }

        static void btnReset_OnClick(OMControl sender, int screen)
        {
            // Reset rotation
            OMImage img = sender.Parent[screen, "imgReflectionTestSource"] as OMImage;
            OMLabel lblX = sender.Parent[screen, "lblRotationX"] as OMLabel;
            OMLabel lblY = sender.Parent[screen, "lblRotationY"] as OMLabel;
            OMLabel lblZ = sender.Parent[screen, "lblRotationZ"] as OMLabel;
            OMSlider sldrX = sender.Parent[screen, "Slider_RotationX"] as OMSlider;
            OMSlider sldrY = sender.Parent[screen, "Slider_RotationY"] as OMSlider;
            OMSlider sldrZ = sender.Parent[screen, "Slider_RotationZ"] as OMSlider;
            OMCube cube = sender.Parent[screen, "cubeTest"] as OMCube;

            sldrX.Value = 0;
            sldrY.Value = 0;
            sldrZ.Value = 0;

            int XValue = sldrX.Value;
            int YValue = sldrY.Value;
            int ZValue = sldrZ.Value;

            cube.Rotation = img.Rotation = new OpenMobile.Math.Vector3(XValue, YValue, ZValue);
            lblX.Text = String.Format("X: {0}", XValue);
            lblY.Text = String.Format("Y: {0}", YValue);
            lblZ.Text = String.Format("Z: {0}", ZValue);
        }

        static void p_Entering(OMPanel sender, int screen)
        {
            //OM.Host.UIHandler.Bars_Hide(screen, false, OpenMobile.UI.UIHandler.Bars.All);
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
            OMCube cube = sender.Parent[screen, "cubeTest"] as OMCube;

            int XValue = sldrX.Value;
            int YValue = sldrY.Value;
            int ZValue = sldrZ.Value;

            lblX.Text = String.Format("X: {0}", XValue);
            lblY.Text = String.Format("Y: {0}", YValue);
            lblZ.Text = String.Format("Z: {0}", ZValue);

            if (_CameraControl)
            {
                OM.Host.RenderingWindowInterface(screen).graphics._3D_ModelView_Set(OpenMobile.Math.Vector3.Zero,
                    new OpenMobile.Math.Vector3(XValue, YValue, ZValue), OpenMobile.Math.Vector3.Zero, 1, false);
            }
            else
            {
                cube.Rotation = img.Rotation = new OpenMobile.Math.Vector3(XValue, YValue, ZValue);
            }
        }

    }
}
