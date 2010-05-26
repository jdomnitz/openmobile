
using System;
using OpenMobile.Plugin;
using OpenMobile;
using Gst.BasePlugins;
using Gst;
using GLib;
using System.Diagnostics;
using Gst.Interfaces;

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
				return (player.SetState(State.Paused)==StateChangeReturn.Success);
			else
				return (player.SetState(State.Playing)==StateChangeReturn.Success);
		}


		public bool pause (int instance)
		{
			if (player!=null)
				return (player.SetState(State.Paused)==StateChangeReturn.Success);
			return false;
		}


		public bool stop (int stop)
		{
			if (player!=null)
			{
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
			theHost.execute(eFunction.backgroundOperationStatus,url);
			loop=new MainLoop();
			if (player==null)
			{
				player = ElementFactory.Make ("playbin2", "play") as PlayBin2;
				player.Bus.AddWatch (new BusFunc (BusCb));
			}
			if (player==null)
				return false;
			player.SetState(State.Null);
			player.Uri="file://"+url;
			player.SetState(State.Playing);
			nowPlaying=OpenMobile.Media.TagReader.getInfo(url);
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
			theHost=host;
			Gst.Application.Init();
			return eLoadStatus.LoadSuccessful;
		}
		
		private bool BusCb (Bus bus, Message message)
		{
			switch (message.Type) {
			case MessageType.Eos:
				stop(0);
				OnMediaEvent(eFunction.nextMedia,0,"");
				break;
			}
			return true;
		}

		public Settings loadSettings ()
		{
			throw new System.NotImplementedException ();
		}


		public bool incomingMessage (string message, string source)
		{
			throw new System.NotImplementedException ();
		}


		
		public bool incomingMessage<T> (string message, string source, ref T data)
		{
			throw new System.NotImplementedException ();
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
			if (player!=null){
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
