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
using System.Threading;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpenMobile.Threading
{
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class SafeThread
    {
        //State:
        // -1 = Kill
        //  0 = Working
        //  1 = Sleeping
        static IPluginHost theHost;
        static int availableThreads;
        private class ThreadState
        {
            public EventWaitHandle waitHandle;
            public int state;
            public ThreadState()
            {
                waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            }
        }
        private static Queue<Function> functions = new Queue<Function>();
        private static Dictionary<int, ThreadState> locks = new Dictionary<int, ThreadState>();
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Function function,IPluginHost host)
        {
            if (theHost == null)
                theHost = host;
            functions.Enqueue(function);
            if (availableThreads > 0)
            {
                lock (locks)
                {
                    foreach (ThreadState state in locks.Values)
                        if (state.state == 1)
                        {
                            state.waitHandle.Set();
                            return;
                        }
                }
            }
            spawnThread();
        }
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Delegate function,object[] args, IPluginHost host)
        {
            Asynchronous(delegate { function.DynamicInvoke(args); }, host);
        }
        private static void spawnThread()
        {
            Thread p = new Thread(() =>
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                ThreadState s = locks[id];
                availableThreads++;
                while (locks[id].state >= 0)
                {
                    availableThreads--;
                    while (functions.Count > 0)
                    {
                        Function f;
                        lock (locks)
                        {
                            try
                            {
                                f = functions.Dequeue();
                            }
                            catch (InvalidOperationException)
                            {
                                return; //race condition
                            }
                        }
                        try
                        {
                            if (f != null)
                                f();
                        }
                        catch (Exception e)
                        {
                            if (theHost!=null)
                                theHost.sendMessage("SandboxedThread", "SafeThread", "", ref e);
                        }
                    }
                    s.state = 1;
                    availableThreads++;
                    s.waitHandle.WaitOne(30000);
                    s.state = 0;
                    if ((functions.Count == 0) && (availableThreads > 1))
                    {
                        lock (locks)
                        {
                            s.state--;
                        }
                        availableThreads--;
                    }
                }
                lock (locks)
                {
                    locks.Remove(id);
                }
            });
            lock (locks)
                locks.Add(p.ManagedThreadId, new ThreadState());
            p.Start();
        }
    }
}