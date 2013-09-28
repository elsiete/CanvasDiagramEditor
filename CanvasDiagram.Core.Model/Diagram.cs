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
    #region Diagram

    public class Diagram
    {
        public string Model { get; set; }
        public UndoRedo History { get; set; }

        public Diagram(string model, UndoRedo history)
        {
            Model = model;
            History = history;
        }
    }

    #endregion
}
