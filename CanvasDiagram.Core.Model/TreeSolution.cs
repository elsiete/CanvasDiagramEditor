// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Core.Model
{
    #region TreeSolution

    public class TreeSolution
    {
        public string Name { get; set; }
        public string TagFileName { get; set; }
        public string TableFileName { get; set; }
        public TreeProjects Projects { get; set; }

        public TreeSolution(string name, string tagFileName, string tableFileName, TreeProjects projects)
        {
            Name = name;
            TagFileName = tagFileName;
            TableFileName = tableFileName;
            Projects = projects;
        }
    }

    #endregion
}
