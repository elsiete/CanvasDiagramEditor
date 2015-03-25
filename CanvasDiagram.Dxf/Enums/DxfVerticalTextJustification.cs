// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Enums
{
    #region DxfVerticalTextJustification

    // Group code: 73
    public enum DxfVerticalTextJustification : int
    {
        Default = 0,
        Baseline = 0,
        Bottom = 1,
        Middle = 2,
        Top = 3
    }

    #endregion
}
