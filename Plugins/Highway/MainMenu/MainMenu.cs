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
using MainMenu;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace OpenMobile
{
    public class MainMenu:IHighLevel
    {
        #region IHighLevel Members

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return manager[screen];
        }

        public string displayName
        {
            get { return "Main Menu"; }
        }

        #endregion

        #region IBasePlugin Members
        ScreenManager manager;
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost=host;
            manager = new ScreenManager(theHost.ScreenCount);
            OMPanel p = new OMPanel("");
            MainMenuButton t1 = new MainMenuButton(146, 143);
            t1.Image = theHost.getSkinImage("HomeButton");
            t1.FocusImage = theHost.getSkinImage("HomeButton_HL");
            t1.DownImage = theHost.getSkinImage("HomeButton_Selected");
            t1.Icon = theHost.getSkinImage("Icons|Music");
            t1.Text = "Music";
            t1.OnClick += new userInteraction(t1_OnClick);
            MainMenuButton t2 = new MainMenuButton(321, 143);
            t2.Image = theHost.getSkinImage("HomeButton");
            t2.FocusImage = theHost.getSkinImage("HomeButton_HL");
            t2.DownImage = theHost.getSkinImage("HomeButton_Selected");
            t2.Icon = theHost.getSkinImage("Icons|Video");
            t2.Text = "Video";
            MainMenuButton t3 = new MainMenuButton(496, 143);
            t3.Image = theHost.getSkinImage("HomeButton");
            t3.FocusImage = theHost.getSkinImage("HomeButton_HL");
            t3.DownImage = theHost.getSkinImage("HomeButton_Selected");
            t3.Icon = theHost.getSkinImage("Icons|Radio");
            t3.Text = "Radio";
            MainMenuButton t4 = new MainMenuButton(671, 143);
            t4.Image = theHost.getSkinImage("HomeButton");
            t4.FocusImage = theHost.getSkinImage("HomeButton_HL");
            t4.DownImage = theHost.getSkinImage("HomeButton_Selected");
            t4.Icon = theHost.getSkinImage("Icons|Facebook");
            t4.Text = "Facebook";
            MainMenuButton b1 = new MainMenuButton(146, 303);
            b1.Image = theHost.getSkinImage("HomeButton");
            b1.FocusImage = theHost.getSkinImage("HomeButton_HL");
            b1.DownImage = theHost.getSkinImage("HomeButton_Selected");
            b1.Icon = theHost.getSkinImage("Icons|Email_New");
            b1.Text = "Email";
            MainMenuButton b2 = new MainMenuButton(321, 303);
            b2.Image = theHost.getSkinImage("HomeButton");
            b2.FocusImage = theHost.getSkinImage("HomeButton_HL");
            b2.DownImage = theHost.getSkinImage("HomeButton_Selected");
            b2.Icon = theHost.getSkinImage("Icons|Weather");
            b2.Text = "Weather";
            MainMenuButton b3 = new MainMenuButton(496, 303);
            b3.Image = theHost.getSkinImage("HomeButton");
            b3.FocusImage = theHost.getSkinImage("HomeButton_HL");
            b3.DownImage = theHost.getSkinImage("HomeButton_Selected");
            b3.Icon = theHost.getSkinImage("Icons|Webcam");
            b3.Text = "Webcam";
            MainMenuButton b4 = new MainMenuButton(671, 303);
            b4.Image = theHost.getSkinImage("HomeButton");
            b4.FocusImage = theHost.getSkinImage("HomeButton_HL");
            b4.DownImage = theHost.getSkinImage("HomeButton_Selected");
            b4.Icon = theHost.getSkinImage("Icons|Settings");
            b4.Text = "Swap Skin";
            //TODO-Remove
            b4.OnClick += new userInteraction(b4_OnClick);
            p.addControl(t1);
            p.addControl(t2);
            p.addControl(t3);
            p.addControl(t4);
            p.addControl(b1);
            p.addControl(b2);
            p.addControl(b3);
            p.addControl(b4);
            manager.loadPanel(p);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void t1_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Music"))
            {
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }
        }

        void b4_OnClick(OMControl sender, int screen)
        {
            if (theHost.SkinPath.EndsWith("Default"))
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Highway");
            }
            else
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Default");
            }
            theHost.execute(OpenMobile.eFunction.restartProgram);
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { throw new NotImplementedException(); }
        }

        public string pluginName
        {
            get { return "MainMenu"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "The Main Menu"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
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
