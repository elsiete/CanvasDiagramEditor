// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Tables
{
    #region DxfVport

    public class DxfVport : DxfObject<DxfVport>
    {
        public DxfVport(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion
}
