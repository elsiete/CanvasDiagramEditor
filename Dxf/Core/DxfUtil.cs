// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Core
{
    #region DxfUtil

    public static class DxfUtil
    {
        public static string ToDxfHandle(this int handle)
        {
            return handle.ToString("X");
        }

        public static string ColorToString(this DxfDefaultColors color)
        {
            return ((int)color).ToString();
        }
    }

    #endregion
}
