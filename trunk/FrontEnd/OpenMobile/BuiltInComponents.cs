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
using OpenMobile.Controls;

namespace OpenMobile
{
    public static class BuiltInComponents
    {
        public static OMPanel AboutPanel()
        {
            //WARNING: REMOVING ANY OF THE BELOW DESCRIPTION IS A VIOLATION OF THE LICENSE AGREEMENT
            OMPanel p=new OMPanel("About");
            OMLabel description=new OMLabel(30,40,900,550);
            description.TextAlignment = Alignment.TopCenter;
            description.Text="OpenMobile is copyright the openMobile Foundation and its contributors\r\n\r\n";
            description.Text += "This program in full or in part is protected under a clarified version of the GPLv3 license which can be found in the application directory.\r\n\r\n";
            description.Text += "Contributors:\r\n";
            description.Text += "Justin Domnitz (justchat_1) - Lead Developer\r\n";
            description.Text += "UnusuallyGenius - Graphics Designer\r\n";
            description.Text += "ws6vert - openOBD and Garmin Mobile PC Projects\r\n";
            description.Text += "Borte - Developer\r\n";
            description.Text += "Extide - Developer\r\n";
            description.Text += "\r\nSupporting Projects:\r\n";
            description.Text += "TagLib Sharp\r\nThe Mono Project\r\niPod Sharp\r\nDBusSharp\r\nSQLite\r\nAqua Gauge";
            p.addControl(description);
            return p;
        }
    }
}
