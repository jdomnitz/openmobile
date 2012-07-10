using System;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using OpenMobile;
using System.Runtime.InteropServices;
using OpenMobile.helperFunctions;
using System.IO;

namespace SystemSensors
{
    [PluginLevel(PluginLevels.System)]
    public class SystemSensors : ISensorProvider 
    {
        List<SensorWrapper> sensors = new List<SensorWrapper>();

        PerformanceCounter cpuCounter;
        PerformanceCounter freeRamCounter;
        Process currentProcess;
        IPluginHost thehost;
        System.Timers.Timer slowTimer;
        System.Timers.Timer fastTimer;

        public System.Collections.Generic.List<Sensor> getAvailableSensors(eSensorType type)
        {
            return Sensors.sensorWrappersToSensors(sensors);
        }

        private object getValue(Sensor sensor)
        {
            switch (sensor.Name)
            {
                case "SystemSensors.CPUUsage":
                    if (cpuCounter == null)
                    {
                        cpuCounter = new PerformanceCounter();
                        cpuCounter.CategoryName = "Processor";
                        cpuCounter.CounterName = "% Processor Time";
                        cpuCounter.InstanceName = "_Total";
                        cpuCounter.NextValue(); //init
                    }
                    return (float)Math.Round(cpuCounter.NextValue(), 0);

                case "SystemSensors.PhysicalMemoryFree":
                    if (freeRamCounter==null)
                        freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
                    return freeRamCounter.NextValue() * 1048576;
                case "SystemSensors.PhysicalMemoryUsed":
                    if (Configuration.RunningOnWindows)
                        return (int)(getUsedPhysicalMemory());
                    return null;
                case "SystemSensors.MemoryUsedPercent":
                    if (Configuration.RunningOnWindows)
                        return (float)Math.Round(getUsedMemoryPercent(),2);
                    return null;
                case "SystemSensors.ProcessMemoryUsed":
                    if (currentProcess == null)
                        currentProcess = Process.GetCurrentProcess();
                    return currentProcess.WorkingSet64 ;
                case "SystemSensors.Date":
                    return DateTime.Now.ToShortDateString();
                case "SystemSensors.DateTime":
                    return DateTime.Now.ToLocalTime();
                case "SystemSensors.Time":
                    return DateTime.Now.ToShortTimeString();
                case "SystemSensors.LongTime":
                    return DateTime.Now.ToLongTimeString();
                case "SystemSensors.LongDate":
                    return DateTime.Now.ToLongDateString(); // .ToString("MMMM d");
                case "SystemSensors.DateText":
                    return DateTime.Now.ToString("D");
            }
            return null;
        }

        private uint getUsedPhysicalMemory()
        {
            MEMORYSTATUS status=new MEMORYSTATUS();
            if (GlobalMemoryStatus(ref status))
            {
                return status.dwTotalPhys - status.dwAvailPhys;
            }
            return 0;
        }
        private float getUsedMemoryPercent()
        {
            MEMORYSTATUS status = new MEMORYSTATUS();
            if (GlobalMemoryStatus(ref status))
            {
                //return (1-(status.dwAvailPhys/(float)status.dwTotalVirtual))*100;
                return status.dwMemoryLoad;
            }
            return 0;
        }

        #region win32
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUS
        {
            /// <summary>
            /// Size of the MEMORYSTATUS data structure, in bytes. You do not need to set this member before calling the GlobalMemoryStatus function; the function sets it. 
            /// </summary>
            public uint dwLength;

            /// <summary>
            /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use). 
            /// Windows NT:  Percentage of approximately the last 1000 pages of physical memory that is in use.
            /// </summary>
            public uint dwMemoryLoad;

            /// <summary>
            /// Total size of physical memory, in bytes. 
            /// </summary>
            public uint dwTotalPhys;

            /// <summary>
            /// Size of physical memory available, in bytes
            /// </summary>
            public uint dwAvailPhys;

            /// <summary>
            /// Size of the committed memory limit, in bytes. 
            /// </summary>
            public uint dwTotalPageFile;

            /// <summary>
            /// Size of available memory to commit, in bytes. 
            /// </summary>
            public uint dwAvailPageFile;

            /// <summary>
            /// Total size of the user mode portion of the virtual address space of the calling process, in bytes. 
            /// </summary>
            public uint dwTotalVirtual;

            /// <summary>
            /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes. 
            /// </summary>
            public uint dwAvailVirtual;

        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);
        #endregion

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            thehost = host;

            // Slow update sensors
            sensors.Add(new SensorWrapper(this.pluginName + ".CPUUsage", "CPU", eSensorDataType.percent, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".PhysicalMemoryFree", "FMem", eSensorDataType.bytes, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".PhysicalMemoryUsed", "UMem", eSensorDataType.bytes, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".MemoryUsedPercent", "Mem", eSensorDataType.percent, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".ProcessMemoryUsed", "UMem", eSensorDataType.bytes, delegate(Sensor sensor) { return getValue(sensor); }));

            // Fast update sensors
            sensors.Add(new SensorWrapper(this.pluginName + ".Date", "Dt", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".LongDate", "Dt", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".Time", "Tm", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".LongTime", "Tm", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".DateTime", "DtTm", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));
            sensors.Add(new SensorWrapper(this.pluginName + ".DateText", "DtTm", eSensorDataType.raw, delegate(Sensor sensor) { return getValue(sensor); }));

            // Update timers: Slow
            slowTimer = new System.Timers.Timer(5000); //These items take a while to refresh to do them less often
            slowTimer.Elapsed += new System.Timers.ElapsedEventHandler(slowTimer_Elapsed);
            slowTimer.Enabled = true;

            // Update timers: Fast
            fastTimer = new System.Timers.Timer(1000);
            fastTimer.Elapsed += new System.Timers.ElapsedEventHandler(fastTimer_Elapsed);
            fastTimer.Enabled = true;

            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void slowTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            slowTimer.Enabled = false;
            for (int i=0; i <= 4; i++)
                sensors[i].UpdateSensorValue( getValue(sensors[i].sensor));
            slowTimer.Enabled = true;
        }

        void fastTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            fastTimer.Enabled = false;
            for (int i = 5; i <= 10; i++)
                sensors[i].UpdateSensorValue(getValue(sensors[i].sensor));
            fastTimer.Enabled = true;
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz, Jon Heizer"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "SystemSensors"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "System Sensors"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public void Dispose()
        {
            //
        }

    }
}
