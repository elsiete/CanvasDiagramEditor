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
    #region DxfInsert

    public class DxfInsert : DxfObject<DxfInsert>
    {
        public DxfInsert(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add("0", "INSERT");

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass("AcDbLine");
            }
        }

        public DxfInsert Block(string name)
        {
            Add("2", name);
            return this;
        }

        public DxfInsert Layer(string layer)
        {
            Add("8", layer);
            return this;
        }

        public DxfInsert Insertion(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfInsert Scale(Vector3 factor)
        {
            Add("41", factor.X);
            Add("42", factor.Y);
            Add("43", factor.Z);
            return this;
        }

        public DxfInsert Rotation(double angle)
        {
            Add("50", angle);
            return this;
        }

        public DxfInsert Columns(int count)
        {
            Add("70", count);
            return this;
        }

        public DxfInsert Rows(int count)
        {
            Add("71", count);
            return this;
        }

        public DxfInsert ColumnSpacing(double value)
        {
            Add("44", value);
            return this;
        }

        public DxfInsert RowSpacing(double value)
        {
            Add("45", value);
            return this;
        }

        public DxfInsert AttributesBegin()
        {
            Add("66", "1"); // attributes follow: 0 - no, 1 - yes
            return this;
        }

        public DxfInsert AddAttribute(DxfAttrib attrib)
        {
            Append(attrib.ToString());
            return this;
        }

        public DxfInsert AttributesEnd()
        {
            Add("0", "SEQEND");
            return this;
        }
    }

    #endregion
}
