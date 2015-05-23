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
