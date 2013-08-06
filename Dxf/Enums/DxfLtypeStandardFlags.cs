﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Enums
{
    #region DxfLtypeStandardFlags

    // Group code: 70
    public enum DxfLtypeStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }

    #endregion
}
