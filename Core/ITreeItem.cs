// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Core
{
    #region ITreeItem

    public interface ITreeItem : IData, IUid, ITag, ISelected
    {
    	IEnumerable<ITreeItem> GetItems();

    	void Add(ITreeItem item);
        void Remove(ITreeItem item);
        void Clear();

        ITreeItem GetParent();
    }

    #endregion
}
