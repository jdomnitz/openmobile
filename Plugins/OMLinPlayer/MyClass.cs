using GLib;
using Gst;
using Gst.BasePlugins;
using Gst.Interfaces;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Media;
namespace OMLinPlayer
{
	public class MyClass : IAVPlayer
	{
		#region IAVPlayer implementation
		public bool play (int instance)
		{
			if (player==null)
				return false;
			State currentState;
			player.GetState(out currentState,500);
			if (currentState==State.Playing)
				pause(instance);
			else
				if (player.SetState(State.Playing)==StateChangeReturn.Success)
				{
					OnMediaEvent(eFunction.Play, instance, "");
					return true;
				}
			return false;
		}


		public bool pause (int instance)
		{
			if (player!=null)
				if (player.SetState(State.Paused)==StateChangeReturn.Success)
				{
					OnMediaEvent(eFunction.Pause, instance, "");
					return true;
				}
			return false;
		}


		public bool stop (int stop)
		{
			if (player!=null)
			{
				if (loop!=null)
					loop.Quit();
				if (player.SetState(State.Null)==StateChangeReturn.Success)
				{
					nowPlaying = new mediaInfo();
            		OnMediaEvent(eFunction.Stop, 0, "");
					return true;
				}
			}
			return false;
		}


		public bool setPosition (int instance, float seconds)
		{
			if (player==null)
				return false;
			player.Seek(Format.Time,SeekFlags.None,(long)(1000000000*seconds));
			return true;
		}


		public bool setPlaybackSpeed (int instance, float speed)
		{
			if (player==null)
				return false;
			return false; //ToDo
		}

		MainLoop loop;
		public bool play (int instance, string url, OpenMobile.eMediaType type)
		{
			stop(instance);
			loop=new MainLoop();
			if (player==null)
			{
				player = ElementFactory.Make ("playbin2", "play") as PlayBin2;
				player.Bus.AddWatch (new BusFunc (BusCb));
			}
			if (player==null)
				return false;
			player.SetState(State.Ready);
			player.Uri="file://"+url;
			player.SetState(State.Playing);
			nowPlaying=TagReader.getInfo(url);
			if (nowPlaying == null)
                nowPlaying = new mediaInfo(url);
            nowPlaying.Type = eMediaType.Local;
            if (nowPlaying.coverArt == null)
                nowPlaying.coverArt = TagReader.getCoverFromDB(nowPlaying.Artist, nowPlaying.Album, theHost);
            if (nowPlaying.coverArt == null)
                nowPlaying.coverArt = TagReader.getFolderImage(nowPlaying.Location);
            if (nowPlaying.coverArt == null)
                nowPlaying.coverArt = TagReader.getLastFMImage(nowPlaying.Artist, nowPlaying.Album);
			OnMediaEvent(eFunction.Play,0,url);
			loop.Run();
			return true;
		}


		public float getCurrentPosition (int instance)
		{
			if (player==null)
				return -1;
			Format f=Format.Time;
			long pos=0;
			player.QueryPosition(ref f,out pos);
			return (float)(pos/1000000000.0);
		}

		public OpenMobile.ePlayerStatus getPlayerStatus (int instance)
		{
			if (player==null)
				return ePlayerStatus.Stopped;
			State currentState;
			player.GetState(out currentState,500);
			if (currentState==State.Playing)
				return ePlayerStatus.Playing;
			else if (currentState==State.Paused)
				return ePlayerStatus.Paused;
			else if (currentState==State.Ready)
				return ePlayerStatus.Ready;
			else
				return ePlayerStatus.Unknown;
		}


		public float getPlaybackSpeed (int instance)
		{
			return 1F;
		}

		#endregion

		public MyClass ()
		{
		}
		#region IBasePlugin implementation
		private PlayBin2 player;
		IPluginHost theHost;
		public eLoadStatus initialize(IPluginHost host)
		{
			Gst.Application.Init();
			theHost=host;
			return eLoadStatus.LoadSuccessful;
		}
		
		private bool BusCb (Bus bus, Message message)
		{
			switch (message.Type)
			{
				case MessageType.Eos:
					stop(0);
					OnMediaEvent(eFunction.nextMedia,0,"");
					break;
			}
			return true;
		}

		public Settings loadSettings ()
		{
			return null;
		}


		public bool incomingMessage (string message, string source)
		{
			return false;
		}


		
		public bool incomingMessage<T> (string message, string source, ref T data)
		{
			return false;
		}


		public string authorName {
			get {
				return "Justin Domnitz";
			}
		}


		public string authorEmail {
			get {
				return "jdomnitz@users.sourceforge.net";
			}
		}


		public string pluginName {
			get {
				return "OMLinPlayer";
			}
		}


		public float pluginVersion {
			get {
				return 0.1F;
			}
		}


		public string pluginDescription {
			get { return "OMPlayer for Linux"; }
		}

		#endregion
		#region IDisposable implementation
		public void Dispose ()
		{
			if (loop!=null)
				if (loop.IsRunning)
					loop.Quit();
			if (player!=null)
			{
				player.SetState(State.Null);
				player.Dispose();
			}
		}

		#endregion
		#region IPlayer implementation
		public event MediaEvent OnMediaEvent;

		public bool setVolume (int instance, int percent)
		{
			if (player!=null)
			{
				player.SetVolume(StreamVolumeFormat.Linear,percent);
				return true;
			}
			return false;
		}


		public int getVolume (int instance)
		{
			if (player==null)
				return 0;
			return (int)player.Volume;
		}

		mediaInfo nowPlaying=new mediaInfo();
		public mediaInfo getMediaInfo (int instance)
		{
			return nowPlaying;
		}


		public bool SetVideoVisible (int instance, bool visible)
		{
			return false;
		}


		public string[] OutputDevices {
			get 
			{
				return new string[] { "Default Device" };
			}
		}


		public bool SupportsAdvancedFeatures {
			get { return false; }
		}
		
		#endregion
	}
}
