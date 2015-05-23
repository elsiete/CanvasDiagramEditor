// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Core
{
    #region DxfUtil

    public static class DxfUtil
    {
        public static string ToDxfHandle(this int handle)
        {
            return handle.ToString("X");
        }

        public static string ColorToString(this DxfDefaultColors color)
        {
            return ((int)color).ToString();
        }
    }

    #endregion
}
