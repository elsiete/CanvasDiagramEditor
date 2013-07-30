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
    #region DxfBlockTypeFlags

    // Group code: 70
    public enum DxfBlockTypeFlags : int
    {
        Default = 0,
        Anonymous = 1,
        NonConstantAttributes = 2,
        Xref = 4,
        XrefOverlay = 8,
        Dependant = 16,
        Reference = 32,
        ReferencesSuccess = 64
    }

    #endregion
}
