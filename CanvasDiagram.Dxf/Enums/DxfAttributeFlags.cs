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
    #region DxfAttributeFlags

    // Group code: 70
    public enum DxfAttributeFlags : int
    {
        Default = 0,
        Invisible = 1,
        Constant = 2,
        Verification = 4,
        Preset = 8
    }

    #endregion
}
