using System;
using OpenMobile.Graphics;
using System.Runtime.InteropServices;
#if LINUX
namespace OpenMobile.Framework
{
	internal sealed class GnomeIcon
	{
		const string thumbPath="thumbnail::path";
        static bool gnomeSupported;
        static GnomeIcon()
        {
            int tmp = 0;
            string[] tmp2 = new string[0];
            try
            {
                gtk_init(ref tmp, ref tmp2);
                gnomeSupported = true;
            }
            catch (DllNotFoundException)
            {
                gnomeSupported = false;
            }
        }
		internal static OImage GetFileIcon(string path)
		{
			IntPtr ret;
            if (!gnomeSupported)
                return null;
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
			IntPtr icon=g_file_info_get_attribute_byte_string(info,thumbPath);
			if(icon!=IntPtr.Zero)
			{
				string iconPath=Marshal.PtrToStringAnsi(icon);
				OImage img= OImage.FromFile(iconPath);
				return img;
			}
			return null;
		}
        static IntPtr StringToFilenamePtr(string str)
        {
            if (str == null)
                return IntPtr.Zero;
            IntPtr dummy, error;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            IntPtr utf8 = g_malloc(new UIntPtr((ulong)bytes.Length + 1));
            Marshal.Copy(bytes, 0, utf8, bytes.Length);
            Marshal.WriteByte(utf8, bytes.Length, 0);
            IntPtr result = g_filename_from_utf8(utf8, -1, IntPtr.Zero, out dummy, out error);
            g_free(utf8);
            if (error != IntPtr.Zero)
                return IntPtr.Zero;
            return result;
        }
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_info_get_attribute_byte_string(IntPtr info,string attribute);
		
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_query_info(IntPtr file,string attribute,int flags,IntPtr cancel,out IntPtr err);
			
		[DllImport("libgio-2.0")]
		private static extern IntPtr g_file_new_for_path(IntPtr path);
		
		[DllImport("libgtk-x11-2.0",EntryPoint="gtk_init")]
		private static extern void gtk_init(ref int argcount,ref string[] args);
		
		[DllImport("libglib-2.0")]
		static extern IntPtr g_filename_from_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		[DllImport("libglib-2.0")]
		static extern void g_free (IntPtr mem);
		
		[DllImport("libglib-2.0")]
		static extern IntPtr g_malloc(UIntPtr size);
	}
}
#endif
