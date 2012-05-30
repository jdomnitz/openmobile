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
    
    Copyright 2012 Jonathan Heizer jheizer@gmail.com
*********************************************************************************/
using System;
using System.Text;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Collections.Generic;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Helper functions for the sensor framework
    /// </summary>
    public static class Sensors
    {
        /// <summary>
        /// Retrieve the specific plugin
        /// </summary>
        /// <param name="Name">Name of the plugin you want</param>
        /// <returns></returns>
        public static Sensor getPluginByName(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return null;

            System.Collections.Generic.List<OpenMobile.Plugin.Sensor> sensors = (System.Collections.Generic.List<OpenMobile.Plugin.Sensor>)BuiltInComponents.Host.getData(eGetData.GetAvailableSensors, "");
            if (sensors == null)
                return null;
            
            Sensor sensor = sensors.Find(s => s.Name == Name);

            return sensor;
        }

        public static List<Sensor> sensorWrappersToSensors(List<SensorWrapper> SensorWrappers)
        {
            List<Sensor> Sensors = new List<Sensor>();
            if (SensorWrappers != null)
                foreach(SensorWrapper wrap in SensorWrappers)
                    Sensors.Add(wrap.sensor);

            return Sensors;
        }
    }
}
