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
