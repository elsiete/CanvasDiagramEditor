// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Core
{
    #region SubclassMarker

    public static class SubclassMarker
    {
        public const string Line = "AcDbLine";
        public const string BlockTableRecord = "AcDbBlockTableRecord";
        public const string LayerTableRecord = "AcDbLayerTableRecord";
        public const string UCSTableRecord = "AcDbUCSTableRecord";
        public const string SymbolTableRecord = "AcDbSymbolTableRecord";

    } 

    #endregion
}
