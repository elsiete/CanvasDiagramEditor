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
