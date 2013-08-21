using NDesk.DBus;
[Interface("org.freedesktop.NetworkManager")]
public interface NetworkManager
{    
    uint state();
    void Enable(bool enable);
    void DeactivateConnection(ObjectPath active_connection);
    ObjectPath ActivateConnection(string service_name, ObjectPath connection, ObjectPath device, ObjectPath specific_object);
    ObjectPath[] GetDevices();
    //event EventHandler<uint> StateChange;
    //event EventHandler<string> DeviceRemoved;
    //event EventHandler<string> DeviceAdded;
    //event EventHandler<uint> StateChanged;
    uint State { get; }
    ObjectPath[] ActiveConnections { get; }
    bool WwanHardwareEnabled { get; }
    bool WwanEnabled { get; }
    bool WirelessHardwareEnabled { get; }
    bool WirelessEnabled { get; }
    bool NetworkingEnabled { get; }
}