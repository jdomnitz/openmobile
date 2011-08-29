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
using OpenMobile.Input;
namespace OpenMobile
{
    partial class RenderingWindow
    {   
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenderingWindow));
            // 
            // RenderingWindow
            // 

            // NB! Mouse and keyboard mapping has been moved to the input router code so do not remove the comments below
            //this.Mouse.ButtonUp += new EventHandler<OpenMobile.Input.MouseButtonEventArgs>(InputRouter.dev_ButtonUp);
            //this.Mouse.MouseClick += new EventHandler<OpenMobile.Input.MouseButtonEventArgs>(InputRouter.dev_MouseClick);
            //this.Mouse.ButtonDown += new EventHandler<OpenMobile.Input.MouseButtonEventArgs>(InputRouter.dev_ButtonDown);
            //this.Mouse.Move += new EventHandler<OpenMobile.Input.MouseMoveEventArgs>(InputRouter.dev_Move);
            //this.Keyboard[0].KeyUp += new EventHandler<OpenMobile.Input.KeyboardKeyEventArgs>(InputRouter.SourceUp);
            //this.Keyboard[0].KeyDown += new EventHandler<OpenMobile.Input.KeyboardKeyEventArgs>(InputRouter.SourceDown);

            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MouseLeave += new System.EventHandler<System.EventArgs>(this.RenderingWindow_MouseLeave);
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(this.RenderingWindow_FormClosing);
            this.Resize += new EventHandler<EventArgs>(this.RenderingWindow_Resize);
            this.Gesture += new EventHandler<OpenMobile.Graphics.TouchEventArgs>(RenderingWindow_Gesture);
            this.ResolutionChange += new EventHandler<OpenMobile.Graphics.ResolutionChange>(RenderingWindow_ResolutionChange);
        }
        #endregion
        private System.Timers.Timer tmrClick;
        private System.Timers.Timer tmrLongClick;
    }
}