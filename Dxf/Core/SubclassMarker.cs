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
        public const string Text = "AcDbText";
        
        public const string AttributeDefinition = "AcDbAttributeDefinition";
        public const string Attribute = "AcDbAttribute";

        public const string BlockTableRecord = "AcDbBlockTableRecord";
        public const string LayerTableRecord = "AcDbLayerTableRecord";

        public const string ViewportTableRecord = "AcDbViewportTableRecord";
        public const string DimStyleTableRecord = "AcDbDimStyleTableRecord";

        public const string UCSTableRecord = "AcDbUCSTableRecord";
        public const string SymbolTableRecord = "AcDbSymbolTableRecord";

        public const string Dictionary = "AcDbDictionary";

        public const string Entity = "AcDbEntity";

        public const string BlockBegin = "AcDbBlockBegin";
        public const string BlockEnd = "AcDbBlockEnd";

        public const string BlockReference = "AcDbBlockReference";
    }

    #endregion
}
