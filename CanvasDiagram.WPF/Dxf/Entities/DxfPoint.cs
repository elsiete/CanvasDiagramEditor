// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Entities
{
    #region DxfPoint

    public class DxfPoint : DxfObject<DxfPoint>
    {
        public DxfPoint(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion
}
