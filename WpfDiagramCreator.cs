// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes; 
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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

    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    
    #endregion

    #region WpfDiagramCreator

    public class WpfDiagramCreator : IDiagramCreator
    {
        #region Properties

        public Action<ElementThumb> SetThumbEvents { get; set; }
        public Action<IElement, double, double, bool> SetPosition { get; set; }

        public Func<List<object>> GetTags { get; set; }
        public Func<IdCounter> GetCounter { get; set; }

        private ICanvas ParserCanvas { get; set; }
        public Path ParserPath { get; set; }

        #endregion

        #region Grid Geometry

        private string CreateGridGeometry(double originX, 
            double originY, 
            double width, 
            double height, 
            double size)
        {
            var sb = new StringBuilder();

            double sizeX = size;
            double sizeY = size;

            // horizontal lines
            for (double y = sizeY + originY /* originY + size */; y < height + originY; y += size)
            {
                sb.AppendFormat("M{0},{1}", originX, y);
                sb.AppendFormat("L{0},{1}", width + originX, y);
            }

            // vertical lines
            for (double x = sizeX + originX /* originX + size */; x < width + originX; x += size)
            {
                sb.AppendFormat("M{0},{1}", x, originY);
                sb.AppendFormat("L{0},{1}", x, height + originY);
            }

            return sb.ToString();
        }

        #endregion

        #region IDiagramCreator

        public void SetCanvas(ICanvas canvas)
        {
            this.ParserCanvas = canvas;
        }

        public ICanvas GetCanvas()
        {
            return this.ParserCanvas;
        }

        public object CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementPin + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateWire(double x1, double y1, double x2, double y2,
            bool startVisible, bool endVisible,
            bool startIsIO, bool endIsIO,
            int id)
        {
            var line = new LineEx()
            {
                Style = Application.Current.Resources[ResourceConstants.KeyStyleWireLine] as Style,
                X1 = 0, //X1 = x1,
                Y1 = 0, //Y1 = y1,
                Margin = new Thickness(x1, y1, 0, 0),
                X2 = x2 - x1, // X2 = x2,
                Y2 = y2 - y1, // Y2 = y2,
                IsStartVisible = startVisible,
                IsEndVisible = endVisible,
                IsStartIO = startIsIO,
                IsEndIO = endIsIO,
                Uid = ModelConstants.TagElementWire + ModelConstants.TagNameSeparator + id.ToString()
            };

            return line;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementInput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            // set element Tag
            var tags = this.GetTags();
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();

                if (tag != null)
                {
                    ElementThumb.SetData(thumb, tag);
                }
            }

            return thumb;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOutput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            // set element Tag
            var tags = this.GetTags();
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();

                if (tag != null)
                {
                    ElementThumb.SetData(thumb, tag);
                }
            }

            return thumb;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementAndGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOrGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            if (ParserPath != null)
            {
                CreateGrid(properties.GridOriginX, properties.GridOriginY,
                    properties.GridWidth, properties.GridHeight,
                    properties.GridSize);
            }

            if (ParserCanvas != null)
            {
                ParserCanvas.SetWidth(properties.PageWidth);
                ParserCanvas.SetHeight(properties.PageHeight);
            }

            return null;
        }

        public object CreateGrid(double originX,
            double originY,
            double width,
            double height,
            double size)
        {
            var path = this.ParserPath;
            if (path == null)
                return null;

            string str = CreateGridGeometry(originX, originY, width, height, size);

            path.Data = Geometry.Parse(str);

            return null;
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {
            var canvas = ParserCanvas;

            Editor.Elements.Insert(canvas, elements.Cast<IElement>(), select);
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            Editor.Editor.IdsUpdateCounter(original, counter);
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            Editor.Editor.ConnectionsUpdate(dict);
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            Editor.Editor.IdsAppend(elements, this.GetCounter());
        }

        #endregion
    }

    #endregion
}
