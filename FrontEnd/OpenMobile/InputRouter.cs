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
using OpenMobile.Input;
using OpenMobile.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;

namespace OpenMobile
{
    public static class InputRouter
    {
        public const string DefaultUnit = "OS Default";
        public const string DisabledUnit = "Disabled";

        private static Thread thInput;
        private static bool thInput_Run;

        /// <summary>
        /// Time for detecting when OM is idle
        /// </summary>
        private static Timer[] tmrIdleDetection;

        /// <summary>
        /// Enabled/disabled state for idle detection per screen
        /// </summary>
        private static bool[] _IdleDetectionEnabled;

        /// <summary>
        /// The idle state per screen
        /// </summary>
        private static bool[] _Idle;

        public static void Dispose()
        {
            for (int i = 0; i < tmrIdleDetection.Length; i++)
			{
                if (tmrIdleDetection[i] != null)
                    tmrIdleDetection[i].Dispose();
			}            

            // Unhide default OS mouse
            Core.RenderingWindows[0].CursorVisible = true;

            thInput_Run = false;
        }

        public static void Initialize()
        {
            // Init openTK
            OpenTK.ToolkitOptions toolkitOptions = OpenTK.ToolkitOptions.Default;
            toolkitOptions.Backend = OpenTK.PlatformBackend.PreferNative;
            toolkitOptions.EnableHighResolution = false;
            OpenTK.Toolkit.Init(toolkitOptions);

            // Init variables
            _Mice_MappedIndex = new int[OM.Host.ScreenCount];
            for (int i = 0; i < _Mice_MappedIndex.Length; i++)
                _Mice_MappedIndex[i] = -1;

            // Idle detection
            tmrIdleDetection = new Timer[BuiltInComponents.Host.ScreenCount];
            int IdleDetectionInterval = BuiltInComponents.SystemSettings.IdleDetectionInterval;
            if (IdleDetectionInterval > 0)
            {
                for (int i = 0; i < tmrIdleDetection.Length; i++)
                {
                    tmrIdleDetection[i] = new Timer(IdleDetectionInterval * 1000);
                    tmrIdleDetection[i].Screen = i;
                    tmrIdleDetection[i].Tag = IdleDetectionState.Normal;
                    tmrIdleDetection[i].Elapsed += new System.Timers.ElapsedEventHandler(tmrIdleDetection_Elapsed);
                }
            }
            _Idle = new bool[BuiltInComponents.Host.ScreenCount];
            _IdleDetectionEnabled = new bool[BuiltInComponents.Host.ScreenCount];
            for (int i = 0; i < _IdleDetectionEnabled.Length; i++)
                _IdleDetectionEnabled[i] = true;

            //thInput_Run = true;
            //thInput = new Thread(Thread_InputHandler);
            //thInput.IsBackground = true;
            //thInput.Start();

            // Connect events
            for (int i = 0; i < Core.theHost.ScreenCount; i++)
            {
                // Connect mouse events
                Core.RenderingWindows[i].MouseDown += InputRouter_MouseDown;
                Core.RenderingWindows[i].MouseUp += InputRouter_MouseUp;
                Core.RenderingWindows[i].MouseMove += InputRouter_MouseMove;

                // Connect keyboard events
                Core.RenderingWindows[i].KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(SourceDown);
                Core.RenderingWindows[i].KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(SourceUp);
                Core.RenderingWindows[i].KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(SourcKeyPress);
            }

            DataSources_Register();
            Commands_Register();

            // Raise system event, informing that input router has completed
            BuiltInComponents.Host.raiseSystemEvent(eFunction.inputRouterInitialized, "1", "1", String.Empty);
        }

        static void InputRouter_MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            RenderingWindow window = sender as RenderingWindow;

            MouseButton mb = MouseButton.None;
            if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Left;
            else if (e.Mouse.RightButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Right;
            else if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Middle;

            MouseMoveEventArgs eOM = new MouseMoveEventArgs(e.Position.X, e.Position.Y, 0, 0, mb);
            window.RenderingWindow_MouseMove(window.Screen, eOM);
        }

        static void InputRouter_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            RenderingWindow window = sender as RenderingWindow;
            
            MouseButton mb = MouseButton.None;
            if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Left;
            else if (e.Mouse.RightButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Right;
            else if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Middle;

            MouseButtonEventArgs eOM = new MouseButtonEventArgs(e.Position.X, e.Position.Y, mb, false);
            window.RenderingWindow_MouseUp(window.Screen, eOM);
        }

        static void InputRouter_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            RenderingWindow window = sender as RenderingWindow;

            MouseButton mb = MouseButton.None;
            if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Left;
            else if (e.Mouse.RightButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Right;
            else if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                mb = MouseButton.Middle;

            MouseButtonEventArgs eOM = new MouseButtonEventArgs(e.Position.X, e.Position.Y, mb, false);
            window.RenderingWindow_MouseDown(window.Screen, eOM);
        }

        private static OpenTK.Input.MouseState[] _MouseStates;
        private static OpenTK.Input.MouseState[] _MouseStates_Stored;
        private static int[] _Mice_MappedIndex;
        private static Point?[] _MousePoint_Initial;
        private static PointF[] _Mouse_ScaleValues;
        private static int _Mouse_MaxDeviceCount = 20;
        private static OpenTK.Input.MouseState _MouseCursorState_Stored;

        private static OpenTK.Input.MouseState _MouseStates_Tmp_Main;
        private static OpenTK.Input.MouseState[] _MouseStates_Tmp;
        private static OpenTK.Input.KeyboardState[] _KeyboardStates_Tmp;
        private static OpenTK.Input.KeyboardState _KeyboardState_Stored;
        private static int _Keyboard_MaxDeviceCount = 20;

        private enum EventTypes
        {
            Unknown,
            Move,
            Down,
            Up
        }
        private static EventTypes _PreviousEventType = EventTypes.Unknown;
        private static int _LastReport = -1;
        private static void Thread_InputHandler()
        {
            _MouseStates = new OpenTK.Input.MouseState[OM.Host.ScreenCount];
            _MouseStates_Stored = new OpenTK.Input.MouseState[_MouseStates.Length];
            _MousePoint_Initial = new Point?[_MouseStates.Length];
            _Mouse_ScaleValues = new PointF[_Mouse_MaxDeviceCount];

            _MouseStates_Tmp = new OpenTK.Input.MouseState[_Mouse_MaxDeviceCount];
            _KeyboardStates_Tmp = new OpenTK.Input.KeyboardState[_MouseStates_Tmp.Length];

            System.Drawing.Point[] lastMousePoint = new System.Drawing.Point[OM.Host.ScreenCount];
            MouseButton[] lastMouseButton = new MouseButton[OM.Host.ScreenCount];

            int mouseCount = 0;
            for (int i = 0; i < _Mouse_MaxDeviceCount; i++)
            {
                if (OpenTK.Input.Mouse.GetState(i).IsConnected)
                    mouseCount++;
            }
            OM.Host.DebugMsg(DebugMessageType.Info, "InputRouter", String.Format("Detected {0} mice devices", mouseCount));

            int keyboardCount = 0;
            for (int i = 0; i < _Keyboard_MaxDeviceCount; i++)
            {
                if (OpenTK.Input.Keyboard.GetState(i).IsConnected)
                    keyboardCount++;
            }
            OM.Host.DebugMsg(DebugMessageType.Info, "InputRouter", String.Format("Detected {0} keyboard devices", keyboardCount));

            // Connect events to system keyboard
            for (int i = 0; i < Core.theHost.ScreenCount; i++)
            {
                // Connect keyboard events
                Core.RenderingWindows[i].KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(SourceDown);
                Core.RenderingWindows[i].KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(SourceUp);
                Core.RenderingWindows[i].KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(SourcKeyPress);
            }

            while (thInput_Run)
            {
                try
                {
                    OpenTK.Input.MouseState mouseCursorState = OpenTK.Input.Mouse.GetCursorState();

                    // Check if any screens are using default mouse mapping (Standard)
                    for (int i = 0; i < OM.Host.ScreenCount; i++)
                    {
                        if (_Mice_MappedIndex[i] == -1)
                        {
                            if (Core.RenderingWindows[i] != null && !Core.RenderingWindows[i].IsDisposed)
                            {
                                //OpenTK.Input.MouseState mouseCursorState = OpenTK.Input.Mouse.GetState(1);
                                
                                IdleDetection_Start(i);

                                // Check if system mouse is within bounds of application window
                                if (Core.RenderingWindows[i].Focused && Core.RenderingWindows[i].Bounds.Contains(new System.Drawing.Point(mouseCursorState.X, mouseCursorState.Y)))
                                {   // Yes mouse is within bounds, generate events to this window
                                    var clientPoint = Core.RenderingWindows[i].PointToClient(new System.Drawing.Point(mouseCursorState.X, mouseCursorState.Y));

                                    var mouseButton = OpenMobile.Input.Mouse.GetMouseButtons(mouseCursorState);
                                    //var mouseButton = OpenMobile.Input.Mouse.GetMouseButtons(i);

                                    // Mouse move event
                                    if (clientPoint != lastMousePoint[i])
                                    {
                                        IdleDetection_Restart(i);

                                        //if (_LastReport != 0)
                                        //    System.Diagnostics.Debug.WriteLine("Raised InputRouter.MouseMove");
                                        _LastReport = 0;
                                        MouseMoveEventArgs eOM = new MouseMoveEventArgs(clientPoint.X, clientPoint.Y, 0, 0, mouseButton);
                                        Core.RenderingWindows[i].RenderingWindow_MouseMove(i, eOM);
                                        _PreviousEventType = EventTypes.Move;
                                    }
                                    // Mouse button events
                                    if (mouseButton != lastMouseButton[i])
                                    {
                                        //Core.RenderingWindows[i].RenderingWindow_MouseMove(i, new MouseMoveEventArgs(clientPoint.X, clientPoint.Y, 0, 0, mouseButton));
                                        _LastReport = 1;
                                        if (mouseButton != MouseButton.None)
                                        {   // Mouse down event
                                            if (_PreviousEventType != EventTypes.Move)
                                            {
                                                //System.Diagnostics.Debug.WriteLine("Forced InputRouter.MouseMove");
                                                Core.RenderingWindows[i].RenderingWindow_MouseMove(i, new MouseMoveEventArgs(clientPoint.X, clientPoint.Y, 0, 0, mouseButton));
                                            }

                                            //System.Diagnostics.Debug.WriteLine("Raised InputRouter.MouseDown");
                                            MouseButtonEventArgs eOM = new MouseButtonEventArgs(clientPoint.X, clientPoint.Y, mouseButton, true);
                                            Core.RenderingWindows[i].RenderingWindow_MouseDown(i, eOM);
                                            _PreviousEventType = EventTypes.Down;
                                        }
                                        else
                                        {   // Mouse up event
                                            //System.Diagnostics.Debug.WriteLine("Raised InputRouter.MouseUp");
                                            MouseButtonEventArgs eOM = new MouseButtonEventArgs(clientPoint.X, clientPoint.Y, mouseButton, false);
                                            Core.RenderingWindows[i].RenderingWindow_MouseUp(i, eOM);
                                            _PreviousEventType = EventTypes.Up;
                                        }
                                    }

                                    lastMousePoint[i] = clientPoint;
                                    lastMouseButton[i] = mouseButton;
                                }
                            }
                        }
                    }
                    Thread.Sleep(0);
                }
                catch
                {
                }
            }
        }

        private static void DataSources_Register()
        {
            // OM is idle
            BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource(BuiltInComponents.OMInternalPlugin, "System", "Idle", "", 0, DataSource.DataTypes.binary, null, "System is idle"), false);

        }

        private static void Commands_Register()
        {
            // Enable / Disable idle setting
            OM.Host.CommandHandler.AddCommand(true, new Command(BuiltInComponents.OMInternalPlugin, "System", "Idle", "Enable", CommandExecutor, 0, false, "Enables idle detection"));
            OM.Host.CommandHandler.AddCommand(true, new Command(BuiltInComponents.OMInternalPlugin, "System", "Idle", "Disable", CommandExecutor, 0, false, "Disables idle detection"));
        }

        private static object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = false;

            switch (command.FullNameWithoutScreen)
            {
                case "System.Idle.Enable":
                    _IdleDetectionEnabled[command.Screen] = true;
                    break;
                case "System.Idle.Disable":
                    _IdleDetectionEnabled[command.Screen] = false;
                    break;
            }

            return null;
        }

        static void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.closeProgram)
            {
                // Unhide default OS mouse (in case it was hidden at time of crash)
                Core.RenderingWindows[0].CursorVisible = true;
            }

            if (function == eFunction.pluginLoadingComplete)
            {
                // Start idle timers
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                    IdleDetection_Restart(i);
            }

        }

        #region Keyboard Device Events

        static void SourcKeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Length; i++)
            {
                if (Core.RenderingWindows[i].Focused)
                {
                    KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs();
                    eOM.Character = e.KeyChar.ToString();
                    eOM.Screen = i;
                    raiseSourceDown(sender, eOM);
                }
            }
        }

        public static void SourceUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Length; i++)
            {
                if (Core.RenderingWindows[i].Focused)
                {
                    KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs((OpenMobile.Input.Key)(int)e.Key);
                    eOM.Shift = e.Shift;
                    eOM.Control = e.Control;
                    eOM.Alt = e.Alt;
                    eOM.Screen = i;
                    raiseSourceUp(sender, eOM);
                }
            } 
        }
        private static void raiseSourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyUp, e) == true)
                return;
            //If an app handles it first don't tell the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Length; i++)
                {
                    Core.RenderingWindows[i].RenderingWindow_KeyUp(sender, e);
                    IdleDetection_Restart(i);
                }
            }
            else if (e.Screen < Core.RenderingWindows.Length)
            {
                if (Core.RenderingWindows[e.Screen].WindowState != OpenTK.WindowState.Minimized)
                {
                    Core.RenderingWindows[e.Screen].RenderingWindow_KeyUp(sender, e);
                    IdleDetection_Restart(e.Screen);
                }
            }
        }
        public static void SourceDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Length; i++)
            {
                if (Core.RenderingWindows[i].Focused)
                {
                    KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs((OpenMobile.Input.Key)(int)e.Key);
                    eOM.Shift = e.Shift;
                    eOM.Control = e.Control;
                    eOM.Alt = e.Alt;
                    eOM.Screen = i;
                    raiseSourceDown(sender, eOM);
                }
            } 
        }
        private static void raiseSourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            // TODO_: Events only returns the last instance of the result so if a event higher in the call structure handles the keypress and others don't then only the last state is reported
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyDown, e) == true)
                return;
            //If an app handles it first don't tell the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Length; i++)
                {
                    Core.RenderingWindows[i].RenderingWindow_KeyDown(sender, e);
                    IdleDetection_Restart(i);
                }
            }
            else
            {
                if (Core.RenderingWindows[e.Screen].WindowState != OpenTK.WindowState.Minimized)
                {
                    Core.RenderingWindows[e.Screen].RenderingWindow_KeyDown(sender, e);
                    IdleDetection_Restart(e.Screen);
                }
            }
        }
        public static bool SendKeyUp(int screen, string Key)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Length))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyUp(null, new KeyboardKeyEventArgs(getKey(Key)));
            IdleDetection_Restart(screen);
            return true;
        }
        public static bool SendKeyDown(int screen, string Key)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Length))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyDown(null, new KeyboardKeyEventArgs(getKey(Key)));
            IdleDetection_Restart(screen);
            return true;
        }
        private static Key getKey(string key)
        {
            switch (key.ToLower())
            {
                case "up":
                    return Key.Up;
                case "down":
                    return Key.Down;
                case "left":
                    return Key.Left;
                case "right":
                    return Key.Right;
                case "enter":
                case "return":
                    return Key.Enter;
                case "scrollup":
                    return Key.PageUp;
                case "scrolldown":
                    return Key.PageDown;
                // TODO: Add enum handling to include more keys

            }
            return Key.Unknown;
        }

        #endregion

        #region IdleDetection

        private enum IdleDetectionState
        {
            Normal,
            IdleEntering,
            Idle,
            IdleLeaving
        }
        
        /// <summary>
        /// Raises the idle event 
        /// </summary>
        private static void raiseIdleEvent(int screen, bool leaving)
        {
            SandboxedThread.Asynchronous(delegate()
            {
                if (!_IdleDetectionEnabled[screen])
                {
                    if (_Idle[screen])
                    {
                        _Idle[screen] = false;
                        // Push update to datasource
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue(screen, "OM", "System.Idle", false, true);
                        Core.theHost.raiseSystemEvent(eFunction.IdleLeaving, screen.ToString(), String.Empty, String.Empty);
                    }

                    return;
                }
                
                if (leaving)
                {
                    _Idle[screen] = false;

                    // Push update to datasource
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue(screen, "OM", "System.Idle", false, true);

                    Core.theHost.raiseSystemEvent(eFunction.IdleLeaving, screen.ToString(), String.Empty, String.Empty);
                }
                else
                {
                    _Idle[screen] = true;

                    // Push update to datasource
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue(screen, "OM", "System.Idle", true, true);

                    Core.theHost.raiseSystemEvent(eFunction.IdleEntering, screen.ToString(), String.Empty, String.Empty);

                    // Execute configured action
                    string action = BuiltInComponents.SystemSettings.IdleDetectionAction(screen);
                    if (!string.IsNullOrEmpty(action))
                        BuiltInComponents.Host.CommandHandler.ExecuteCommand(action);
                }
            }
            );
        }

        /// <summary>
        /// Idle detection timer event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void tmrIdleDetection_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int screen = ((Timer)sender).Screen;
            tmrIdleDetection[screen].Stop();
            //OM.Host.DebugMsg(DebugMessageType.Info, "Idledetection entering idle state");
            tmrIdleDetection[screen].Tag = IdleDetectionState.IdleEntering;
            raiseIdleEvent(screen, false);
            tmrIdleDetection[screen].Tag = IdleDetectionState.Idle;
        }

        /// <summary>
        /// Restarts the idledetection timer
        /// </summary>
        /// <param name="screen"></param>
        private static void IdleDetection_Restart(int screen)
        {
            try
            {
                if (tmrIdleDetection != null)
                {
                    if (tmrIdleDetection.Length > screen)
                    {
                        if (tmrIdleDetection[screen] != null)
                        {
                            if ((IdleDetectionState)tmrIdleDetection[screen].Tag == IdleDetectionState.Idle)
                            {   // Timer was active, send leaving idle mode event
                                //OM.Host.DebugMsg(DebugMessageType.Info, "Idledetection leaving idle state");
                                tmrIdleDetection[screen].Tag = IdleDetectionState.IdleLeaving;
                                raiseIdleEvent(screen, true);
                                tmrIdleDetection[screen].Tag = IdleDetectionState.Normal;
                            }

                            //OM.Host.DebugMsg(DebugMessageType.Info, "Idledetection timer restarted");
                            tmrIdleDetection[screen].Stop();
                            tmrIdleDetection[screen].Start();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Starts the idledetection timer
        /// </summary>
        /// <param name="screen"></param>
        private static void IdleDetection_Start(int screen)
        {
            try
            {
                if (tmrIdleDetection != null)
                {
                    if (tmrIdleDetection.Length > screen)
                    {
                        if (tmrIdleDetection[screen] != null)
                        {
                            //if ((IdleDetectionState)tmrIdleDetection[screen].Tag == IdleDetectionState.Idle)
                            //{   // Timer was active, send leaving idle mode event
                            //    OM.Host.DebugMsg(DebugMessageType.Info, "Idledetection leaving idle state");
                            //    tmrIdleDetection[screen].Tag = IdleDetectionState.IdleLeaving;
                            //    raiseIdleEvent(screen, true);
                            //    tmrIdleDetection[screen].Tag = IdleDetectionState.Normal;
                            //}
                            if ((IdleDetectionState)tmrIdleDetection[screen].Tag == IdleDetectionState.Normal)
                            {
                                if (!tmrIdleDetection[screen].Enabled)
                                {
                                    //OM.Host.DebugMsg(DebugMessageType.Info, "Idledetection timer started");
                                    tmrIdleDetection[screen].Start();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }


        #endregion
    }
}
