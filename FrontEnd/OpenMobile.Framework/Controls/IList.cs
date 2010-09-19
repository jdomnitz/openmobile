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
        int SelectedIndex{get;set;}
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
