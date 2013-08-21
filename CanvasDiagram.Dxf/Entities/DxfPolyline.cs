// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Entities
{
    #region DxfPolyline

    public class DxfPolyline : DxfObject<DxfPolyline>
    {
        public DxfPolyline(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion
}
