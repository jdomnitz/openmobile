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

namespace Ripper
{
  public class DataReadEventArgs : EventArgs
  {
    private byte[] m_Data;
    private uint m_DataSize;
    public DataReadEventArgs(byte[] data, uint size)
    {
      m_Data = data;
      m_DataSize = size;
    }
    public byte[] Data
    {
      get 
      {
        return m_Data;
      }
    }
    public uint DataSize
    {
      get
      {
        return m_DataSize;
      }
    }
  }

  public class ReadProgressEventArgs : EventArgs
  {
    private uint m_Bytes2Read;
    private uint m_BytesRead;
    private bool m_CancelRead = false;
    public ReadProgressEventArgs(uint bytes2read, uint bytesread)
    {
      m_Bytes2Read = bytes2read;
      m_BytesRead = bytesread;
    }
    public uint Bytes2Read
    {
      get
      {
        return m_Bytes2Read;
      }
    }
    public uint BytesRead
    {
      get
      {
        return m_BytesRead;
      }
    }
    public bool CancelRead
    {
      get
      {
        return m_CancelRead;
      }
      set
      {
        m_CancelRead = value;
      }
    }
  }

  
  public delegate void CdDataReadEventHandler(object sender, DataReadEventArgs ea);
  public delegate void CdReadProgressEventHandler(object sender, ReadProgressEventArgs ea);
}