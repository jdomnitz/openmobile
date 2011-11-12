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
        private List<OMPanel[]> panels;
        private string _DefaultPanel = "";

        /// <summary>
        /// Sets or gets the name of the default panel (this panel is returned if the requested panel is "")
        /// </summary>
        public string DefaultPanel {
            get
            {
                return _DefaultPanel;
            }
            set
            {
                if ((value == null) || (value == "") || (value == string.Empty))
                    throw new Exception("Default panel name can not be set to null, empty or \"\"");
                _DefaultPanel = value;
            }
        }

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
        /// Gets the panel for the given screen (default panel)
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">IndexOutOfRangeException</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
        public OMPanel this[int screen]
        {
            get
            {
                // Try to get default panel
                OMPanel panel = this[screen, ""];

                // No default panel?
                if (panel == null)
                {   // Try to load panel by old style method (by number)
                    lock (this)
                    {
                        if (panels.Count == 0)
                            return null;
                        if ((screen < 0) || (screen >= screens))
                            throw new IndexOutOfRangeException();
                        if (panels[0] == null)
                        {
                            Thread.Sleep(300);
                            if (panels[0] == null)
                                return null;
                        }

                        // Try to find first panel with data for requested screen
                        for (int i = 0; i < panels[0].Length; i++)
                        {
                            if (panels[i][screen] != null)
                                return panels[i][screen];
                        }
                        return panels[0][screen];
                    }
                }

                return panel;
            }
        }
        /// <summary>
        /// Gets the requested panel for the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public OMPanel this[int screen, string name]
        {
            get
            {
                lock (this)
                {
                    if ((screen < 0) || (screen >= screens))
                        throw new IndexOutOfRangeException();

                    // Load default panel?
                    if (name == "")
                        name = DefaultPanel;

                    OMPanel[] p = panels.Find(x => ((x[screen] != null) && (x[screen].Name == name)));
                    if (p == null)
                        return null;
                    else
                        return p[screen];
                }
            }
        }

        /// <summary>
        /// Sets the default panel
        /// </summary>
        /// <param name="panel"></param>
        public void SetDefaultPanel(OMPanel panel)
        {
            DefaultPanel = panel.Name;
        }


        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadPanel(OMPanel source, bool Default)
        {
            if (Default)
                SetDefaultPanel(source);
            loadPanel(source);
        }
        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        public void loadPanel(OMPanel source)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;

                OMPanel[] collection = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                    if (i == screens - 1)
                        collection[i] = source;
                    else
                        collection[i] = source.Clone();
                panels.Add(collection);
            }
        }
        /// <summary>
        /// Load a panel array containing screen specific versions of a panel
        /// <para>Note: All panels must have the same name</para>
        /// </summary>
        /// <param name="source"></param>
        public void loadPanel(OMPanel[] source)
        {
            if (source == null)
                return;
            if (source.Length != screens)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                foreach (OMPanel p in source)
                    p.Manager = this;

                panels.Add(source);
            }
        }

        /// <summary>
        /// Loads a panel thats shared between all screens instead of being screen-independent
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadSharedPanel(OMPanel source, bool Default)
        {
            if (Default)
                SetDefaultPanel(source);
            loadSharedPanel(source);
        }
        /// <summary>
        /// Loads a panel thats shared between all screens instead of being screen-independent
        /// </summary>
        /// <param name="source"></param>
        public void loadSharedPanel(OMPanel source)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;

                OMPanel[] collection = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                    collection[i] = source;
                panels.Add(collection);
            }
        }


        /// <summary>
        /// Loads a panel that belongs to a specific screen
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadSinglePanel(OMPanel source, int screen, bool Default)
        {
            if (Default)
                SetDefaultPanel(source);
            loadSinglePanel(source, screen);
        }
        /// <summary>
        /// Loads a panel that belongs to a specific screen
        /// </summary>
        /// <param name="source"></param>
        /// <param name="screen"></param>
        public void loadSinglePanel(OMPanel source, int screen)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;

                OMPanel[] collection = new OMPanel[screens];
                if ((screen < 0) || (screen >= screens))
                    return;
                collection[screen] = source;
                panels.Add(collection);
            }
        }

        /// <summary>
        /// Unloads a panel for duplication
        /// </summary>
        /// <param name="name">Panel name</param>
        public void unloadPanel(string name)
        {
            // Reset default panel
            if (name == DefaultPanel)
                _DefaultPanel = "";
            
            unloadPanel(name, 0);
        }
        /// <summary>
        /// Unloads a panel only from a specific screen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screen"></param>
        public void unloadPanel(string name, int screen)
        {
            // Reset default panel
            if (name == DefaultPanel)
                _DefaultPanel = "";

            lock (this)
            {
                OMPanel[] p = panels.Find(x => x[screen].Name == name);
                if (p == null)
                    return;
                else
                {
                    // Remove this screenmanager as manager for the new panel
                    foreach (OMPanel panel in p)
                        panel.Manager = null;

                    // Unload panel from cache
                    panels.Remove(p);
                }
            }
        }
        #region IDisposable Members

        /// <summary>
        /// Cleanup and dispose of active resources
        /// </summary>
        public void Dispose()
        {
            //
        }

        #endregion
    }
}
