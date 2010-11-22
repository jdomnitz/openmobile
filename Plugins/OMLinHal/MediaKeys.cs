//
// MultimediaKeyService.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Alexander Hixon <hixon.alexander@mediati.org>
//   Jan Arne Petersen <jap@gnome.org>
//
// Copyright (C) 2007-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Mono.Unix;

using NDesk.DBus;
using System.Diagnostics;

namespace MultimediaKeys
{
    public class MultimediaKeysService : IDisposable
    {
        private const string BusName = "org.gnome.SettingsDaemon";
        private const string ObjectPath = "/org/gnome/SettingsDaemon";

        public delegate void MediaPlayerKeyPressedHandler (string application, string key);

        // GNOME 2.20
        [Interface ("org.gnome.SettingsDaemon")]
        private interface ISettingsDaemon220
        {
            void GrabMediaPlayerKeys (string application, uint time);
            void ReleaseMediaPlayerKeys (string application);
            event MediaPlayerKeyPressedHandler MediaPlayerKeyPressed;
        }

        // GNOME 2.22
        [Interface ("org.gnome.SettingsDaemon.MediaKeys")]
        private interface ISettingsDaemon222
        {
            void GrabMediaPlayerKeys (string application, uint time);
            void ReleaseMediaPlayerKeys (string application);
            event MediaPlayerKeyPressedHandler MediaPlayerKeyPressed;
        }

        private ISettingsDaemon222 settings_daemon_222;
        private ISettingsDaemon220 settings_daemon_220;

        public void Initialize ()
        {
            try {
                settings_daemon_222 = Bus.Session.GetObject<ISettingsDaemon222> (BusName,
                    new ObjectPath (ObjectPath + "/MediaKeys"));
                settings_daemon_222.GrabMediaPlayerKeys ("OMLinHal", 0);
                settings_daemon_222.MediaPlayerKeyPressed += OnMediaPlayerKeyPressed;

                Debug.Print ("Using GNOME 2.22 API for Multimedia Keys");
            } catch {
                settings_daemon_222 = null;

                try {
                    settings_daemon_220 = Bus.Session.GetObject<ISettingsDaemon220> (BusName,
                        new ObjectPath (ObjectPath));
                    settings_daemon_220.GrabMediaPlayerKeys ("OMLinHal", 0);
                    settings_daemon_220.MediaPlayerKeyPressed += OnMediaPlayerKeyPressed;

                    Debug.Print ("Using GNOME 2.20 API for Multimedia keys");
                } catch {
                    settings_daemon_220 = null;
                    //throw new ApplicationException ("No support GNOME Settings Daemon could be reached.");
                }
            }
        }

        public void Dispose ()
        {
            if (settings_daemon_222 != null) {
                settings_daemon_222.MediaPlayerKeyPressed -= OnMediaPlayerKeyPressed;
                settings_daemon_222.ReleaseMediaPlayerKeys ("OMLinHal");
                settings_daemon_222 = null;
            }

            if (settings_daemon_220 != null) {
                settings_daemon_220.MediaPlayerKeyPressed -= OnMediaPlayerKeyPressed;
                settings_daemon_220.ReleaseMediaPlayerKeys ("OMLinHal");
                settings_daemon_220 = null;
            }
        }
		public event MediaPlayerKeyPressedHandler keyPressed;
        private void OnMediaPlayerKeyPressed (string application, string key)
        {
            if (keyPressed!=null)
				keyPressed(application,key);
        }
    }
}


