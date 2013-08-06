// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfLine

    public class DxfLine : DxfObject<DxfLine>
    {
        public string Layer { get; set; }
        public string Color { get; set; }
        public double Thickness { get; set; }
        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }
        public Vector3 ExtrusionDirection { get; set; }

        public DxfLine(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLine Defaults()
        {
            Layer = "0";
            Color = "0";
            Thickness = 0.0;
            StartPoint = new Vector3(0.0, 0.0, 0.0);
            EndPoint = new Vector3(0.0, 0.0, 0.0);
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);

            return this;
        }

        public DxfLine Create()
        {
            Add(0, CodeName.Line);

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Line);

            Add(8, Layer);
            Add(62, Color);
            Add(39, Thickness);

            Add(10, StartPoint.X);
            Add(20, StartPoint.Y);
            Add(30, StartPoint.Z);

            Add(11, EndPoint.X);
            Add(21, EndPoint.Y);
            Add(31, EndPoint.Z);
            
            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            return this;
        }
    } 

    #endregion
}
