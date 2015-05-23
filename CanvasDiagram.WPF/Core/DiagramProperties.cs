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
    #region DiagramProperties

    public class DiagramProperties
    {
        #region Properties

        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public int GridOriginX { get; set; }
        public int GridOriginY { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public int GridSize { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public double SnapOffsetX { get; set; }
        public double SnapOffsetY { get; set; }

        #endregion

        #region Defaults

        public static DiagramProperties Default
        {
            get
            {
                return new DiagramProperties()
                {
                    PageWidth = 1260,
                    PageHeight = 891,
                    GridOriginX = 330,
                    GridOriginY = 31,
                    GridWidth = 600,
                    GridHeight = 750,
                    GridSize = 30,
                    SnapX = 15,
                    SnapY = 15,
                    SnapOffsetX = 0,
                    SnapOffsetY = 1
                };
            }
        }

        #endregion
    } 

    #endregion
}
