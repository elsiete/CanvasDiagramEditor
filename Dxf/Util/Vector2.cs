// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor.Dxf.Util
{
    #region Vector2

    public class Vector2
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        internal Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    #endregion
}
