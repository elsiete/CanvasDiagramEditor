// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region LineUtil

    public static class LineUtil
    {
        #region Size

        public static double Zet(double startX, double startY, double endX, double endY)
        {
            double alpha = Math.Atan2(startY - endY, endX - startX);
            double theta = Math.PI - alpha;
            return theta - Math.PI / 2;
        }

        public static double Width(double radius, double thickness, double zet)
        {
            return Math.Sin(zet) * (radius + thickness);
        }

        public static double Height(double radius, double thickness, double zet)
        {
            return Math.Cos(zet) * (radius + thickness);
        }

        #endregion

        #region Points

        public static IPoint LineStart(double x, double y, double width, double height, bool visible)
        {
            return visible ?
                new PointEx(x + (2 * width), y - (2 * height)) :
                new PointEx(x, y);
        }

        public static IPoint LineEnd(double x, double y, double width, double height, bool visible)
        {
            return visible ? 
                new PointEx(x - (2 * width), y + (2 * height)) :
                new PointEx(x, y);
        }

        public static IPoint EllipseStart(double x, double y, double width, double height, bool visible)
        {
            return visible ?
                new PointEx(x + width, y - height) :
                new PointEx(x, y);
        }

        public static IPoint EllipseEnd(double x, double y, double width, double height, bool visible)
        {
            return visible ?
                new PointEx(x - width, y + height) :
                new PointEx(x, y);
        }

        #endregion
    }

    #endregion
}
