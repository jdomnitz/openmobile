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
//this.KeyUp += new System.Windows.Forms.KeyEventHandler(InputRouter.SourceUp);
//this.KeyDown+=new System.Windows.Forms.KeyEventHandler(InputRouter.SourceDown);
using System;
namespace OpenMobile
{
    partial class RenderingWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenderingWindow));
            this.tmrClosing = new System.Windows.Forms.Timer(this.components);
            this.tmrClick = new System.Windows.Forms.Timer(this.components);
            this.tmrMouse = new System.Windows.Forms.Timer(this.components);
            this.tmrLongClick = new System.Windows.Forms.Timer(this.components);
            // 
            // tmrClosing
            // 
            this.tmrClosing.Interval = 20;
            this.tmrClosing.Tick += new System.EventHandler(this.tmrClosing_Tick);
            // 
            // tmrClick
            // 
            this.tmrClick.Interval = 20;
            this.tmrClick.Tick += new System.EventHandler(this.tmrClick_Tick);
            // 
            // tmrMouse
            // 
            this.tmrMouse.Interval = 200;
            this.tmrMouse.Tick += new System.EventHandler(this.tmrMouse_Tick);
            // 
            // tmrLongClick
            // 
            this.tmrLongClick.Interval = 500;
            this.tmrLongClick.Tick += new System.EventHandler(this.tmrLongClick_Tick);
            // 
            // RenderingWindow
            // 
            //TODO - Fix everything not implemented
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Mouse.ButtonUp += new EventHandler<OpenMobile.Input.MouseButtonEventArgs>(this.RenderingWindow_MouseUp);
            //this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RenderingWindow_MouseDoubleClick);
            //this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RenderingWindow_MouseClick);
            this.Mouse.ButtonDown += new EventHandler<OpenMobile.Input.MouseButtonEventArgs>(this.RenderingWindow_MouseDown);
            this.MouseLeave += new System.EventHandler<System.EventArgs>(this.RenderingWindow_MouseLeave);
            //this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RenderingWindow_FormClosing);
            this.Resize += new System.EventHandler<System.EventArgs>(this.RenderingWindow_Resize);
            this.Mouse.Move += new EventHandler<OpenMobile.Input.MouseMoveEventArgs>(this.RenderingWindow_MouseMove);
            this.Keyboard.KeyUp += new EventHandler<OpenMobile.Input.KeyboardKeyEventArgs>(InputRouter.SourceUp);
            this.Keyboard.KeyDown += new EventHandler<OpenMobile.Input.KeyboardKeyEventArgs>(InputRouter.SourceDown);
            //this.ResizeEnd += new System.EventHandler(RenderingWindow_ResizeEnd);

        }
        #endregion

        private System.Windows.Forms.Timer tmrClosing;
        private System.Windows.Forms.Timer tmrClick;
        private System.Windows.Forms.Timer tmrMouse;
        private System.Windows.Forms.Timer tmrLongClick;
    }
}