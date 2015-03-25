// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Entities
{
    #region DxfLwpolyline

    public class DxfLwpolyline : DxfObject<DxfLwpolyline>
    {
        public DxfLwpolyline(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    /*

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

    public static string DxfLwpolyline(int numberOfVertices,
        DxfLwpolylineFlags lwpolylineFlags,
        double constantWidth,
        double elevation,
        double thickness,
        DxfLwpolylineVertex [] vertices,
        Vector3 extrusionDirection,
        string layer,
        string color)
    {
        var b = new DxfBuilder();

        // lwpolyline
        b.Add(0, "LWPOLYLINE");

        // layer
        if (layer != null)
            b.Add(8, layer);

        // color
        if (color != null)
            b.Add(62, color);

        // number of vertices
        b.Add(90, numberOfVertices);

        // polyline flags
        if (lwpolylineFlags != DxfLwpolylineFlags.Default)
            b.Add(70, (int)lwpolylineFlags);

        // constant width
        if (constantWidth != 0.0)
            b.Add(43, constantWidth);

        // elevation 
        if (elevation != 0.0)
            b.Add(38, elevation);

        // thickness 
        if (thickness != 0.0)
            b.Add(39, thickness);
 
        if (vertices != null)
        {
            // vertices
            foreach(var vertex in vertices)
            {
                b.Add(10, vertex.Coordinates.X);
                b.Add(20, vertex.Coordinates.Y);

                if (constantWidth == 0.0)
                {
                    // starting width
                    if (vertex.StartWidth != 0.0)
                        b.Add(40, vertex.StartWidth);

                    // end width
                    if (vertex.EndWidth != 0.0)
                        b.Add(41, vertex.EndWidth);
                }

                // bulge
                if (vertex.Bulge != 0.0)
                    b.Add(42, vertex.Bulge);
            }
        }

        if (extrusionDirection != null)
        {
            b.Add(210, extrusionDirection.X);
            b.Add(220, extrusionDirection.Y);
            b.Add(230, extrusionDirection.Z);
        }

        return b.Build();
    }
     * 
    */
}
