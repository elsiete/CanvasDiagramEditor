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
    #region TreeProject

    public class TreeProject
    {
        public string Name { get; set; }
        public TreeDiagrams Diagrams { get; set; }

        public TreeProject(string name, TreeDiagrams diagrams)
        {
            Name = name;
            Diagrams = diagrams;
        }
    }

    #endregion
}
