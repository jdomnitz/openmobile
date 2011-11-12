using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMobile
{
    /// <summary>
    /// OMPanel history collection
    /// </summary>
    internal class historyCollection
    {
        /// <summary>
        /// Creates a new history item
        /// </summary>
        public struct historyItem
        {
            public bool forgettable;
            public string pluginName;
            public string panelName;
            public historyItem(string pluginName, string panelName, bool forgetMe)
            {
                this.pluginName = pluginName;
                this.panelName = panelName;
                this.forgettable = forgetMe;
            }
        }
        bool[] disabled;
        Stack<historyItem>[] items;
        historyItem[] currentItem;
        public void setDisabled(int screen, bool isDisabled)
        {
            disabled[screen] = isDisabled;
        }
        public bool getDisabled(int screen)
        {
            return disabled[screen];
        }
        public historyCollection(int count)
        {
            items = new Stack<historyItem>[count];
            currentItem = new historyItem[count];
            disabled = new bool[count];
            for (int i = 0; i < count; i++)
                items[i] = new Stack<historyItem>(10);
        }

        public void Enqueue(int screen, string pluginName, string panelName, bool forgetMe)
        {
            if (items[screen].Count == 10)
            {
                Stack<historyItem> tmp = new Stack<historyItem>(10);
                for (int i = 0; i < 6; i++)
                {
                    tmp.Push(items[screen].Pop());
                }
            }
            if ((currentItem[screen].pluginName != null) && (currentItem[screen].forgettable == false))
                items[screen].Push(currentItem[screen]);
            currentItem[screen] = new historyItem(pluginName, panelName, forgetMe);
        }
        public historyItem CurrentItem(int screen)
        {
            return currentItem[screen];
        }
        public historyItem Peek(int screen)
        {
            return items[screen].Peek();
        }
        public historyItem Dequeue(int screen)
        {
            historyItem tmp = currentItem[screen];
            currentItem[screen] = items[screen].Pop();
            return tmp;
        }
        public int Count(int screen)
        {
            return items[screen].Count;
        }
        public void Clear(int screen)
        {
            items[screen].Clear();
            currentItem[screen] = new historyItem();
        }
    }
}
