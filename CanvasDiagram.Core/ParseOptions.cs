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
    #region ParseOptions

    public class ParseOptions
    {
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
