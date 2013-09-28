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
    #region UndoRedo

    public class UndoRedo
    {
        public Stack<string> Undo { get; set; }
        public Stack<string> Redo { get; set; }

        public UndoRedo(Stack<string> undo, Stack<string> redo)
        {
            Undo = undo;
            Redo = redo;
        }
    }

    #endregion
}
