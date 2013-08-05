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

namespace CanvasDiagramEditor.Dxf.Blocks
{
    #region DxfBlock

    public class DxfBlock : DxfObject
    {
        public DxfBlock()
            : base()
        {
        }

        public DxfBlock Begin()
        {
            Add("0", "BLOCK");
            return this;
        }

        public DxfBlock XrefPath(string name)
        {
            Add("1", name);
            return this;
        }

        public DxfBlock Name(string name)
        {
            Add("2", name);
            Add("3", name);
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

        public DxfBlock Add(DxfObject obj)
        {
            Append(obj.ToString());
            return this;
        }

        public DxfBlock End()
        {
            Add("0", "ENDBLK");
            return this;
        }
    }

    #endregion
}
