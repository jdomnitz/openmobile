using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenMobile
{
    public static class Configuration
    {
        /// <summary>Gets a System.Boolean indicating whether OpenMobile is running on a Windows platform.</summary>
        public static bool RunningOnWindows 
        {
            get { return OpenTK.Configuration.RunningOnWindows; } 
        }

        /// <summary>Gets a System.Boolean indicating whether OpenMobile is running on an X11 platform.</summary>
        public static bool RunningOnX11
        {
            get { return OpenTK.Configuration.RunningOnX11; }
        }
        /// <summary>
        /// Gets a System.Boolean indicating whether OpenMobile is running on a tablet PC
        /// </summary>
        public static bool TabletPC
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> indicating whether OpenMobile is running on a Unix platform.
        /// </summary>
        public static bool RunningOnUnix
        {
            get { return OpenTK.Configuration.RunningOnUnix; }
        }


        /// <summary>Gets a System.Boolean indicating whether OpenMobile is running on an X11 platform.</summary>
        public static bool RunningOnLinux { get { return OpenTK.Configuration.RunningOnLinux; } }


        /// <summary>Gets a System.Boolean indicating whether OpenMobile is running on a MacOS platform.</summary>
        public static bool RunningOnMacOS { get { return OpenTK.Configuration.RunningOnMacOS; } }

        /// <summary>
        /// Gets a System.Boolean indicating whether OpenMobile is running on embedded hardware
        /// </summary>
        public static bool RunningOnEmbedded { get { return false; } }


    }
}
