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
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static OMListItem.subItemFormat subItemformat;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            OMPanel p = new OMPanel("Images");

            OMImage imgNoCover = new OMImage("imgNoCover", 0, 100, new imageItem(MediaLoader.MissingCoverImage));
            p.addControl(imgNoCover);

            OMImage imgBackground = new OMImage("imgBackground", 330, 100, host.getSkinImage("OMIconBlack_Transparent"));
            imgBackground.BackgroundColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            imgBackground.Left = 500 - (imgBackground.Image.image.Width / 2);
            imgBackground.Top = 300 - (imgBackground.Image.image.Height / 2);
            p.addControl(imgBackground);

            OMImage imgReflectionTestSource = new OMImage("imgReflectionTestSource", 730, 130, 200, 200, host.getSkinImage("Unknown Album.png"));
            //imgReflectionTestSource.Rotation = new OpenMobile.Math.Vector3(0, 0, 45);
            p.addControl(imgReflectionTestSource);

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

            OMImage imgReflectionTestTarget = new OMImage("imgReflectionTestTarget", imgReflectionTestSource.Left, imgReflectionTestSource.Top + imgReflectionTestSource.Height, imgReflectionTestSource.Width, imgReflectionTestSource.Height);
            if (imgReflectionTestSource.Image != null)
                imgReflectionTestTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgReflectionTestSource.Image, 0.7f, true);
                //imgReflectionTestTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgReflectionTestSource.Image, true);
            p.addControl(imgReflectionTestTarget);

            manager.loadPanel(p);
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
