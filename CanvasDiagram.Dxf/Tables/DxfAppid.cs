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
    #region DxfAppid

    public class DxfAppid : DxfObject<DxfAppid>
    {
        public DxfAppid(DxfAcadVer version, int id)
            : base(version, id  )
        {
            Add(0, "APPID");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.RegAppTableRecord);
            }
        }

        public DxfAppid Application(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfAppid StandardFlags(DxfAppidStandardFlags flags)
        {
            Add(70, (int)flags);
            return this;
        }
    }

    #endregion
}
