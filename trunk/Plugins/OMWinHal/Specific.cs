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
using System.Runtime.InteropServices;
using System.Text;
using OpenMobile;
using VistaAudio.CoreAudioApi;
using WaveLib.AudioMixer;
using OpenMobile.Platform.Windows;
using System.Collections.Generic;

namespace OMHal
{
    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class Specific
    {
        static MMDevice[] device;
        static WaveLib.AudioMixer.Mixers m;
        static int[] lastVolume;
        private static OperatingSystem os = System.Environment.OSVersion;
        const int SC_MONITORPOWER = 0xF170;
        const int WM_SYSCOMMAND = 0x0112;
        private static bool monitorOn = true;
        /// <summary>
        /// Gets the system volume
        /// </summary>
        /// <returns>Int (Range 0-100)</returns>
        public static int getVolume(int instance)
        {
            if (os.Version.Major < 6)
            {
                if ((m==null)||(instance >= m.Playback.Devices.Count))
                    return -1;
                lock (m)
                {
                    try
                    {
                        m.Playback.DeviceId = m.Playback.Devices[instance].DeviceId;
                    }
                    catch (MixerException)
                    {
                        return 0;
                    }
                    MixerLine l = m.Playback.Lines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                    if ((l == null) || (l.ContainsVolume == false))
                        return -1;
                    lastVolume[instance] = l.Volume;
                    if ((l.ContainsMute) && (l.Mute == true))
                    {
                        lastVolume[instance] = -1;
                        return lastVolume[instance];
                    }
                    return (int)Math.Round(((double)l.Volume / l.VolumeMax) * 100.0);
                }
            }
            else
            {
                if (device != null)
                {
                    if (instance < device.Length)
                    {
                        if (device[instance].AudioEndpointVolume.Mute == true)
                            return -1;
                        else
                            return (int)Math.Round(device[instance].AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    }
                }
                return -1;
            }
        }
        /// <summary>
        /// Sets the system volume
        /// </summary>
        /// <param name="volume">Integer (Range: 0-100)</param>
        public static void setVolume(int volume,int instance)
        {
            if (os.Version.Major < 6)
            {
                if ((m==null)||(instance >= m.Playback.Devices.Count))
                    return;
                lock (m)
                {
                    try
                    {
                        m.Playback.DeviceId = m.Playback.Devices[instance].DeviceId;
                    }
                    catch (MixerException)
                    {
                        return;
                    }
                    MixerLine l = m.Playback.Lines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                    if ((l == null) || (l.ContainsVolume == false))
                        return;
                    if (volume == -1)
                        if (l.ContainsMute == true)
                            l.Mute = true;
                        else
                            l.Volume = l.VolumeMin;
                    else
                    {
                        if (l.ContainsMute == true)
                            l.Mute = false;
                        if (volume >= 0)
                            l.Volume = (int)((volume / 100.0) * l.VolumeMax);
                    }
                }
            }
            else
            {
                if (device != null)
                {
                    if (instance >= device.Length)
                        return;
                    if (volume == -1)
                        device[instance].AudioEndpointVolume.Mute = true;
                    else if(volume==-2)
                        device[instance].AudioEndpointVolume.Mute=false;
                    else
                    {
                        device[instance].AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);
                        device[instance].AudioEndpointVolume.Mute = false;
                    }
                }
            }
        }
        public static void setSubVolume(int instance, int level)
        {
            if (os.Version.Major < 6) //Xp
            {
                if (instance > m.Playback.Devices.Count)
                    return;
                if (instance == 0)
                    m.Playback.DeviceId = m.Playback.DeviceIdDefault;
                else
                    m.Playback.DeviceId = m.Playback.Devices[instance - 1].DeviceId;
                MixerLine l = m.Playback.Lines.GetMixerFirstLineByComponentType(WaveLib.AudioMixer.MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                if (l.Channels == 3)
                    l.Channel = Channel.Channel_3;
                else if (l.Channels > 5)
                    l.Channel = Channel.Channel_6;
                else
                    return;
                l.Volume = (int)((level / 100.0) * l.VolumeMax);
            }
            else
            {
                if (device != null)
                {
                    if (instance >= device.Length)
                        return;
                    device[instance].AudioEndpointVolume.setSub(level);
                }
            }
        }
        public static void setBalance(int instance,int balance)//Left=0
        {
            if (os.Version.Major < 6) //Xp
            {
                if (instance > m.Playback.Devices.Count)
                    return;
                if (instance == 0)
                    m.Playback.DeviceId = m.Playback.DeviceIdDefault;
                else
                    m.Playback.DeviceId = m.Playback.Devices[instance - 1].DeviceId;
                double volume;
                MixerLine l = m.Playback.Lines.GetMixerFirstLineByComponentType(WaveLib.AudioMixer.MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                l.Channel = Channel.Left;
                volume = ((double)l.Volume / l.VolumeMax);
                l.Channel = Channel.Right;
                volume += ((double)l.Volume / l.VolumeMax);
                l.Volume = (int)(((100-balance) / 100.0) * volume * l.VolumeMax);
                l.Channel = Channel.Left;
                l.Volume = (int)((balance / 100.0) * volume * l.VolumeMax);
                if (l.Channels >= 4)
                {
                    l.Channel = Channel.Channel_3;
                    volume = ((double)l.Volume / l.VolumeMax);
                    l.Channel = Channel.Channel_4;
                    volume += ((double)l.Volume / l.VolumeMax);
                    l.Volume = (int)(((100 - balance) / 100.0) * volume * l.VolumeMax);
                    l.Channel = Channel.Channel_3;
                    l.Volume = (int)((balance / 100.0) * volume * l.VolumeMax);
                }
            }
            else
            {
                if (device != null)
                {
                    if (instance >= device.Length)
                        return;
                    device[instance].AudioEndpointVolume.setBalance(balance);
                }
            }
        }
        public static bool hookVolume(IntPtr handle)
        {
            if (os.Version.Major < 6) //Xp
            {
                m = new WaveLib.AudioMixer.Mixers();
                lastVolume = new int[m.Playback.Devices.Count];
                m.Playback.MixerLineChanged += new Mixer.MixerLineChangeHandler(Playback_MixerLineChanged);
                for (int i = 0; i < m.Playback.Devices.Count; i++)
                {
                    m.Playback.DeviceId = m.Playback.Devices[i].DeviceId;
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(i).ToString(), i.ToString(), "");
                }
                return true;
            }
            else
            {
                try
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDeviceCollection col= DevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
                    if (col.Count == 0)
                        return false;
                    device = new MMDevice[col.Count + 1];
                    device[0] = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                    //uint count;
                    //device[0].Bass.GetChannelCount(out count);
                    for (int i = 0; i < col.Count; i++)
                    {
                        device[i + 1] = col[col.Count-i-1];
                        col[i].AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
                    }
                    for(int i=0;i<device.Length;i++)
                        Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(i).ToString(), i.ToString(), "");
                }
                catch (Exception) { }
                initMonitors();
                return (device!=null);
            }
        }

        static void Playback_MixerLineChanged(Mixer mixer, MixerLine line)
        {
            if (lastVolume[mixer.DeviceId] == line.Volume)
                return;
            if (line.Mute && (lastVolume[mixer.DeviceId] == -1))
                return;
            Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(mixer.DeviceId).ToString(), (mixer.DeviceId).ToString(), "");
            if (mixer.DeviceIdDefault==mixer.DeviceId)
                Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(mixer.DeviceId).ToString(), "0", "");
        }
        static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            for (int i = 0; i < device.Length; i++)
            {
                if ((data.MasterVolume != device[i].AudioEndpointVolume.MasterVolumeLevelScalar)||(device[i].AudioEndpointVolume.Mute!=data.Muted))
                    continue;
                if (data.Muted == false)
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, ((int)Math.Round(data.MasterVolume * 100)).ToString(), i.ToString(), "");
                else
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1", i.ToString(), "");
            }
        }

        internal static void eject()
        {
            StringBuilder buf = new StringBuilder();
            mciSendString("SET CDAudio door open", buf, 127, IntPtr.Zero);
        }
        internal static void SetBrightness(int instance, int value)
        {
            if ((!monitorOn) && (value != 0))
            {
                monitorOn = true;
                SwitchOnMonitor(instance);
            }
            if (value == 0)
            {
                monitorOn = false;
                SwitchOffMonitor(instance);
                return;
            }
            if (value == 1) //round down
                value = 0;
            Set(instance,value);
        }
        public delegate IntPtr getHandle();
        public static event getHandle OnHandleRequested;
        private static void SwitchOffMonitor(int screen)
        {
            if (screen == 0)
                SendMessage(OnHandleRequested().ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
            else
            {
                OpenMobile.Platform.Windows.DeviceMode dev = new OpenMobile.Platform.Windows.DeviceMode();
                dev.DeviceName=screen.ToString();
                dev.PelsHeight=0;
                dev.PelsWidth=0;
                dev.Fields=OpenMobile.Platform.Windows.Constants.DM_POSITION;
                OpenMobile.Platform.Windows.Functions.ChangeDisplaySettingsEx(@"\\.\DISPLAY" + (screen + 1).ToString(), dev, IntPtr.Zero,0x4, null);
            }
        }
        private static void SwitchOnMonitor(int instance)
        {
            if (instance == 0)
                SendMessage(OnHandleRequested().ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, -1);
            //else
                //TODO
        }
        private static void Set(int screen, int targetBrightness)
        {
            if (os.Version.Major < 6)
            {
                if (screen > 0)
                {
                    OpenMobile.Platform.Windows._VIDEOPARAMETERS par = new OpenMobile.Platform.Windows._VIDEOPARAMETERS();
                    par.guid = new Guid("02C62061-1097-11d1-920F-00A024DF156E");
                    par.dwCommand = 0x2;
                    par.dwFlags = 0x0040;
                    par.dwBrightness = (uint)targetBrightness;
                    IntPtr vp = Marshal.AllocHGlobal(Marshal.SizeOf(par));
                    try
                    {
                        Marshal.StructureToPtr(par, vp, false);
                        OpenMobile.Platform.Windows.Functions.ChangeDisplaySettingsEx(@"\\.\DISPLAY" + (screen + 1).ToString(), null, IntPtr.Zero, 0x20, vp);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(vp);
                    }
                }
                else
                {
                    planB(screen, targetBrightness);
                }
            }
            else
            {
                uint min, current, max;
                if (screen >= monitors.Count) screen = 0;
                if ((monitors.Count == 0) || !GetMonitorBrightness(monitors[screen], out min, out current, out max))
                {
                    planB(screen, targetBrightness);
                    return;
                }
                SetMonitorBrightness(monitors[screen], (uint)((targetBrightness / 100.0) * (max - min)) + min);
            }
        }

        private static void planB(int screen, int brightness)
        {
            IntPtr dev = CreateFile(@"\\.\LCD", 0x80000000, 0x7, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
            try
            {
                BRIGHTNESS br = new BRIGHTNESS();
                br.ucACBrightness = (byte)brightness;
                br.ucDCBrightness = (byte)brightness;
                IntPtr outbuf = new IntPtr();
                int bytes;
                DeviceIoControl(dev, 0x23049C, ref br, Marshal.SizeOf(br), outbuf, 0, out bytes, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(dev);
            }
        }
        public struct BRIGHTNESS
        {
            public byte ucDisplayPolicy;
            public byte ucACBrightness;
            public byte ucDCBrightness;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
                IntPtr hDevice,
                uint dwIoControlCode,
                ref BRIGHTNESS InBuffer,
                int nInBufferSize,
                IntPtr OutBuffer,
                int nOutBufferSize,
                out int pBytesReturned,
                IntPtr lpOverlapped);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr CreateFile(
              string lpFileName,
              uint dwDesiredAccess,
              uint dwShareMode,
              IntPtr SecurityAttributes,
              uint dwCreationDisposition,
              uint dwFlagsAndAttributes,
              IntPtr hTemplateFile
              );
        static List<IntPtr> monitors = new List<IntPtr>();
        delegate bool monitorAdded(IntPtr hmon, IntPtr arg2, IntPtr arg3, IntPtr arg4);
        private static void initMonitors()
        {
            EnumDisplayMonitors(IntPtr.Zero,IntPtr.Zero,Added,IntPtr.Zero);
        }

        private static bool Added(IntPtr hmon, IntPtr arg2, IntPtr arg3, IntPtr arg4)
        {
            uint size;
            if ((!GetNumberOfPhysicalMonitorsFromHMONITOR(hmon, out size))||(size==0))
                return false;
            Monitor[] mons = new Monitor[size];
            if (!GetPhysicalMonitorsFromHMONITOR(hmon, size,mons))
                return false;
           // Marshal.PtrToStructure(monPtr, mons);
            foreach (Monitor monitor in mons)
            {
                monitors.Add(monitor.hPhysicalMonitor);
            }
            bool success=DestroyPhysicalMonitors(size, mons);
            return true;
        }
        private static void getPrivledge()
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, 0x28, ref htok);
            if (!ok)
                return;
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = 0x2;
            LookupPrivilegeValue(null, "SeShutdownPrivilege", ref tp.Luid);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }
        public static void Shutdown(bool restart)
        {
            getPrivledge();
            if (restart)
                ExitWindowsEx(0x12, 0x80000000);
            else
                ExitWindowsEx(0x18, 0x80000000);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Monitor
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
        phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name,
        ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(uint flg, uint rea);
        [DllImport("dxva2.dll")]
        static extern bool SetMonitorBrightness(IntPtr hMonitor,uint dwNewBrightness);
        [DllImport("dxva2.dll",SetLastError=true)]
        static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);
        [DllImport("dxva2.dll")]
        static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor,out uint pdwNumberOfPhysicalMonitors);
        [DllImport("dxva2.dll")]
        static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor,uint dwPhysicalMonitorArraySize,[Out] Monitor[] pPhysicalMonitorArray);
        [DllImport("dxva2.dll")]
        static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize,Monitor[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
        [DllImport("Powrprof.dll", SetLastError = true)]
        static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
        [DllImport("powrprof.dll")]
        static extern bool IsPwrHibernateAllowed();
        [DllImport("powrprof.dll")]
        static extern bool IsPwrSuspendAllowed();
        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc,IntPtr lprcClip, monitorAdded lpfnEnum,IntPtr dwData);

        internal static void Hibernate()
        {
            getPrivledge();
            if (IsPwrHibernateAllowed())
                SetSuspendState(true, false, false);
        }

        internal static void Suspend()
        {
            getPrivledge();
            if ((IsPwrSuspendAllowed()) && (SetSuspendState(false, false, false)))
                return;
            else if (IsPwrHibernateAllowed())
                SetSuspendState(true, false, false);
        }
    }
}
