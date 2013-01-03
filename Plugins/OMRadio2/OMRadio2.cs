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
using OpenMobile;
using OpenMobile.Threading;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;

namespace OMRadio2
{
    [SkinIcon("*»")] //"*»"
    public class OMRadio2 : IHighLevel
    {
        private ScreenManager manager;
        private IPluginHost theHost;
        private enum StationListSources { Live, Presets }
        private StationListSources StationListSource = StationListSources.Live;
        private List<stationInfo> Presets = new List<stationInfo>();
        private Settings settings;
        private bool SourceSelected = false;

        #region IHighLevel Members

        public OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen, name];
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string displayName
        {
            get { return "Radio2"; }
        }

        #endregion

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Bjørn Morten Orderløkken"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMRadio2"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Radio control panel"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-RadioWaves"); }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);


            OMPanel panelRadio = new OMPanel("Radio");

            #region Bottom menu buttons

            // Calculate where to place the buttons based on amount of buttons needed
            const int MenuButtonCount = 1;
            const int MenuButtonWidth = 160;
            int MenuButtonStartLocation = 500 - ((MenuButtonWidth * MenuButtonCount) / 2);

            OMButton[] Button_BottomBar = new OMButton[MenuButtonCount];
            for (int i = 0; i < Button_BottomBar.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "5", "");
                        //Button_BottomBar[i].OnClick += new userInteraction(List_Zones_OnLongClick);
                        break;
                    case 1:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "", "Activate");
                        //Button_BottomBar[i].OnClick += new userInteraction(Button_BottomBar_Activate_OnClick);
                        break;
                    case 2:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "", "Restore");
                        //Button_BottomBar[i].OnClick += new userInteraction(Button_BottomBar_Restore_OnClick);
                        break;
                    default:
                        break;
                }

                panelRadio.addControl(Button_BottomBar[i]);
            }

            #endregion


            manager.loadPanel(panelRadio, true);


            return eLoadStatus.LoadSuccessful;
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
