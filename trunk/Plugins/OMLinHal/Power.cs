using NDesk.DBus;

public delegate void VoidCallback();
[Interface("org.gnome.SessionManager")] //Session Bus
public interface SessionManager 
{
    void RequestReboot();
    void RequestShutdown();
    event VoidCallback SessionOver;
}

[Interface("org.kde.KSMServerInterface")] //Session Bus
public interface ksmserver
{
    void logout(int arg1, int arg2, int arg3);
}

[Interface("org.freedesktop.UPower")] //System Bus
public interface UPower 
{
    void Hibernate();
    void Suspend();
    event VoidCallback Resuming;
    event VoidCallback Sleeping;
}

[Interface("org.gnome.PowerManager.Backlight")]
public interface Backlight 
{    
    void SetBrightness(uint percentage_brightness);
}

[Interface("org.kde.PowerDevil")]
public interface PowerDevil
{
    void setBrightness(int percentage_brightness);
    void turnOffScreen();
}