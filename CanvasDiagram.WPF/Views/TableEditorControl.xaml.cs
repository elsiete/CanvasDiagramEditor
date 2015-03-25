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
    #region TableEditorControl

    public partial class TableEditorControl : UserControl
    {
        #region Properties

        public List<object> Tables { get; set; }

        #endregion

        #region Fields

        private Point DragStartPoint;

        #endregion

        #region Constructor

        public TableEditorControl()
        {
            InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                Initialize();
                TableList.Focus();
            };

            this.TableList.PreviewMouseLeftButtonDown += TableList_PreviewMouseLeftButtonDown;
            this.TableList.PreviewMouseMove += TableList_PreviewMouseMove;
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

        private void TableList_PreviewMouseMove(object sender, MouseEventArgs e)
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
                    DiagramTable table = (DiagramTable)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    DataObject dragData = new DataObject("Table", table);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            } 
        }

        private void TableList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragStartPoint = e.GetPosition(null);
        }

        #endregion

        #region UserControl Events

        public void Initialize()
        {
            if (Tables != null)
                InsertTables();
        }

        public void InsertTables()
        {
            var list = TableList;

            list.Items.Clear();

            foreach (var table in Tables)
                list.Items.Add(table);

            list.SelectedIndex = 0;
        }

        #endregion

        #region Button Events

        private void CreateNewTable()
        {
            int id = Tables.Count > 0 ? Tables.Cast<DiagramTable>().Max(x => x.Id) + 1 : 0;
            string strId = id.ToString();

            var table = new DiagramTable()
            {
                Id = id,
                Revision1 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                Revision2 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                Revision3 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                PathLogo1 = "",
                PathLogo2 = "",
                Drawn = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Checked = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Approved = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Title = "LOGIC DIAGRAM",
                SubTitle1 = "DIAGRAM" + strId,
                SubTitle2 = "",
                SubTitle3 = "",
                Rev = "0",
                Status = "-",
                Page = "-",
                Pages = "-",
                Project = "Sample",
                OrderNo = "",
                DocumentNo = "",
                ArchiveNo = ""
            };

            Tables.Add(table);

            int index = TableList.Items.Add(table);

            TableList.SelectedIndex = index;
            TableList.ScrollIntoView(TableList.SelectedItem);
        }

        private void ButtonNewTable_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTable();
        }

        public string GetLogoFileName()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Supported Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.bmp|" +
                        "Png (*.png)|*.png|" +
                        "Jpg (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                        "Tif (*.tif;*.tiff)|*.tif;*.tiff|" +
                        "Bmp (*.bmp)|*.bmp|" +
                        "All Files (*.*)|*.*",
                Title = "Open Logo Image (115x80 @ 96dpi)"
            };

            var res = dlg.ShowDialog();
            if (res == true)
                return dlg.FileName;

            return null;
        }

        private void ButtonSetLogo1_Click(object sender, RoutedEventArgs e)
        {
            var table = TableList.SelectedItem as DiagramTable;
            if (table == null)
                return;

            string fileName = GetLogoFileName();
            if (fileName != null)
            {
                table.PathLogo1 = fileName;

                TableList.SelectedItem = null;
                TableList.SelectedItem = table;
            }
        }

        private void ButtonSetLogo2_Click(object sender, RoutedEventArgs e)
        {
            var table = TableList.SelectedItem as DiagramTable;
            if (table == null)
                return;

            string fileName = GetLogoFileName();
            if (fileName != null)
            {
                table.PathLogo2 = fileName;

                TableList.SelectedItem = null;
                TableList.SelectedItem = table;
            }
        }

        #endregion
    } 

    #endregion
}
