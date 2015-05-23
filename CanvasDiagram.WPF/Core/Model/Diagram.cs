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
