
namespace CanvasDiagramEditor.Export
{
    #region References

    using CanvasDiagramEditor;
    using System;
    using System.Collections.Generic;
    using Office = Microsoft.Office.Core;
    using Word = Microsoft.Office.Interop.Word;

    #endregion

    #region MsoWordExport

    public class MsoWordExport : IDiagramExport
    {
        #region Microsoft Word 2013 Export

        private Office.MsoShapeStyleIndex defaultShapeStyle = Office.MsoShapeStyleIndex.msoShapeStylePreset25;
        private Office.MsoShapeStyleIndex defaultLineStyle = Office.MsoShapeStyleIndex.msoLineStylePreset20;

        public void CreateDocument(string fileName, IEnumerable<string> diagrams)
        {
            System.Diagnostics.Debug.Print("Creating document: {0}", fileName);

            var word = new Word.Application();

            var doc = CreateDocument(word, diagrams);

            // save and close document
            doc.SaveAs2(fileName);

            (doc as Word._Document).Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            (word as Word._Application).Quit(Word.WdSaveOptions.wdDoNotSaveChanges);

            System.Diagnostics.Debug.Print("Done.");
        }

        private Word.Document CreateDocument(Word.Application word, IEnumerable<string> diagrams)
        {
            // create new document
            var doc = word.Documents.Add();

            doc.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
            doc.PageSetup.Orientation = Word.WdOrientation.wdOrientLandscape;

            // margin = 20.0f;
            // 801.95 + 40, 555.35 + 40
            // 841.95, 595.35
            // 780, 540
            // left,right: 30.975f top,bottom: 27.675f

            doc.PageSetup.LeftMargin = 30.975f;
            doc.PageSetup.RightMargin = 30.975f;
            doc.PageSetup.TopMargin = 27.675f;
            doc.PageSetup.BottomMargin = 27.675f;

            foreach (var diagram in diagrams)
            {
                // create diagram canvas
                var canvas = CreateCanvas(doc);
                var items = canvas.CanvasItems;

                CreateElements(items, diagram);
            }

            return doc;
        }

        private static Word.Shape CreateCanvas(Word.Document doc)
        {
            float left = doc.PageSetup.LeftMargin;
            float top = doc.PageSetup.TopMargin;
            float width = doc.PageSetup.PageWidth - doc.PageSetup.LeftMargin - doc.PageSetup.RightMargin;
            float height = doc.PageSetup.PageHeight - doc.PageSetup.TopMargin - doc.PageSetup.BottomMargin;

            System.Diagnostics.Debug.Print("document width, height: {0},{1}", width, height);

            var canvas = doc.Shapes.AddCanvas(left, top, width, height);

            canvas.WrapFormat.AllowOverlap = (int)Office.MsoTriState.msoFalse;
            canvas.WrapFormat.Type = Word.WdWrapType.wdWrapInline;

            return canvas;
        }

        private void CreateElements(Word.CanvasShapes items, string diagram)
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

                    if (StringUtil.Compare(args[0], ModelConstants.PrefixRootElement))
                    {
                        if (StringUtil.StartsWith(name, ModelConstants.TagElementPin) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreatePin(items, x, y);
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
                                CreateInput(items, x, y, "Input");
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
                                CreateOutput(items, x, y, "Output");
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
                                CreateAndGate(items, x, y);
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
                                CreateOrGate(items, x, y);
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

                            bool start = false;
                            bool end = false;

                            if (length == 8)
                            {
                                start = bool.Parse(args[6]);
                                end = bool.Parse(args[7]);
                            }

                            wires.Add(() =>
                            {
                                CreateWire(items, x1, y1, x2, y2, start, end);
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

        private void CreatePin(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeOval,
                x - 4.0f, y - 4.0f, 8.0f, 8.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateWire(Word.CanvasShapes items, float x1, float y1, float x2, float y2, bool start, bool end)
        {
            var line = items.AddLine(x1, y1, x2, y2);

            line.Line.ForeColor.RGB = unchecked((int)0x00000000);
            line.Line.Weight = 1.0f;

            line.ShapeStyle = defaultLineStyle;

            if (start == true)
            {
                line.Line.BeginArrowheadStyle = Office.MsoArrowheadStyle.msoArrowheadOval;
                line.Line.BeginArrowheadWidth = Office.MsoArrowheadWidth.msoArrowheadWidthMedium;
                line.Line.BeginArrowheadLength = Office.MsoArrowheadLength.msoArrowheadLengthMedium;
            }

            if (end == true)
            {
                line.Line.EndArrowheadStyle = Office.MsoArrowheadStyle.msoArrowheadOval;
                line.Line.EndArrowheadWidth = Office.MsoArrowheadWidth.msoArrowheadWidthMedium;
                line.Line.EndArrowheadLength = Office.MsoArrowheadLength.msoArrowheadLengthMedium;
            }
        }

        private void CreateInput(Word.CanvasShapes items, float x, float y, string text)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 285.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateOutput(Word.CanvasShapes items, float x, float y, string text)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 285.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateAndGate(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 30.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = "&";

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateOrGate(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 30.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = "≥1";

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateText(Word.CanvasShapes items, float x, float y, float width, float height, string text)
        {
            var textBox = items.AddTextbox(Office.MsoTextOrientation.msoTextOrientationHorizontal,
                x, y, width, height);

            textBox.Line.Visible = Office.MsoTriState.msoFalse;
            textBox.Fill.Visible = Office.MsoTriState.msoFalse;

            var textFrame = textBox.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;
        }

        private void SetTextFrameFormat(Word.TextFrame textFrame)
        {
            textFrame.AutoSize = (int)Office.MsoAutoSize.msoAutoSizeNone;

            textFrame.VerticalAnchor = Office.MsoVerticalAnchor.msoAnchorMiddle;

            textFrame.MarginLeft = 0.0f;
            textFrame.MarginTop = 0.0f;
            textFrame.MarginRight = 0.0f;
            textFrame.MarginBottom = 0.0f;

            textFrame.TextRange.Font.Name = "Arial";
            textFrame.TextRange.Font.Size = 12.0f;
            textFrame.TextRange.Font.TextColor.RGB = unchecked((int)0x00000000);

            textFrame.TextRange.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            textFrame.TextRange.Paragraphs.SpaceAfter = 0.0f;
            textFrame.TextRange.Paragraphs.SpaceBefore = 0.0f;
            textFrame.TextRange.Paragraphs.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
        }

        #endregion
    }

    #endregion
}
