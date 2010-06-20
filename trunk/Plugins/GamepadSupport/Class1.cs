using OpenMobile.Plugin;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using System;
using System.Collections.Generic;
using OpenMobile;
using OpenMobile.Data;

namespace GamepadSupport
{
    public class Gamepad : IOther
    {
        #region IBasePlugin Members
        private List<Device> joysticks = new List<Device>();
        IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            DeviceList dl = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            while (dl.MoveNext())
            {
                DeviceInstance di = (DeviceInstance)dl.Current;
                if ((di.DeviceType == DeviceType.Gamepad) || (di.DeviceType == DeviceType.Joystick))
                {
                    joysticks.Add(new Device(di.ProductGuid));
                    joysticks[joysticks.Count - 1].Acquire();
                }
            }
            if (joysticks.Count > 0)
            {
                new Thread(new ThreadStart(mainLoop)).Start();
                return OpenMobile.eLoadStatus.LoadSuccessful;
            }
            return eLoadStatus.LoadFailedGracefulUnloadRequested;
        }

        void mainLoop()
        {
            Thread.CurrentThread.IsBackground = true;
            int[] yCount = new int[joysticks.Count];
            int[] xCount = new int[joysticks.Count];
            int[] bCount = new int[joysticks.Count];
            int[] b2Count = new int[joysticks.Count];
            while (!disposed)
            {
                for (int i = 0; i < joysticks.Count; i++)
                {
                    JoyState state = new JoyState(joysticks[i].CurrentJoystickState);
                    if (state.HAT_Y == 1)
                    {
                        if (yCount[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "Up");
                        yCount[i]++;
                    }
                    else if (state.HAT_Y == -1)
                    {
                        if (yCount[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "Down");
                        yCount[i]++;
                    }
                    else
                        yCount[i] = 0;
                    if (state.HAT_X == 1)
                    {
                        if (xCount[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "Right");
                        xCount[i]++;
                    }
                    else if (state.HAT_X == -1)
                    {
                        if (xCount[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "Left");
                        xCount[i]++;
                    }
                    else
                        xCount[i] = 0;
                    if (state.B1 == true)
                    {
                        if (bCount[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "Enter");
                        bCount[i]++;
                    }
                    else
                        bCount[i] = 0;
                    if (state.B2 == true)
                    {
                        if (b2Count[i] % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.goBack, settings[i].Value);
                        b2Count[i]++;
                    }
                    else
                        b2Count[i] = 0;
                    if (state.Y > 0.2)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollUp");
                    else if (state.Y < -0.2)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollDown");
                    if (state.Y > 0.4)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollUp");
                    else if (state.Y < -0.4)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollDown");
                    if (state.Y > 0.8)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollUp");
                    else if (state.Y < -0.8)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollDown");
                    if (state.Y > 0.9)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollUp");
                    else if (state.Y < -0.9)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollDown");
                    if (state.Y == 1.0)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollUp");
                    else if (state.Y == -1.0)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, settings[i].Value, "ScrollDown");
                }
                Thread.Sleep(50);
            }
            for (int i = 0; i < joysticks.Count; i++)
                joysticks[i].Dispose();
        }
        /// <summary>
        /// Easy to use Joystick-state structure
        /// Tailored for the Logitech Freedom 2.4 joystick with 4 axes, 10 buttons and a "hat"
        /// </summary>
        public struct JoyState
        {
            // analog axis in range [-1,+1]
            public float X, Y, R, S;
            // buttons
            public bool B1, B2;
            // joystick hat axes in range {-1, 0, +1}
            public int HAT_X, HAT_Y;

            public JoyState(JoystickState state)
            {
                const float center = 32768.0f - 0.5f;

                // axes in range [-1,+1]
                X = (state.X - center) / center; // positive right
                Y = -(state.Y - center) / center; // positive up
                R = (state.Rz - center) / center; // positive clockwise
                S = -(state.GetSlider()[0] - center) / center; // positive up
                // clamp small values to zero
                if (Math.Abs(X) < 0.09) X = 0;
                if (Math.Abs(Y) < 0.09) Y = 0;
                if (Math.Abs(R) < 0.09) R = 0;
                if (Math.Abs(S) < 0.09) S = 0;

                // buttons
                byte[] buttons = state.GetButtons();
                B1 = buttons[0] >= 128;
                B2 = buttons[1] >= 128;
                //B3 = buttons[2] >= 128;
                //B4 = buttons[3] >= 128;
                //B5 = buttons[4] >= 128;
                //B6 = buttons[5] >= 128;
                //B7 = buttons[6] >= 128;
                //B8 = buttons[7] >= 128;
                //B9 = buttons[8] >= 128;
                //B10 = buttons[9] >= 128;

                // joystick hat state
                int hat = state.GetPointOfView()[0];
                HAT_X = 0; HAT_Y = 0;
                if (hat != -1)
                {
                    int h = hat / 1000;
                    if (h > 0 && h < 18) HAT_X = 1;
                    if (h > 18 && h < 36) HAT_X = -1;
                    if (h > 9 && h < 27) HAT_Y = -1;
                    if (h > 27 || h < 9) HAT_Y = 1;
                }
            }
        }
        Settings settings;
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Gamepad Controller");
                settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
                List<string> screens = generateScreens();
                List<string> values = generateValues();
                using (PluginSettings s = new PluginSettings())
                {
                    string tmp = s.getSetting("Gamepad1.Destination");
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Gamepad1.Destination", "Target", "Select a target for controller 1", screens, values, (tmp == "") ? "0" : tmp));
                    tmp = s.getSetting("Gamepad2.Destination");
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Gamepad2.Destination", "Target", "Select a target for controller 2", screens, values, (tmp == "") ? "0" : tmp));
                    tmp = s.getSetting("Gamepad3.Destination");
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Gamepad3.Destination", "Target", "Select a target for controller 3", screens, values, (tmp == "") ? "0" : tmp));
                    tmp = s.getSetting("Gamepad4.Destination");
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Gamepad4.Destination", "Target", "Select a target for controller 4", screens, values, (tmp == "") ? "0" : tmp));
                }
            }
            return settings;
        }

        void settings_OnSettingChanged(Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
                s.setSetting(setting.Name, setting.Value);
        }

        private List<string> generateValues()
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < 8; i++)
                ret.Add(i.ToString());
            return ret;
        }

        private List<string> generateScreens()
        {
            List<string> ret = new List<string>();
            for (int i = 1; i < 9; i++)
                ret.Add("Screen " + i.ToString());
            return ret;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "Gamepad Support"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Gamepad Controller Support"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new System.NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new System.NotImplementedException();
        }

        #endregion
        bool disposed = false;
        #region IDisposable Members

        public void Dispose()
        {
            disposed = true;
        }

        #endregion
    }
}
