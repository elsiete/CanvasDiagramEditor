// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Enums
{
    #region DxfLayerStandardFlags

    // Group code: 70
    public enum DxfLayerStandardFlags : int
    {
        Default = 0,
        Frozen = 1,
        FrozenByDefault = 2,
        Locked = 4,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion
}
