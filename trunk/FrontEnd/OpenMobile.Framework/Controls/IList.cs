using System;
using System.Collections.Generic;
namespace OpenMobile.Controls
{
    public interface IList
    {
        void Add(string item);
        void Add(OpenMobile.OMListItem item);
        bool AddDistinct(OpenMobile.OMListItem item);
        void AddRange(string[] source);
        void AddRange(System.Collections.Generic.List<OpenMobile.OMListItem> source);
        void AddRange(System.Collections.Generic.List<string> source);
        void Clear();
        int Count { get; }
        List<OMListItem> getRange(int index, int count);
        int indexOf(string item);
        void Select(int index);
        void Sort();
        int Start { get; }
        OMListItem this[int index] { get; set; }
    }
}
