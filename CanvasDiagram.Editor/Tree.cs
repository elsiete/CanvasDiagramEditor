// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region Aliases

    using MapPin = Tuple<string, string>;
    using MapWire = Tuple<object, object, object>;
    using MapWires = Tuple<object, List<Tuple<string, string>>>;
    using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    using UndoRedo = Tuple<Stack<string>, Stack<string>>;
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>;
    using TreeProject = Tuple<string, Stack<Stack<string>>>;
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    using TreeSolution = Tuple<string, string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    using Position = Tuple<double, double>;
    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    using Solution = Tuple<string, IEnumerable<string>>;

    #endregion

    #region Tree

    public static class Tree
    {
        #region Tree

        public static TreeItemType GetTreeItemType(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return TreeItemType.None;

            if (StringUtil.StartsWith(uid, Constants.TagHeaderSolution))
                return TreeItemType.Solution;

            if (StringUtil.StartsWith(uid, Constants.TagHeaderProject))
                return TreeItemType.Project;

            if (StringUtil.StartsWith(uid, Constants.TagHeaderDiagram))
                return TreeItemType.Diagram;

            return TreeItemType.None;
        }

        public static void SelectPreviousItem(ITree tree, bool selectParent)
        {
            // get current diagram
            var selected = tree.GetSelectedItem() as ITreeItem;

            if (selected != null &&
                StringUtil.StartsWith(selected.GetUid(), Constants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.GetParent() as ITreeItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    int index = parent.GetItemIndex(selected);
                    int count = parent.GetItemsCount();

                    // use '<' key for navigation in tree (project scope)
                    if (count > 0 && index > 0)
                    {
                        // select previous diagram
                        index = index - 1;

                        var item = parent.GetItem(index) as ITreeItem;
                        item.SetSelected(true);
                        item.PushIntoView();
                    }

                    // use 'Ctrl + <' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        SelectPreviousParentItem(parent);
                    }
                }
            }
        }

        public static void SelectPreviousParentItem(ITreeItem parent)
        {
            // get parent of current project
            var parentParent = parent.GetParent() as ITreeItem;
            int parentIndex = parentParent.GetItemIndex(parent);
            int parentCount = parentParent.GetItemsCount();

            if (parentCount > 0 && parentIndex > 0)
                SelectLastItemInPreviousProject(parentParent, parentIndex);
        }

        public static void SelectLastItemInPreviousProject(ITreeItem parentParent, int parentIndex)
        {
            // get previous project
            int index = parentIndex - 1;
            var parentProject = parentParent.GetItem(index);
            int count = parentProject.GetItemsCount();

            // select last item in previous project
            if (count > 0)
            {
                var item = parentProject.GetItem(count - 1);

                item.SetSelected(true);
                item.PushIntoView();
            }
        }

        public static void SelectNextItem(ITree tree, bool selectParent)
        {
            // get current diagram
            var selected = tree.GetSelectedItem() as ITreeItem;

            if (selected != null &&
                StringUtil.StartsWith(selected.GetUid(), Constants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.GetParent() as ITreeItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    int index = parent.GetItemIndex(selected);
                    int count = parent.GetItemsCount();

                    // use '>' key for navigation in tree (project scope)
                    if (count > 0 && index < count - 1)
                    {
                        // select next diagram
                        index = index + 1;

                        var item = parent.GetItem(index);
                        item.SetSelected(true);
                        item.PushIntoView();
                    }

                    // use 'Ctrl + >' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        SelectNextParentItem(parent);
                    }
                }
            }
        }

        public static void SelectNextParentItem(ITreeItem parent)
        {
            // get parent of current project
            var parentParent = parent.GetParent() as ITreeItem;
            int parentIndex = parentParent.GetItemIndex(parent);
            int parentCount = parentParent.GetItemsCount();

            if (parentCount > 0 && parentIndex < parentCount - 1)
                SelectFirstItemInNextProject(parentParent, parentIndex);
        }

        public static void SelectFirstItemInNextProject(ITreeItem parentParent, int parentIndex)
        {
            // get next project
            int index = parentIndex + 1;
            var parentProject = parentParent.GetItem(index);

            // select first item in next project
            if (parentProject.GetItemsCount() > 0)
            {
                var item = parentProject.GetItem(0);
                item.SetSelected(true);
                item.PushIntoView();
            }
        }

        public static TreeItemType SwitchItems(ICanvas canvas,
            IDiagramCreator creator,
            ITreeItem oldItem, 
            ITreeItem newItem,
            Action<DiagramProperties> setProperties)
        {
            if (newItem == null)
                return TreeItemType.None;

            string oldUid = oldItem == null ? null : oldItem.GetUid();
            string newUid = newItem == null ? null : newItem.GetUid();
            bool isOldItemDiagram = oldUid == null ? false : StringUtil.StartsWith(oldUid, Constants.TagHeaderDiagram);
            bool isNewItemDiagram = newUid == null ? false : StringUtil.StartsWith(newUid, Constants.TagHeaderDiagram);
            var oldItemType = GetTreeItemType(oldUid);
            var newItemType = GetTreeItemType(newUid);

            if (oldItemType == TreeItemType.Diagram)
                Model.Store(canvas, oldItem);

            if (newItemType == TreeItemType.Diagram)
            {
                Model.Load(canvas, creator, newItem);

                if (setProperties != null)
                    setProperties(canvas.GetProperties());
            }

            return newItemType;
        }

        public static ITreeItem CreateSolutionItem(string uid, 
            Func<ITreeItem> createSolution, 
            IdCounter counter)
        {
            var solution = createSolution();

            if (uid == null)
            {
                int id = counter.Next();

                solution.SetUid(Constants.TagHeaderSolution + Constants.TagNameSeparator + id.ToString());
            }
            else
            {
                solution.SetUid(uid);
            }

            return solution;
        }

        public static ITreeItem CreateProjectItem(string uid,
            Func<ITreeItem> createProject, 
            IdCounter counter)
        {
            var project = createProject();

            if (uid == null)
            {
                int id = counter.Next();

                project.SetUid(Constants.TagHeaderProject + Constants.TagNameSeparator + id.ToString());
            }
            else
            {
                project.SetUid(uid);
            }

            return project;
        }

        public static ITreeItem CreateDiagramItem(string uid,
            Func<ITreeItem> createDiagram, 
            IdCounter counter)
        {
            var diagram = createDiagram();

            if (uid == null)
            {
                int id = counter.Next();

                diagram.SetUid(Constants.TagHeaderDiagram + Constants.TagNameSeparator + id.ToString());
            }
            else
            {
                diagram.SetUid(uid);
            }

            return diagram;
        }

        public static TreeItemType AddNewItem(ITree tree,
            Func<ITreeItem> createProject,
            Func<ITreeItem> createDiagram,
            IdCounter counter)
        {
            var selected = tree.GetSelectedItem() as ITreeItem;

            string uid = selected.GetUid();
            var type = GetTreeItemType(uid);

            if (type == TreeItemType.Diagram)
            {
                var project = selected.GetParent() as ITreeItem;
                AddDiagram(project, true, createDiagram, counter);
                return TreeItemType.Diagram;
            }
            else if (type == TreeItemType.Project)
            {
                AddDiagram(selected, false, createDiagram, counter);
                return TreeItemType.Diagram;
            }
            else if (type == TreeItemType.Solution)
            {
                AddProject(selected, createProject, counter);
                return TreeItemType.Project;
            }

            return TreeItemType.None;
        }

        public static void AddProject(ITreeItem solution,
            Func<ITreeItem> createProject,
            IdCounter counter)
        {
            var project = CreateProjectItem(null, createProject, counter);

            solution.Add(project);
        }

        public static void AddDiagram(ITreeItem project,
            bool select,
            Func<ITreeItem> createDiagram,
            IdCounter counter)
        {
            var diagram = CreateDiagramItem(null, createDiagram, counter);

            project.Add(diagram);

            Model.Store(null, diagram);

            if (select == true)
                diagram.SetSelected(true);
        }

        public static void DeleteSolution(ITreeItem solution)
        {
            var tree = solution.GetParent() as ITree;
            var projects = solution.GetItems().ToList();

            foreach (var project in projects)
            {
                var diagrams = project.GetItems().ToList();

                foreach (var diagram in diagrams)
                    project.Remove(diagram);

                solution.Remove(project);
            }

            tree.Remove(solution as ITreeItem);
        }

        public static void DeleteProject(ITreeItem project)
        {
            var solution = project.GetParent() as ITreeItem;
            var diagrams = project.GetItems().ToList();

            foreach (var diagram in diagrams)
                project.Remove(diagram);

            solution.Remove(project);
        }

        public static void DeleteDiagram(ITreeItem diagram)
        {
            var project = diagram.GetParent() as ITreeItem;

            project.Remove(diagram);
        }

        public static void Clear(ITree tree)
        {
            var items = tree.GetItems().ToList();

            foreach (var item in items)
                DeleteSolution(item);
        }

        public static void CreateNewSolution(ITree tree,
            ICanvas canvas,
            Func<ITreeItem> createSolution,
            Func<ITreeItem> createProject,
            Func<ITreeItem> createDiagram,
            IdCounter counter)
        {
            CreateDefaultSolution(tree,
                createSolution,
                createProject,
                createDiagram,
                counter);
        }

        public static void CreateDefaultSolution(ITree tree,
            Func<ITreeItem> createSolution,
            Func<ITreeItem> createProject,
            Func<ITreeItem> createDiagram,
            IdCounter counter)
        {
            var solutionItem = CreateSolutionItem(null, createSolution, counter);
            tree.Add(solutionItem);

            var projectItem = CreateProjectItem(null, createProject, counter);
            solutionItem.Add(projectItem);

            var diagramItem = CreateDiagramItem(null, createDiagram, counter);
            projectItem.Add(diagramItem);

            diagramItem.SetSelected(true);
        }

        #endregion
    }

    #endregion
}
