// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

#endregion

namespace CanvasDiagramEditor.Controls
{
    #region SolutionTreeViewItem

    public class SolutionTreeViewItem : TreeViewItem, ITreeItem
    {
        #region ITreeItem

        public IEnumerable<ITreeItem> GetItems()
        {
            var elements = this.Items.Cast<FrameworkElement>();

            return elements.Cast<ITreeItem>();
        }

        public void Add(ITreeItem item)
        {
            this.Items.Add(item as FrameworkElement);
        }

        public void Remove(ITreeItem item)
        {
            this.Items.Remove(item as FrameworkElement);
        }

        public void Clear()
        {
            this.Items.Clear();
        }

        public object GetParent()
        {
            return this.Parent;
        }

        #endregion

        #region IUid

        public string GetUid()
        {
            return this.Uid;
        }

        public void SetUid(string uid)
        {
            this.Uid = uid;
        }

        #endregion

        #region ITag

        public object GetTag()
        {
            return this.Tag;
        }

        public void SetTag(object tag)
        {
            this.Tag = tag;
        }

        #endregion

        #region IData

        public object GetData()
        {
            return null;
        }

        public void SetData(object data)
        {
        }

        #endregion

        #region ISelected

        public bool GetSelected()
        {
            return this.IsSelected;
        }

        public void SetSelected(bool selected)
        {
            this.IsSelected = selected;
        }

        #endregion
    } 

    #endregion
}
