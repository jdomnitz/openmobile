/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using OpenMobile;
using OpenMobile.Media;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OMPlayer
{
  public sealed class OMPlayer : IAVPlayer
  {
    IntPtr Handle;
    public event MediaEvent OnMediaEvent;
    private const int WMGraphNotify = 0x0400 + 13;
    //private const int VolumeFull = 0;
    //private const int VolumeSilence = -10000;

    private IGraphBuilder graphBuilder = null;
    private IMediaControl mediaControl = null;
    private IMediaEventEx mediaEventEx = null;
    private IVideoWindow  videoWindow = null;
    private IBasicAudio   basicAudio = null;
    private IBasicVideo   basicVideo = null;
    private IMediaSeeking mediaSeeking = null;
    private IMediaPosition mediaPosition = null;

    private mediaInfo nowPlaying = new mediaInfo();
    private bool isAudioOnly = false;
    private int currentVolume = 100;
    private ePlayerStatus currentState = ePlayerStatus.Stopped;
    private double currentPlaybackRate = 1.0;

    private IntPtr hDrain = IntPtr.Zero;

    private bool PlayMovieInWindow(string filename)
    {
      int hr = 0;

      if (filename == string.Empty)
        return false;
      lock (this)
      {
          this.graphBuilder = (IGraphBuilder)new FilterGraph();

          // Have the graph builder construct its the appropriate graph automatically
          hr = this.graphBuilder.RenderFile(filename, null);
          if (hr != 0)
              return false;
          // QueryInterface for DirectShow interfaces
          this.mediaControl = (IMediaControl)this.graphBuilder;
          this.mediaEventEx = (IMediaEventEx)this.graphBuilder;
          this.mediaSeeking = (IMediaSeeking)this.graphBuilder;
          this.mediaPosition = (IMediaPosition)this.graphBuilder;
          
          // Query for video interfaces, which may not be relevant for audio files
          this.videoWindow = this.graphBuilder as IVideoWindow;
          this.basicVideo = this.graphBuilder as IBasicVideo;

          // Query for audio interfaces, which may not be relevant for video-only files
          this.basicAudio = this.graphBuilder as IBasicAudio;
          
          // Is this an audio-only file (no video component)?
          CheckVisibility();

          // Have the graph signal event via window callbacks for performance
          //hr = this.mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
          DsError.ThrowExceptionForHR(hr);

          if (!this.isAudioOnly)
          {
              // Setup the video window
              hr = this.videoWindow.put_Owner(this.Handle);

              DsError.ThrowExceptionForHR(hr);

              hr = this.videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
              DsError.ThrowExceptionForHR(hr);
              hr = InitVideoWindow(1, 1);
              DsError.ThrowExceptionForHR(hr);
          }

          // Complete window initialization
          this.currentPlaybackRate = 1.0;

          // Run the graph to play the media file
          hr = this.mediaControl.Run();
          DsError.ThrowExceptionForHR(hr);
      }
      this.currentState=ePlayerStatus.Playing;
      return true;
    }

    private void CloseClip()
    {
      int hr = 0;

      // Stop media playback
          if (this.mediaControl != null)
          {
              try
              {
                  hr = this.mediaControl.Stop();
              }
              catch (System.AccessViolationException) { }


              // Clear global flags
              this.currentState = ePlayerStatus.Stopped;
              this.isAudioOnly = true;

              // Free DirectShow interfaces
              CloseInterfaces();

              // No current media state
              this.currentState = ePlayerStatus.Ready;
          }
    }

    private int InitVideoWindow(int nMultiplier, int nDivider)
    {
        int hr = 0;
      /*
      int lHeight, lWidth;

      if (this.basicVideo == null)
        return 0;

      // Read the default video size
      hr = this.basicVideo.GetVideoSize(out lWidth, out lHeight);
      if (hr == DsResults.E_NoInterface)
        return 0;

      // Account for requests of normal, half, or double size
      lWidth  = lWidth  * nMultiplier / nDivider;
      lHeight = lHeight * nMultiplier / nDivider;

      Application.DoEvents();
       */
      Form f = (Form)Control.FromHandle(this.Handle);
      hr = this.videoWindow.SetWindowPosition(0, 0, f.Width, f.Height);

      return hr;
    }

    private void CheckVisibility()
    {
      int hr = 0;
      OABool lVisible;

      if ((this.videoWindow == null) || (this.basicVideo == null))
      {
        // Audio-only files have no video interfaces.  This might also
        // be a file whose video component uses an unknown video codec.
        this.isAudioOnly = true;
        return;
      }
      else
      {
        // Clear the global flag
        this.isAudioOnly = false;
      }

      hr = this.videoWindow.get_Visible(out lVisible);
      if (hr < 0)
      {
        // If this is an audio-only clip, get_Visible() won't work.
        //
        // Also, if this video is encoded with an unsupported codec,
        // we won't see any video, although the audio will work if it is
        // of a supported format.
        if (hr == unchecked((int) 0x80004002)) //E_NOINTERFACE
        {
          this.isAudioOnly = true;
        }
        else
          DsError.ThrowExceptionForHR(hr);
      }
    }

    private void CloseInterfaces()
    {
        int hr = 0;

        try
        {
            // Relinquish ownership (IMPORTANT!) after hiding video window
            if (!this.isAudioOnly)
            {
                hr = this.videoWindow.put_Visible(OABool.False);
                DsError.ThrowExceptionForHR(hr);
                hr = this.videoWindow.put_Owner(IntPtr.Zero);
                DsError.ThrowExceptionForHR(hr);
            }

            if (this.mediaEventEx != null)
            {
                hr = this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                DsError.ThrowExceptionForHR(hr);
            }

            // Release and zero DirectShow interfaces
            if (this.mediaEventEx != null)
                this.mediaEventEx = null;
            if (this.mediaSeeking != null)
                this.mediaSeeking = null;
            if (this.mediaPosition != null)
                this.mediaPosition = null;
            if (this.mediaControl != null)
                this.mediaControl = null;
            if (this.basicAudio != null)
                this.basicAudio = null;
            if (this.basicVideo != null)
                this.basicVideo = null;
            if (this.videoWindow != null)
                this.videoWindow = null;

            if (this.graphBuilder != null)
                try
                {
                    lock (this)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(this.graphBuilder);
                    }
                }
                catch (Exception) { }
            this.graphBuilder = null;
        }
        catch
        {
        }
    }

    /*
     * Media Related methods
     */
    public bool SupportsAdvancedFeatures
    {
        get
        {
            return false;
        }
    }
    public bool pause(int instance)
    {
      if (this.mediaControl == null)
        return false;

      // Toggle play/pause behavior
      if((this.currentState == ePlayerStatus.Paused) || (this.currentState == ePlayerStatus.Stopped))
      {
          if (this.mediaControl.Run() >= 0)
          {
              this.currentState = ePlayerStatus.Playing;
              this.OnMediaEvent(eFunction.Play,instance,"");
          }
      }
      else
      {
          if (this.mediaControl.Pause() >= 0)
          {
              this.currentState = ePlayerStatus.Paused;
              this.OnMediaEvent(eFunction.Pause,instance,"");
          }
      }
      return true;
    }

    public bool stop(int instance)
    {
        DsLong pos = new DsLong(0);
        // Stop and reset postion to beginning
        if ((this.currentState == ePlayerStatus.Paused) || (this.currentState == ePlayerStatus.Playing))
        {
            retry:
            lock (this)
            {
                if ((this.mediaControl == null) || (this.mediaSeeking == null))
                    return false;
                try
                {
                    this.currentState = ePlayerStatus.Ready;
                    this.mediaControl.Stop();
                    this.mediaPosition.put_CurrentPosition(0);
                }
                catch (AccessViolationException) { Thread.Sleep(50); goto retry; }
                this.mediaControl = null;
                // Clear global flags
                this.isAudioOnly = true;
                this.OnMediaEvent(eFunction.Stop, instance, "");
            }
        }
        return true;
    }

    private int ToggleFullScreen()
    {
      int hr = 0;
      OABool lMode;

      // Don't bother with full-screen for audio-only files
      if ((this.isAudioOnly) || (this.videoWindow == null))
        return 0;

      // Read current state
      hr = this.videoWindow.get_FullScreenMode(out lMode);
      DsError.ThrowExceptionForHR(hr);

      if (lMode == OABool.False)
      {
        // Save current message drain
        hr = this.videoWindow.get_MessageDrain(out hDrain);
        DsError.ThrowExceptionForHR(hr);

        // Set message drain to application main window
        hr = this.videoWindow.put_MessageDrain(this.Handle);
        DsError.ThrowExceptionForHR(hr);

        // Switch to full-screen mode
        lMode = OABool.True;
        hr = this.videoWindow.put_FullScreenMode(lMode);
        DsError.ThrowExceptionForHR(hr);
      }
      else
      {
        // Switch back to windowed mode
        lMode = OABool.False;
        hr = this.videoWindow.put_FullScreenMode(lMode);
        DsError.ThrowExceptionForHR(hr);

        // Undo change of message drain
        hr = this.videoWindow.put_MessageDrain(hDrain);
        DsError.ThrowExceptionForHR(hr);

        // Reset video window
        hr = this.videoWindow.SetWindowForeground(OABool.True);
        DsError.ThrowExceptionForHR(hr);
      }

      return hr;
    }

    private void waitForStop()
    {
        FilterState s;
        if (this.mediaControl == null)
            return;
        this.mediaControl.GetState(1000,out s);
        double lastPosition=-1;
        while ((s == FilterState.Running)||(s==FilterState.Paused))
        {
            Thread.Sleep(1000);
            double d=lastPosition;
            if (this.mediaPosition != null)
                d = getCurrentPosition(0);
            if ((currentState != ePlayerStatus.Paused) && (d == lastPosition))
            {
                currentState = ePlayerStatus.Stopped;
                this.OnMediaEvent(eFunction.Stop, 0, "");//ToDo - Instance Specific
                if (this.currentState!=ePlayerStatus.Ready)
                    this.OnMediaEvent(eFunction.nextMedia, 0, ""); //ToDo - Instance Specific
                //stop(0);//ToDo - Instance Specific
                return;
            }
            else
            {
                lastPosition = d;
            }
        }
    }
    public string[] OutputDevices
    {
        get
        {
            DsDevice[] d= DsDevice.GetDevicesOfCat(FilterCategory.AMKSRender);
            string[] h = new string[d.Length];
            for (int i = 0; i < h.Length; i++)
                h[i] = d[i].Name;
            Array.Reverse(h);
            return h;
        }
    }
    private int SetRate(double rate)
    {
      int hr = 0;

      // If the IMediaPosition interface exists, use it to set rate
      if (this.mediaPosition != null)
      {
        hr = this.mediaSeeking.SetRate(rate);
        if (hr == 0)
        {
          this.currentPlaybackRate = rate;
        }
      }

      return hr;
    }

    private void HandleGraphEvent()
    {
      int hr = 0;
      EventCode evCode;
      IntPtr evParam1, evParam2;

      // Make sure that we don't access the media event interface
      // after it has already been released.
      if (this.mediaEventEx == null)
        return;

      // Process all queued events
      while(this.mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
      {
        // Free memory associated with callback, since we're not using it
        hr = this.mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

        // If this is the end of the clip, reset to beginning
        if(evCode == EventCode.Complete)
        {
          DsLong pos = new DsLong(0);
          // Reset to first frame of movie
          hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, 
            null, AMSeekingSeekingFlags.NoPositioning);
          if (hr < 0)
          {
            // Some custom filters (like the Windows CE MIDI filter)
            // may not implement seeking interfaces (IMediaSeeking)
            // to allow seeking to the start.  In that case, just stop
            // and restart for the same effect.  This should not be
            // necessary in most cases.
            hr = this.mediaControl.Stop();
            hr = this.mediaControl.Run();
          }
        }
      }
    }

    /*
     * WinForm Related methods
     */
/*
    protected override void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case WMGraphNotify :
        {
          HandleGraphEvent();
          break;
        }
      }
      */
      // Pass this message to the video window for notification of system changes
    
#region IAVPlayer Members

public bool  play(int instance)
{
        if (this.mediaControl == null)
            return false;
        if (this.mediaControl.Run() >= 0)
        {
            this.currentState = ePlayerStatus.Playing;
            OnMediaEvent(eFunction.Play, instance, "");
            return true;
        }
        return false;
}

public bool  setPosition(int instance,float seconds)
{
   return (0== mediaPosition.put_CurrentPosition((double)seconds));
}
public float getPlaybackSpeed(int instance)
{
    return (float)this.currentPlaybackRate;
}
public bool  setPlaybackSpeed(int instance,float speed)
{
    SetRate((double)speed);
    if (this.currentPlaybackRate > 1)
        currentState = ePlayerStatus.FastForwarding;
    else if (this.currentPlaybackRate < 1)
        currentState = ePlayerStatus.Rewinding;
    else
        if ((currentState == ePlayerStatus.FastForwarding) || (currentState == ePlayerStatus.Rewinding))
            currentState = ePlayerStatus.Playing;
    OnMediaEvent(eFunction.setPlaybackSpeed, instance, speed.ToString());
    return true;
}
Thread t;
public bool  play(int instance,string url, eMediaType type)
{
    try
    {
        lock (this)
        {
            if (currentState == ePlayerStatus.Playing)
                stop(instance);
            if (PlayMovieInWindow(url) == false)
                return false;
        }
        nowPlaying = TagReader.getInfo(url);
        if (nowPlaying.coverArt == null)
            nowPlaying.coverArt = TagReader.getFolderImage(nowPlaying.Location);
        OnMediaEvent(eFunction.Play, instance, url);
        if (t != null)
            t.Abort();
        t = new Thread(new ThreadStart(waitForStop));
        t.Start();
        return true;
    }
    catch (Exception) {
        return false;
    }
}

public mediaInfo getMediaInfo(int instance)
{
    return nowPlaying;
}

public float getCurrentPosition(int instance)
{
    double time;
        if (mediaPosition == null)
            return -1;
        if ((currentState == ePlayerStatus.Stopped) || (currentState == ePlayerStatus.Ready))
            return 0;
        lock (mediaPosition)
        {
            mediaPosition.get_CurrentPosition(out time);
        }
        return (float)time;
}

public ePlayerStatus getPlayerStatus(int instance)
{
    return currentState;
}

public int getVolume(int instance)
{
 	return this.currentVolume;
}

#endregion

#region IBasePlugin Members

public string  authorName
{
	get { return "Justin Domnitz"; }
}

public string  authorEmail
{
	get { return "jdomnitz@gmail.com"; }
}

public string  pluginName
{
    get { return "OMPlayer"; }
}

public float  pluginVersion
{
	get { return 0.2F; }
}

public string  pluginDescription
{
	get { return "DirectShow Media Player"; }
}

public bool  incomingMessage(string message, string source)
{
 	throw new NotImplementedException();
}
public bool incomingMessage<T>(string message, string source, ref T data)
{
    throw new NotImplementedException();
}
public eLoadStatus  initialize(IPluginHost host)
{
    Handle = host.UIHandle(0); //ToDo - Support multiple streams
    host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
    using (PluginSettings setting = new PluginSettings())
    {
        if (setting.getSetting("Music.AutoResume") == "True")
        {
            host.execute(eFunction.loadAVPlayer, "0", "OMPlayer");
            play(0, setting.getSetting("Music.LastPlayingURL"), eMediaType.Local);
            if (mediaPosition==null)
                return eLoadStatus.LoadSuccessful;
            double pos;
            if (double.TryParse(setting.getSetting("Music.LastPlayingPosition"),out pos)==true)
            {
                if (pos > 1)
                    pos -= 1;
                mediaPosition.put_CurrentPosition(pos);
            }
        }
    }
    return eLoadStatus.LoadSuccessful;
}

void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
{
    if (function == eFunction.closeProgram)
    {
        using (PluginSettings settings = new PluginSettings())
        {
            if (nowPlaying != null)
            {
                if (this.getPlayerStatus(0) == ePlayerStatus.Stopped)
                {
                    settings.setSetting("Music.LastPlayingURL", "");
                    return;
                }
                settings.setSetting("Music.LastPlayingURL", nowPlaying.Location);
                double position;
                if (mediaPosition != null)
                {
                    mediaPosition.get_CurrentPosition(out position);
                    settings.setSetting("Music.LastPlayingPosition", position.ToString());
                }
                else
                {
                    settings.setSetting("Music.LastPlayingURL", "");
                }
            }
            else
            {
                settings.setSetting("Music.LastPlayingURL", "");
            }
        }
    }

}

#endregion

#region IDisposable Members

public void  Dispose()
{
    CloseClip();
    GC.SuppressFinalize(this);
}

#endregion  
    public bool setVolume(int instance,int percent)
    {
        if ((this.graphBuilder == null) || (this.basicAudio == null))
            return false;
        if ((percent >= 0) && (percent <= 100))
            this.currentVolume = percent;
        else if (percent == -1)
            this.currentVolume = 0;
        else
            return false;
        return (this.basicAudio.put_Volume((100 - this.currentVolume) * -100) == 0);
    }
  }
}
