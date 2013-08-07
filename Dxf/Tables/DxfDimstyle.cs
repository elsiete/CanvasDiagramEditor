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
    #region DxfDimstyle

    public class DxfDimstyle : DxfObject<DxfDimstyle>
    {
        public string Name { get; set; }
        public DxfDimstyleStandardFlags DimstyleStandardFlags { get; set; }

        public DxfDimstyle(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfDimstyle Defaults()
        {
            Name = string.Empty;
            DimstyleStandardFlags = DxfDimstyleStandardFlags.Default;
            return this;
        }

        public DxfDimstyle Create()
        {
            Add(0, CodeName.Dimstyle);

            if (Version > DxfAcadVer.AC1009)
            {
                Add(105, Id.ToDxfHandle()); // Dimstyle handle code is 105 instead of 5
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.DimStyleTableRecord);
            }

            Add(2, Name);
            Add(70, (int)DimstyleStandardFlags);

            return this;
        }
    }

    #endregion
}
