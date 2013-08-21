﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Blocks
{
    #region DxfBlock

    public class DxfBlock : DxfObject<DxfBlock>
    {
        public string Name { get; set; }
        public string Layer { get; set; }
        public DxfBlockTypeFlags BlockTypeFlags { get; set; }
        public Vector3 BasePoint { get; set; }
        public string XrefPathName { get; set; }
        public string Description { get; set; }
        public int EndId { get; set; }
        public string EndLayer { get; set; }
        public List<object> Entities { get; set; }

        public DxfBlock(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlock Defaults()
        {
            Name = string.Empty;
            Layer = "0";
            BlockTypeFlags = DxfBlockTypeFlags.Default;
            BasePoint = new Vector3(0.0, 0.0, 0.0);
            XrefPathName = null;
            Description = null;
            EndId = 0;
            EndLayer = "0";
            Entities = null;

            return this;
        }

        public DxfBlock Create()
        {
            Add(0, CodeName.Block);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.Entity);
            }

            Add(8, Layer);

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass(SubclassMarker.BlockBegin);
            }

            Add(2, Name);
            Add(70, (int)BlockTypeFlags);

            Add(10, BasePoint.X);
            Add(20, BasePoint.Y);
            Add(30, BasePoint.Z);

            Add(3, Name);

            if (XrefPathName != null)
            {
                Add(1, XrefPathName);
            }

            if (Version > DxfAcadVer.AC1014 && Description != null)
            {
                Add(4, Description);
            }

            if (Entities != null)
            {
                foreach (var entity in Entities)
                {
                    Append(entity.ToString());
                }
            }

            Add(0, CodeName.Endblk);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(EndId);
                Subclass(SubclassMarker.Entity);
                Add(8, EndLayer);
                Subclass(SubclassMarker.BlockEnd);
            }

            return this;
        }
    }

    #endregion
}
