// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor.Dxf
{
    #region DxfPoint3

    public class DxfPoint3
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        internal DxfPoint3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    #endregion
}
