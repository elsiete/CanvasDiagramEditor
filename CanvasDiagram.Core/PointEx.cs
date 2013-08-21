﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

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
