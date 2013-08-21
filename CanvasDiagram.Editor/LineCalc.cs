// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region LineCalc

    public static class LineCalc
    {
        #region Calculate Size

        public static double CalculateZet(double startX, double startY, double endX, double endY)
        {
            double alpha = Math.Atan2(startY - endY, endX - startX);
            double theta = Math.PI - alpha;
            double zet = theta - Math.PI / 2;
            return zet;
        }

        public static double CalculateSizeX(double radius, double thickness, double zet)
        {
            double sizeX = Math.Sin(zet) * (radius + thickness);
            return sizeX;
        }

        public static double CalculateSizeY(double radius, double thickness, double zet)
        {
            double sizeY = Math.Cos(zet) * (radius + thickness);
            return sizeY;
        }

        #endregion

        #region Get Points

        public static PointEx GetLineStart(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            PointEx lineStart;

            if (isStartVisible)
            {
                double lx = startX + (2 * sizeX);
                double ly = startY - (2 * sizeY);

                lineStart = new PointEx(lx, ly);
            }
            else
            {
                lineStart = new PointEx(startX, startY);
            }

            return lineStart;
        }

        public static PointEx GetLineEnd(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            PointEx lineEnd;

            if (isEndVisible)
            {
                double lx = endX - (2 * sizeX);
                double ly = endY + (2 * sizeY);

                lineEnd = new PointEx(lx, ly);
            }
            else
            {
                lineEnd = new PointEx(endX, endY);
            }

            return lineEnd;
        }

        public static PointEx GetEllipseStartCenter(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            PointEx ellipseStartCenter;

            if (isStartVisible)
            {
                double ex = startX + sizeX;
                double ey = startY - sizeY;

                ellipseStartCenter = new PointEx(ex, ey);
            }
            else
            {
                ellipseStartCenter = new PointEx(startX, startY);
            }

            return ellipseStartCenter;
        }

        public static PointEx GetEllipseEndCenter(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            PointEx ellipseEndCenter;

            if (isEndVisible)
            {
                double ex = endX - sizeX;
                double ey = endY + sizeY;

                ellipseEndCenter = new PointEx(ex, ey);
            }
            else
            {
                ellipseEndCenter = new PointEx(endX, endY);
            }

            return ellipseEndCenter;
        }

        #endregion
    }

    #endregion
}
