// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;

    #endregion

    #region Elements

    public static class Elements
    {
        #region Elements

        public static IEnumerable<IElement> GetSelected(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get selected thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                if (thumb.GetSelected() == true)
                {
                    elements.Add(thumb);
                }
            }

            // get selected wires
            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                if (wire.GetSelected() == true)
                {
                    elements.Add(wire);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetSelectedThumbs(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get selected thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                if (thumb.GetSelected() == true)
                {
                    elements.Add(thumb);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetSelectedWires(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get selected wires
            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                if (wire.GetSelected() == true)
                {
                    elements.Add(wire);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetAll(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get all thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                elements.Add(thumb);
            }

            // get all wires
            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                elements.Add(wire);
            }

            return elements;
        }

        public static IEnumerable<IElement> GetThumbs(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get all thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                elements.Add(thumb);
            }

            return elements;
        }

        public static IEnumerable<IElement> GetWires(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get all wires
            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                elements.Add(wire);
            }

            return elements;
        }

        public static void Insert(ICanvas canvas, IEnumerable<IElement> elements, bool select)
        {
            foreach (var element in elements)
            {
                canvas.Add(element);

                if (select == true)
                {
                    element.SetSelected(true);
                }
            }
        }

        #endregion
    }

    #endregion
}
