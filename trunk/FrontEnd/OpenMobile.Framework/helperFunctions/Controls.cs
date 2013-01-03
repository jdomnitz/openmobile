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
using System.Threading;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace OpenMobile.helperFunctions.Controls
{
     /// <summary>
    /// A set of up and down control methods
    /// </summary>
    public class UpDown
    {

        /// <summary>
        /// Provides up/down functionality for a set of controls consisting of one or more buttons and one textbox.
        /// <para>The buttons provides up/down controls where the step size is set by assigning a integer value to the button.Tag property</para>
        /// <para>The textbox is used to provide feedback for selection</para>
        /// <para>Connect the same event to all buttons and trigg this method from all buttons</para>
        /// </summary>
        /// <param name="screen">Current screen</param>
        /// <param name="upDownButtons">The button that is used for this up/down</param>
        /// <param name="textBox">The textbox target</param>
        /// <param name="stringArray">The string array to use for selection</param>
        static public void UpDownTextBoxControl(int screen, OMButton upDownButtons, OMTextBox textBox, string[] stringArray)
        {
            // Errorcheck
            if (stringArray == null)
            {
                textBox.Text = "";
                return;
            }

            // Find index in array
            int Index;
            try
            {
                Index = Array.FindIndex(stringArray, p => p == textBox.Text);
            }
            catch
            {   // No match, return a default of first index to get a valid value
                textBox.Text = stringArray[0];
                return;
            }

            // Calculate new index
            Index += (int)upDownButtons.Tag;

            // Limit check
            if (Index < stringArray.GetLowerBound(0))
                Index = stringArray.GetLowerBound(0);
            if (Index >= stringArray.GetUpperBound(0))
                Index = stringArray.GetUpperBound(0);

            // Save new selection
            textBox.Text = stringArray[Index];
        }
    }

    public static class DefaultControls
    {
        /// <summary>
        /// Gets a generic usage button 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        static public OMButton GetButton(string name, int x, int y, int w, int h, string Icon, string Text)
        {
            OMButton btn = new OMButton(name, x, y, w, h);

            // Set background image
            btn.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonBackground));

            // Set focus image
            btn.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonBackgroundFocused));

            // Set focus clicked image
            btn.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonBackgroundClicked));

            // Set overlay image
            btn.OverlayImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonForeground, Icon, Text));

            btn.Transition = eButtonTransition.None;

            return btn;
        }

        /// <summary>
        /// Gets a generic button for placement on either the top or bottom edge of the screen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        static public OMButton GetHorisontalEdgeButton(string name, int x, int y, int w, int h, string Icon, string Text)
        {
            OMButton btn = new OMButton(name, x, y, w, h);

            // Set background image
            btn.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonBackground));

            // Set focus image
            ButtonGraphic.GraphicData gd = new ButtonGraphic.GraphicData();
            gd.Width = w;
            gd.Height = h;
            gd.ImageType = ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            gd.BackgroundFocusSize = 1.0f;
            btn.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            // Set overlay image
            btn.OverlayImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonForeground, Icon, Text));

            return btn;
        }

        /// <summary>
        /// Updates a generic button for placement on either the top or bottom edge of the screen
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="Icon"></param>
        /// <param name="Text"></param>
        static public void UpdateHorisontalEdgeButton(OMButton btn, int x, int y, int w, int h, string Icon, string Text)
        {
            // Set background image
            btn.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonBackground));

            // Set focus image
            ButtonGraphic.GraphicData gd = new ButtonGraphic.GraphicData();
            gd.Width = w;
            gd.Height = h;
            gd.ImageType = ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            gd.BackgroundFocusSize = 1.0f;
            btn.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            // Set overlay image
            btn.OverlayImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(w, h, ButtonGraphic.ImageTypes.ButtonForeground, Icon, Text));
        }

    }
}
