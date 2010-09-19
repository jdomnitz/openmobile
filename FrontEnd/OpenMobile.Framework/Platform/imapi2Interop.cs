// Imapi2Interop.cs
//
// by Eric Haddan
//
// Parts taken from Microsoft's Interop.cs
//
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace IMAPI2.Interop
{
    #region IMAPI2 Enums

    public enum EmulationType
    {
        EmulationNone,
        Emulation12MFloppy,
        Emulation144MFloppy,
        Emulation288MFloppy,
        EmulationHardDisk
    }

    public enum IMAPI_PROFILE_TYPE
    {
        IMAPI_PROFILE_TYPE_INVALID = 0,
        IMAPI_PROFILE_TYPE_NON_REMOVABLE_DISK = 1,
        IMAPI_PROFILE_TYPE_REMOVABLE_DISK = 2,
        IMAPI_PROFILE_TYPE_MO_ERASABLE = 3,
        IMAPI_PROFILE_TYPE_MO_WRITE_ONCE = 4,
        IMAPI_PROFILE_TYPE_AS_MO = 5,
        IMAPI_PROFILE_TYPE_CDROM = 8,
        IMAPI_PROFILE_TYPE_CD_RECORDABLE = 9,
        IMAPI_PROFILE_TYPE_CD_REWRITABLE = 10,
        IMAPI_PROFILE_TYPE_DVDROM = 0x10,
        IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE = 0x11,
        IMAPI_PROFILE_TYPE_DVD_RAM = 0x12,
        IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE = 0x13,
        IMAPI_PROFILE_TYPE_DVD_DASH_RW_SEQUENTIAL = 0x14,
        IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_SEQUENTIAL = 0x15,
        IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_LAYER_JUMP = 0x16,
        IMAPI_PROFILE_TYPE_DVD_PLUS_RW = 0x1a,
        IMAPI_PROFILE_TYPE_DVD_PLUS_R = 0x1b,
        IMAPI_PROFILE_TYPE_DDCDROM = 0x20,
        IMAPI_PROFILE_TYPE_DDCD_RECORDABLE = 0x21,
        IMAPI_PROFILE_TYPE_DDCD_REWRITABLE = 0x22,
        IMAPI_PROFILE_TYPE_DVD_PLUS_RW_DUAL = 0x2a,
        IMAPI_PROFILE_TYPE_DVD_PLUS_R_DUAL = 0x2b,
        IMAPI_PROFILE_TYPE_BD_ROM = 0x40,
        IMAPI_PROFILE_TYPE_BD_R_SEQUENTIAL = 0x41,
        IMAPI_PROFILE_TYPE_BD_R_RANDOM_RECORDING = 0x42,
        IMAPI_PROFILE_TYPE_BD_REWRITABLE = 0x43,
        IMAPI_PROFILE_TYPE_HD_DVD_ROM = 0x50,
        IMAPI_PROFILE_TYPE_HD_DVD_RECORDABLE = 0x51,
        IMAPI_PROFILE_TYPE_HD_DVD_RAM = 0x52,
        IMAPI_PROFILE_TYPE_NON_STANDARD = 0xffff
    }
    #endregion

    #region DDiscMaster2Events
    /// <summary>
    /// Provides notification of the arrival/removal of CD/DVD (optical) devices.
    /// </summary>
    [ComImport]
    [Guid("27354131-7F64-5B0F-8F00-5D77AFBE261E")]
    [TypeLibType(TypeLibTypeFlags.FNonExtensible|TypeLibTypeFlags.FOleAutomation|TypeLibTypeFlags.FDispatchable)]
    public interface DDiscMaster2Events
    {
        // A device was added to the system
        [DispId(0x100)]     // DISPID_DDISCMASTER2EVENTS_DEVICEADDED
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void NotifyDeviceAdded([In, MarshalAs(UnmanagedType.IDispatch)] object sender,  string uniqueId);

        // A device was removed from the system
        [DispId(0x101)]     // DISPID_DDISCMASTER2EVENTS_DEVICEREMOVED
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void NotifyDeviceRemoved([In, MarshalAs(UnmanagedType.IDispatch)] object sender,  string uniqueId);
    }


    [ComVisible(false)]
    [TypeLibType(TypeLibTypeFlags.FHidden)]
    [ComEventInterface(typeof(DDiscMaster2Events), typeof(DiscMaster2_EventProvider))]
    public interface DiscMaster2_Event
    {
        // Events
        event DiscMaster2_NotifyDeviceAddedEventHandler NotifyDeviceAdded;
        event DiscMaster2_NotifyDeviceRemovedEventHandler NotifyDeviceRemoved;
    }

    [ClassInterface(ClassInterfaceType.None)]
    internal sealed class DiscMaster2_EventProvider : DiscMaster2_Event, IDisposable
    {
        // Fields
        private Hashtable m_aEventSinkHelpers = new Hashtable();
        private IConnectionPoint m_connectionPoint = null;

        // Methods
        public DiscMaster2_EventProvider(object pointContainer)
        {
            lock (this)
            {
                Guid eventsGuid = typeof(DDiscMaster2Events).GUID;
                IConnectionPointContainer connectionPointContainer = pointContainer as IConnectionPointContainer;

                connectionPointContainer.FindConnectionPoint(ref eventsGuid, out m_connectionPoint);
            }
        }

        public event DiscMaster2_NotifyDeviceAddedEventHandler NotifyDeviceAdded
        {
            add
            {
                lock (this)
                {
                    DiscMaster2_SinkHelper helper =
                        new DiscMaster2_SinkHelper(value);
                    int cookie;

                    m_connectionPoint.Advise(helper, out cookie);
                    helper.Cookie = cookie;
                    m_aEventSinkHelpers.Add(helper.NotifyDeviceAddedDelegate, helper);
                }
            }

            remove
            {
                lock (this)
                {
                    DiscMaster2_SinkHelper helper =
                        m_aEventSinkHelpers[value] as DiscMaster2_SinkHelper;
                    if (helper != null)
                    {
                        m_connectionPoint.Unadvise(helper.Cookie);
                        m_aEventSinkHelpers.Remove(helper.NotifyDeviceAddedDelegate);
                    }
                }
            }
        }

        public event DiscMaster2_NotifyDeviceRemovedEventHandler NotifyDeviceRemoved
        {
            add
            {
                lock (this)
                {
                    DiscMaster2_SinkHelper helper =
                        new DiscMaster2_SinkHelper(value);
                    int cookie;

                    m_connectionPoint.Advise(helper, out cookie);
                    helper.Cookie = cookie;
                    m_aEventSinkHelpers.Add(helper.NotifyDeviceRemovedDelegate, helper);
                }
            }

            remove
            {
                lock (this)
                {
                    DiscMaster2_SinkHelper helper =
                        m_aEventSinkHelpers[value] as DiscMaster2_SinkHelper;
                    if (helper != null)
                    {
                        m_connectionPoint.Unadvise(helper.Cookie);
                        m_aEventSinkHelpers.Remove(helper.NotifyDeviceRemovedDelegate);
                    }
                }
            }
        }

        ~DiscMaster2_EventProvider()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            Monitor.Enter(this);
            try
            {
                foreach (DiscMaster2_SinkHelper helper in m_aEventSinkHelpers.Values)
                {
                    m_connectionPoint.Unadvise(helper.Cookie);
                }

                m_aEventSinkHelpers.Clear();
                Marshal.ReleaseComObject(m_connectionPoint);
            }
            catch (SynchronizationLockException)
            {
                return;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void DiscMaster2_NotifyDeviceAddedEventHandler([In, MarshalAs(UnmanagedType.IDispatch)]object sender,  string uniqueId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void DiscMaster2_NotifyDeviceRemovedEventHandler([In, MarshalAs(UnmanagedType.IDispatch)]object sender,  string uniqueId);

    [ClassInterface(ClassInterfaceType.None)]
    [TypeLibType(TypeLibTypeFlags.FHidden)]
    public sealed class DiscMaster2_SinkHelper : DDiscMaster2Events
    {
        // Fields
        private int m_dwCookie;
        private DiscMaster2_NotifyDeviceAddedEventHandler m_AddedDelegate = null;
        private DiscMaster2_NotifyDeviceRemovedEventHandler m_RemovedDelegate = null;

        public DiscMaster2_SinkHelper(DiscMaster2_NotifyDeviceAddedEventHandler eventHandler)
        {
            if (eventHandler == null)
                throw new ArgumentNullException("Delegate (callback function) cannot be null");
            m_dwCookie = 0;
            m_AddedDelegate = eventHandler;
        }

        public DiscMaster2_SinkHelper(DiscMaster2_NotifyDeviceRemovedEventHandler eventHandler)
        {
            if (eventHandler == null)
                throw new ArgumentNullException("Delegate (callback function) cannot be null");
            m_dwCookie = 0;
            m_RemovedDelegate = eventHandler;
        }

        public void NotifyDeviceAdded(object sender, string uniqueId)
        {
            m_AddedDelegate(sender, uniqueId);
        }

        public void NotifyDeviceRemoved(object sender, string uniqueId)
        {
            m_RemovedDelegate(sender, uniqueId);
        }

        public int Cookie
        {
            get
            {
                return m_dwCookie;
            }
            set
            {
                m_dwCookie = value;
            }
        }

        public DiscMaster2_NotifyDeviceAddedEventHandler NotifyDeviceAddedDelegate
        {
            get
            {
                return m_AddedDelegate;
            }
            set
            {
                m_AddedDelegate = value;
            }
        }

        public DiscMaster2_NotifyDeviceRemovedEventHandler NotifyDeviceRemovedDelegate
        {
            get
            {
                return m_RemovedDelegate;
            }
            set
            {
                m_RemovedDelegate = value;
            }
        }
    }

    #endregion DDiscMaster2Events

    #region Interfaces

    /// <summary>
    /// IDiscMaster2 is used to get an enumerator for the set of CD/DVD (optical) devices on the system
    /// </summary>
    [ComImport]
    [Guid("27354130-7F64-5B0F-8F00-5D77AFBE261E")]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FNonExtensible)]
    public interface IDiscMaster2
    {
        // Enumerates the list of CD/DVD devices on the system (VT_BSTR)
        [DispId(-4), TypeLibFunc((short)0x41)]
        IEnumerator GetEnumerator();

        // Gets a single recorder's ID (ZERO BASED INDEX)
        [DispId(0)]
        string this[int index] { get; }

        // The current number of recorders in the system.
        [DispId(1)]
        int Count { get; }

        // Whether IMAPI is running in an environment with optical devices and permission to access them.
        [DispId(2)]
        bool IsSupportedEnvironment { get; }
    }

    /// <summary>
    ///  Represents a single CD/DVD type device, and enables many common operations via a simplified API.
    /// </summary>
    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FNonExtensible)]
    [Guid("27354133-7F64-5B0F-8F00-5D77AFBE261E")]
    public interface IDiscRecorder2
    {
        // Ejects the media (if any) and opens the tray
        [DispId(0x100)]
        void EjectMedia();

        // Close the media tray and load any media in the tray.
        [DispId(0x101)]
        void CloseTray();

        // Acquires exclusive access to device.  May be called multiple times.
        [DispId(0x102)]
        void AcquireExclusiveAccess(bool force, string clientName);

        // Releases exclusive access to device.  Call once per AcquireExclusiveAccess().
        [DispId(0x103)]
        void ReleaseExclusiveAccess();

        // Disables Media Change Notification (MCN).
        [DispId(260)]
        void DisableMcn();

        // Re-enables Media Change Notification after a call to DisableMcn()
        [DispId(0x105)]
        void EnableMcn();

        // Initialize the recorder, opening a handle to the specified recorder.
        [DispId(0x106)]
        void InitializeDiscRecorder(string recorderUniqueId);

        // The unique ID used to initialize the recorder.
        [DispId(0)]
        string ActiveDiscRecorder { get; }

        // The vendor ID in the device's INQUIRY data.
        [DispId(0x201)]
        string VendorId { get; }

        // The Product ID in the device's INQUIRY data.
        [DispId(0x202)]
        string ProductId { get; }

        // The Product Revision in the device's INQUIRY data.
        [DispId(0x203)]
        string ProductRevision { get; }

        // Get the unique volume name (this is not a drive letter).
        [DispId(0x204)]
        string VolumeName { get; }

        // Drive letters and NTFS mount points to access the recorder.
        [DispId(0x205)]
        object[] VolumePathNames { [DispId(0x205)] get; }

        // One of the volume names associated with the recorder.
        [DispId(0x206)]
        bool DeviceCanLoadMedia { [DispId(0x206)] get; }

        // Gets the legacy 'device number' associated with the recorder.  This number is not guaranteed to be static.
        [DispId(0x207)]
        int LegacyDeviceNumber { [DispId(0x207)] get; }

        // Gets a list of all feature pages supported by the device
        [DispId(520)]
        object[] SupportedFeaturePages { [DispId(520)] get; }

        // Gets a list of all feature pages with 'current' bit set to true
        [DispId(0x209)]
        object[] CurrentFeaturePages { [DispId(0x209)] get; }

        // Gets a list of all profiles supported by the device
        [DispId(0x20a)]
        object[] SupportedProfiles { [DispId(0x20a)] get; }

        // Gets a list of all profiles with 'currentP' bit set to true
        [DispId(0x20b)]
        object[] CurrentProfiles { [DispId(0x20b)] get; }

        // Gets a list of all MODE PAGES supported by the device
        [DispId(0x20c)]
        object[] SupportedModePages { [DispId(0x20c)] get; }

        // Queries the device to determine who, if anyone, has acquired exclusive access
        [DispId(0x20d)]
        string ExclusiveAccessOwner { [DispId(0x20d)] get; }
    }

    #endregion // Interfaces

    /// <summary>
    /// Microsoft IMAPIv2 Disc Master
    /// </summary>
    [ComImport]
    [Guid("27354130-7F64-5B0F-8F00-5D77AFBE261E")]
    [CoClass(typeof(MsftDiscMaster2Class))]
    public interface MsftDiscMaster2 : IDiscMaster2 //, DiscMaster2_Event
    {
    }

    [ComImport, ComSourceInterfaces("DDiscMaster2Events\0")]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [Guid("2735412E-7F64-5B0F-8F00-5D77AFBE261E")]
    [ClassInterface(ClassInterfaceType.None)]
    public class MsftDiscMaster2Class
    {
    }

    [ComImport]
    [CoClass(typeof(MsftDiscRecorder2Class))]
    [Guid("27354133-7F64-5B0F-8F00-5D77AFBE261E")]
    public interface MsftDiscRecorder2 : IDiscRecorder2
    {
    }


    [ComImport]
    [Guid("2735412D-7F64-5B0F-8F00-5D77AFBE261E")]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [ClassInterface(ClassInterfaceType.None)]
    public class MsftDiscRecorder2Class 
    {
    }
}



