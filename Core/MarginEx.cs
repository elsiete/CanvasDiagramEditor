// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Core
{
    #region MarginEx

    public class MarginEx : IMargin
    {
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
    }

    #endregion
}
