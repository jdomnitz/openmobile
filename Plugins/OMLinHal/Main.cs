using System;
using Gtk;

namespace OMLinHal
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			NDesk.DBus.BusG.Init();
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Opacity=0;
			//win.Show ();
			Application.Run ();
		}
	}
}

