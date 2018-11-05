using UnityEngine;

namespace Framewerk.ViewComponents.ListComponent
{
    public class VirtualizedListMapper
    {
        /// <summary>
        /// Data index of first visible item in the list
        /// </summary>
        public int FirstDataIndex { get; set; }

        /// <summary>
        /// DataIndex of last visible item in the list
        /// </summary>
        public int LastDataIndex
        {
            get
            {
                return FirstDataIndex + ViewCount - 1;
            }
        }

        /// <summary>
        /// View index of first visible item in the list
        /// </summary>
        public int FirstViewIndex
        {
            get
            {
                return GetViewIndex(FirstDataIndex);
            }
        }

        /// <summary>
        /// View index of last visible item in the list
        /// </summary>
        public int LastViewIndex
        {
            get
            {
                return GetViewIndex(LastDataIndex);
            }
        }

        /// Total number of views in the list
        public int ViewCount { get; private set; }
        
        /// Total number of data in the list
        public int DataCount { get; private set; }

        public VirtualizedListMapper(int viewCount, int dataCount)
        {
            //there can't be more views than data
            ViewCount = Mathf.Min(viewCount, dataCount);
            
            DataCount = dataCount;
        }

        public bool IsItemVisible(int dataIndex)
        {
            return dataIndex >= FirstDataIndex && dataIndex <= LastDataIndex;
        }

        public int GetViewIndex(int dataIndex)
        {
            if (ViewCount == 0)
                return 0;
            
            return PosMod(dataIndex, ViewCount);
        }

        //TODO: move to math utils
        //Remain - Always positive modulo 
        private int PosMod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}