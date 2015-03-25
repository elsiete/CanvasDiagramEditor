﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Dxf.Core
{
    #region CodeName

    public static class CodeName
    {
        public const string Section = "SECTION";
        public const string EndSec = "ENDSEC";

        public const string SeqEnd = "SEQEND";

        public const string Entities = "ENTITIES";

        public const string Line = "LINE";
        public const string Text = "TEXT";
        public const string Circle = "CIRCLE";
        public const string Attdef = "ATTDEF";
        public const string Attrib = "ATTRIB";
        public const string Ltype = "LTYPE";
        public const string BlockRecord = "BLOCK_RECORD";
        public const string Block = "BLOCK";
        public const string Endblk = "ENDBLK";
        public const string Insert = "INSERT";

        public const string Vport = "VPORT";
        public const string Dimstyle = "DIMSTYLE";
        public const string Layer = "LAYER";
        public const string Ucs = "UCS";
        public const string Dictionary = "DICTIONARY";
        public const string Class = "CLASS";
    } 

    #endregion
}
