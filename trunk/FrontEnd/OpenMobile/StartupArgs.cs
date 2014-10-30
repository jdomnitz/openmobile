using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenMobile
{
    /// <summary>
    /// Startup arguments data
    /// </summary>
    public class StartupArguments
    {
        /// <summary>
        /// The requested graphic engine (if any)
        /// </summary>
        public Graphics.eGraphicEngines RequestedGraphicEngine
        {
            get
            {
                return this._RequestedGraphicEngine;
            }
            set
            {
                if (this._RequestedGraphicEngine != value)
                {
                    this._RequestedGraphicEngine = value;
                }
            }
        }
        private Graphics.eGraphicEngines _RequestedGraphicEngine = Graphics.eGraphicEngines.Unspecified;

        /// <summary>
        /// Processes startup arguments 
        /// </summary>
        /// <param name="args"></param>
        public StartupArguments(string[] args)
        {
            foreach (string arg in args)
            {
                // Specific graphics engine
                if (arg.ToLower().StartsWith("-graphics="))
                {
                    // Remove the identifier to get the actual data
                    var data = arg.Replace("-graphics=", "");
                    Enum.TryParse<Graphics.eGraphicEngines>(data, true, out _RequestedGraphicEngine);
                }
            }
        }
    }
}
