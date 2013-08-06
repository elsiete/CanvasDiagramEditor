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
    #region DxfCircle

    public class DxfCircle : DxfObject<DxfCircle>
    {
        public DxfCircle(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add("0", "CIRCLE");

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass("AcDbCircle");
            }
        }

        public DxfCircle Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfCircle Color(string color)
        {
            Add("62", color);
            return this;
        }

        public DxfCircle Thickness(double thickness)
        {
            Add("39", thickness);
            return this;
        }

        public DxfCircle Radius(double radius)
        {
            Add("40", radius);
            return this;
        }

        public DxfCircle Center(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfCircle Extrusion(Vector3 direction)
        {
            Add("210", direction.X);
            Add("220", direction.Y);
            Add("230", direction.Z);
            return this;
        }
    } 

    #endregion
}
