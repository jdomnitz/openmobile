using System;
using System.IO;
using System.Reflection;
//using System.Windows.Forms;
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Threading;

namespace OMDebug
{
    public sealed class Debugger:IOther
    {
        public Settings loadSettings()
        {
            return null;
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
            log("********"+source + "******\r\n" + message);
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
            throw new NotImplementedException();
        }
        IPluginHost theHost;
        StreamWriter writer;
        int time;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            writer = new StreamWriter(OpenMobile.Path.Combine(theHost.DataPath, "Debug.txt"), true);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            new Thread(() => writeHeader()).Start();
            return eLoadStatus.LoadSuccessful;
        }
        private void writeHeader()
        {
            while (Graphics.Version == null)
                Thread.Sleep(10);
            writer.WriteLine("------------------Software-------------------");
            writer.WriteLine("OS: " + OpenMobile.Framework.OSSpecific.getOSVersion());
            writer.WriteLine("Framework: " + OpenMobile.Framework.OSSpecific.getFramework());
            writer.WriteLine("Open Mobile: v" + Process.GetCurrentProcess().Modules[0].FileVersionInfo.FileVersion);
            writer.WriteLine("Open GL v." + Graphics.Version);
            writer.WriteLine("------------------Hardware-------------------");
            writer.WriteLine("Processors: " + Environment.ProcessorCount);
            writer.WriteLine("Architecture: x" + ((IntPtr.Size == 4) ? "86" : "64"));
            writer.WriteLine("Screens: " + DisplayDevice.AvailableDisplays.Count.ToString());
            writer.WriteLine("Graphics Card: " + Graphics.GraphicsEngine);
            writer.WriteLine("----------------Inital Assemblies-------------");
            time = Environment.TickCount;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                log("LOADED (" + a.GetName() + ")");
            writer.WriteLine("---------------------------------------------");
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            writer.WriteLine("-------------Exception: " + ex.GetType().Name + "-----------------");
            log("Exception Message: " + ex.Message);
            log("Fatal: "+e.IsTerminating.ToString());
            log("Source: " + ex.Source);
            log("Stack Trace: " + ex.StackTrace);
            log("Relevant Data: " + getData(ex.Data));
            writer.WriteLine("----------------------------------------------------------------");
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
            log("LOADED (" + args.LoadedAssembly.GetName() + ")@" + args.LoadedAssembly.CodeBase);
        }

        void theHost_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if ((function == eFunction.userInputReady) || (function == eFunction.gesture))
                return; //Protect Users Privacy - Potentially contain password info
            log(function.ToString() + "(" + arg1 + "," + arg2 + "," + arg3 + ")");
        }

        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            log("Storage Event("+type.ToString()+","+justInserted+","+arg+")");
        }

        void theHost_OnPowerChange(OpenMobile.ePowerEvent type)
        {
            log("Power Event(" + type.ToString() + ")");
        }

        void theHost_OnMediaEvent(OpenMobile.eFunction function, int instance, string arg)
        {
            log(function.ToString() + "(" + instance.ToString() + "," + arg + ")");
        }
        private void log(string text)
        {
            if (writer.BaseStream != null)
            {
                writer.WriteLine(((Environment.TickCount - time) / 1000.0).ToString("0.000") + ": " + text);
                writer.Flush();
            }
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
