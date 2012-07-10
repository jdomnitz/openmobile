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
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpenMobile.Threading
{
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class LocalSafeThread
    {
        /// <summary>
        /// The function for the task manager to execute (should be a void function)
        /// </summary>
        public delegate void Function();
     
        //State:
        // -1 = Kill
        //  0 = Working
        //  1 = Sleeping
        static int availableThreads;
        static int maxThreads;
        static LocalSafeThread()
        {
            maxThreads = 25 * Environment.ProcessorCount;
        }
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
        public static void Asynchronous(Function function)
        {
            lock (functions)
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
        public static void Asynchronous(Delegate function, object[] args)
        {
            Asynchronous(delegate { function.DynamicInvoke(args); });
        }
        private static void spawnThread()
        {
            if (locks.Count >= maxThreads)
                return; //max thread count
            Thread p = new Thread(() =>
            {
                Function f;
                int id = Thread.CurrentThread.ManagedThreadId;
                ThreadState s = locks[id];
                availableThreads++;
                while (s.state >= 0)
                {
                    availableThreads--;
                    while (functions.Count > 0)
                    {
                        try
                        {
                            f = functions.Dequeue();
                        }
                        catch (InvalidOperationException)
                        {
                            locks.Remove(id);
                            return; //race condition
                        }
                        try
                        {
                            if (f != null)
                                f();
                        }
                        catch (Exception e)
                        {
                            // TODO: Add error handling
                            // No error handling available here
                        }
                    }
                    s.state = 1;
                    availableThreads++;
                    s.waitHandle.WaitOne(30000);
                    s.state = 0;
                    if ((functions.Count == 0) && (availableThreads > 1))
                    {
                        lock (locks)
                            s.state--;
                        availableThreads--;
                    }
                }
                lock (locks)
                    locks.Remove(id);
            });
            lock (locks)
                locks.Add(p.ManagedThreadId, new ThreadState());
            p.Name = "OpenMobile.LocalSafeThread";
            p.Start();
        }
    }
}