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

using System.Threading;

namespace OpenMobile.Threading
{
    /// <summary>
    /// The function for the task manager to execute (should be a void function)
    /// </summary>
    public delegate void Function();
    /// <summary>
    /// Handles execution of background tasks using the systems idle resources
    /// </summary>
    public static class TaskManager
    {
        static EventWaitHandle sync = new EventWaitHandle(false, EventResetMode.AutoReset);
        public struct taskItem
        {
            public Function function;
            public ePriority Priority;
            public string TaskName;
            public taskItem(Function function, ePriority Priority,string name)
            {
                this.function = function;
                this.Priority = Priority;
                this.TaskName = name;
            }
        }
        public static List<taskItem> Tasks = new List<taskItem>();
        static Thread taskThread;
        static EventWaitHandle enabled=new EventWaitHandle(false,EventResetMode.ManualReset);
        /// <summary>
        /// Add a BackgroundTask to the task list
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskPriority"></param>
        public static void QueueTask(Function task, ePriority taskPriority)
        {
            QueueTask(task, taskPriority, "Unknown Task");
        }
        /// <summary>
        /// Add a BackgroundTask to the task list
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskPriority"></param>
        /// <param name="taskName"></param>
        public static void QueueTask(Function task,ePriority taskPriority,string taskName)
        {
            Tasks.Add(new taskItem(task, taskPriority,taskName));
            sync.Set();
            lock(Tasks)//No particular reason other then its a mutual object to provide thread safety
            {
                if (taskThread == null)
                {
                    taskThread = new Thread(new ThreadStart(executeTask));
                    taskThread.IsBackground = true;
                    taskThread.Priority = ThreadPriority.BelowNormal;
                    taskThread.Name = "OM Task Manager";
                    taskThread.Start();
                }
            }
        }
        /// <summary>
        /// Enables the Task Manager (called automatically after plugins have been loaded)
        /// </summary>
        public static void Enable()
        {
            priorities = (ePriority[])Enum.GetValues(typeof(ePriority));
            Array.Reverse(priorities);
            enabled.Set();
        }
        static ePriority[] priorities;
        private static taskItem getNext()
        {
            taskItem ret;
            foreach (ePriority p in priorities)
            {
                if (Tasks.Exists(k => k.Priority == p) == true)
                {
                    ret = Tasks.Find(k => k.Priority == p);
                    Tasks.Remove(ret);
                    return ret;
                }
            }
            throw new InvalidOperationException("Task Scheduler unable to prioritize");
        }
        static void executeTask()
        {
            while (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested)
            {
                enabled.WaitOne();
                sync.WaitOne();
                try
                {
                    taskItem current = getNext();
                    current.function.Invoke();
                }
                catch { }
                if (Tasks.Count>0)
                    sync.Set();
            }
        }
    }
}
