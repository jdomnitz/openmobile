using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Data;

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

            public static DebugMessage Decode(string RawMessage)
            {
                DebugMessage Msg = new DebugMessage();
                string Type = RawMessage.Substring(0, RawMessage.IndexOf('|')).ToLower();
                Msg.Text = RawMessage.Substring(RawMessage.IndexOf('|'));
                if (Type == "")
                    Msg.Type = DebugMessageType.Info;
                else
                {
                    switch (Type)
                    {
                        case "e":
                            Msg.Type = DebugMessageType.Error;
                            break;
                        case "w":
                            Msg.Type = DebugMessageType.Warning;
                            break;
                        case "i":
                            Msg.Type = DebugMessageType.Info;
                            break;
                        default:
                            Msg.Type = DebugMessageType.Info;
                            break;
                    }
                }
                return Msg;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private DebugMessageType OutputFilter = DebugMessageType.Info;
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
                    settings.Add(new Setting(SettingTypes.MultiChoice, "OMDebug.OutputFilter", "Filter", "Show messages of this type and above", OptionList, OptionList, setting.getSetting("OMDebug.OutputFilter")));
                }
                settings.OnSettingChanged += new SettingChanged(Setting_Changed);
            }
            return settings;
        }

        private void Setting_Changed(int screen, Setting s)
        {
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting(s.Name, s.Value);
            try
            {
                OutputFilter = (DebugMessageType)Enum.Parse(typeof(DebugMessageType), s.Value);
            }
            catch
            {
                OutputFilter = DebugMessageType.Info;
            }
            writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " : OMDebug.OutputFilter set to " + OutputFilter.ToString());
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
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
            get { return 0.5F; }
        }

        public string pluginDescription
        {
            get { return "Provides a basic debugger for Open Mobile"; }
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

            //WriteToLog("********"+source + "******\r\n" + message);
            WriteToLog("(" + source + ") => \t", DebugMessage.Decode(message));
            return true;
        }
        /*
        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            writer.WriteLine("-------------Exception " + ex.Message + "-----------------");
            log("Source: "+ex.Source);
            log("Stack Trace: " + ex.StackTrace);
            writer.WriteLine("----------------------------------------------------------------");
        }*/

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // If this is an string array then dump the whole array
            if (data is string[])
            {
                string[] Msgs = data as string[];
                DebugMessage msg = DebugMessage.Decode(message);
                WriteToLog(new DebugMessage("(" + source + ") => \t" + msg.Text + "\r\n", msg.Type));
                foreach (string s in Msgs)
                    WriteToLog(false, "\t", new DebugMessage(s, msg.Type));
                return true;
            }
            return false;
        }

        IPluginHost theHost;
        StreamWriter writer;
        int time;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;

            using (PluginSettings setting = new PluginSettings())
            {
                string value = setting.getSetting("OMDebug.OutputFilter");
                try
                {
                    OutputFilter = (DebugMessageType)Enum.Parse(typeof(DebugMessageType), setting.getSetting("OMDebug.OutputFilter"));
                }
                catch
                {
                    OutputFilter = DebugMessageType.Info;
                }

            }
            writer = new StreamWriter(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"), true);
            time = Environment.TickCount;
            writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " : ***** OpenMobile startup initiated (Current filter level = " + OutputFilter.ToString() + ")*****");
            //WriteToLog(" : ***** OpenMobile startup initiated (Current filter level = " + OutputFilter.ToString() + ")*****");

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            writeHeader();
            //OpenMobile.Threading.SafeThread.Asynchronous(() => writeHeader(), theHost);
            OpenMobile.Threading.SafeThread.Asynchronous(() => writeLateHeader(), theHost);
            return eLoadStatus.LoadSuccessful;
        }
        private void writeHeader()
        {
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            theHost.OnKeyPress += new KeyboardEvent(theHost_OnKeyPress);
            WriteToLog(false, "------------------Software-------------------");
            WriteToLog(false, "\tOS: " + OpenMobile.Framework.OSSpecific.getOSVersion());
            WriteToLog(false, "\tFramework: " + OpenMobile.Framework.OSSpecific.getFramework());
            WriteToLog(false, "\tOpenMobile: v" + Assembly.GetEntryAssembly().GetName().Version.ToString());
            WriteToLog(false, "------------------Hardware-------------------");
            WriteToLog(false, "\tProcessors: " + Environment.ProcessorCount);
            WriteToLog(false, "\tArchitecture: " + OpenMobile.Framework.OSSpecific.getArchitecture().ToString());
            WriteToLog(false, "\tScreens: " + DisplayDevice.AvailableDisplays.Count.ToString());
            WriteToLog(false, "\tEmbedded: " + Configuration.RunningOnEmbedded);

            // List audio devices
            string[] AudioDevices = new string[0];
            object o;
            theHost.getData(eGetData.GetAudioDevices, "", out o);
            if (o != null)
                AudioDevices = (string[])o;
            foreach (string s in AudioDevices)
                WriteToLog(false, "\tAudioDevice: " + s);

            WriteToLog(false, "----------------Inital Assemblies-------------");
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                WriteToLog(false, "\tLOADED (" + a.GetName() + ")");
            WriteToLog(false, "---------------------------------------------");
        }
        private void writeLateHeader()
        {
            while (Graphics.Version == null)
                Thread.Sleep(10);
            WriteToLog("------------------Graphics Initialized-------------------");
            WriteToLog(false, "\t" + Graphics.GLType + " v" + Graphics.Version);
            WriteToLog(false, "\tGraphics Engine: " + Graphics.GraphicsEngine);
            WriteToLog(false, "\tGraphics Card: " + Graphics.Renderer);
            WriteToLog(false, "---------------------------------------------");

        }

        bool theHost_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg)
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
            WriteToLog(new DebugMessage("-------------Exception: " + ex.GetType().Name + "-----------------", DebugMessageType.Error));
            WriteToLog(false,"",new DebugMessage("Exception Message: " + ex.Message, DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("Fatal: " + e.IsTerminating.ToString(), DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("Source: " + ex.Source, DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("Stack Trace: " + ex.StackTrace, DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("Relevant Data: " + getData(ex.Data), DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("Exception Message: " + ex.Message, DebugMessageType.Error));
            WriteToLog(false, "", new DebugMessage("----------------------------------------------------------------", DebugMessageType.Error));
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

        void theHost_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if ((function == eFunction.userInputReady)
                || (function == eFunction.gesture)
                || (function == eFunction.multiTouchGesture))
                return; //Protect Users Privacy - Potentially contains password info
            WriteToLog("(Event:OnSystemEvent) => \t" + function.ToString() + ", " + arg1 + ", " + arg2 + ", " + arg3);

            if (function == eFunction.inputRouterInitialized)
            {
                WriteToLog(false, "", new DebugMessage("------------------MultiZone devices-------------------", DebugMessageType.Info));
                // List audio devices
                string[] Devices = new string[0];
                object o;
                theHost.getData(eGetData.GetAudioDevices, "", out o);
                if (o != null)
                    Devices = (string[])o;
                foreach (string s in Devices)
                    WriteToLog(false, "", new DebugMessage("\tAudioDevice: " + s, DebugMessageType.Info));

                // List keyboard devices
                o = new object();
                theHost.getData(eGetData.GetAvailableKeyboards, "", out o);
                if (o != null)
                    Devices = (string[])o;
                foreach (string s in Devices)
                    WriteToLog(false, "", new DebugMessage("\tKeyboard unit: " + s, DebugMessageType.Info));

                // List mice devices
                o = new object();
                theHost.getData(eGetData.GetAvailableMice, "", out o);
                if (o != null)
                    Devices = (string[])o;
                foreach (string s in Devices)
                    WriteToLog(false, "", new DebugMessage("\tMice unit: " + s, DebugMessageType.Info));

                WriteToLog(false, "", new DebugMessage("---------------------------------------------", DebugMessageType.Info));
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

        void theHost_OnMediaEvent(OpenMobile.eFunction function, int instance, string arg)
        {
            WriteToLog("(Event:OnMediaEvent) => \t" + function.ToString() + ", " + instance.ToString() + ", " + arg);
        }
        List<string> queue = new List<string>();


        private void WriteToLog(DebugMessage[] Msg)
        {
            foreach (DebugMessage m in Msg)
                WriteToLog(false, "\t", m);
        }
        private void WriteToLog(DebugMessage Msg)
        {
            WriteToLog(true, "", Msg);
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
        private void WriteToLog(bool TimeStamp, string text, DebugMessage Msg)
        {
            // Filter messages
            if (Msg.Type < OutputFilter)
                return;

            if (writer == null)
            {
                if (TimeStamp)
                    queue.Add(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + text + Msg.ToString());
                else
                    queue.Add(text + Msg.ToString());
            }
            else if (writer.BaseStream != null)
            {
                // Write out any queued messages
                if (queue.Count > 0)
                {
                    foreach (string queued in queue)
                    {
                        if (TimeStamp)
                            writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + text + Msg.ToString());
                        else
                            writer.WriteLine(text + Msg.ToString());
                    }
                    queue.Clear();
                }

                if (TimeStamp)
                    writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + " [" + Msg.Type.ToString() + "]: " + text + Msg.ToString());
                else
                    writer.WriteLine(text + Msg.ToString());
                writer.Flush();
            }
        }

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
            if (writer!=null)
                writer.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
