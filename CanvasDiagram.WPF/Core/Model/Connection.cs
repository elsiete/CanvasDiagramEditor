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
    #region Connection

    public class Connection
    {
        public IElement Element { get; set; }
        public List<Wire> Wires { get; set; }

        public Connection(IElement element, List<Wire> wires)
        {
            Element = element;
            Wires = wires;
        }
    }

    #endregion
}
