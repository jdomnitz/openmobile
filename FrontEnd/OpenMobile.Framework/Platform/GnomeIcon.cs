using System;
using OpenMobile.Graphics;
using System.Runtime.InteropServices;
namespace OpenMobile.Framework
{
	public class GnomeIcon
	{
		const string thumbPath="thumbnail::path,preview::icon";
		public static OImage GetFileIcon(string path)
		{
			IntPtr ret;
			int tmp=0;
			string[] tmp2=new string[0];
			gtk_init(ref tmp,ref tmp2);
			IntPtr pth=StringToFilenamePtr(path);
			if (pth==IntPtr.Zero)
				return null;
			ret=g_file_new_for_path(pth);
			if (ret==IntPtr.Zero)
				return null;
			IntPtr error;
			IntPtr info=g_file_query_info(ret,thumbPath,0,IntPtr.Zero,out error);
			if (info==IntPtr.Zero)
				return null;
			IntPtr icon=g_file_info_get_attribute_byte_string(info,"thumbnail::path");
			if(icon!=IntPtr.Zero)
			{
				string iconPath=Marshal.PtrToStringAnsi(icon);
				OImage img= OImage.FromFile(iconPath);
				return img;
			}
			return null;
		}
		
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_info_get_attribute_byte_string(IntPtr info,string attribute);
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_query_info(IntPtr file,string attribute,int flags,IntPtr cancel,out IntPtr err);
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_info_get_icon(IntPtr obj);
			
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_new_for_path(IntPtr path);
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_themed_icon_get_names(IntPtr icon);
		
		[DllImport("libgtk-x11-2.0",EntryPoint="gtk_init")]
		private static extern void gtk_init(ref int argcount,ref string[] args);
		
		[DllImport("libglib-2.0")]
		static extern IntPtr g_filename_from_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		public static IntPtr StringToFilenamePtr (string str) 
		{
			if (str == null)
				return IntPtr.Zero;

			IntPtr dummy, error;
			IntPtr utf8 = StringToPtrGStrdup (str);
			IntPtr result = g_filename_from_utf8 (utf8, -1, IntPtr.Zero, out dummy, out error);
			g_free (utf8);
			if (error != IntPtr.Zero)
				return IntPtr.Zero;

			return result;
		}
		public static IntPtr StringToPtrGStrdup (string str) {
			if (str == null)
				return IntPtr.Zero;
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (str);
			IntPtr result = g_malloc (new UIntPtr ((ulong)bytes.Length + 1));
			Marshal.Copy (bytes, 0, result, bytes.Length);
			Marshal.WriteByte (result, bytes.Length, 0);
			return result;
		}

		[DllImport("libglib-2.0")]
		static extern void g_free (IntPtr mem);
		
		[DllImport("libglib-2.0")]
		static extern IntPtr g_malloc(UIntPtr size);
	}
}

