// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

#endregion

namespace CanvasDiagram.Editor
{
    #region Table

    public class Revision
    {
        public string Version { get; set; }
        public string Date { get; set; }
        public string Remarks { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public string Date { get; set; }
    }

    public class DiagramTable
    {
        #region Properties

        public int Id { get; set; }

        public Revision Revision1 { get; set; }
        public Revision Revision2 { get; set; }
        public Revision Revision3 { get; set; }

        //public ImageSource Logo1 { get; set; }
        //public ImageSource Logo2 { get; set; }

        public string PathLogo1 { get; set; }
        public string PathLogo2 { get; set; }

        public Person Drawn { get; set; }
        public Person Checked { get; set; }
        public Person Approved { get; set; }

        public string Title { get; set; }
        public string SubTitle1 { get; set; }
        public string SubTitle2 { get; set; }
        public string SubTitle3 { get; set; }

        public string Rev { get; set; }
        public string Status { get; set; }
        public string Page { get; set; }
        public string Pages { get; set; }
        public string Project { get; set; }
        public string OrderNo { get; set; }
        public string DocumentNo { get; set; }
        public string ArchiveNo { get; set; }

        #endregion
    }

    #endregion
}
