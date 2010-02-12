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
using System.Drawing;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace ControlDemo
{
    public sealed class Class1: IHighLevel
    {

    #region IHighLevel Members
    OMPanel p = new OMPanel();
    ScreenManager manager;
    IPluginHost theHost;

    public eLoadStatus initialize(IPluginHost host)
    {
        theHost = host;
        OMAnimatedLabel prog = new OMAnimatedLabel();
        prog.Top=400;
        prog.Left=100;
        prog.Width=250;
        prog.Height=30;
        prog.Text = "Im a sample string";
        prog.ContiuousAnimation = eAnimation.GlowPulse;
        prog.OutlineColor = Color.Red;
        OMImage img = new OMImage(700,200,125,100);
        img.Image = new imageItem(Widget.generate("Media", host));
        OMProgress tb = new OMProgress(200, 100, 50, 100);
        tb.Value = 60;
        tb.Vertical = true;
        OMCheckbox ch = new OMCheckbox();
        ch.Left = 100;
        ch.Top = 200;
        ch.Width = 225;
        ch.Height = 30;
        ch.Text = "Im a checkbox";
        OMButton embed = new OMButton(450, 100, 250, 40);
        embed.Image = theHost.getSkinImage("Full");
        embed.Text = "Embed Notepad";
        embed.OnClick += new userInteraction(embed_OnClick);
        OMList list = new OMList();
        list.Left = 400;
        list.Top = 200;
        list.Width = 150;
        list.Height = 230; //235
        list.Font=new Font("Times New Roman",24F);
        list.Add("a");
        list.Add("b");
        list.Add("c");
        list.Add("d");
        list.Add("e");
        list.Add("f");
        list.Add("g");
        list.Add("h");
        list.Add("i");
        list.Add("j");
        list.Add("a");
        list.Add("b");
        list.Add("c");
        list.Add("d");
        list.Add("e");
        list.Add("f");
        list.Add("g");
        list.Add("h");
        list.Add("i");
        list.Add("j");
        list.ListStyle = eListStyle.ImageList;
        for (int i = 0; i < list.Count; i++)
            list[i].image = theHost.getSkinImage("Play").image;
        OMSlider slider = new OMSlider();
        slider.Left = 20;
        slider.Height = 20;
        slider.Width = 500;
        slider.Top = 470;
        slider.SliderBarHeight = 10;
        slider.SliderWidth = 35;
        slider.Slider = theHost.getSkinImage("Slider");
        slider.SliderBar = theHost.getSkinImage("Slider.Bar");
        p.addControl(ch);
        p.addControl(embed);
        p.addControl(prog);
        p.addControl(list);
        p.addControl(slider);
        p.addControl(tb);
        p.addControl(img);
        p.BackgroundColor1 = Color.FromArgb(180, Color.Red);
        p.BackgroundColor2 = Color.Transparent;
        p.BackgroundType = backgroundStyle.Gradiant;
        manager = new ScreenManager(theHost.ScreenCount);
        manager.loadPanel(p);
        return eLoadStatus.LoadSuccessful;
    }
    bool embedded = false;
    void embed_OnClick(object sender, int screen)
    {
        if (embedded==true)
        {
            OpenMobile.Framework.OSSpecific.unEmbedApplication("Notepad",screen);
            return;
        }
        embedded=OpenMobile.Framework.OSSpecific.embedApplication("Notepad", screen,theHost,new Rectangle(-50,-10, 400, 400));
    }
    void b8_OnClick(object sender, int screen)
    {
        theHost.execute(eFunction.TransitionFromPanel,screen.ToString(), "ControlDemo");
        theHost.execute(eFunction.TransitionToPanel,screen.ToString(), "MainMenu");
        theHost.execute(eFunction.ExecuteTransition,screen.ToString());
    }
    public OMPanel loadPanel(string name,int screen)
    {
        return manager[screen];
    }
    public OMPanel loadSettings(string name,int screen)
    {
        throw new NotImplementedException();
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
        p = null;
    }

    #endregion
    }
}
