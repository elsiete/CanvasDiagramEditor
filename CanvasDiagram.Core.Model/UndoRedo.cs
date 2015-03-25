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
