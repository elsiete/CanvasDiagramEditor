// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Tables
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
