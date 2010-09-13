using NDesk.DBus;
public delegate void objectcallback(ObjectPath o);

[Interface("org.freedesktop.UDisks")]
public interface UDisks {
    
    void Uninhibit(string cookie);
    string Inhibit();
    ObjectPath FindDeviceByMajorMinor(long device_major, long device_minor);
    ObjectPath FindDeviceByDeviceFile(string device_file);
    string[] EnumerateDeviceFiles();
    ObjectPath[] EnumerateDevices();
    ObjectPath[] EnumeratePorts();
    ObjectPath[] EnumerateExpanders();
    ObjectPath[] EnumerateAdapters();
    event objectcallback DeviceChanged;
    event objectcallback DeviceRemoved;
    event objectcallback DeviceAdded;
}

