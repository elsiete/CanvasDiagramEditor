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

    using Position = Tuple<double, double>;
    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;

    #endregion

    #region Editor

    public static class Editor
    {
        #region Selection

        public static void SelectionToggleWire(IElement element)
        {
            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("ToggleLineSelection: {0}, uid: {1}, parent: {2}",
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as ILine;

                // select/deselect line
                bool isSelected = line.GetSelected();
                line.SetSelected(isSelected ? false : true);
            }
        }

        public static void SetThumbsSelection(ICanvas canvas, bool isSelected)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                thumb.SetSelected(isSelected);
            }
        }

        public static void SetLinesSelection(ICanvas canvas, bool isSelected)
        {
            var lines = canvas.GetElements().OfType<ILine>();

            // deselect all lines
            foreach (var line in lines)
            {
                line.SetSelected(isSelected);
            }
        }

        public static void SelectAll(ICanvas canvas)
        {
            SetThumbsSelection(canvas, true);
            SetLinesSelection(canvas, true);
        }

        public static void SelectNone(ICanvas canvas)
        {
            SetThumbsSelection(canvas, false);
            SetLinesSelection(canvas, false);
        }

        #endregion

        #region Select Connected

        public static void SelectConnected(ICanvas canvas)
        {
            var elements = Elements.GetSelectedThumbs(canvas);

            if (elements != null)
            {
                var element = elements.FirstOrDefault();

                if (element != null)
                {
                    SelectNone(canvas);

                    var visited = new HashSet<string>();

                    SelectConnected(element, visited);

                    visited = null;
                }
            }
        }

        public static void SelectConnected(IElement element, HashSet<string> visited)
        {
            if (element == null)
            {
                return;
            }

            var elmentTag = element.GetTag();
            if (elmentTag != null)
            {
                visited.Add(element.GetUid());
                element.SetSelected(true);

                var selection = elmentTag as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                {
                    SelectConnected(tuple, element, visited);
                }
            }
        }

        public static void SelectConnected(MapWire tuple, IElement root, HashSet<string> visited)
        {
            var line = tuple.Item1 as ILine;
            var tag = line.GetTag() as Tuple<object, object>;

            line.SetSelected(true);

            if (tag == null)
            {
                return;
            }

            var startRoot = tag.Item1 as IElement;
            var endRoot = tag.Item2 as IElement;

            if (startRoot != null &&
                StringUtil.Compare(startRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(startRoot.GetUid()) == false)
            {
                SelectConnected(startRoot, visited);
            }

            if (endRoot != null &&
                StringUtil.Compare(endRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(endRoot.GetUid()) == false)
            {
                SelectConnected(endRoot, visited);
            }
        }

        #endregion

        #region IDs

        public static void IdsAppend(IEnumerable<object> elements, IdCounter counter)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements.Cast<IElement>())
            {
                string[] uid = element.GetUid().Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = IdsGetUpdatedElement(counter, type);

                //System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                string appendedUid = string.Concat(type, ModelConstants.TagNameSeparator, appendedId.ToString());
                element.SetUid(appendedUid);

                //if (element.Tag != null)
                //{
                //    var _tuples = element.Tag as List<TagMap>;
                //    foreach (var _tuple in _tuples)
                //    {
                //        System.Diagnostics.Debug.Print("  -{0}", _tuple.Item1.Uid);
                //    }
                //}
            }
        }

        public static int IdsGetUpdatedElement(IdCounter counter, string type)
        {
            int appendedId = -1;

            switch (type)
            {
                case ModelConstants.TagElementWire:
                    appendedId = counter.WireCount;
                    counter.WireCount += 1;
                    break;
                case ModelConstants.TagElementInput:
                    appendedId = counter.InputCount;
                    counter.InputCount += 1;
                    break;
                case ModelConstants.TagElementOutput:
                    appendedId = counter.OutputCount;
                    counter.OutputCount += 1;
                    break;
                case ModelConstants.TagElementAndGate:
                    appendedId = counter.AndGateCount;
                    counter.AndGateCount += 1;
                    break;
                case ModelConstants.TagElementOrGate:
                    appendedId = counter.OrGateCount;
                    counter.OrGateCount += 1;
                    break;
                case ModelConstants.TagElementPin:
                    appendedId = counter.PinCount;
                    counter.PinCount += 1;
                    break;
                default:
                    throw new Exception("Unknown element type.");
            }

            return appendedId;
        }

        public static void IdsUpdateCounter(IdCounter original, IdCounter counter)
        {
            original.PinCount = Math.Max(original.PinCount, counter.PinCount);
            original.WireCount = Math.Max(original.WireCount, counter.WireCount);
            original.InputCount = Math.Max(original.InputCount, counter.InputCount);
            original.OutputCount = Math.Max(original.OutputCount, counter.OutputCount);
            original.AndGateCount = Math.Max(original.AndGateCount, counter.AndGateCount);
            original.OrGateCount = Math.Max(original.OrGateCount, counter.OrGateCount);
        }

        #endregion

        #region Connections

        public static void ConnectionsUpdate(IDictionary<string, MapWires> dict)
        {
            // update wire to element connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1 as IElement;
                var wires = item.Value.Item2;

                if (element.GetTag() == null)
                {
                    element.SetTag(new Selection(false, new List<MapWire>()));
                }

                if (wires.Count > 0)
                {
                    var selection = element.GetTag() as Selection;
                    var tuples = selection.Item2;

                    foreach (var wire in wires)
                    {
                        string _name = wire.Item1;
                        string _type = wire.Item2;

                        if (StringUtil.Compare(_type, ModelConstants.WireStartType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, element, null);

                                tuples.Add(mapWire);

                                var lineEx = line as ILine;
                                if (lineEx.GetTag() != null)
                                {
                                    var endRoot = lineEx.GetTag() as IElement;
                                    if (endRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.SetTag(new Tuple<object, object>(element, endRoot));
                                    }
                                }
                                else
                                {
                                    // set line Tag as start root element
                                    lineEx.SetTag(element);
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire Start: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapTag(line, element, null);
                            //tuples.Add(_tuple);
                        }
                        else if (StringUtil.Compare(_type, ModelConstants.WireEndType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, null, element);

                                tuples.Add(mapWire);

                                var lineEx = line as ILine;
                                if (lineEx.GetTag() != null)
                                {
                                    var startRoot = lineEx.GetTag() as IElement;
                                    if (startRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.SetTag(new Tuple<object, object>(startRoot, element));
                                    }
                                }
                                else
                                {
                                    // set line Tag as end root element
                                    lineEx.SetTag(element);
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire End: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapWire(line, null, element);
                            //tuples.Add(_tuple);
                        }
                    }
                }
            }
        }

        public static Position GetPinPosition(IElement root, IThumb pin)
        {
            // get root position in canvas
            double rx = root.GetX();
            double ry = root.GetY();

            // get pin position in canvas (relative to root)
            double px = pin.GetX();
            double py = pin.GetY();

            // calculate real pin position
            double x = rx + px;
            double y = ry + py;

            return new Position(x, y);
        }

        #endregion

        #region Delete

        public static void DeleteElement(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        public static void DeleteElement(ICanvas canvas, IElement element)
        {
            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as ILine;

                DeleteWire(canvas, line);
            }
            else
            {
                canvas.Remove(element);
            }
        }

        public static void DeleteWire(ICanvas canvas, ILine line)
        {
            canvas.Remove(line);

            RemoveWireConnections(canvas, line);

            DeleteEmptyPins(canvas);
        }

        public static void DeleteEmptyPins(ICanvas canvas)
        {
            var pins = FindEmptyPins(canvas);

            // remove empty pins
            foreach (var pin in pins)
            {
                canvas.Remove(pin);
            }
        }

        public static List<IElement> FindEmptyPins(ICanvas canvas)
        {
            var pins = new List<IElement>();

            foreach (var element in canvas.GetElements())
            {
                string uid = element.GetUid();

                if (IsElementPin(uid))
                {
                    var elementTag = element.GetTag();
                    if (elementTag != null)
                    {
                        var selection = elementTag as Selection;
                        var tuples = selection.Item2;

                        if (tuples.Count <= 0)
                        {
                            // empty pin
                            pins.Add(element);
                        }
                    }
                    else
                    {
                        // empty pin
                        pins.Add(element);
                    }
                }
            }

            return pins;
        }

        public static bool IsElementPin(string uid)
        {
            return uid != null &&
                   StringUtil.StartsWith(uid, ModelConstants.TagElementPin);
        }

        public static Connections RemoveWireConnections(ICanvas canvas, ILine line)
        {
            var connections = new Connections();

            foreach (var element in canvas.GetElements())
            {
                var elementTag = element.GetTag();
                if (elementTag  != null && !(element is ILine))
                {
                    RemoveWireConnections(line, connections, element);
                }
            }

            return connections;
        }

        public static void RemoveWireConnections(ILine line, Connections connections, IElement element)
        {
            var selection = element.GetTag() as Selection;
            var tuples = selection.Item2;
            var map = new List<MapWire>();

            CreateMapWire(line, tuples, map);

            if (map.Count > 0)
            {
                connections.Add(new Connection(element, map));
            }

            foreach (var tuple in map)
            {
                tuples.Remove(tuple);
            }
        }

        private static void CreateMapWire(ILine line, List<MapWire> tuples, List<MapWire> map)
        {
            foreach (var tuple in tuples)
            {
                var _line = tuple.Item1 as ILine;

                if (StringUtil.Compare(_line.GetUid(), line.GetUid()))
                {
                    map.Add(tuple);
                }
            }
        }

        #endregion
    }

    #endregion
}
