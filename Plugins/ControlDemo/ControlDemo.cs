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
using System.Timers;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Windows.Forms;

namespace ControlDemo
{
    public sealed class ControlDemo: IHighLevel
    {

    #region IHighLevel Members
    OMPanel p = new OMPanel();
    ScreenManager manager;
    IPluginHost theHost;

    public eLoadStatus initialize(IPluginHost host)
    {
        theHost = host;
        theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
        OMImage img = new OMImage(0,0,1000,600);
        img.Image = new imageItem(new Bitmap(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height));
        OMButton embed = new OMButton(350, 550, 250, 40);
        embed.Image = theHost.getSkinImage("Full");
        embed.Text = "Embed GMPC";
        embed.OnClick += new userInteraction(embed_OnClick);
        p.addControl(embed);
        p.addControl(img);
        p.BackgroundColor1 = Color.FromArgb(180, Color.Red);
        p.BackgroundColor2 = Color.Transparent;
        p.BackgroundType = backgroundStyle.Gradiant;
        manager = new ScreenManager(theHost.ScreenCount);
        manager.loadPanel(p);
        return eLoadStatus.LoadSuccessful;
    }

    void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
    {
        if ((function == eFunction.TransitionFromAny) && embedded == true)
            embed_OnClick(this, int.Parse(arg1));
        if ((function == eFunction.TransitionToPanel) && (embedded == true))
        {
            if (arg2 == "ControlDemo")
                return;
            int i=int.Parse(arg1);
            OMImage img=((OMImage)manager[i][1]);
            Graphics g = Graphics.FromImage(img.Image.image);
            Form f=(Form)Form.FromHandle(theHost.UIHandle(i));
            g.CopyFromScreen(0,0,0,0, Screen.PrimaryScreen.Bounds.Size,CopyPixelOperation.SourceCopy);
            g.Dispose();
            embed_OnClick(this, int.Parse(arg1));
        }
    }
    bool embedded = false;
    void embed_OnClick(object sender, int screen)
    {
        if (embedded==true)
        {
            embedded=!OpenMobile.Framework.OSSpecific.unEmbedApplication("Que",screen);
            return;
        }
        embedded=OpenMobile.Framework.OSSpecific.embedApplication("Que", screen,theHost,new Rectangle(0,99, 1000, 501));
    }
    void b8_OnClick(object sender, int screen)
    {
        theHost.execute(eFunction.TransitionFromPanel,screen.ToString(), "ControlDemo");
        theHost.execute(eFunction.TransitionToPanel,screen.ToString(), "MainMenu");
        theHost.execute(eFunction.ExecuteTransition,screen.ToString());
    }
    public OMPanel loadPanel(string name,int screen)
    {
        embed_OnClick(this, screen);
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
