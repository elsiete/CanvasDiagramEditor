// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Enums
{
    #region DxfAcadVer

    // DXF support status: 
    // AC1006 - supported
    // AC1009 - supported
    // AC1012 - not supported
    // AC1014 - not supported
    // AC1015 - ?

    // AutoCAD drawing database version number: 
    // AC1006 = R10
    // AC1009 = R11 and R12, 
    // AC1012 = R13
    // AC1014 = R14
    // AC1015 = AutoCAD 2000

    public enum DxfAcadVer : int
    {
        AC1006 = 0, // R10
        AC1009 = 1, // R11 and R12
        AC1012 = 2, // R13
        AC1014 = 3, // R14
        AC1015 = 4 // AutoCAD 2000
    }

    #endregion
}
