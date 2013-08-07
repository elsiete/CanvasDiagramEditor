// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf;
using CanvasDiagramEditor.Dxf.Blocks;
using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Entities;
using CanvasDiagramEditor.Dxf.Classes;
using CanvasDiagramEditor.Dxf.Objects;
using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Tables;
using CanvasDiagramEditor.Util;
using CanvasDiagramEditor.Core;
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

        private DxfAcadVer Version = DxfAcadVer.AC1015;
        private int HandleCounter = 0;

        private DxfEntities Entities = null;

        private const double PageWidth = 1260.0;
        private const double PageHeight = 891.0;

        private const string LayerFrame = "FRAME";
        private const string LayerGrid = "GRID";
        private const string LayerTable = "TABLE";
        private const string LayerIO = "IO";
        private const string LayerWires = "WIRES";
        private const string LayerElements = "ELEMENTS";

        private string StylePrimatyFont = "arial.ttf"; // arialuni.ttf
        private string StylePrimatyFontDescription = "Arial"; // Arial Unicode MS

        private string StyleBigFont = "";

        private double ShortenLineSize = 15.0;
        private double InvertedCircleRadius = 4.0;
        private double InvertedCircleThickness = 0.0;

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
            string layer,  double pageOffsetX, double pageOffsetY)
        {
            double _x1 = pageOffsetX > 0.0 ? pageOffsetX - x1 + offsetX : x1 + offsetX;
            double _y1 = pageOffsetY > 0.0 ? pageOffsetY - y1 + offsetY : y1 + offsetY;
            double _x2 = pageOffsetX > 0.0 ? pageOffsetX - x2 + offsetX : x2 + offsetX;
            double _y2 = pageOffsetY > 0.0 ? pageOffsetY - y2 + offsetY : y2 + offsetY;

            double thickness = 0.0;

            //LineWeights.TryGetValue(layer, out thickness);

            var line = new DxfLine(Version, GetNextHandle())
            {
                Layer = layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
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
            string layer,
            double pageOffsetX, double pageOffsetY)
        {
            double _x = pageOffsetX > 0.0 ? pageOffsetX - x + offsetX : x + offsetX;
            double _y = pageOffsetY > 0.0 ? pageOffsetY - y + offsetY : y + offsetY;

            double thickness = 0.0;

            //LineWeights.TryGetValue(layer, out thickness);

            var circle = new DxfCircle(Version, GetNextHandle())
                .Layer(layer)
                .Color(DxfDefaultColors.ByLayer.ColorToString())
                .Thickness(thickness)
                .Radius(radius)
                .Center(new Vector3(_x, _y, 0.0));

            return circle;
        }

        private DxfAttdef AttdefIO(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef(Version, GetNextHandle())
            {
                Thickness = 0.0,
                Layer = LayerIO,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(x, y, 0.0),
                TextHeight = 6.0,
                DefaultValue = tag,
                TextRotation = 0.0,
                ScaleFactorX = 1.0,
                ObliqueAngle = 0.0,
                TextStyle = "TextElementIO",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Left,
                SecondAlignment = new Vector3(x, y, 0.0),
                ExtrusionDirection = new Vector3(0.0, 0.0, 1.0),
                Prompt = tag,
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = DxfVerticalTextJustification.Middle
            };

            return attdef.Create();
        }

        private DxfAttrib AttribIO(string tag, string text,
            double x, double y,
            bool isVisible)
        {
            var attrib = new DxfAttrib(Version, GetNextHandle())
                .Layer(LayerIO)
                .StartPoint(new Vector3(x, y, 0.0))
                .TextHeight(6.0)
                .DefaultValue(text)
                .TextStyle("TextElementIO")
                .HorizontalTextJustification(DxfHorizontalTextJustification.Left)
                .AlignmentPoint(new Vector3(x, y, 0.0))
                .Tag(tag)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
                .VerticalTextJustification(DxfVerticalTextJustification.Middle);
       
            return attrib;
        }

        private DxfAttdef AttdefGate(string tag, double x, double y, bool isVisible)
        {
            var attdef = new DxfAttdef(Version, GetNextHandle())
            {
                Thickness = 0.0,
                Layer = LayerElements,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(x, y, 0.0),
                TextHeight = 10.0,
                DefaultValue = tag,
                TextRotation = 0.0,
                ScaleFactorX = 1.0,
                ObliqueAngle = 0.0,
                TextStyle = "TextElementGate",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Center,
                SecondAlignment = new Vector3(x, y, 0.0),
                ExtrusionDirection = new Vector3(0.0, 0.0, 1.0),
                Prompt = tag,
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = DxfVerticalTextJustification.Middle
            };

            return attdef.Create();
        }

        private DxfAttrib AttribGate(string tag, string text,
            double x, double y,
            bool isVisible)
        {
            var attrib = new DxfAttrib(Version, GetNextHandle())
                .Layer(LayerElements)
                .StartPoint(new Vector3(x, y, 0.0))
                .TextHeight(10.0)
                .DefaultValue(text)
                .TextStyle("TextElementGate")
                .HorizontalTextJustification(DxfHorizontalTextJustification.Center)
                .AlignmentPoint(new Vector3(x, y, 0.0))
                .Tag(tag)
                .AttributeFlags(isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible)
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
            var txt = new DxfText(Version, GetNextHandle())
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

        private DxfBlockRecord CreateBlockRecordForBlock(string name)
        {
            var blockRecord = new DxfBlockRecord(Version, GetNextHandle())
            {
                Name = name
            };

            return blockRecord.Create();
        }

        private IEnumerable<DxfAppid> TableAppids()
        {
            var appids = new List<DxfAppid>();

            // ACAD - default must be present
            if (Version > DxfAcadVer.AC1009)
            {
                var acad = new DxfAppid(Version, GetNextHandle())
                    .Application("ACAD")
                    .StandardFlags(DxfAppidStandardFlags.Default);

                appids.Add(acad);
            }

            // CADE - CAnvasDiagramEditor
            var cade = new DxfAppid(Version, GetNextHandle())
                .Application("CADE")
                .StandardFlags(DxfAppidStandardFlags.Default);

            appids.Add(cade);

            return appids;
        }

        private IEnumerable<DxfDimstyle> TableDimstyles()
        {
            var dimstyles = new List<DxfDimstyle>();

            if (Version > DxfAcadVer.AC1009)
            {
                dimstyles.Add(new DxfDimstyle(Version, GetNextHandle())
                {
                    Name = "Standard"
                }.Create()); 
            }

            return dimstyles;

            //return Enumerable.Empty<DxfDimstyle>();
        }
        
        private IEnumerable<DxfLayer> TableLayers()
        {
            var layers = new List<DxfLayer>();

            // default layer 0 - must be present
            if (Version > DxfAcadVer.AC1009)
            {
                layers.Add(new DxfLayer(Version, GetNextHandle())
                {
                    Name = "0",
                    LayerStandardFlags = DxfLayerStandardFlags.Default,
                    Color = DxfDefaultColors.Default.ColorToString(),
                    LineType = "Continuous",
                    PlottingFlag = true,
                    LineWeight = DxfLineWeight.LnWtByLwDefault,
                    PlotStyleNameHandle = "0"
                }.Create());
            }

            // layer: FRAME
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerFrame,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.DarkGrey.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: GRID
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerGrid,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.LightGrey.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());


            // layer: TABLE
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerTable,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.DarkGrey.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt013,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: IO
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerIO,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.Magenta.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt025,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: WIRES
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerWires,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.Default.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt018,
                PlotStyleNameHandle = "0"
            }.Create());

            // layer: ELEMENTS
            layers.Add(new DxfLayer(Version, GetNextHandle())
            {
                Name = LayerElements,
                LayerStandardFlags = DxfLayerStandardFlags.Default,
                Color = DxfDefaultColors.Blue.ColorToString(),
                LineType = "Continuous",
                PlottingFlag = true,
                LineWeight = DxfLineWeight.LnWt035,
                PlotStyleNameHandle = "0"
            }.Create());

            return layers;
        }

        private IEnumerable<DxfLtype> TableLtypes()
        {
            var ltypes = new List<DxfLtype>();

            // default ltypes ByLayer, ByBlock and Continuous - must be present

            // ByLayer
            ltypes.Add(new DxfLtype(Version, GetNextHandle())
            {
                Name = "ByLayer",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByLayer",
                DashLengthItems = 0,
                TotalPatternLenght = 0.0,
                DashLenghts = null,
            }.Create());

            // ByBlock
            ltypes.Add(new DxfLtype(Version, GetNextHandle())
            {
                Name = "ByBlock",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByBlock",
                DashLengthItems = 0,
                TotalPatternLenght = 0.0,
                DashLenghts = null,
            }.Create());

            // Continuous
            ltypes.Add(new DxfLtype(Version, GetNextHandle())
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

            // style: Standard
            var standard = new DxfStyle(Version, GetNextHandle())
                .Name("Standard")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                standard.Add(1001, "ACAD");
                standard.Add(1000, StylePrimatyFontDescription);
                standard.Add(1071, 0);
            }

            styles.Add(standard);

            // style: TextFrameHeaderSmall
            var textFrameHeaderSmall = new DxfStyle(Version, GetNextHandle())
                .Name("TextFrameHeaderSmall")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textFrameHeaderSmall.Add(1001, "ACAD");
                textFrameHeaderSmall.Add(1000, StylePrimatyFontDescription);
                textFrameHeaderSmall.Add(1071, 0);
            }

            styles.Add(textFrameHeaderSmall);

            // style: TextFrameHeaderLarge
            var textFrameHeaderLarge = new DxfStyle(Version, GetNextHandle())
                .Name("TextFrameHeaderLarge")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textFrameHeaderLarge.Add(1001, "ACAD");
                textFrameHeaderLarge.Add(1000, StylePrimatyFontDescription);
                textFrameHeaderLarge.Add(1071, 0);
            }

            styles.Add(textFrameHeaderLarge);

            // style: TextFrameNumber
            var textFrameNumber = new DxfStyle(Version, GetNextHandle())
                .Name("TextFrameNumber")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textFrameNumber.Add(1001, "ACAD");
                textFrameNumber.Add(1000, StylePrimatyFontDescription);
                textFrameNumber.Add(1071, 0);
            }

            styles.Add(textFrameNumber);

            // style: TextTableHeader
            var textTableHeader = new DxfStyle(Version, GetNextHandle())
                .Name("TextTableHeader")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textTableHeader.Add(1001, "ACAD");
                textTableHeader.Add(1000, StylePrimatyFontDescription);
                textTableHeader.Add(1071, 0);
            }

            styles.Add(textTableHeader);

            // style: TextTableTag
            var textTableTag = new DxfStyle(Version, GetNextHandle())
                .Name("TextTableTag")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textTableTag.Add(1001, "ACAD");
                textTableTag.Add(1000, StylePrimatyFontDescription);
                textTableTag.Add(1071, 0);
            }

            styles.Add(textTableTag);

            // style: TextElementGate
            var textElementGate = new DxfStyle(Version, GetNextHandle())
                .Name("TextElementGate")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textElementGate.Add(1001, "ACAD");
                textElementGate.Add(1000, StylePrimatyFontDescription);
                textElementGate.Add(1071, 0);
            }

            styles.Add(textElementGate);

            // style: TextElementIO
            var textElementIO = new DxfStyle(Version, GetNextHandle())
                .Name("TextElementIO")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0.0)
                .WidthFactor(1.0)
                .ObliqueAngle(0.0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(0.0)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                textElementIO.Add(1001, "ACAD");
                textElementIO.Add(1000, StylePrimatyFontDescription);
                textElementIO.Add(1071, 0);
            }

            styles.Add(textElementIO);

            return styles;
        }

        private IEnumerable<DxfUcs> TableUcss()
        {
            return Enumerable.Empty<DxfUcs>();
        }

        private IEnumerable<DxfView> TableViews()
        {
            var view = new DxfView(Version, GetNextHandle())
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
            var vports = new List<DxfVport>();

            if (Version > DxfAcadVer.AC1009)
            {
                vports.Add(new DxfVport(Version, GetNextHandle())
                {
                    Name = "*Active"
                }.Create()); 
            }

            return vports;

            //return Enumerable.Empty<DxfVport>();
        }

        #endregion

        #region Blocks

        public IEnumerable<DxfBlock> DefaultBlocks()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                var blocks = new List<DxfBlock>();

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                    .Begin("*Model_Space", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(GetNextHandle(), LayerTable));

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                    .Begin("*Paper_Space", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(GetNextHandle(), LayerTable));

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                    .Begin("*Paper_Space0", "0")
                    .BlockTypeFlags(DxfBlockTypeFlags.Default)
                    .Base(new Vector3(0, 0, 0))
                    .End(GetNextHandle(), LayerTable));

                return blocks;
            }

            return Enumerable.Empty<DxfBlock>();
        }

        public DxfBlock BlockFrame()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("FRAME", LayerFrame)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            block.Add(Line(0.0, 20.0, 600.0, 20.0, 330.0, -15.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(600.0, 770.0, 0.0, 770.0, 330.0, -15.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 770.0, 0.0, 0.0, 330.0, -15.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(600.0, 0.0, 600.0, 770.0, 330.0, -15.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 15.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1245.0, 816.0, 15.0, 816.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 876.0, 1245.0, 876.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1245.0, 876.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(15.0, 15.0, 15.0, 876.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1.0, 1.0, 1259.0, 1.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1259.0, 890.0, 1.0, 890.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1.0, 890.0, 1.0, 1.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(1259.0, 1.0, 1259.0, 890.0, 0.0, 0.0, LayerFrame, pageOffsetX, pageOffsetY));

            block.Add(Line(30.0, 0.0, 30.0, 750.0, 15.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(240.0, 750.0, 240.0, 0.0, 15.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(315.0, 0.0, 0.0, 0.0, 15.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 750.0, 315.0, 750.0, 15.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));

            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                block.Add(Line(0.0, y, 315.0, y, 15.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            }

            block.Add(Line(210.0, 0.0, 210.0, 750.0, 930.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(285.0, 750.0, 285.0, 0.0, 930.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(315.0, 0.0, 0.0, 0.0, 930.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 750.0, 315.0, 750.0, 930.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));

            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                block.Add(Line(0.0, y, 315.0, y, 930.0, -35.0, LayerFrame, pageOffsetX, pageOffsetY));
            }

            // TODO: text

            return block.End(GetNextHandle(), LayerFrame);
        }

        public DxfBlock BlockTable()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("TABLE", LayerTable)
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            block.Add(Line(0.0, 15.0, 175.0, 15.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 15.0, 1230.0, 15.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(1230.0, 30.0, 965.0, 30.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(695.0, 30.0, 405.0, 30.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(175.0, 30.0, 0.0, 30.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(0.0, 45.0, 175.0, 45.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 45.0, 695.0, 45.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(965.0, 45.0, 1230.0, 45.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(30.0, 0.0, 30.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(75.0, 0.0, 75.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(175.0, 60.0, 175.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY)); 
            block.Add(Line(290.0, 0.0, 290.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(405.0, 60.0, 405.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(465.0, 0.0, 465.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(595.0, 60.0, 595.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(640.0, 0.0, 640.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(695.0, 60.0, 695.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(965.0, 0.0, 965.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY)); 
            block.Add(Line(1005.0, 60.0, 1005.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(1045.0, 0.0, 1045.0, 60.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));
            block.Add(Line(1100.0, 60.0, 1100.0, 0.0, 15.0, -1647.0, LayerTable, pageOffsetX, pageOffsetY));

            // TODO: text

            // TODO: attributes

            return block.End(GetNextHandle(), LayerTable);
        }

        public DxfBlock BlockGrid()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("GRID", LayerGrid)
                .BlockTypeFlags(DxfBlockTypeFlags.Default)
                .Base(new Vector3(0, 0, 0));

            // TODO: lines

            return block.End(GetNextHandle(), LayerGrid);
        }

        public DxfBlock BlockInput()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("INPUT", LayerIO)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));

            block.Add(AttdefIO("ID", 288, 30, false));
            block.Add(AttdefIO("TAGID", 288, 0, false));
            block.Add(AttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(AttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(AttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(AttdefIO("CONDITION", 213, 7.5, true));

            return block.End(GetNextHandle(), LayerIO);
        }

        public DxfBlock BlockOutput()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("OUTPUT", LayerIO)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));
 
            block.Add(Line(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0.0, 30.0));
            block.Add(Line(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0.0, 30.0));

            block.Add(AttdefIO("ID", 288, 30, false));
            block.Add(AttdefIO("TAGID", 288, 0, false));
            block.Add(AttdefIO("DESIGNATION", 3, 21.5, true));
            block.Add(AttdefIO("DESCRIPTION", 3, 7.5, true));
            block.Add(AttdefIO("SIGNAL", 213, 21.5, true));
            block.Add(AttdefIO("CONDITION", 213, 7.5, true));

            return block.End(GetNextHandle(), LayerIO);
        }

        public DxfBlock BlockAndGate()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("ANDGATE", LayerElements)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements,0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End(GetNextHandle(), LayerElements);
        }

        public DxfBlock BlockOrGate()
        {
            var block = new DxfBlock(Version, GetNextHandle())
                .Begin("ORGATE", LayerElements)
                .BlockTypeFlags(DxfBlockTypeFlags.NonConstantAttributes)
                .Base(new Vector3(0, 0, 0));

            block.Add(Line(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0.0, 30.0));
            block.Add(Line(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0.0, 30.0));
            block.Add(Line(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0.0, 30.0));
            block.Add(Line(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements,0.0, 30.0));

            block.Add(AttdefGate("ID", 30.0, 30.0, false));
            block.Add(AttdefGate("TEXT", 15.0, 15.0, true));

            return block.End(GetNextHandle(), LayerElements);
        }

        #endregion

        #region Page Frame,Table & Grid

        public DxfInsert CreateFrame(double x, double y)
        {
            var frame = new DxfInsert(Version, GetNextHandle())
                .Block("FRAME")
                .Layer(LayerFrame)
                .Insertion(new Vector3(x, PageHeight - 891.0 - y, 0));

            return frame;
        }

        public DxfInsert CreateTable(double x, double y)
        {
            var table = new DxfInsert(Version, GetNextHandle())
                .Block("TABLE")
                .Layer(LayerTable)
                .Insertion(new Vector3(x, PageHeight - 60.0 - y, 0));

            // TODO: attributes

            return table;
        }

        public DxfInsert CreateGrid(double x, double y)
        {
            var frame = new DxfInsert(Version, GetNextHandle())
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
                    0.0, 891.0);

                Entities.Add(circle);
            }

            if (endVisible == true)
            {
                var circle = Circle(ellipseEndCenter.X, ellipseEndCenter.Y,
                    InvertedCircleRadius,
                    0.0, 0.0,
                    LayerWires,
                    0.0, 891.0);

                Entities.Add(circle);
            }

            var line = Line(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0.0, 0.0,
                LayerWires,
                0.0, 891.0);

            Entities.Add(line);

            return null;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var insert = new DxfInsert(Version, GetNextHandle())
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
                    .AttributesEnd(GetNextHandle(), LayerIO);
            }

            Entities.Add(insert);

            return null;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var insert = new DxfInsert(Version, GetNextHandle())
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
                    .AttributesEnd(GetNextHandle(), LayerIO);
            }

            Entities.Add(insert);

            return null;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("ANDGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false))
                .AddAttribute(AttribGate("TEXT", "&", x + 15.0, (PageHeight - y - 15.0), true))
                .AttributesEnd(GetNextHandle(), LayerElements);

            Entities.Add(insert);

            return null;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("ORGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(x, PageHeight - 30.0 - y, 0));

            // Arial, arial.ttf, ≥, \U+2265
            // Arial Unicode MS, arialuni.ttf ≥, \U+2265

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, (PageHeight - y - 30), false))
                .AddAttribute(AttribGate("TEXT", "\\U+22651", x + 15.0, (PageHeight - y - 15.0), true))
                .AttributesEnd(GetNextHandle(), LayerElements);

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

        #region Parse Options

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

        #region Handle Counter

        private int GetNextHandle()
        {
            this.HandleCounter += 1;
            return this.HandleCounter;
        }

        private void ResetHandleCounter()
        {
            this.HandleCounter = 0;
        }

        #endregion

        #region Generate Dxf

        public string GenerateDxf(string model, DxfAcadVer version)
        {
            this.Version = version;
            ResetHandleCounter();

            // initialize parser
            var parser = new DiagramParser();
            var parseOptions = DefaultParseOptions();

            // dxf file sections
            DxfHeader header = null;
            DxfClasses classes = null;
            DxfTables tables = null;
            DxfBlocks blocks = null;
            DxfObjects objects = null;

            // create dxf file
            var dxf = new DxfFile(Version, GetNextHandle());
            
            // create header
            header = new DxfHeader(Version, GetNextHandle()).Begin().Default();

            // create classes
            if (Version > DxfAcadVer.AC1009)
            {
                classes = new DxfClasses(Version, GetNextHandle())
                    .Begin();

                // classes.Add(new DxfClass(...));

                classes.End();
            }

            // create tables
            tables = new DxfTables(Version, GetNextHandle());

            tables.Begin();
            tables.AddAppidTable(TableAppids(), GetNextHandle());
            tables.AddDimstyleTable(TableDimstyles(), GetNextHandle());

            if (Version > DxfAcadVer.AC1009)
            {
                var records = new List<DxfBlockRecord>();

                // TODO: each BLOCK must have BLOCK_RECORD entry

                // required block records by dxf format
                records.Add(CreateBlockRecordForBlock("*Model_Space"));
                records.Add(CreateBlockRecordForBlock("*Paper_Space"));
                records.Add(CreateBlockRecordForBlock("*Paper_Space0"));

                // canvas Diagram block records
                records.Add(CreateBlockRecordForBlock("FRAME"));
                records.Add(CreateBlockRecordForBlock("TABLE"));
                records.Add(CreateBlockRecordForBlock("GRID"));
                records.Add(CreateBlockRecordForBlock("INPUT"));
                records.Add(CreateBlockRecordForBlock("OUTPUT"));
                records.Add(CreateBlockRecordForBlock("ANDGATE"));
                records.Add(CreateBlockRecordForBlock("ORGATE"));

                tables.AddBlockRecordTable(records, GetNextHandle());
            }

            tables.AddLtypeTable(TableLtypes(), GetNextHandle());
            tables.AddLayerTable(TableLayers(), GetNextHandle());
            tables.AddStyleTable(TableStyles(), GetNextHandle());
            tables.AddUcsTable(TableUcss(), GetNextHandle());
            tables.AddViewTable(TableViews(), GetNextHandle());
            tables.AddVportTable(TableVports(), GetNextHandle());
            tables.End();

            // create blocks
            blocks = new DxfBlocks(Version, GetNextHandle())
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

            // create entities
            Entities = new DxfEntities(Version, GetNextHandle())
                .Begin()
                .Add(CreateFrame(0.0, 0.0))
                .Add(CreateGrid(0.0, 0.0))
                .Add(CreateTable(0.0, 0.0));

            parser.Parse(model, this, parseOptions);

            Entities.End();

            // create objects
            if (Version > DxfAcadVer.AC1009)
            {
                objects = new DxfObjects(Version, GetNextHandle()).Begin();

                // mamed dictionary
                var namedDict = new DxfDictionary(Version, GetNextHandle())
                {
                    OwnerDictionaryHandle = 0.ToDxfHandle(),
                    HardOwnerFlag = false,
                    DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting,
                    Entries = new Dictionary<string, string>()
                };

                // base dictionary
                var baseDict = new DxfDictionary(Version, GetNextHandle())
                {
                    OwnerDictionaryHandle = namedDict.Id.ToDxfHandle(),
                    HardOwnerFlag = false,
                    DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting,
                    Entries = new Dictionary<string, string>()
                };

                // add baseDict to namedDict
                namedDict.Entries.Add(baseDict.Id.ToDxfHandle(), "ACAD_GROUP");

                // TODO: add more named object dictionaries

                // finalize dictionaries
                objects.Add(namedDict.Create());
                objects.Add(baseDict.Create());

                // finalize objects
                objects.End();
            }

            // finalize dxf file

            dxf.Header(header.End(GetNextHandle()));

            if (Version > DxfAcadVer.AC1009)
            {
                dxf.Classes(classes);
            }

            dxf.Tables(tables);
            dxf.Blocks(blocks);
            dxf.Entities(Entities);

            if (Version > DxfAcadVer.AC1009)
            {
                dxf.Objects(objects);
            }

            dxf.Eof();

            // return dxf file contents
            return dxf.ToString();
        }

        #endregion
    }

    #endregion
}
