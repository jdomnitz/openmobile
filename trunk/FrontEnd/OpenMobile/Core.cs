﻿/*********************************************************************************
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
        private static void loadMainMenu()
        {

            Assembly pluginAssembly = Assembly.Load("UI");
            //Modifications by Borte
            IHighLevel availablePlugin=null;
            try
            {
                availablePlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetTypes()[0]);
            }
            catch (Exception e)
            {
                //handled below
            }
            if (availablePlugin == null)
            {
                availablePlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.UI"));
                if (availablePlugin == null)
                    Application.ShowError(RenderingWindows[0].WindowHandle, "No UI Skin available!", "No skin available!");
            }
            //End Modifications by Borte
            pluginCollection.Add(availablePlugin);
            availablePlugin.initialize(theHost);
            
            pluginAssembly = Assembly.Load("MainMenu");
            //Modifications by Borte
            IHighLevel mmPlugin=null;
            try
            {
                mmPlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetTypes()[0]);
            }
            catch { }
            if (mmPlugin == null)
            {
                mmPlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.MainMenu"));
                if (mmPlugin == null)
                    Application.ShowError(RenderingWindows[0].WindowHandle, "No Main Menu Skin available!", "No skin available!");
            }
            //End Modifications by Borte
            pluginCollection.Add(mmPlugin);
            mmPlugin.initialize(theHost);
            var a=mmPlugin.GetType().GetCustomAttributes(typeof(InitialTransition),false);
            new Thread(() =>
            {
                for (int i = 0; i < RenderingWindows.Count; i++)
                {
                    RenderingWindows[i].transitionInPanel(availablePlugin.loadPanel("", i));
                    RenderingWindows[i].executeTransition(eGlobalTransition.None);
                    theHost.execute(eFunction.TransitionToPanel, i.ToString(), "MainMenu", "");
                    if (a.Length == 0)
                        RenderingWindows[i].executeTransition(eGlobalTransition.None);
                    else
                        RenderingWindows[i].executeTransition(((InitialTransition)a[0]).Transition);
                }
            }).Start();
            object[] b = mmPlugin.GetType().GetCustomAttributes(typeof(FinalTransition), false);
            if (b.Length > 0)
                exitTransition=((FinalTransition)b[0]).Transition;
        }
        /// <summary>
        /// Load each of the plugins into the plugin's array (pluginCollection)
        /// </summary>
        private static void loadEmUp()
        {
            foreach (string file in Directory.GetFiles(theHost.PluginPath,"*.dll"))
            {
                try
                {
                    loadAndCheck(file);
                }catch(Exception)
                {}
            }
            foreach (string file in Directory.GetFiles(theHost.SkinPath, "*.dll"))
            {
                try
                {
                    loadAndCheck(file);
                }
                catch (Exception)
                { }
            }
        }
        private static void loadAndCheck(string file)
        {
            if (file.EndsWith("UI.dll") || file.EndsWith("MainMenu.dll"))
                return;
            string last = Path.GetFileNameWithoutExtension(file);
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath= Path.Combine(theHost.PluginPath, last);
            Assembly pluginAssembly = Assembly.Load(last);
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic) //Only look at public types
                {
                    if (!pluginType.IsAbstract)  //Only look at non-abstract types
                    {
                        Type typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IBasePlugin");

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IBasePlugin availablePlugin = (IBasePlugin)Activator.CreateInstance(pluginType);
							if (typeof(INetwork).IsInstanceOfType(availablePlugin))
								((INetwork)availablePlugin).OnWirelessEvent+=theHost.raiseWirelessEvent;
                            pluginCollection.Add(availablePlugin);
                        }
                    }
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string loc = Path.Combine(theHost.PluginPath, args.Name + ".dll");
            if (File.Exists(loc))
                return Assembly.LoadFrom(loc);
            else
                return Assembly.LoadFrom(Path.Combine(theHost.SkinPath, args.Name + ".dll"));
        }
        public static eLoadStatus[] status;
        /// <summary>
        /// Initialize each of the plugins in the plugin's array (pluginCollection)
        /// </summary>
        private static void getEmReady()
        {
            status = new eLoadStatus[pluginCollection.Count];
            for (int i = 2; i < pluginCollection.Count;i++ ) //Try to initialize the plugins
            {
                try
                {
                    theHost.sendMessage("OMDebug", "Plugin Manager", "Initializing "+pluginCollection[i].pluginName);
                    if (pluginCollection[i]!=null)
                        status[i]= pluginCollection[i].initialize(theHost);
                }
                catch (Exception e)
                {
                    status[i] = eLoadStatus.LoadFailedUnloadRequested;
                    string ex = spewException(e);
                    theHost.sendMessage("OMDebug","Plugin Manager",ex);
                    Debug.Print(ex);
                }
            }
            for(int i=2;i<pluginCollection.Count;i++) //Give them all a second chance if they need it
                try
                {
                    if (status[i]==eLoadStatus.LoadFailedRetryRequested)
                        status[i] = pluginCollection[i].initialize(theHost);
                }
                catch (Exception e)
                { 
                    status[i] = eLoadStatus.LoadFailedUnloadRequested;
                    string ex = spewException(e);
                    theHost.sendMessage("OMDebug", "Plugin Manager", ex);
                    Debug.Print(ex);
                }
            for(int i=2;i<pluginCollection.Count;i++) //and then two strikes their out...kill anything that still can't initialize
                if ((status[i] == eLoadStatus.LoadFailedRetryRequested) || (status[i] == eLoadStatus.LoadFailedUnloadRequested) || (status[i] == eLoadStatus.LoadFailedGracefulUnloadRequested))
                {
                    try
                    {
                        pluginCollection[i].Dispose();
                        if (status[i] != eLoadStatus.LoadFailedGracefulUnloadRequested)
                            theHost.execute(eFunction.backgroundOperationStatus, pluginCollection[i].pluginName+" CRASHED!");
                    }
                    catch (Exception) { }
                    pluginCollection[i]=null;
                }
            status=null;
            pluginCollection.RemoveAll(p => p == null);
        }

        private static void initialize()
        {
            //Uncomment these to time the startup
            //DateTime start = DateTime.Now;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            loadMainMenu();
            loadEmUp();
            theHost.hal = new HalInterface();
            theHost.load(); //Stagger I/O
            getEmReady();
            theHost.raiseSystemEvent(eFunction.pluginLoadingComplete,"","","");
			theHost.hal.snd("32");
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(theHost.NetworkChange_NetworkAvailabilityChanged);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(theHost.NetworkChange_NetworkAddressChanged);
            OpenMobile.Threading.TaskManager.Enable(Core.theHost); //Start executing background tasks
            SystemEvents.DisplaySettingsChanged+=new EventHandler(theHost.SystemEvents_DisplaySettingsChanged);
            if (OpenMobile.Net.Network.IsAvailable==true)
                theHost.raiseSystemEvent(eFunction.connectedToInternet, "", "", "");
            using (PluginSettings settings = new PluginSettings())
            {
                if (settings.getSetting("UI.HideCursor") == "True")
                    for (int i = 0; i < RenderingWindows.Count; i++)
                        RenderingWindows[i].hideCursor();
                if (settings.getSetting("UI.MinGraphics") == "True")
                    theHost.GraphicsLevel = eGraphicsLevel.Minimal;
            }
            pluginCollection.TrimExcess();
            ThreadPool.SetMaxThreads(50, 500);
            Application.Run();
        }
        static bool ErroredOut;
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ErroredOut) //Prevent multiple bugs
                return;
            ErroredOut = true;
            Exception ex = (Exception)e.ExceptionObject;
            if (theHost.hal!=null)
                theHost.hal.close();
            string strEx = spewException(ex);
            FileStream fs=File.OpenWrite(Path.Combine(theHost.DataPath, "AppCrash-"+DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day.ToString()+".log"));
            fs.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(strEx), 0, strEx.Length);
            fs.Close();
            //ErrorReporting reporting=new ErrorReporting(strEx);
            //reporting.ShowDialog(System.Windows.Forms.Form.FromHandle(RenderingWindows[0].getHandle()));
            //if ((DateTime.Now- Process.GetCurrentProcess().StartTime).TotalMinutes>1) //Prevent Loops
            //    theHost.execute(eFunction.restartProgram);
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
            return err;
        }
        [STAThread]
        public static void Main()
        {
            // Added by Borte to be able to set amount of screens with startup parameter
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.ToLower().StartsWith("-screencount=") == true)
                {
                    try
                    {
                        theHost.ScreenCount = int.Parse(arg.Substring(13));
                    }
                    catch (ArgumentException) { break; }
                }
                else if (arg.ToLower()=="-fullscreen")
                {
                    Fullscreen = GameWindowFlags.Fullscreen;
                }
                // Override current skin to use
                else if (arg.ToLower().StartsWith("-skinpath=") == true)
                {
                    theHost.SkinPath = arg.Substring(10);
                }
            }
            // Initialize screens
            RenderingWindows = new List<RenderingWindow>(theHost.ScreenCount);
            for (int i = 0; i < RenderingWindows.Capacity; i++)
                RenderingWindows.Add(new RenderingWindow(i));
            if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
            {
                using (PluginSettings settings = new PluginSettings())
                    settings.createDB();
                if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
                {
                    Application.ShowError(IntPtr.Zero,"A required SQLite database OMData was not found in the application directory.  An attempt to create the database failed!  This database is required for Open Mobile to run.","Database Missing!");
                    Environment.Exit(0);
                    return;
                }
            }
            Thread rapidMenu=new Thread(new ThreadStart(Core.initialize));
            rapidMenu.Start();
            if (RenderingWindows.Count == 0)
                throw new PlatformException("Unable to detect any monitors on this platform!");
            for (int i = 1; i<RenderingWindows.Count; i++)
                RenderingWindows[i].RunAsync(Fullscreen);
            RenderingWindows[0].Run(Fullscreen);
            for (int i = 0; i < RenderingWindows.Count; i++)
                RenderingWindows[i].Dispose();
            for (int i = 0; i < pluginCollection.Count;i++ )
                if (pluginCollection[i] != null)
                    pluginCollection[i].Dispose();
            Environment.Exit(0); //Force
        }
    }
}
