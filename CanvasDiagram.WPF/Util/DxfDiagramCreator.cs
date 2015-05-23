// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf;
using CanvasDiagram.Dxf.Blocks;
using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Entities;
using CanvasDiagram.Dxf.Classes;
using CanvasDiagram.Dxf.Objects;
using CanvasDiagram.Dxf.Enums;
using CanvasDiagram.Dxf.Tables;
using CanvasDiagram.Util;
using CanvasDiagram.Core;
using CanvasDiagram.Core.Model;
using CanvasDiagram.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#endregion

namespace CanvasDiagram.WPF
{
    using FactoryFunc = Func<object[], double, double, bool, object>;

    #region DxfDiagramCreator

    public class DxfDiagramCreator : IDiagramCreator
    {
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

        private const double PageWidth = 1260;
        private const double PageHeight = 891;

        private const string LayerFrame = "FRAME";
        private const string LayerGrid = "GRID";
        private const string LayerTable = "TABLE";
        private const string LayerIO = "IO";
        private const string LayerWires = "WIRES";
        private const string LayerElements = "ELEMENTS";

        private string StylePrimatyFont = "arial.ttf"; // arialuni.ttf
        private string StylePrimatyFontDescription = "Arial"; // Arial Unicode MS

        private string StyleBigFont = "";

        private double ShortenLineSize = 15;
        private double InvertedCircleRadius = 4;
        private double InvertedCircleThickness = 0;

        #endregion

        #region Constructor

        public DxfDiagramCreator()
        {
            InitializeFactory();
        }

        #endregion

        #region Dxf X/Y-Coordinate Translate

        private double X(double x)
        {
            return x;
        }

        private double Y(double y)
        {
            return y == 0 ? y : -y;
        }

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
                    return tag;
            }

            return null;
        }

        #endregion

        #region Dxf Wrappers

        private DxfLine Line(double x1, double y1,
            double x2, double y2,
            double offsetX, double offsetY,
            string layer)
        {
            double _x1 = X(x1 + offsetX);
            double _y1 = Y(y1 + offsetY);
            double _x2 = X(x2 + offsetX);
            double _y2 = Y(y2 + offsetY);

            double thickness = 0;

            var line = new DxfLine(Version, GetNextHandle())
            {
                Layer = layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                Thickness = thickness,
                StartPoint = new Vector3(_x1, _y1, 0),
                EndPoint = new Vector3(_x2, _y2, 0),
                ExtrusionDirection = new Vector3(0, 0, 1)
            };

            return line.Create();
        }

        private DxfCircle Circle(double x, double y,
            double radius,
            double offsetX, double offsetY,
            string layer)
        {
            double _x = X(x + offsetX);
            double _y = Y(y + offsetY);

            double thickness = 0;

            var circle = new DxfCircle(Version, GetNextHandle())
                .Layer(layer)
                .Color(DxfDefaultColors.ByLayer.ColorToString())
                .Thickness(thickness)
                .Radius(radius)
                .Center(new Vector3(_x, _y, 0));

            return circle;
        }

        private DxfAttdef AttdefTable(string tag, double x, double y, 
            string defaultValue,
            bool isVisible,
            DxfHorizontalTextJustification horizontalTextJustification,
            DxfVerticalTextJustification verticalTextJustification)
        {
            var attdef = new DxfAttdef(Version, GetNextHandle())
            {
                Thickness = 0,
                Layer = LayerTable,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(X(x), Y(y), 0),
                TextHeight = 6,
                DefaultValue = defaultValue,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextTableTag",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = horizontalTextJustification,
                SecondAlignment = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                Prompt = tag,
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = verticalTextJustification
            };

            return attdef.Create();
        }

        private DxfAttrib AttribTable(string tag, string text,
            double x, double y,
            bool isVisible,
            DxfHorizontalTextJustification horizontalTextJustification,
            DxfVerticalTextJustification verticalTextJustification)
        {
            var attrib = new DxfAttrib(Version, GetNextHandle())
            {
                Thickness = 0,
                Layer = LayerTable,
                StartPoint = new Vector3(X(x), Y(y), 0),
                TextHeight = 6,
                DefaultValue = text,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextTableTag",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = horizontalTextJustification,
                AlignmentPoint = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = DxfVerticalTextJustification.Middle
            };

            return attrib.Create();
        }

        private DxfAttdef AttdefIO(string tag, double x, double y, 
            string defaultValue, bool isVisible)
        {
            var attdef = new DxfAttdef(Version, GetNextHandle())
            {
                Thickness = 0,
                Layer = LayerIO,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(X(x), Y(y), 0),
                TextHeight = 6,
                DefaultValue = defaultValue,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextElementIO",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Left,
                SecondAlignment = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
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
            {
                Thickness = 0,
                Layer = LayerIO,
                StartPoint = new Vector3(X(x), Y(y), 0),
                TextHeight = 6,
                DefaultValue = text,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextElementIO",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Left,
                AlignmentPoint = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = DxfVerticalTextJustification.Middle
            };

            return attrib.Create();
        }

        private DxfAttdef AttdefGate(string tag, double x, double y, 
            string defaultValue, bool isVisible)
        {
            var attdef = new DxfAttdef(Version, GetNextHandle())
            {
                Thickness = 0,
                Layer = LayerElements,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(X(x), Y(y), 0),
                TextHeight = 10,
                DefaultValue = defaultValue,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextElementGate",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Center,
                SecondAlignment = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
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
            {
                Thickness = 0,
                Layer = LayerElements,
                StartPoint = new Vector3(X(x), Y(y), 0),
                TextHeight = 10,
                DefaultValue = text,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = "TextElementGate",
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = DxfHorizontalTextJustification.Center,
                AlignmentPoint = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                Tag = tag,
                AttributeFlags = isVisible ? DxfAttributeFlags.Default : DxfAttributeFlags.Invisible,
                FieldLength = 0,
                VerticalTextJustification = DxfVerticalTextJustification.Middle
            };

            return attrib.Create();
        }

        private DxfText Text(string text, 
            string style,
            string layer,
            double height,
            double x, double y,
            DxfHorizontalTextJustification horizontalJustification,
            DxfVerticalTextJustification verticalJustification)
        {
            var txt = new DxfText(Version, GetNextHandle())
            {
                Thickness = 0,
                Layer = layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(X(x), Y(y), 0),
                TextHeight = height,
                DefaultValue = text,
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = style,
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = horizontalJustification,
                SecondAlignment = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                VerticalTextJustification = verticalJustification
            };

            return txt.Create();
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
                Color = 250.ToString(),
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
                Color = 251.ToString(),
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
                Color = 250.ToString(),
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
                Color = DxfDefaultColors.Default.ColorToString(),
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
                Color = DxfDefaultColors.Default.ColorToString(),
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
                TotalPatternLenght = 0,
                DashLenghts = null,
            }.Create());

            // ByBlock
            ltypes.Add(new DxfLtype(Version, GetNextHandle())
            {
                Name = "ByBlock",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByBlock",
                DashLengthItems = 0,
                TotalPatternLenght = 0,
                DashLenghts = null,
            }.Create());

            // Continuous
            ltypes.Add(new DxfLtype(Version, GetNextHandle())
            {
                Name = "Continuous",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "Solid line",
                DashLengthItems = 0,
                TotalPatternLenght = 0,
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
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
                .Center(new Vector2(PageWidth / 2, PageHeight / 2))
                .ViewDirection(new Vector3(0, 0, 1))
                .TargetPoint(new Vector3(0, 0, 0))
                .FrontClippingPlane(0)
                .BackClippingPlane(0)
                .Twist(0);

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
        }

        #endregion

        #region Blocks

        public IEnumerable<DxfBlock> DefaultBlocks()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                var blocks = new List<DxfBlock>();
                string layer = "0";

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                {
                    Name = "*Model_Space",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = GetNextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                {
                    Name = "*Paper_Space",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = GetNextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                blocks.Add(new DxfBlock(Version, GetNextHandle())
                {
                    Name = "*Paper_Space0",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = GetNextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                return blocks;
            }

            return Enumerable.Empty<DxfBlock>();
        }

        public DxfBlock BlockFrame()
        {
            var layer = LayerFrame;

            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "FRAME",
                Layer = layer,
                BlockTypeFlags = DxfBlockTypeFlags.Default,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "Page Frame",
                EndId = GetNextHandle(),
                EndLayer = layer,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            double left;
            double top;

            // frame

            left = 330;
            top = 1;

            entities.Add(Line(0, 30, 600, 30, left, top, layer));
            entities.Add(Line(600, 780, 0, 780, left, top, layer));
            entities.Add(Line(0, 780, 0, 0, left, top, layer));
            entities.Add(Line(600, 0, 600, 780, left, top, layer));

            left = 1;
            top = 1;

            entities.Add(Line(0, 0, 1258, 0, left, top, layer));
            entities.Add(Line(1258, 809, 0, 809, left, top, layer));
            entities.Add(Line(0, 889, 1258, 889, left, top, layer));
            entities.Add(Line(1258, 889, 1258, 0, left, top, layer));
            entities.Add(Line(0, 0, 0, 889, left, top, layer));

            // inputs

            left = 1;
            top = 31;

            entities.Add(Line(29, 0, 29, 750, left, top, layer));
            entities.Add(Line(239, 750, 239, 0, left, top, layer));
            entities.Add(Line(329, 0, 0, 0, left, top, layer));
            entities.Add(Line(0, 750, 329, 750, left, top, layer));

            for (double y = 30; y <= 720; y += 30)
                entities.Add(Line(0, y, 329, y, left, top, layer));

            // outputs

            left = 930;
            top = 31;

            entities.Add(Line(210, 0, 210, 750, left, top, layer));
            entities.Add(Line(300, 750, 300, 0, left, top, layer));
            entities.Add(Line(329, 0, 0, 0, left, top, layer));
            entities.Add(Line(0, 750, 329, 750, left, top, layer));

            for (double y = 30; y <= 720; y += 30)
                entities.Add(Line(0, y, 329, y, left, top, layer));

            // TODO: text headers

            left = 1.0;
            top = 1.0;

            entities.Add(Text("I N P U T S",
                "TextFrameHeaderLarge",
                layer,
                10,
                left + (329.0 / 2.0),
                top + 15.0,
                DxfHorizontalTextJustification.Center,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("F U N C T I O N",
                "TextFrameHeaderLarge",
                layer,
                10,
                left + (329.0 + (600.0 / 2.0)),
                top + 15.0,
                DxfHorizontalTextJustification.Center,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("O U T P U T S",
                "TextFrameHeaderLarge",
                layer,
                10,
                left + (329.0 + 600.0 + (329.0 / 2.0)),
                top + 15.0,
                DxfHorizontalTextJustification.Center,
                DxfVerticalTextJustification.Middle));

            //  text inputs

            left = 30.0 + 3.0;
            top = 31.0;

            entities.Add(Text("Designation", 
                "TextFrameHeaderSmall", 
                layer, 
                6, 
                left, 
                top + 7.5, 
                DxfHorizontalTextJustification.Left, 
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Description", 
                "TextFrameHeaderSmall", 
                layer, 
                6, 
                left, 
                top + 22.5, 
                DxfHorizontalTextJustification.Left, 
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Signal",
                "TextFrameHeaderSmall",
                layer,
                6,
                210.0 + left,
                top + 7.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Condition",
                "TextFrameHeaderSmall",
                layer,
                6,
                210.0 + left,
                top + 22.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            //  text inputs line numbers

            left = 1.0 + (29.0 / 2.0);
            top = 31.0 + 15.0;

            for (int n = 1; n <= 24; n++)
            {
                entities.Add(Text(n.ToString("D2"),
                    "TextFrameNumber",
                    layer,
                    8,
                    left,
                    top + ((double)n * 30.0),
                    DxfHorizontalTextJustification.Center,
                    DxfVerticalTextJustification.Middle));
            }

            // text outputs

            left = 930 + 3;
            top = 31.0;

            entities.Add(Text("Designation",
                "TextFrameHeaderSmall",
                layer,
                6,
                left,
                top + 7.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Description",
                "TextFrameHeaderSmall",
                layer,
                6,
                left,
                top + 22.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Signal",
                "TextFrameHeaderSmall",
                layer,
                6,
                210.0 + left,
                top + 7.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            entities.Add(Text("Condition",
                "TextFrameHeaderSmall",
                layer,
                6,
                210.0 + left,
                top + 22.5,
                DxfHorizontalTextJustification.Left,
                DxfVerticalTextJustification.Middle));

            //  text inputs line numbers

            left = 1230.0 + (29.0 / 2.0);
            top = 31.0 + 15.0;

            for (int n = 1; n <= 24; n++)
            {
                entities.Add(Text(n.ToString("D2"),
                    "TextFrameNumber",
                    layer,
                    8,
                    left,
                    top + ((double)n * 30.0),
                    DxfHorizontalTextJustification.Center,
                    DxfVerticalTextJustification.Middle));
            }

            return block.Create();
        }

        public DxfBlock BlockTable()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "TABLE",
                Layer = LayerTable,
                BlockTypeFlags = DxfBlockTypeFlags.NonConstantAttributes,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "Page Table",
                EndId = GetNextHandle(),
                EndLayer = LayerTable,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            entities.Add(Line(0, 20, 175, 20, 0, 0, LayerTable));
            entities.Add(Line(405, 20, 1258, 20, 0, 0, LayerTable));
            entities.Add(Line(1258, 40, 965, 40, 0, 0, LayerTable));
            entities.Add(Line(695, 40, 405, 40, 0, 0, LayerTable));
            entities.Add(Line(175, 40, 0, 40, 0, 0, LayerTable));
            entities.Add(Line(0, 60, 175, 60, 0, 0, LayerTable));
            entities.Add(Line(405, 60, 695, 60, 0, 0, LayerTable));
            entities.Add(Line(965, 60, 1258, 60, 0, 0, LayerTable));

            entities.Add(Line(30, 0, 30, 80, 0, 0, LayerTable));
            entities.Add(Line(75, 0, 75, 80, 0, 0, LayerTable));
            entities.Add(Line(175, 80, 175, 0, 0, 0, LayerTable)); 
            entities.Add(Line(290, 0, 290, 80, 0, 0, LayerTable));
            entities.Add(Line(405, 80, 405, 0, 0, 0, LayerTable));
            entities.Add(Line(465, 0, 465, 80, 0, 0, LayerTable));
            entities.Add(Line(595, 80, 595, 0, 0, 0, LayerTable));
            entities.Add(Line(640, 0, 640, 80, 0, 0, LayerTable));
            entities.Add(Line(695, 80, 695, 0, 0, 0, LayerTable));
            entities.Add(Line(965, 0, 965, 80, 0, 0, LayerTable)); 
            entities.Add(Line(1005, 80, 1005, 0, 0, 0, LayerTable));
            entities.Add(Line(1045, 0, 1045, 80, 0, 0, LayerTable));
            entities.Add(Line(1100, 80, 1100, 0, 0, 0, LayerTable));

            // TODO: table headers text

            // Rev.
            // Date
            // Remarks
            // Drawn
            // Checked
            // Approved
            // Rev.
            // Status
            // Page
            // Pages
            // Project
            // Order No
            // Doc. No
            // Arch. No

            // TODO: REFACTOR DxfText !!! and Text(...) method

            // TODO: table attributes

            entities.Add(AttdefTable("ID", 0, 0, "ID", false, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("TABLEID", 0, 0, "TABLEID", false, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION1_VERSION", 3, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION2_VERSION", 3, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION3_VERSION", 3, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION1_DATE", 33, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION2_DATE", 33, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION3_DATE", 33, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION1_REMARKS", 78, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION2_REMARKS", 78, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REVISION3_REMARKS", 78, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("DRAWN_NAME", 468, 10, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("CHECKED_NAME", 468, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("APPROVED_NAME", 468, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("DRAWN_DATE", 643, 10, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("CHECKED_DATE", 643, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("APPROVED_DATE", 643, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("TITLE", 698, 10, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("SUBTITLE1", 698, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("SUBTITLE2", 698, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("SUBTITLE3", 698, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("REV", 968, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("STATUS", 1008, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("PAGE", 968, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("PAGES", 1008, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("PROJECT", 1103, 10, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("ORDER_NO", 1103, 30, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("DOCUMENT_NO", 1103, 50, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));
            entities.Add(AttdefTable("ARCHIVE_NO", 1103, 70, "", true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle));

            return block.Create();
        }

        public DxfBlock BlockGrid()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "GRID",
                Layer = LayerGrid,
                BlockTypeFlags = DxfBlockTypeFlags.Default,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "Page Grid",
                EndId = GetNextHandle(),
                EndLayer = LayerGrid,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            // TODO: lines

            return block.Create();
        }

        public DxfBlock BlockInput()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "INPUT",
                Layer = LayerIO,
                BlockTypeFlags = DxfBlockTypeFlags.NonConstantAttributes,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "Input Signal",
                EndId = GetNextHandle(),
                EndLayer = LayerIO,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            entities.Add(Line(0, 0, 300, 0, 0, 0, LayerIO));
            entities.Add(Line(300, 30, 0, 30, 0, 0, LayerIO));
            entities.Add(Line(0, 30, 0, 0, 0, 0, LayerIO));
            entities.Add(Line(210, 0, 210, 30, 0, 0, LayerIO));
            entities.Add(Line(300, 30, 300, 0, 0, 0, LayerIO));

            double offsetX = 3;

            entities.Add(AttdefIO("ID", 300 + offsetX, 30, "ID", false));
            entities.Add(AttdefIO("TAGID", 300 + offsetX, 0, "TAGID", false));
            entities.Add(AttdefIO("DESIGNATION", offsetX, 7.5, "DESIGNATION", true));
            entities.Add(AttdefIO("DESCRIPTION", offsetX, 22.5, "DESCRIPTION", true));
            entities.Add(AttdefIO("SIGNAL", 210 + offsetX, 7.5, "SIGNAL", true));
            entities.Add(AttdefIO("CONDITION", 210 + offsetX, 22.5, "CONDITION", true));

            return block.Create();
        }

        public DxfBlock BlockOutput()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "OUTPUT",
                Layer = LayerIO,
                BlockTypeFlags = DxfBlockTypeFlags.NonConstantAttributes,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "Output Signal",
                EndId = GetNextHandle(),
                EndLayer = LayerIO,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            entities.Add(Line(0, 0, 300, 0, 0, 0, LayerIO));
            entities.Add(Line(300, 30, 0, 30, 0, 0, LayerIO));
            entities.Add(Line(0, 30, 0, 0, 0, 0, LayerIO));
            entities.Add(Line(210, 0, 210, 30, 0, 0, LayerIO));
            entities.Add(Line(300, 30, 300, 0, 0, 0, LayerIO));

            double offsetX = 3;

            entities.Add(AttdefIO("ID", 300 + offsetX, 30, "ID", false));
            entities.Add(AttdefIO("TAGID", 300 + offsetX, 0, "TAGID", false));
            entities.Add(AttdefIO("DESIGNATION", offsetX, 7.5, "DESIGNATION", true));
            entities.Add(AttdefIO("DESCRIPTION", offsetX, 22.5, "DESCRIPTION", true));
            entities.Add(AttdefIO("SIGNAL", 210 + offsetX, 7.5, "SIGNAL", true));
            entities.Add(AttdefIO("CONDITION", 210 + offsetX, 22.5, "CONDITION", true));

            return block.Create();
        }

        public DxfBlock BlockAndGate()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "ANDGATE",
                Layer = LayerElements,
                BlockTypeFlags = DxfBlockTypeFlags.NonConstantAttributes,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "AND Gate",
                EndId = GetNextHandle(),
                EndLayer = LayerElements,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            entities.Add(Line(0, 0, 30, 0, 0, 0, LayerElements));
            entities.Add(Line(0, 30, 30, 30, 0, 0, LayerElements));
            entities.Add(Line(0, 0, 0, 30, 0, 0, LayerElements));
            entities.Add(Line(30, 0, 30, 30, 0, 0, LayerElements));

            entities.Add(AttdefGate("ID", 30, 30, "ID", false));
            entities.Add(AttdefGate("TEXT", 15, 15, "&", true));

            return block.Create();
        }

        public DxfBlock BlockOrGate()
        {
            var block = new DxfBlock(Version, GetNextHandle())
            {
                Name = "ORGATE",
                Layer = LayerElements,
                BlockTypeFlags = DxfBlockTypeFlags.NonConstantAttributes,
                BasePoint = new Vector3(0, 0, 0),
                XrefPathName = null,
                Description = "OR Gate",
                EndId = GetNextHandle(),
                EndLayer = LayerElements,
                Entities = new List<object>()
            };

            var entities = block.Entities;

            entities.Add(Line(0, 0, 30, 0, 0, 0, LayerElements));
            entities.Add(Line(0, 30, 30, 30, 0, 0, LayerElements));
            entities.Add(Line(0, 0, 0, 30, 0, 0, LayerElements));
            entities.Add(Line(30, 0, 30, 30, 0, 0, LayerElements));

            entities.Add(AttdefGate("ID", 30, 30, "ID", false));
            entities.Add(AttdefGate("TEXT", 15, 15, "\\U+22651", true));

            return block.Create();
        }

        #endregion

        #region Page Frame,Table & Grid

        public DxfInsert CreateFrame(double x, double y)
        {
            var frame = new DxfInsert(Version, GetNextHandle())
                .Block("FRAME")
                .Layer(LayerFrame)
                .Insertion(new Vector3(X(x), Y(y), 0));

            return frame;
        }

        public DxfInsert CreateTable(double x, double y, DiagramTable table)
        {
            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("TABLE")
                .Layer(LayerTable)
                .Insertion(new Vector3(X(x), Y(y), 0));

            if (table != null)
            {
                insert.AttributesBegin()
                      .AddAttribute(AttribTable("ID", table.Id.ToString(), 0, 0, false, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("TABLEID", "", 0, 0, false, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION1_VERSION", table.Revision1.Version, x + 3, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION2_VERSION", table.Revision2.Version, x + 3, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION3_VERSION", table.Revision3.Version, x + 3, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION1_DATE", table.Revision1.Date, x + 33, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION2_DATE", table.Revision2.Date, x + 33, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION3_DATE", table.Revision3.Date, x + 33, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION1_REMARKS", table.Revision1.Remarks, x + 78, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION2_REMARKS", table.Revision2.Remarks, x + 78, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REVISION3_REMARKS", table.Revision3.Remarks, x + 78, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("DRAWN_NAME", table.Drawn.Name, x + 468, y + 10, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("CHECKED_NAME", table.Checked.Name, x + 468, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("APPROVED_NAME", table.Approved.Name, x + 468, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("DRAWN_DATE", table.Drawn.Date, x + 643, y + 10, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("CHECKED_DATE", table.Checked.Date, x + 643, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("APPROVED_DATE", table.Approved.Date, x + 643, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("TITLE", table.Title, x + 698, y + 10, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("SUBTITLE1", table.SubTitle1, x + 698, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("SUBTITLE2", table.SubTitle2, x + 698, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("SUBTITLE3", table.SubTitle3, x + 698, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("REV", table.Rev, x + 968, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("STATUS", table.Status, x + 1008, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("PAGE", table.Page, x + 968, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("PAGES", table.Pages, x + 1008, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("PROJECT", table.Project, x + 1103, y + 10, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("ORDER_NO", table.OrderNo, x + 1103, y + 30, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("DOCUMENT_NO", table.DocumentNo, x + 1103, y + 50, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AddAttribute(AttribTable("ARCHIVE_NO", table.ArchiveNo, x + 1103, y + 70, true, DxfHorizontalTextJustification.Left, DxfVerticalTextJustification.Middle))
                      .AttributesEnd(GetNextHandle(), LayerTable);
            }

            return insert;
        }

        public DxfInsert CreateGrid(double x, double y)
        {
            var frame = new DxfInsert(Version, GetNextHandle())
                .Block("GRID")
                .Layer(LayerGrid)
                .Insertion(new Vector3(X(x), Y(y), 0));

            return frame;
        }

        #endregion

        #region Factory

        private Dictionary<string, FactoryFunc> Factory { get; set; }

        private void InitializeFactory()
        {
            Factory = new Dictionary<string, FactoryFunc>()
            {
                {  Constants.TagElementPin, CreatePin },
                {  Constants.TagElementWire, CreateWire },
                {  Constants.TagElementInput, CreateInput },
                {  Constants.TagElementOutput, CreateOutput },
                {  Constants.TagElementAndGate, CreateAndGate },
                {  Constants.TagElementOrGate, CreateOrGate },
            };
        }

        private object CreatePin(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 1)
                return null;

            int id = (int)data[0];

            // TODO:

            return null;
        }

        private object CreateWire(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 9)
                return null;

            double x1 = (double)data[0];
            double y1 = (double)data[1];
            double x2 = (double)data[2];
            double y2 = (double)data[3];
            bool startVisible = (bool)data[4];
            bool endVisible = (bool)data[5];
            bool startIsIO = (bool)data[6];
            bool endIsIO = (bool)data[7];
            int id = (int)data[8];

            double sx = x1;
            double sy = y1;
            double ex = x2;
            double ey = y2;

            double zet = LineUtil.Zet(sx, sy, ex, ey);
            double width = LineUtil.Width(InvertedCircleRadius, InvertedCircleThickness, zet);
            double height = LineUtil.Height(InvertedCircleRadius, InvertedCircleThickness, zet);

            bool shortenStart = ShortenStart;
            bool shortenEnd = ShortenEnd;
            bool isStartIO = startIsIO;
            bool isEndIO = endIsIO;

            // shorten start
            if (isStartIO == true && isEndIO == false && shortenStart == true &&
                (Math.Round(sy, 1) == Math.Round(ey, 1)))
            {
                sx = ex - ShortenLineSize;
            }

            // shorten end
            if (isStartIO == false && isEndIO == true && shortenEnd == true &&
                 (Math.Round(sy, 1) == Math.Round(ey, 1)))
            {
                ex = sx + ShortenLineSize;
            }

            // get ellipse position
            IPoint ellipseStart = LineUtil.EllipseStart(sx, sy, width, height, startVisible);
            IPoint ellipseEnd = LineUtil.EllipseEnd(ex, ey, width, height, endVisible);

            // get line position
            IPoint lineStart = LineUtil.LineStart(sx, sy, width, height, startVisible);
            IPoint lineEnd = LineUtil.LineEnd(ex, ey, width, height, endVisible);

            if (startVisible == true)
            {
                var circle = Circle(ellipseStart.X, ellipseStart.Y,
                    InvertedCircleRadius,
                    0, 0,
                    LayerWires);

                Entities.Add(circle);
            }

            if (endVisible == true)
            {
                var circle = Circle(ellipseEnd.X, ellipseEnd.Y,
                    InvertedCircleRadius,
                    0, 0,
                    LayerWires);

                Entities.Add(circle);
            }

            var line = Line(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0, 0,
                LayerWires);

            Entities.Add(line);

            return null;
        }

        private object CreateInput(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 2)
                return null;

            int id = (int)data[0];
            int tagId = (int)data[1];

            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("INPUT")
                .Layer(LayerIO)
                .Insertion(new Vector3(X(x), Y(y), 0));

            var tag = GetTagById(tagId);
            if (tag != null)
            {
                double offsetX = 3;

                insert.AttributesBegin()
                      .AddAttribute(AttribIO("ID", id.ToString(), x + 300 + offsetX, y + 30, false))
                      .AddAttribute(AttribIO("TAGID", tag.Id.ToString(), x + 300 + offsetX, y, false))
                      .AddAttribute(AttribIO("DESIGNATION", tag.Designation, x + offsetX, y + 7.5, true))
                      .AddAttribute(AttribIO("DESCRIPTION", tag.Description, x + offsetX, y + 22.5, true))
                      .AddAttribute(AttribIO("SIGNAL", tag.Signal, x + 210 + offsetX, y + 7.5, true))
                      .AddAttribute(AttribIO("CONDITION", tag.Condition, x + 210 + offsetX, y + 22.5, true))
                      .AttributesEnd(GetNextHandle(), LayerIO);
            }

            Entities.Add(insert);

            return null;
        }

        private object CreateOutput(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 2)
                return null;

            int id = (int)data[0];
            int tagId = (int)data[1];

            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("OUTPUT")
                .Layer(LayerIO)
                .Insertion(new Vector3(X(x), Y(y), 0));

            var tag = GetTagById(tagId);
            if (tag != null)
            {
                double offsetX = 3;

                insert.AttributesBegin()
                      .AddAttribute(AttribIO("ID", id.ToString(), x + 300 + offsetX, y + 30, false))
                      .AddAttribute(AttribIO("TAGID", tag.Id.ToString(), x + 300 + offsetX, y, false))
                      .AddAttribute(AttribIO("DESIGNATION", tag.Designation, x + offsetX, y + 7.5, true))
                      .AddAttribute(AttribIO("DESCRIPTION", tag.Description, x + offsetX, y + 22.5, true))
                      .AddAttribute(AttribIO("SIGNAL", tag.Signal, x + 210 + offsetX, y + 7.5, true))
                      .AddAttribute(AttribIO("CONDITION", tag.Condition, x + 210 + offsetX, y + 22.5, true))
                      .AttributesEnd(GetNextHandle(), LayerIO);
            }

            Entities.Add(insert);

            return null;
        }

        private object CreateAndGate(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 1)
                return null;

            int id = (int)data[0];

            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("ANDGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(X(x), Y(y), 0));

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, y + 30, false))
                .AddAttribute(AttribGate("TEXT", "&", x + 15, y + 15, true))
                .AttributesEnd(GetNextHandle(), LayerElements);

            Entities.Add(insert);

            return null;
        }

        private object CreateOrGate(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 1)
                return null;

            int id = (int)data[0];

            var insert = new DxfInsert(Version, GetNextHandle())
                .Block("ORGATE")
                .Layer(LayerElements)
                .Insertion(new Vector3(X(x), Y(y), 0));

            // Arial, arial.ttf, ≥, \U+2265
            // Arial Unicode MS, arialuni.ttf ≥, \U+2265

            insert.AttributesBegin()
                .AddAttribute(AttribGate("ID", id.ToString(), x + 30, y + 30, false))
                .AddAttribute(AttribGate("TEXT", "\\U+22651", x + 15, y + 15, true))
                .AttributesEnd(GetNextHandle(), LayerElements);

            Entities.Add(insert);

            return null;
        }

        #endregion

        #region IDiagramCreator

        public void SetCanvas(ICanvas canvas)
        {
        }

        public ICanvas GetCanvas()
        {
            return null;
        }

        public object CreateElement(string type, object[] data, double x, double y, bool snap)
        {
            FactoryFunc func;
            bool result = Factory.TryGetValue(type, out func);

            if (result == true && func != null)
                return func(data, x, y, snap);

            return null;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            return null;
        }

        public object CreateGrid(double originX,
            double originY,
            double width,
            double height,
            double size)
        {
            return null;
        }

        public void UpdateConnections(IDictionary<string, Child> dict)
        {
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
        }

        public void AppendIds(IEnumerable<object> elements)
        {
        }

        public void InsertElements(IEnumerable<object> elements, 
            bool select,
            double offsetX,
            double offsetY)
        {
        }

        #endregion

        #region Parse Options

        private ParseOptions DefaultParseOptions()
        {
            var parseOptions = new ParseOptions()
            {
                OffsetX = 0,
                OffsetY = 0,
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

        public string GenerateDxf(string model, DxfAcadVer version, DiagramTable table)
        {
            this.Version = version;

            ResetHandleCounter();

            // initialize parser
            var parser = new Parser();
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
                .Add(CreateFrame(0, 0))
                .Add(CreateGrid(330, 31))
                .Add(CreateTable(1, 810, table));

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
                dxf.Classes(classes);

            dxf.Tables(tables);
            dxf.Blocks(blocks);
            dxf.Entities(Entities);

            if (Version > DxfAcadVer.AC1009)
                dxf.Objects(objects);

            dxf.Eof();

            // return dxf file contents
            return dxf.ToString();
        }

        #endregion
    }

    #endregion
}
