// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf;
using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Util;
using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#endregion

namespace CanvasDiagramEditor
{
    #region Aliases

    using MapPin = Tuple<string, string>;
    using MapWire = Tuple<object, object, object>;
    using MapWires = Tuple<object, List<Tuple<string, string>>>;
    using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    using History = Tuple<Stack<string>, Stack<string>>;
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>;
    using TreeProject = Tuple<string, Stack<Stack<string>>>;
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    using TreeSolution = Tuple<string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    using CanvasDiagramEditor.Dxf.Entities;

    #endregion

    #region DxfDiagramCreator

    public class DxfDiagramCreator : IDiagramCreator
    {
        #region Lineweights in Millimeters

        public static double LogicThicknessMm = 0.18;
        public static double WireThicknessMm = 0.18;
        public static double ElementThicknessMm = 0.35;
        public static double IOThicknessMm = 0.25;
        public static double PageThicknessMm = 0.13;

        #endregion

        #region Properties

        public bool ShortenStart { get; set; }
        public bool ShortenEnd { get; set; }

        public DiagramProperties DiagramProperties { get; set; }
        public List<object> Tags = null;

        #endregion

        #region Fields

        private StringBuilder DxfString = null;

        //private double PageWidth = 1260.0;
        private double PageHeight = 891.0;

        private static string LayerFrame = "FRAME";
        private static string LayerGrid = "GRID";
        private static string LayerTable = "TABLE";
        private static string LayerIO = "IO";
        private static string LayerWires = "WIRES";
        private static string LayerElements = "ELEMENTS";

        private double ShortenLineSize = 15.0;
        private double InvertedCircleRadius = 4.0;
        private double InvertedCircleThickness = 0.0;

        private Dictionary<string, double> LineWeights = new Dictionary<string, double>()
        {
            { LayerFrame, PageThicknessMm },
            { LayerGrid, PageThicknessMm },
            { LayerTable, PageThicknessMm },
            { LayerIO, IOThicknessMm },
            { LayerWires, WireThicknessMm },
            { LayerElements, ElementThicknessMm }
        };

        #endregion

        #region IO Tags

        public Tag GetTagById(int tagId)
        {
            // set element Tag
            var tags = this.Tags;
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();

                if (tag != null)
                {
                    return tag;
                }
            }

            return null;
        }

        #endregion

        #region Dxf Wrappers

        public string DxfLine(double x1, double y1,
            double x2, double y2,
            double offsetX, double offsetY,
            string layer, int color,
            double pageOffsetX, double pageOffsetY)
        {
            double _x1 = pageOffsetX > 0.0 ? pageOffsetX - x1 + offsetX : x1 + offsetX;
            double _y1 = pageOffsetY > 0.0 ? pageOffsetY - y1 + offsetY : y1 + offsetY;
            double _x2 = pageOffsetX > 0.0 ? pageOffsetX - x2 + offsetX : x2 + offsetX;
            double _y2 = pageOffsetY > 0.0 ? pageOffsetY - y2 + offsetY : y2 + offsetY;

            double thickness = 0.0;

            //LineWeights.TryGetValue(layer, out thickness);

            var line = new DxfLine()
                .Layer(layer)
                .Color(color.ToString())
                .Thickness(thickness)
                .Start(new Vector3(_x1, _y1, 0.0))
                .End(new Vector3(_x2, _y2, 0.0));

            return line.ToString();

            /*
            double lineWeight = 0.0;

            //LineWeights.TryGetValue(layer, out lineWeight);

            const int numberOfVertices = 2;

            var vertex1 = new DxfLwpolylineVertex(new Vector2(_x1, _y1), 0.0, 0.0, 0.0);
            var vertex2 = new DxfLwpolylineVertex(new Vector2(_x2, _y2), 0.0, 0.0, 0.0);

            return DxfFactory.DxfLwpolyline(numberOfVertices,
                DxfLwpolylineFlags.Default,
                lineWeight,
                0.0,
                0.0,
                new DxfLwpolylineVertex[numberOfVertices] { vertex1, vertex2 },
                null,
                layer,
                color.ToString());
            */
        }

        public string DxfCircle(double x, double y,
            double radius,
            double offsetX, double offsetY,
            string layer, int color,
            double pageOffsetX, double pageOffsetY)
        {
            double _x = pageOffsetX > 0.0 ? pageOffsetX - x + offsetX : x + offsetX;
            double _y = pageOffsetY > 0.0 ? pageOffsetY - y + offsetY : y + offsetY;

            double thickness = 0.0;

            //LineWeights.TryGetValue(layer, out thickness);

            var circle = new DxfCircle()
                .Layer(layer)
                .Color(color.ToString())
                .Thickness(thickness)
                .Radius(radius)
                .Center(new Vector3(_x, _y, 0.0));

            return circle.ToString();
        }

        private string DxfAttdefIO(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef()
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .Tag(tag)
                .DefaultValue(tag)
                .Prompt(tag)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .TextHeight(6.0)
                .TextStyle("TEXTIO")
                .Layer(LayerIO)
                .HorizontalTextJustification(DxfHorizontalTextJustification.Left)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return attdef.ToString();
        }

        private string DxfAttribIO(string tag, string text,
            double x, double y,
            bool isVisible)
        {
            var attrib = new DxfAttrib()
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .Tag(tag)
                .DefaultValue(text)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .TextHeight(6.0)
                .TextStyle("TEXTIO")
                .Layer(LayerIO)
                .HorizontalTextJustification(DxfHorizontalTextJustification.Left)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return attrib.ToString();
        }

        private string DxfAttdefGate(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef()
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .Tag(tag)
                .DefaultValue(tag)
                .Prompt(tag)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .TextHeight(10.0)
                .TextStyle("TEXTGATE")
                .Layer(LayerElements)
                .HorizontalTextJustification(DxfHorizontalTextJustification.Center)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return attdef.ToString();
        }

        private string DxfAttribGate(string tag, string text,
            double x, double y,
            bool isVisible)
        {
            var attrib = new DxfAttrib()
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .Tag(tag)
                .DefaultValue(text)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .TextHeight(10.0)
                .TextStyle("TEXTGATE")
                .Layer(LayerElements)
                .HorizontalTextJustification(DxfHorizontalTextJustification.Center)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return attrib.ToString();
        }

        private string DxfTextGate(string text, 
            double x1, double y1,
            double x2, double y2)
        {
            var txt = new DxfText()
                .Layer(LayerElements)
                .Text(text)
                .TextStyle("TEXTGATE")
                .TextHeight(10.0)
                .FirstAlignment(new Vector3(x1, y1, 0.0))
                .SecondAlignment(new Vector3(x2, y2, 0.0))
                .HorizontalTextJustification(DxfHorizontalTextJustification.Center)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return txt.ToString();
        }

        #endregion

        #region Dxf Diagram

        public string DxfHeader(string title)
        {
             var b = new DxfBuilder();

            // Current DXF compatibility: AC1009 = R11 and R12

            // header comment
            b.AddComment(title);

            // begin header section
            b.Add("0", "SECTION");
            b.Add("2", "HEADER");

            // the AutoCAD drawing database version number: 
            // AC1006 = R10
            // AC1009 = R11 and R12, 
            // AC1012 = R13, AC1014 = R14, 
            // AC1015 = AutoCAD 2000
            b.Add("9", "$ACADVER");
            b.Add("1", "AC1009");

            // drawing extents upper-right corner
            b.Add("9", "$EXTMAX");
            b.Add("10", "1260.0");
            b.Add("20", "891.0");

            // drawing extents lower-left corner
            b.Add("9", "$EXTMIN");
            b.Add("10", "0.0");
            b.Add("20", "0.0");

            // insertion base 
            b.Add("9", "$INSBASE");
            b.Add("10", "0.0");
            b.Add("20", "0.0");
            b.Add("30", "0.0");

            // drawing limits upper-right corner 
            b.Add("9", "$LIMMAX");
            b.Add("10", "1260.0");
            b.Add("20", "891.0");

            // drawing limits lower-left corner 
            b.Add("9", "$LIMMIN");
            b.Add("10", "0.0");
            b.Add("20", "0.0");

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
            b.Add("9", "$INSUNITS");
            b.Add("70", (int) 4);

            // units format for coordinates and distances
            b.Add("9", "$LUNITS");
            b.Add("70", (int) 2);

            // units precision for coordinates and distances
            b.Add("9", "$LUPREC");
            b.Add("70", (int) 4);

            // sets drawing units
            b.Add("9", "$MEASUREMENT");
            b.Add("70", (int) 1); // 0 = English; 1 = Metric

            // end header section
            b.Add("0", "ENDSEC");

            return b.Build();
        }

        private string DxfTableAppid()
        {
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfAppidBegin(1));

            // appid: ACAD
            sb.AppendLine("0");
            sb.AppendLine("APPID");

            sb.AppendLine("2");
            sb.AppendLine("CDE"); // CanvasDiagramEditor

            sb.AppendLine("70");
            sb.AppendLine("0");

            sb.Append(DxfTables.DxfAppidEnd());

            return sb.ToString();
        }

        private string DxfTableLayers()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfLayersBegin(6));

            // layer: FRAME
            str = DxfTables.DxfLayer(LayerFrame,
                DxfLayerFlags.Default,
                "8",
                "CONTINUOUS");

            sb.Append(str);

            // layer: GRID
            str = DxfTables.DxfLayer(LayerGrid,
                DxfLayerFlags.Default,
                "9",
                "CONTINUOUS");

            sb.Append(str);

            // layer: TABLE
            str = DxfTables.DxfLayer(LayerTable,
                DxfLayerFlags.Default,
                "8",
                "CONTINUOUS");

            sb.Append(str);

            // layer: IO
            str = DxfTables.DxfLayer(LayerIO,
                DxfLayerFlags.Default,
                "6",
                "CONTINUOUS");

            sb.Append(str);

            // layer: WIRES
            str = DxfTables.DxfLayer(LayerWires,
                DxfLayerFlags.Default,
                "5",
                "CONTINUOUS");

            sb.Append(str);

            // layer: ELEMENTS
            str = DxfTables.DxfLayer(LayerElements,
                DxfLayerFlags.Default,
                "5",
                "CONTINUOUS");

            sb.Append(str);

            sb.Append(DxfTables.DxfLayersEnd());

            return sb.ToString();
        }

        private string DxfTableLineStyles()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfLtypesBegin(1));

            // ltype: CONTINUOUS
            str = DxfTables.DxfLtype(
                "CONTINUOUS",
                DxfLtypeFlags.Default,
                "Solid line",
                0.0, 0.0, null);

            sb.Append(DxfTables.DxfLtypesEnd());

            return sb.ToString();
        }

        private string DxfTableTextStyles()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfStylesBegin(8));

            // style: TEXTHEADERSMALL
            str = DxfTables.DxfStyle("TEXTHEADERSMALL",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTHEADERLARGE
            str = DxfTables.DxfStyle("TEXTHEADERLARGE",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTNUMBER
            str = DxfTables.DxfStyle("TEXTNUMBER",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTTABLEHEADER
            str = DxfTables.DxfStyle("TEXTTABLEHEADER",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTTABLETEXT
            str = DxfTables.DxfStyle("TEXTTABLETEXT",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTTABLE
            str = DxfTables.DxfStyle("TEXTNUMBER",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTGATE
            str = DxfTables.DxfStyle("TEXTGATE",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            // style: TEXTIO
            str = DxfTables.DxfStyle("TEXTIO",
                DxfStyleFlags.Default,
                0.0, 1.0, 0.0,
                DxfTextGenerationFlags.Default,
                0.0,
                "arial.ttf", "");

            sb.Append(str);

            sb.Append(DxfTables.DxfStylesEnd());

            return sb.ToString();
        }

        private string DxfTableViews()
        {
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfViewsBegin(1));

            // view: DIAGRAM
            sb.AppendLine("0");
            sb.AppendLine("VIEW");

            sb.AppendLine("2");
            sb.AppendLine("DIAGRAM");

            sb.AppendLine("70");
            sb.AppendLine("0");

            sb.AppendLine("40");
            sb.AppendLine("891.0");

            sb.AppendLine("10");
            sb.AppendLine("630.0");

            sb.AppendLine("20");
            sb.AppendLine("445.5");

            sb.AppendLine("41");
            sb.AppendLine("1260.0");

            sb.AppendLine("11");
            sb.AppendLine("0");

            sb.AppendLine("21");
            sb.AppendLine("0");

            sb.AppendLine("31");
            sb.AppendLine("0");

            sb.AppendLine("12");
            sb.AppendLine("0");

            sb.AppendLine("22");
            sb.AppendLine("0");

            sb.AppendLine("32");
            sb.AppendLine("0");

            sb.AppendLine("43");
            sb.AppendLine("0");

            sb.AppendLine("44");
            sb.AppendLine("0");

            sb.AppendLine("50");
            sb.AppendLine("0");

            sb.Append(DxfTables.DxfViewsEnd());

            return sb.ToString();
        }

        public string DxfTablesAll()
        {
            var sb = new StringBuilder();

            sb.Append(DxfTables.DxfTablesBegin());

            sb.Append(DxfTableAppid());

            sb.Append(DxfTableLineStyles());

            sb.Append(DxfTableLayers());

            sb.Append(DxfTableTextStyles());

            sb.Append(DxfTableViews());

            sb.Append(DxfTables.DxfTablesEnd());

            return sb.ToString();
        }

        public string DxfBlockFrame()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: FRAME
            sb.AppendLine("2");
            sb.AppendLine("FRAME");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            // page frame

            // M 0,20 L 600,20 
            str = DxfLine(0.0, 20.0, 600.0, 20.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 600,770 L 0,770 
            str = DxfLine(600.0, 770.0, 0.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 0,770 L 0,0 
            str = DxfLine(0.0, 770.0, 0.0, 0.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 600,0 L 600,770
            str = DxfLine(600.0, 0.0, 600.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 15,15 L 1245,15 
            str = DxfLine(15.0, 15.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1245,816 L 15,816
            str = DxfLine(1245.0, 816.0, 15.0, 816.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 15,876 L 1245,876
            str = DxfLine(15.0, 876.0, 1245.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1245,876 L 1245,15
            str = DxfLine(1245.0, 876.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 15,15 L 15,876
            str = DxfLine(15.0, 15.0, 15.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1,1 L 1259,1 
            str = DxfLine(1.0, 1.0, 1259.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1259,890 L 1,890 
            str = DxfLine(1259.0, 890.0, 1.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1,890 L 1,1 
            str = DxfLine(1.0, 890.0, 1.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1259,1 L 1259,890
            str = DxfLine(1259.0, 1.0, 1259.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // inputs

            // M 30,0 L 30,750 
            str = DxfLine(30.0, 0.0, 30.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 240,750 L 240,0
            str = DxfLine(240.0, 750.0, 240.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 315,0 L 0,0
            str = DxfLine(315.0, 0.0, 0.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 0,750 L 315,750
            str = DxfLine(0.0, 750.0, 315.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);
         
            // M 0,30 L 315,30 
            // M 315,60 L 0,60 
            // M 0,90 L 315,90 
            // M 315,120 L 0,120 
            // M 0,150 L 315,150 
            // M 315,180 L 0,180 
            // M 0,210 L 315,210 
            // M 315,240 L 0,240
            // M 0,270 L 315,270 
            // M 315,300 L 0,300 
            // M 0,330 L 315,330 
            // M 315,360 L 0,360
            // M 0,390 L 315,390 
            // M 315,420 L 0,420 
            // M 0,450 L 315,450 
            // M 315,480 L 0,480 
            // M 0,510 L 315,510 
            // M 315,540 L 0,540
            // M 0,570 L 315,570 
            // M 315,600 L 0,600 
            // M 0,630 L 315,630 
            // M 315,660 L 0,660 
            // M 0,690 L 315,690
            // M 315,720 L 0,720
            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                str = DxfLine(0.0, y, 315.0, y, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
                sb.Append(str);
            }

            // outputs

            // M 210,0 L 210,750
            str = DxfLine(210.0, 0.0, 210.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 285,750 L 285,0
            str = DxfLine(285.0, 750.0, 285.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 315,0 L 0,0 
            str = DxfLine(315.0, 0.0, 0.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 0,750 L 315,750
            str = DxfLine(0.0, 750.0, 315.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 0,30 L 315,30 
            // M 315,60 L 0,60 
            // M 0,90 L 315,90 
            // M 315,120 L 0,120 
            // M 0,150 L 315,150 
            // M 315,180 L 0,180
            // M 0,210 L 315,210 
            // M 315,240 L 0,240 
            // M 0,270 L 315,270 
            // M 315,300 L 0,300 
            // M 0,330 L 315,330 
            // M 315,360 L 0,360 
            // M 0,390 L 315,390 
            // M 315,420 L 0,420 
            // M 0,450 L 315,450 
            // M 315,480 L 0,480 
            // M 0,510 L 315,510 
            // M 315,540 L 0,540 
            // M 0,570 L 315,570 
            // M 315,600 L 0,600 
            // M 0,630 L 315,630 
            // M 315,660 L 0,660 
            // M 0,690 L 315,690 
            // M 315,720 L 0,720
            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                str = DxfLine(0.0, y, 315.0, y, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
                sb.Append(str);
            }




            // TODO: text





            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlockTable()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: TABLE
            sb.AppendLine("2");
            sb.AppendLine("TABLE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            // page table

            // M 0,15 L 175,15 
            str = DxfLine(0.0, 15.0, 175.0, 15.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 405,15 L 1230,15 
            str = DxfLine(405.0, 15.0, 1230.0, 15.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1230,30 L 965,30 
            str = DxfLine(1230.0, 30.0, 965.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 695,30 L 405,30 
            str = DxfLine(695.0, 30.0, 405.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 175,30, 0,30 
            str = DxfLine(175.0, 30.0, 0.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 0,45 L 175,45 
            str = DxfLine(0.0, 45.0, 175.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 405,45 L 695,45
            str = DxfLine(405.0, 45.0, 695.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 965,45 L 1230,45
            str = DxfLine(965.0, 45.0, 1230.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 30,0 L 30,60 
            str = DxfLine(30.0, 0.0, 30.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 75,0 L 75,60 
            str = DxfLine(75.0, 0.0, 75.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 175,60 L 175,0 
            str = DxfLine(175.0, 60.0, 175.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 290,0 L 290,60 
            str = DxfLine(290.0, 0.0, 290.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 405,60 L 405,0 
            str = DxfLine(405.0, 60.0, 405.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 465,0 L 465,60 
            str = DxfLine(465.0, 0.0, 465.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 605,60 L 605,0 
            str = DxfLine(605.0, 60.0, 605.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 650,0 L 650,60 
            str = DxfLine(650.0, 0.0, 650.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 695,60 L 695,0 
            str = DxfLine(695.0, 60.0, 695.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 965,0 L 965,60 
            str = DxfLine(965.0, 0.0, 965.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1005,60 L 1005,0 
            str = DxfLine(1005.0, 60.0, 1005.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1045,0 L 1045,60 
            str = DxfLine(1045.0, 0.0, 1045.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);

            // M 1100,60 L 1100,0
            str = DxfLine(1100.0, 60.0, 1100.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY);
            sb.Append(str);


            // TODO: text


            // TODO: attributes


            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlockGrid()
        {
            //string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: GRID
            sb.AppendLine("2");
            sb.AppendLine("GRID");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");



            // TODO: lines



            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlockInput()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: INPUT
            sb.AppendLine("2");
            sb.AppendLine("INPUT");

            sb.AppendLine("3");
            sb.AppendLine("INPUT");

            // block type
            sb.AppendLine("70");
            sb.AppendLine(((int)DxfBlockTypeFlags.NonConstantAttributes).ToString());

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 285,0 
            str = DxfLine(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 285,30 L 0,30
            str = DxfLine(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,30  L 0,0 
            str = DxfLine(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 210,0 L 210,30 
            str = DxfLine(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 285,30 L 285,0
            str = DxfLine(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // tag text
            str = DxfAttdefIO("ID", 288, 30, false);
            sb.Append(str);

            str = DxfAttdefIO("TAGID", 288, 0, false);
            sb.Append(str);

            str = DxfAttdefIO("DESIGNATION", 3, 21.5, true);
            sb.Append(str);

            str = DxfAttdefIO("DESCRIPTION", 3, 7.5, true);
            sb.Append(str);

            str = DxfAttdefIO("SIGNAL", 213, 21.5, true);
            sb.Append(str);

            str = DxfAttdefIO("CONDITION", 213, 7.5, true);
            sb.Append(str);

            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlockOutput()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: OUTPUT
            sb.AppendLine("2");
            sb.AppendLine("OUTPUT");

            sb.AppendLine("3");
            sb.AppendLine("OUTPUT");

            // block type
            sb.AppendLine("70");
            sb.AppendLine(((int)DxfBlockTypeFlags.NonConstantAttributes).ToString());

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 285,0 
            str = DxfLine(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 285,30 L 0,30
            str = DxfLine(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,30  L 0,0 
            str = DxfLine(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 210,0 L 210,30 
            str = DxfLine(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // M 285,30 L 285,0
            str = DxfLine(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.Append(str);

            // tag text
            str = DxfAttdefIO("ID", 288, 30, false);
            sb.Append(str);

            str = DxfAttdefIO("TAGID", 288, 0, false);
            sb.Append(str);

            str = DxfAttdefIO("DESIGNATION", 3, 21.5, true);
            sb.Append(str);

            str = DxfAttdefIO("DESCRIPTION", 3, 7.5, true);
            sb.Append(str);

            str = DxfAttdefIO("SIGNAL", 213, 21.5, true);
            sb.Append(str);

            str = DxfAttdefIO("CONDITION", 213, 7.5, true);
            sb.Append(str);

            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockAndGate()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: ANDGATE
            sb.AppendLine("2");
            sb.AppendLine("ANDGATE");

            sb.AppendLine("3");
            sb.AppendLine("ANDGATE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine(((int)DxfBlockTypeFlags.NonConstantAttributes).ToString());

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 30,0
            str = DxfLine(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,30 L 30,30
            str = DxfLine(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,0 L 0,30
            str = DxfLine(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 30,0 L 30,30
            str = DxfLine(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // tag text
            str = DxfAttdefGate("ID", 30.0, 30.0, false);
            sb.Append(str);

            str = DxfAttdefGate("TEXT", 15.0, 15.0, true);
            sb.Append(str);

            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlockOrGate()
        {
            string str = null;
            var sb = new StringBuilder();

            sb.Append(DxfBlocks.DxfBlockBegin());

            // name: ORGATE
            sb.AppendLine("2");
            sb.AppendLine("ORGATE");

            sb.AppendLine("3");
            sb.AppendLine("ORGATE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine(((int)DxfBlockTypeFlags.NonConstantAttributes).ToString());

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 30,0
            str = DxfLine(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,30 L 30,30
            str = DxfLine(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 0,0 L 0,30
            str = DxfLine(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // M 30,0 L 30,30
            str = DxfLine(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.Append(str);

            // tag text
            str = DxfAttdefGate("ID", 30.0, 30.0, false);
            sb.Append(str);

            str = DxfAttdefGate("TEXT", 15.0, 15.0, true);
            sb.Append(str);

            sb.Append(DxfBlocks.DxfBlockEnd());

            return sb.ToString();
        }

        public string DxfBlocksAll()
        {
            var sb = new StringBuilder();

            // begin blocks
            sb.Append(DxfBlocks.DxfBlocksBegin());

            // blocks
            sb.Append(DxfBlockFrame());
            sb.Append(DxfBlockTable());
            sb.Append(DxfBlockGrid());
            sb.Append(DxfBlockInput());
            sb.Append(DxfBlockOutput());
            sb.Append(DxfBlockAndGate());
            sb.Append(DxfBlockOrGate());

            // end blocks
            sb.Append(DxfBlocks.DxfBlocksEnd());

            return sb.ToString();
        }

        #endregion

        #region Insert

        public object InsertPageFrame(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("FRAME");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerFrame);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 891.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object InsertPageTable(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("TABLE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerTable);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 60.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("1");

            // attributes


            // TODO: attributes



            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object InsertPageGrid(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("GRID");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerTable);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 60.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        #endregion

        #region IDiagramCreator

        public object CreatePin(double x, double y, int id, bool snap)
        {
            return null;
        }

        public object CreateWire(double x1, double y1, 
            double x2, double y2, 
            bool startVisible, bool endVisible, 
            bool startIsIO, bool endIsIO, 
            int id)
        {
            string str = null;
            
            double startX = x1;
            double startY = y1;
            double endX = x2;
            double endY = y2;

            double zet = LineCalc.CalculateZet(startX, startY, endX, endY);
            double sizeX = LineCalc.CalculateSizeX(InvertedCircleRadius, InvertedCircleThickness, zet);
            double sizeY = LineCalc.CalculateSizeY(InvertedCircleRadius, InvertedCircleThickness, zet);

            bool shortenStart = ShortenStart;
            bool shortenEnd = ShortenEnd;
            bool isStartIO = startIsIO;
            bool isEndIO = endIsIO;

            // shorten start
            if (isStartIO == true && isEndIO == false && shortenStart == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    startX = endX - ShortenLineSize;
                }
            }

            // shorten end
            if (isStartIO == false && isEndIO == true && shortenEnd == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    endX = startX + ShortenLineSize;
                }
            }

            // get start and end ellipse position
            Point ellipseStartCenter = LineCalc.GetEllipseStartCenter(startX, startY, sizeX, sizeY, startVisible);
            Point ellipseEndCenter = LineCalc.GetEllipseEndCenter(endX, endY, sizeX, sizeY, endVisible);

            // get line position
            Point lineStart = LineCalc.GetLineStart(startX, startY, sizeX, sizeY, startVisible);
            Point lineEnd = LineCalc.GetLineEnd(endX, endY, sizeX, sizeY, endVisible);

            if (startVisible == true)
            {
                str = DxfCircle(ellipseStartCenter.X, ellipseStartCenter.Y, 
                    InvertedCircleRadius,
                    0.0, 0.0, 
                    LayerWires, 
                    0, 
                    0.0, 891.0);

                DxfString.Append(str);
            }

            if (endVisible == true)
            {
                str = DxfCircle(ellipseEndCenter.X, ellipseEndCenter.Y,
                    InvertedCircleRadius,
                    0.0, 0.0,
                    LayerWires,
                    0,
                    0.0, 891.0);

                DxfString.Append(str);
            }

            str = DxfLine(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0.0, 0.0,
                LayerWires,
                0,
                0.0, 891.0);

            DxfString.Append(str);

            return null;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("INPUT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerIO);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes
            var tag = GetTagById(tagId);
            if (tag != null)
            {
                string str = null;

                // attributes follow: 0 - no, 1 - yes
                sb.AppendLine("");
                sb.AppendLine("66");
                sb.AppendLine("1");

                str = DxfAttribIO("ID", id.ToString(), x + 288.0, (PageHeight - y - 30), false);
                sb.Append(str);

                str = DxfAttribIO("TAGID", tag.Id.ToString(), x + 288.0, (PageHeight - y), false);
                sb.Append(str);

                str = DxfAttribIO("DESIGNATION", tag.Designation, x + 3.0, (PageHeight - y - 7.5), true);
                sb.Append(str);

                str = DxfAttribIO("DESCRIPTION", tag.Description, x + 3.0, (PageHeight - y - 21.5), true);
                sb.Append(str);

                str = DxfAttribIO("SIGNAL", tag.Signal, x + 213.0, (PageHeight - y - 7.5), true);
                sb.Append(str);

                str = DxfAttribIO("CONDITION", tag.Condition, x + 213.0, (PageHeight - y - 21.5), true);
                sb.Append(str);

                sb.AppendLine("0");
                sb.Append("SEQEND");
            }

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("OUTPUT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerIO);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes
            var tag = GetTagById(tagId);
            if (tag != null)
            {
                string str = null;

                // attributes follow: 0 - no, 1 - yes
                sb.AppendLine("");
                sb.AppendLine("66");
                sb.AppendLine("1");

                str = DxfAttribIO("ID", id.ToString(), x + 288.0, (PageHeight - y - 30), false);
                sb.Append(str);

                str = DxfAttribIO("TAGID", tag.Id.ToString(), x + 288.0, (PageHeight - y), false);
                sb.Append(str);

                str = DxfAttribIO("DESIGNATION", tag.Designation, x + 3.0, (PageHeight - y - 7.5), true);
                sb.Append(str);

                str = DxfAttribIO("DESCRIPTION", tag.Description, x + 3.0, (PageHeight - y - 21.5), true);
                sb.Append(str);

                str = DxfAttribIO("SIGNAL", tag.Signal, x + 213.0, (PageHeight - y - 7.5), true);
                sb.Append(str);

                str = DxfAttribIO("CONDITION", tag.Condition, x + 213.0, (PageHeight - y - 21.5), true);
                sb.Append(str);

                sb.AppendLine("0");
                sb.Append("SEQEND");
            }

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("ANDGATE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerElements);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            sb.AppendLine("");
            sb.AppendLine("66");
            sb.AppendLine("1");

            string str = null;

            str = DxfAttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false);
            sb.Append(str);

            str = DxfAttribGate("TEXT", "&", x + 15.0, (PageHeight - y - 15.0), true);
            sb.Append(str);

            sb.AppendLine("0");
            sb.Append("SEQEND");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("ORGATE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerElements);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToDxfString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToDxfString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            sb.AppendLine("");
            sb.AppendLine("66");
            sb.AppendLine("1");

            string str = null;

            str = DxfAttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false);
            sb.Append(str);

            str = DxfAttribGate("TEXT", "\\U+22651", x + 15.0, (PageHeight - y - 15.0), true);
            sb.Append(str);

            sb.AppendLine("0");
            sb.Append("SEQEND");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            return null;
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {

        }

        #endregion

        #region Generate Dxf

        public string GenerateDxfFromModel(string model)
        {
            var parser = new DiagramParser();

            var parseOptions = new ParseOptions()
            {
                OffsetX = 0.0,
                OffsetY = 0.0,
                AppendIds = false,
                UpdateIds = false,
                Select = false,
                CreateElements = true,
                Counter = null,
                Properties = null
            };

            DxfString = new StringBuilder();

            // header
            DxfString.Append(DxfHeader("Diagram"));

            // tables
            DxfString.Append(DxfTablesAll());

            // blocks
            DxfString.Append(DxfBlocksAll());

            // begin entities
            DxfString.Append(DxfEntities.DxfEntitiesBegin());

            // page frame
            InsertPageFrame(0.0, 0.0);

            // page grid
            InsertPageGrid(0.0, 0.0);

            // page table
            InsertPageTable(0.0, 0.0);

            // create entities
            var solution = parser.Parse(model, this, parseOptions);

            // end entities
            DxfString.Append(DxfEntities.DxfEntitiesEnd());

            // eof
            DxfString.Append(new DxfEof().ToString());

            return DxfString.ToString();
        }

        #endregion
    }

    #endregion
}
