using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Framework;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace OpenMobile.Framework.OS.Windows
{
    public class OSInteraction : OSInteractionBase
    {
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(uint flg, uint rea);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("Powrprof.dll", SetLastError = true)]
        static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
        [DllImport("powrprof.dll")]
        static extern bool IsPwrHibernateAllowed();
        [DllImport("powrprof.dll")]
        static extern bool IsPwrSuspendAllowed();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        public OSInteraction(Action<CallbackEvents> eventCallback)
            : base(eventCallback)
        {
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);

            // Late init in a separate thread so we don't block the main thread
            Task.Factory.StartNew(() =>
                {
                    // Check for already running on battery
                    if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                        base.RaiseEvent(CallbackEvents.Power_Battery_RunningOnBattery);
                });
        }

        void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    base.RaiseEvent(CallbackEvents.System_LogOffPending);
                    e.Cancel = true;    // Cancel request to allow OM enough time to exit gracefully
                    break;
                case SessionEndReasons.SystemShutdown:
                    base.RaiseEvent(CallbackEvents.System_ShutdownPending);
                    e.Cancel = true;    // Cancel request to allow OM enough time to exit gracefully
                    break;
                default:
                    break;
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    base.RaiseEvent(CallbackEvents.System_SystemResumed);
                    break;
                case PowerModes.StatusChange:
                    {
                        if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                        {
                            if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Low)
                                base.RaiseEvent(CallbackEvents.Power_Battery_Low);
                            else if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Critical)
                                base.RaiseEvent(CallbackEvents.Power_Battery_Critical);
                            else
                                base.RaiseEvent(CallbackEvents.Power_Battery_RunningOnBattery);
                        }
                        else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                            base.RaiseEvent(CallbackEvents.Power_Battery_RunningOnLine);
                    }
                    break;
                case PowerModes.Suspend:
                    base.RaiseEvent(CallbackEvents.System_SleepOrHibernatePending);
                    break;
                default:
                    break;
            }
        }

        public override bool Shutdown()
        {
            System_Shutdown(false);
            return true;
        }

        public override bool Restart()
        {
            System_Shutdown(true);
            return true;
        }

        public override bool Hibernate()
        {
            OS_GetPrivledge();
            if (IsPwrHibernateAllowed())
                SetSuspendState(true, false, false);
            return true;
        }

        public override bool Suspend()
        {
            OS_GetPrivledge();
            if ((IsPwrSuspendAllowed()) && (SetSuspendState(false, false, false)))
                return false;
            else if (IsPwrHibernateAllowed())
                SetSuspendState(true, false, false);
            return true;
        }       

        #region OS Call: Shutdown / Restart

        private static void OS_GetPrivledge()
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

        private static void System_Shutdown(bool restart)
        {
            OS_GetPrivledge();
            if (restart)
                ExitWindowsEx(0x12, 0x80000000);
            else
                ExitWindowsEx(0x18, 0x80000000);
        }

        #endregion
    }
}
