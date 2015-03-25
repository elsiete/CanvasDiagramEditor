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
    #region DxfVport

    public class DxfVport : DxfObject<DxfVport>
    {
        public string Name { get; set; }
        public DxfVportStandardFlags VportStandardFlags { get; set; }

        public DxfVport(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfVport Defaults()
        {
            Name = string.Empty;
            VportStandardFlags = DxfVportStandardFlags.Default;
            return this;
        }

        public DxfVport Create()
        {
            Add(0, CodeName.Vport);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.ViewportTableRecord);
            }

            Add(2, Name);
            Add(70, (int)VportStandardFlags);

            return this;
        }
    }

    #endregion
}
