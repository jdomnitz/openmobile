#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMobile.Input
{
    /// <summary>
    /// Defines the event data for <see cref="KeyboardDevice"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone a KeyboardEventArgs instance using the 
    /// <see cref="KeyboardKeyEventArgs(KeyboardKeyEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class KeyboardKeyEventArgs : EventArgs
    {
        #region Fields

        Key key;
        bool shift;
        bool control;
        bool caps;
        int screen=-1;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new KeyboardEventArgs instance.
        /// </summary>
        public KeyboardKeyEventArgs() { }

        /// <summary>
        /// Constructs a new KeyboardEventArgs instance.
        /// </summary>
        /// <param name="args">An existing KeyboardEventArgs instance to clone.</param>
        public KeyboardKeyEventArgs(KeyboardKeyEventArgs args)
        {
            Key = args.Key;
        }
        public KeyboardKeyEventArgs(Key existingKey)
        {
            Key = existingKey;
        }
        #endregion

        #region Public Members

        /// <summary>
        /// Gets the <see cref="Key"/> that generated this event.
        /// </summary>
        public Key Key
        {
            get { return key; }
            internal set { key = value; }
        }
        public bool Shift
        {
            get { return shift; }
            set { shift = value; }
        }
        public bool Control
        {
            get { return control; }
            set { control = value; }
        }
        public int KeyCode
        {
            get { return (int)key; }
        }
        public bool CapsLock
        {
            get { return caps; }
            set { caps = value; }
        }
        /// <summary>
        /// -1 for unknown
        /// </summary>
        public int Screen
        {
            get { return screen; }
            set { screen = value; }
        }
        #endregion
    }
}
