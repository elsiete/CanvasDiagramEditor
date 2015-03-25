// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Core;
using CanvasDiagram.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 

#endregion

namespace CanvasDiagram.WPF
{
    #region ExplorerControl

    public partial class ExplorerControl : UserControl
    {
        #region Properties

        public DiagramEditor Editor { get; set; }
        public FrameworkElement DiagramView { get; set; }
        public FrameworkElement ProjectView { get; set; }
        public FrameworkElement SolutionView { get; set; }

        #endregion

        #region Constructor

        public ExplorerControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Create Items

        public ITreeItem CreateTreeDiagramItem()
        {
            var diagram = new SolutionTreeViewItem();

            diagram.Header = Constants.TagHeaderDiagram;
            diagram.ContextMenu = this.Resources["DiagramContextMenuKey"] as ContextMenu;
            diagram.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            return diagram as ITreeItem;
        }

        public ITreeItem CreateTreeProjectItem()
        {
            var project = new SolutionTreeViewItem();

            project.Header = Constants.TagHeaderProject;
            project.ContextMenu = this.Resources["ProjectContextMenuKey"] as ContextMenu;
            project.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;
            project.IsExpanded = true;

            return project as ITreeItem;
        }

        public ITreeItem CreateTreeSolutionItem()
        {
            var solution = new SolutionTreeViewItem();

            solution.Header = Constants.TagHeaderSolution;
            solution.ContextMenu = this.Resources["SolutionContextMenuKey"] as ContextMenu;
            solution.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;
            solution.IsExpanded = true;

            return solution as ITreeItem;
        }

        #endregion

        #region TreeView Events

        private void SolutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Editor == null)
                return;

            var type = TreeEditor.SwitchItems(Editor.Context.CurrentCanvas, 
                Editor.Context.DiagramCreator, 
                e.OldValue as ITreeItem, 
                e.NewValue as ITreeItem, 
                Editor.Context.SetProperties);

            if (DiagramView != null)
                DiagramView.Visibility = (type == TreeItemType.Diagram) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region TreeViewItem Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as SolutionTreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                item.BringIntoView();
                e.Handled = true;
            }
        }

        #endregion

        #region TreeViewItem ContextMenu Events

        private void SolutionAddProject_Click(object sender, RoutedEventArgs e)
        {
            TreeEditor.AddProject(SolutionTree.SelectedItem as SolutionTreeViewItem, 
                Editor.Context.CreateProject, 
                Editor.Context.CurrentCanvas.GetCounter());
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.Create();
        }

        private void ProjectAddDiagramAndPaste_Click(object sender, RoutedEventArgs e)
        {
            Editor.CreateAndPaste();
        }

        private void DiagramAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.Create();
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            TreeEditor.DeleteProject(SolutionTree.SelectedItem as SolutionTreeViewItem);
        }

        private void DiagramDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            TreeEditor.DeleteDiagram(SolutionTree.SelectedItem as SolutionTreeViewItem);
        }

        private void DiagramAddDiagramAndPaste_Click(object sender, RoutedEventArgs e)
        {
            Editor.CreateAndPaste();
        }

        #endregion
    }

    #endregion
}
