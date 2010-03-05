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
using System.Collections.Generic;
using System.Drawing;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Handles generation and storage of widgets
    /// </summary>
    public static class Widget
    {
        private static List<imageItem> cache;
        /// <summary>
        /// Generate a widget image
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Image generate(string pluginName, IPluginHost host)
        {
            if (cache==null)
                cache=new List<imageItem>();
            object b;
            host.getData(eGetData.GetPlugins, pluginName,out b);
            OMPanel p = ((IHighLevel)b).loadPanel("Widget", 0);
            if (p == null)
                return null;
            Image img;
            if (cache.Exists(x => x.name == pluginName) == true)
                img = cache.Find(x => x.name == pluginName).image;
            else
                img = new Bitmap(1000, 600);
            Graphics g;
            try
            {
                g = Graphics.FromImage(img);
            }
            catch (System.InvalidOperationException) { return null; }
            g.Clear(Color.Transparent);
            renderingParams param=new renderingParams();
            for (int i = 0; i < p.controlCount;i++ )
            {
                p[i].Render(g, param);
            }
            g.Dispose();
            cache.Add(new imageItem(img, pluginName));
            return img;
        }
    }
}
