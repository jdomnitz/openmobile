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
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;
using OpenMobile.Framework.Data;

namespace OpenMobile
{
    /// <summary>
    /// The OM host
    /// </summary>
    public static class OM
    {
        /// <summary>
        /// A reference to the pluginhost to use for the framework.
        /// </summary>
        public static IPluginHost Host
        {
            get
            {
                return BuiltInComponents.Host;
            }
        }

        /// <summary>
        /// A reference to a global setting object 
        /// </summary>
        public static IBasePlugin GlobalSetting
        {
            get
            {
                return BuiltInComponents.OMInternalPlugin;
            }
        }
    }

    /// <summary>
    /// Built in plugins
    /// </summary>
    public static class BuiltInComponents
    {
        private class OMInternalPluginClass : BasePluginCode
        {
            public OMInternalPluginClass()
                : base("OM", (BuiltInComponents.Host == null ? imageItem.NONE : BuiltInComponents.Host.getSkinImage("Icons|Icon-OM")), 1.0f, "Internal usage plugin", "OM", "")
            {
            }
            public override eLoadStatus initialize(IPluginHost host)
            {
                return eLoadStatus.LoadSuccessful;
            }
        }

        /// <summary>
        /// A screenmanager for panels created and handled by the framework
        /// NB! Panels from the framework is always common for all screens
        /// </summary>
        public static OpenMobile.Framework.ScreenManager Panels = null;

        private static IPluginHost _Host = null;
        /// <summary>
        /// A reference to the pluginhost to use for the framework.
        /// NB! This reference is not valid unless it's initialized from the core at startup
        /// </summary>
        public static IPluginHost Host
        {
            get 
            {
                return _Host; 
            }
            set
            {
                _Host = value;
                OMInternalPlugin = new OMInternalPluginClass();
                Panels = new OpenMobile.Framework.ScreenManager(OMInternalPlugin);
            }
        }

        /// <summary>
        /// An internal usage OM plugin
        /// </summary>
        public static IBasePlugin OMInternalPlugin = new OMInternalPluginClass();

        /// <summary>
        /// The copyright information to be displayed on the about screen
        /// </summary>
        public static string AboutText
        {
            get
            {
                //WARNING: REMOVING ANY OF THE BELOW DESCRIPTION IS A VIOLATION OF THE LICENSE AGREEMENT

                string Text = "OpenMobile is copyrighted (2009 - 2013) to the OpenMobile Foundation and its contributors.\r\n\r\n";
                Text += "This program in full or in part is protected under a clarified version of the GPLv3 license which can be found in the application directory.\r\n\r\n";
                Text += "Contributors:\r\n";
                Text += "Justin Domnitz (justchat_1) - Founder / Developer\r\n";
                Text += "UnusuallyGenius - Graphics\r\n";
                Text += "ws6vert - openOBD and Garmin Mobile PC Projects\r\n";
                Text += "Borte - Developer (Framework/Plugins/Graphics)\r\n";
                Text += "jheizer - Developer\r\n";
                Text += "jmullan99@gmail.com - Tester / plugin developer\r\n";
                Text += "Efess - Navit developer\r\n";
                Text += "detlion1643 - Plugin Developer\r\n";
                Text += "\r\nSupporting Projects:\r\n";
                Text += "TagLib Sharp, The Mono Project, iPod Sharp, DBusSharp, Sqlite, Aqua Gauge, CoreAudio, mPlayer and the Open ToolKit Project";
                return Text;
            }
        }
        private static OMPanel panelAbout;
        /// <summary>
        /// Returns an About panel for display
        /// </summary>
        /// <returns></returns>
        public static OMPanel AboutPanel()
        {
            if (panelAbout == null)
            {
                panelAbout = new OMPanel("About");
                panelAbout.PanelType = OMPanel.PanelTypes.Modal;
                panelAbout.Priority = ePriority.High;
                panelAbout.Forgotten = true;
                panelAbout.BackgroundType = backgroundStyle.SolidColor;
                panelAbout.BackgroundColor1 = Color.FromArgb(180, Color.Black);

                OMContainer contAboutContainer = new OMContainer("contAboutContainer", 0, 0, 1000, 600);
                panelAbout.addControl(contAboutContainer);

                //OMLabel description = new OMLabel(30, 55, 940, 540);
                OMLabel lblDescription = new OMLabel("lblDescription", 25, 0, 950, 550);
                lblDescription.TextAlignment = OpenMobile.Graphics.Alignment.WordWrap | OpenMobile.Graphics.Alignment.TopCenter;
                lblDescription.Text = AboutText;
                contAboutContainer.addControl(lblDescription);

                OMLabel lblInfo = new OMLabel("lblInfo", lblDescription.Left, lblDescription.Region.Bottom, lblDescription.Width, 25);
                lblInfo.Text = "Click this message to return";
                contAboutContainer.addControl(lblInfo);

                OMButton btnReturn = new OMButton("btnReturn", lblDescription.Left, lblDescription.Top, lblDescription.Width, lblDescription.Height + lblInfo.Height);
                btnReturn.OnClick += new userInteraction(btnReturn_OnClick);
                contAboutContainer.addControl(btnReturn);
            }
            return panelAbout;
        }

        static void btnReturn_OnClick(OMControl sender, int screen)
        {
            Host.execute(eFunction.goBack, screen.ToString());
        }

        static IPluginHost theHost;

        /// <summary>
        /// Returns a settings object with OM system settings
        /// </summary>
        /// <returns></returns>
        public static Settings GlobalSettings()
        {
            theHost = BuiltInComponents.Host;

            Settings gl = new Settings("System Settings");
            Setting graphics = new Setting(SettingTypes.MultiChoice, "UI.MinGraphics", String.Empty, "Disable Enhanced Graphics", Setting.BooleanList, Setting.BooleanList);
            Setting volume = new Setting(SettingTypes.MultiChoice, "UI.VolumeChangesVisible", "", "Show Volume Level when adjusting volume", Setting.BooleanList, Setting.BooleanList);
            Setting ShowCursor = new Setting(SettingTypes.MultiChoice, "UI.ShowCursor", String.Empty, "Show OM mouse/pointer cursors", Setting.BooleanList, Setting.BooleanList);
            Setting ShowDebugInfo = new Setting(SettingTypes.MultiChoice, "UI.ShowDebugInfo", String.Empty, "Show debug info overlay", Setting.BooleanList, Setting.BooleanList);
            Setting ShowGestures = new Setting(SettingTypes.MultiChoice, "UI.ShowGestures", String.Empty, "Draw gestures while gesturing", Setting.BooleanList, Setting.BooleanList);
            Setting OpenGLVsync = new Setting(SettingTypes.MultiChoice, "OpenGL.VSync", String.Empty, "Use VSync for opengl", Setting.BooleanList, Setting.BooleanList);
            //Setting SkinColor = new Setting(SettingTypes.Text, "UI.SkinColor", "Foreground", "Skin foreground color (R,G,B)");
            Setting SkinFocusColor = new Setting(SettingTypes.Text, "UI.SkinFocusColor", "Focus color", "Skin focus color (R,G,B)");
            Setting SkinTextColor = new Setting(SettingTypes.Text, "UI.SkinTextColor", "Text color", "Skin text color (R,G,B)");
            Setting UseIconOverlayColor = new Setting(SettingTypes.MultiChoice, "UI.UseIconOverlayColor", String.Empty, "Overlay icons with text color", Setting.BooleanList, Setting.BooleanList);
            //Setting TransitionSpeed = new Setting(SettingTypes.MultiChoice, "UI.TransitionSpeed", "Transition speed", "Transition; speed multiplier", PanelTransitionEffectHandler.GetEffectNames(), PanelTransitionEffectHandler.GetEffectNames());
            Setting TransitionSpeed = PanelTransitionEffectHandler.Setting_TransitionSpeed();
            Setting TransitionDefaultEffect = PanelTransitionEffectHandler.Setting_TransitionDefaultEffect();
            //Setting TransitionDefaultEffect = new Setting(SettingTypes.MultiChoice, "UI.TransitionDefaultEffect", "Transition effect", "Transition; Default effect", PanelTransitionEffectHandler.GetEffectNames(), PanelTransitionEffectHandler.GetEffectNames());
            Setting IdleDetectionInterval = new Setting(SettingTypes.Text, "UI.IdleDetection.Interval", "Seconds", "Seconds before a screen is set to idle state");
            
            using (PluginSettings settings = new PluginSettings())
            {
                graphics.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.MinGraphics");
                volume.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.VolumeChangesVisible");
                ShowCursor.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.ShowCursor");
                ShowDebugInfo.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.ShowDebugInfo");
                ShowGestures.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.ShowGestures");
                OpenGLVsync.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "OpenGL.VSync");
                //SkinColor.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.SkinColor");
                SkinFocusColor.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.SkinFocusColor");
                SkinTextColor.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.SkinTextColor");
                TransitionSpeed.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.TransitionSpeed");
                TransitionDefaultEffect.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.TransitionDefaultEffect");
                UseIconOverlayColor.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.UseIconOverlayColor");
                IdleDetectionInterval.Value = settings.getSetting(BuiltInComponents.OMInternalPlugin, "UI.IdleDetection.Interval");
            }
            gl.Add(graphics);
            gl.Add(volume);
            gl.Add(ShowCursor);
            gl.Add(ShowDebugInfo);
            gl.Add(ShowGestures);
            gl.Add(OpenGLVsync);
            //gl.Add(SkinColor);
            gl.Add(SkinFocusColor);
            gl.Add(SkinTextColor);
            gl.Add(UseIconOverlayColor);
            gl.Add(TransitionSpeed);
            gl.Add(TransitionDefaultEffect);

            // Home position setting
            gl.Add(new Setting(SettingTypes.Text, "Location.Home.UserEntered", "Home location:", "Postcode, city and state / country, etc.", StoredData.Get(BuiltInComponents.OMInternalPlugin, "Location.Home.UserEntered")));
            
            gl.Add(IdleDetectionInterval);
            gl.OnSettingChanged += new SettingChanged(SettingsChanged);

            // Add settings for each screen
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                // Add settings for idle actions
                string name = String.Format("UI.IdleDetection.Action.{0}", i);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, name, "");
                gl.Add(new Setting(SettingTypes.Text, name, String.Format("Screen {0}", i), "The action to execute when set to idle", StoredData.Get(BuiltInComponents.OMInternalPlugin, name)));

                // Add settings for startup commands
                name = String.Format("UI.Startup.Action.{0}", i);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, name, "");
                gl.Add(new Setting(SettingTypes.Text, name, String.Format("Screen {0}", i), "The action to execute at startup", StoredData.Get(BuiltInComponents.OMInternalPlugin, name)));
            }

            return gl;
        }

        static void SettingsChanged(int screen, Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
            {
                s.setSetting(BuiltInComponents.OMInternalPlugin, setting.Name, setting.Value);
                switch (setting.Name)
                {
                    case "UI.MinGraphics":
                        if (setting.Value == "True")
                            theHost.GraphicsLevel = eGraphicsLevel.Minimal;
                        else
                            theHost.GraphicsLevel = eGraphicsLevel.Standard;
                        break;

                    case "UI.ShowCursor":
                        if (setting.Value == "True")
                            theHost.ShowCursors = true;
                        else
                            theHost.ShowCursors = false;
                        break;

                    case "UI.ShowDebugInfo":
                        if (setting.Value == "True")
                            theHost.ShowDebugInfo = true;
                        else
                            theHost.ShowDebugInfo = false;
                        break;

                    case "UI.TransitionSpeed":
                        BuiltInComponents.SystemSettings.TransitionSpeed = StoredData.GetFloat(OMInternalPlugin, "UI.TransitionSpeed", 1.0F);
                        break;

                    case "UI.TransitionDefaultEffect":
                        BuiltInComponents.SystemSettings.TransitionDefaultEffect = setting.Value;
                        break;

                    case "UI.SkinTextColor":
                        BuiltInComponents.SystemSettings.SkinTextColor = StoredData.GetColor(OMInternalPlugin, "UI.SkinTextColor", Color.White);
                        break;

                    case "OM;UI.SkinFocusColor":
                        BuiltInComponents.SystemSettings.SkinFocusColor = StoredData.GetColor(OMInternalPlugin, "UI.SkinFocusColor", Color.Blue);
                        break;

                    case "OM;UI.ShowGestures":
                        BuiltInComponents.SystemSettings.ShowGestures = StoredData.GetBool(OMInternalPlugin, "UI.ShowGestures");
                        break;

                    case "UI.UseIconOverlayColor":
                        BuiltInComponents.SystemSettings.UseIconOverlayColor = StoredData.GetBool(OMInternalPlugin, "UI.UseIconOverlayColor");
                        break;

                    case "UI.IdleDetection.Interval":
                        BuiltInComponents.SystemSettings.IdleDetectionInterval = StoredData.GetInt(OMInternalPlugin, "UI.IdleDetection.Interval");
                        break;

                    case "Location.Home.UserEntered":
                        {   // Try to get a location based on the info the user entered
                            Location newLoc;
                            if (Location.TryParse(setting.Value, out newLoc))
                            {   // We where able to parse it, use it directly
                                BuiltInComponents.SystemSettings.Location_Home = newLoc;
                            }
                            else
                            {   // No parsing was possible, use it as a keyword and hope that some plugin catches it..
                                BuiltInComponents.SystemSettings.Location_Home = Location.FromKeyword(setting.Value);
                            }
                        }
                        break;

                    default:
                        theHost.execute(eFunction.settingsChanged, setting.Name);
                        break;
                }
            }
        }

        /// <summary>
        /// OpenMobile System settings
        /// </summary>
        public static class SystemSettings
        {
            /// <summary>
            /// Initializes systemsettings
            /// </summary>
            public static void Init()
            {
                // Set default values
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.SkinFocusColor", "00,00,FF");
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.SkinTextColor", "FF,FF,FF");
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.UseIconOverlayColor", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.MinGraphics", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.VolumeChangesVisible", true);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.ShowCursor", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.ShowDebugInfo", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.ShowGestures", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.TransitionSpeed", 1.0f);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.TransitionDefaultEffect", "Random");
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "OpenGL.VSync", false);
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, "UI.IdleDetection.Interval", 60);

                // Update local data variables (for speed)
                BuiltInComponents.Host.ShowDebugInfo = StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowDebugInfo");
                BuiltInComponents.Host.ShowCursors = StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowCursor");
                _SkinTextColor = StoredData.GetColor(BuiltInComponents.OMInternalPlugin, "UI.SkinTextColor", Color.White);
                _SkinFocusColor = StoredData.GetColor(BuiltInComponents.OMInternalPlugin, "UI.SkinFocusColor", Color.Blue);
                _UseIconOverlayColor = StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.UseIconOverlayColor");
            }

            /// <summary>
            /// OM System setting: True = use minimalistic graphics
            /// </summary>
            public static bool UseSimpleGraphics
            {
                get
                {
                    return StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.MinGraphics");
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "UI.MinGraphics", value);
                }
            }

            /// <summary>
            /// OM System setting: True = Show volume changes
            /// </summary>
            public static bool VolumeChangesVisible
            {
                get
                {
                    return StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.VolumeChangesVisible");
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "UI.VolumeChangesVisible", value);
                }
            }

            /// <summary>
            /// OM System setting: True = Show OM system cursors
            /// </summary>
            public static bool ShowCursor
            {
                get
                {
                    return StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowCursor");
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowCursor", value);
                }
            }

            /// <summary>
            /// OM System setting: True = Show debug info
            /// </summary>
            public static bool ShowDebugInfo
            {
                get
                {
                    return StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowDebugInfo");
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowDebugInfo", value);
                }
            }

            private static bool _ShowGestures = false;
            /// <summary>
            /// OM System setting: True = Show debug info
            /// </summary>
            public static bool ShowGestures
            {
                get
                {
                    return _ShowGestures;
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "UI.ShowGestures", value);
                    _ShowGestures = value;
                }
            }

            /// <summary>
            /// OM System setting: True = Use VSync for OpengGL rendering
            /// </summary>
            public static bool OpenGLVSync
            {
                get
                {
                    return StoredData.GetBool(BuiltInComponents.OMInternalPlugin, "OpenGL.VSync");
                }
                set
                {
                    StoredData.SetBool(BuiltInComponents.OMInternalPlugin, "OpenGL.VSync", value);
                }
            }

            /// <summary>
            /// OM System setting: Transition speed multiplier
            /// </summary>
            public static float TransitionSpeed
            {
                get
                {
                    if (_TransitionSpeed == 0f)
                        _TransitionSpeed = StoredData.GetFloat(BuiltInComponents.OMInternalPlugin, "UI.TransitionSpeed", 1f);
                    if (_TransitionSpeed == 0)
                    {
                        _TransitionSpeed = 1f;
                        StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.TransitionSpeed", 1f);
                    }
                    return _TransitionSpeed;
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.TransitionSpeed", value);
                    _TransitionSpeed = value;
                }
            }
            private static float _TransitionSpeed = 0f;

            /// <summary>
            /// OM System setting: Transition default effect
            /// </summary>
            public static string TransitionDefaultEffect
            {
                get
                {
                    if (String.IsNullOrEmpty(_TransitionDefaultEffect))
                        _TransitionDefaultEffect = StoredData.Get(BuiltInComponents.OMInternalPlugin, "UI.TransitionDefaultEffect");
                    // Safety check
                    if (String.IsNullOrEmpty(_TransitionDefaultEffect))
                    {
                        StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.TransitionDefaultEffect", "Random");
                        _TransitionDefaultEffect = "Random";
                    }
                    return _TransitionDefaultEffect; 
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.TransitionDefaultEffect", value);
                    _TransitionDefaultEffect = value;
                }
            }
            private static string _TransitionDefaultEffect = "";

            /// <summary>
            /// OM System setting: Use icon overlay?
            /// </summary>
            public static bool UseIconOverlayColor
            {
                get
                {
                    return _UseIconOverlayColor;
                }
                set
                {
                    if (_UseIconOverlayColor != value)
                    {
                        StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.UseIconOverlayColor", value);
                        _UseIconOverlayColor = value;
                    }
                }
            }
            private static bool _UseIconOverlayColor;
        

            private static OpenMobile.Graphics.Color _SkinFocusColor = Color.Blue;
            /// <summary>
            /// OM System setting: Skin Focus color
            /// </summary>
            public static OpenMobile.Graphics.Color SkinFocusColor
            {
                get
                {
                    return _SkinFocusColor;
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.SkinFocusColor", String.Format("{0},{1},{2}", value.R.ToString("X2"), value.G.ToString("X2"), value.B.ToString("X2")));
                    _SkinTextColor = StoredData.GetColor(BuiltInComponents.OMInternalPlugin, "UI.SkinFocusColor", Color.White);
                }
            }


            private static OpenMobile.Graphics.Color _SkinTextColor = Color.White;
            /// <summary>
            /// OM System setting: Skin text color
            /// </summary>
            public static OpenMobile.Graphics.Color SkinTextColor
            {
                get
                {
                    return _SkinTextColor;
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.SkinTextColor", String.Format("{0},{1},{2}", value.R.ToString("X2"), value.G.ToString("X2"), value.B.ToString("X2")));
                    _SkinTextColor = StoredData.GetColor(BuiltInComponents.OMInternalPlugin, "UI.SkinTextColor", Color.White);
                }
            }

            /// <summary>
            /// The name of the default music database 
            /// </summary>
            public static string DefaultDB_Music
            {
                get
                {
                    return StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase", value);
                }
            }

            /// <summary>
            /// The name of the default CD database 
            /// </summary>
            public static string DefaultDB_CD
            {
                get
                {
                    return StoredData.Get(OM.GlobalSetting, "Default.CDDatabase");
                }
                set
                {
                    StoredData.Set(OM.GlobalSetting, "Default.CDDatabase", value);
                }
            }

            /// <summary>
            /// The name of the default database for external media (external harddrives, phones etc...)
            /// </summary>
            public static string DefaultDB_Removable
            {
                get
                {
                    return StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.RemovableDatabase");
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "Default.RemovableDatabase", value);
                }
            }

            /// <summary>
            /// The name of the default Apple database (iPhone, iPod etc...)
            /// </summary>
            public static string DefaultDB_Apple
            {
                get
                {
                    return StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.AppleDatabase");
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "Default.AppleDatabase", value);
                }
            }

            /// <summary>
            /// OM System setting: Idle detection interval
            /// </summary>
            public static int IdleDetectionInterval
            {
                get
                {
                    return StoredData.GetInt(BuiltInComponents.OMInternalPlugin, "UI.IdleDetection.Interval");
                }
                set
                {
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "UI.IdleDetection.Interval", value);
                }
            }

            /// <summary>
            /// OM System setting: Idle detection action (per screen setting)
            /// </summary>
            public static string IdleDetectionAction(int screen)
            {
                return StoredData.Get(BuiltInComponents.OMInternalPlugin, String.Format("UI.IdleDetection.Action.{0}", screen));
            }

            /// <summary>
            /// OM System setting: Startup action (per screen setting)
            /// </summary>
            public static string StartupAction(int screen)
            {
                return StoredData.Get(BuiltInComponents.OMInternalPlugin, String.Format("UI.Startup.Action.{0}", screen));
            }

            /// <summary>
            /// The position set as home by the user
            /// </summary>
            public static Location Location_Home
            {
                get
                {
                    // Should we get the data from the DB?
                    if (_Location_Home == null)
                    {   // Yes, extract data object
                        string XML = StoredData.Get(BuiltInComponents.OMInternalPlugin, "Location.Home.Data");
                        try
                        {
                            Location_Home = OpenMobile.helperFunctions.XML.Serializer.fromXML<Location>(XML);
                        }
                        catch 
                        {
                            Location_Home = new Location();
                        }
                    }
                    return _Location_Home;
                }
                set
                {
                    _Location_Home = value;
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, "Location.Home.Data", OpenMobile.helperFunctions.XML.Serializer.toXML(_Location_Home));

                    // Push update to datasource
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OM;Location.Favorite.Home", _Location_Home);
                }
            }
            private static Location _Location_Home = null;

        }

        /// <summary>
        /// System data providers
        /// </summary>
        public static class DataSources
        {
            /// <summary>
            /// Init the system data providers
            /// </summary>
            static public void Init()
            {
                // Time with HH:MM
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Time", "", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current time in a short format"));

                // Time with HH:MM:SS
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Time", "Long", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current time in a long format with seconds"));

                // Time with local time
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Time", "Local", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current time in a local style format"));

                // Date
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Date", "", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current date in a short format"));

                // Date long
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Date", "Long", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current date in a long format"));

                // Date text
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Date", "Text", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current date in a local format"));

                // Create a datasource
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "DateTime", "", 1000, DataSource.DataTypes.text, DateTimeProvider, "Current date and time in a local format"));

                //// CPU Usage
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "CPU", "Load", 1000, DataSource.DataTypes.percent, ComputerDataProvider, "Total CPU load"));

                //// Memory Usage
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Memory", "Free", 1000, DataSource.DataTypes.bytes, ComputerDataProvider, "Total free memory in bytes"));

                //// Memory Usage
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Memory", "Used", 1000, DataSource.DataTypes.bytes, ComputerDataProvider, "Total used memory in bytes"));

                //// Memory Usage
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Memory", "UsedPercent", 1000, DataSource.DataTypes.percent, ComputerDataProvider, "Total used memory in percent"));

                //// Memory Usage
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "System", "Memory", "ProcessUsed", 1000, DataSource.DataTypes.bytes, ComputerDataProvider, "Currently used memory by OM"));

                // Home location
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", "Location", "Favorite", "Home", 0, DataSource.DataTypes.raw, DataProvider, "Home location as set by the user"));


            }



            static private System.Diagnostics.PerformanceCounter cpuCounter;
            static private System.Diagnostics.PerformanceCounter freeRamCounter;
            static private System.Diagnostics.Process currentProcess;

            static private object ComputerDataProvider(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
            {
                result = true;
                switch (dataSource.FullName)
                {
                    /*
                    case "System.CPU.Load":
                        {
                            try
                            {
                                if (cpuCounter == null)
                                {
                                    cpuCounter = new System.Diagnostics.PerformanceCounter();
                                    cpuCounter.CategoryName = "Processor";
                                    cpuCounter.CounterName = "% Processor Time";
                                    cpuCounter.InstanceName = "_Total";
                                    cpuCounter.NextValue(); //init
                                }
                                return (float)System.Math.Round(cpuCounter.NextValue(), 0);
                            }
                            catch
                            {   // Error handler in case performance counter fails
                                return 0;
                            }
                        }
                    case "System.Memory.Free":
                        {
                            try
                            {
                                if (freeRamCounter == null)
                                    freeRamCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
                                return freeRamCounter.NextValue() * 1048576;
                            }
                            catch
                            {   // Error handler in case performance counter fails
                                return 0;
                            }
                        }
                    case "System.Memory.Used":
                        if (Configuration.RunningOnWindows)
                            return (int)(OpenMobile.Framework.Windows.getUsedPhysicalMemory());
                        return null;
                    case "System.Memory.UsedPercent":
                        if (Configuration.RunningOnWindows)
                            return (float)System.Math.Round(OpenMobile.Framework.Windows.getUsedMemoryPercent(), 2);
                        return null;
                    case "System.Memory.ProcessUsed":
                        if (currentProcess == null)
                            currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                        return currentProcess.WorkingSet64;
                    */

                    default:
                        result = false;
                        return null;
                }
            }

            static private object DateTimeProvider(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
            {
                result = true;
                switch (dataSource.FullName)
                {
                    case "System.Time":
                        return DateTime.Now.ToShortTimeString();
                    case "System.Time.Long":
                        return DateTime.Now.ToLongTimeString();
                    case "System.Time.Local":
                        return DateTime.Now.ToLocalTime();
                    case "System.Date":
                        return DateTime.Now.ToShortDateString();
                    case "System.Date.Long":
                        return DateTime.Now.ToLongDateString();
                    case "System.Date.Text":
                        return DateTime.Now.ToString("D");
                    case "System.DateTime":
                        return DateTime.Now.ToString();
                    default:
                        result = false;
                        return null;
                }
            }

            static private object DataProvider(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
            {
                result = true;
                switch (dataSource.FullName)
                {
                    case "Location.Favorite.Home":
                        return BuiltInComponents.SystemSettings.Location_Home;
                    default:
                        result = false;
                        return null;
                }
            }

        }

    }

    /// <summary>
    /// Timing functions
    /// </summary>
    public static class Timing
    {
        static DateTime start = DateTime.Now;
        static DateTime intermediate = DateTime.Now;

        /// <summary>
        /// Internal use only! 
        /// Gets timing data since last call of this method (used for timing of code)
        /// </summary>
        /// <returns></returns>
        public static string GetTiming()
        {
            double Duration = (DateTime.Now - intermediate).TotalMilliseconds;
            intermediate = DateTime.Now;
            return Duration.ToString() + " (" + (DateTime.Now - start).TotalMilliseconds.ToString() + ")";
        }
    }

    /// <summary>
    /// Provides a set of OM errorhandling methods
    /// </summary>
    public static class ErrorHandling
    {
        /// <summary>
        /// Formats all required data for an exception into a string 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nTargetSite: \r" + e.TargetSite);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nTargetSite: \r" + e.TargetSite);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }
    }
}
