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
using System;
using System.IO;
using OpenMobile.Framework;

namespace OpenMobile
{
    static class StorageAnalyzer
    {
        public static void AnalyzeAsync(string path, bool justInserted)
        {
            SandboxedThread.Asynchronous(delegate { AnalyzeWorker(path, justInserted); });
        }

        private static void AnalyzeWorker(string path, bool justInserted)
        {
            Core.theHost.RaiseStorageEvent(Analyze(path), justInserted, path);
        }

        public static eMediaType Analyze(string path)
        {
            try
            {
                string[] folders = Directory.GetDirectories(path);
                if (folders.Length == 0)
                {
                    if ((Directory.GetFiles(path).Length > 0) && (Directory.GetFiles(path)[0].EndsWith(".cda")))
                        return eMediaType.AudioCD;
                }
                else
                    foreach (string folder in folders)
                    {
                        switch (Path.GetFileName(folder).ToUpper())
                        {
                            case "VIDEO_TS":
                                return eMediaType.DVD;
                            case "BDMV":
                                return eMediaType.BluRay;
                            case "IPOD_CONTROL":
                                return eMediaType.AppleDevice;
                            case "HVDVD_TS":
                                return eMediaType.HDDVD;
                            case "DCIM":
                                if (folders.Length == 1)
                                    return eMediaType.Camera;
                                break;
                        }
                    }
                if (File.Exists(Path.Combine(path, "OpenDrive.ini")))
                    return eMediaType.OpenDrive;
                if (OpenMobile.Media.DeviceInfo.get(path).DriveType == eDriveType.Phone)
                    return eMediaType.Smartphone;
                if (OpenMobile.Media.DeviceInfo.get(path).DriveType == eDriveType.CDRom)
                    return eMediaType.AudioCD;
                return eMediaType.LocalHardware;
            }
            catch (Exception)
            {
                return eMediaType.LocalHardware;
            };
        }
    }
}
