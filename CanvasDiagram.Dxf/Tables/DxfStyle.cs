// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Tables
{
    #region DxfStyle

    public class DxfStyle : DxfObject<DxfStyle>
    {
        public DxfStyle(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add(0, "STYLE");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.TextStyleTableRecord);
            }
        }

        public DxfStyle Name(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfStyle StandardFlags(DxfStyleFlags flags)
        {
            Add(70, (int)flags);
            return this;
        }

        public DxfStyle FixedTextHeight(double height)
        {
            Add(40, height);
            return this;
        }

        public DxfStyle WidthFactor(double factor)
        {
            Add(41, factor);
            return this;
        }

        public DxfStyle ObliqueAngle(double angle)
        {
            Add(50, angle);
            return this;
        }

        public DxfStyle TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add(71, (int)flags);
            return this;
        }

        public DxfStyle LastHeightUsed(double height)
        {
            Add(42, height);
            return this;
        }

        public DxfStyle PrimaryFontFile(string name)
        {
            Add(3, name);
            return this;
        }

        public DxfStyle BifFontFile(string name)
        {
            Add(4, name);
            return this;
        }
    }

    #endregion
}
