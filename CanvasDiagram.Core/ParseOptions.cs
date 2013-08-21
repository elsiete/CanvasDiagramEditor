// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Core
{
    #region ParseOptions

    public class ParseOptions
    {
        #region Constructor

        public ParseOptions()
        {
        }

        #endregion

        #region Properties

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool AppendIds { get; set; }
        public bool UpdateIds { get; set; }
        public bool Select { get; set; }
        public bool CreateElements { get; set; }
        public DiagramProperties Properties { get; set; }
        public IdCounter Counter { get; set; } 

        #endregion
    } 

    #endregion
}
