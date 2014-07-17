using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Data;
using System.Diagnostics;

namespace OMDebug
{
    public sealed class Debugger:IOther
    {
        private class DebugMessage
        {
            /* Messages can be classified by "tagging" the message with a code in the beginning of the string
            * (this is done automatically if the helperfunctions is used, otherwise tag the messages with the 
            * following format: code|message
            * Code description:
            *      E   :   Error
            *      W   :   Warning
            *      I   :   Info (this is the default level if no code is present)
           */
            
            public string Text { get; set; }
            public DebugMessageType Type { get; set; }

            public DebugMessage()
            {
                this.Type = DebugMessageType.Info;
            }
            public DebugMessage(string Text)
            {
                this.Text = Text;
                this.Type = DebugMessageType.Info;
            }
            public DebugMessage(string Text, DebugMessageType Type)
            {
                this.Text = Text;
                this.Type = Type;
            }
            public DebugMessage(DebugMessageType Type)
            {
                this.Text = "";
                this.Type = Type;
            }

            public static DebugMessage Decode(string RawMessage)
            {
                DebugMessage Msg = new DebugMessage();
                if (RawMessage.Contains("|"))
                {
                    string Type = RawMessage.Substring(0, RawMessage.IndexOf('|')).ToLower();
                    Msg.Text = RawMessage.Substring(RawMessage.IndexOf('|')+1).Trim();
                    if (Type == "")
                        Msg.Type = DebugMessageType.Unspecified;
                    else
                    {
                        Msg.Type = DebugMessageType.Unspecified;
                        foreach (DebugMessageType MsgType in Enum.GetValues(typeof(DebugMessageType)))
                            if (Type == MsgType.ToString().Substring(0, 1).ToLower())
                                Msg.Type = MsgType;
                    }
                }
                else
                {
                    Msg.Text = RawMessage;
                    Msg.Type = DebugMessageType.Unspecified;
                }
                return Msg;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private DebugMessageType OutputFilter = DebugMessageType.Unspecified;
        private Settings settings;

        public Settings loadSettings()
        {
            if ((settings == null) && (theHost != null))
            {
                settings = new Settings("Debug settings");
                using (PluginSettings setting = new PluginSettings())
                {
                    // Output filter level
                    List<string> OptionList = new List<string>(Enum.GetNames(typeof(DebugMessageType)));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "OMDebug.OutputFilter", "Filter", "Minimum log messages level", OptionList, OptionList, setting.getSetting(this, "OMDebug.OutputFilter")));

                    settings.Add(new Setting(SettingTypes.Button, "OMDebug.OpenDebugLog", String.Empty, "Open debug log"));

                    settings.Add(new Setting(SettingTypes.Button, "OMDebug.ClearDebugLog", String.Empty, "Clear debug log!"));
                }
                settings.OnSettingChanged += new SettingChanged(Setting_Changed);
            }
            return settings;
        }

        private void Setting_Changed(int screen, Setting s)
        {
            switch (s.Name)
            {
                case "OMDebug.OpenDebugLog":
                    {
                        Process.Start(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"));
                    }
                    break;

                case "OMDebug.ClearDebugLog":
                    {
                        ClearLog();
                    }
                    break;

                case "OMDebug.OutputFilter":
                    {
                        using (PluginSettings settings = new PluginSettings())
                            settings.setSetting(this, s.Name, s.Value);
                        try
                        {
                            OutputFilter = (DebugMessageType)Enum.Parse(typeof(DebugMessageType), s.Value);
                        }
                        catch
                        {
                            OutputFilter = DebugMessageType.Unspecified;
                        }
                        if (time == 0)
                            time = Environment.TickCount;
                        writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " : OMDebug.OutputFilter set to " + OutputFilter.ToString());
                    }
                    break;

                default:
                    break;
            }

        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz / Borte"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@users.sourceforge.net"; }
        }

        public string pluginName
        {
            get { return "OMDebug"; }
        }

        public float pluginVersion
        {
            get { return 1.0F; }
        }

        public string pluginDescription
        {
            get { return "Provides a basic debugger for Open Mobile"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-Debug"); }
        }

        private void ClearLog()
        {
            lock (writer)
            {
                writer.Close();
                System.IO.File.Delete(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"));
                writer = new StreamWriter(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"), true);
                writer.WriteLine(String.Format("Log file cleared at {0}", DateTime.Now));
            }
        }

        public bool incomingMessage(string message, string source)
        {
            /* Messages can be classified by "tagging" the message with a code in the beginning of the string
             * (this is done automatically if the helperfunctions is used, otherwise tag the messages with the 
             * following format: code|message
             * Code description:
             *      E   :   Error
             *      W   :   Warning
             *      I   :   Info (this is the default level if no code is present)
            */
            if (message == "CLEARLOG")
                ClearLog();

            WriteToLog("(" + source + ") => \t", DebugMessage.Decode(message));
            return true;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // If this is an string array then dump the whole array
            if (data is string[])
            {
                string[] Msgs = data as string[];
                DebugMessage msg = DebugMessage.Decode(message);
                //WriteToLog(new DebugMessage("(" + source + ") => \t" + msg.Text, msg.Type));
                WriteToLog(new DebugMessage("(" + source + ") => \t" + msg.Text, msg.Type), Msgs);
                return true;
            }
            return false;
        }

        IPluginHost theHost;
        StreamWriter writer;
        int time = 0;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;

            // Get setting from DB
            using (PluginSettings setting = new PluginSettings())
            {
                string value = setting.getSetting(this, "OMDebug.OutputFilter");
                try
                {
                    OutputFilter = (DebugMessageType)Enum.Parse(typeof(DebugMessageType), setting.getSetting(this, "OMDebug.OutputFilter"));
                }
                catch
                {
                    OutputFilter = DebugMessageType.Unspecified;
                }
            }

            writer = new StreamWriter(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"), true);
            if (time == 0)
                time = Environment.TickCount;
            writer.WriteLine("");
            writer.WriteLine("");
            writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " : ***** OpenMobile startup initiated at " + DateTime.Now + " (Current filter level = " + OutputFilter.ToString() + ")*****");

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            OpenMobile.Threading.SafeThread.Asynchronous(() => LateInit(), theHost);
            OpenMobile.Threading.SafeThread.Asynchronous(() => writeGraphicsInfo(), theHost);
            return eLoadStatus.LoadSuccessful;
        }
        private void LateInit()
        {
            // Get setting from DB
            using (PluginSettings setting = new PluginSettings())
            {
                string value = setting.getSetting(this, "OMDebug.OutputFilter");
                try
                {
                    OutputFilter = (DebugMessageType)Enum.Parse(typeof(DebugMessageType), setting.getSetting(this, "OMDebug.OutputFilter"));
                }
                catch
                {
                    OutputFilter = DebugMessageType.Unspecified;
                }
            }

            // Connect events
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            theHost.OnKeyPress += new KeyboardEvent(theHost_OnKeyPress);

            // Software info
            List<string> Texts = new List<string>();
            Texts.Add("OS: " + OpenMobile.Framework.OSSpecific.getOSVersion());
            Texts.Add("Framework: " + OpenMobile.Framework.OSSpecific.getFramework());
            Texts.Add("OpenMobile: v" + Assembly.GetEntryAssembly().GetName().Version.ToString());
            Texts.Add("OS environment: " + OpenMobile.Framework.OSSpecific.getOSEnvironment() + "bit");
            Texts.Add("App environment: " + OpenMobile.Framework.OSSpecific.getAppEnvironment() + "bit");
            if (System.Diagnostics.Debugger.IsAttached == true)
                Texts.Add("IDE: " + "Yes (Debugger attached)");

            // Dump version info for all OpenMobile system files
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                string AssemblyName = a.GetName().Name;
                if (AssemblyName.ToLower().Contains("openmobile"))
                {
                    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);
                    Texts.Add(AssemblyName + ": Version=" + a.GetName().Version + ", FileVersion=" + string.Format("{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart));
                }
            }

            WriteToLog(false, "------------------Software-------------------", new DebugMessage(DebugMessageType.Info),Texts.ToArray());

            // Hardware info
            Texts.Clear();
            Texts.Add("Processors: " + Environment.ProcessorCount);
            Texts.Add("Architecture: " + OpenMobile.Framework.OSSpecific.getArchitecture().ToString());
            Texts.Add("Screens: " + OM.Host.ScreenCount.ToString());
            Texts.Add("Embedded: " + Configuration.RunningOnEmbedded);
            WriteToLog(false, "------------------Hardware-------------------", new DebugMessage(DebugMessageType.Info), Texts.ToArray());

            // Initial assemblies
            Texts.Clear();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                //System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);
                //Texts.Add("LOADED -> " + a.GetName() + ", FileVersion=" + string.Format("{0}.{1}.{2}.{3}",fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart));
                Texts.Add("PRELOADED -> " + a.GetName());
            }
            Texts.Add("---------------------------------------------");
            WriteToLog(false, "--------------Inital Assemblies--------------", new DebugMessage(DebugMessageType.Info), Texts.ToArray());
        }
        private void writeGraphicsInfo()
        {
            // Wait for graphics to initialize
            while (Graphics.Version == null)
                Thread.Sleep(10);

            // Software info
            List<string> Texts = new List<string>();
            Texts.Add(Graphics.GLType + " v" + Graphics.Version);
            Texts.Add("Graphics Engine: " + Graphics.GraphicsEngine);
            Texts.Add("Graphics Card: " + Graphics.Renderer);
            Texts.Add("---------------------------------------------");
            WriteToLog(false, "------------------Graphics Initialized-------------------", new DebugMessage(DebugMessageType.Info), Texts.ToArray());
        }

        bool theHost_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg, ref bool handled)
        {
            if (arg.Key == OpenMobile.Input.Key.VolumeUp)
                WriteToLog("(Event:OnKeyPress) => \tVolume Up!");
            if (arg.Key == OpenMobile.Input.Key.VolumeDown)
                WriteToLog("(Event:OnKeyPress) => \tVolume Down!");
            return false;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            List<string> Texts = new List<string>();
            Texts.Add("Exception Message: " + ex.Message);
            Texts.Add("Fatal: " + e.IsTerminating.ToString());
            Texts.Add("Source: " + ex.Source);
            Texts.Add("Stack Trace: " + ex.StackTrace);
            Texts.Add("Stack Trace: " + ex.StackTrace);
            Texts.Add("Relevant Data: " + getData(ex.Data));
            Texts.Add("Exception Message: " + ex.Message);
            Texts.Add("----------------------------------------------------------------");
            WriteToLog(true, "-------------Exception: " + ex.GetType().Name + "-----------------", new DebugMessage(DebugMessageType.Error), Texts.ToArray());
        }

        private string getData(System.Collections.IDictionary iDictionary)
        {
            string ret="";
            foreach (object o in iDictionary.Values)
                ret+=o.ToString();
            return ret;
        }

        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            WriteToLog("(Event:AssemblyLoad) => \t" + args.LoadedAssembly.GetName() + ")@" + args.LoadedAssembly.CodeBase);
        }

        void theHost_OnSystemEvent(OpenMobile.eFunction function, object[] args)
        {
            List<string> Texts = new List<string>();
            
            if ((function == eFunction.userInputReady)
                || (function == eFunction.multiTouchGesture))
                return; //Protect Users Privacy - Potentially contains password info

            // Log arguments
            string sArgs = "";
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    sArgs += String.Format(", arg{0}: {1}", i, args[i]);
                }
            }
            WriteToLog("(Event:OnSystemEvent) => \t" + function.ToString() + sArgs);

            if (function == eFunction.inputRouterInitialized)
            {
                // List audio devices
                string[] Devices = theHost.getData(eGetData.GetAudioDevices, "") as string[];
                if (Devices != null)
                    for (int i = 0; i < Devices.Length; i++)
                        Texts.Add("AudioDevice(" + i.ToString() + "): " + Devices[i]);
                else
                    Texts.Add("AudioDevice: No data/units available");

                //// List keyboard devices
                //foreach (string s in (theHost.getData(eGetData.GetAvailableKeyboards, "") as string[]))
                //    Texts.Add("Keyboard unit: " + s);

                //// List mice devices
                //foreach (string s in (theHost.getData(eGetData.GetAvailableMice, "") as string[]))
                //    Texts.Add("Mice unit: " + s);

                // Write out current multizone settings
                Texts.Add("Multizone settings: ");
                using (PluginSettings settings = new PluginSettings())
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        Texts.Add("Screen" + (i + 1).ToString() + ".SoundCard: " + settings.getSetting(BuiltInComponents.OMInternalPlugin, "Screen" + (i + 1).ToString() + ".SoundCard"));
                        Texts.Add("Screen" + (i + 1).ToString() + ".Keyboard: " + settings.getSetting(BuiltInComponents.OMInternalPlugin, "Screen" + (i + 1).ToString() + ".Keyboard"));
                        Texts.Add("Screen" + (i + 1).ToString() + ".Mouse: " + settings.getSetting(BuiltInComponents.OMInternalPlugin, "Screen" + (i + 1).ToString() + ".Mouse"));
                    }
                }
                Texts.Add("---------------------------------------------");
                WriteToLog(false, "------------------MultiZone devices-------------------", new DebugMessage(DebugMessageType.Info), Texts.ToArray());
            }
        }
        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            WriteToLog("(Event:OnStorageEvent) => \t" + type.ToString() + ", " + justInserted + ", " + arg);
        }
        void theHost_OnPowerChange(OpenMobile.ePowerEvent type)
        {
            WriteToLog("(Event:OnPowerChange) => \t" + type.ToString());
        }
        void theHost_OnMediaEvent(OpenMobile.eFunction function, Zone zone, string arg)
        {
            if (zone != null)
                WriteToLog("(Event:OnMediaEvent) => \t" + function.ToString() + ", [Zone:" + zone.Name.ToString() + "], " + arg);
            else
                WriteToLog("(Event:OnMediaEvent) => \t" + function.ToString() + ", [Zone:NULL], " + arg);
        }

        private void WriteToLog(DebugMessage[] Msg)
        {
            foreach (DebugMessage m in Msg)
                WriteToLog(false, "\t", m);
        }
        private void WriteToLog(DebugMessage Msg)
        {
            WriteToLog(true, "", Msg);
        }
        private void WriteToLog(DebugMessage Msg, string[] texts)
        {
            WriteToLog(true, "", Msg, texts);
        }
        private void WriteToLog(string text)
        {
            WriteToLog(true, "", new DebugMessage(text));
        }
        private void WriteToLog(bool TimeStamp, string text)
        {
            WriteToLog(TimeStamp, "", new DebugMessage(text));
        }
        private void WriteToLog(string text, DebugMessage Msg)
        {
            WriteToLog(true, text, Msg);
        }
        private void WriteToLog(string text, DebugMessage Msg, string[] texts)
        {
            WriteToLog(true, text, Msg, texts);
        }
        private void WriteToLog(bool TimeStamp, string header, DebugMessage Msg)
        {
            if (time == 0)
                time = Environment.TickCount;

            // Filter messages
            if (Msg.Type < OutputFilter)
                return;

            // Print error and warning messages to console
            bool IDE = false;
            if ((System.Diagnostics.Debugger.IsAttached) && ((Msg.Type == DebugMessageType.Error) | (Msg.Type == DebugMessageType.Warning)))
                IDE = true;

            if (writer == null)
            {
                if (TimeStamp)
                    queue.Add(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + header + Msg.ToString());
                else
                    queue.Add(header + Msg.ToString());
            }
            else if (writer.BaseStream != null)
            {
                lock (writer)
                {

                    // Write out any queued messages
                    if (queue.Count > 0)
                    {
                        foreach (string queued in queue)
                        {
                            if (IDE)
                                System.Diagnostics.Debug.WriteLine(queued);
                            writer.WriteLine(queued);
                        }
                        queue.Clear();
                    }

                    if (TimeStamp)
                    {
                        string Text = (((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + header + Msg.ToString());
                        if (IDE)
                            System.Diagnostics.Debug.WriteLine(Text);
                        writer.WriteLine(Text);
                    }
                    else
                    {
                        string Text = header + Msg.ToString();
                        if (IDE)
                            System.Diagnostics.Debug.WriteLine(Text);
                        writer.WriteLine(Text);
                    }
                    writer.Flush();
                }
            }
        }
        private void WriteToLog(bool TimeStamp, string header, DebugMessage Msg, string[] texts)
        {
            if (time == 0)
                time = Environment.TickCount;
            
            // Filter messages
            if (Msg.Type < OutputFilter)
                return;
            
            // Print error and warning messages to console
            bool IDE = false;
            if ((System.Diagnostics.Debugger.IsAttached) && ((Msg.Type == DebugMessageType.Error) | (Msg.Type == DebugMessageType.Warning)))
                IDE = true;
          
            if (writer == null)
            {
                if (TimeStamp)
                    queue.Add(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + header + Msg.ToString());
                else
                    queue.Add(header + Msg.ToString());
            }
            else if (writer.BaseStream != null)
            {
                lock (writer)
                {
                    // Write out any queued messages
                    if (queue.Count > 0)
                    {
                        foreach (string queued in queue)
                        {
                            if (IDE)
                                System.Diagnostics.Debug.WriteLine(queued);
                            writer.WriteLine(queued);
                        }
                        queue.Clear();
                    }

                    if (TimeStamp)
                    {
                        string Text = ((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + header + Msg.ToString();
                        if (IDE)
                            System.Diagnostics.Debug.WriteLine(Text);
                        writer.WriteLine(Text);
                    }
                    else
                    {
                        string Text = header + Msg.ToString();
                        if (IDE)
                            System.Diagnostics.Debug.WriteLine(Text);
                        writer.WriteLine(Text);
                    }

                    // Write out text array
                    foreach (string text in texts)
                    {
                        if (IDE)
                            System.Diagnostics.Debug.WriteLine("\t" + text);
                        writer.WriteLine("\t" + text);
                    }

                    writer.Flush();
                }
            }
        }

        List<string> queue = new List<string>();
        private void WriteToQueue(bool TimeStamp, string text)
        {
            if (TimeStamp)
                queue.Add(((Environment.TickCount - time) / 1000.0).ToString("0.000") + ": " + text);
            else
                queue.Add(text);
        }
        private void WriteToQueue(string[] text)
        {
            foreach (string s in text)
                WriteToQueue(false, "\t" + s);
        }
        private void WriteToQueue(string text)
        {
            WriteToQueue(true, text);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //if (writer!=null)
            //    writer.Dispose();
            //GC.SuppressFinalize(this);
        }

        #endregion
    }
}
