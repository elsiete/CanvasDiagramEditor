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
    #region Selection

    public class Selection
    {
        public bool IsSelected { get; set; }
        public List<Wire> Wires { get; set; }

        public Selection(bool selected, List<Wire> wires)
        {
            IsSelected = selected;
            Wires = wires;
        }
    }

    #endregion
}
