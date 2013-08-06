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
    #region DxfLtype

    public class DxfLtype : DxfObject<DxfLtype>
    {
        public string Name { get; set; }
        public DxfLtypeStandardFlags LtypeStandardFlags { get; set; }
        public string Description { get; set; }
        public int DashLengthItems { get; set; }
        public double TotalPatternLenght { get; set; }
        public double[] DashLenghts { get; set; }

        public DxfLtype(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLtype Defaults()
        {
            Name = string.Empty;
            LtypeStandardFlags = DxfLtypeStandardFlags.Default;
            Description = string.Empty;
            DashLengthItems = 0;
            TotalPatternLenght = 0.0;
            DashLenghts = null;

            return this;
        }

        public DxfLtype Create()
        {
            Add(0, CodeName.Ltype);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass("AcDbSymbolTableRecord");
                Subclass("AcDbLinetypeTableRecord");
            }

            Add(2, Name);
            Add(70, (int)LtypeStandardFlags);
            Add(3, Description);
            Add(72, 65); // alignment code; value is always 65, the ASCII code for A

            Add(73, DashLengthItems);
            Add(40, TotalPatternLenght);

            if (DashLenghts != null)
            {
                // dash length 0,1,2...n-1 = DashLengthItems
                foreach (var lenght in DashLenghts)
                {
                    Add(49, lenght);
                }
            }

            if (Version > DxfAcadVer.AC1009)
            {
                // TODO: multiple complex linetype elements
                // 74
                // 75
                // 340
                // 46
                // 50
                // 44
                // 45
                // 9
            }

            return this;
        }
    }

    #endregion
}
