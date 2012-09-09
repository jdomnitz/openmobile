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
        static IPluginHost theHost;
        static int availableThreads;
        static int maxThreads;
        static SafeThread()
        {
            maxThreads = 25 * Environment.ProcessorCount;
        }
        private class ThreadState
        {
            /// <summary>
            /// Thread sync handle
            /// </summary>
            public EventWaitHandle waitHandle;

            public enum States { Kill, Working, Sleeping }

            /// <summary>
            /// Thread state
            /// States: -1 = Kill, 0 = Working, 1 = Sleeping
            /// </summary>
            public States state;

            /// <summary>
            ///  Initializes a new threadstate
            /// </summary>
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
        public static void Asynchronous(Function function, IPluginHost host)
        {
            if (theHost == null)
                theHost = host;
            lock (functions)
                functions.Enqueue(function);
            
            // Do we have thread available?
            if (availableThreads > 0)
            {   // Yes, use one of these
                lock (locks)
                {
                    foreach (ThreadState state in locks.Values)
                        if (state.state == ThreadState.States.Sleeping)
                        {
                            state.waitHandle.Set();
                            return;
                        }
                }
            }

            // No, spawn a new thread
            spawnThread();
        }
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        public static void Asynchronous(Function function)
        {
            Asynchronous(function, BuiltInComponents.Host);
        }
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Delegate function, object[] args, IPluginHost host)
        {
            Asynchronous(delegate { function.DynamicInvoke(args); }, host);
        }
        /// <summary>
        /// Creates a new asynchronous safe thread
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <param name="host"></param>
        public static void Asynchronous(Delegate function, object[] args)
        {
            Asynchronous(function, args, BuiltInComponents.Host);
        }

/*
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
                            if (theHost != null)
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
                            s.state--;
                        availableThreads--;
                    }
                }
                lock (locks)
                    locks.Remove(id);
            });

            lock (locks)
            {
                // Only spawn new thread if not already active
                if (!locks.ContainsKey(p.ManagedThreadId))
                {
                    locks.Add(p.ManagedThreadId, new ThreadState());
                    p.Name = "OpenMobile.SafeThread";
                    p.Start();                    
                }
            }
        }
*/

        private static void spawnThread()
        {
            // Limit amount of threads to use
            if (locks.Count >= maxThreads)
                return; 

            // Create new thread object
            Thread p = new Thread(ExecuteActions);

            // Spawn new thread
            lock (locks)
            {
                // Only spawn new thread if not already active
                if (!locks.ContainsKey(p.ManagedThreadId))
                {
                    // Register new thread in thread pool
                    locks.Add(p.ManagedThreadId, new ThreadState());
                    
                    // Start new thread
                    p.Name = String.Format("OpenMobile.SafeThread.{0}", p.ManagedThreadId);
                    p.Start();
                }
            }
        }

        private static void ExecuteActions()
        {
            // Report thread as available
            availableThreads++;

            // Get ID of this thread            
            int id = Thread.CurrentThread.ManagedThreadId;

            // Extract thread local context data
            ThreadState s = locks[id];
            
            // Set thread state
            s.state = ThreadState.States.Working;

            while (s.state == ThreadState.States.Working)
            {
                // Report this thread as busy
                availableThreads--;

                // Execute functions
                while (functions.Count > 0)
                {
                    // Extract function from gueue
                    Function f = null;
                    try
                    {
                        lock (functions)
                        {
                            if (functions.Count > 0)
                                f = functions.Dequeue();
                        }
                    }
                    catch
                    {
                    }

                    // Execute function
                    try
                    {  
                        if (f != null)
                            f();
                    }
                    catch (Exception e)
                    {   // Send error to debug log
                        BuiltInComponents.Host.DebugMsg(Thread.CurrentThread.Name, e);
                    }
                }

                // No more functions, go to idle state
                s.state = ThreadState.States.Sleeping;

                // Report thread as available
                availableThreads++;

                // Idle
                while (s.state == ThreadState.States.Sleeping)
                {
                    // Stop and wait for trigger (max 30 seconds, after timeout thread will die)
                    s.waitHandle.WaitOne(30000);

                    // Should we terminate this thread?
                    if ((functions.Count == 0) && (availableThreads > 1))
                    {   // Yes terminate this thread
                        s.state = ThreadState.States.Kill;
                    }
                    else
                    {   // No keep thread running

                        // Go back to idle or execute functions?
                        if (functions.Count >= 0)
                            s.state = ThreadState.States.Working;
                        else
                            s.state = ThreadState.States.Sleeping;
                    }
                }
            }

            // Report this thread as killed
            availableThreads--;

            // Remove thread data
            lock (locks)
                locks.Remove(id);
        }
    }
}