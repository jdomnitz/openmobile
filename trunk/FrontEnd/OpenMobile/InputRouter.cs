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
using OpenMobile.helperFunctions;

namespace OpenMobile
{
    public static class InputRouter
    {
        public const string DefaultUnit = "OS Default";
        public const string DisabledUnit = "Disabled";

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
        }

        public static void Initialize()
        {
            // Idle detection
            int IdleDetectionInterval = BuiltInComponents.SystemSettings.IdleDetectionInterval;
            if (IdleDetectionInterval > 0)
            {
                tmrIdleDetection = new Timer[BuiltInComponents.Host.ScreenCount];
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

            // Connect mouse events
            for (int i = 0; i < Core.theHost.ScreenCount; i++)
            {
                // Connect mouse events
                Core.RenderingWindows[i].MouseDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(dev_ButtonDown);
                Core.RenderingWindows[i].MouseUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(dev_ButtonUp);
                Core.RenderingWindows[i].MouseMove += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(dev_Move);

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

        #region Mouse Device Events

        static internal void dev_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                MouseMoveEventArgs eOM = new MouseMoveEventArgs(e.X, e.Y, e.XDelta, e.YDelta, OpenMobile.Input.Mouse.GetMouseButtons(Core.RenderingWindows[i].Mouse));
                Core.RenderingWindows[i].RenderingWindow_MouseMove(i, eOM);
            } 
        }

        static internal void dev_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                MouseButtonEventArgs eOM = new MouseButtonEventArgs(e.X, e.Y, OpenMobile.Input.Mouse.GetMouseButtons(Core.RenderingWindows[i].Mouse), false);
                Core.RenderingWindows[i].RenderingWindow_MouseUp(i, eOM);
            } 
        }

        static internal void dev_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                MouseButtonEventArgs eOM = new MouseButtonEventArgs(e.X, e.Y, OpenMobile.Input.Mouse.GetMouseButtons(Core.RenderingWindows[i].Mouse), true);
                Core.RenderingWindows[i].RenderingWindow_MouseDown(i, eOM);
            } 
        }

        #endregion

        #region Keyboard Device Events

        static void SourcKeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs();
                eOM.Character = e.KeyChar.ToString();
                eOM.Screen = i;
                raiseSourceDown(sender, eOM);
            }
        }

        public static void SourceUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            // Pass event along
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs((OpenMobile.Input.Key)(int)e.Key);
                eOM.Shift = e.Shift;
                eOM.Control = e.Control;
                eOM.Alt = e.Alt;
                eOM.Screen = i;
                raiseSourceUp(sender, eOM);
            } 
        }
        private static void raiseSourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyUp, e) == true)
                return;
            //If an app handles it first don't tell the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
                {
                    Core.RenderingWindows[i].RenderingWindow_KeyUp(sender, e);
                    IdleDetection_Restart(i);
                }
            }
            else if (e.Screen < Core.RenderingWindows.Count)
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
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
            {
                IdleDetection_Restart(i);
                KeyboardKeyEventArgs eOM = new KeyboardKeyEventArgs((OpenMobile.Input.Key)(int)e.Key);
                eOM.Shift = e.Shift;
                eOM.Control = e.Control;
                eOM.Alt = e.Alt;
                eOM.Screen = i;
                raiseSourceDown(sender, eOM);
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
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
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
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyUp(null, new KeyboardKeyEventArgs(getKey(Key)));
            IdleDetection_Restart(screen);
            return true;
        }
        public static bool SendKeyDown(int screen, string Key)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
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
            if (tmrIdleDetection != null)
            {
                if (tmrIdleDetection.Length > screen)
                {
                    if (tmrIdleDetection[screen] != null)
                    {
                        if ((IdleDetectionState)tmrIdleDetection[screen].Tag == IdleDetectionState.Idle)
                        {   // Timer was active, send leaving idle mode event
                            tmrIdleDetection[screen].Tag = IdleDetectionState.IdleLeaving;
                            raiseIdleEvent(screen, true);
                            tmrIdleDetection[screen].Tag = IdleDetectionState.Normal;
                        }

                        tmrIdleDetection[screen].Stop();
                        tmrIdleDetection[screen].Start();
                    }
                }
            }
        }

        #endregion
    }
}
