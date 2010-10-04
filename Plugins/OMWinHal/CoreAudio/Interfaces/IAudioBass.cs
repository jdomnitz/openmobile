using System.Runtime.InteropServices;
using System;

namespace OSSpecificLib.CoreAudioApi.Interfaces
{
    [Guid("A2B1A1D9-4DB3-425D-A2B2-BD335CB3E2E5"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioBass:IPerChannelDbLevel
    {

    }
    [Guid("C2F8E001-F205-4BC9-99BC-C13B1E048CCB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPerChannelDbLevel
    {
        int GetChannelCount(out uint pnChannelCount);
        int GetLevelRange(uint nChannel,out float pfMinLevelDB,out float pfMaxLevelDB,out float pfStepping);
        int GetLevel(uint nChannel, out float pfLevelDB);
        int SetLevel(uint nChannel, float pfLevelDB);
        //int SetLevel(uint nChannel, float pfLevelDB,Guid EventContext);
        int SetLevelAllChannels(float[] aLevelsDB,ulong cChannels);
        int SetLevelUniform(float pfLevelDB);
    }
}
