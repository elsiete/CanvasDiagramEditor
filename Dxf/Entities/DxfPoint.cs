﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfPoint

    public class DxfPoint : DxfObject<DxfPoint>
    {
        public DxfPoint(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion
}
