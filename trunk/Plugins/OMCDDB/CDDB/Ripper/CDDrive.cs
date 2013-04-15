//
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2003 Idael Cardoso. 
//
// Note: The FreeDB and Ripper classes have been modified from their original versions
// Outdated code was removed and the Track class was modified to seperate artist from title
//*******************************************************************************
using System;
using System.Runtime.InteropServices;

namespace Ripper
{
  public class CDBufferFiller
  {
    byte[] BufferArray;
    int WritePosition = 0;

    public CDBufferFiller(byte[] aBuffer)
    {
      BufferArray = aBuffer;
    }
    public void OnCdDataRead(object sender, DataReadEventArgs ea)
    {
      Buffer.BlockCopy(ea.Data, 0, BufferArray, WritePosition, (int)ea.DataSize);
      WritePosition += (int)ea.DataSize;
    }

  }
  
  /// <summary>
	/// 
	/// </summary>
  public class CDDrive: IDisposable
	{
    private IntPtr cdHandle;
    private bool TocValid = false;
    private Win32Functions.CDROM_TOC Toc = null;
    private char m_Drive = '\0';
    
    public CDDrive()
    {
      Toc = new Win32Functions.CDROM_TOC();
      cdHandle = IntPtr.Zero;
    }
    
    public bool Open(char Drive)
    {
      Close();
      if ( Win32Functions.GetDriveType(Drive+":\\") == Win32Functions.DriveTypes.DRIVE_CDROM )
      {
        cdHandle = Win32Functions.CreateFile("\\\\.\\"+Drive+':', Win32Functions.GENERIC_READ, Win32Functions.FILE_SHARE_READ, IntPtr.Zero, Win32Functions.OPEN_EXISTING, 0, IntPtr.Zero);
        if ( ((int)cdHandle != -1) && ((int)cdHandle != 0) )
        {
          m_Drive = Drive;
          return true;
        }          
        else
        {
          return true;
        }
      }
      else
      {
        return false;
      }
    }

    public void Close()
    {
      UnLockCD();
      if ( ((int)cdHandle != -1) && ((int)cdHandle != 0) )
      {
        Win32Functions.CloseHandle(cdHandle);
      }
      cdHandle = IntPtr.Zero;
      m_Drive = '\0';
      TocValid = false;
    }

    public bool IsOpened
    {
      get
      {
        return ((int)cdHandle != -1) && ((int)cdHandle != 0);
      }
    }

    public void Dispose()
    {
      Close();
      GC.SuppressFinalize(this);
    }

    ~CDDrive()      
    {
      Dispose();
    }

    protected bool ReadTOC()
    {
      if ( ((int)cdHandle != -1) && ((int)cdHandle != 0) )
      {
        uint BytesRead = 0; 
        TocValid = Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_CDROM_READ_TOC, IntPtr.Zero, 0, Toc, (uint)Marshal.SizeOf(Toc), ref BytesRead, IntPtr.Zero) != 0;
      }
      else
      {
        TocValid = false;
      }
      return TocValid;
    }
    protected int GetStartSector(int track)
    {
      if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) )
      {
        Win32Functions.TRACK_DATA td = Toc.TrackData[track-1];
        return (td.Address_1*60*75 + td.Address_2*75 + td.Address_3)-150;
      }
      else 
      {
        return -1; 
      }
    }
    protected int GetEndSector(int track)
    {
      if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) )
      {
        Win32Functions.TRACK_DATA td = Toc.TrackData[track];
        return (td.Address_1*60*75 + td.Address_2*75 + td.Address_3)-151;
      }
      else 
      {
        return -1;
      }
    }

    protected const int NSECTORS = 13;
    protected const int UNDERSAMPLING = 1;
    protected const int CB_CDDASECTOR = 2368;
    protected const int CB_QSUBCHANNEL = 16;
    protected const int CB_CDROMSECTOR = 2048;
    protected const int CB_AUDIO = (CB_CDDASECTOR-CB_QSUBCHANNEL);
    /// <summary>
    /// Read Audio Sectors
    /// </summary>
    /// <param name="sector">The sector where to start to read</param>
    /// <param name="Buffer">The length must be at least CB_CDDASECTOR*Sectors bytes</param>
    /// <param name="NumSectors">Number of sectors to read</param>
    /// <returns>True on success</returns>
    protected bool ReadSector(int sector, byte[] Buffer, int NumSectors) 
    {
      if ( TocValid && ((sector+NumSectors) <= GetEndSector(Toc.LastTrack)) && (Buffer.Length >= CB_AUDIO*NumSectors))
      {
        Win32Functions.RAW_READ_INFO rri = new Win32Functions.RAW_READ_INFO();
        rri.TrackMode = Win32Functions.TRACK_MODE_TYPE.CDDA;
        rri.SectorCount = (uint)NumSectors;
        rri.DiskOffset = sector*CB_CDROMSECTOR;

        uint BytesRead = 0;
        if ( Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_CDROM_RAW_READ, rri, (uint)Marshal.SizeOf(rri), Buffer, (uint)NumSectors*CB_AUDIO, ref BytesRead, IntPtr.Zero) != 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else 
      { 
        return false;
      }
    }
    /// <summary>
    /// Lock the CD drive 
    /// </summary>
    /// <returns>True on success</returns>
    public bool LockCD()
    {
      if (((int)cdHandle != -1) && ((int)cdHandle != 0))
      {
        uint Dummy = 0;
        Win32Functions.PREVENT_MEDIA_REMOVAL pmr = new Win32Functions.PREVENT_MEDIA_REMOVAL();
        pmr.PreventMediaRemoval = 1;
        return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_MEDIA_REMOVAL, pmr, (uint)Marshal.SizeOf(pmr), IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0;
      }
      else 
      {
        return false;
      }
    }
    /// <summary>
    /// Unlock CD drive
    /// </summary>
    /// <returns>True on success</returns>
    public bool UnLockCD()
    {
      if (((int)cdHandle != -1) && ((int)cdHandle != 0))
      {
        uint Dummy = 0;
        Win32Functions.PREVENT_MEDIA_REMOVAL pmr = new Win32Functions.PREVENT_MEDIA_REMOVAL();
        pmr.PreventMediaRemoval = 0;
        return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_MEDIA_REMOVAL, pmr, (uint)Marshal.SizeOf(pmr), IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0;
      }
      else 
      {
        return false;
      }
    }
    /// <summary>
    /// Close the CD drive door
    /// </summary>
    /// <returns>True on success</returns>
    public bool LoadCD()
    {
      TocValid = false;
      if (((int)cdHandle != -1) && ((int)cdHandle != 0))
      {
        uint Dummy = 0;
        return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_LOAD_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0;
      }
      else 
      {
        return false;
      }
    }
    
    /// <summary>
    /// Check if there is CD in the drive
    /// </summary>
    /// <returns>True on success</returns>
    public bool IsCDReady()
    {
      if (((int)cdHandle != -1) && ((int)cdHandle != 0))
      {
        uint Dummy = 0;
        if (Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_CHECK_VERIFY, IntPtr.Zero, 0, IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0)
        {
          return true;
        }
        else
        {
          TocValid = false;
          return false;
        }
      }
      else 
      {
        TocValid = false;
        return false;
      }
    }
    /// <summary>
    /// If there is a CD in the drive read its TOC
    /// </summary>
    /// <returns>True on success</returns>
    public bool Refresh()
    {
      if ( IsCDReady() )
      {
        return ReadTOC();
      }
      else 
      {
        return false;
      }
    }
    /// <summary>
    /// Return the number of tracks on the CD
    /// </summary>
    /// <returns>-1 on error</returns>
    public int GetNumTracks()
    {
      if ( TocValid )
      {
        return Toc.LastTrack - Toc.FirstTrack + 1;
      }
      else return -1;
    }
    /// <summary>
    /// Read the digital data of the track
    /// </summary>
    /// <param name="track">Track to read</param>
    /// <param name="Data">Buffer that will receive the data</param>
    /// <param name="DataSize">On return the size needed to read the track</param>
    /// <param name="StartSecond">First second of the track to read, 0 means to start at beginning of the track</param>
    /// <param name="Seconds2Read">Number of seconds to read, 0 means to read until the end of the track</param>
    /// <param name="OnProgress">Delegate to indicate the reading progress</param>
    /// <returns>Negative value means an error. On success returns the number of bytes read</returns>
    public int ReadTrack(int track, byte[] Data, ref uint DataSize, uint StartSecond, uint Seconds2Read, CdReadProgressEventHandler ProgressEvent)
    {
      if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) )
      {
        int StartSect = GetStartSector(track);
        int EndSect = GetEndSector(track);
        if ( (StartSect += (int)StartSecond*75) >= EndSect )
        {
          StartSect -= (int)StartSecond*75;
        }
        if ( (Seconds2Read > 0) && ( (int)(StartSect + Seconds2Read*75) < EndSect ) )
        {
          EndSect = StartSect + (int)Seconds2Read*75;
        }
        DataSize = (uint)(EndSect - StartSect)*CB_AUDIO;
        if ( Data != null)
        {
          if ( Data.Length >= DataSize )
          {
            CDBufferFiller BufferFiller = new CDBufferFiller(Data);
            return ReadTrack(track, new CdDataReadEventHandler(BufferFiller.OnCdDataRead), StartSecond, Seconds2Read, ProgressEvent);
          }
          else 
          {
            return 0;
          }
        }
        else
        {
          return 0;
        }
      }
      else
      {
        return -1;
      }
    }
    /// <summary>
    /// Read the digital data of the track
    /// </summary>
    /// <param name="track">Track to read</param>
    /// <param name="Data">Buffer that will receive the data</param>
    /// <param name="DataSize">On return the size needed to read the track</param>
    /// <param name="OnProgress">Delegate to indicate the reading progress</param>
    /// <returns>Negative value means an error. On success returns the number of bytes read</returns>
    public int ReadTrack(int track, byte[] Data, ref uint DataSize, CdReadProgressEventHandler ProgressEvent)
    {
      return ReadTrack(track, Data, ref DataSize, 0, 0, ProgressEvent);
    }
    /// <summary>
    /// Read the digital data of the track
    /// </summary>
    /// <param name="track">Track to read</param>
    /// <param name="OnDataRead">Call each time data is read</param>
    /// <param name="StartSecond">First second of the track to read, 0 means to start at beginning of the track</param>
    /// <param name="Seconds2Read">Number of seconds to read, 0 means to read until the end of the track</param>
    /// <param name="OnProgress">Delegate to indicate the reading progress</param>
    /// <returns>Negative value means an error. On success returns the number of bytes read</returns>
    public int ReadTrack(int track, CdDataReadEventHandler DataReadEvent, uint StartSecond, uint Seconds2Read, CdReadProgressEventHandler ProgressEvent)
    {
      if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) && (DataReadEvent != null) )
      {
        int StartSect = GetStartSector(track);
        int EndSect = GetEndSector(track);
        if ( (StartSect += (int)StartSecond*75) >= EndSect )
        {
          StartSect -= (int)StartSecond*75;
        }
        if ( (Seconds2Read > 0) && ( (int)(StartSect + Seconds2Read*75) < EndSect ) )
        {
          EndSect = StartSect + (int)Seconds2Read*75;
        }
        uint Bytes2Read = (uint)(EndSect - StartSect)*CB_AUDIO;
        uint BytesRead = 0;
        byte[] Data = new byte[CB_AUDIO*NSECTORS];
        bool Cont = true;
        bool ReadOk = true;
        if ( ProgressEvent != null )
        {
          ReadProgressEventArgs rpa = new ReadProgressEventArgs(Bytes2Read, 0);
          ProgressEvent(this, rpa);
          Cont = !rpa.CancelRead;
        }
        for (int sector = StartSect; (sector < EndSect) && (Cont) && (ReadOk); sector+=NSECTORS)
        {
          int Sectors2Read = ( (sector + NSECTORS) < EndSect )?NSECTORS:(EndSect-sector);
          ReadOk = ReadSector(sector, Data, Sectors2Read);
          if ( ReadOk )
          {
            DataReadEventArgs dra = new DataReadEventArgs(Data, (uint)(CB_AUDIO*Sectors2Read));
            DataReadEvent(this, dra);
            BytesRead += (uint)(CB_AUDIO*Sectors2Read);
            if ( ProgressEvent != null )
            {
              ReadProgressEventArgs rpa = new ReadProgressEventArgs(Bytes2Read, BytesRead);
              ProgressEvent(this, rpa);
              Cont = !rpa.CancelRead;
            }
          }
        }
        if ( ReadOk )
        {
          return (int)BytesRead;
        }
        else
        {
          return -1;
        }

      }
      else
      {
        return -1;
      }
    }

    public static char[] GetCDDriveLetters()
    {
      string res = "";
      for ( char c = 'C'; c <= 'Z'; c++)
      {
        if ( Win32Functions.GetDriveType(c+":") == Win32Functions.DriveTypes.DRIVE_CDROM )
        {
          res += c;
        }
      }
      return res.ToCharArray();
    }

	  #region CDDB Enhancements
	  
	  /// <summary>
	  /// Retrieve the data required to perform a CDDB Query
	  /// </summary>
	  /// <returns></returns>
	  public string GetCDDBQuery()
	  {   
          
		  int numTracks = GetNumTracks();
		  if (numTracks == -1)
			  throw new Exception("Unable to retrieve the number of tracks, Cannot calculate DiskID.");

		  string postfix  = numTracks.ToString();

		  int   i,t = 0,n=0;

		  double ofs=0;

		  int secs = 0;

		  /* For backward compatibility this algorithm must not change */
		  i = 0; 
		  
		  while (i < numTracks) 
		  {
              //Console.WriteLine("Track {0}: {1}:{2}", i, GetSeconds(i) / 60, GetSeconds(i) % 60);

			  ofs = (((Toc.TrackData[ i].Address_1 * 60)+ Toc.TrackData[ i].Address_2)*75)+ Toc.TrackData[ i].Address_3;
			  n = n + cddb_sum((Toc.TrackData[ i].Address_1 * 60) + Toc.TrackData[ i].Address_2 );
			  secs += GetSeconds(i);
			  postfix += "+" + string.Format("{0}",ofs);

			  i++;
		  }
          

		  int numSecs = Toc.TrackData[ i].Address_1 * 60 + Toc.TrackData[ i].Address_2;
          
          //Console.WriteLine("n = {0}, numSecs = {1}, secs = {2}", n, numSecs, secs);

		  postfix += "+" + numSecs;
		  Win32Functions.TRACK_DATA last    = Toc.TrackData[ numTracks ];
		  Win32Functions.TRACK_DATA first    = Toc.TrackData[ 0 ];

		  t = ((last.Address_1 * 60) + last.Address_2) -
			  ( (first.Address_1 * 60) + first.Address_2 );

		  ulong lDiscId = (((uint)n % 0xff) << 24 | (uint)t << 8 | (uint)numTracks);

		  string sDiscId = String.Format("{0:x8}",lDiscId);

		  string qs = sDiscId + "+" + postfix;

		  return qs;
	  }


	  protected int GetSeconds(int track)
	  {
		  if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) )
		  {
			  int start = (GetStartSector(track)+150) / 75;
			  int end = (GetEndSector(track)+150) / 75;

			  int begin = 2;
			  if (track > Toc.FirstTrack)
				  begin = (GetEndSector2(track -1)+150)/75;

			  //return (end - begin);

			  return end - start;


		  }
		  else 
		  {
			  return -1; 
		  }
	  }

	  private int  cddb_sum(int n)
	  {
		  int	ret;

		  //Console.WriteLine("n={0}",n);

		  /* For backward compatibility this algorithm must not change */

		  ret = 0;

		  while (n > 0) 
		  {
			  ret = ret + (n % 10);
			  n = n / 10;
		  }

		  return (ret);
	  }


	  protected int GetEndSector2(int track)
	  {
		  if ( TocValid && (track >= Toc.FirstTrack) && (track <= Toc.LastTrack) )
		  {
			  Win32Functions.TRACK_DATA td = Toc.TrackData[track];
			  return (td.Address_1*60*75 + td.Address_2*75)-151;
		  }
		  else 
		  {
			  return -1;
		  }
	  }


	  #endregion
  }
}
