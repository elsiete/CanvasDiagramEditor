// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Core;
using CanvasDiagram.Editor;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using CanvasDiagram.Core.Model; 

#endregion

namespace CanvasDiagram.WPF
{
    #region Lineweights

    public static class Lineweights
    {
        #region StrokeThicknes / Lineweights in Millimeters

        public static double LogicThicknessMm = 0.18;
        public static double WireThicknessMm = 0.18;
        public static double ElementThicknessMm = 0.35;
        public static double IOThicknessMm = 0.25;
        public static double PageThicknessMm = 0.13;

        #endregion
    } 

    #endregion

    #region WpfDiagramPrinter

    public class WpfDiagramPrinter
    {
        #region Properties

        public IDiagramCreator DiagramCreator { get; set; }
        public string ResourcesUri { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public bool ShortenStart { get; set; }
        public bool ShortenEnd { get; set; }

        #endregion

        #region Fixed Document

        private void SetPrintStrokeSthickness(ResourceDictionary resources)
        {
            if (resources == null)
                return;

            resources[ResourceConstants.KeyLogicStrokeThickness] = DipUtil.MmToDip(Lineweights.LogicThicknessMm);
            resources[ResourceConstants.KeyWireStrokeThickness] = DipUtil.MmToDip(Lineweights.WireThicknessMm);
            resources[ResourceConstants.KeyElementStrokeThickness] = DipUtil.MmToDip(Lineweights.ElementThicknessMm);
            resources[ResourceConstants.KeyIOStrokeThickness] = DipUtil.MmToDip(Lineweights.IOThicknessMm);
            resources[ResourceConstants.KeyPageStrokeThickness] = DipUtil.MmToDip(Lineweights.PageThicknessMm);
        }

        private void SetPrintColors(ResourceDictionary resources)
        {
            if (resources == null)
                return;

            var backgroundColor = resources[ResourceConstants.KeyLogicBackgroundColor] as SolidColorBrush;
            backgroundColor.Color = Colors.White;

            var gridColor = resources[ResourceConstants.KeLogicGridColory] as SolidColorBrush;
            gridColor.Color = Colors.Transparent;

            var pageColor = resources[ResourceConstants.KeyLogicTemplateColor] as SolidColorBrush;
            pageColor.Color = Colors.Black;

            var logicColor = resources[ResourceConstants.KeyLogicColor] as SolidColorBrush;
            logicColor.Color = Colors.Black;

            var logicSelectedColor = resources[ResourceConstants.KeyLogicSelectedColor] as SolidColorBrush;
            logicSelectedColor.Color = Colors.Black;

            var helperColor = resources[ResourceConstants.KeyLogicTransparent] as SolidColorBrush;
            helperColor.Color = Colors.Transparent;
        }

        private void SetElementResources(ResourceDictionary resources, bool fixedStrokeThickness)
        {
            resources.Source = new Uri(ResourcesUri, UriKind.Relative);

            if (fixedStrokeThickness == false)
                SetPrintStrokeSthickness(resources);

            SetPrintColors(resources);
        }

        private FrameworkElement CreateDiagramElement(string diagram,
            Size areaExtent,
            Point origin,
            Rect area,
            bool fixedStrokeThickness,
            ResourceDictionary resources,
            DiagramTable table)
        {
            var grid = new Grid()
            {
                ClipToBounds = true,
                Resources = resources
            };

            var template = new Control()
            {
                Template = grid.Resources[ResourceConstants.KeyLandscapePageTemplate] as ControlTemplate
            };

            var canvas = new DiagramCanvas()
            {
                Width = PageWidth,
                Height = PageHeight
            };

            ModelEditor.Parse(diagram,
                canvas, this.DiagramCreator,
                0, 0,
                false, false, false, true);

            grid.Children.Add(template);
            grid.Children.Add(canvas);

            LineEx.SetShortenStart(grid, ShortenStart);
            LineEx.SetShortenEnd(grid, ShortenEnd);

            TableGrid.SetData(grid, table);

            return grid;
        }

        public FixedDocument CreateFixedDocument(IEnumerable<string> diagrams,
            Size areaExtent,
            Size areaOrigin,
            bool fixedStrokeThickness,
            DiagramTable table)
        {
            var origin = new Point(areaOrigin.Width, areaOrigin.Height);
            var area = new Rect(origin, areaExtent);
            var scale = Math.Min(areaExtent.Width / PageWidth, areaExtent.Height / PageHeight);

            var fixedDocument = new FixedDocument() { Name = "diagrams" };
            //fixedDocument.DocumentPaginator.PageSize = new Size(areaExtent.Width, areaExtent.Height);

            SetElementResources(fixedDocument.Resources, fixedStrokeThickness);

            foreach (var diagram in diagrams)
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage();

                //pageContent.Child = fixedPage;
                ((IAddChild)pageContent).AddChild(fixedPage);

                fixedDocument.Pages.Add(pageContent);

                fixedPage.Width = areaExtent.Width;
                fixedPage.Height = areaExtent.Height;

                var element = CreateDiagramElement(diagram,
                    areaExtent,
                    origin,
                    area,
                    fixedStrokeThickness,
                    fixedDocument.Resources,
                    table);

                // transform and scale for print
                element.LayoutTransform = new ScaleTransform(scale, scale);

                // set element position
                FixedPage.SetLeft(element, areaOrigin.Width);
                FixedPage.SetTop(element, areaOrigin.Height);

                // add element to page
                fixedPage.Children.Add(element);

                // update fixed page layout
                //fixedPage.Measure(areaExtent);
                //fixedPage.Arrange(area);
            }

            return fixedDocument;
        }

        private FixedDocumentSequence CreateFixedDocumentSequence(IEnumerable<IEnumerable<string>> projects,
            Size areaExtent,
            Size areaOrigin,
            bool fixedStrokeThickness,
            DiagramTable table)
        {
            var fixedDocumentSeq = new FixedDocumentSequence() { Name = "diagrams" };

            foreach (var diagrams in projects)
            {
                var fixedDocument = CreateFixedDocument(diagrams,
                    areaExtent,
                    areaOrigin,
                    fixedStrokeThickness,
                    table);

                var documentRef = new DocumentReference();
                documentRef.BeginInit();
                documentRef.SetDocument(fixedDocument);
                documentRef.EndInit();

                (fixedDocumentSeq as IAddChild).AddChild(documentRef);
            }

            return fixedDocumentSeq;
        }

        #endregion

        #region Print

        private void SetPrintDialogOptions(PrintDialog dlg)
        {
            if (dlg == null)
                throw new ArgumentNullException();

            dlg.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();

            dlg.PrintTicket = dlg.PrintQueue.DefaultPrintTicket;
            dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
            dlg.PrintTicket.OutputQuality = OutputQuality.High;
            dlg.PrintTicket.TrueTypeFontMode = TrueTypeFontMode.DownloadAsNativeTrueTypeFont;
        }

        private bool ShowPrintDialog(PrintDialog dlg)
        {
            if (dlg == null)
                throw new ArgumentNullException();

            SetPrintDialogOptions(dlg);
            return (dlg.ShowDialog() == true) ? true : false;
        }

        public void Print(IEnumerable<string> diagrams, string name, DiagramTable table)
        {
            if (diagrams == null)
                throw new ArgumentNullException();

            var dlg = new PrintDialog();
            ShowPrintDialog(dlg);

            var caps = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            var areaExtent = new Size(caps.PageImageableArea.ExtentWidth, caps.PageImageableArea.ExtentHeight);
            var areaOrigin = new Size(caps.PageImageableArea.OriginWidth, caps.PageImageableArea.OriginHeight);

            var fixedDocument = CreateFixedDocument(diagrams,
                areaExtent,
                areaOrigin,
                false,
                table);

            dlg.PrintDocument(fixedDocument.DocumentPaginator, name);
        }

        public void PrintSequence(IEnumerable<IEnumerable<string>> projects, string name, DiagramTable table)
        {
            if (projects == null)
                throw new ArgumentNullException();

            var dlg = new PrintDialog();
            ShowPrintDialog(dlg);

            var caps = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            var areaExtent = new Size(caps.PageImageableArea.ExtentWidth, caps.PageImageableArea.ExtentHeight);
            var areaOrigin = new Size(caps.PageImageableArea.OriginWidth, caps.PageImageableArea.OriginHeight);

            var fixedDocumentSeq = CreateFixedDocumentSequence(projects,
                areaExtent,
                areaOrigin,
                false,
                table);

            dlg.PrintDocument(fixedDocumentSeq.DocumentPaginator, name);
        }

        #endregion
    } 
    
    #endregion
}
