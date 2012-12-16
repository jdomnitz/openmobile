#region GPL
//    This file is part of OpenMobile.

//    OpenMobile is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    OpenMobile is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with OpenMobile.  If not, see <http://www.gnu.org/licenses/>.

//Copyright 2012 Jonathan Heizer jheizer@gmail.com
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;

namespace OMOBD2
{
    public class omobd2 : IRawHardware
    {
        private ObdLibSharp.ObdThread _obd;
        private Dictionary<string, SensorWrapper> _Sensors;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            //_obd = new ObdLibSharp.ObdThread();
            
            ////for now;
            //try
            //{
            //    _obd.setPort("COM7");
            //    _obd.setBaud(11500);
            //    //_obd.connect();

            //    _obd.pidReply += new ObdLibSharp.ObdThread.pidReplyDelegate(_obd_pidReply);
            //}
            //catch (Exception ex)
            //{
            //}

            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void _obd_pidReply(string mode, string val, int set, double time)
        {
            if (_Sensors != null)
            {
                SensorWrapper Sen = _Sensors[mode];

                if (Sen != null)
                {
                    Sen.UpdateSensorValue(val);
                }
            }
        }


        public Settings loadSettings()
        {
            return null;
        }

        public List<Sensor> getAvailableSensors(eSensorType type)
        {
            if (_Sensors != null)
            {
                _Sensors = new Dictionary<string, SensorWrapper>();

                _Sensors.Add("03", new SensorWrapper(this.pluginName + ".FuelStatus" ,"03" , eSensorDataType.raw, null));
                _obd.addModePidReq("01", "03");

                _Sensors.Add("04", new SensorWrapper(this.pluginName + ".Load" ,"04", eSensorDataType.percent , null));
                _obd.addModePidReq("01", "04");

                _Sensors.Add("05", new SensorWrapper(this.pluginName + ".Temp", "05", eSensorDataType.degreesC , null));
                _obd.addModePidReq("01", "05");

            }

            return Sensors.sensorWrappersToSensors(new List<OpenMobile.Plugin.SensorWrapper>(_Sensors.Values));

        }
      
        public string authorName
        {
            get { return "Jon Heizer"; }
        }

        public string authorEmail
        {
            get { return "jheizer@gmail.com"; }
        }

        public string pluginName
        {
            get { return "OMOBD2"; }
        }

        public float pluginVersion
        {
            get { return 1.0f; }
        }

        public string pluginDescription
        {
            get { return "OBD2 adapter support"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public void resetDevice()
        {
        }

        public string deviceInfo
        {
            get { return "ODB2 Adapter"; }
        }

        public string firmwareVersion
        {
            get { return ""; }
        }
        public void Dispose()
        {
            if(_obd != null)
                _obd.disconnect();
        }
    }
}
