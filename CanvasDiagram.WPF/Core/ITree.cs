// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
