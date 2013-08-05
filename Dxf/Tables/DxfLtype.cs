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

    public class DxfLtype : DxfObject
    {
        public DxfLtype()
            : base()
        {
            Add("0", "LTYPE");
            Add("72", "65"); // alignment code; value is always 65, the ASCII code for A
        }

        public DxfLtype Name(string name)
        {
            Add("2", name);
            return this;
        }

        public DxfLtype Description(string description)
        {
            Add("3", description);
            return this;
        }

        public DxfLtype StandardFlags(DxfLtypeFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfLtype DashLengthItems(int items)
        {
            Add("73", items);
            return this;
        }

        public DxfLtype TotalPatternLenght(double lenght)
        {
            Add("40", lenght);
            return this;
        }

        public DxfLtype DashLenghts(double[] lenghts)
        {
            if (lenghts != null)
            {
                // dash length 0,1,2...n-1 = DashLengthItems
                foreach (var lenght in lenghts)
                {
                    Add("49", lenght);
                }
            }
            return this;
        }
    }

    #endregion
}
