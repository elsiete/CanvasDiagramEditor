﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Enums
{
    #region DxfStyleFlags

    // Group code: 70
    public enum DxfStyleFlags : int
    {
        Default = 0,
        Shape = 1,
        VerticalText = 4,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }

    #endregion
}
