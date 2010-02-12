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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace OpenMobile
{
    static class StorageAnalyzer
    {
        public static void AnalyzeAsync(string path){
            Thread t = new Thread(new ThreadStart(delegate{AnalyzeWorker(path);}));
            t.Priority = ThreadPriority.BelowNormal;
            t.Start();
        }

        private static void AnalyzeWorker(string path)
        {
            Core.theHost.RaiseStorageEvent(Analyze(path), path);
        }

        public static eMediaType Analyze(string path)
        {
            try
            {
                string[] folders = Directory.GetDirectories(path);
                if (folders.Length == 0)
                    if (Directory.GetFiles(path)[0].EndsWith(".cda"))
                        return eMediaType.AudioCD;
                else
                    foreach (string folder in folders)
                    {
                        switch (folder.ToUpper())
                        {
                            case "VIDEO_TS":
                                return eMediaType.DVD;
                            case "BDMV":
                                return eMediaType.BluRay;
                            case "IPOD_CONTROL":
                                return eMediaType.iPod;
                            case "HVDVD_TS":
                                return eMediaType.HDDVD;
                        }
                    }
                if (File.Exists(Path.Combine(path, "OpenDrive.ini")))
                    return eMediaType.OpenDrive;
                return eMediaType.LocalHardware;
            }catch(Exception){
                return eMediaType.LocalHardware;
            };
        }
    }
}
