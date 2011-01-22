#if WINDOWS
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenMobile.Graphics;
namespace OpenMobile.Framework
{
    internal static class IconExtractor
    {
        static Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
        static Guid IID_IExtractImage = new Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1");
        private static Bitmap GetThumbnailImage(string fileName, int size, int colorDepth)
        {
            Bitmap functionReturnValue = default(Bitmap);

            IShellFolder desktopFolder = default(IShellFolder);
            IShellFolder someFolder = default(IShellFolder);
            IExtractImage extract = default(IExtractImage);
            IntPtr pidl = default(IntPtr);
            IntPtr filePidl = default(IntPtr);

            //Divide the file name into a path and file name
            string folderName = System.IO.Path.GetDirectoryName(fileName);
            string shortFileName = Path.GetFileName(fileName);

            //Get the desktop IShellFolder
            SHGetDesktopFolder(ref desktopFolder);

            int tmp1 = 0;
            int tmp2 = 0;
            //Get the parent folder IShellFolder
            desktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, folderName, ref tmp1, ref pidl, ref tmp2);
            desktopFolder.BindToObject(pidl, IntPtr.Zero, ref IID_IShellFolder, ref someFolder);

            //Get the file's IExtractImage
            someFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, shortFileName, ref tmp1, ref filePidl, ref tmp2);
            try
            {
                someFolder.GetUIObjectOf(IntPtr.Zero, 1, ref filePidl, ref IID_IExtractImage, ref tmp1, ref extract);
            }
            catch (Exception)
            {
                Marshal.FreeCoTaskMem(pidl);
                Marshal.FreeCoTaskMem(filePidl);
                return null;
            }
            //Set the size
            SIZE _size = default(SIZE);
            _size.cx = size;
            _size.cy = size;

            int flags = 0x0240;
            IntPtr bmp = default(IntPtr);
            IntPtr thePath = Marshal.AllocHGlobal(1024);

            //Interop will throw an exception if one of these calls fail.
            try
            {
                extract.GetLocation(thePath, 1024, ref tmp1, ref _size, colorDepth, ref flags);
                extract.Extract(ref bmp);
            }
            catch (Exception) { }


            //Free the global memory we allocated for the path string
            Marshal.FreeHGlobal(thePath);
            //Free the pidls. The Runtime Callable Wrappers 
            //should automatically release the COM objects
            Marshal.FreeCoTaskMem(pidl);
            Marshal.FreeCoTaskMem(filePidl);

            if (!bmp.Equals(IntPtr.Zero))
            {
                functionReturnValue = Image.FromHbitmap(bmp);
            }
            else
            {
                functionReturnValue = null;
            }
            return functionReturnValue;
        }
        [ComImportAttribute(), GuidAttribute("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IExtractImage
        {
            void GetLocation(IntPtr pszPathBuffer, int cch, ref int pdwPriority, ref SIZE prgSize, int dwRecClrDepth, ref int pdwFlags);

            void Extract(ref IntPtr phBmpThumbnail);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [ComImportAttribute()]
        [GuidAttribute("000214E6-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellFolder
        {

            void ParseDisplayName(IntPtr hwndOwner, IntPtr pbcReserved,
              [MarshalAs(UnmanagedType.LPWStr)]string lpszDisplayName,
              ref int pchEaten, ref IntPtr ppidl, ref int pdwAttributes);

            void EnumObjects(IntPtr hwndOwner, uint grfFlags, ref IntPtr ppenumIDList);

            void BindToObject(IntPtr pidl, IntPtr pbcReserved, ref Guid riid,
              ref IShellFolder ppvOut);

            void BindToStorage(IntPtr pidl, IntPtr pbcReserved, ref Guid riid, IntPtr ppvObj);

            [PreserveSig()]
            int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

            void CreateViewObject(IntPtr hwndOwner, ref Guid riid,
              IntPtr ppvOut);

            void GetAttributesOf(int cidl, IntPtr apidl, ref uint rgfInOut);

            void GetUIObjectOf(IntPtr hwndOwner, int cidl, ref IntPtr apidl, ref Guid riid, ref int prgfInOut, ref IExtractImage ppvOut);

            void GetDisplayNameOf(IntPtr pidl, uint uFlags, ref IntPtr lpName);

            void SetNameOf(IntPtr hwndOwner, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]string lpszName, uint uFlags, ref IntPtr ppidlOut);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern int SHGetDesktopFolder(ref IShellFolder ppshf);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath,
                                    uint dwFileAttributes,
                                    ref SHFILEINFO psfi,
                                    uint cbSizeFileInfo,
                                    uint uFlags);
        public static OImage GetFileIcon(string path, int size)
        {
            Bitmap img = GetThumbnailImage(path, size, 24);
            if (img == null)
            {
                SHFILEINFO info = new SHFILEINFO();
                SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), 0x104);
                if (info.hIcon != IntPtr.Zero)
                    img = Bitmap.FromHicon(info.hIcon);
            }
            return new OImage(img);
        }
    }
}
#endif