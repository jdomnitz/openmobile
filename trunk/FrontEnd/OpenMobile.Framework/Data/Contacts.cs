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
    /// A phonebook entry/contact
    /// </summary>
    public struct contact
    {
        /// <summary>
        /// Contact Name
        /// </summary>
        public string name;
        /// <summary>
        /// Cell number 1
        /// </summary>
        public string cell1;
        /// <summary>
        /// Cell number 2
        /// </summary>
        public string cell2;
        /// <summary>
        /// email address
        /// </summary>
        public string email;
        /// <summary>
        /// Fax number
        /// </summary>
        public string fax;
        /// <summary>
        /// Home number
        /// </summary>
        public string home;
        /// <summary>
        /// URL to contact image
        /// </summary>
        public string imageURL;
        /// <summary>
        /// Work number 1
        /// </summary>
        public string work1;
        /// <summary>
        /// Work number 2
        /// </summary>
        public string work2;
        /// <summary>
        /// Extra info
        /// </summary>
        public string comments;
    }
    /// <summary>
    /// Contacts
    /// </summary>
    public sealed class Contacts:IDisposable
    {
        private SQLiteConnection asyncCon;
        private SQLiteCommand cmd;
        private SQLiteDataReader asyncReader;
        /// <summary>
        /// Begin asynchronously reading contacts
        /// </summary>
        /// <returns></returns>
        public bool beginRead()
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Application.StartupPath, "Data", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            cmd = asyncCon.CreateCommand();
            cmd.CommandText = "SELECT * FROM Phonebook";
            asyncReader = cmd.ExecuteReader();
            return true;
        }
        /// <summary>
        /// begin asynchronously writing contacts
        /// </summary>
        /// <returns></returns>
        public bool beginWrite()
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Application.StartupPath, "Data", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            cmd = asyncCon.CreateCommand();
            return true;
        }
        /// <summary>
        /// Read the next available contact.  Begin read should be called before this
        /// </summary>
        /// <param name="storeAlso"></param>
        /// <returns></returns>
        public contact readNext(bool storeAlso)
        {
            contact ret = new contact();
            if (asyncReader.Read() == false)
                return ret;
            ret.cell1 = asyncReader["Cell1"].ToString();
            ret.cell2 = asyncReader["Cell2"].ToString();
            ret.email = asyncReader["Email"].ToString();
            ret.fax = asyncReader["Fax"].ToString();
            ret.home = asyncReader["Home"].ToString();
            ret.imageURL = asyncReader["imageURL"].ToString();
            ret.name = asyncReader["Name"].ToString();
            ret.work1 = asyncReader["Work1"].ToString();
            ret.work2 = asyncReader["Work2"].ToString();
            ret.comments = asyncReader["Comments"].ToString();
            if (storeAlso == true)
                Collections.contacts.Add(ret);
            return ret;
        }
        /// <summary>
        /// Write the next contact.  Called after begin write.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool writeNext(contact c)
        {
            StringBuilder query = new StringBuilder("BEGIN;DELETE FROM Phonebook WHERE Name='");
            {
                query.Append(c.name);
                query.Append("';INSERT INTO Phonebook ('Cell1','Cell2','Email','Fax','Home','imageURL','Name','Work1','Work2','Comments')VALUES('");
                query.Append(c.cell1);
                query.Append("','");
                query.Append(c.cell2);
                query.Append("','");
                query.Append(c.email);
                query.Append("','");
                query.Append(c.fax);
                query.Append("','");
                query.Append(c.home);
                query.Append("','");
                query.Append(c.imageURL);
                query.Append("','");
                query.Append(General.escape(c.name));
                query.Append("','");
                query.Append(c.work1);
                query.Append("','");
                query.Append(c.work2);
                query.Append("','");
                query.Append(c.comments);
                query.Append("');COMMIT");
            }
            cmd.CommandText = query.ToString();
            return (cmd.ExecuteNonQuery() == 1);
        }
        /// <summary>
        /// Closes the database connections
        /// </summary>
        public void close()
        {
            if (asyncReader != null)
                asyncReader.Dispose();
            if (asyncCon != null)
                asyncCon.Dispose();
            if (cmd != null)
                cmd.Dispose();
            asyncReader = null;
            asyncCon = null;
            cmd = null;
        }
        /// <summary>
        /// Closes the database connections and disposes resources
        /// </summary>
        public void  Dispose()
        {
            close();
            GC.SuppressFinalize(this);
        }
    }
}
