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
using OpenMobile.Graphics;
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.helperFunctions;

namespace ControlDemo
{
    [SkinIcon("*#")]
    public sealed class ControlDemo : IHighLevel
    {

    #region IHighLevel Members
    ScreenManager manager;
    IPluginHost theHost;
    public eLoadStatus initialize(IPluginHost host)
    {
        OMPanel p = new OMPanel("");
        theHost = host;
        OMAnimatedLabel label1 = new OMAnimatedLabel(50, 100, 200, 30);
        label1.Text = "This is an example of scrolling text how well does it work";
        label1.ContiuousAnimation = eAnimation.BounceScroll;
        OMAnimatedLabel label2 = new OMAnimatedLabel(50, 200, 200, 30);
        label2.Text = label1.Text;
        label2.ContiuousAnimation = eAnimation.Scroll;
        label2.Format = eTextFormat.Glow;
        label2.OutlineColor = Color.Blue;
        OMAnimatedLabel label3 = new OMAnimatedLabel(50, 250, 200, 30);
        label3.Text = "Unveil Me Right";
        label3.ContiuousAnimation = eAnimation.UnveilRight;
        OMAnimatedLabel label4 = new OMAnimatedLabel(50, 300, 200, 30);
        label4.Text = "Unveil Me Left";
        label4.ContiuousAnimation = eAnimation.UnveilLeft;
        OMAnimatedLabel label5 = new OMAnimatedLabel(50, 350, 200, 30);
        label5.Text = "This is the starting text";
        ReflectedImage gauge = new ReflectedImage();
        gauge.Left = 300;
        gauge.Top = 150;
        gauge.Width = 300;
        gauge.Height = 200;
        gauge.Image=theHost.getSkinImage("OMGenius");
        OMButton button = new OMButton(50, 400, 200, 50);
        button.Text = "Toggle Buffer";
        button.Image = imageItem.MISSING;
        button.OnClick += new userInteraction(button_OnClick);
        p.addControl(label5);
        p.addControl(button);
        p.addControl(gauge);

        OMButton btnDialog = new OMButton(450, 450, 200, 50);
        btnDialog.Name = "btnDialog";
        btnDialog.Text = "Dialog";
        btnDialog.Image = imageItem.MISSING;
        btnDialog.OnClick += new userInteraction(btnDialog_OnClick);
        p.addControl(btnDialog);

        OMLabel lblTtest = new OMLabel(300, 200, 200, 50);
        lblTtest.Name = "Test";
        lblTtest.Text = "Test";
        lblTtest.SkinDebug = true;
        p.addControl(lblTtest);

        OMButton OK = new OMButton(200, 200, 200, 110);
        OK.Image = theHost.getSkinImage("Full");
        OK.FocusImage = theHost.getSkinImage("Full.Highlighted");
        OK.Text = "OK";
        OK.Name = "OK";
        OK.Transition = eButtonTransition.None;
        OK.SkinDebug = true;
        OK.OnClick += new userInteraction(OK_OnClick);
        p.addControl(OK);

        OMImage AnimatedImage = new OMImage();
        AnimatedImage.Name = "Ani";
        AnimatedImage.Left = 600;
        AnimatedImage.Top = 150;
        AnimatedImage.Image = theHost.getSkinImage("OM");
        AnimatedImage.Width = AnimatedImage.Image.image.Width;
        AnimatedImage.Height = AnimatedImage.Image.image.Height;
        AnimatedImage.Animate = AnimatedImage.CanAnimate;   // Activate animation if possible
        p.addControl(AnimatedImage);

        OMList List1 = new OMList(550,200,200,200);
        List1.Name = "List1";
        List1.ListStyle = eListStyle.MultiList;
        List1.Add(new OMListItem("gggg"));
        List1.Add(new OMListItem("gg"));
        List1.Add(new OMListItem("bbbb"));
        List1.Add(new OMListItem("cccc"));
        List1.Add(new OMListItem("cc"));
        List1.Add(new OMListItem("dddd"));
        List1.Add(new OMListItem("ffff"));
        List1.Add(new OMListItem("ff"));

        OMListItem ListItem = new OMListItem("Line1", "linemulti");
        ListItem.image = theHost.getSkinImage("OM").image;
        List1.Add(ListItem);

        List1.SelectedIndexChanged += new OMList.IndexChangedDelegate(List1_SelectedIndexChanged);
        p.addControl(List1);
        OMList List2 = new OMList(750, 200, 200, 200);
        List2.Name = "List2";
        p.addControl(List2);

        System.Timers.Timer t = new System.Timers.Timer(100);
        t.Elapsed += new ElapsedEventHandler(t_Elapsed);
        t.Enabled = true;
        manager = new ScreenManager(theHost.ScreenCount);
        p.Entering += new PanelEvent(p_Entering);
        p.Leaving += new PanelEvent(p_Leaving);
        manager.loadPanel(p);

        return eLoadStatus.LoadSuccessful;
    }

    void List1_SelectedIndexChanged(OMList sender, int screen)
    {
        OMList List1 = (OMList)manager[screen]["List1"];
        OMList List2 = (OMList)manager[screen]["List2"];
        string find = List1.SelectedItem.text.Substring(0, 1);
        List2.Items = List1.Items.FindAll(a => a.text.StartsWith(find)).ConvertAll(x => (OpenMobile.OMListItem)x.Clone());
    }

    void OK_OnClick(OMControl sender, int screen)
    {
        OMButton btn = (OMButton)manager[screen]["btnDialog"];
        btn.Image = theHost.getSkinImage("Full");
    }


    void p_Leaving(OMPanel sender, int screen)
    {
        theHost.sendMessage("UI", "", "{" + screen + "}Leaving panel :" + sender.Name);
    }

    void p_Entering(OMPanel sender, int screen)
    {
        theHost.sendMessage("UI", "", "{" + screen + "}Entering panel :" + sender.Name);
    }

    int frame;
    void btnDialog_OnClick(OMControl sender, int screen)
    {
        OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this.pluginName, sender.Parent.Name);
        dialog.Header = "Radio message";
        dialog.Text = "Is this awesome?";
        dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Busy;
        dialog.Button = OpenMobile.helperFunctions.Forms.buttons.Yes |
                            OpenMobile.helperFunctions.Forms.buttons.No;

        theHost.sendMessage("UI", "", "{" + screen + "}MsgBox result: " + dialog.ShowMsgBox(screen).ToString());
    }

    void t_Elapsed(object sender, ElapsedEventArgs e)
    {
        //((OMGauge)manager[0][2]).Value = OpenMobile.Framework.Math.Calculation.RandomNumber(0, 20);
    }

    void button_OnClick(OMControl sender, int screen)
    {
        OMGauge g = ((OMGauge)manager[screen][2]);
        if (g.BufferSize == 5)
            g.BufferSize = 0;
        else
            g.BufferSize = 5;
    }

    public OMPanel loadPanel(string name,int screen)
    {
        return manager[screen, name];
    }
    public Settings loadSettings()
    {
        return null;
    }
    #endregion
    #region IBasePlugin Members

    public string authorName
    {
        get { return "Justin Domnitz"; }
    }

    public string authorEmail
    {
        get { return "admin@domnitzsolutions.com"; }
    }

    public string pluginName
    {
        get { return "ControlDemo"; }
    }
    public string displayName
        {
            get { return "Control Demo"; }
        }
    public float pluginVersion
    {
        get { return 0.1F; }
    }

    public string pluginDescription
    {
        get { return "Demonstrate inter-panel transitions and the larger control set"; }
    }

    public bool incomingMessage(string message, string source)
    {
        throw new NotImplementedException();
    }
    public bool incomingMessage<T>(string message, string source, ref T data)
    {
        throw new NotImplementedException();
    }
    #endregion
    #region IDisposable Members

    public void Dispose()
    {
        //
    }

    #endregion
    }
}
