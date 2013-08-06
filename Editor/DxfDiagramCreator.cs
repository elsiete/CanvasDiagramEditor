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
        private DxfAcadVer Version = DxfAcadVer.Default;
        private int HandleCounter = 0;

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

            var line = new DxfLine(Version, HandleCounter++)
            {
                Layer = layer,
                Color = color.ToString(),
                Thickness = thickness,
                StartPoint = new Vector3(_x1, _y1, 0.0),
                EndPoint = new Vector3(_x2, _y2, 0.0),
                ExtrusionDirection = new Vector3(0.0, 0.0, 1.0)
            };

            return line.Create();

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

            var circle = new DxfCircle(Version, HandleCounter++)
                .Layer(layer)
                .Color(color.ToString())
                .Thickness(thickness)
                .Radius(radius)
                .Center(new Vector3(_x, _y, 0.0));

            return circle;
        }

        private DxfAttdef AttdefIO(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef(Version, HandleCounter++)
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .DefaultValue(tag)
                .Prompt(tag)
                .Tag(tag)
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
            var attrib = new DxfAttrib(Version, HandleCounter++)
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .DefaultValue(text)
                .Tag(tag)
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
            var attdef = new DxfAttdef(Version, HandleCounter++)
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .DefaultValue(tag)
                .Prompt(tag)
                .Tag(tag)
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
            var attrib = new DxfAttrib(Version, HandleCounter++)
                .FirstAlignment(new Vector3(x, y, 0.0))
                .SecondAlignment(new Vector3(x, y, 0.0))
                .DefaultValue(text)
                .Tag(tag)
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
            var txt = new DxfText(Version, HandleCounter++)
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

        private IEnumerable<DxfAppid> TableAppids()
        {
            var appids = new List<DxfAppid>();

            // ACAD - default must be present
            if (Version > DxfAcadVer.AC1009)
            {
                var acad = new DxfAppid(Version, HandleCounter++)
                    .Application("ACAD")
                    .StandardFlags(DxfAppidStandardFlags.Default);

                appids.Add(acad);
            }

            // CADE - CAnvasDiagramEditor
            var cade = new DxfAppid(Version, HandleCounter++)
                .Application("CADE")
                .StandardFlags(DxfAppidStandardFlags.Default);

            appids.Add(cade);

            return appids;
        }

        private IEnumerable<DxfDimstyle> TableDimstyles()
        {
            return Enumerable.Empty<DxfDimstyle>();
        }

        private IEnumerable<DxfBlockRecord> TableBlockRecords()
        {
            return Enumerable.Empty<DxfBlockRecord>();
        }

        private IEnumerable<DxfLayer> TableLayers()
        {
            var layers = new List<DxfLayer>();

            // default layer 0 - must be present
            if (Version > DxfAcadVer.AC1009)
            {
                layers.Add(new DxfLayer(Version, HandleCounter++)
                {
                    Name = "0",
                    LayerStandardFlags = DxfLayerStandardFlags.Default,
                    Color = 0,
                    LineType = "Continuous",
                    PlottingFlag = true,
                    LineWeight = LineWeight.LnWtByLwDefault,
                    PlotStyleNameHandle = "0"
                }.Create());
            }

            // layer: FRAME
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerFrame,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 8,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: GRID
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerGrid,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 9,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());


            // layer: TABLE
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerTable,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 8,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: IO
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerIO,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 6,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt025,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: WIRES
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerWires,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 5,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt018,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: ELEMENTS
            layers.Add(new DxfLayer(Version, HandleCounter++)
            {
                Name = LayerElements,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = 5,
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = LineWeight.LnWt035,
                PlotStyleNameHandle = "0"
            }.Create());

            return layers;
        }

        private IEnumerable<DxfLtype> TableLtypes()
        {
            var ltypes = new List<DxfLtype>();

            // default ltypes ByLayer, ByBlock and Continuous - must be present

            // ByLayer
            ltypes.Add(new DxfLtype(Version, HandleCounter++)
            {
                Name = "ByLayer",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByLayer",
                DashLengthItems = 0,
                TotalPatternLenght = 0.0,
                DashLenghts = null,
            }.Create());

            // ByBlock
            ltypes.Add(new DxfLtype(Version, HandleCounter++)
            {
                Name = "ByBlock",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByBlock",
                DashLengthItems = 0,
                TotalPatternLenght = 0.0,
                DashLenghts = null,
            }.Create());

            // Continuous
            ltypes.Add(new DxfLtype(Version, HandleCounter++)
            {
                Name = "Continuous",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "Solid line",
                DashLengthItems = 0,
                TotalPatternLenght = 0.0,
                DashLenghts = null,
            }.Create());

            return ltypes;
        }

        private IEnumerable<DxfStyle> TableStyles()
        {
            var styles = new List<DxfStyle>();

            // style: TextFrameHeaderSmall
            styles.Add(new DxfStyle(Version, HandleCounter++)
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
            styles.Add(new DxfStyle(Version, HandleCounter++)
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
            styles.Add(new DxfStyle(Version, HandleCounter++)
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
            styles.Add(new DxfStyle(Version, HandleCounter++)
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
            styles.Add(new DxfStyle(Version, HandleCounter++)
                .Name("TextTableTag")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont));

            // style: TextElementGate
            styles.Add(new DxfStyle(Version, HandleCounter++)
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
            styles.Add(new DxfStyle(Version, HandleCounter++)
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

        private IEnumerable<DxfUcs> TableUcss()
        {
            return Enumerable.Empty<DxfUcs>();
        }

        private IEnumerable<DxfView> TableViews()
        {
            var view = new DxfView(Version, HandleCounter++)
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

        private IEnumerable<DxfVport> TableVports()
        {
            return Enumerable.Empty<DxfVport>();
        }

        #endregion

        #region Blocks

        public IEnumerable<DxfBlock> DefaultBlocks()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                var blocks = new List<DxfBlock>();

                blocks.Add(new DxfBlock(Version, HandleCounter++)
                    .Begin("*Model_Space", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(HandleCounter++, LayerTable));

                blocks.Add(new DxfBlock(Version, HandleCounter++)
                    .Begin("*Paper_Space", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(HandleCounter++, LayerTable));

                blocks.Add(new DxfBlock(Version, HandleCounter++)
                    .Begin("*Paper_Space0", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(HandleCounter++, LayerTable));

                return blocks;
            }

            return Enumerable.Empty<DxfBlock>();
        }

        public DxfBlock BlockFrame()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("FRAME", LayerFrame)
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

            return block.End(HandleCounter++, LayerFrame);
        }

        public DxfBlock BlockTable()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("TABLE", LayerTable)
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

            return block.End(HandleCounter++, LayerTable);
        }

        public DxfBlock BlockGrid()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("GRID", LayerGrid)
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            // TODO: lines

            return block.End(HandleCounter++, LayerGrid);
        }

        public DxfBlock BlockInput()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("INPUT", LayerIO)
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

            return block.End(HandleCounter++, LayerIO);
        }

        public DxfBlock BlockOutput()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("OUTPUT", LayerIO)
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

            return block.End(HandleCounter++, LayerIO);
        }

        public DxfBlock BlockAndGate()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("ANDGATE", LayerElements)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End(HandleCounter++, LayerElements);
        }

        public DxfBlock BlockOrGate()
        {
            var block = new DxfBlock(Version, HandleCounter++)
                .Begin("ORGATE", LayerElements)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End(HandleCounter++, LayerElements);
        }

        #endregion

        #region Page Frame,Table & Grid

        public DxfInsert CreateFrame(double x, double y)
        {
            var frame = new DxfInsert(Version, HandleCounter++)
                .Block("FRAME")
                .Layer(LayerFrame)
                .Insertion(new Vector3(x, PageHeight - 891.0 - y, 0));

            return frame;
        }

        public DxfInsert CreateTable(double x, double y)
        {
            var table = new DxfInsert(Version, HandleCounter++)
                .Block("TABLE")
                .Layer(LayerTable)
                .Insertion(new Vector3(x, PageHeight - 60.0 - y, 0));

            // TODO: attributes

            return table;
        }

        public DxfInsert CreateGrid(double x, double y)
        {
            var frame = new DxfInsert(Version, HandleCounter++)
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
            var insert = new DxfInsert(Version, HandleCounter++)
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
            var insert = new DxfInsert(Version, HandleCounter++)
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
            var insert = new DxfInsert(Version, HandleCounter++)
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
            var insert = new DxfInsert(Version, HandleCounter++)
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

        public string GenerateDxf(string model, DxfAcadVer version)
        {
            this.Version = version;
            this.HandleCounter = 0;

            // initialize parser
            var parser = new DiagramParser();
            var parseOptions = DefaultParseOptions();

            // create dxf file
            var dxf = new DxfFile(Version, HandleCounter++)
                .Comment("Diagram")
                .Header(new DxfHeader(Version, HandleCounter++).Begin().Default().End());

            // create tables
            var tables = new DxfTables(Version, HandleCounter++)
                .Begin()
                .AddAppidTable(TableAppids(), HandleCounter++)
                .AddDimstyleTable(TableDimstyles(), HandleCounter++)
                .AddBlockRecordTable(TableBlockRecords(), HandleCounter++)
                .AddLtypeTable(TableLtypes(), HandleCounter++)
                .AddLayerTable(TableLayers(), HandleCounter++)
                .AddStyleTable(TableStyles(), HandleCounter++)
                .AddUcsTable(TableUcss(), HandleCounter++)
                .AddViewTable(TableViews(), HandleCounter++)
                .AddVportTable(TableVports(), HandleCounter++)
                .End();

            dxf.Tables(tables);

            // create blocks
            var blocks = new DxfBlocks(Version, HandleCounter++)
                .Begin()
                .Add(DefaultBlocks())
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
            Entities = new DxfEntities(Version, HandleCounter++)
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
