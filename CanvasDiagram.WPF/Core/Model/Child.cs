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
    #region Child

    public class Child
    {
        public object Element { get; set; }
        public List<Pin> Pins { get; set; }

        public Child(object element, List<Pin> pins)
        {
            Element = element;
            Pins = pins;
        }
    }

    #endregion
}
