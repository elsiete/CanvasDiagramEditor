// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region Aliases

    //using MapPin = Tuple<string, string>;
    //using MapWire = Tuple<object, object, object>;
    //using MapWires = Tuple<object, List<Tuple<string, string>>>;
    //using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    //using UndoRedo = Tuple<Stack<string>, Stack<string>>;
    //using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    //using TreeDiagram = Stack<string>;
    //using TreeDiagrams = Stack<Stack<string>>;
    //using TreeProject = Tuple<string, Stack<Stack<string>>>;
    //using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    //using TreeSolution = Tuple<string, string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    //using Position = Tuple<double, double>;
    //using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    //using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    //using Solution = Tuple<string, IEnumerable<string>>;

    #endregion

    #region Wire

    public static class Wire
    {
        #region Connect

        public static ILine Connect(ICanvas canvas, IElement root, ILine line, double x, double y, IDiagramCreator creator)
        {
            var rootTag = root.GetTag();
            if (rootTag == null)
                root.SetTag(new Selection(false, new List<MapWire>()));

            var selection = root.GetTag() as Selection;
            var tuples = selection.Item2;

            if (line == null)
                return FirstConnection(canvas, root, x, y, tuples, creator);
            else
                return SecondConnection(root, line, x, y, tuples);
        }

        private static ILine FirstConnection(ICanvas canvas, IElement root, double x, double y, List<MapWire> tuples, IDiagramCreator creator)
        {
            var counter = canvas.GetCounter();
            string rootUid = root.GetUid();

            bool startIsIO = StringUtil.StartsWith(rootUid, Constants.TagElementInput)
                || StringUtil.StartsWith(rootUid, Constants.TagElementOutput);

            var line = creator.CreateElement(Constants.TagElementWire,
                new object[] 
                {
                    x, y,
                    x, y,
                    false, false,
                    startIsIO, false,
                    counter.Next()
                },
                0.0, 0.0, false) as ILine;

            // update connections
            var tuple = new MapWire(line, root, null);
            tuples.Add(tuple);

            canvas.Add(line);

            // line Tag is start root element
            if (line != null || !(line is ILine))
                line.SetTag(root);

            return line;
        }

        private static ILine SecondConnection(IElement root, ILine line, double x, double y, List<MapWire> tuples)
        {
            var margin = line.GetMargin();

            line.SetX2(x - margin.Left);
            line.SetY2(y - margin.Top);

            // update IsEndIO flag
            string rootUid = root.GetUid();

            bool endIsIO = StringUtil.StartsWith(rootUid, Constants.TagElementInput) ||
                StringUtil.StartsWith(rootUid, Constants.TagElementOutput);

            line.SetEndIO(endIsIO);

            // update connections
            var tuple = new MapWire(line, null, root);
            tuples.Add(tuple);

            // line Tag is start root element
            var lineTag = line.GetTag();
            if (lineTag != null)
            {
                // line Tag is start root element
                var start = lineTag as IElement;
                if (start != null)
                {
                    // line Tag is Tuple of start & end root element
                    // this Tag is used to find all connected elements
                    line.SetTag(new Tuple<object, object>(start, root));
                }
            }

            return null;
        }

        #endregion

        #region Reconnect

        public static void Reconnect(ICanvas canvas,
            ILine line, IElement splitPin,
            double x, double y,
            Connections connections,
            ILine currentLine,
            IDiagramCreator creator)
        {
            var c1 = connections[0];
            var c2 = connections[1];
            var map1 = c1.Item2.FirstOrDefault();
            var map2 = c2.Item2.FirstOrDefault();
            var startRoot = (map1.Item2 != null ? map1.Item2 : map2.Item2) as IElement;
            var endRoot = (map1.Item3 != null ? map1.Item3 : map2.Item3) as IElement;
            var location = GetLocation(map1, map2);

            if (location.Item1 != null && location.Item2 != null)
            {
                PointEx start = location.Item1;
                PointEx end = location.Item2;
                double x1 = start.X;
                double y1 = start.Y;
                double x2 = x1 + end.X;
                double y2 = y1 + end.Y;
                bool isStartVisible = line.GetStartVisible();
                bool isEndVisible = line.GetEndVisible();
                bool isStartIO = line.GetStartIO();
                bool isEndIO = line.GetEndIO();

                var startLine = Connect(canvas, startRoot, currentLine, x1, y1, creator);
                var splitLine = Connect(canvas, splitPin, startLine, x, y, creator);
                var endLine = Connect(canvas, splitPin, splitLine, x, y, creator);

                Connect(canvas, endRoot, endLine, x2, y2, creator);

                startLine.SetStartVisible(isStartVisible);
                startLine.SetStartIO(isStartIO);
                endLine.SetEndVisible(isEndVisible);
                endLine.SetEndIO(isEndIO);
            }
            else
            {
                throw new InvalidOperationException(
                    "LineEx should have corrent location info for Start and End.");
            }
        }

        public static Tuple<PointEx, PointEx> GetLocation(MapWire map1, MapWire map2)
        {
            var line1 = map1.Item1 as ILine;
            var start1 = map1.Item2;
            var end1 = map1.Item3;
            var line2 = map2.Item1 as ILine;
            var start2 = map2.Item2;
            var end2 = map2.Item3;
            PointEx startPoint = null;
            PointEx endPoint = null;

            if (start1 != null)
            {
                var margin = line1.GetMargin();
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new PointEx(left, top);
            }

            if (end1 != null)
            {
                double left = line1.GetX2();
                double top = line1.GetY2();

                endPoint = new PointEx(left, top);
            }

            if (start2 != null)
            {
                var margin = line2.GetMargin();
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new PointEx(left, top);
            }

            if (end2 != null)
            {
                double left = line2.GetX2();
                double top = line2.GetY2();

                endPoint = new PointEx(left, top);
            }

            return new Tuple<PointEx, PointEx>(startPoint, endPoint);
        }

        #endregion

        #region Split

        public static bool Split(ICanvas canvas, ILine line, ILine currentLine, IPoint point, IDiagramCreator creator, bool snap)
        {
            // create split pin
            var splitPin = Insert.Pin(canvas, point, creator, snap);

            // connect current line to split pin
            double x = splitPin.GetX();
            double y = splitPin.GetY();

            var _currentLine = Connect(canvas, splitPin, currentLine, x, y, creator);

            // remove original hit tested line
            canvas.Remove(line);

            // remove wire connections
            var connections = Model.RemoveWireConnections(canvas, line);

            // connected original root element to split pin
            if (connections != null && connections.Count == 2)
                Reconnect(canvas, line, splitPin, x, y, connections, _currentLine, creator);
            else
                throw new InvalidOperationException("LineEx should have only two connections: Start and End.");

            return true;
        }

        #endregion
    }

    #endregion
}
