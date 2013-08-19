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
    #region EntityName

    public static class CodeName
    {
        public const string Section = "SECTION";

        public const string Line = "LINE";
        public const string Text = "TEXT";
        public const string Attdef = "ATTDEF";
        public const string Attrib = "ATTRIB";
        public const string Ltype = "LTYPE";
        public const string BlockRecord = "BLOCK_RECORD";
        public const string Block = "BLOCK";
        public const string Endblk = "ENDBLK";

        public const string Vport = "VPORT";
        public const string Dimstyle = "DIMSTYLE";
        public const string Layer = "LAYER";
        public const string Ucs = "UCS";
        public const string Dictionary = "DICTIONARY";
        public const string Class = "CLASS";
    } 

    #endregion
}
