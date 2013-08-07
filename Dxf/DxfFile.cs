// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Blocks;
using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Classes;
using CanvasDiagramEditor.Dxf.Entities;
using CanvasDiagramEditor.Dxf.Tables;
using CanvasDiagramEditor.Dxf.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf
{
    #region DxfFile

    public class DxfFile : DxfObject<DxfFile>
    {
        public DxfFile(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfFile Header(DxfHeader header)
        {
            Append(header.ToString());
            return this;
        }

        public DxfFile Classes(DxfClasses classes)
        {
            Append(classes.ToString());
            return this;
        }

        public DxfFile Tables(DxfTables tables)
        {
            Append(tables.ToString());
            return this;
        }

        public DxfFile Blocks(DxfBlocks blocks)
        {
            Append(blocks.ToString());
            return this;
        }

        public DxfFile Entities(DxfEntities entities)
        {
            Append(entities.ToString());
            return this;
        }

        public DxfFile Objects(DxfObjects objects)
        {
            Append(objects.ToString());
            return this;
        }

        public DxfFile Eof()
        {
            Add("0", "EOF");
            return this;
        }
    }

    #endregion
}
