// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Util
{
    #region DxfDoubleExtensions

    public static class DxfDoubleExtensions
    {
        public static string ToDxfString(this double value)
        {
            return value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
        }
    }

    #endregion
}
