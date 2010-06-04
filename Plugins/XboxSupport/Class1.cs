using OpenMobile.Plugin;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using System;

namespace XboxSupport
{
    public class XBOX:IOther
    {
        #region IBasePlugin Members
        private Device joystick=null;
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            DeviceList dl = Manager.GetDevices(DeviceType.Gamepad, EnumDevicesFlags.AttachedOnly);
            theHost = host;
            while (dl.MoveNext())
            {
                DeviceInstance di = (DeviceInstance)dl.Current;
                if (di.DeviceType == DeviceType.Gamepad)
                {
                    joystick = new Device(di.ProductGuid);
                    joystick.Acquire();

                    new Thread(new ThreadStart(mainLoop)).Start();
                    return OpenMobile.eLoadStatus.LoadSuccessful;
                }
            }
            return OpenMobile.eLoadStatus.LoadFailedGracefulUnloadRequested;
        }

        void mainLoop()
        {
            Thread.CurrentThread.IsBackground = true;
                int yCount = 0;
                int xCount = 0;
                int bCount = 0;
                int b2Count = 0;
                while (!disposed)
                {
                    JoyState state = new JoyState(joystick.CurrentJoystickState);
                    if (state.HAT_Y == 1)
                    {
                        if (yCount % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, "0","Up");
                        yCount++;
                    }
                    else if (state.HAT_Y == -1)
                    {
                        if (yCount % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "Down");
                        yCount++;
                    }
                    else
                        yCount = 0;
                    if (state.HAT_X == 1)
                    {
                        if (xCount % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "Right");
                        xCount++;
                    }
                    else if (state.HAT_X == -1)
                    {
                        if (xCount % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "Left");
                        xCount++;
                    }
                    else
                        xCount = 0;
                    if (state.B1 == true)
                    {
                        if (bCount % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "Enter");
                        bCount++;
                    }
                    else
                        bCount = 0;
                    if (state.B2 == true)
                    {
                        if (b2Count % 5 == 0)
                            theHost.execute(OpenMobile.eFunction.goBack,"0");
                        b2Count++;
                    }
                    else
                        b2Count = 0;
                    if (state.Y>0.2)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollUp");
                    else if(state.Y < -0.2)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollDown");
                    if (state.Y > 0.4)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollUp");
                    else if (state.Y < -0.4)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollDown");
                    if (state.Y > 0.8)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollUp");
                    else if (state.Y < -0.8)
                        theHost.execute(OpenMobile.eFunction.sendKeyPress, "0", "ScrollDown");
                    Thread.Sleep(50);
                }
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
        public Settings loadSettings()
        {
            throw new System.NotImplementedException();
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
            get { return "XboxSupport"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Xbox Support"; }
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
        bool disposed=false;
        #region IDisposable Members

        public void Dispose()
        {
            disposed = true;
        }

        #endregion
    }
}
