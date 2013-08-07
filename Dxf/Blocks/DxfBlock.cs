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

namespace CanvasDiagramEditor.Dxf.Blocks
{
    #region DxfBlock

    public class DxfBlock : DxfObject<DxfBlock>
    {
        public DxfBlock(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlock Begin(string name, string layer)
        {
            Add("0", "BLOCK");

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass("AcDbEntity");
            }

            Add("8", layer);

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass("AcDbBlockBegin");
            }

            Add("2", name);
            Add("3", name);

            return this;
        }

        public DxfBlock XrefPath(string name)
        {
            Add("1", name);
            return this;
        }

        public DxfBlock BlockTypeFlags(DxfBlockTypeFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfBlock Base(Vector3 point)
        {
            Add("10", point.X);
            Add("20", point.Y);
            Add("30", point.Z);
            return this;
        }

        public DxfBlock Add<T>(T entity)
        {
            Append(entity.ToString());
            return this;
        }

        public DxfBlock End(int id, string layer)
        {
            Add(0, "ENDBLK");

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.Entity);
                Add(8, layer);
                Subclass(SubclassMarker.BlockEnd);
            }

            return this;
        }
    }

    #endregion
}
