#region References

using CanvasDiagramEditor;
using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Windows;
using A = DocumentFormat.OpenXml.Drawing;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Ds = DocumentFormat.OpenXml.CustomXmlDataProperties;
using M = DocumentFormat.OpenXml.Math;
using Ovml = DocumentFormat.OpenXml.Vml.Office;
using V = DocumentFormat.OpenXml.Vml;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using Wp = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Wpc = DocumentFormat.OpenXml.Office2010.Word.DrawingCanvas;
using Wps = DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
using Wvml = DocumentFormat.OpenXml.Vml.Wordprocessing;

#endregion

namespace CanvasDiagramEditor.Export
{
    #region OpenXmlExport

    public class OpenXmlExport : IDiagramExport
    {
        #region Open XmlOpen XML SDK 2.5 for Microsoft Office

        // Conversion factor from XAML units to Centimenters, 30 XAML units = 1.0cm
        private static double xamlToCm = 30;

        // 1 Centimeter = 360000 EMUs
        private static double emu_1cm = 360000L;

        public void CreateDocument(string filePath, IEnumerable<string> diagrams)
        {
            using (WordprocessingDocument document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();

                Document document1 = CreateDocument();

                Body body1 = new Body();

                foreach (var diagram in diagrams)
                {
                    Paragraph paragraph1 = CreateParagraph(diagram);
                    SectionProperties sectionProperties1 = CreateSectionProperties();

                    body1.Append(paragraph1);
                    body1.Append(sectionProperties1);
                }

                document1.Append(body1);

                mainPart.Document = document1;
            }
        }

        private static Document CreateDocument()
        {
            Document document1 = new Document() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "w14 w15 wp14" } };

            document1.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
            document1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            document1.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
            document1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            document1.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
            document1.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
            document1.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
            document1.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            document1.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
            document1.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            document1.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            document1.AddNamespaceDeclaration("w15", "http://schemas.microsoft.com/office/word/2012/wordml");
            document1.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
            document1.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
            document1.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
            document1.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");

            return document1;
        }

        private static Paragraph CreateParagraph(string diagram)
        {
            Paragraph paragraph1 = new Paragraph() { RsidParagraphAddition = "00DD51F1", RsidParagraphProperties = "00170F2C", RsidRunAdditionDefault = "00566780" };

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };

            paragraphProperties1.Append(spacingBetweenLines1);

            BookmarkStart bookmarkStart1 = new BookmarkStart() { Name = "_GoBack", Id = "0" };

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            NoProof noProof1 = new NoProof();
            Languages languages1 = new Languages() { EastAsia = "pl-PL" };

            runProperties1.Append(noProof1);
            runProperties1.Append(languages1);

            AlternateContent alternateContent1 = new AlternateContent();

            AlternateContentChoice alternateContentChoice1 = new AlternateContentChoice() { Requires = "wpc" };

            Drawing drawing1 = CreateDrawing(diagram);

            alternateContentChoice1.Append(drawing1);

            //AlternateContentFallback alternateContentFallback1 = new AlternateContentFallback();

            //Picture picture1 = new Picture();

            //V.Group group1 = CreateGroup();
            //picture1.Append(group1);

            //alternateContentFallback1.Append(picture1);

            alternateContent1.Append(alternateContentChoice1);
            //alternateContent1.Append(alternateContentFallback1);

            run1.Append(runProperties1);
            run1.Append(alternateContent1);

            BookmarkEnd bookmarkEnd1 = new BookmarkEnd() { Id = "0" };

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(bookmarkStart1);
            paragraph1.Append(run1);
            paragraph1.Append(bookmarkEnd1);

            return paragraph1;
        }

        private static SectionProperties CreateSectionProperties()
        {
            SectionProperties sectionProperties1 = new SectionProperties() { RsidR = "00DD51F1", RsidSect = "00170F2C" };

            PageSize pageSize1 = new PageSize() { Width = (UInt32Value)16838U, Height = (UInt32Value)11906U, Orient = PageOrientationValues.Landscape };
            PageMargin pageMargin1 = new PageMargin() { Top = 1418, Right = (UInt32Value)1332U, Bottom = 1418, Left = (UInt32Value)1332U, Header = (UInt32Value)709U, Footer = (UInt32Value)709U, Gutter = (UInt32Value)0U };
            Columns columns1 = new Columns() { Space = "708" };
            DocGrid docGrid1 = new DocGrid() { LinePitch = 360 };

            sectionProperties1.Append(pageSize1);
            sectionProperties1.Append(pageMargin1);
            sectionProperties1.Append(columns1);
            sectionProperties1.Append(docGrid1);

            return sectionProperties1;
        }

        private static Drawing CreateDrawing(string diagram)
        {
            Drawing drawing1 = new Drawing();

            Wp.Inline inline1 = new Wp.Inline() { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U };
            Wp.Extent extent1 = new Wp.Extent() { Cx = 9000000L, Cy = 5760000L };
            Wp.EffectExtent effectExtent1 = new Wp.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L };
            Wp.DocProperties docProperties1 = new Wp.DocProperties() { Id = (UInt32Value)2U, /* Name = "Kanwa 2" */ Name = Guid.NewGuid().ToString() };

            Wp.NonVisualGraphicFrameDrawingProperties nonVisualGraphicFrameDrawingProperties1 = new Wp.NonVisualGraphicFrameDrawingProperties();

            A.GraphicFrameLocks graphicFrameLocks1 = new A.GraphicFrameLocks();
            graphicFrameLocks1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            nonVisualGraphicFrameDrawingProperties1.Append(graphicFrameLocks1);

            A.Graphic graphic1 = new A.Graphic();

            graphic1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            A.GraphicData graphicData1 = new A.GraphicData() { Uri = "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas" };

            Wpc.WordprocessingCanvas wordprocessingCanvas1 = CreateCanvas(diagram);

            graphicData1.Append(wordprocessingCanvas1);

            graphic1.Append(graphicData1);

            inline1.Append(extent1);
            inline1.Append(effectExtent1);
            inline1.Append(docProperties1);
            inline1.Append(nonVisualGraphicFrameDrawingProperties1);
            inline1.Append(graphic1);

            drawing1.Append(inline1);

            return drawing1;
        }

        private static Wpc.WordprocessingCanvas CreateCanvas(string diagram)
        {
            Wpc.WordprocessingCanvas wordprocessingCanvas1 = new Wpc.WordprocessingCanvas();

            Wpc.BackgroundFormatting backgroundFormatting1 = new Wpc.BackgroundFormatting();
            Wpc.WholeFormatting wholeFormatting1 = new Wpc.WholeFormatting();

            //Wps.WordprocessingShape wordprocessingShape1 = CreateWpShape1_AndGate();
            //Wps.WordprocessingShape wordprocessingShape2 = CreateWpShape2_Wire();

            wordprocessingCanvas1.Append(backgroundFormatting1);
            wordprocessingCanvas1.Append(wholeFormatting1);

            //wordprocessingCanvas1.Append(wordprocessingShape1);
            //wordprocessingCanvas1.Append(wordprocessingShape2);

            CreateElements(wordprocessingCanvas1, diagram);

            return wordprocessingCanvas1;
        }

        private static void CreateElements(Wpc.WordprocessingCanvas wordprocessingCanvas, string diagram)
        {
            string name = null;
            var lines = diagram.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            var elements = new List<Action>();
            var wires = new List<Action>();

            foreach (var line in lines)
            {
                var args = line.Split(ModelConstants.ArgumentSeparator);
                int length = args.Length;

                if (length >= 2)
                {
                    name = args[1];

                    if (StringUtil.Compare(args[0], ModelConstants.PrefixRoot))
                    {
                        if (StringUtil.StartsWith(name, ModelConstants.TagElementPin) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                //Wps.WordprocessingShape wordprocessingShape = CreateWpShapePin(x / xamlToCm, y / xamlToCm);
                                //wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (StringUtil.StartsWith(name, ModelConstants.TagElementInput) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeInput(x / xamlToCm, y / xamlToCm, "Input");
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (StringUtil.StartsWith(name, ModelConstants.TagElementOutput) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeOutput(x / xamlToCm, y / xamlToCm, "Output");
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (StringUtil.StartsWith(name, ModelConstants.TagElementAndGate) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeAndGate(x / xamlToCm, y / xamlToCm);
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (StringUtil.StartsWith(name, ModelConstants.TagElementOrGate) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeOrGate(x / xamlToCm, y / xamlToCm);
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                            (length == 6 || length == 8))
                        {
                            float x1 = float.Parse(args[2]);
                            float y1 = float.Parse(args[3]);
                            float x2 = float.Parse(args[4]);
                            float y2 = float.Parse(args[5]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            bool isStartVisible = false;
                            bool isEndVisible = false;

                            if (length == 8)
                            {
                                isStartVisible = bool.Parse(args[6]);
                                isEndVisible = bool.Parse(args[7]);
                            }

                            double radius = 4.0;
                            double thickness = 1.0 / 2.0;

                            double startX = x1;
                            double startY = y1;
                            double endX = x2;
                            double endY = y2;

                            double zet = LineExCalc.CalculateZet(startX, startY, endX, endY);
                            double sizeX = LineExCalc.CalculateSizeX(radius, thickness, zet);
                            double sizeY = LineExCalc.CalculateSizeY(radius, thickness, zet);

                            Point ellipseStartCenter = LineExCalc.GetEllipseStartCenter(startX, startY, sizeX, sizeY, isStartVisible);
                            Point ellipseEndCenter = LineExCalc.GetEllipseEndCenter(endX, endY, sizeX, sizeY, isEndVisible);
                            Point lineStart = LineExCalc.GetLineStart(startX, startY, sizeX, sizeY, isStartVisible);
                            Point lineEnd = LineExCalc.GetLineEnd(endX, endY, sizeX, sizeY, isEndVisible);

                            wires.Add(() =>
                            {
                                // line
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeWire(
                                    lineStart.X / xamlToCm, lineStart.Y / xamlToCm,
                                    lineEnd.X / xamlToCm, lineEnd.Y / xamlToCm,
                                    isStartVisible, isEndVisible);

                                wordprocessingCanvas.Append(wordprocessingShape);

                                // start inverter
                                if (isStartVisible == true)
                                {
                                    Wps.WordprocessingShape wordprocessingShapeInverter = CreateWpShapeInverter(
                                        ellipseStartCenter.X / xamlToCm, ellipseStartCenter.Y / xamlToCm);

                                    wordprocessingCanvas.Append(wordprocessingShapeInverter);
                                }

                                // end inverter
                                if (isEndVisible == true)
                                {
                                    Wps.WordprocessingShape wordprocessingShapeInverter = CreateWpShapeInverter(
                                        ellipseEndCenter.X / xamlToCm, ellipseEndCenter.Y / xamlToCm);

                                    wordprocessingCanvas.Append(wordprocessingShapeInverter);
                                }
                            });
                        }
                    }
                }
            }

            // create wires, bottom ZOrder
            foreach (var action in wires)
            {
                action();
            }

            // create elements, top ZOrder
            foreach (var action in elements)
            {
                action();
            }
        }

        private static Wps.WordprocessingShape CreateWpShapePin(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm) - 30000L;
            Int64 Y = (Int64)(y * emu_1cm) - 30000L;

            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = X, Y = Y };
            A.Extents extents2 = new A.Extents() { Cx = 60000L, Cy = 60000L }; // Width,Height = 60000L, Margin Left/Top = -30000L;

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Ellipse };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStylePin();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        private static Wps.WordprocessingShape CreateWpShapeInverter(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm) - 48000L;
            Int64 Y = (Int64)(y * emu_1cm) - 48000L;

            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = X, Y = Y };
            A.Extents extents2 = new A.Extents() { Cx = 96000L, Cy = 96000L }; // Width,Height = 96000L, Margin Left/Top = -48000L;

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Ellipse };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.NoFill noFill1 = new A.NoFill();
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(noFill1);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStyleInverter();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        private static Wps.WordprocessingShape CreateWpShapeWire(double x1, double y1, double x2, double y2, bool start, bool end)
        {
            // x1, y1, x2, y2 are in Centimeters

            Int64 X = 0L;
            Int64 Y = 0L;
            Int64 Cx = 0L;
            Int64 Cy = 0L;

            if (x2 >= x1)
            {
                X = (Int64)(x1 * emu_1cm);
                Cx = (Int64)((x2 - x1) * emu_1cm);
            }
            else
            {
                X = (Int64)(x2 * emu_1cm);
                Cx = (Int64)((x1 - x2) * emu_1cm);
            }

            if (y2 >= y1)
            {
                Y = (Int64)(y1 * emu_1cm);
                Cy = (Int64)((y2 - y1) * emu_1cm);
            }
            else
            {
                Y = (Int64)(y2 * emu_1cm);
                Cy = (Int64)((y1 - y2) * emu_1cm);
            }

            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = X, Y = Y };
            A.Extents extents2 = new A.Extents() { Cx = Cx, Cy = Cy };

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Line };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            //if (start == true)
            //{
            //    A.HeadEnd headEnd1 = new A.HeadEnd() { Type = A.LineEndValues.Oval };
            //    outline1.Append(headEnd1);
            //}

            //if (end == true)
            //{
            //    A.TailEnd tailEnd1 = new A.TailEnd() { Type = A.LineEndValues.Oval };
            //    outline1.Append(tailEnd1);
            //}

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStyle2();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        private static Wps.WordprocessingShape CreateWpShapeAndGate(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L }; // 1cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "&";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeOrGate(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L }; // 1cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "≥1";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeInput(double x, double y, string text)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 6L * 360000L, Cy = 360000L }; // 6cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = text;

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeOutput(double x, double y, string text)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 6L * 360000L, Cy = 360000L }; // 6cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = text;

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        // NOT USED
        private static Wps.WordprocessingShape CreateWpShape1_AndGate()
        {
            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = "Prostokąt 1" };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = 0L, Y = 720090L };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L };

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "&";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        // NOT USED
        private static Wps.WordprocessingShape CreateWpShape2_Wire()
        {
            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = "Łącznik prosty 3" };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = 542778L, Y = 901798L };
            A.Extents extents2 = new A.Extents() { Cx = 1440000L, Cy = 0L };

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Line };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStyle2();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        // AndGate/OrGate/Input/Output Style
        private static Wps.ShapeStyle CreateShapeStyle1()
        {
            Wps.ShapeStyle shapeStyle1 = new Wps.ShapeStyle();

            A.LineReference lineReference1 = new A.LineReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor1 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference1.Append(schemeColor1);

            A.FillReference fillReference1 = new A.FillReference() { Index = (UInt32Value)1U };
            A.SchemeColor schemeColor2 = new A.SchemeColor() { Val = A.SchemeColorValues.Light1 };

            fillReference1.Append(schemeColor2);

            A.EffectReference effectReference1 = new A.EffectReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor3 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference1.Append(schemeColor3);

            A.FontReference fontReference1 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor4 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fontReference1.Append(schemeColor4);

            shapeStyle1.Append(lineReference1);
            shapeStyle1.Append(fillReference1);
            shapeStyle1.Append(effectReference1);
            shapeStyle1.Append(fontReference1);

            return shapeStyle1;
        }

        // Wire Style
        private static Wps.ShapeStyle CreateShapeStyle2()
        {
            Wps.ShapeStyle shapeStyle2 = new Wps.ShapeStyle();

            A.LineReference lineReference2 = new A.LineReference() { Index = (UInt32Value)3U };
            A.SchemeColor schemeColor5 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference2.Append(schemeColor5);

            A.FillReference fillReference2 = new A.FillReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor6 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fillReference2.Append(schemeColor6);

            A.EffectReference effectReference2 = new A.EffectReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor7 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference2.Append(schemeColor7);

            A.FontReference fontReference2 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor8 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };

            fontReference2.Append(schemeColor8);

            shapeStyle2.Append(lineReference2);
            shapeStyle2.Append(fillReference2);
            shapeStyle2.Append(effectReference2);
            shapeStyle2.Append(fontReference2);

            return shapeStyle2;
        }

        // Pin Style
        private static Wps.ShapeStyle CreateShapeStylePin()
        {
            Wps.ShapeStyle shapeStyle1 = new Wps.ShapeStyle();

            A.LineReference lineReference1 = new A.LineReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor1 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference1.Append(schemeColor1);

            A.FillReference fillReference1 = new A.FillReference() { Index = (UInt32Value)1U };
            A.SchemeColor schemeColor2 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fillReference1.Append(schemeColor2);

            A.EffectReference effectReference1 = new A.EffectReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor3 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference1.Append(schemeColor3);

            A.FontReference fontReference1 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor4 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fontReference1.Append(schemeColor4);

            shapeStyle1.Append(lineReference1);
            shapeStyle1.Append(fillReference1);
            shapeStyle1.Append(effectReference1);
            shapeStyle1.Append(fontReference1);

            return shapeStyle1;
        }

        // Inverter Style
        private static Wps.ShapeStyle CreateShapeStyleInverter()
        {
            Wps.ShapeStyle shapeStyle1 = new Wps.ShapeStyle();

            A.LineReference lineReference1 = new A.LineReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor1 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference1.Append(schemeColor1);

            A.FillReference fillReference1 = new A.FillReference() { Index = (UInt32Value)1U };
            A.SchemeColor schemeColor2 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fillReference1.Append(schemeColor2);

            A.EffectReference effectReference1 = new A.EffectReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor3 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference1.Append(schemeColor3);

            A.FontReference fontReference1 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor4 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fontReference1.Append(schemeColor4);

            shapeStyle1.Append(lineReference1);
            shapeStyle1.Append(fillReference1);
            shapeStyle1.Append(effectReference1);
            shapeStyle1.Append(fontReference1);

            return shapeStyle1;
        }

        // NOT USED
        private static V.Group CreateGroup()
        {
            V.Group group1 = new V.Group() { Id = "Kanwa 2", Style = "width:708.65pt;height:453.55pt;mso-position-horizontal-relative:char;mso-position-vertical-relative:line", CoordinateSize = "89998,57594", OptionalString = "_x0000_s1026", EditAs = V.EditAsValues.Canvas };

            group1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQC2gziS/gAAAOEBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRQU7DMBBF\n90jcwfIWJU67QAgl6YK0S0CoHGBkTxKLZGx5TGhvj5O2G0SRWNoz/78nu9wcxkFMGNg6quQqL6RA\n0s5Y6ir5vt9lD1JwBDIwOMJKHpHlpr69KfdHjyxSmriSfYz+USnWPY7AufNIadK6MEJMx9ApD/oD\nOlTrorhX2lFEilmcO2RdNtjC5xDF9pCuTyYBB5bi6bQ4syoJ3g9WQ0ymaiLzg5KdCXlKLjvcW893\nSUOqXwnz5DrgnHtJTxOsQfEKIT7DmDSUCaxw7Rqn8787ZsmRM9e2VmPeBN4uqYvTtW7jvijg9N/y\nJsXecLq0q+WD6m8AAAD//wMAUEsDBBQABgAIAAAAIQA4/SH/1gAAAJQBAAALAAAAX3JlbHMvLnJl\nbHOkkMFqwzAMhu+DvYPRfXGawxijTi+j0GvpHsDYimMaW0Yy2fr2M4PBMnrbUb/Q94l/f/hMi1qR\nJVI2sOt6UJgd+ZiDgffL8ekFlFSbvV0oo4EbChzGx4f9GRdb25HMsYhqlCwG5lrLq9biZkxWOiqY\n22YiTra2kYMu1l1tQD30/bPm3wwYN0x18gb45AdQl1tp5j/sFB2T0FQ7R0nTNEV3j6o9feQzro1i\nOWA14Fm+Q8a1a8+Bvu/d/dMb2JY5uiPbhG/ktn4cqGU/er3pcvwCAAD//wMAUEsDBBQABgAIAAAA\nIQB6MeNb8AIAAMIHAAAOAAAAZHJzL2Uyb0RvYy54bWy0VV1P2zAUfZ+0/2D5faQfjEJFiqoipkkI\nqsHEs+s4bYRjZ7ZpUt72wD9j/2vHTtIyKFRiWx/cm/h+33Nujk+qXJKlMDbTKqbdvQ4lQnGdZGoe\n0+/XZ58OKbGOqYRJrURMV8LSk9HHD8dlMRQ9vdAyEYbAibLDsojpwrliGEWWL0TO7J4uhMJlqk3O\nHB7NPEoMK+E9l1Gv0zmISm2SwmgurMXb0/qSjoL/NBXcXaapFY7ImCI3F04Tzpk/o9ExG84NKxYZ\nb9Jg78giZ5lC0LWrU+YYuTPZC1d5xo22OnV7XOeRTtOMi1ADqul2nlUzYWrJbCiGozttgpD+od/Z\nHD2Ay2GJYYggYxS2WA/F/l2wqwUrRKjBDvnFcmpIlgAplCiWAxBTtMPp28cHR7p+GmUR1K6KqWme\nLETf2io1uf9H00gVJriK6QAgOGrGKCpHOG76Bx38KOG4b2S4ijYeCmPdF6Fz4oWYGqAkDI8tz62r\nVVsV2PmM6hyC5FZS+DSk+iZS1IKAvWAdMCsm0pAlA9qS21APwgZNb5JmUq6NutuMpGuNGl1vJgKO\n14adbYabaGvtEFErtzbMM6XN28Zprd9WXdfqy3bVrGrmMdPJClM0uiaTLfhZhj6eM+umzIA9aD02\ngrvEkUpdxlQ3EiULbe63vff6gBluKSnBxpjaH3fMCErkVwUAeuq2gmmFWSuou3yi0XKACtkEEQbG\nyVZMjc5vsCjGPgqumOKIFVPuTPswcfVWwKrhYjwOaqBowdy5uvKEq+flcXFd3TBTNOBxQN2FbkHO\nhs8wVOv6USg9vnM6zQLAfEvrPjatBuFq7P935vVb5v36+fjA71V2i0rAwRXpP+HfRO3i3+f93mCA\n7Q6SHXW6g6PDepe2JOzu729YGAj6OgFlpvyGeNE8z1H/WioCFHV7A7Da03M3I/tvw3w7I3cQ63VG\n7uD/OxjpqvUeeI2RNXzadgT8oDHhO4EW/fElevoc2rf59I5+AwAA//8DAFBLAwQUAAYACAAAACEA\n4PXjX9oAAAAGAQAADwAAAGRycy9kb3ducmV2LnhtbEyPwU7DMBBE70j9B2uRuFE7UNES4lQVAgRH\nUuDsxkscYa+D7Tbh73G50MtKoxnNvK3Wk7PsgCH2niQUcwEMqfW6p07C2/bxcgUsJkVaWU8o4Qcj\nrOvZWaVK7Ud6xUOTOpZLKJZKgklpKDmPrUGn4twPSNn79MGplGXouA5qzOXO8ishbrhTPeUFowa8\nN9h+NXsngVA8NDbw59S+fwzme9U9vSxGKS/Op80dsIRT+g/DET+jQ52Zdn5POjIrIT+S/u7RWxTL\na2A7CbdiWQCvK36KX/8CAAD//wMAUEsBAi0AFAAGAAgAAAAhALaDOJL+AAAA4QEAABMAAAAAAAAA\nAAAAAAAAAAAAAFtDb250ZW50X1R5cGVzXS54bWxQSwECLQAUAAYACAAAACEAOP0h/9YAAACUAQAA\nCwAAAAAAAAAAAAAAAAAvAQAAX3JlbHMvLnJlbHNQSwECLQAUAAYACAAAACEAejHjW/ACAADCBwAA\nDgAAAAAAAAAAAAAAAAAuAgAAZHJzL2Uyb0RvYy54bWxQSwECLQAUAAYACAAAACEA4PXjX9oAAAAG\nAQAADwAAAAAAAAAAAAAAAABKBQAAZHJzL2Rvd25yZXYueG1sUEsFBgAAAAAEAAQA8wAAAFEGAAAA\nAA==\n"));

            V.Shapetype shapetype1 = new V.Shapetype() { Id = "_x0000_t75", CoordinateSize = "21600,21600", Filled = false, Stroked = false, OptionalNumber = 75, PreferRelative = true, EdgePath = "m@4@5l@4@11@9@11@9@5xe" };
            V.Stroke stroke1 = new V.Stroke() { JoinStyle = V.StrokeJoinStyleValues.Miter };

            V.Formulas formulas1 = CreateFormulas();

            V.Path path1 = new V.Path() { AllowGradientShape = true, ConnectionPointType = Ovml.ConnectValues.Rectangle, AllowExtrusion = false };
            Ovml.Lock lock1 = new Ovml.Lock() { Extension = V.ExtensionHandlingBehaviorValues.Edit, AspectRatio = true };

            shapetype1.Append(stroke1);
            shapetype1.Append(formulas1);
            shapetype1.Append(path1);
            shapetype1.Append(lock1);

            V.Shape shape1 = new V.Shape() { Id = "_x0000_s1027", Style = "position:absolute;width:89998;height:57594;visibility:visible;mso-wrap-style:square", Type = "#_x0000_t75" };
            V.Fill fill1 = new V.Fill() { DetectMouseClick = true };
            V.Path path2 = new V.Path() { ConnectionPointType = Ovml.ConnectValues.None };

            shape1.Append(fill1);
            shape1.Append(path2);

            V.Rectangle rectangle1 = new V.Rectangle() { Id = "Prostokąt 1", Style = "position:absolute;top:7200;width:3600;height:3600;visibility:visible;mso-wrap-style:square;v-text-anchor:middle", OptionalString = "_x0000_s1028", FillColor = "white [3201]", StrokeColor = "black [3200]", StrokeWeight = "1pt" };
            rectangle1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQDw94q7/QAAAOIBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRzUrEMBDH\n74LvEOYqbaoHEWm6B6tHFV0fYEimbdg2CZlYd9/edD8u4goeZ+b/8SOpV9tpFDNFtt4puC4rEOS0\nN9b1Cj7WT8UdCE7oDI7ekYIdMayay4t6vQvEIrsdKxhSCvdSsh5oQi59IJcvnY8TpjzGXgbUG+xJ\n3lTVrdTeJXKpSEsGNHVLHX6OSTxu8/pAEmlkEA8H4dKlAEMYrcaUSeXszI+W4thQZudew4MNfJUx\nQP7asFzOFxx9L/lpojUkXjGmZ5wyhjSRJQ8YKGvKv1MWzIkL33VWU9lGfl98J6hz4cZ/uUjzf7Pb\nbHuj+ZQu9z/UfAMAAP//AwBQSwMEFAAGAAgAAAAhADHdX2HSAAAAjwEAAAsAAABfcmVscy8ucmVs\nc6SQwWrDMAyG74O9g9G9cdpDGaNOb4VeSwe7CltJTGPLWCZt376mMFhGbzvqF/o+8e/2tzCpmbJ4\njgbWTQuKomXn42Dg63xYfYCSgtHhxJEM3Elg372/7U40YalHMvokqlKiGBhLSZ9aix0poDScKNZN\nzzlgqWMedEJ7wYH0pm23Ov9mQLdgqqMzkI9uA+p8T9X8hx28zSzcl8Zy0Nz33r6iasfXeKK5UjAP\nVAy4LM8w09zU50C/9q7/6ZURE31X/kL8TKv1x6wXNXYPAAAA//8DAFBLAwQUAAYACAAAACEAMy8F\nnkEAAAA5AAAAEAAAAGRycy9zaGFwZXhtbC54bWyysa/IzVEoSy0qzszPs1Uy1DNQUkjNS85PycxL\nt1UKDXHTtVBSKC5JzEtJzMnPS7VVqkwtVrK34+UCAAAA//8DAFBLAwQUAAYACAAAACEAYzhX6cEA\nAADaAAAADwAAAGRycy9kb3ducmV2LnhtbERPS2sCMRC+F/ofwhR6q1mFFlmNIoqoZYv4OHgcNuNm\ncTNZkqjrv2+EQk/Dx/ec8bSzjbiRD7VjBf1eBoK4dLrmSsHxsPwYgggRWWPjmBQ8KMB08voyxly7\nO+/oto+VSCEcclRgYmxzKUNpyGLouZY4cWfnLcYEfSW1x3sKt40cZNmXtFhzajDY0txQedlfrYK5\nKzark78sFsXpczssfmbme10p9f7WzUYgInXxX/znXus0H56vPK+c/AIAAP//AwBQSwECLQAUAAYA\nCAAAACEA8PeKu/0AAADiAQAAEwAAAAAAAAAAAAAAAAAAAAAAW0NvbnRlbnRfVHlwZXNdLnhtbFBL\nAQItABQABgAIAAAAIQAx3V9h0gAAAI8BAAALAAAAAAAAAAAAAAAAAC4BAABfcmVscy8ucmVsc1BL\nAQItABQABgAIAAAAIQAzLwWeQQAAADkAAAAQAAAAAAAAAAAAAAAAACkCAABkcnMvc2hhcGV4bWwu\neG1sUEsBAi0AFAAGAAgAAAAhAGM4V+nBAAAA2gAAAA8AAAAAAAAAAAAAAAAAmAIAAGRycy9kb3du\ncmV2LnhtbFBLBQYAAAAABAAEAPUAAACGAwAAAAA=\n"));

            V.TextBox textBox1 = new V.TextBox() { Inset = "0,0,0,0" };

            TextBoxContent textBoxContent2 = new TextBoxContent();

            Paragraph paragraph3 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties3 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines3 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification2 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties2 = new ParagraphMarkRunProperties();
            RunFonts runFonts3 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties2.Append(runFonts3);

            paragraphProperties3.Append(spacingBetweenLines3);
            paragraphProperties3.Append(justification2);
            paragraphProperties3.Append(paragraphMarkRunProperties2);

            Run run3 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties3 = new RunProperties();
            RunFonts runFonts4 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties3.Append(runFonts4);
            Text text2 = new Text();
            text2.Text = "&";

            run3.Append(runProperties3);
            run3.Append(text2);

            paragraph3.Append(paragraphProperties3);
            paragraph3.Append(run3);

            textBoxContent2.Append(paragraph3);

            textBox1.Append(textBoxContent2);

            rectangle1.Append(textBox1);

            V.Line line1 = new V.Line() { Id = "Łącznik prosty 3", Style = "position:absolute;visibility:visible;mso-wrap-style:square", OptionalString = "_x0000_s1029", StrokeColor = "black [3200]", StrokeWeight = "1pt", ConnectorType = Ovml.ConnectorValues.Straight, From = "5427,9017", To = "19827,9017" };
            line1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQD+JeulAAEAAOoBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRzU7EIBDH\n7ya+A+FqWqoHY0zpHqwe1Zj1AQhMW2I7EAbr7ts73e5ejGviEeb/8RuoN7tpFDMk8gG1vC4rKQBt\ncB57Ld+3T8WdFJQNOjMGBC33QHLTXF7U230EEuxG0nLIOd4rRXaAyVAZIiBPupAmk/mYehWN/TA9\nqJuqulU2YAbMRV4yZFO30JnPMYvHHV+vJAlGkuJhFS5dWpoYR29NZlI1o/vRUhwbSnYeNDT4SFeM\nIdWvDcvkfMHR98JPk7wD8WpSfjYTYyiXaNkAweaQWFf+nbSgTlSErvMWyjYRL7V6T3DnSlz4wgTz\nf/Nbtr3BfEpXh59qvgEAAP//AwBQSwMEFAAGAAgAAAAhAJYFM1jUAAAAlwEAAAsAAABfcmVscy8u\ncmVsc6SQPWsDMQyG90L/g9He8yVDKSW+bIWsIYWuxtZ9kLNkJHNN/n1MoaVXsnWUXvQ8L9rtL2k2\nC4pOTA42TQsGKXCcaHDwfnp7egGjxVP0MxM6uKLCvnt82B1x9qUe6ThlNZVC6mAsJb9aq2HE5LXh\njFSTniX5UkcZbPbh7Ae027Z9tvKbAd2KaQ7RgRziFszpmqv5DztNQVi5L03gZLnvp3CPaiN/0hGX\nSvEyYHEQRb+WgktTy4G979380xuYCENh+aiOlfwnqfbvBnb1zu4GAAD//wMAUEsDBBQABgAIAAAA\nIQAzLwWeQQAAADkAAAAUAAAAZHJzL2Nvbm5lY3RvcnhtbC54bWyysa/IzVEoSy0qzszPs1Uy1DNQ\nUkjNS85PycxLt1UKDXHTtVBSKC5JzEtJzMnPS7VVqkwtVrK34+UCAAAA//8DAFBLAwQUAAYACAAA\nACEAl7YFf8EAAADaAAAADwAAAGRycy9kb3ducmV2LnhtbESPQYvCMBSE7wv+h/AEb2vqCmVbjSKC\nsBdBXXfx+GyebbF5KUnU+u+NIHgcZuYbZjrvTCOu5HxtWcFomIAgLqyuuVSw/119foPwAVljY5kU\n3MnDfNb7mGKu7Y23dN2FUkQI+xwVVCG0uZS+qMigH9qWOHon6wyGKF0ptcNbhJtGfiVJKg3WHBcq\nbGlZUXHeXYyCP/o/uzTL5Op4uGxOZp+lWq6VGvS7xQREoC68w6/2j1YwhueVeAPk7AEAAP//AwBQ\nSwECLQAUAAYACAAAACEA/iXrpQABAADqAQAAEwAAAAAAAAAAAAAAAAAAAAAAW0NvbnRlbnRfVHlw\nZXNdLnhtbFBLAQItABQABgAIAAAAIQCWBTNY1AAAAJcBAAALAAAAAAAAAAAAAAAAADEBAABfcmVs\ncy8ucmVsc1BLAQItABQABgAIAAAAIQAzLwWeQQAAADkAAAAUAAAAAAAAAAAAAAAAAC4CAABkcnMv\nY29ubmVjdG9yeG1sLnhtbFBLAQItABQABgAIAAAAIQCXtgV/wQAAANoAAAAPAAAAAAAAAAAAAAAA\nAKECAABkcnMvZG93bnJldi54bWxQSwUGAAAAAAQABAD5AAAAjwMAAAAA\n"));
            V.Stroke stroke2 = new V.Stroke() { JoinStyle = V.StrokeJoinStyleValues.Miter };

            line1.Append(stroke2);
            Wvml.AnchorLock anchorLock1 = new Wvml.AnchorLock();

            group1.Append(shapetype1);
            group1.Append(shape1);
            group1.Append(rectangle1);
            group1.Append(line1);
            group1.Append(anchorLock1);

            return group1;
        }

        // NOT USED
        private static V.Formulas CreateFormulas()
        {
            V.Formulas formulas1 = new V.Formulas();

            V.Formula formula1 = new V.Formula() { Equation = "if lineDrawn pixelLineWidth 0" };
            V.Formula formula2 = new V.Formula() { Equation = "sum @0 1 0" };
            V.Formula formula3 = new V.Formula() { Equation = "sum 0 0 @1" };
            V.Formula formula4 = new V.Formula() { Equation = "prod @2 1 2" };
            V.Formula formula5 = new V.Formula() { Equation = "prod @3 21600 pixelWidth" };
            V.Formula formula6 = new V.Formula() { Equation = "prod @3 21600 pixelHeight" };
            V.Formula formula7 = new V.Formula() { Equation = "sum @0 0 1" };
            V.Formula formula8 = new V.Formula() { Equation = "prod @6 1 2" };
            V.Formula formula9 = new V.Formula() { Equation = "prod @7 21600 pixelWidth" };
            V.Formula formula10 = new V.Formula() { Equation = "sum @8 21600 0" };
            V.Formula formula11 = new V.Formula() { Equation = "prod @7 21600 pixelHeight" };
            V.Formula formula12 = new V.Formula() { Equation = "sum @10 21600 0" };

            formulas1.Append(formula1);
            formulas1.Append(formula2);
            formulas1.Append(formula3);
            formulas1.Append(formula4);
            formulas1.Append(formula5);
            formulas1.Append(formula6);
            formulas1.Append(formula7);
            formulas1.Append(formula8);
            formulas1.Append(formula9);
            formulas1.Append(formula10);
            formulas1.Append(formula11);
            formulas1.Append(formula12);

            return formulas1;
        }

        #endregion
    }

    #endregion
}
