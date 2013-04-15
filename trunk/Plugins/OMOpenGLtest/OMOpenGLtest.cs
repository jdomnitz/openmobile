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
using OpenMobile;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Controls;
using System.Collections.Generic;
using OpenMobile.mPlayer;
using OpenMobile.helperFunctions;

namespace OMOpenGLtest
{
    public sealed class OMOpenGLtest : HighLevelCode
    {
        public OMOpenGLtest()
            : base("OMOpenGLtest", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "OpenGL Test", "OpenGL Test", "OM Dev team/Borte", "")
        {
        }

        private Dictionary<int, mPlayer> Players = new Dictionary<int, mPlayer>();
        private Dictionary<int, string> SelectedFile = new Dictionary<int, string>();

        /// <summary>
        /// Data per screen instance
        /// </summary>
        StoredData.ScreenInstanceData _ScreenData = new StoredData.ScreenInstanceData();

        public override eLoadStatus initialize(IPluginHost host)
        {
            return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // Create a new panel
            OMPanel panel = new OMPanel("Panel");

            int backgroundOpacity = 170;
            OMButton btnCmd1 = OMButton.PreConfigLayout_BasicStyle("btnCmd1", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Top + 20, 120, 70, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.Top, "", "Stop");
            btnCmd1.OnClick += new userInteraction(btnCmd1_OnClick);
            panel.addControl(btnCmd1);

            OMButton btnCmd2 = OMButton.PreConfigLayout_BasicStyle("btnCmd2", btnCmd1.Region.Left, btnCmd1.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.None, "", "Play");
            btnCmd2.OnClick += new userInteraction(btnCmd2_OnClick);
            panel.addControl(btnCmd2);

            OMButton btnCmd3 = OMButton.PreConfigLayout_BasicStyle("btnCmd3", btnCmd1.Region.Left, btnCmd1.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.None, "", "Pause");
            btnCmd3.OnClick += new userInteraction(btnCmd3_OnClick);
            btnCmd3.Visible = false;
            panel.addControl(btnCmd3);

            OMButton btnCmd4 = OMButton.PreConfigLayout_BasicStyle("btnCmd4", btnCmd1.Region.Left, btnCmd2.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.None, "", "Open");
            btnCmd4.OnClick +=new userInteraction(btnCmd4_OnClick);
            panel.addControl(btnCmd4);

            OMButton btnCmd5 = OMButton.PreConfigLayout_BasicStyle("btnCmd5", btnCmd1.Region.Left, btnCmd4.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.None, "", "Seek-");
            btnCmd5.OnClick += new userInteraction(btnCmd5_OnClick);
            panel.addControl(btnCmd5);

            OMButton btnCmd6 = OMButton.PreConfigLayout_BasicStyle("btnCmd6", btnCmd1.Region.Left, btnCmd5.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.None, "", "Seek+");
            btnCmd6.OnClick += new userInteraction(btnCmd6_OnClick);
            panel.addControl(btnCmd6);

            OMButton btnCmd7 = OMButton.PreConfigLayout_BasicStyle("btnCmd7", btnCmd1.Region.Left, btnCmd6.Region.Bottom - 1, btnCmd1.Region.Width, btnCmd1.Region.Height, backgroundOpacity, OpenMobile.Graphics.GraphicCorners.Bottom, "", "Set");
            btnCmd7.OnClick += new userInteraction(btnCmd7_OnClick);
            panel.addControl(btnCmd7);

            OMLabel lblInfo = new OMLabel("lblInfo", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Bottom - 20, OM.Host.ClientArea[0].Width, 20);
            lblInfo.Text = "Waiting for player...";
            panel.addControl(lblInfo);

            // Create a target window
            OMTargetWindow VideoWindow = new OMTargetWindow("VideoWindow", 150, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Right - 180, OM.Host.ClientArea[0].Height-20);
            VideoWindow.OnWindowCreated += new OMTargetWindow.WindowArgs(VideoWindow_OnWindowCreated);
            VideoWindow.OnWindowDisposed += new OMTargetWindow.WindowArgs(VideoWindow_OnWindowDisposed);
            VideoWindow.OnWindowCovered += new ControlEventHandler(VideoWindow_OnWindowCovered);
            VideoWindow.OnWindowUncovered += new ControlEventHandler(VideoWindow_OnWindowUncovered);
            VideoWindow.OnClick += new userInteraction(VideoWindow_OnClick);
            VideoWindow.Visible = false;
            //VideoWindow.SkinDebug = true;
            panel.addControl(VideoWindow);

            // Load the panel into the local manager for panels
            PanelManager.loadPanel(panel, true);

            host.OnMediaEvent += new MediaEvent(host_OnMediaEvent);

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        void host_OnMediaEvent(eFunction function, Zone zone, string arg)
        {
            if (function == eFunction.ZoneSetActive)
            {
                int screen = -1;
                int.TryParse(arg, out screen);
                if (screen >= 0)
                {
                    // Players audiodevice follows zone
                    AudioDevice audioDevice = BuiltInComponents.Host.ZoneHandler.GetActiveZone(screen).AudioDevice;
                    if (!audioDevice.IsDefault)
                        Players[screen].AudioDevice = audioDevice.Name;
                    else
                        Players[screen].AudioDevice = "";
                    Players[screen].Start(true, false);
                }
            }
        }

        void VideoWindow_OnWindowUncovered(OMControl sender, int screen)
        {
            OMTargetWindow window = sender as OMTargetWindow;
            window.RedirectReset();
            _ScreenData.SetProperty(screen, "VideoWindowMinized", false);
        }

        void VideoWindow_OnWindowCovered(OMControl sender, int screen)
        {
            OMTargetWindow window = sender as OMTargetWindow;

            // Cancel any active fullscreen mode
            window.Fullscreen = false;

            window.RedirectOutputWindow(new Rectangle(150, OM.Host.ClientFullArea.Bottom - 59, 88, 53), true, true);

            _ScreenData.SetProperty(screen, "VideoWindowMinized", true);
        }

        #region VideoWindow events

        void VideoWindow_OnWindowDisposed(OMTargetWindow sender, int screen, GameWindow window, IntPtr handle)
        {
            Players[screen].Dispose();
        }

        void VideoWindow_OnClick(OMControl sender, int screen)
        {
            if (_ScreenData.GetProperty<bool>(screen, "VideoWindowMinized"))
                OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Panel.Goto.Default.OMOpenGLtest", screen));
            else
            {
                // Togle fullscreen 
                OMTargetWindow videoWindow = sender.Parent[screen, "VideoWindow"] as OMTargetWindow;
                videoWindow.Fullscreen = !videoWindow.Fullscreen;
            }
        }

        void VideoWindow_OnWindowCreated(OMTargetWindow sender, int screen, GameWindow window, IntPtr handle)
        {

            mPlayer _mPlayer = new mPlayer();
            _mPlayer.AudioMode = MPlayerAudioModeName.DirectSound;
            _mPlayer.VideoMode = MPlayerVideoModeName.GL;
            _mPlayer.WindowHandle = handle;
            
            // Save a reference to screen number
            _mPlayer.Tag = sender; 

            // Set disc player to use with mPlayer
            _mPlayer.DiscDevice = "g:";

            if (screen == 0)
                _mPlayer.SetSyncMaster(OM.Host.ScreenCount);
            else
            {
                _mPlayer.SetSyncSlave(screen);
                _mPlayer.AudioDevice = null;
            }

            // Players audiodevice follows zone
            AudioDevice audioDevice = BuiltInComponents.Host.ZoneHandler.GetActiveZone(screen).AudioDevice;
            if (!audioDevice.IsDefault)
                _mPlayer.AudioDevice = audioDevice.Name;

            // Connect events
            _mPlayer.OnPlaybackStarted += new mPlayer.PlayerEventHandler(_mPlayer_OnPlaybackStarted);
            _mPlayer.OnPlaybackStopped += new mPlayer.PlayerEventHandler(_mPlayer_OnPlaybackStopped);
            _mPlayer.OnPlaybackPaused += new mPlayer.PlayerEventHandler(_mPlayer_OnPlaybackPaused);
            _mPlayer.OnAudioPresent += new mPlayer.PlayerEventHandler(_mPlayer_OnAudioPresent);
            _mPlayer.OnVideoPresent += new mPlayer.PlayerEventHandler(_mPlayer_OnVideoPresent);
            _mPlayer.OnMediaInfoUpdated += new mPlayer.PlayerEventHandler(_mPlayer_OnMediaInfoUpdated);
            _mPlayer.OnStartupCompleted += new mPlayer.PlayerEventHandler(_mPlayer_OnStartupCompleted);

            _mPlayer.Start();

            lock (Players)
            {
                Players.Add(screen, _mPlayer);
            }
        }

        #endregion

        #region mPlayer events

        void _mPlayer_OnStartupCompleted(mPlayer sender)
        {
            // Enable play button
            OMLabel lblInfo = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["lblInfo"] as OMLabel;
            if (lblInfo != null)
                lblInfo.Text = "Player ready!";
        }

        void _mPlayer_OnMediaInfoUpdated(mPlayer sender)
        {
            OMLabel lbl = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["lblInfo"] as OMLabel;
            if (lbl != null && sender.MediaInfo != null)
            {
                switch (sender.PlayerState)
                {
                    case mPlayer.PlayerStates.None:
                        lbl.Text = "Waiting for player...";
                        break;
                    case mPlayer.PlayerStates.AudioDetection:
                        lbl.Text = "Detecting audio devices...";
                        break;
                    case mPlayer.PlayerStates.Startup:
                        lbl.Text = "Player starting...";
                        break;
                    case mPlayer.PlayerStates.Idle:
                        lbl.Text = "Player ready!";
                        break;
                    case mPlayer.PlayerStates.Playing:
                    case mPlayer.PlayerStates.Paused:
                        {
                            switch (sender.PlaybackMode)
                            {
                                case mPlayer.PlaybackModes.PlayList:
                                case mPlayer.PlaybackModes.Normal:
                                    {
                                        if (String.IsNullOrEmpty(sender.MediaInfo.Artist))
                                            lbl.Text = string.Format("{0} - {1}:{2}:{3} / {4}:{5}:{6} ({7}%) / {8}+{9}", sender.MediaInfo.Url, sender.MediaInfo.Length.Hours, sender.MediaInfo.Length.Minutes, sender.MediaInfo.Length.Seconds, sender.MediaInfo.PlaybackPos.Hours, sender.MediaInfo.PlaybackPos.Minutes, sender.MediaInfo.PlaybackPos.Seconds, sender.MediaInfo.PlaybackPos_Percent, (sender.MediaInfo.HasAudio == true ? "Audio" : ""), (sender.MediaInfo.HasVideo == true ? "Video" : ""));
                                        else
                                            lbl.Text = string.Format("{0} - {1} [{2}/{3}]", sender.MediaInfo.Artist, sender.MediaInfo.Title, sender.MediaInfo.PlaybackPosText, sender.MediaInfo.LengthText);
                                    }
                                    break;
                                case mPlayer.PlaybackModes.CDDA:
                                    lbl.Text = string.Format("CD Track {0} of {1} [{2}/{3}]", sender.CDDAInfo.CurrentTrack, sender.CDDAInfo.TrackCount, sender.MediaInfo.PlaybackPosText, sender.MediaInfo.LengthText);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }

                //if (sender.MediaInfo.Length == TimeSpan.Zero)
                //    lbl.Text = "Player ready!";
                //else
                //    lbl.Text = string.Format("{0} - {1}:{2}:{3} / {4}:{5}:{6} ({7}%) / {8}+{9}", sender.MediaInfo.Url, sender.MediaInfo.Length.Hours, sender.MediaInfo.Length.Minutes, sender.MediaInfo.Length.Seconds, sender.MediaInfo.PlaybackPos.Hours, sender.MediaInfo.PlaybackPos.Minutes, sender.MediaInfo.PlaybackPos.Seconds, sender.MediaInfo.PlaybackPos_Percent, (sender.MediaInfo.HasAudio == true ? "Audio" : ""), (sender.MediaInfo.HasVideo == true ? "Video" : ""));
            }
        }

        void _mPlayer_OnVideoPresent(mPlayer sender)
        {
            // Show video window
            OMTargetWindow videoWindow = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["VideoWindow"] as OMTargetWindow;
            if (videoWindow != null)
                if (!videoWindow.Visible)
                    videoWindow.Visible = true;
        }

        void _mPlayer_OnAudioPresent(mPlayer sender)
        {
            if (!sender.HasVideo)
            {
                // Hide video window
                OMTargetWindow videoWindow = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["VideoWindow"] as OMTargetWindow;
                if (videoWindow != null)
                    if (videoWindow.Visible)
                        videoWindow.Visible = false;
            }
        }

        void _mPlayer_OnPlaybackPaused(mPlayer sender)
        {
            // Enable play button
            OMButton btnPlay = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd2"] as OMButton;
            if (btnPlay != null)
                btnPlay.Visible = true;
            
            // Disable pause button
            OMButton btnPause = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd3"] as OMButton;
            if (btnPause != null)
                btnPause.Visible = false;
        }

        void _mPlayer_OnPlaybackStopped(mPlayer sender)
        {
            //// Disable stop button
            //OMButton btnStop = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd1"] as OMButton;
            //if (btnStop != null)
            //    btnStop.Visible = false;

            // Enable play button
            OMButton btnPlay = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd2"] as OMButton;
            if (btnPlay != null)
                btnPlay.Visible = true;

            // Disable pause button
            OMButton btnPause = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd3"] as OMButton;
            if (btnPause != null)
                btnPause.Visible = false;

            // Toggle fullscreen (if active)
            OMTargetWindow videoWindow = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["VideoWindow"] as OMTargetWindow;
            if (videoWindow != null)
            {
                if (videoWindow.Fullscreen)
                    videoWindow.Fullscreen = false;
                videoWindow.Visible = false;
            }
        }

        void _mPlayer_OnPlaybackStarted(mPlayer sender)
        {
            //// Enable stop button
            //OMButton btnStop = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd1"] as OMButton;
            //if (btnStop != null)
            //    btnStop.Visible = true;

            // Disable play button
            OMButton btnPlay = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd2"] as OMButton;
            if (btnPlay != null)
                btnPlay.Visible = false;

            // Enable pause button
            OMButton btnPause = (sender.Tag as OMTargetWindow).Parent.getActivePanel()["btnCmd3"] as OMButton;
            if (btnPause != null)
                btnPause.Visible = true;
        }

        #endregion

        #region Buttons

        void btnCmd1_OnClick(OMControl sender, int screen)
        {
            Players[screen].Stop();
        }

        void btnCmd2_OnClick(OMControl sender, int screen)
        {
            if (Players[screen].PlayerState == mPlayer.PlayerStates.Paused)
            {
                Players[screen].Pause();
            }
            else
            {
                if (SelectedFile.ContainsKey(screen))
                    Players[screen].PlayFile(SelectedFile[screen]);
            }
        }

        void btnCmd3_OnClick(OMControl sender, int screen)
        {
            Players[screen].Pause();
        }

        void btnCmd4_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath path = new OpenMobile.helperFunctions.General.getFilePath(BuiltInComponents.Host);
            string url = path.getFile(screen, "MainMenu", "");
            if (url != null)
            {
                if (SelectedFile.ContainsKey(screen))
                    SelectedFile[screen] = url;
                else
                    SelectedFile.Add(screen, url);

                // Start playback
                Players[screen].PlayFile(SelectedFile[screen]);

                //for (int i = 0; i < OM.Host.ScreenCount; i++)
                //    Players[i].PlayFile(SelectedFile[screen]);
            }
        }

        void btnCmd5_OnClick(OMControl sender, int screen)
        {
            //Players[screen].SeekBwd(5);
            //Players[screen].SendCommand(Commands.Previous);
            Players[screen].Previous();
            //Players[screen].PlayFile(@"D:\Music\Jorn\Starfire\Jorn - Burn - Starfire.mp3");
        }

        void btnCmd6_OnClick(OMControl sender, int screen)
        {
            //Players[screen].SeekFwd(5);
            Players[screen].Next();
            //Players[screen].PlayFile(@"D:\Music\Jorn\Starfire\Jorn - Edge of the Blade - Starfire.mp3");
        }

        void btnCmd7_OnClick(OMControl sender, int screen)
        {
            //Players[screen].SetPlaybackPos(50);
            //Players[screen].PlayDVD();
            //Players[screen].PlayCD();
            Players[screen].PlayList(@"D:\Music\playlist.txt");
        }

        #endregion
    }
}
