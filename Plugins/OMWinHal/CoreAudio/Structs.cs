using System;

namespace OSSpecificLib.CoreAudioApi
{
    enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2,
        EDataFlow_enum_count = 3
    }
    enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2,
        ERole_enum_count = 3
    }
    [Flags]
    enum EEndpointHardwareSupport
    {
        Volume = 0x00000001,
        Mute = 0x00000002,
        Meter = 0x00000004
    }
    [Flags]
    enum EDeviceState : uint
    {
        DEVICE_STATE_ACTIVE = 0x00000001,
        DEVICE_STATE_UNPLUGGED = 0x00000002,
        DEVICE_STATE_NOTPRESENT = 0x00000004,
        DEVICE_STATEMASK_ALL = 0x00000007
    }
    struct PropertyKey
    {
        public Guid fmtid;
        public int pid;
    };
    sealed class killWarning
    {
        public killWarning()
        {
            PropertyKey k = new PropertyKey();
            k.fmtid = Guid.Empty;
            k.pid = 0;
        }
    }
}
