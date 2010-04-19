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
using System.IO;
using System.Reflection;
using System.Xml;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Converts between an object and an xml stream
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Convert a filepath to an OMPanel
        /// </summary>
        /// <param name="file">The File</param>
        /// <param name="host">The Plugin Host</param>
        /// <returns></returns>
        public static OMPanel deserializePanel(string file,IPluginHost host)
        {
            FileStream f = File.OpenRead(file);
            OMPanel p=deserializePanel(f, host);
            f.Close();
            return p;
        }
        /// <summary>
        /// Convert a file stream to an OMPanel
        /// </summary>
        /// <param name="s">The stream</param>
        /// <param name="host">The plugin host for image handling</param>
        public static OMPanel deserializePanel(Stream s, IPluginHost host)
        {
            OMPanel p = new OMPanel();
            XmlTextReader reader = new XmlTextReader(s);
            while (reader.NodeType != XmlNodeType.Element)
                reader.Read();
            if (reader.Name == "ControlCollection")
                reader.Read();
            Type t = Type.GetType("OpenMobile.Controls." + reader.Name);
            object o = Activator.CreateInstance(t);
            PropertyInfo info = t.GetProperty("TypeName");
            while (reader.Read())
            {
                if ((reader.Depth == 1) && (reader.NodeType == XmlNodeType.Element))
                {
                    t = Type.GetType("OpenMobile.Controls." + reader.Name);
                    o = Activator.CreateInstance(t);
                }
                else if ((reader.Depth == 2) && (reader.NodeType == XmlNodeType.Element))
                {
                    info = t.GetProperty(reader.Name);
                }
                else if ((reader.NodeType == XmlNodeType.Text) && (reader.Depth == 3)&&(info!=null))
                {
                    if (info.CanWrite == true)
                    {
                        if (info.PropertyType.BaseType == typeof(Enum))
                            info.SetValue(o, Enum.Parse(info.PropertyType, reader.Value), null);
                        else if (info.PropertyType == typeof(int))
                            info.SetValue(o, int.Parse(reader.Value), null);
                        else if (info.PropertyType == typeof(float))
                            info.SetValue(o, float.Parse(reader.Value), null);
                        else if (info.PropertyType == typeof(double))
                            info.SetValue(o, double.Parse(reader.Value), null);
                        else if (info.PropertyType == typeof(Color))
                        {
                            string[] c=reader.Value.Split(new char[]{','});
                            if (c.Length==1)
                                info.SetValue(o,Color.FromName(c[0]),null);
                            else
                                info.SetValue(o, Color.FromArgb(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), int.Parse(c[3])), null);
                        }
                        else if (info.PropertyType == typeof(Font))
                        {
                            string[] f = reader.Value.Split(new char[] { ',' });
                            info.SetValue(o, new Font(f[0], float.Parse(f[1]), (FontStyle)Enum.Parse(typeof(FontStyle), f[2])), null);
                        }
                        else
                            try
                            {
                                if (reader.Value == "##Null##")
                                    info.SetValue(o, null, null);
                                else if (info.PropertyType == typeof(imageItem))
                                {
                                    imageItem item = host.getSkinImage(reader.Value);
                                    info.SetValue(o, item, null);
                                }
                                else
                                    info.SetValue(o, reader.Value, null);
                            }
                            catch (ArgumentException) { }
                    }
                }
                else if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Depth == 1))
                {
                    p.addControl((OMControl)o);
                }
            }
            return p;
        }
    }
}
