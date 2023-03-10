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
using System.Collections.Generic;

namespace OMSkinPluginTest
{
    public sealed class OMSkinPluginTest : HighLevelCode
    {
        public OMSkinPluginTest()
            : base("OMSkinPluginTest", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "A test plugin", "OMSkinPluginTest", "OM Dev team/Borte", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Create a new panel
            OMPanel panel = new OMPanel("Panel");

            // Load the panel into the local manager for panels
            base.PanelManager.loadPanel(panel, true);


            // Use this to goto your local panels:
            // base.GotoPanel(screen, panelName);

            // Use this to show additional local panels:
            // base.ShowPanel(screen, panelName);

            // Return
            return eLoadStatus.LoadSuccessful;
        }

    }
}
