using NDesk.DBus;

[Interface("org.freedesktop.DBus.Properties")]
public interface Properties
{    
    object Get(string inf, string propname);
}

