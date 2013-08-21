// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Core
{
    #region ITreeItem

    public interface ITreeItem : IData, IUid, ITag, ISelected
    {
    	IEnumerable<ITreeItem> GetItems();
        int GetItemsCount();

        ITreeItem GetItem(int index);
        int GetItemIndex(ITreeItem item);

        void Add(ITreeItem item);
        void Remove(ITreeItem item);
        void Clear();

        object GetParent();

        void PushIntoView();
    }

    #endregion
}
