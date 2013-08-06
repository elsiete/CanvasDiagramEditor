// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Tables
{
    #region DxfStyle

    public class DxfStyle : DxfObject<DxfStyle>
    {
        public DxfStyle(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add("0", "STYLE");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass("AcDbSymbolTableRecord");
                Subclass("AcDbTextStyleTableRecord");
            }
        }

        public DxfStyle Name(string name)
        {
            Add("2", name);
            return this;
        }

        public DxfStyle StandardFlags(DxfStyleFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfStyle FixedTextHeight(double height)
        {
            Add("40", height);
            return this;
        }

        public DxfStyle WidthFactor(double factor)
        {
            Add("41", factor);
            return this;
        }

        public DxfStyle ObliqueAngle(double angle)
        {
            Add("50", angle);
            return this;
        }

        public DxfStyle TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add("71", (int)flags);
            return this;
        }

        public DxfStyle LastHeightUsed(double height)
        {
            Add("42", height);
            return this;
        }

        public DxfStyle PrimaryFontFile(string name)
        {
            Add("3", name);
            return this;
        }

        public DxfStyle BifFontFile(string name)
        {
            Add("4", name);
            return this;
        }
    }

    #endregion
}
