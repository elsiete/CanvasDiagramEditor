// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf
{
    #region DxfHeader

    public class DxfHeader : DxfObject
    {
        public DxfHeader()
            : base()
        {
        }

        public DxfHeader Begin()
        {
            Add("0", "SECTION");
            Add("2", "HEADER");
            return this;
        }

        public DxfHeader Default()
        {
            // Current DXF compatibility: AC1009 = R11 and R12

            // the AutoCAD drawing database version number: 
            // AC1006 = R10
            // AC1009 = R11 and R12, 
            // AC1012 = R13, AC1014 = R14, 
            // AC1015 = AutoCAD 2000
            Add("9", "$ACADVER");
            Add("1", "AC1009");

            // drawing extents upper-right corner
            Add("9", "$EXTMAX");
            Add("10", "1260.0");
            Add("20", "891.0");

            // drawing extents lower-left corner
            Add("9", "$EXTMIN");
            Add("10", "0.0");
            Add("20", "0.0");

            // insertion base 
            Add("9", "$INSBASE");
            Add("10", "0.0");
            Add("20", "0.0");
            Add("30", "0.0");

            // drawing limits upper-right corner 
            Add("9", "$LIMMAX");
            Add("10", "1260.0");
            Add("20", "891.0");

            // drawing limits lower-left corner 
            Add("9", "$LIMMIN");
            Add("10", "0.0");
            Add("20", "0.0");

            // default drawing units for AutoCAD DesignCenter blocks
            /* 
            0 = Unitless;
            1 = Inches; 
            2 = Feet; 
            3 = Miles; 
            4 = Millimeters; 
            5 = Centimeters; 
            6 = Meters; 
            7 = Kilometers; 
            8 = Microinches; 
            9 = Mils; 
            10 = Yards; 
            11 = Angstroms; 
            12 = Nanometers; 
            13 = Microns; 
            14 = Decimeters; 
            15 = Decameters; 
            16 = Hectometers; 
            17 = Gigameters; 
            18 = Astronomical units; 
            19 = Light years; 
            20 = Parsecs
            */

            // units format for coordinates and distances
            Add("9", "$INSUNITS");
            Add("70", (int)4);

            // units format for coordinates and distances
            Add("9", "$LUNITS");
            Add("70", (int)2);

            // units precision for coordinates and distances
            Add("9", "$LUPREC");
            Add("70", (int)4);

            // sets drawing units
            Add("9", "$MEASUREMENT");
            Add("70", (int)1); // 0 = English; 1 = Metric

            return this;
        }

        public DxfHeader End()
        {
            Add("0", "ENDSEC");
            return this;
        }
    }

    #endregion
}
