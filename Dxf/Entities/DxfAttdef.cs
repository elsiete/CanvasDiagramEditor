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
    #region DxfAttdef

    public class DxfAttdef : DxfObject<DxfAttdef>
    {
        public DxfAttdef(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add("0", "ATTDEF");

            Entity();

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass("AcDbText");
            }
        }

        public DxfAttdef Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfAttdef Thickness(double thickness)
        {
            Add("39", thickness);
            return this;
        }

        public DxfAttdef DefaultValue(string value)
        {
            Add("1", value);
            return this;
        }

        public DxfAttdef Tag(string tag)
        {
            Add("2", tag);
            return this;
        }

        public DxfAttdef Prompt(string prompt)
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Subclass("AcDbAttributeDefinition");
            }

            Add("3", prompt);
            return this;
        }

        public DxfAttdef TextStyle(string style)
        {
            Add("7", style);
            return this;
        }

        public DxfAttdef TextHeight(double height)
        {
            Add("40", height);
            return this;
        }

        public DxfAttdef TextRotation(double rotation)
        {
            Add("50", rotation);
            return this;
        }

        public DxfAttdef ObliqueAngle(double angle)
        {
            Add("51", angle);
            return this;
        }

        public DxfAttdef ScaleFactorX(double factor)
        {
            Add("41", factor);
            return this;
        }

        public DxfAttdef FirstAlignment(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfAttdef SecondAlignment(Vector3 point)
        {
            Add("11", point.X);
            Add("21", point.Y);
            Add("31", point.Z);
            return this;
        }

        public DxfAttdef Extrusion(Vector3 direction)
        {
            Add("210", direction.X);
            Add("220", direction.Y);
            Add("230", direction.Z);
            return this;
        }

        public DxfAttdef AttributeFlags(DxfAttributeFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfAttdef TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add("71", (int)flags);
            return this;
        }

        public DxfAttdef HorizontalTextJustification(DxfHorizontalTextJustification justification)
        {
            Add("72", (int)justification);
            return this;
        }

        public DxfAttdef VerticalTextJustification(DxfVerticalTextJustification justification)
        {
            Add("74", (int)justification);
            return this;
        }
    } 

    #endregion
}
