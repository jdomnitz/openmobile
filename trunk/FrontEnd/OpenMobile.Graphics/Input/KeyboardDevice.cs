﻿#region --- License ---
/* Copyright (c) 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion

#region --- Using directives ---

using System;

using OpenMobile.Input;
using System.Diagnostics;

#endregion

namespace OpenMobile.Input
{
    /// <summary>
    /// Represents a keyboard device and provides methods to query its status. 
    /// </summary>
    public sealed class KeyboardDevice : IInputDevice
    {
        //private IKeyboard keyboard;
        private bool[] keys = new bool[Enum.GetValues(typeof(Key)).Length];
        private string description;
        private IntPtr devID;
        private int instance=-1;
        private KeyboardKeyEventArgs args = new KeyboardKeyEventArgs();

        #region --- Constructors ---

        internal KeyboardDevice() { }

        #endregion

        #region --- IKeyboard members ---

        /// <summary>
        /// Gets a value indicating the status of the specified Key.
        /// </summary>
        /// <param name="key">The Key to check.</param>
        /// <returns>True if the Key is pressed, false otherwise.</returns>
        public bool this[Key key]
        {
            get { return keys[(int)key]; }
            internal set
            {
                    keys[(int)key] = value;

                    if (value && KeyDown != null)
                    {
                        args.Key = key;
                        args.Shift = keys[(int)Key.ShiftLeft] || keys[(int)Key.ShiftRight];
                        args.Control = keys[(int)Key.ControlLeft] || keys[(int)Key.ControlRight];
                        args.CapsLock = keys[(int)Key.CapsLock];
                        KeyDown(this, args);
                    }
                    else if (!value && KeyUp != null)
                    {
                        args.Key = key;
                        args.Shift = keys[(int)Key.ShiftLeft] || keys[(int)Key.ShiftRight];
                        args.Control = keys[(int)Key.ControlLeft] || keys[(int)Key.ControlRight];
                        args.CapsLock = keys[(int)Key.CapsLock];
                        KeyUp(this, args);
                    }
            }
        }

        /// <summary>
        /// Gets an IntPtr representing a device dependent ID.
        /// </summary>
        public IntPtr DeviceID
        {
            get { return devID; }
            internal set
			{
				devID = value;
				keys[(int)Key.CapsLock]=CapsLock;
			}
        }

        public int Instance
        {
            get { return instance; }
            internal set { instance = value; }
        }

        #region KeyDown

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;

        #endregion

        #region KeyUp

        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;

        #endregion

        #endregion

        #region --- IInputDevice Members ---

        /// <summary>
        /// Gets a <see cref="System.String"/> which describes this instance.
        /// </summary>
        public string Description
        {
            get { return description; }
            internal set { description = value; }
        }

        /// <summary>
        /// Gets the <see cref="InputDeviceType"/> for this instance.
        /// </summary>
        public InputDeviceType DeviceType
        {
            get { return InputDeviceType.Keyboard; }
        }

		public bool CapsLock
		{
			get
			{	
				#if LINUX
				if (Configuration.RunningOnLinux)
				{
					uint state;
					if (devID==IntPtr.Zero)
						Platform.X11.Functions.XkbGetIndicatorState(Platform.X11.API.DefaultDisplay,0x0100,out state);
					else
						Platform.X11.Functions.XkbGetIndicatorState(Platform.X11.API.DefaultDisplay,(uint)devID,out state);
					return (state&0x1)!=0;
				}
				#endif
                #if WINDOWS
                #if LINUX
                else 
                #endif
                    if (Configuration.RunningOnWindows)
                    {
                        if (devID == IntPtr.Zero)
                            return Console.CapsLock;
                    }
                #endif
				return false;
			}
		}
        #endregion

        #region --- Public Methods ---

        /// <summary>Returns the hash code for this KeyboardDevice.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //return base.GetHashCode();
            return (int)(devID.GetHashCode() ^ description.GetHashCode());
        }

        #endregion

        #region --- Internal Methods ---

        #region internal void ClearKeys()

        internal void ClearKeys()
        {
            for (int i = 0; i < keys.Length; i++)
                if (this[(Key)i])       // Make sure KeyUp events are *not* raised for keys that are up, even if key repeat is on.
                    this[(Key)i] = false;
        }

        #endregion

        #endregion
    }
}