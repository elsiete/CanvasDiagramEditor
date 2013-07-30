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
    #region DxfLwpolylineVertex

    public class DxfLwpolylineVertex
    {
        public Vector2 Coordinates { get; private set; }
        public double StartWidth { get; private set; }
        public double EndWidth { get; private set; }
        public double Bulge { get; private set; }

        internal DxfLwpolylineVertex(Vector2 coordinates, 
            double startWidth, 
            double endWidth,
            double bulge)
        {
            Coordinates = coordinates;
            StartWidth = startWidth;
            EndWidth = endWidth;
            Bulge = bulge;
        }
    }

    #endregion
}
