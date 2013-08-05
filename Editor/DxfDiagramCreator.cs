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

namespace CanvasDiagramEditor.Editor
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
    using CanvasDiagramEditor.Dxf.Blocks;

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

        public DxfLine Line(double x1, double y1,
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

            return line;

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

        public DxfCircle DxfCircle(double x, double y,
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

            return circle;
        }

        private DxfAttdef DxfAttdefIO(string tag, double x, double y, bool isVisible)
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

            return attdef;
        }

        private DxfAttrib AttribIO(string tag, string text,
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

            return attrib;
        }

        private DxfAttdef AttdefGate(string tag, double x, double y, bool isVisible)
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

            return attdef;
        }

        private DxfAttrib AttribGate(string tag, string text,
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

            return attrib;
        }

        private DxfText DxfTextGate(string text, 
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

            return txt;
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

            sb.Append(str);

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

        public DxfBlock DxfBlockFrame()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("FRAME")
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            block.Add(Line(0.0, 20.0, 600.0, 20.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(600.0, 770.0, 0.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 770.0, 0.0, 0.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(600.0, 0.0, 600.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 15.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1245.0, 816.0, 15.0, 816.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 876.0, 1245.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1245.0, 876.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 15.0, 15.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1.0, 1.0, 1259.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1259.0, 890.0, 1.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1.0, 890.0, 1.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1259.0, 1.0, 1259.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY));

            block.Add(Line(30.0, 0.0, 30.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(240.0, 750.0, 240.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(315.0, 0.0, 0.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 750.0, 315.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));

            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                block.Add(Line(0.0, y, 315.0, y, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            }

            block.Add(Line(210.0, 0.0, 210.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(285.0, 750.0, 285.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(315.0, 0.0, 0.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 750.0, 315.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));

            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                block.Add(Line(0.0, y, 315.0, y, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY));
            }

            // TODO: text

            return block.End();
        }

        public DxfBlock DxfBlockTable()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("TABLE")
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            block.Add(Line(0.0, 15.0, 175.0, 15.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 15.0, 1230.0, 15.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1230.0, 30.0, 965.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(695.0, 30.0, 405.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(175.0, 30.0, 0.0, 30.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 45.0, 175.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 45.0, 695.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(965.0, 45.0, 1230.0, 45.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(30.0, 0.0, 30.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(75.0, 0.0, 75.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(175.0, 60.0, 175.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY)); 
            block.Add(Line(290.0, 0.0, 290.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 60.0, 405.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(465.0, 0.0, 465.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(595.0, 60.0, 595.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(640.0, 0.0, 640.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(695.0, 60.0, 695.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(965.0, 0.0, 965.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY)); 
            block.Add(Line(1005.0, 60.0, 1005.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1045.0, 0.0, 1045.0, 60.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));
            block.Add(Line(1100.0, 60.0, 1100.0, 0.0, 15.0, -1647.0, LayerTable, 0, pageOffsetX, pageOffsetY));

            // TODO: text

            // TODO: attributes

            return block.End();
        }

        public DxfBlock DxfBlockGrid()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("GRID")
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            // TODO: lines

            return block.End();
        }

        public DxfBlock DxfBlockInput()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("INPUT")
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));

            block.Add(DxfAttdefIO("ID", 288, 30, false));
            block.Add(DxfAttdefIO("TAGID", 288, 0, false));
            block.Add(DxfAttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(DxfAttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(DxfAttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(DxfAttdefIO("CONDITION", 213, 7.5, true));

            return block.End();
        }

        public DxfBlock DxfBlockOutput()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("OUTPUT")
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));
 
            block.Add(Line(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0));

            block.Add(DxfAttdefIO("ID", 288, 30, false));
            block.Add(DxfAttdefIO("TAGID", 288, 0, false));
            block.Add(DxfAttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(DxfAttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(DxfAttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(DxfAttdefIO("CONDITION", 213, 7.5, true));

            return block.End();
        }

        public DxfBlock DxfBlockAndGate()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("ANDGATE")
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End();
        }

        public DxfBlock DxfBlockOrGate()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("ORGATE")
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End();
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
            if (isStartIO == true && 
                isEndIO == false && 
                shortenStart == true &&
                (Math.Round(startY, 1) == Math.Round(endY, 1)))
            {
                startX = endX - ShortenLineSize;
            }

            // shorten end
            if (isStartIO == false && 
                isEndIO == true && 
                shortenEnd == true &&
                 (Math.Round(startY, 1) == Math.Round(endY, 1)))
            {
                endX = startX + ShortenLineSize;
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
                    0.0, 891.0).ToString();

                DxfString.Append(str);
            }

            if (endVisible == true)
            {
                str = DxfCircle(ellipseEndCenter.X, ellipseEndCenter.Y,
                    InvertedCircleRadius,
                    0.0, 0.0,
                    LayerWires,
                    0,
                    0.0, 891.0).ToString();

                DxfString.Append(str);
            }

            str = Line(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0.0, 0.0,
                LayerWires,
                0,
                0.0, 891.0).ToString();

            DxfString.Append(str);

            return null;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var insert = new DxfInsert()
                .Block("INPUT")
                .Layer(LayerIO)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            var tag = GetTagById(tagId);
            if (tag != null)
            {
                insert.AttributesBegin()
                    .AddAttribute(AttribIO("ID", id.ToString(), x + 288.0, (PageHeight - y - 30), false))
                    .AddAttribute(AttribIO("TAGID", tag.Id.ToString(), x + 288.0, (PageHeight - y), false))
                    .AddAttribute(AttribIO("DESIGNATION", tag.Designation, x + 3.0, (PageHeight - y - 7.5), true))
                    .AddAttribute(AttribIO("DESCRIPTION", tag.Description, x + 3.0, (PageHeight - y - 21.5), true))
                    .AddAttribute(AttribIO("SIGNAL", tag.Signal, x + 213.0, (PageHeight - y - 7.5), true))
                    .AddAttribute(AttribIO("CONDITION", tag.Condition, x + 213.0, (PageHeight - y - 21.5), true))
                    .AttributesEnd();
            }

            DxfString.Append(insert.ToString());

            return null;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var insert = new DxfInsert()
                .Block("OUTPUT")
                .Layer(LayerIO)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            var tag = GetTagById(tagId);
            if (tag != null)
            {
                insert.AttributesBegin()
                    .AddAttribute(AttribIO("ID", id.ToString(), x + 288.0, (PageHeight - y - 30), false))
                    .AddAttribute(AttribIO("TAGID", tag.Id.ToString(), x + 288.0, (PageHeight - y), false))
                    .AddAttribute(AttribIO("DESIGNATION", tag.Designation, x + 3.0, (PageHeight - y - 7.5), true))
                    .AddAttribute(AttribIO("DESCRIPTION", tag.Description, x + 3.0, (PageHeight - y - 21.5), true))
                    .AddAttribute(AttribIO("SIGNAL", tag.Signal, x + 213.0, (PageHeight - y - 7.5), true))
                    .AddAttribute(AttribIO("CONDITION", tag.Condition, x + 213.0, (PageHeight - y - 21.5), true))
                    .AttributesEnd();
            }

            DxfString.Append(insert.ToString());

            return null;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var insert = new DxfInsert()
                .Block("ANDGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false))
                .AddAttribute(AttribGate("TEXT", "&", x + 15.0, (PageHeight - y - 15.0), true))
                .AttributesEnd();

            DxfString.Append(insert.ToString());

            return null;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var insert = new DxfInsert()
                .Block("ORGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false))
                .AddAttribute(AttribGate("TEXT", "\\U+22651", x + 15.0, (PageHeight - y - 15.0), true))
                .AttributesEnd();

            DxfString.Append(insert.ToString());

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
            var blocks = new DxfBlocks().Begin();

            blocks.Add(DxfBlockFrame());
            blocks.Add(DxfBlockTable());
            blocks.Add(DxfBlockGrid());
            blocks.Add(DxfBlockInput());
            blocks.Add(DxfBlockOutput());
            blocks.Add(DxfBlockAndGate());
            blocks.Add(DxfBlockOrGate());

            DxfString.Append(blocks.End().ToString());

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
