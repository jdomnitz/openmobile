using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenMobile.Plugins.OMNavit
{
    /// <summary>
    /// Managed wrapper for GList doubly linked lists 
    /// used for Manged/Native interop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GList : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeGList
        {
            public IntPtr Data;
            public IntPtr Next;
            public IntPtr Prev;
        }

        private enum Allocator { GLib, Managed }
        private Allocator _allocator;
        private IntPtr _nativePointer;
        private Action<IntPtr> _freeMethod;
        private List<NativeGList> _nodes;

        /// <summary>
        /// A new list generated in Native code
        /// </summary>
        /// <param name="pNativePointer"></param>
        /// <param name="pFreeMethod"></param>
        public GList(IntPtr pNativePointer, Action<IntPtr> pFreeMethod)
        {
            _allocator = Allocator.GLib;
            _nodes = new List<NativeGList>();
            _freeMethod = pFreeMethod;
            _nativePointer = pNativePointer;

            ExtractNodes();
        }

        /// <summary>
        /// New List passed to Native code
        /// </summary>
        public GList(Action<IntPtr> pFreeMethod)
        {
            _allocator = Allocator.Managed;
            _nodes = new List<NativeGList>();
        }

        private void ExtractNodes()
        {
            if (_nativePointer != IntPtr.Zero)
            {
                IntPtr tempPtr = _nativePointer;
                NativeGList start = MarshalNativeGList(tempPtr);
                NativeGList tempList = start;

                if (start.Data != IntPtr.Zero)
                    _nodes.Add(start);

                while ((tempPtr = tempList.Prev) != IntPtr.Zero)
                {
                    tempList = MarshalNativeGList(tempPtr);
                    if (tempList.Data != IntPtr.Zero)
                        _nodes.Insert(0, tempList);

                }

                tempPtr = _nativePointer;
                tempList = start;

                while ((tempPtr = tempList.Next) != IntPtr.Zero)
                {
                    tempList = MarshalNativeGList(tempPtr);
                    if (tempList.Data != IntPtr.Zero)
                        _nodes.Add(tempList);
                }
            }
        }

        public List<T> ToList<T>(Func<IntPtr, T> pMarshallMethod)
        {
            return _nodes.Select(node => pMarshallMethod(node.Data)).ToList();
        }

        public void AppendNode(IntPtr pPointer)
        {
            if (_allocator == Allocator.GLib)
                return; // don't support adding using GLib's allocator yet

            var newNode = new NativeGList
            {
                Prev = _nodes.Count > 1
                    ? _nodes[_nodes.Count - 2].Next
                        : _nodes.Count > 0
                            ? _nativePointer
                            : IntPtr.Zero,
                Next = IntPtr.Zero,
                Data = pPointer
            };

            var nativePointer = Marshal.AllocHGlobal(Marshal.SizeOf(newNode));
            Marshal.StructureToPtr(newNode, nativePointer, false);

            if (_nodes.Count > 0)
            {
                var node = _nodes[_nodes.Count - 1];
                node.Next = nativePointer;
                _nodes[_nodes.Count - 1] = node;
            }
            else _nativePointer = nativePointer;
        }

        private NativeGList MarshalNativeGList(IntPtr pGList)
        {
            return (NativeGList)Marshal.PtrToStructure(pGList, typeof(NativeGList));
        }

        private IntPtr AllocateNativeGList(NativeGList pGList)
        {
            return IntPtr.Zero;
            //Masrha
            //Marshal.StructureToPtr(pGList,
        }

        private void Destroy()
        {
            if (_freeMethod != null)
                _freeMethod(_nativePointer);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Destroy();
        }

        #endregion
    }
}
