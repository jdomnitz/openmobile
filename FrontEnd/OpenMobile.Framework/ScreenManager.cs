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
using OpenMobile.Controls;
using System.Threading;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Manages panel cloning and memory management for multiple screens
    /// </summary>
    public sealed class ScreenManager : IDisposable
    {
        private int screens;
        List<OMPanel[]> panels;
        /// <summary>
        /// Create a new screen manager
        /// </summary>
        /// <param name="numberOfScreens"></param>
        public ScreenManager(int numberOfScreens)
        {
            screens = numberOfScreens;
            panels = new List<OMPanel[]>();
        }
        /// <summary>
        /// Gets the panel for the given screen
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException</exception>
        /// <exception cref="PanelNotAvailableForThisScreenException">PanelNotAvailableForThisScreenException</exception>
        public OMPanel this[int index]
        {
            get
            {
                lock (this)
                {
                    if ((index < 0) || (index >= screens)||(panels.Count==0))
                        throw new ArgumentOutOfRangeException();
                    if (panels[0] == null)
                    {
                        Thread.Sleep(300);
                        if (panels[0] == null)
                            throw new PanelNotAvailableForThisScreenException("Source Panel has not been defined!  Use load panel before trying to access panels!");
                    }
                    return panels[0][index];
                }
            }
        }
        /// <summary>
        /// Gets the requested panel for the given screen
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public OMPanel this[int index, string name]
        {
            get
            {
                lock (this)
                {
                    OMPanel[] p = panels.Find(x => x[0].Name == name);
                    if (p == null)
                        return null;
                    else
                        return p[index];
                }
            }
        }


        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        public void loadPanel(OMPanel source)
        {//ToDo Re-add smart instance management
            lock (this)
            {
                OMPanel[] collection = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                    collection[i] = source.Clone();
                panels.Add(collection);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Cleanup and dispose of active resources
        /// </summary>
        public void Dispose()
        {
            if (panels != null)
                panels.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
