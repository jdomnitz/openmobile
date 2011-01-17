using System;
using System.Runtime.InteropServices;

namespace QTPlayer
{
    internal static class API
    {
        const string Library = @"C:\Program Files\QuickTime\QTSystem\QTMLClient.dll";


        [DllImport(Library)]
        internal static extern IntPtr CFURLCreateFromFileSystemRepresentation(int flags,string file,int length,bool unknown);

        [DllImport(Library)]
        internal static extern IntPtr NewMovieFromDataRef(out IntPtr movie, int flags, IntPtr unknown, IntPtr file, IntPtr subtype);
    }
}
