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
namespace OpenMobile.Controls
{
    /// <summary>
    /// The basis for OMList
    /// </summary>
    public interface IList
    {
        /// <summary>
        /// Add a string to the list
        /// </summary>
        /// <param name="item"></param>
        void Add(string item);
        /// <summary>
        /// Add a listitem to the list
        /// </summary>
        /// <param name="item"></param>
        void Add(OpenMobile.OMListItem item);
        /// <summary>
        /// Adds a unique list item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool AddDistinct(OpenMobile.OMListItem item);
        /// <summary>
        /// Adds a range of strings to the list
        /// </summary>
        /// <param name="source"></param>
        void AddRange(string[] source);
        /// <summary>
        /// Adds a range of list items to the list
        /// </summary>
        /// <param name="source"></param>
        void AddRange(System.Collections.Generic.List<OpenMobile.OMListItem> source);
        /// <summary>
        /// Adds a range of strings to the list
        /// </summary>
        /// <param name="source"></param>
        void AddRange(System.Collections.Generic.List<string> source);
        /// <summary>
        /// Clears the list
        /// </summary>
        void Clear();
        /// <summary>
        /// Returns the number of items in a list
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets a range of list items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<OMListItem> getRange(int index, int count);
        /// <summary>
        /// Gets the index of the item matching the given name
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int indexOf(string item);
        /// <summary>
        /// Select the given item and scroll it into view
        /// </summary>
        /// <param name="index"></param>
        void Select(int index);
        /// <summary>
        /// Returns the index of the currently selected item
        /// </summary>
        int SelectedIndex { get; set; }
        /// <summary>
        /// Sorts the list alphabetically
        /// </summary>
        void Sort();
        /// <summary>
        /// Gets the start of the list (listIndex)
        /// </summary>
        int Start { get; }
        /// <summary>
        /// Returns the list item at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        OMListItem this[int index] { get; set; }
    }
}
