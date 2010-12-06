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
#if DEBUG
using System.Diagnostics;
#endif
using System.Collections.Generic;

namespace OpenMobile
{
    public delegate void Function();
    /// <summary>
    /// Provides a thread which is sandboxed to prevent crashing the rest of the framework when an error occurs
    /// </summary>
    public static class SandboxedThread
    {
        //State:
        // -1 = Kill
        //  0 = Working
        //  1 = Sleeping
        private struct ThreadState
        {
            public EventWaitHandle waitHandle;
            public int state;
            public ThreadState(int state)
            {
                waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                this.state = state;
            }
        }
        private static List<Thread> threadPool = new List<Thread>();
        private static Queue<Function> functions = new Queue<Function>();
        private static Dictionary<int, ThreadState> locks = new Dictionary<int, ThreadState>();
        static int availableThreads;

        public static void Asynchronous(Function function)
        {
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
        private static void spawnThread()
        {
            Thread p = new Thread(() =>
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                ThreadState s = locks[id];
                availableThreads++;
                while (locks[id].state >=0)
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
                            if (f!=null)
                                f();
                        }
                        catch (Exception e)
                        {
                            handle(e);
                        }
                    }
                    s.state = 1;
                    locks[id] = s;;
                    availableThreads++;
                    locks[id].waitHandle.WaitOne(30000);
                    s.state = 0;
                    locks[id] = s;
                    if ((functions.Count == 0) && (availableThreads > 1))
                    {
                        lock (locks)
                        {
                            s = locks[id];
                            s.state --;
                            locks[id] = s;
                        }
                        availableThreads--;
                    }
                }
                lock (locks)
                {
                    threadPool.Remove(Thread.CurrentThread);
                    locks.Remove(id);
                }
            });
            lock (locks)
                locks.Add(p.ManagedThreadId, new ThreadState(0));
            p.Start();
            threadPool.Add(p);
        }
        public static void handle(Exception e)
        {
            string message = e.GetType().ToString() + "(" + e.Message + ")\r\n\r\n" + e.StackTrace + "\r\n********";
            Core.theHost.sendMessage("OMDebug", e.Source, message);
            #if DEBUG
            Debug.Print(message);
            #endif
            int index = Core.pluginCollection.FindIndex(p => ((p!=null)&&(p.pluginName == e.Source)));
            IBasePlugin sample = null;
            if (index > -1)
                sample = Core.pluginCollection[index];
            if (sample != null)
            {
                if (Core.status != null)
                    Core.pluginCollection[index] = null;
                else if (Core.RenderingWindows[0] != null)
                    Core.pluginCollection.Remove(sample);
                Core.theHost.raiseSystemEvent(eFunction.backgroundOperationStatus, sample.pluginName + " CRASHED!", "ERROR", "");
                sample.Dispose();
            }
        }
    }
}
