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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Media;


namespace ControlDemo
{
    public static class panelDataSources
    {

        public static OMPanel Initialize()
        {
            OMPanel p = new OMPanel("DataSources");

            OMButton btnTest = new OMButton("btnTest", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, 100, 100);
            btnTest.Image = OM.Host.getSkinImage("Unknown Album");
            btnTest.DataSource_Image = "Screen{:S:}.Zone.MediaInfo.CoverArt";
            p.addControl(btnTest);

            //Screen0.Zone.MediaInfo.CoverArt

            return p;
        }
    }
}
