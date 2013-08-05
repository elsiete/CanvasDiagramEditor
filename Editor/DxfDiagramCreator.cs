// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf;
using CanvasDiagramEditor.Dxf.Blocks;
using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Entities;
using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Tables;
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

        private DxfEntities Entities = null;

        private const double PageWidth = 1260.0;
        private const double PageHeight = 891.0;

        private const string LayerFrame = "FRAME";
        private const string LayerGrid = "GRID";
        private const string LayerTable = "TABLE";
        private const string LayerIO = "IO";
        private const string LayerWires = "WIRES";
        private const string LayerElements = "ELEMENTS";

        private string StylePrimatyFont = "arial.ttf";
        private string StyleBigFont = "";

        private double ShortenLineSize = 15.0;
        private double InvertedCircleRadius = 4.0;
        private double InvertedCircleThickness = 0.0;

        //private Dictionary<string, double> LineWeights = new Dictionary<string, double>()
        //{
        //    { LayerFrame, PageThicknessMm },
        //    { LayerGrid, PageThicknessMm },
        //    { LayerTable, PageThicknessMm },
        //    { LayerIO, IOThicknessMm },
        //    { LayerWires, WireThicknessMm },
        //    { LayerElements, ElementThicknessMm }
        //};

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

        private DxfLine Line(double x1, double y1,
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

        private DxfCircle Circle(double x, double y,
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

        private DxfAttdef AttdefIO(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef()
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .Tag(tag)
                .DefaultValue(tag)
                .Prompt(tag)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .TextHeight(6.0)
                .TextStyle("TextElementIO")
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
                .TextStyle("TextElementIO")
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
                .TextStyle("TextElementGate")
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
                .TextStyle("TextElementGate")
                .Layer(LayerElements)
                .HorizontalTextJustification(DxfHorizontalTextJustification.Center)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);

            return attrib;
        }

        private DxfText Text(string text, 
            string style,
            string layer,
            double height,
            double x1, double y1,
            double x2, double y2,
            DxfHorizontalTextJustification horizontalJustification,
            DxfVerticalTextJustification verticalJustification)
        {
            var txt = new DxfText()
                .Layer(layer)
                .Text(text)
                .TextStyle(style)
                .TextHeight(height)
                .FirstAlignment(new Vector3(x1, y1, 0.0))
                .SecondAlignment(new Vector3(x2, y2, 0.0))
                .HorizontalTextJustification(horizontalJustification)
                .VerticalTextJustification(verticalJustification);

            return txt;
        }

        #endregion

        #region Tables

        private IEnumerable<DxfAppid> TableAppid()
        {
            var appid = new DxfAppid()
                .Application("CADE") // CAnvasDiagramEditor
                .StandardFlags(DxfAppidStandardFlags.Default);

            yield return appid;
        }

        private IEnumerable<DxfLayer> TableLayers()
        {
            var layers = new List<DxfLayer>();

            // layer: FRAME
            layers.Add(new DxfLayer()
                .Name(LayerFrame)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(8)
                .LineType("CONTINUOUS"));

            // layer: GRID
            layers.Add(new DxfLayer()
                .Name(LayerGrid)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(9)
                .LineType("CONTINUOUS"));

            // layer: TABLE
            layers.Add(new DxfLayer()
                .Name(LayerTable)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(8)
                .LineType("CONTINUOUS"));

            // layer: IO
            layers.Add(new DxfLayer()
                .Name(LayerIO)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(6)
                .LineType("CONTINUOUS"));

            // layer: WIRES
            layers.Add(new DxfLayer()
                .Name(LayerWires)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(5)
                .LineType("CONTINUOUS"));

            // layer: ELEMENTS
            layers.Add(new DxfLayer()
                .Name(LayerElements)
                .StandardFlags(DxfLayerFlags.Default)
                .Color(5)
                .LineType("CONTINUOUS"));

            return layers;
        }

        private IEnumerable<DxfLtype> TableLtypes()
        {
            var ltype = new DxfLtype()
                .Name("CONTINUOUS")
                .StandardFlags(DxfLtypeFlags.Default)
                .Description("Solid Line")
                .DashLengthItems(0)
                .TotalPatternLenght(0.0)
                .DashLenghts(null);

            yield return ltype;
        }

        private IEnumerable<DxfStyle> TablesStyles()
        {
            var styles = new List<DxfStyle>();

            // style: TextFrameHeaderSmall
            styles.Add(new DxfStyle()
                .Name("TextFrameHeaderSmall")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextFrameHeaderLarge
            styles.Add(new DxfStyle()
                .Name("TextFrameHeaderLarge")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextFrameNumber
            styles.Add(new DxfStyle()
                .Name("TextFrameNumber")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextTableHeader
            styles.Add(new DxfStyle()
                .Name("TextTableHeader")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextTableTag
            styles.Add(new DxfStyle()
                .Name("TextTableTag")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextFrameNumber
            styles.Add(new DxfStyle()
                .Name("TextFrameNumber")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextElementGate
            styles.Add(new DxfStyle()
                .Name("TextElementGate")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextElementIO
            styles.Add(new DxfStyle()
                .Name("TextElementIO")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            return styles;
        }

        private IEnumerable<DxfView> TableViews()
        {
            var view = new DxfView()
                .Name("DIAGRAM")
                .StandardFlags(DxfViewStandardFlags.Default)
                .Height(PageHeight)
                .Width(PageWidth)
                .Center(new Vector2(PageWidth / 2.0, PageHeight / 2.0))
                .ViewDirection(new Vector3(0.0, 0.0, 0.0))
                .TargetPoint(new Vector3(0.0, 0.0, 0.0))
                .FrontClippingPlane(0.0)
                .BackClippingPlane(0.0)
                .Twist(0.0);

            yield return view;
        }

        #endregion

        #region Blocks

        public DxfBlock BlockFrame()
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

        public DxfBlock BlockTable()
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

        public DxfBlock BlockGrid()
        {
            var block = new DxfBlock()
                .Begin()
                .Name("GRID")
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            // TODO: lines

            return block.End();
        }

        public DxfBlock BlockInput()
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

            block.Add(AttdefIO("ID", 288, 30, false));
            block.Add(AttdefIO("TAGID", 288, 0, false));
            block.Add(AttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(AttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(AttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(AttdefIO("CONDITION", 213, 7.5, true));

            return block.End();
        }

        public DxfBlock BlockOutput()
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

            block.Add(AttdefIO("ID", 288, 30, false));
            block.Add(AttdefIO("TAGID", 288, 0, false));
            block.Add(AttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(AttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(AttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(AttdefIO("CONDITION", 213, 7.5, true));

            return block.End();
        }

        public DxfBlock BlockAndGate()
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

        public DxfBlock BlockOrGate()
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

        #region Page Frame,Table & Grid

        public DxfInsert CreateFrame(double x, double y)
        {
            var frame = new DxfInsert()
                .Block("FRAME")
                .Layer(LayerFrame)
                .Insertion(new Vector3(x, PageHeight - 891.0 - y, 0));

            return frame;
        }

        public DxfInsert CreateTable(double x, double y)
        {
            var table = new DxfInsert()
                .Block("TABLE")
                .Layer(LayerTable)
                .Insertion(new Vector3(x, PageHeight - 60.0 - y, 0));

            // TODO: attributes

            return table;
        }

        public DxfInsert CreateGrid(double x, double y)
        {
            var frame = new DxfInsert()
                .Block("GRID")
                .Layer(LayerGrid)
                .Insertion(new Vector3(x, PageHeight - 60.0 - y, 0));

            return frame;
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
                var circle = Circle(ellipseStartCenter.X, ellipseStartCenter.Y, 
                    InvertedCircleRadius,
                    0.0, 0.0, 
                    LayerWires, 
                    0, 
                    0.0, 891.0);

                Entities.Add(circle);
            }

            if (endVisible == true)
            {
                var circle = Circle(ellipseEndCenter.X, ellipseEndCenter.Y,
                    InvertedCircleRadius,
                    0.0, 0.0,
                    LayerWires,
                    0,
                    0.0, 891.0);

                Entities.Add(circle);
            }

            var line = Line(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0.0, 0.0,
                LayerWires,
                0,
                0.0, 891.0);

            Entities.Add(line);

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

            Entities.Add(insert);

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

            Entities.Add(insert);

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

            Entities.Add(insert);

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

            Entities.Add(insert);

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

        public string GenerateDxf(string model)
        {
            // initialize parser
            var parser = new DiagramParser();
            var parseOptions = DefaultParseOptions();

            // create dxf file
            var dxf = new DxfFile()
                .Comment("Diagram")
                .Header(new DxfHeader().Begin().Default().End());

            // create tables
            var tables = new DxfTables()
                .Begin()
                .AddAppidTable(TableAppid())
                .AddLtypeTable(TableLtypes())
                .AddLayerTable(TableLayers())
                .AddStyleTable(TablesStyles())
                .AddViewTable(TableViews())
                .End();

            dxf.Tables(tables);

            // create blocks
            var blocks = new DxfBlocks()
                .Begin()
                .Add(BlockFrame())
                .Add(BlockTable())
                .Add(BlockGrid())
                .Add(BlockInput())
                .Add(BlockOutput())
                .Add(BlockAndGate())
                .Add(BlockOrGate())
                .End();

            dxf.Blocks(blocks);

            // create entities
            Entities = new DxfEntities()
                .Begin()
                .Add(CreateFrame(0.0, 0.0))
                .Add(CreateGrid(0.0, 0.0))
                .Add(CreateTable(0.0, 0.0));

            parser.Parse(model, this, parseOptions);

            Entities.End();
            dxf.Entities(Entities);

            // end of dxf file
            return dxf.Eof().ToString();
        }

        private ParseOptions DefaultParseOptions()
        {
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

            return parseOptions;
        }

        #endregion
    }

    #endregion
}
