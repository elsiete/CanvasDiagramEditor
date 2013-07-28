// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor.Parser
{
    #region DiagramProperties

    public class DiagramProperties
    {
        #region Constructor

        public DiagramProperties()
        {
        }

        #endregion

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
    } 

    #endregion
}
