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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Controls;

namespace OMSkinPluginSample
{
    public sealed class OMSkinPluginSample : HighLevelCode
    {
        public OMSkinPluginSample()
            : base("OMSkinPluginSample", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "A sample plugin for a simple panel", "Panel Sample", "OM Dev team/Borte", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Create a new panel
            OMPanel panel = new OMPanel("Panel");

            //// Create a new label
            //OMLabel lblHelloWorld = new OMLabel("lblHelloWorld", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Width, OM.Host.ClientArea[0].Height, "Hello World!");
            
            //// Add the new label to the panel
            //panel.addControl(lblHelloWorld);

            OMButton btnPlay = OMButton.PreConfigLayout_BasicStyle("btnPlay", 0, 100, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Play");
            panel.addControl(btnPlay);

            // Load the panel into the local manager for panels
            PanelManager.loadPanel(panel, true);

            // Return
            return eLoadStatus.LoadSuccessful;
        }
    }
}
