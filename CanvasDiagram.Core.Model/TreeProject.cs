// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

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
