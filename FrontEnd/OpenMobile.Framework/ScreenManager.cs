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
using System.Runtime.Serialization;
using OpenMobile.Plugin;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Manages panel cloning and memory management for multiple screens
    /// </summary>
    [Serializable]
    public sealed class ScreenManager : IDisposable
    {
        public delegate OMPanel PanelInitializeMethod(); 

        private int screens;
        private List<OMPanel[]> panels;
        private string _DefaultPanelName = "";
        private Dictionary<string, PanelInitializeMethod> _QueuedPanels = new Dictionary<string, PanelInitializeMethod>();

        /// <summary>
        /// Sets or gets the name of the default panel (this panel is returned if the requested panel is "")
        /// </summary>
        public string DefaultPanelName
        {
            get
            {
                return _DefaultPanelName;
            }
            private set
            {
                _DefaultPanelName = value;
            }
        }

        /// <summary>
        /// The default panel for this screen manager
        /// </summary>
        public OMPanel[] DefaultPanel
        {
            get
            {
                return this._DefaultPanel;
            }
            private set
            {
                if (this._DefaultPanel != value)
                {
                    this._DefaultPanel = value;
                }
            }
        }
        private OMPanel[] _DefaultPanel;        

        /// <summary>
        /// The ownerplugin of this object
        /// </summary>
        public IBasePlugin OwnerPlugin
        {
            get
            {
                return this._OwnerPlugin;
            }
            set
            {
                if (this._OwnerPlugin != value)
                {
                    this._OwnerPlugin = value;
                }
            }
        }
        private IBasePlugin _OwnerPlugin;        

        /// <summary>
        /// Create a new screen manager
        /// </summary>
        /// <param name="ownerPlugin"></param>
        public ScreenManager(IBasePlugin ownerPlugin)
        {
            screens = BuiltInComponents.Host.ScreenCount;
            panels = new List<OMPanel[]>();
            _OwnerPlugin = ownerPlugin;
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
                    // Load any default panels
                    if (!String.IsNullOrEmpty(_QueuedDefaultPanel))
                    {
                        // Check for queued panel
                        QueuedPanel_Initialize(screen, _QueuedDefaultPanel);
                        _DefaultPanelName = _QueuedDefaultPanel;
                        _QueuedDefaultPanel = String.Empty;
                    }

                    // Exit if no panel is available 
                    if (panels.Count == 0 && _QueuedPanels.Count == 0)
                        return null;

                    if ((screen < 0) || (screen >= screens))
                        throw new IndexOutOfRangeException();

                    // Load default panel?
                    if (name == "")
                    {
                        // Try to get default panel based on name
                        if (!String.IsNullOrEmpty(_DefaultPanelName))
                            return panels.Find(x => ((x[screen] != null) && (x[screen].Name == _DefaultPanelName)))[screen];

                        // Try to get default panel based on first item loaded
                        if (_DefaultPanel != null && _DefaultPanel[screen] == null)
                            return panels.Find(x => (x[screen] != null))[screen];

                        // Try to get default panel based on object
                        if (_DefaultPanel != null && _DefaultPanel.Length >= screen)
                            return _DefaultPanel[screen];

                        // Return first hit with an empty name
                        if (_DefaultPanelName != null)
                            return panels.Find(x => ((x[screen] != null) && (x[screen].Name == _DefaultPanelName)))[screen];

                        return null;
                    }

                    OMPanel[] p = panels.Find(x => ((x[screen] != null) && (x[screen].Name == name)));
                    if (p == null)
                    {
                        // Check for queued panel
                        if (QueuedPanel_Initialize(screen, name))
                            return this[screen, name];

                        return null;
                    }
                    else
                        return p[screen];
                }
            }
        }

        private bool QueuedPanel_Initialize(int screen, string panelName)
        {
            if (!_QueuedPanels.ContainsKey(panelName))
                return false;

            // Inform user
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner( InfoBanner.Styles.AnimatedBanner, "Please wait, loading panel"));

            OMPanel p = null;
            // Initialize panel
            try
            {
                p = _QueuedPanels[panelName]();
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg(String.Format("ScreenManager: Exception while loading queued panel {0}", panelName), e);
                OM.Host.UIHandler.InfoBanner_Hide(screen);
                return false;
            }

            if (p == null)
            {
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "ScreenManager", String.Format("Failed to load queued panel {0}", panelName));
                OM.Host.UIHandler.InfoBanner_Hide(screen);
                return false;
            }

            // Save initialize panel to cache
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                p.Manager = this;

                OMPanel[] panelsToAdd = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                {
                    panelsToAdd[i] = p.Clone(i);
                    panelsToAdd[i].Manager = this;
                    panelsToAdd[i].ActiveScreen = i;
                    panelsToAdd[i].OwnerPlugin = _OwnerPlugin;
                }

                // Save panel
                this.panels.Add(panelsToAdd);
            }

            OM.Host.UIHandler.InfoBanner_Hide(screen);
            return true;
        }

        /// <summary>
        /// Is the specified panel loaded and available from this screenmanager
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsPanelLoaded(int screen, string name)
        {
            return panels.Find(x => ((x[screen] != null) && (x[screen].Name == name))) != null;
        }

        /// <summary>
        /// Sets the default panel
        /// </summary>
        /// <param name="panels"></param>
        public void SetDefaultPanel(OMPanel[] panels)
        {
            _DefaultPanelName = "";
            foreach (OMPanel panel in panels)
                if (panel != null && panel.Name != null)
                    _DefaultPanelName = panel.Name;
            _DefaultPanel = panels;

            for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
            {
                // Create commands for goto default panels
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", screen), "Panel.Goto.Default", OwnerPlugin.pluginName, CommandExecutor, 0, false, "Unloads all other panels and changes to the default panel for this plugin"));
            }
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = true;

            // Goto default panel
            if (command.NameLevel2 == "Panel.Goto.Default")
                BuiltInComponents.Host.execute(eFunction.GotoPanel, helperFunctions.DataHelpers.GetScreenFromString(command.NameLevel1), command.NameLevel3);

            // Goto default panel
            if (command.NameLevel2 == "Panel.Goto")
                BuiltInComponents.Host.execute(eFunction.GotoPanel, helperFunctions.DataHelpers.GetScreenFromString(command.NameLevel1), command.NameLevel3.Replace(".", ";"));

            else
            {   // Default action
                result = false;
            }
            return null;
        }

        private string _QueuedDefaultPanel = String.Empty;

        /// <summary>
        /// Loads a panel as available but does not initialize it until it's needed (saves memory)
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="initMethod"></param>
        public void QueuePanel(string panelName, PanelInitializeMethod initMethod, bool Default = false)
        {
            // Check for already existing panel
            if (_QueuedPanels.ContainsKey(panelName))
                return;

            if (Default)
                _QueuedDefaultPanel = panelName;

            // Queue this panel
            _QueuedPanels.Add(panelName, initMethod);

            // Create commands to goto a panel specified via parameters
            for (int i = 0; i < screens; i++)
            {                
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Panel.Goto", _OwnerPlugin.pluginName + (String.IsNullOrEmpty(panelName) ? "" : "." + panelName), CommandExecutor, 0, false, "Unloads all other panels and changes to the new panel"));
            }

            // Create default panel commands
            if (Default)
            {
                for (int i = 0; i < screens; i++)
                {
                    // Create commands for goto default panels
                    BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Panel.Goto.Default", _OwnerPlugin.pluginName, CommandExecutor, 0, false, "Unloads all other panels and changes to the default panel for this plugin"));
                }
            }
        }

        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadPanel(OMPanel source, bool Default)
        {
            loadPanel(source, Default, true);
        }

        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        public void loadPanel(OMPanel source)
        {
            loadPanel(source, false, true);
        }

        /// <summary>
        /// Loads a panel for duplication
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default"></param>
        /// <param name="commandGeneration"></param>
        public void loadPanel(OMPanel source, bool Default, bool? commandGeneration)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;

                OMPanel[] collection = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                    {
                        collection[i] = source.Clone(i);
                        collection[i].Manager = this;
                        collection[i].ActiveScreen = i;
                    }
                Panels_Add_Internal(collection, Default, true);
            }
        }
        /// <summary>
        /// Load a panel array containing screen specific versions of a panel
        /// <para>Note: All panels must have the same name</para>
        /// </summary>
        /// <param name="source"></param>
        public void loadPanel(OMPanel[] source)
        {
            loadPanel(source, false, true);
        }

        /// <summary>
        /// Load a panel array containing screen specific versions of a panel
        /// <para>Note: All panels must have the same name</para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="commandGeneration"></param>
        public void loadPanel(OMPanel[] source, bool Default, bool? commandGeneration)
        {
            if (source == null)
                return;
            if (source.Length != screens)
                return;
            {
                // Set this screenmanager as manager for the new panel
                for (int i = 0; i < source.Length; i++)
                    loadPanel(source[i], Default, commandGeneration);
            }
        }

        /// <summary>
        /// Loads a panel thats shared between all screens instead of being screen-independent
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadSharedPanel(OMPanel source, bool Default)
        {
            loadSharedPanel(source, Default, true);
        }

        /// <summary>
        /// Loads a panel thats shared between all screens instead of being screen-independent
        /// </summary>
        /// <param name="source"></param>
        public void loadSharedPanel(OMPanel source)
        {
            loadSharedPanel(source, false, true);
        }

        /// <summary>
        /// Loads a panel thats shared between all screens instead of being screen-independent
        /// </summary>
        /// <param name="source"></param>
        /// <param name="commandGeneration"></param>
        public void loadSharedPanel(OMPanel source, bool Default, bool? commandGeneration)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;

                OMPanel[] collection = new OMPanel[screens];
                for (int i = 0; i < screens; i++)
                {
                    collection[i] = source;
                    collection[i].ActiveScreen = i;
                }
                Panels_Add_Internal(collection, Default, true);
            }
        }

        /// <summary>
        /// Loads a panel that belongs to a specific screen
        /// </summary>
        /// <param name="source"></param>
        /// <param name="screen"></param>
        /// <param name="Default">True = Set as default panel</param>
        public void loadSinglePanel(OMPanel source, int screen, bool Default)
        {
            loadSinglePanel(source, screen, Default, true);
        }

        /// <summary>
        /// Loads a panel that belongs to a specific screen
        /// </summary>
        /// <param name="source"></param>
        /// <param name="screen"></param>
        public void loadSinglePanel(OMPanel source, int screen)
        {
            loadSinglePanel(source, screen, false, true);
        }

        /// <summary>
        /// Loads a panel that belongs to a specific screen
        /// </summary>
        /// <param name="source"></param>
        /// <param name="screen"></param>
        /// <param name="Default"></param>
        /// <param name="commandGeneration"></param>
        public void loadSinglePanel(OMPanel source, int screen, bool Default, bool? commandGeneration)
        {
            if (source == null)
                return;
            lock (this)
            {
                // Set this screenmanager as manager for the new panel
                source.Manager = this;
                source.ActiveScreen = screen;

                OMPanel[] collection = new OMPanel[screens];
                if ((screen < 0) || (screen >= screens))
                    return;
                collection[screen] = source;
                Panels_Add_Internal(collection, Default, commandGeneration);
            }
        }

        private void Panels_Add_Internal(OMPanel[] panelsToAdd, bool Default, bool? generateCommand)
        {
            if (generateCommand == null)
                generateCommand = false;

            // Map owner plugin
            foreach (OMPanel panel in panelsToAdd)
            {
                if (panel != null)
                    panel.OwnerPlugin = _OwnerPlugin;
            }

            this.panels.Add(panelsToAdd);

            if (Default)
                SetDefaultPanel(panelsToAdd);

            if ((bool)generateCommand)
            {
                for (int i = 0; i < panelsToAdd.Length; i++)
                {
                    if (panelsToAdd[i] != null)
                    {
                        // Create commands to goto a panel specified via parameters
                        BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Panel.Goto",panelsToAdd[i].OwnerPlugin.pluginName + ( String.IsNullOrEmpty(panelsToAdd[i].Name) ? "" : "." + panelsToAdd[i].Name), CommandExecutor, 0, false, "Unloads all other panels and changes to the new panel"));
                    }
                }
            }
        }

        /// <summary>
        /// Unloads a panel for duplication
        /// </summary>
        /// <param name="name">Panel name</param>
        public void unloadPanel(string name)
        {
            // Reset default panel
            if (name == DefaultPanelName)
                _DefaultPanelName = "";

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
            if (name == DefaultPanelName)
                _DefaultPanelName = "";

            lock (this)
            {
                List<OMPanel[]> PossiblePanels = panels.FindAll(x => x[screen] != null);
                OMPanel[] p = PossiblePanels.Find(x => x[screen].Name == name);
                if (p == null)
                    return;
                else
                {
                    if (p[screen] == null)
                        return;
                    // Remove this screenmanager as manager for the new panel
                    p[screen].Manager = null;

                    // Unload panel from cache
                    p[screen] = null;

                    // Remove item if all panels is set to null
                    for (int i = 0; i < p.Length; i++)
                    {
                        if (p[i] != null)
                            return;
                    }

                    // If we reach this level then we should remove the item as well
                    panels.Remove(p);
                }

                /*
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
                */
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

        /// <summary>
        /// Returns a string representing this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}({1})", base.ToString(), this.GetHashCode());
        }
    }
}
