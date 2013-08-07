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

namespace CanvasDiagramEditor.Dxf.Tables
{
    #region DxfBlockRecord

    public class DxfBlockRecord : DxfObject<DxfBlockRecord>
    {
        public string Name { get; set; }

        public DxfBlockRecord(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlockRecord Defaults()
        {
            Name = string.Empty;
            return this;
        }

        public DxfBlockRecord Create()
        {
            Add(0, CodeName.BlockRecord);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.BlockTableRecord);
            }

            Add(2, Name);

            return this;
        }
    }


    #endregion
}
