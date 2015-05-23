// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Core
{
    #region PointEx

    public class PointEx : IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointEx(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    #endregion
}
