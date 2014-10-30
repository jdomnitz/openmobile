using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenMobile
{
    internal class OSFunctions
    {
        /// <summary>
        /// OS interaction object
        /// </summary>
        static public OpenMobile.Framework.OSInteractionBase OS
        {
            get
            {
                return _OS;
            }
            set
            {
                if (_OS != value)
                {
                    _OS = value;
                }
            }
        }
        static private OpenMobile.Framework.OSInteractionBase _OS;

        static OSFunctions()
        {
            // Initialize OS specific code
            if (Configuration.RunningOnWindows)
                _OS = new OpenMobile.Framework.OS.Windows.OSInteraction(OSSpecificEventCallback);
            //else
            //    _OS = new OpenMobile.Framework.OS.Linux.OSInteraction(OSSpecificEventCallback);
        }

        /// <summary>
        /// Initialize os functions
        /// </summary>
        static public void Init()
        {
            // Dummy call to ensure we initialize the OS functions
        }

        static private void OSSpecificEventCallback(OpenMobile.Framework.OSInteractionBase.CallbackEvents ev)
        {
            OM.Host.DebugMsg(DebugMessageType.Info, "OpenMobile.Core.OSSpecificEventCallback", String.Format("Received OS Event: {0}", ev));

            switch (ev)
            {
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.Unspecified:
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.System_LogOffPending:
                    Core.CloseProgram(ShutdownModes.Normal);    
                    //Core.theHost.execute(eFunction.closeProgram);
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.System_ShutdownPending:
                    Core.CloseProgram(ShutdownModes.Normal);    
                    //Core.theHost.execute(eFunction.closeProgram);
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.System_SystemResumed:
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.System_SleepOrHibernatePending:
                    Core.CloseProgram(ShutdownModes.Normal);    
                    //Core.theHost.execute(eFunction.closeProgram);
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.Power_Battery_Low:
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.Power_Battery_Critical:
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.Power_Battery_RunningOnBattery:
                    break;
                case OpenMobile.Framework.OSInteractionBase.CallbackEvents.Power_Battery_RunningOnLine:
                    break;
                default:
                    break;
            }
        }
    }
}
