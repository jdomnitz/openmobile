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
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using System.Diagnostics;
using OpenMobile.Graphics;

namespace OpenMobile
{
    public static class Core
    {
        public static PluginHost theHost = new PluginHost();
        public static List<RenderingWindow> RenderingWindows = null;
        public static List<IBasePlugin> pluginCollection = new List<IBasePlugin>();
        public static bool exitTransition = true;
        public static GameWindowFlags Fullscreen;

        /// <summary>
        /// Loads an assembly with error message if it fails.
        /// Application exits when an error is detected
        /// </summary>
        /// <param name="AssemblyName"></param>
        /// <returns></returns>
        private static Assembly LoadSkinDLL(string DLLName)
        {
            try
            {
                return Assembly.Load(DLLName);
            }
            catch
            {
                using (PluginSettings s = new PluginSettings())
                {
                    Application.ShowError(null, "OpenMobile was unable to load required skin file:\n\n" + DLLName + ".dll\n\nMake sure that required files are located in the correct skin folder.\nSetting active skin back to \"Default\" since loading skin \"" + s.getSetting("UI.Skin") + "\" failed!\n\nApplication will now exit!", "Missing required file!");
                    // Set skin back to default
                    s.setSetting("UI.Skin", "Default");
                }
                Environment.Exit(0);
                return null;
            }
        }

        private static void loadDebug()
        {   // Load debug dll (if available)
            IBasePlugin Plugin = null;
            string DebugDll = Path.Combine(theHost.PluginPath, "!OMDebug.dll");
            if (!File.Exists(DebugDll))
            {
                DebugDll = Path.Combine(theHost.PluginPath, "OMDebug.dll");
                if (!File.Exists(DebugDll))
                    return;
            }

            // Load dll
            try
            {
                Plugin = loadAndCheck(DebugDll, true);
                if (Plugin != null)
                    Plugin.initialize(theHost);
            }
            catch
            {   // No error handling, dll will be loaded later if this failed
            }
        }

        private static void loadMainMenu()
        {
            #region UI

            Assembly pluginAssembly = LoadSkinDLL("UI");
            IHighLevel UIPlugin = null;
            try
            {
                foreach (Type t in pluginAssembly.GetTypes())
                    if (t.IsPublic)
                        UIPlugin = (IHighLevel)Activator.CreateInstance(t);
            }
            catch (Exception)
            {
                //handled below
            }
            if (UIPlugin == null)
            {   // Retry load but with a different type
                UIPlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.UI"));
                
                // Does it still fail?
                if (UIPlugin == null)
                    // Yes, show error
                    Application.ShowError(null, "No UI Skin available!", "No skin available!");
            }
            pluginCollection.Add(UIPlugin);

            #endregion 

            #region MainMenu

            pluginAssembly = LoadSkinDLL("MainMenu");
            IBasePlugin MainMenuPlugin = null;
            try
            {
                foreach (Type t in pluginAssembly.GetTypes())
                    if (t.IsPublic)
                        MainMenuPlugin = (IBasePlugin)Activator.CreateInstance(t);
            }
            catch { }
            if (MainMenuPlugin == null)
            {   // Retry load but with a different type
                MainMenuPlugin = (IBasePlugin)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.MainMenu"));

                // Does it still fail?
                if (MainMenuPlugin == null)
                    // Yes, show error
                    Application.ShowError(null, "No Main Menu Skin available!", "No skin available!");
            }
            pluginCollection.Add(MainMenuPlugin);

            #endregion
        }

        private static void initMainMenu()
        {
            var a = pluginCollection[2].GetType().GetCustomAttributes(typeof(InitialTransition), false);
            SandboxedThread.Asynchronous(() =>
            {
                pluginCollection[1].initialize(theHost);
                pluginCollection[2].initialize(theHost);
                for (int i = 0; i < RenderingWindows.Count; i++)
                {
                    // Load UI as background
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "UI", "background");
                    RenderingWindows[i].TransitionInPanel(((IHighLevel)pluginCollection[1]).loadPanel(String.Empty, i));
                    RenderingWindows[i].ExecuteTransition(eGlobalTransition.None);
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "MainMenu", String.Empty);
                    if (a.Length == 0)
                        RenderingWindows[i].ExecuteTransition(eGlobalTransition.None);
                    else
                        RenderingWindows[i].ExecuteTransition(((InitialTransition)a[0]).Transition);
                }
            });
            object[] b = pluginCollection[1].GetType().GetCustomAttributes(typeof(FinalTransition), false);
            if (b.Length > 0)
                exitTransition = ((FinalTransition)b[0]).Transition;
        }
        /// <summary>
        /// Load each of the plugins into the plugin's array (pluginCollection)
        /// </summary>
        private static void loadEmUp()
        {
            foreach (string file in Directory.GetFiles(theHost.PluginPath, "*.dll"))
            {
                try
                {
                    loadAndCheck(file);
                }
                catch (Exception e)
                {
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Loader", "Exception: " + ex);
#if DEBUG
                    Debug.Print(ex);
#endif
                }
            }
            foreach (string file in Directory.GetFiles(theHost.SkinPath, "*.dll"))
            {
                try
                {
                    loadAndCheck(file);
                }
                catch (Exception e)
                {
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Loader", "Exception: " + ex);
#if DEBUG
                    Debug.Print(ex);
#endif
                }
            }
        }

        private static bool IsPluginLoaded(IBasePlugin Plugin)
        {
            IBasePlugin plugin = pluginCollection.Find(a => a.pluginName == Plugin.pluginName);
            return (plugin != null ? true : false);
        }

        private static IBasePlugin loadAndCheck(string file)
        {
            return loadAndCheck(file, false);
        }
        private static IBasePlugin loadAndCheck(string file, bool LoadSpecific)
        {
            Assembly pluginAssembly;
            if (!LoadSpecific)
                pluginAssembly = Assembly.Load(Path.GetFileNameWithoutExtension(file));
            else
                pluginAssembly = Assembly.LoadFrom(file);

            IBasePlugin availablePlugin = null;
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic) //Only look at public types
                {
                    if (!pluginType.IsAbstract)  //Only look at non-abstract types
                    {
                        //Make sure the interface we want to use actually exists
                        if (typeof(IBasePlugin).IsAssignableFrom(pluginType))
                        {
                            availablePlugin = (IBasePlugin)Activator.CreateInstance(pluginType);
                            
                            // Did we load this dll before? If so skip it
                            if (IsPluginLoaded(availablePlugin))
                                continue;

                            if (typeof(INetwork).IsInstanceOfType(availablePlugin))
                                ((INetwork)availablePlugin).OnWirelessEvent += theHost.raiseWirelessEvent;
                            if (typeof(IBluetooth).IsInstanceOfType(availablePlugin))
                                ((IBluetooth)availablePlugin).OnWirelessEvent += theHost.raiseWirelessEvent;
                            pluginCollection.Add(availablePlugin);
                        }
                    }
                }
            }
            return availablePlugin;
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name;
            int comma = name.IndexOf(',');
            if (comma >= 0)
                name = name.Substring(0, comma);
            string loc = Path.Combine(theHost.PluginPath, name + ".dll");
            if (File.Exists(loc))
                return Assembly.LoadFrom(loc);
            else
                loc = Path.Combine(theHost.SkinPath, name + ".dll");
            if (File.Exists(loc))
                return Assembly.LoadFrom(loc);
            foreach (string folder in Directory.GetDirectories(theHost.PluginPath))
            {
                loc = Path.Combine(folder, name + ".dll");
                if (File.Exists(loc))
                    return Assembly.LoadFrom(loc);
            }
            return null;
        }
        /// <summary>
        /// Order to of types to try to load the plugins by
        /// </summary>
        private static Type[] pluginTypes = new Type[] {typeof(IRawHardware), typeof(IDataProvider), typeof(IAVPlayer), typeof(IPlayer), typeof(ITunedContent), typeof(IMediaDatabase), typeof(INetwork), typeof(IHighLevel), typeof(INavigation), typeof(IOther), typeof(IBasePlugin)};
        public static eLoadStatus[] status;
        /// <summary>
        /// Initialize each of the plugins in the plugin's array (pluginCollection)
        /// </summary>
        private static void getEmReady()
        {
            status = new eLoadStatus[pluginCollection.Count];
            foreach (Type tp in pluginTypes)
            {
                for (int i = 3; i < pluginCollection.Count; i++) //Try to initialize the plugins
                {
                    try
                    {
                        if (pluginCollection[i] != null)
                        {
                            if (tp.IsInstanceOfType(pluginCollection[i]) && status[i] == eLoadStatus.NotLoaded)
                            {
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Plugin Manager", "Initializing " + pluginCollection[i].pluginName);
                                status[i] = pluginCollection[i].initialize(theHost);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        status[i] = eLoadStatus.LoadFailedUnloadRequested;
                        string ex = spewException(e);
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Manager", "Exception: " + ex);
                        Debug.Print(ex);
                    }
                }
            }
            foreach (Type tp in pluginTypes)
            {             
                for (int i = 3; i < pluginCollection.Count; i++) //Give them all a second chance if they need it
                try
                {
                    if (tp.IsInstanceOfType(pluginCollection[i]))
                    {
                        if (status[i] == eLoadStatus.LoadFailedRetryRequested)
                            status[i] = pluginCollection[i].initialize(theHost);
                    }
                }
                catch (Exception e)
                {
                    status[i] = eLoadStatus.LoadFailedUnloadRequested;
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Manager", "Exception2: " + ex);
                    Debug.Print(ex);
                }
            }
            for (int i = 3; i < pluginCollection.Count; i++) //and then two strikes their out...kill anything that still can't initialize
            {
                if ((status[i] == eLoadStatus.LoadFailedRetryRequested) || (status[i] == eLoadStatus.LoadFailedUnloadRequested) || (status[i] == eLoadStatus.LoadFailedGracefulUnloadRequested))
                {
                    try
                    {
                        pluginCollection[i].Dispose();
                        if (status[i] != eLoadStatus.LoadFailedGracefulUnloadRequested)
                            theHost.execute(eFunction.backgroundOperationStatus, pluginCollection[i].pluginName + " CRASHED!");
                    }
                    catch (Exception) { }
                    pluginCollection[i] = null;

                    // Remove status for this plugin
                    eLoadStatus[] temp = new eLoadStatus[status.Length - 1];
                    Array.Copy(status, 0, temp, 0, i);
                    Array.Copy(status, i + 1, temp, i, status.Length - i - 1);
                    status = temp;

                    // Remove plugin
                    pluginCollection.RemoveAll(p => p == null);
                }
            }
        }        


        private static void initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            // Load and initialize UI and MainMenu
            loadMainMenu(); 
            initMainMenu();

            // Init plugins
            InitPluginsAndData();

            // Start application
            Application.Run();
        }

        private static void InitPluginsAndData()
        {
            // Load plugins
            loadEmUp(); 

            // Initialize plugins
            getEmReady(); 

            // Tell pluginhost to start loading data
            theHost.Load();

            // Inform system that plugin loading is completed
            theHost.raiseSystemEvent(eFunction.pluginLoadingComplete, String.Empty, String.Empty, String.Empty);

            // Enumerate available devices
            theHost.Hal_Send("32");

            // Start executing background tasks
            OpenMobile.Threading.TaskManager.Enable(Core.theHost);

            // Set graphic level
            using (PluginSettings settings = new PluginSettings())
            {
                if (settings.getSetting("UI.MinGraphics") == "True")
                    theHost.GraphicsLevel = eGraphicsLevel.Minimal;
            }

            // Raise network events (if available)
            if (OpenMobile.Net.Network.IsAvailable)
                theHost.raiseSystemEvent(eFunction.connectedToInternet, String.Empty, String.Empty, String.Empty);

            // Clean plugincollection list
            pluginCollection.TrimExcess();
        }

        static bool ErroredOut;
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ErroredOut) //Prevent multiple bugs
                return;
            ErroredOut = true;
            Exception ex = (Exception)e.ExceptionObject;
            theHost.Stop();
            string strEx = spewException(ex);
            FileStream fs = File.OpenWrite(Path.Combine(theHost.DataPath, "AppCrash-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Second.ToString() + ".log"));
            fs.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(strEx), 0, strEx.Length);
            fs.Close();

            // Unhide default OS mouse (in case it was hidden at time of crash)
            try
            {
                Core.RenderingWindows[0].DefaultMouse.ShowCursor(Core.RenderingWindows[0].WindowInfo);
            }
            catch { }

            //ErrorReporting reporting=new ErrorReporting(strEx);
            //reporting.ShowDialog(System.Windows.Forms.Form.FromHandle(RenderingWindows[0].getHandle()));
            if ((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMinutes > 3) //Prevent Loops
                theHost.execute(eFunction.restartProgram);
            Environment.Exit(0);
        }
        private static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }

        [STAThread]
        private static void Main()
        {
            // Only handle system wide crashes if not in IDE
            if (!System.Diagnostics.Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #region Process startup arguments

            foreach (string arg in Environment.GetCommandLineArgs())
            {
                // Restrict amount of screens at startup time
                if (arg.ToLower().StartsWith("-screencount=") == true)
                {
                    try
                    {
                        theHost.ScreenCount = int.Parse(arg.Substring(13));
                    }
                    catch (ArgumentException) { break; }
                }

                    // Fullscreen mode?
                else if (arg.ToLower() == "-fullscreen")
                {
                    Fullscreen = GameWindowFlags.Fullscreen;
                }

                // Override current skin to use
                else if (arg.ToLower().StartsWith("-skinpath=") == true)
                {
                    theHost.SkinPath = arg.Substring(10);
                }

                // Specific startup screen is given
                else if (arg.ToLower().StartsWith("-startupscreen=") == true)
                {
                    // Restrict amount of possible screens since we're changing the screensetups
                    theHost.ScreenCount = 1; 

                    // Save startupscreen to pluginhost's data
                    if (arg.Length >= 15)
                    {
                        try
                        {
                            theHost.StartupScreen = int.Parse(arg.Substring(15));
                        }
                        catch (ArgumentException) { break; }
                    }
               }
            }

            #endregion

            // Initialize screens
            RenderingWindows = new List<RenderingWindow>(theHost.ScreenCount);
            for (int i = 0; i < RenderingWindows.Capacity; i++)
                RenderingWindows.Add(new RenderingWindow(i));

            #region Check for missing database (and create if needed)

            if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
            {
                // Create database
                using (PluginSettings settings = new PluginSettings())
                {
                    if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
                    {
                        Application.ShowError(IntPtr.Zero, "A required SQLite database OMData was not found in the application directory.  An attempt to create the database failed!  This database is required for Open Mobile to run.", "Database Missing!");
                        Environment.Exit(0);
                        return;
                    }
                }
            }

            #endregion

            // Load and initialize debug
            loadDebug();

            // Start all pluginhost features (including HAL)
            theHost.Start();

            // Write startup params to debug log
            string[] Args = Environment.GetCommandLineArgs();
            for (int i = 0; i < Args.Length; i++)
                Args[i] = i.ToString() + " > " + Args[i];
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Startup arguments:", Args);

            // Error check
            if (RenderingWindows.Count == 0)
                throw new Exception("Unable to detect any monitors on this platform!");

            Thread rapidMenu = new Thread(Core.initialize);
            rapidMenu.Start();

            for (int i = 1; i < RenderingWindows.Count; i++)
                RenderingWindows[i].RunAsync(Fullscreen);

            RenderingWindows[0].Run(Fullscreen);

            for (int i = 0; i < RenderingWindows.Count; i++)
                RenderingWindows[i].Dispose();
            for (int i = 0; i < pluginCollection.Count; i++)
                if (pluginCollection[i] != null)
                    pluginCollection[i].Dispose();
            InputRouter.Dispose();
            Environment.Exit(0); //Force
        }

        internal static bool loadPlugin(string arg)
        {
            int count = pluginCollection.Count;
            loadAndCheck(arg);
            if (pluginCollection.Count == count + 1)
            {
                eLoadStatus status;
                try
                {
                    status = pluginCollection[count].initialize(theHost);
                    if (status == eLoadStatus.LoadSuccessful)
                        return true;
                }
                catch (Exception e)
                {
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Manager", "Exception: " + ex);
                    Debug.Print(ex);
                }
                pluginCollection.RemoveAt(count);
            }
            return false;
        }
    }
}
