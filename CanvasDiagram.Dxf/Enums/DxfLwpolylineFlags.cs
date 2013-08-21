// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Enums
{
    #region DxfLwpolylineFlags

    // Group code: 70
    public enum DxfLwpolylineFlags : int
    {
        Default = 0,
        Closed = 1,
        Plinegen = 128
    }

    #endregion
}
