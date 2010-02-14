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
using System.Windows.Forms;
using Microsoft.Win32;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace OpenMobile
{
    public static class Core
    {
        public static PluginHost theHost = new PluginHost();
        public static List<RenderingWindow> RenderingWindows=new List<RenderingWindow>(theHost.ScreenCount);
        public static List<IBasePlugin> pluginCollection = new List<IBasePlugin>();
        public static bool exitTransition = true;

        private static void loadMainMenu()
        {

            Assembly pluginAssembly = Assembly.Load("UI");
            //Modifications by Borte
            IHighLevel availablePlugin=null;
            try
            {
                availablePlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetTypes()[0]);
            }
            catch 
            {
                //handled below
            }
            if (availablePlugin==null)
                availablePlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.UI"));
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
                mmPlugin = (IHighLevel)Activator.CreateInstance(pluginAssembly.GetType("OpenMobile.MainMenu"));
            //End Modifications by Borte
            pluginCollection.Add(mmPlugin);
            mmPlugin.initialize(theHost);
            var a=mmPlugin.GetType().GetCustomAttributes(typeof(InitialTransition),false);
            for (int i = 0; i <RenderingWindows.Count; i++)
            {
                RenderingWindows[i].transitionInPanel(availablePlugin.loadPanel("", i));
                RenderingWindows[i].transitionInPanel(mmPlugin.loadPanel("", i));
                theHost.raiseSystemEvent(eFunction.TransitionToPanel, i.ToString(), "MainMenu", "");
                if (a.Length==0)
                    RenderingWindows[i].executeTransition(eGlobalTransition.None);
                else
                    RenderingWindows[i].executeTransition(((InitialTransition)a[0]).Transition);
            }
            var b = mmPlugin.GetType().GetCustomAttributes(typeof(FinalTransition), false);
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
                        //Gets a type object of the interface we need the plugins to match
                        Type typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IHighLevel", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IHighLevel availablePlugin = (IHighLevel)Activator.CreateInstance(pluginType);
                            
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IAVPlayer", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IAVPlayer availablePlugin = (IAVPlayer)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IOther", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IOther availablePlugin = (IOther)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IRawHardware", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IRawHardware availablePlugin = (IRawHardware)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IMediaDatabase", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface !=null)
                        {
                            IMediaDatabase availablePlugin = (IMediaDatabase)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.ITunedContent", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface !=null)
                        {
                            ITunedContent availablePlugin = (ITunedContent)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.IDataProvider", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            IDataProvider availablePlugin = (IDataProvider)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.INetwork", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            INetwork availablePlugin = (INetwork)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                        //Next one
                        typeInterface = pluginType.GetInterface("OpenMobile.Plugin.ISpeech", true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            ISpeech availablePlugin = (ISpeech)Activator.CreateInstance(pluginType);
                            pluginCollection.Add(availablePlugin);
                            return;
                        }
                    }
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom(Path.Combine(theHost.PluginPath, args.Name + ".dll"));
        }
        private static eLoadStatus[] status;
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
                    status[i]= pluginCollection[i].initialize(theHost);
                }
                catch (Exception) { status[i] = eLoadStatus.LoadFailedUnloadRequested; }
            }
            for(int i=2;i<pluginCollection.Count;i++) //Give them all a second chance if they need it
                try
                {
                    if (status[i]==eLoadStatus.LoadFailedRetryRequested)
                        status[i] = pluginCollection[i].initialize(theHost);
                }
                catch (Exception) { status[i] = eLoadStatus.LoadFailedUnloadRequested; }
            for(int i=2;i<pluginCollection.Count;i++) //and then two strikes their out...kill anything that still can't initialize
                if ((status[i]==eLoadStatus.LoadFailedRetryRequested)||(status[i]==eLoadStatus.LoadFailedUnloadRequested))
                {
                    try
                    {
                        pluginCollection[i].Dispose();
                        theHost.execute(eFunction.backgroundOperationStatus, "PLUGIN CRASHED: " + pluginCollection[i].pluginName);
                    }
                    catch (Exception) { }
                    pluginCollection.RemoveAt(i);
                }
        }

        private static void initialize()
        {
            //Uncomment these to time the startup
            //DateTime start = DateTime.Now;
            if (Directory.Exists(theHost.DataPath) == false)
                Directory.CreateDirectory(theHost.DataPath);
            if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
            {
                using (PluginSettings settings = new PluginSettings())
                    settings.createDB();
                if (File.Exists(Path.Combine(theHost.DataPath, "OMData")) == false)
                {
                    RenderingWindows[0].Invoke(RenderingWindows[0].ShowMessage, new object[] { (object)"A required SQLite database OMData was not found in the application directory.  An attempt to create the database failed!  This database is required for Open Mobile to run." });
                    Application.Exit();
                    return;
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            loadMainMenu();
            loadEmUp();
            theHost.hal = new HalInterface();
            getEmReady();
            theHost.raiseSystemEvent(eFunction.pluginLoadingComplete,"","","");
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(theHost.NetworkChange_NetworkAvailabilityChanged);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(theHost.NetworkChange_NetworkAddressChanged);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(theHost.SystemEvents_PowerModeChanged);
            SystemEvents.SessionEnding += new SessionEndingEventHandler(theHost.SystemEvents_SessionEnding);
            OpenMobile.Threading.TaskManager.Enable(); //Start executing background tasks
            if (Net.Network.IsAvailable==true)
                theHost.raiseSystemEvent(eFunction.connectedToInternet, "", "", "");
            pluginCollection.TrimExcess();
        }

        [STAThread]
        public static void Main()
        {
            for (int i = 0; i < RenderingWindows.Capacity; i++)
                RenderingWindows.Add(new RenderingWindow(i));
            Thread rapidMenu=new Thread(new ThreadStart(Core.initialize));
            rapidMenu.Start();
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                if (Environment.GetCommandLineArgs()[1].ToLower() == "-fullscreen")
                {
                    for (int i = 0; i < RenderingWindows.Count; i++)
                    {
                        RenderingWindows[i].fullscreen = true;
                        RenderingWindows[i].FormBorderStyle = FormBorderStyle.None;
                        RenderingWindows[i].TopMost = true;
                        RenderingWindows[i].WindowState = FormWindowState.Maximized;
                    }
                }
                if (Environment.GetCommandLineArgs()[1].ToLower().StartsWith("-size=")==true)
                {
                    string[] part = Environment.GetCommandLineArgs()[1].Substring(6).Split(new char[] { 'x' });
                    for (int i = 0; i < RenderingWindows.Count; i++)
                    {
                        try
                        {
                            RenderingWindows[i].Size = new System.Drawing.Size(int.Parse(part[0]), int.Parse(part[1]));
                        }
                        catch (ArgumentException) { break; }
                    }
                }
            }
            for (int i = 1; i < theHost.ScreenCount; i++)
                RenderingWindows[i].Show();
            Application.Run(RenderingWindows[0]);
            foreach (IBasePlugin p in pluginCollection)
                p.Dispose();
            Environment.Exit(0); //Force
        }
    }
}
