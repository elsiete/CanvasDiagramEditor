// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Core.Model;
using CanvasDiagram.Editor;
using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Util;
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

namespace CanvasDiagram.WPF
{
    using FactoryFunc = Func<object[], double, double, bool, object>;

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

        #region Constructor

        public WpfDiagramCreator()
        {
            InitializeFactory();
        }

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
            for (double y = sizeY + originY; y < height + originY; y += size)
            {
                sb.Append('M');
                sb.Append(originX);
                sb.Append(',');
                sb.Append(y);

                sb.Append('L');
                sb.Append(width + originX);
                sb.Append(',');
                sb.Append(y);
            }

            // vertical lines
            for (double x = sizeX + originX; x < width + originX; x += size)
            {
                sb.Append('M');
                sb.Append(x);
                sb.Append(',');
                sb.Append(originY);

                sb.Append('L');
                sb.Append(x);
                sb.Append(',');
                sb.Append(height + originY);
            }

            return sb.ToString();
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

            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = Constants.TagElementPin + Constants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
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
                Uid = Constants.TagElementWire + Constants.TagNameSeparator + id.ToString()
            };

            return line;
        }

        private object CreateInput(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 2)
                return null;

            int id = (int)data[0];
            int tagId = (int)data[1];

            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = Constants.TagElementInput + Constants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            var tags = this.GetTags();
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();
                if (tag != null)
                    ElementThumb.SetData(thumb, tag);
            }

            return thumb;
        }

        private object CreateOutput(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 2)
                return null;

            int id = (int)data[0];
            int tagId = (int)data[1];

            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = Constants.TagElementOutput + Constants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            var tags = this.GetTags();
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();
                if (tag != null)
                    ElementThumb.SetData(thumb, tag);
            }

            return thumb;
        }

        private object CreateAndGate(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 1)
                return null;

            int id = (int)data[0];

            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = Constants.TagElementAndGate + Constants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
        }

        private object CreateOrGate(object[] data, double x, double y, bool snap)
        {
            if (data == null || data.Length != 1)
                return null;

            int id = (int)data[0];

            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = Constants.TagElementOrGate + Constants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetPosition(thumb, x, y, snap);

            return thumb;
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

        public void InsertElements(IEnumerable<object> elements, 
            bool select,
            double offsetX,
            double offsetY)
        {
            var canvas = ParserCanvas;

            ModelEditor.Insert(canvas, 
                elements.Cast<IElement>(), 
                select,
                offsetX,
                offsetY);
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            ModelEditor.IdsUpdateCounter(original, counter);
        }

        public void UpdateConnections(IDictionary<string, Child> dict)
        {
            ModelEditor.ConnectionsUpdate(dict);
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            ModelEditor.IdsAppend(elements, this.GetCounter());
        }

        #endregion
    }

    #endregion
}
