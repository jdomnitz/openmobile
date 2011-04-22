using System;
using System.Runtime.InteropServices;

    public class ASound
    {
        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_card_next(ref Int32 card);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_open(out UIntPtr mixer, UInt32 mode);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Int32 snd_mixer_attach(UIntPtr mixer, String name);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Int32 snd_mixer_detach(UIntPtr mixer, String name);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_close(UIntPtr mixer);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_register(UIntPtr mixer, UIntPtr options, UIntPtr classp);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_load(UIntPtr mixer);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr snd_mixer_first_elem(UIntPtr mixer);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr snd_mixer_elem_next(UIntPtr elem);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_get_playback_volume_range(UIntPtr elem, out IntPtr min, out IntPtr max);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_set_playback_volume_all(UIntPtr mixer, IntPtr newValue);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_get_playback_volume(UIntPtr mixer, Int32 channel, out IntPtr val);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_set_playback_switch_all(UIntPtr mixer, Int32 newValue);

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 snd_mixer_selem_get_playback_switch(UIntPtr mixer, Int32 channel, out Int32 val);

        public static String snd_mixer_selem_get_name(UIntPtr elem)
        {
            IntPtr ret = _snd_mixer_selem_get_name(elem);
            return Marshal.PtrToStringAnsi(ret);
        }

        [DllImport("libasound.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint="snd_mixer_selem_get_name")]
        private static extern IntPtr _snd_mixer_selem_get_name(UIntPtr elem);
    }

