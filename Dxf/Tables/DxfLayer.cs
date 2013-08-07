﻿// Copyright (C) Wiesław Šoltés 2013. 
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

    public class DxfLayer : DxfObject<DxfLayer>
    {
        public string Name { get; set; }
        public DxfLayerStandardFlags LayerStandardFlags { get; set; }
        public string Color { get; set; }
        public string LineType { get; set; }
        public bool PlottingFlag { get; set; }
        public DxfLineWeight LineWeight { get; set; }
        public string PlotStyleNameHandle { get; set; }

        public DxfLayer(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLayer Defaults()
        {
            Name = string.Empty;
            LayerStandardFlags = DxfLayerStandardFlags.Default;
            Color = DxfDefaultColors.Default.ColorToString();
            LineType = string.Empty;
            PlottingFlag = true;
            LineWeight = DxfLineWeight.LnWtByLwDefault;
            PlotStyleNameHandle = "0";

            return this;
        }

        public DxfLayer Create()
        {
            Add(0, CodeName.Layer);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.LayerTableRecord);
            }

            Add(2, Name);
            Add(70, (int)LayerStandardFlags);
            Add(62, Color);
            Add(6, LineType);

            if (Version > DxfAcadVer.AC1009)
            {
                Add(290, PlottingFlag);
                Add(370, (int)LineWeight);
                Add(390, PlotStyleNameHandle);
            }

            return this;
        }
    }

    #endregion
}
