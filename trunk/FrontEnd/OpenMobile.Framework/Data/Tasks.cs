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
using System.Data.SQLite;
using System.Text;
using System.Windows.Forms;
using OpenMobile.helperFunctions;

namespace OpenMobile.Data
{
    /// <summary>
    /// Tasks (To Do)
    /// </summary>
    public sealed class Tasks:IDisposable
    {
        private SQLiteConnection asyncCon;
        private SQLiteCommand cmd;
        private SQLiteDataReader asyncReader;
        
        /// <summary>
        /// State of the task
        /// </summary>
        [Flags]
        public enum state
        {
            /// <summary>
            /// Not Set
            /// </summary>
            NotSet=0,
            /// <summary>
            /// Remind
            /// </summary>
            Remind=1,
            /// <summary>
            /// Event is in the past
            /// </summary>
            Past=2,
            /// <summary>
            /// Task Complete
            /// </summary>
            Completed=4,
            /// <summary>
            /// Hide the task
            /// </summary>
            Hide=8
        }
        /// <summary>
        /// A task
        /// </summary>
        public sealed class task
        {
            /// <summary>
            /// Task Category
            /// </summary>
            public int Category;
            /// <summary>
            /// Task Description
            /// </summary>
            public string Description;
            int id=-1;
            /// <summary>
            /// Task ID (set by database)
            /// </summary>
            public int ID { get{return id;} }
            /// <summary>
            /// Task Importance
            /// </summary>
            public priority Importance=priority.Normal;
            /// <summary>
            /// Task Title
            /// </summary>
            public string Title;
            /// <summary>
            /// Task State
            /// </summary>
            public state State;
            /// <summary>
            /// Create a new task
            /// </summary>
            /// <param name="ID">Should be given by the database</param>
            public task(int ID)
            {
                id = ID;
            }
            /// <summary>
            /// Create a new task
            /// </summary>
            public task() { }
        }
        /// <summary>
        /// Begin reading all tasks from the database
        /// </summary>
        /// <returns></returns>
        public bool beginRead()
        {
            if (asyncCon == null)
                asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            cmd = asyncCon.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tasks";
            asyncCon.Open();
            asyncReader = cmd.ExecuteReader();
            return true;
        }
        /// <summary>
        /// Begin reading tasks that match the given taskID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool beginRead(int ID)
        {
            if (asyncCon == null)
                asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            cmd = asyncCon.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tasks WHERE ID='"+ID+"'";
            asyncCon.Open();
            asyncReader = cmd.ExecuteReader();
            return true;
        }
        /// <summary>
        /// Return the next task
        /// </summary>
        /// <returns></returns>
        public task readNext()
        {
            if ((asyncReader==null)||(asyncReader.Read()==false))
                return new task();
            int ID = asyncReader.GetInt32(asyncReader.GetOrdinal("ID"));
            task ret = new task(ID);
            ret.Category = asyncReader.GetInt32(asyncReader.GetOrdinal("Category"));
            ret.Description = asyncReader["Description"].ToString();
            ret.Importance = (priority)Enum.Parse(typeof(priority), asyncReader["Importance"].ToString());
            ret.State = (state)Enum.Parse(typeof(state), asyncReader["State"].ToString());
            ret.Title = asyncReader["Title"].ToString();
            return ret;
        }
        /// <summary>
        /// Begin asynchronously writing tasks
        /// </summary>
        /// <returns></returns>
        public bool beginWrite()
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            cmd = asyncCon.CreateCommand();
            return true;
        }
        /// <summary>
        /// Delete the task with the given ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool deleteTask(int ID)
        {
            if (asyncCon == null)
                return false;
            cmd.CommandText = "DELETE FROM Tasks WHERE ID='" + ID + "'";
            return (cmd.ExecuteNonQuery() ==1);
        }
        /// <summary>
        /// Write the given task.  Called after beingWrite
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool write(task t)
        {
            StringBuilder query;
            if (t.ID!=-1)
            {
                query = new StringBuilder("DELETE FROM Tasks WHERE ID='");
                query.Append(t.ID);
                query.Append("';INSERT INTO Tasks ('Category','Description','Importance','State','Title','ID')VALUES('");
            }
            else
            {
                query = new StringBuilder("INSERT OR REPLACE INTO Tasks ('Category','Description','Importance','State','Title')VALUES('");
            }
            {
                query.Append(t.Category);
                query.Append("','");
                query.Append(General.escape(t.Description));
                query.Append("','");
                query.Append((int)t.Importance);
                query.Append("','");
                query.Append((int)t.State);
                query.Append("','");
                query.Append(General.escape(t.Title));
                if (t.ID>=0)
                {
                    query.Append("','");
                    query.Append(t.ID);
                }
                query.Append("')");
            }
            cmd.CommandText = query.ToString();
            return (cmd.ExecuteNonQuery() == 1);
        }
        /// <summary>
        /// Close the connection to the database
        /// </summary>
        public void Close()
        {
            try
            {
                if (asyncReader != null)
                    asyncReader.Dispose();
                if (asyncCon != null)
                    asyncCon.Dispose();
                asyncReader = null;
                asyncCon = null;
            }
            catch (Exception) { }
        }
        /// <summary>
        /// Closes the connection and unloads the class
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
