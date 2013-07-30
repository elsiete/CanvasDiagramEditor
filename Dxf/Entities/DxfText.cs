// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfText

    public class DxfText : DxfObject
    {
        public DxfText()
            : base()
        {
            Add("0", "TEXT");
        }

        public DxfText Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfText Thickness(double thickness)
        {
            Add("39", thickness);
            return this;
        }

        public DxfText Text(string text)
        {
            Add("1", text);
            return this;
        }

        public DxfText TextStyle(string style)
        {
            Add("7", style);
            return this;
        }

        public DxfText TextHeight(double height)
        {
            Add("40", height);
            return this;
        }

        public DxfText TextRotation(double rotation)
        {
            Add("50", rotation);
            return this;
        }

        public DxfText ObliqueAngle(double angle)
        {
            Add("51", angle);
            return this;
        }

        public DxfText ScaleFactorX(double factor)
        {
            Add("41", factor);
            return this;
        }

        public DxfText FirstAlignment(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfText SecondAlignment(Vector3 point)
        {
            Add("11", point.X);
            Add("21", point.Y);
            Add("31", point.Z);
            return this;
        }

        public DxfText Extrusion(Vector3 direction)
        {
            Add("210", direction.X);
            Add("220", direction.Y);
            Add("230", direction.Z);
            return this;
        }

        public DxfText TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add("71", (int)flags);
            return this;
        }

        public DxfText HorizontalTextJustification(DxfHorizontalTextJustification justification)
        {
            Add("72", (int)justification);
            return this;
        }

        public DxfText VerticalTextJustification(DxfVerticalTextJustification justification)
        {
            Add("73", (int)justification);
            return this;
        }


    } 
    
    #endregion
}
