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
    #region Wire

    public class Wire
    {
        public object Line { get; set; }
        public IElement Start { get; set; }
        public IElement End { get; set; }

        public Wire(object line, IElement start, IElement end)
        {
            Line = line;
            Start = start;
            End = end;
        }
    }

    #endregion
}
