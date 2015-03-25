// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Core;
using CanvasDiagram.Editor;
using CanvasDiagram.Util;
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
    #region TagEditorControl

    public partial class TagEditorControl : UserControl
    {
        #region Properties

        public List<object> Tags { get; set; }
        public List<IElement> Selected { get; set; }

        #endregion

        #region Fields

        private bool IgnoreSelectionChange = false;

        private Point DragStartPoint;

        #endregion

        #region Constructor

        public TagEditorControl()
        {
            InitializeComponent();

            FilterByDesignation.TextChanged += (sender, e) => FilterTagList();
            FilterBySignal.TextChanged += (sender, e) => FilterTagList();
            FilterByCondition.TextChanged += (sender, e) => FilterTagList();
            FilterByDescription.TextChanged += (sender, e) => FilterTagList();

            SelectedList.SelectionChanged += (sender, e) => SelectedListSelected();

            this.Loaded += (sender, e) =>
            {
                Initialize();
                TagList.Focus();
            };

            this.TagList.PreviewMouseLeftButtonDown += TagList_PreviewMouseLeftButtonDown;
            this.TagList.PreviewMouseMove += TagList_PreviewMouseMove;
        }

        #endregion

        #region Drag & Drop

        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private void TagList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = DragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed && 
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listView = sender as ListView;
                var listViewItem = FindVisualParent<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                    Tag tag = (Tag)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    DataObject dragData = new DataObject("Tag", tag);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            } 
        }

        private void TagList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragStartPoint = e.GetPosition(null);
        }

        #endregion

        #region UserControl Events

        public void Initialize()
        {
            if (Tags != null)
                InsertTags();

            UpdateSelected();
        }

        public void UpdateSelected()
        {
            ClearSelected();

            if (Selected != null)
                InsertSelected();

            FilterTagList();
        }

        public void ClearSelected()
        {
            var list = SelectedList;

            list.Items.Clear();
        }

        public void InsertSelected()
        {
            var list = SelectedList;

            foreach (var element in Selected)
                list.Items.Add(new Tuple<FrameworkElement>(element as FrameworkElement));

            list.SelectedIndex = 0;
        }

        public void InsertTags()
        {
            var list = TagList;

            list.Items.Clear();

            foreach (var tag in Tags)
                list.Items.Add(tag);

            list.SelectedIndex = 0;
        }

        public void FilterTagList()
        {
            string designation = FilterByDesignation.Text.ToUpper();
            string signal = FilterBySignal.Text.ToUpper();
            string condition = FilterByCondition.Text.ToUpper();
            string description = FilterByDescription.Text.ToUpper();

            bool filterByDesignation = designation.Length > 0;
            bool filterBySignal = signal.Length > 0;
            bool filterByCondition = condition.Length > 0;
            bool filterByDescription = description.Length > 0;

            if (filterByDesignation == false &&
                filterBySignal == false &&
                filterByCondition == false &&
                filterByDescription == false)
            {
                IgnoreSelectionChange = true;
                TagList.Items.Filter = null;
            }
            else
            {
                IgnoreSelectionChange = true;
                TagList.Items.Filter = null;

                IgnoreSelectionChange = true;
                TagList.Items.Filter = new Predicate<object>((item) =>
                {
                    bool filter1 = true;
                    bool filter2 = true;
                    bool filter3 = true;
                    bool filter4 = true;

                    var tag = item as Tag;

                    if (filterByDesignation == true)
                        filter1 = tag.Designation.ToUpper().Contains(designation) == true;

                    if (filterBySignal == true)
                        filter2 = tag.Signal.ToUpper().Contains(signal) == true;

                    if (filterByCondition == true)
                        filter3 = tag.Condition.ToUpper().Contains(condition) == true;

                    if (filterByDescription == true)
                        filter4 = tag.Description.ToUpper().Contains(description) == true;

                    return filter1 && filter2 && filter3 && filter4;
                });

                if (TagList.Items.Count > 0)
                    TagList.ScrollIntoView(TagList.Items[0]);
            }
        }

        #endregion

        #region Button Events

        private void CreateNewTag()
        {
            int id = Tags.Count > 0 ? Tags.Cast<Tag>().Max(x => x.Id) + 1 : 0;
            string strId = id.ToString();

            var tag = new Tag()
            {
                Id = id,
                Designation = "Designation" + strId,
                Signal = "Signal" + strId,
                Condition = "Condition" + strId,
                Description = "Description" + strId
            };

            Tags.Add(tag);

            IgnoreSelectionChange = true;

            int index = TagList.Items.Add(tag);

            TagList.SelectedIndex = index;
            TagList.ScrollIntoView(TagList.SelectedItem);
        }

        private void ButtonNewTag_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTag();
        }

        private void ButtonResetFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterByDesignation.Text = "";
            FilterBySignal.Text = "";
            FilterByCondition.Text = "";
            FilterByDescription.Text = "";
        }

        #endregion

        #region ListView Events

        private void SelectedListSelected()
        {
            var item = SelectedList.SelectedItem;
            var tuple = item as Tuple<FrameworkElement>;

            if (item == null || tuple == null)
                return;

            var thumb = tuple.Item1 as IThumb;
            var tag = thumb.GetData() as Tag;
            var canvas = thumb.GetParent() as ICanvas;

            if (canvas != null)
                HistoryEditor.Add(canvas);

            TagList.SelectedItem = tag;
            TagList.ScrollIntoView(TagList.SelectedItem);
        }

        private void TagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreSelectionChange == true)
            {
                IgnoreSelectionChange = false;
                return;
            }

            UpdateSelectedElementTag();
        }

        private void TagList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UpdateSelectedElementTag();
        }

        private void TagList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateSelectedElementTag();
        }

        private void UpdateSelectedElementTag()
        {
            var tag = TagList.SelectedItem as Tag;
            var item = SelectedList.SelectedItem;

            if (item != null)
            {
                var tuple = item as Tuple<FrameworkElement>;
                var element = tuple.Item1;

                ElementThumb.SetData(element, tag);
            }
        }

        #endregion
    } 

    #endregion
}
