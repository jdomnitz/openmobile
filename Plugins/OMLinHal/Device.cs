using NDesk.DBus;
using System;
[Interface("org.freedesktop.UDisks.Device")]
public interface Device 
{
    void DriveDetach(string[] options);
    void DriveEject(string[] options);
    void DrivePollMedia();
    void DriveUninhibitPolling(string cookie);
    string FilesystemMount(string type, string[] options);
	void FilesystemUnmount(string[] options);
    string LinuxLoopFilename { get; }
    uint OpticalDiscNumSessions { get; }
    uint OpticalDiscNumAudioTracks { get; }
    uint OpticalDiscNumTracks { get; }
    bool OpticalDiscIsClosed { get; }
    bool OpticalDiscIsAppendable { get; }
    bool OpticalDiscIsBlank { get; }
    bool DriveIsRotational { get; }
    bool DriveCanSpindown { get; }
    bool DriveCanDetach { get; }
    bool DriveIsMediaEjectable { get; }
    string DriveMedia { get; }
    string[] DriveMediaCompatibility { get; }
    ulong DriveConnectionSpeed { get; }
    string DriveConnectionInterface { get; }
    string DriveWriteCache { get; }
    uint DriveRotationRate { get; }
    string DriveWwn { get; }
    string DriveSerial { get; }
    string DriveRevision { get; }
    string DriveModel { get; }
    string DriveVendor { get; }
    int PartitionTableCount { get; }
    string PartitionTableScheme { get; }
    ulong PartitionAlignmentOffset { get; }
    ulong PartitionSize { get; }
    ulong PartitionOffset { get; }
    int PartitionNumber { get; }
    string[] PartitionFlags { get; }
    string PartitionUuid { get; }
    string PartitionLabel { get; }
    string PartitionType { get; }
    string PartitionScheme { get; }
    string IdLabel { get; }
    string IdUuid { get; }
    string IdVersion { get; }
    string IdType { get; }
    string IdUsage { get; }
    string DevicePresentationIconName { get; }
    string DevicePresentationName { get; }
    bool DevicePresentationNopolicy { get; }
    bool DevicePresentationHide { get; }
    ulong DeviceBlockSize { get; }
    ulong DeviceSize { get; }
    uint DeviceMountedByUid { get; }
    string[] DeviceMountPaths { get; }
    bool DeviceIsMounted { get; }
    bool DeviceIsOpticalDisc { get; }
    bool DeviceIsDrive { get; }
    bool DeviceIsReadOnly { get; }
    bool DeviceIsMediaAvailable { get; }
    bool DeviceIsRemovable { get; }
    bool DeviceIsPartitionTable { get; }
    bool DeviceIsPartition { get; }
    bool DeviceIsSystemInternal { get; }
    string[] DeviceFileByPath { get; }
    string[] DeviceFileById { get; }
    string DeviceFilePresentation { get; }
    string DeviceFile { get; }
    long DeviceMinor { get; }
    long DeviceMajor { get; }
    ulong DeviceMediaDetectionTime { get; }
    ulong DeviceDetectionTime { get; }
    string NativePath { get; }
	event VoidCallback Changed;
}