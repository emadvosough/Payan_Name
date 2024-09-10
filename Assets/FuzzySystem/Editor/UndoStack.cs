using System.Collections.Generic;
using UnityEditor;

namespace FuzzySystem.Editor
{
    public class UndoStack
    {
        private List<HistoryItem> historyList = new List<HistoryItem>(50);

        private int pointerIndex = 0;

        private HistoryItem tempTopHistoryItem = null;

        public void Undo(FuzzyLogicEditor fuzzyLogicEditor)
        {
            if (fuzzyLogicEditor != null)
            {
                if (pointerIndex <= 0)
                {
                    EditorApplication.Beep();
                }
                else
                {
                    if (pointerIndex == historyList.Count)
                    {
                        tempTopHistoryItem = new HistoryItem();
                        tempTopHistoryItem.serializedData = FuzzySystem.Serialize(fuzzyLogicEditor.FuzzySystem);
                    }

                    pointerIndex--;
                    var historyItem = historyList[pointerIndex];
                    fuzzyLogicEditor.FuzzySystem = FuzzySystem.Deserialize(historyItem.serializedData, fuzzyLogicEditor.FuzzySystem);
                }
            }
        }

        public void Redo(FuzzyLogicEditor fuzzyLogicEditor)
        {
            if (fuzzyLogicEditor != null)
            {
                if (pointerIndex >= historyList.Count - 1)
                {
                    if (tempTopHistoryItem == null)
                    {
                        EditorApplication.Beep();
                    }
                    else
                    {
                        fuzzyLogicEditor.FuzzySystem = FuzzySystem.Deserialize(tempTopHistoryItem.serializedData, fuzzyLogicEditor.FuzzySystem);
                        tempTopHistoryItem = null;
                        pointerIndex = historyList.Count;
                    }
                }
                else
                {
                    pointerIndex++;
                    var historyItem = historyList[pointerIndex];
                    fuzzyLogicEditor.FuzzySystem = FuzzySystem.Deserialize(historyItem.serializedData, fuzzyLogicEditor.FuzzySystem);
                }
            }
        }

        public void Record(FuzzySystem fuzzySystem)
        {
            Record_Internal(fuzzySystem);
        }

        public void Empty()
        {
            historyList.Clear();
            pointerIndex = 0;
            tempTopHistoryItem = null;
        }

        private void Record_Internal(FuzzySystem fuzzySystem)
        {
            if (fuzzySystem != null)
            {
                tempTopHistoryItem = null;

                for (int i = historyList.Count - 1; i >= pointerIndex; --i)
                {
                    historyList.RemoveAt(i);
                }

                if (historyList.Count + 1 > historyList.Capacity)
                {
                    historyList.RemoveAt(0);
                }

                var historyItem = new HistoryItem();
                historyItem.serializedData = FuzzySystem.Serialize(fuzzySystem);
                historyList.Add(historyItem);
                pointerIndex = historyList.Count;

                GUIUtils.Get(fuzzySystem).isChanged = true;
            }
        }

        private class HistoryItem
        {
            public byte[] serializedData = null;
        }
    }
}