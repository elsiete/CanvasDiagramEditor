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
    #region DxfAppidStandardFlags

    // Group code: 70
    public enum DxfAppidStandardFlags : int
    {
        Default = 0,
        IgnoreXdata = 1,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion
}
