using NDesk.DBus;

public delegate void VoidCallback();
[Interface("org.gnome.SessionManager")] //Session Bus
public interface SessionManager 
{
	
    void RequestReboot();
    void RequestShutdown();
    event VoidCallback SessionOver;
}

[Interface("org.freedesktop.UPower")] //System Bus
public interface UPower {
    
    void Hibernate();
    void Suspend();
    event VoidCallback Resuming;
    event VoidCallback Sleeping;
}