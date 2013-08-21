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
    #region ITree

    public interface ITree : IData, IUid, ITag, ISelected
    {
    	IEnumerable<ITreeItem> GetItems();

    	void Add(ITreeItem item);
        void Remove(ITreeItem item);
        void Clear();

        object GetSelectedItem();
    }

    #endregion
}
