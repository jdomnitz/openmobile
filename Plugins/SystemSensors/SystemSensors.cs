using System;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using OpenMobile;
using System.Runtime.InteropServices;

namespace SystemSensors
{
    public class SystemSensors:IRawHardware
    {
        List<Sensor> sensors = new List<Sensor>();
        PerformanceCounter cpuCounter;
        PerformanceCounter freeRamCounter;
        Process currentProcess;
        public override System.Collections.Generic.List<Sensor> getAvailableSensors(eSensorType type)
        {
            return sensors;
        }

        public override bool setValue(int PID, object value)
        {
            return false;
        }
        public override object getValue(int PID)
        {
            int localPID = UnmaskPID(PID);
            switch (localPID)
            {
                case 0:
                    if (cpuCounter == null)
                    {
                        cpuCounter = new PerformanceCounter();
                        cpuCounter.CategoryName = "Processor";
                        cpuCounter.CounterName = "% Processor Time";
                        cpuCounter.InstanceName = "_Total";
                        cpuCounter.NextValue(); //init
                    }
                    return cpuCounter.NextValue();
                case 1:
                    if (freeRamCounter==null)
                        freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
                    return freeRamCounter.NextValue();
                case 2:
                    if (Configuration.RunningOnWindows)
                        return (int)(getUsedPhysicalMemory()/1048576);
                    return null;
                case 3:
                    if (Configuration.RunningOnWindows)
                        return (float)Math.Round(getUsedMemoryPercent(),2);
                    return null;
                case 4:
                    if (currentProcess == null)
                        currentProcess = Process.GetCurrentProcess();
                    return currentProcess.WorkingSet64 / 1048576;
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
                return (1-(status.dwAvailPhys/(float)status.dwTotalVirtual))*100;
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

        public override void resetDevice()
        {
            //
        }

        public override string deviceInfo
        {
            get { return "System Sensors"; }
        }

        public override string firmwareVersion
        {
            get { return OSSpecific.getOSVersion(); }
        }

        public override OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            InitPIDMask();
            Sensor CPU = new Sensor("System.CPUUsage", MaskPID(0), eSensorType.deviceSuppliesData);
            Sensor FreeMemory = new Sensor("System.PhysicalMemoryFree", MaskPID(1), eSensorType.deviceSuppliesData);
            Sensor UsedMemory = new Sensor("System.PhysicalMemoryUsed", MaskPID(2), eSensorType.deviceSuppliesData);
            Sensor UsedMemoryPercent = new Sensor("System.MemoryUsedPercent", MaskPID(3), eSensorType.deviceSuppliesData);
            Sensor ProcessMemory = new Sensor("System.ProcessMemoryUsed", MaskPID(4), eSensorType.deviceSuppliesData);
            sensors.Add(CPU);
            sensors.Add(FreeMemory);
            sensors.Add(UsedMemory);
            sensors.Add(UsedMemoryPercent);
            sensors.Add(ProcessMemory);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        public override Settings loadSettings()
        {
            return null;
        }

        public override string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public override string authorEmail
        {
            get { return ""; }
        }

        public override string pluginName
        {
            get { return "SystemSensors"; }
        }

        public override float pluginVersion
        {
            get { return 0.1F; }
        }

        public override string pluginDescription
        {
            get { return "System Sensors"; }
        }

        public override bool incomingMessage(string message, string source)
        {
            return false;
        }

        public override bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public override void Dispose()
        {
            //
        }
    }
}
