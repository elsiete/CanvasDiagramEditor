// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfLine

    public class DxfLine : DxfEntity
    {
        public DxfLine()
            : base()
        {
            Add("0", "LINE");
        }

        public DxfLine Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfLine Color(string color)
        {
            Add("62", color);
            return this;
        }

        public DxfLine Thickness(double thickness)
        {
            Add("39", thickness);
            return this;
        }

        public DxfLine Start(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfLine End(Vector3 point)
        {
            Add("11", point.X);
            Add("21", point.Y);
            Add("31", point.Z);
            return this;
        }

        public DxfLine Extrusion(Vector3 direction)
        {
            Add("210", direction.X);
            Add("220", direction.Y);
            Add("230", direction.Z);
            return this;
        }
    } 

    #endregion
}
