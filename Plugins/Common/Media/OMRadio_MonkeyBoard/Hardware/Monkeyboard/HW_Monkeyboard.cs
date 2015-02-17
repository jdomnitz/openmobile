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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OMRadio_MonkeyBoard.Hardware.Monkeyboard
{
    internal class HW_Monkeyboard
    {
        #region DLL Imports

        #region Enums

        public enum MotMode : sbyte
        {
            SlideShow = 0,
            EPG = 1
        }

        public enum RADIO_TUNE_BAND : sbyte
        {
            DAB = 0,
            FM = 1
        }

        public enum DABNameMode : sbyte
        {
            Short = 0,
            Long = 1
        }

        public enum PlayStatus : sbyte
        {
            Playing = 0,
            Searching = 1,
            Tuning = 2,
            Stop = 3,
            Sorting = 4,
            Reconfiguring = 5
        }

        public enum StereoMode : sbyte
        {
            Mono = 0,
            Stereo = 1
        }

        public enum StereoStatus : sbyte
        {
            /// <summary>
            /// Stereo
            /// </summary>
            Stereo = 0,

            /// <summary>
            /// Joint stereo
            /// </summary>
            JointStereo = 1,

            /// <summary>
            /// Dual channel (full stereo)
            /// </summary>
            DualChannel = 2,

            /// <summary>
            /// Single channel (Mono)
            /// </summary>
            SingleChannel = 3
        }

        public enum ProgramType : sbyte
        {
            NotAvailable = 0,
            News = 1,
            CurrentAffairs = 2,
            Information = 3,
            Sport = 4,
            Education = 5,
            Drama = 6,
            Arts = 7,
            Science = 8,
            Talk = 9,
            PopMusic = 10,
            RockMusic = 11,
            EasyListening = 12,
            LightClassical = 13,
            ClassicalMusic = 14,
            OtherMusic = 15,
            Weather = 16,
            Finance = 17,
            Children = 18,
            Factual = 19,
            Religion = 20,
            PhoneIn = 21,
            Travel = 22,
            Leisure = 23,
            JazzAndBlues = 24,
            CountryMusic = 25,
            NationalMusic = 26,
            OldiesMusic = 27,
            FolkMusic = 28,
            Documentary = 29,
            Undefined1 = 30,
            Undefined2 = 31
        }

        #endregion

        // Clear the DAB programs stored in the module's database
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ClearDatabase();

        //Deprecated?
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 CommVersion();

        // Open the COM port of the radio and set mute behavior.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OpenRadioPort(string com_port, bool usehardmute);

        // Hard reset the radio module by pulling the RESET pin LOW
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HardResetRadio();

        // Close the COM port of the radio
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CloseRadioPort();

        // Play radio stream in FM or DAB
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PlayStream(RADIO_TUNE_BAND mode, Int32 channel);

        // Stop currently played FM or DAB stream
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StopStream();

        /// <summary>
        /// Set the volume of the radio in steps of 0 - 16
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetVolume(sbyte volume);
        
        /// <summary>
        /// Sets the volume in range 0 - 100%
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static bool SetVolumePercent(int volume)
        {
            return SetVolume((sbyte)(16f * (volume / 100f)));
        }

        // Add one volume step to the current volume.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte VolumePlus();

        // Minus one volume step from the current volume
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte VolumeMinus();

        // Mute the volume
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VolumeMute();

        /// <summary>
        /// Get the current volume in steps of 0 - 16
        /// </summary>
        /// <returns></returns>
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte GetVolume();

        /// <summary>
        /// Get the current volume in range 0 - 100%
        /// </summary>
        /// <returns></returns>
        public static int GetVolumePercent()
        {
            var vol = GetVolume();
            return (int)(((float)vol / 16f) * 100f);
        }

        // Determine if the current mode is DAB or FM
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RADIO_TUNE_BAND GetPlayMode();

        // Forward to the next available stream in the current mode. When radio is in DAB mode, the dab index will be incremented and then played.
        // When the radio is in FM mode, search by increasing the FM frequency until a channel is found
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NextStream();

        // Backward to the previous available stream in the current mode. When radio is in DAB mode, the dab index will be decremented and then played.
        // When the radio is in FM mode, search by decreasing the FM frequency until a channel is found.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PrevStream();

        // Get the signal strengh of the current playing stream. bit error can't be utilized at this point in time
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte GetSignalStrength(ref int biterror);

        // Get the current playing program type to be used to identify the genre
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ProgramType GetProgramType(RADIO_TUNE_BAND mode, Int32 dabIndex);

        // Get the RDS text of the current stream
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern sbyte GetProgramText(string strtextBuffer);

        // Get the current DAB data rate
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int16 GetDataRate();

        // Get the ensemble name of the current program
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetEnsembleName(Int32 dabIndex, DABNameMode namemode, string programName);

        // Get the preset DAB index or preset FM frequency. The module is able to store 10 DAB and 10 FM preset
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern Int32 GetPreset(sbyte mode, sbyte presetindex);

        // Store program into preset location.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool SetPreset(sbyte mode, sbyte presetindex, Int32 channel);

        // Get the stereo reception status of the current playing stream
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern StereoStatus GetStereo();

        // Get the index of current playing DAB stream or the current playing frequency
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetPlayIndex();

        // Get the current DAB frequency index in while DAB is auto searching
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte GetFrequency();

        // Determine if the current radio status is playing, searching, tuning, stop sorting or reconfiguring
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern PlayStatus GetPlayStatus();

        // Set radio to forced mono or auto detect stereo mode.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetStereoMode(StereoMode Mode);

        // Get the current stereo mode in the radio configuration
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern StereoMode GetStereoMode();

        // Auto search DAB channels. Currently stored DAB channels will be cleared
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool DABAutoSearch(byte startindex, byte endindex);

        // Auto search DAB channels. Currently stored DAB channels will NOT be cleared
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool DABAutoSearchNoClear(byte startindex, byte endindex);

        // Get the total number of DAB programs stored in the module
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetTotalProgram();

        // Check if the module is ready to receive command.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsSysReady();

        // Get the name of the current program
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetProgramName(RADIO_TUNE_BAND mode, Int32 dabIndex, DABNameMode namemode, String programName);

        // Get the name of the current program
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetProgramInfo(Int32 dabIndex, ref byte serviceComponentID, ref Int32 serviceID, ref Int16 ensembleID);

        // Get the sampling rate
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetSamplingRate();


        #endregion

        #region 2nd Generation (Pro boards) DLL Imports

        #region Enums

        public enum ApplicationType : long
        {
            MOT_SlideShow = 0,
            MOT_BWS = 1,
            TPEG = 2,
            DGPS = 3,
            TMC = 4,
            EPG = 5,
            DAB_Java = 6,
            DMB = 7,
            Push_Radio = 8
        }

        public enum BBEStatus : sbyte
        {
            BBE_EQ_Off = 0,
            BBE_On = 1,
            EQ_On = 2
        }

        #endregion

        //Get the type of MOT application of the specified DAB channel.
        //Return Values: 0 is MOT SlideShow, 1 is MOT BWS, 2 is TPEG, 3 is DGPS, 4 is TMC, 5 is EPG, 6 is DAB Java, 7 is DMB, 8 is Push Radio.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ApplicationType GetApplicationType(Int32 channel);

        //Get the filename of the SlideShow image.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetImage(string ImageFileName);

        //Query radio module for MOT data
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MotQuery();

        //Reset the MOT segment buffer
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MotReset(MotMode mode);

        //Deprecated function as of December 2012
        //Get the signal quality of the current DAB channel.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte GetDABSignalQuality();

        //Set BBE HD Sound or Preset EQ
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetBBEEQ(BBEStatus BBEOn, sbyte EQMode, sbyte BBELo, sbyte BBEHi, sbyte BBECFreq, sbyte BBEMachFreq, sbyte BBEMachGain, sbyte BBEMachQ, sbyte BBESurr, sbyte BBEMp, sbyte BBEHpF, sbyte BBEHiMode);

        //Get parameters of BBE HD Sound or Mode of EQ.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetBBEEQ(ref BBEStatus BBEOn, ref sbyte EQMode, ref sbyte BBELo, ref sbyte BBEHi, ref sbyte BBECFreq, ref sbyte BBEMachFreq, ref sbyte BBEMachGain, ref sbyte BBEMachQ, ref sbyte BBESurr, ref sbyte BBEMp, ref sbyte BBEHpF, ref sbyte BBEHiMode);

        //Set audio headroom.
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetHeadroom(sbyte headroom);

        //Get the headroom volume
        [DllImport("keystonecomm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte GetHeadroom();

        #endregion

    }
}
