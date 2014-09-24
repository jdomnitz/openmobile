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
using OpenMobile.helperFunctions.Plugins;
using System.Linq;

namespace OpenMobile
{
    public static class Core
    {
        public static PluginHost theHost = new PluginHost();
        public static List<RenderingWindow> RenderingWindows = null;
        public static List<IBasePlugin> pluginCollection = new List<IBasePlugin>();
        public static List<IBasePlugin> pluginCollectionDisabled = new List<IBasePlugin>();
        public static bool exitTransition = true;
        public static OpenTK.GameWindowFlags Fullscreen;
        public static StartupArguments StartupArgs;

        /// <summary>
        /// The maximum allowed time for a plugin to use for initialization
        /// </summary>
        private static int _Plugin_MaxInitTimeMS = 250;

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
                    Application.ShowError(null, "OpenMobile was unable to load required skin file:\n\n" + DLLName + ".dll\n\nMake sure that required files are located in the correct skin folder.\nSetting active skin back to \"Default\" since loading skin \"" + s.getSetting(BuiltInComponents.OMInternalPlugin, "UI.Skin") + "\" failed!\n\nApplication will now exit!", "Missing required file!");
                    // Set skin back to default
                    s.setSetting(BuiltInComponents.OMInternalPlugin, "UI.Skin", "Default");
                }
                Environment.Exit(0);
                return null;
            }
        }

        private static void loadDebug()
        {   // Load debug dll (if available)
            status = new eLoadStatus[1];
            IBasePlugin Plugin = null;
            string[] DebugDlls = Directory.GetFiles(theHost.PluginPath, "*OMDebug*.dll", SearchOption.AllDirectories);

            foreach (string file in DebugDlls)
            {
                // Load dll
                try
                {
                    Plugin = loadAndCheck(theHost.PluginPath, file, true);
                    if (Plugin != null)
                    {
                        status[0] = Plugin.initialize(theHost);
                        if (status[0] == eLoadStatus.LoadSuccessful)
                            break;
                    }
                }
                catch
                {   // No error handling, dll will be loaded later if this failed
                }
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
            IBasePlugin UI = pluginCollection.Find(x => x.pluginName.ToLower() == "ui");
            
            if (UI == null)
            {
                Application.ShowError(null, "No UI Skin available!", "No skin available!");
                Environment.Exit(0);
            }

            IBasePlugin MainMenu = pluginCollection.Find(x => x.pluginName.ToLower() == "mainmenu");
            if (MainMenu == null)
            {
                Application.ShowError(null, "No Main Menu Skin available!", "No skin available!");
                Environment.Exit(0);
            }

            status[pluginCollection.IndexOf(UI)] = UI.initialize(theHost);
            status[pluginCollection.IndexOf(MainMenu)] = MainMenu.initialize(theHost);

            var a = MainMenu.GetType().GetCustomAttributes(typeof(InitialTransition), false);
            //SandboxedThread.Asynchronous(() =>
            {
                for (int i = 0; i < RenderingWindows.Count; i++)
                {
                    // Load UI as background
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "UI", "background");
                    //RenderingWindows[i].TransitionInPanel(((IHighLevel)UI).loadPanel(String.Empty, i));
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "UI", "");
                    RenderingWindows[i].ExecuteTransition("None", 0);
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "MainMenu", String.Empty);
                    if (a.Length == 0)
                        RenderingWindows[i].ExecuteTransition("None", 0);
                    else
                        RenderingWindows[i].ExecuteTransition(((InitialTransition)a[0]).Transition.ToString(), 0);
                }
            }//);
            object[] b = MainMenu.GetType().GetCustomAttributes(typeof(FinalTransition), false);
            if (b.Length > 0)
                exitTransition = ((FinalTransition)b[0]).Transition;
        }
        /// <summary>
        /// Load each of the plugins into the plugin's array (pluginCollection)
        /// </summary>
        private static void loadEmUp()
        {
            string[] Files = Directory.GetFiles(theHost.PluginPath, "*.dll", SearchOption.AllDirectories);
            foreach (string file in Files)
            {
                try
                {
                    loadAndCheck(theHost.PluginPath, file, true);
                }
                catch (Exception e)
                {
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Loader (Plugins)", "Exception: " + ex);
#if DEBUG
                    Debug.Print(ex);
#endif
                }
            }

            // Load skin specific dll's 
            Files = Directory.GetFiles(theHost.SkinPath, "*.dll", SearchOption.AllDirectories);
            foreach (string file in Files) //Directory.GetFiles(theHost.SkinPath, "*.dll"))
            {
                try
                {
                    loadAndCheck(theHost.SkinPath, file, true);
                }
                catch (Exception e)
                {
                    string ex = spewException(e);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Loader (Skin)", "Exception: " + ex);
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

        private static IBasePlugin loadAndCheck(string baseFolder, string file)
        {
            return loadAndCheck(baseFolder, file, false);
        }
        private static IBasePlugin loadAndCheck(string baseFolder, string file, bool LoadSpecific)
        {
            try
            {
                // Should we limit the amount of folders to search trough?
                if (!String.IsNullOrWhiteSpace(baseFolder))
                {   // Yes. Limit is set to maximum one level below the base folder
                    string diffPath = file.Replace(baseFolder, "");
                    int folderLevel = diffPath.Count(x => x.Equals(System.IO.Path.DirectorySeparatorChar));
                    if (folderLevel > 2)
                        return null;
                }

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
            catch (Exception ex)
            {
                //BuiltInComponents.Host.DebugMsg( DebugMessageType.Error, String.Format("Unable to load file: {0}", file));
                //OM.Host.DebugMsg(String.Format("Plugin Manager was unable to load file {0}", file), ex);
            }
            return null;
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

        private static bool IsPluginLevel(IBasePlugin Plugin, PluginLevels PluginLevel)
        {
            // Check plugin level attribute flags
            PluginLevel[] a = (PluginLevel[])Plugin.GetType().GetCustomAttributes(typeof(PluginLevel), false);
            PluginLevels Level = PluginLevels.Normal; // Default
            if (a.Length > 0)
                Level = a[0].TypeOfPlugin;
            if ((Level | PluginLevel) == PluginLevel)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Order to of types to try to load the plugins by
        /// </summary>
        private static Type[] pluginTypes = new Type[] { typeof(IRawHardware), typeof(IAudioDeviceProvider), typeof(IDataSource), typeof(IMediaProvider), typeof(IAVPlayer), typeof(IPlayer), typeof(ITunedContent), typeof(IMediaDatabase), typeof(INetwork), typeof(IHighLevel), typeof(INavigation), typeof(IOther), typeof(IBasePlugin) };
        public static eLoadStatus[] status;

        private static void Plugin_Initialize(int index)
        {
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Plugin Manager", String.Format("Initializing {0}", pluginCollection[index].pluginName));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            status[index] = pluginCollection[index].initialize(theHost);
            sw.Stop();
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Plugin Manager", String.Format("Initialization of {0} completed (used {1}ms)", pluginCollection[index].pluginName, sw.ElapsedMilliseconds));
            // Inform user about slow plugins
            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (sw.ElapsedMilliseconds > _Plugin_MaxInitTimeMS)
                {
                    theHost.UIHandler.AddNotification(new Notification(Notification.Styles.Warning, pluginCollection[index], null, null, String.Format("Plugin {0} is slow to initialize", pluginCollection[index].pluginName), String.Format("Used a total of {0}ms of maximum {1}ms", sw.ElapsedMilliseconds, _Plugin_MaxInitTimeMS)));
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "Plugin Manager", String.Format("Plugin {0} has slow initializing (used {1}ms)", pluginCollection[index].pluginName, sw.ElapsedMilliseconds));
                }
            }
        }

        /// <summary>
        /// Initialize each of the plugins in the plugin's array (pluginCollection)
        /// </summary>
        private static void getEmReady(bool SystemOnly)
        {
            Array.Resize<eLoadStatus>(ref status, pluginCollection.Count);
            //status = new eLoadStatus[pluginCollection.Count];
            foreach (Type tp in pluginTypes)
            {
                for (int i = 0; i < pluginCollection.Count; i++) //Try to initialize the plugins
                {
                    try
                    {
                        if (pluginCollection[i] != null)
                        {
                            if (tp.IsInstanceOfType(pluginCollection[i]) && status[i] == eLoadStatus.NotLoaded)
                            {
                                // Skip plugins that is not marked as system if SystemOnly is true
                                if (SystemOnly)
                                    if (!IsPluginLevel(pluginCollection[i], PluginLevels.System))
                                        continue;

                                // Check if required os configuration flags is valid
                                if (!helperFunctions.Plugins.Plugins.IsPluginOSConfigurationFlagsValid(pluginCollection[i]))
                                {
                                    // Add to disabled plugins list instead
                                    if (!pluginCollectionDisabled.Contains(pluginCollection[i]))
                                        pluginCollectionDisabled.Add(pluginCollection[i]);
                                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "Plugin Manager", String.Format("Plugin not supported on this system: {0} [Required ConfFlags: {1} | System ConfFlags: {2}]", pluginCollection[i].pluginName, Plugins.GetPluginOSConfigurationFlags(pluginCollection[i]), Plugins.GetSystemOSConfigurationFlags()));
                                    continue;
                                }

                                // Skip plugins that's not enabled
                                if (!helperFunctions.Plugins.Plugins.GetPluginEnabled(pluginCollection[i].pluginName))
                                {
                                    // Add to disabled plugins list instead
                                    if (!pluginCollectionDisabled.Contains(pluginCollection[i]))
                                        pluginCollectionDisabled.Add(pluginCollection[i]);
                                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Plugin Manager", String.Format("Plugin disabled: {0}", pluginCollection[i].pluginName));
                                    continue;
                                }

                                Plugin_Initialize(i);
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
                for (int i = 0; i < pluginCollection.Count; i++) //Give them all a second chance if they need it
                    try
                    {
                        if (tp.IsInstanceOfType(pluginCollection[i]))
                        {
                            if (status[i] == eLoadStatus.LoadFailedRetryRequested)
                            {
                                // Skip plugins that is not marked as system if SystemOnly is true
                                if (SystemOnly)
                                    if (!IsPluginLevel(pluginCollection[i], PluginLevels.System))
                                        continue;

                                // Check if required os configuration flags is valid
                                if (!helperFunctions.Plugins.Plugins.IsPluginOSConfigurationFlagsValid(pluginCollection[i]))
                                {
                                    // Add to disabled plugins list instead
                                    if (!pluginCollectionDisabled.Contains(pluginCollection[i]))
                                        pluginCollectionDisabled.Add(pluginCollection[i]);
                                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "Plugin Manager", String.Format("Plugin not supported on this system: {0} Required ConfFlags: {1}, System ConfFlags:{2}" + pluginCollection[i].pluginName, OpenMobile.helperFunctions.Plugins.Plugins.GetPluginOSConfigurationFlags(pluginCollection[i]), OpenMobile.helperFunctions.Plugins.Plugins.GetSystemOSConfigurationFlags()));
                                    continue;
                                }

                                // Skip plugins that's not enabled
                                if (!helperFunctions.Plugins.Plugins.GetPluginEnabled(pluginCollection[i].pluginName))
                                {
                                    // Add to disabled plugins list instead
                                    if (!pluginCollectionDisabled.Contains(pluginCollection[i]))
                                        pluginCollectionDisabled.Add(pluginCollection[i]);
                                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Plugin Manager", String.Format("Plugin disabled: {0}", pluginCollection[i].pluginName));
                                    continue;
                                }

                                Plugin_Initialize(i);
                            }
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
            for (int i = 0; i < pluginCollection.Count; i++) //and then two strikes their out...kill anything that still can't initialize
            {
                if ((status[i] == eLoadStatus.LoadFailedRetryRequested) || (status[i] == eLoadStatus.LoadFailedUnloadRequested) || (status[i] == eLoadStatus.LoadFailedGracefulUnloadRequested))
                {
                    try
                    {
                        pluginCollection[i].Dispose();
                        if (status[i] != eLoadStatus.LoadFailedGracefulUnloadRequested)
                        {
                            theHost.UIHandler.AddNotification(new Notification(Notification.Styles.Warning, pluginCollection[i], null, null, String.Format("Loading of plugin {0} failed", pluginCollection[i].pluginName), String.Format("Plugin {0} ({1}) by {2} was unloaded.", pluginCollection[i].pluginName, pluginCollection[i].pluginDescription, pluginCollection[i].authorName)));
                            BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Plugin Manager", String.Format("{0} CRASHED!", pluginCollection[i].pluginName));
                        }

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

            // Filter out disabled plugins from main pluginlist
            foreach (IBasePlugin plugin in pluginCollectionDisabled)
                pluginCollection.Remove(plugin);
        }


        private static void initialize()
        {
            SetStartupInfoText("Initializing...");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            // Load plugins
            SetStartupInfoText("Loading plugins...");
            loadEmUp();

            // Initialize system plugins
            SetStartupInfoText("Initializing system...");
            getEmReady(true); 

            // Load and initialize UI and MainMenu
            //loadMainMenu(); 
            SetStartupInfoText("Initializing graphics...");
            initMainMenu();

            // Init plugins
            SetStartupInfoText("Initializing plugins...");
            InitPluginsAndData();

            // Start application
            SetStartupInfoText("Loading completed");
            Application.Run();
        }

        private static void InitPluginsAndData()
        {

            // Initialize plugins
            getEmReady(false); 

            // Tell pluginhost to start loading data
            theHost.LateInit();

            // Inform system that plugin loading is completed
            SandboxedThread.Asynchronous(() => 
                {
                    theHost.raiseSystemEvent(eFunction.pluginLoadingComplete, String.Empty, String.Empty, String.Empty);
                });

            // Enumerate available devices
            theHost.Hal_Send("32");

            // Start executing background tasks
            OpenMobile.Threading.TaskManager.Enable(Core.theHost);

            // Set graphic level
            using (PluginSettings settings = new PluginSettings())
            {
                if (BuiltInComponents.SystemSettings.UseSimpleGraphics)
                    theHost.GraphicsLevel = eGraphicsLevel.Minimal;
            }

            // Raise network events (if available)
            theHost.Network_PollForInternetAvailability();

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
                Core.RenderingWindows[0].CursorVisible = true;
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

        static private void SetStartupInfoText(string text)
        {
            for (int i = 0; i < RenderingWindows.Count; i++)
                RenderingWindows[i].StartupInfoText = text;
        }

        [MTAThread]
        private static void Main()
        {
            // Only handle system wide crashes if not in IDE
            if (!System.Diagnostics.Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #region Process startup arguments

            StartupArgs = new StartupArguments(Environment.GetCommandLineArgs());

            Size InitialScreenSize = new Size(800, 480);

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
                    Fullscreen = OpenTK.GameWindowFlags.Fullscreen;
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

                // Specific size is given (-ScreenSize=1000x600)
                else if (arg.ToLower().StartsWith("-screensize=") == true)
                {
                    try
                    {
                        string[] SizeString = arg.Substring(12).Split('x');
                        InitialScreenSize = new Size(int.Parse(SizeString[0]), int.Parse(SizeString[1]));
                    }
                    catch (ArgumentException) { break; }
                }
            }

            #endregion

            // Initialize screens
            RenderingWindows = new List<RenderingWindow>(theHost.ScreenCount);
            for (int i = 0; i < RenderingWindows.Capacity; i++)
                RenderingWindows.Add(new RenderingWindow(i, InitialScreenSize, Fullscreen));

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

            InputRouter.Initialize();

            Thread rapidMenu = new Thread(Core.initialize);
            rapidMenu.Name = "OpenMobile.Core.rapidMenu";
            rapidMenu.Start();
            
            for (int i = 1; i < RenderingWindows.Count; i++)
                RenderingWindows[i].RunAsync();
            RenderingWindows[0].Run();

            try
            {
                for (int i = 0; i < RenderingWindows.Count; i++)
                    RenderingWindows[i].Dispose();
                for (int i = 0; i < pluginCollection.Count; i++)
                    if (pluginCollection[i] != null)
                        pluginCollection[i].Dispose();
                InputRouter.Dispose();
                theHost.Dispose();
                OpenMobile.Threading.SafeThread.Dispose();
                BuiltInComponents.Dispose();
                Application.Dispose();
            }
            catch
            {   // No use of logging messages as we've already disposed the debug plugin...
            }
            Thread.Sleep(2500);
            Environment.Exit(0); //Force
        }

        internal static bool loadPlugin(string arg)
        {
            int count = pluginCollection.Count;
            loadAndCheck(null, arg);
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
