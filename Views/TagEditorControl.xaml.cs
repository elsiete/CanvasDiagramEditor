// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Util;
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

namespace CanvasDiagramEditor
{
    #region TagEditorControl

    public partial class TagEditorControl : UserControl
    {
        #region Properties

        public List<object> Tags { get; set; }
        public List<IElement> Selected { get; set; }

        #endregion

        #region Constructor

        public TagEditorControl()
        {
            InitializeComponent();

            this.Loaded += TagEditorControl_Loaded;
        }

        #endregion

        #region UserControl Events

        void TagEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Tags != null)
            {
                InsertTags();
            }

            if (Selected != null)
            {
                InsertSelected();
            }

            FilterTagList();
        } 

        private void InsertSelected()
        {
            var list = SelectedList;

            foreach (var element in Selected)
            {
                list.Items.Add(new Tuple<FrameworkElement>(element as FrameworkElement));
            }

            list.SelectedIndex = 0;
        }

        private void InsertTags()
        {
            var list = TagList;

            foreach (var tag in Tags)
            {
                list.Items.Add(tag);
            }

            list.SelectedIndex = 0;
        }

        #endregion

        #region Button Events

        private void ButtonNewTag_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTag();
        }

        private bool IgnoreSelectionChange = false;

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

        #endregion

        #region ListView Events

        private void SelectedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedListSelected();
        }

        private void SelectedListSelected()
        {
            var item = SelectedList.SelectedItem;
            var tuple = item as Tuple<FrameworkElement>;
            var element = tuple.Item1;

            var tag = ElementThumb.GetData(element) as Tag;

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
            {
                UpdateSelectedElementTag();
            }
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

        #region Filter TagList

        private void FilterTagList()
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
                TagList.Items.Filter = new Predicate<object>(
                    (item) =>
                    {
                        bool filter1 = true;
                        bool filter2 = true;
                        bool filter3 = true;
                        bool filter4 = true;

                        var tag = item as Tag;

                        if (filterByDesignation == true)
                        {
                            filter1 = tag.Designation.ToUpper().Contains(designation) == true;
                        }

                        if (filterBySignal == true)
                        {
                            filter2 = tag.Signal.ToUpper().Contains(signal) == true;
                        }

                        if (filterByCondition == true)
                        {
                            filter3 = tag.Condition.ToUpper().Contains(condition) == true;
                        }

                        if (filterByDescription == true)
                        {
                            filter4 = tag.Description.ToUpper().Contains(description) == true;
                        }

                        return filter1 && filter2 && filter3 && filter4;
                    });

                if (TagList.Items.Count > 0)
                {
                    TagList.ScrollIntoView(TagList.Items[0]);
                }
            }
        }

        private void FilterByDesignation_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTagList();
        }

        private void FilterBySignal_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTagList();
        }

        private void FilterByCondition_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTagList();
        }

        private void FilterByDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTagList();
        }

        #endregion
    } 

    #endregion
}
