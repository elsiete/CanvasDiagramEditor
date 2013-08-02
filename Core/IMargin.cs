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
    #region IMargin

    public interface IMargin
    {
        double Bottom { get; set; }
        double Left { get; set; }
        double Right { get; set; }
        double Top { get; set; }
    }

    #endregion
}
