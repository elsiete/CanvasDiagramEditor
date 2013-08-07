// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows; 

#endregion

namespace CanvasDiagramEditor.Editor
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

        public static Point GetLineStart(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            Point lineStart;

            if (isStartVisible)
            {
                double lx = startX + (2 * sizeX);
                double ly = startY - (2 * sizeY);

                lineStart = new Point(lx, ly);
            }
            else
            {
                lineStart = new Point(startX, startY);
            }

            return lineStart;
        }

        public static Point GetLineEnd(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            Point lineEnd;

            if (isEndVisible)
            {
                double lx = endX - (2 * sizeX);
                double ly = endY + (2 * sizeY);

                lineEnd = new Point(lx, ly);
            }
            else
            {
                lineEnd = new Point(endX, endY);
            }

            return lineEnd;
        }

        public static Point GetEllipseStartCenter(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            Point ellipseStartCenter;

            if (isStartVisible)
            {
                double ex = startX + sizeX;
                double ey = startY - sizeY;

                ellipseStartCenter = new Point(ex, ey);
            }
            else
            {
                ellipseStartCenter = new Point(startX, startY);
            }

            return ellipseStartCenter;
        }

        public static Point GetEllipseEndCenter(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            Point ellipseEndCenter;

            if (isEndVisible)
            {
                double ex = endX - sizeX;
                double ey = endY + sizeY;

                ellipseEndCenter = new Point(ex, ey);
            }
            else
            {
                ellipseEndCenter = new Point(endX, endY);
            }

            return ellipseEndCenter;
        }

        #endregion
    }

    #endregion
}
