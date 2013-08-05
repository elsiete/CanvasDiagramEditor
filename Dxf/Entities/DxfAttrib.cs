// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfAttrib

    public class DxfAttrib : DxfEntity
    {
        public DxfAttrib()
            : base()
        {
            Add("0", "ATTRIB");
        }

        public DxfAttrib Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfAttrib Thickness(double thickness)
        {
            Add("39", thickness);
            return this;
        }

        public DxfAttrib DefaultValue(string value)
        {
            Add("1", value);
            return this;
        }

        public DxfAttrib Tag(string tag)
        {
            Add("2", tag);
            return this;
        }

        public DxfAttrib TextStyle(string style)
        {
            Add("7", style);
            return this;
        }

        public DxfAttrib TextHeight(double height)
        {
            Add("40", height);
            return this;
        }

        public DxfAttrib TextRotation(double rotation)
        {
            Add("50", rotation);
            return this;
        }

        public DxfAttrib ObliqueAngle(double angle)
        {
            Add("51", angle);
            return this;
        }

        public DxfAttrib ScaleFactorX(double factor)
        {
            Add("41", factor);
            return this;
        }

        public DxfAttrib FirstAlignment(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfAttrib SecondAlignment(Vector3 point)
        {
            Add("11", point.X);
            Add("21", point.Y);
            Add("31", point.Z);
            return this;
        }

        public DxfAttrib Extrusion(Vector3 direction)
        {
            Add("210", direction.X);
            Add("220", direction.Y);
            Add("230", direction.Z);
            return this;
        }

        public DxfAttrib AttributeFlags(DxfAttributeFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfAttrib TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add("71", (int)flags);
            return this;
        }

        public DxfAttrib HorizontalTextJustification(DxfHorizontalTextJustification justification)
        {
            Add("72", (int)justification);
            return this;
        }

        public DxfAttrib VerticalTextJustification(DxfVerticalTextJustification justification)
        {
            Add("74", (int)justification);
            return this;
        }
    } 

    #endregion
}
