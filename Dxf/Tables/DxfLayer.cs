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
    #region DxfLayer

    public class DxfLayer : DxfObject
    {
        public DxfLayer()
            : base()
        {
            Add("0", "LAYER");
        }

        public DxfLayer Name(string name)
        {
            Add("2", name);
            return this;
        }

        public DxfLayer StandardFlags(DxfLayerFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }

        public DxfLayer Color(int number)
        {
            Add("62", number);
            return this;
        }

        public DxfLayer LineType(string name)
        {
            Add("6", name);
            return this;
        }
    }

    #endregion
}
