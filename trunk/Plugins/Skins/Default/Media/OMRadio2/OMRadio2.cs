﻿/*********************************************************************************
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
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;

namespace OMRadio2
{
    public sealed class OMRadio : HighLevelCode
    {
        public OMRadio()
            : base("OMRadio", OM.Host.getSkinImage("Icons|Icon-Radio"), 1f, "Radio skin v2", "Radio", "OM Dev team", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Queue panels
            //PanelManager.QueuePanel(panelProviders.PanelName, new panelProviders(this).Initialize);

            // Load main panel
            PanelManager.loadPanel(new panelMain(this).Initialize(), true);
            
            return eLoadStatus.LoadSuccessful;
        }
    }
}
