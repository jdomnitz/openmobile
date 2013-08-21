using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Controls;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenMobile.Plugins.OMNavit
{
    public class NavitEventCallback
    {
        private static object _eventIdLockObject = new object();
        private static int _nextEventId = 1;

        private DateTime _lastExecution;

        public bool MarkedForRemoval { get; private set; }
        public int EventId { get; private set;}
        public int Timeout { get; private set;}

        public DateTime TimeNextCall 
        {
            get
            {
                return _lastExecution.AddMilliseconds(Timeout);
            }
        }

        public NavitEventCallback(int pTimeout)
        {
            _lastExecution = DateTime.Now;

            //lock(_eventIdLockObject)
                EventId = _nextEventId++;
            Timeout = pTimeout;
        }

        public void Executed()
        {
            _lastExecution = DateTime.Now;
        }

        public void MarkForRemoval()
        {
            MarkedForRemoval = true;
        }
    }
    public class NavitOpenGLInterface : OMControl, IMouse, IDisposable, IKey//, IThrow
    {
        private List<NavitEventCallback> _eventCallbacks;
        
        private const int MOUSE_DOWN = 0;
        private const int MOUSE_UP = 1;

        List<object> _umanaged_resources = new List<object>();

        internal const string NavitLibrary = "libnavit_core.dll";
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NavitEventAddCallback(int timeout);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavitDebugMessageCallback(string message, int level);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NavitEventRemoveCallback(int eventId);


        // Structures 

        [StructLayout(LayoutKind.Sequential)]
        public struct MapItem
        {
            public string Name;
            public int Type;
            public Coord Coord;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct AddressSearchRequest
        {
            public string Country;
            public string Town;
            public string Postal;
            public string Street;
            public string HouseNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AddressSearchResult
        {
            public int Id;
            public string Country;
            public string Town;
            public string Postal;
            public string Street;
            public string HouseNumber;

            public Coord Coord;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct NavitPoint
        {
            public int X;
            public int Y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct NavitPoint3d
        {
            public int X;
            public int Y;
            //public int Z;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public double X;
            public double Y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PoiSearchResult
        {
            public int Type;
            public float Distance;
            public string Direction;
            public string Name;
            public Coord Coord;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct poiSearchRequest
        {
            public byte sel;
            public byte selnb;
            public int pageCount;
            public byte pagenb;
            public byte dist;
            public byte isAddressFilter;
            public string filterstr;
            public IntPtr filter; // GLst
        };

        public enum PoiType
        {
            None = -1,
            Bank,
            Fuel,
            Lodging,
            Food,
            Shopping,
            Medical,
            Parking,
            Park,
            Other
        }

        /// <summary>
        /// POI search/filtering parameters.
        /// </summary>
        public struct PoiSearchRequest
        {
            /// <summary>
            ///  Index to struct selector selectors[], shows what type of POIs is defined.
            /// </summary>		
            public PoiType Type;
            /// <summary>
            ///  Radius (number of 10-kilometer intervals) to search for POIs.
            /// </summary>		
            public int DistMultiplyer;
            /// <summary>
            /// Number of Pois per page
            /// </summary>
            public int PoisPerPage;
            /// <summary>
            /// Page number of results
            /// </summary>
            public int PageNumber;
            /// <summary>
            ///  Should filter phrase be compared to postal address of the POI.
            ///  =1 - address filter, =0 - name filter
            /// </summary>
            public bool IsAddressFilter;
            /// <summary>
            ///  Filter string, casefold()ed and divided into substrings at the spaces, which are replaced by ASCII 0/// .
            /// </summary>		
            public string FilterString;
        };

        // Functions

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "main_real"), SuppressUnmanagedCodeSecurity]
        private static extern int NavitLoad(int args, string[] lol);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_opengl_initialize"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitOpenGLInitialize(int x, int y, int width, int height);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_display"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitDraw();

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_display_displaylist"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitDrawBasic();

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_input_key_press"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitKeyPress(int pKeyCode);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_input_mouse_move"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitMouseMove(int x, int y);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_input_mouse_click"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitMouseClick(int button, int state, int x, int y);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_event_call"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitCallEvent(int eventId);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_register_callback_event_add"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitRegisterEventAddDelegate(NavitEventAddCallback addEventDelegate);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_register_callback_event_remove"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitRegisterEventRemoveDelegate(NavitEventRemoveCallback removeEventDelegate);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "external_register_callback_debug"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitRegisterDebugMessageDelegate(NavitDebugMessageCallback debugMessageDelegate);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_search_address"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr NavitSearchAddress(ref AddressSearchRequest pRequest, int pMaxCount);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_set_destination_from_coord"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitSetDestination(Coord pDestination, string pDescription);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_clear_destination"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitClearDestination();

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_get_current_position"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr NavitGetCurrentPosition();

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_get_points_of_interest"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr NavitGetPointsOfInterest(IntPtr poiSearchRequestPtr);//ref poiSearchRequest pointer);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_destroy_poi_search_results"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitFreePointsOfInterests(IntPtr pointer);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_destroy_address_search_results"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitFreeAddressSearchResults(IntPtr pointer);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_get_map_item_at_screen_location"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr NavitGetMapItemFromScreenPoint(NavitPoint pPoint);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "gui_external_destroy_map_item"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitFreeMapItem(IntPtr pointer);

        [DllImport(NavitLibrary, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "gui_external_free_memory"), SuppressUnmanagedCodeSecurity]
        private static extern void NavitFreeMemory(IntPtr pointer);

#if DEBUG
        private Action<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.MapItem> DoSetMapItem;
#endif
        public NavitOpenGLInterface()
        {
   
#if DEBUG
            if (false) // Set to true if you want to display NavitTestForm
            {
                var navit = new NavitTest();
                navit.DoAddressSearchRequest = GetAddressSearchResults;
                navit.DoPoiSearch = GetPoiSearchResults;
                navit.DoGetCurrentPosition = GetCurrentPosition;
                navit.DoClearDestination = ClearDestination;
                navit.DoSetDestination = SetDestination;
                DoSetMapItem = navit.DoSetMapItem;


                // Start a windows forms app loop
                System.Threading.Thread thread = new System.Threading.Thread(
                    () => System.Windows.Forms.Application.Run(navit))
                    {
                        Name = "OM Navit Test Form Message Loop"
                    };
                thread.Start();
            }
#endif
            _eventCallbacks = new List<NavitEventCallback>();

            // Register callbacks
            NavitRegisterEventAddDelegate(AddUmanagedReference(new NavitEventAddCallback(AddEventTimeout)) as NavitEventAddCallback);
            NavitRegisterEventRemoveDelegate(AddUmanagedReference(new NavitEventRemoveCallback(RemoveEventTimeout)) as NavitEventRemoveCallback);
            NavitRegisterDebugMessageDelegate(AddUmanagedReference(new NavitDebugMessageCallback(DebugMessageOut)) as NavitDebugMessageCallback);

            string[] args = new string[] { " " };

            NavitLoad(args.Length, args);
            //Start loading on a different thread for now.
            //new Action(()=> NavitLoad(args.Length, args)).BeginInvoke(null,null);

            //System.Threading.Thread.Sleep(3000);
        }


        private bool redraw = false;
        private bool firstLoad = true;

        public override void Render(OpenMobile.Graphics.Graphics pGraphics, OpenMobile.renderingParams pRenderingParams)
        {
            try
            {
                if (firstLoad)
                {
                    NavitOpenGLInitialize(0, 74, 719, 372);
                    //NavitDraw();
                    firstLoad = false;

                    return;
                }
                if (redraw)
                {
                    //NavitDraw();
                    redraw = false;
                }

                ProcessEventCallbacks();
                //NavitDraw(); 
                NavitDrawBasic();
            }
            catch (Exception anException)
            {
            }
            finally
            {
            }
        }

        // Should be called only from the rendering thread
        private void ProcessEventCallbacks()
        {
            //NavitCallEvent(99);
            //return;
            NavitEventCallback[] copiedList = null;
            lock (_eventCallbacks)
            {
                copiedList = new NavitEventCallback[_eventCallbacks.Count];
                _eventCallbacks.CopyTo(copiedList);
            }

            foreach (var eventCallback in copiedList)
                if (eventCallback.TimeNextCall < DateTime.Now)
                {
                    NavitCallEvent(eventCallback.EventId);
                    DebugMessageOut("Executing event: " + eventCallback.EventId, 0);
                    eventCallback.Executed();
                    DebugMessageOut("Done Executing event: " + eventCallback.EventId, 0);

                }
        }

        private void DebugMessageOut(StringBuilder message, int level)
        {
            DebugMessageOut(message.ToString(), level);
        }

        private void DebugMessageOut(string message, int level)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private int AddEventTimeout(int pTimeout)
        {
            var eventCallback = new NavitEventCallback(pTimeout);
            int eventId = eventCallback.EventId;

            DebugMessageOut("Adding event: " + eventId + " with timeout " + pTimeout, 0);

            lock (_eventCallbacks)
                _eventCallbacks.Add(eventCallback);

            return eventId;
        }

        private void RemoveEventTimeout(int pEventId)
        {
            DebugMessageOut("Removing event: " + pEventId, 0);
            lock (_eventCallbacks)
            {
                NavitEventCallback temp = null;
                foreach (var eventCallback in _eventCallbacks)
                    if (eventCallback.EventId == pEventId)
                    {
                        temp = eventCallback;
                        break;
                    }

                if (temp != null)
                    _eventCallbacks.Remove(temp);
            }
        }

        private object AddUmanagedReference(object pUmmanagedReference)
        {
            _umanaged_resources.Add(GCHandle.Alloc(pUmmanagedReference));

            return pUmmanagedReference;
        }


        public void SearchForAddress()
        {
            var request = new AddressSearchRequest();
            request.Country = "US";
            request.Town = "Torrington";
            request.Street = "Al";
            request.HouseNumber = "15";

            var results = GetAddressSearchResults(request);
        }

        private AddressSearchResult[] GetAddressSearchResults(AddressSearchRequest pRequest)
        {
            IntPtr resultPtr = NavitSearchAddress(ref pRequest, 34);

            using (var glist = new GList(resultPtr, NavitFreeAddressSearchResults))
            {

                var returnArray = glist
                    .ToList((ptr) => (AddressSearchResult)Marshal.PtrToStructure(ptr, typeof(AddressSearchResult)))
                    .ToArray();

                return returnArray;
            }
        }

        private poiSearchRequest translateToInternalSearchRequest(PoiSearchRequest pSearchRequest)
        {
            return new poiSearchRequest
            {
                dist = (byte)pSearchRequest.DistMultiplyer,
                filter = IntPtr.Zero,
                filterstr = pSearchRequest.FilterString,
                sel = (byte)(pSearchRequest.Type == PoiType.None ? 0 : 1),
                selnb = (byte)pSearchRequest.Type,
                isAddressFilter = (byte)(pSearchRequest.IsAddressFilter ? 0 : 1),
                pageCount = pSearchRequest.PoisPerPage,
                pagenb = (byte)pSearchRequest.PageNumber
            };
        }

        private PoiSearchResult[] GetPoiSearchResults(PoiSearchRequest pRequest)
        {
            var request = translateToInternalSearchRequest(pRequest);

            // Move to Unmanaged memory
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, pointer, false);

            IntPtr resultPtr = NavitGetPointsOfInterest(pointer);

            using (var glist = new GList(resultPtr, NavitFreePointsOfInterests))
            {
                var returnArray = glist
                    .ToList((ptr) => (PoiSearchResult)Marshal.PtrToStructure(ptr, typeof(PoiSearchResult)))
                    .ToArray();

                return returnArray;
            }
        }

        public void SetDestination(Coord pCoord)
        {
            NavitSetDestination(pCoord, "Test Address");
        }

        public void ClearDestination()
        {
            NavitClearDestination();
        }

        public MapItem GetMapItemAtScreenLocation(OpenMobile.Graphics.Point pPoint)
        {
            var point = new NavitPoint
            {
                X = (int)pPoint.X,
                Y = (int)pPoint.Y
            };
            var pointer = NavitGetMapItemFromScreenPoint(point);

            try
            {
                return (MapItem)Marshal.PtrToStructure(pointer, typeof(MapItem));
            }
            finally
            {
                NavitFreeMapItem(pointer);
            }
        }

        public Coord GetCurrentPosition()
        {
            var pointer = NavitGetCurrentPosition();

            try
            {
                return (Coord)Marshal.PtrToStructure(pointer, typeof(Coord));
            }
            finally
            {
                NavitFreeMemory(pointer);
            }
        }

        #region IMouse Members

        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, float WidthScale, float HeightScale)
        {
            redraw = true;

            //int x = (int)((1000F / 736F) * e.X);
            //int y = (int)((488F / 371F) * (e.Y - 76));
            int x = e.X;
            int y = (int)e.Y - 76;

            System.Diagnostics.Debug.WriteLine("Moving: " + mode.ToString());
            try
            {
                NavitMouseMove(x, y);
            }
            catch(Exception ex)
            {

            }
            raiseUpdate(true);
        }

        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {

            int x = e.X;
            int y = (int)e.Y - 76;

            System.Diagnostics.Debug.WriteLine("DOWN");
            //mode = OpenMobile.eModeType.Scrolling;
            // State is mouse Up or mouse Down

            try
            {
                NavitMouseClick((int)e.Button, MOUSE_DOWN, x, y);
            }
            catch (Exception ex)
            {

            }
            raiseUpdate(true);
        }

        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            int x = e.X;
            int y = (int)e.Y - 76;

            System.Diagnostics.Debug.WriteLine("UP " + x + " " + y.ToString());

            var point = new OpenMobile.Graphics.Point
            {
                X = x,
                Y = y
            };

            var mapItem = GetMapItemAtScreenLocation(point);
            
            if (DoSetMapItem != null)
                DoSetMapItem(mapItem);
            //mode = OpenMobile.eModeType.Normal;
            NavitMouseClick((int)e.Button, MOUSE_UP, x, y);
            raiseUpdate(true);
        }

        #endregion

        //#region IThrow Members

        //public void MouseThrow(int screen, OpenMobile.Graphics.Point TotalDistance, OpenMobile.Graphics.Point RelativeDistance)
        //{
            
        //}

        //public void MouseThrowStart(int screen, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.PointF scaleFactors, ref bool Cancel)
        //{
            
        //}

        //public void MouseThrowEnd(int screen, OpenMobile.Graphics.Point EndLocation)
        //{
            
        //}

        //#endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IKey Members

        public bool KeyDown(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, float WidthScale, float HeightScale)
        {
            NavitKeyPress(e.KeyCode);
            return true;
        }

        public bool KeyUp(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, float WidthScale, float HeightScale)
        {
            return false;
        }

        #endregion
    }
}
