/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System.Diagnostics;
using System.IO;

namespace OpenMobile.Framework
{
    internal static class Linux
    {
        internal static eDriveType detectType(string path)
        {
            string local = translateLocal(path);
            if (string.IsNullOrEmpty(local))
                return eDriveType.Unknown;
            if (local == "rootfs")
                return eDriveType.Fixed;
            string response = null;
            ProcessStartInfo info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveIsMediaEjectable'");
            info.RedirectStandardOutput = true; info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = new Process();
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            response = p.StandardOutput.ReadToEnd().Trim();
            if (p.ExitCode != 0)
                response = null;
            if (response == "true")
                return eDriveType.CDRom;
            info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveCanDetach'");
            info.RedirectStandardOutput = true; info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            p = new Process();
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();
            response = p.StandardOutput.ReadToEnd().Trim();
            if (p.ExitCode != 0)
                response = null;
            if (response == "true")
            {
                info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveModel'");
                info.RedirectStandardOutput = true; info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                p = new Process();
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                response = p.StandardOutput.ReadToEnd().Trim();
                if (p.ExitCode != 0)
                    response = null;
                if (response.Contains("Phone"))
                    return eDriveType.Phone;
                if (response.Contains("iPod") || response.Contains("Apple Mobile Device"))
                    return eDriveType.iPod;
                return eDriveType.Removable;
            }
            return eDriveType.Fixed;
        }
        private static string translateLocal(string path)
        {
            StreamReader r = new StreamReader(File.OpenRead("/proc/mounts"));
            string[] lines = r.ReadToEnd().Split(new char[] { '\r', '\n' });
            foreach (string line in lines)
                if (line.Contains(path))
                    return Path.GetFileName(line.Substring(0, line.IndexOf(' ')));
            return null;
        }

        internal static string getVolumeLabel(string path)
        {
            if (path == "/")
                return "File System (/)";
            string local = translateLocal(path);
            string response = null;
            if (local != null)
            {
                ProcessStartInfo info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'IdLabel'");
                info.RedirectStandardOutput = true; info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                response = p.StandardOutput.ReadToEnd().Trim();
                if (p.ExitCode != 0)
                    response = null;
            }
            if (path.StartsWith("cdda:"))
            {
                local = path.Substring(7).TrimEnd(new char[] { '/' });
                path = null;
            }
            ProcessStartInfo info2 = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'OpticalDiscNumAudioTracks'");
            info2.RedirectStandardOutput = true; info2.UseShellExecute = false;
            info2.WindowStyle = ProcessWindowStyle.Hidden;
            Process t = new Process();
            t.StartInfo = info2;
            t.Start();
            t.WaitForExit();
            string tracks = t.StandardOutput.ReadToEnd().Trim();
            if (path == null)
                return "CD-ROM|" + tracks;
            else if (string.IsNullOrEmpty(response))
                return Path.GetFileName(new DriveInfo(path).VolumeLabel) + "|" + tracks;
            else
                return response.Replace("_", " ");
        }
    }
}
