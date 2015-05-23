// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Core.Model;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region WireEditor

    public static class WireEditor
    {
        #region Connect

        public static ILine Connect(ICanvas canvas, IElement root, ILine line, double x, double y, IDiagramCreator creator)
        {
            var rootTag = root.GetTag();
            if (rootTag == null)
                root.SetTag(new Connection(root, new List<Wire>()));

            var connection = root.GetTag() as Connection;
            var wires = connection.Wires;

            if (line == null)
                return FirstConnection(canvas, root, x, y, wires, creator);
            else
                return SecondConnection(root, line, x, y, wires);
        }

        private static ILine FirstConnection(ICanvas canvas, IElement root, double x, double y, List<Wire> wires, IDiagramCreator creator)
        {
            var counter = canvas.GetCounter();
            string rootUid = root.GetUid();

            bool startIsIO = StringUtil.StartsWith(rootUid, Constants.TagElementInput)
                || StringUtil.StartsWith(rootUid, Constants.TagElementOutput);

            var line = creator.CreateElement(Constants.TagElementWire,
                new object[] 
                {
                    x, y, x, y,
                    false, false,
                    startIsIO, false,
                    counter.Next()
                },
                0.0, 0.0, false) as ILine;

            wires.Add(new Wire(line, root, null));

            canvas.Add(line);

            if (line != null || !(line is ILine))
                line.SetTag(root);

            return line;
        }

        private static ILine SecondConnection(IElement root, ILine line, double x, double y, List<Wire> wires)
        {
            var margin = line.GetMargin();

            line.SetX2(x - margin.Left);
            line.SetY2(y - margin.Top);

            string rootUid = root.GetUid();

            bool endIsIO = StringUtil.StartsWith(rootUid, Constants.TagElementInput) ||
                StringUtil.StartsWith(rootUid, Constants.TagElementOutput);

            line.SetEndIO(endIsIO);

            var wire = new Wire(line, null, root);
            wires.Add(wire);

            var lineTag = line.GetTag();
            if (lineTag != null)
            {
                var start = lineTag as IElement;
                if (start != null)
                    line.SetTag(new Wire(line, start, root));
            }

            return null;
        }

        #endregion

        #region Reconnect

        public static void Reconnect(ICanvas canvas,
            ILine line, IElement splitPin,
            double x, double y,
            List<Connection> connections,
            ILine currentLine,
            IDiagramCreator creator)
        {
            var wire1 = connections[0].Wires.FirstOrDefault();
            var wire2 = connections[1].Wires.FirstOrDefault();
            var startRoot = (wire1.Start != null ? wire1.Start : wire2.Start) as IElement;
            var endRoot = (wire1.End != null ? wire1.End : wire2.End) as IElement;

            PointEx start;
            PointEx end;
            GetLocation(wire1, wire2, out start, out end);

            if (start != null && end != null)
            {
                var startLine = Connect(canvas, startRoot, currentLine, start.X, start.Y, creator);
                var splitLine = Connect(canvas, splitPin, startLine, x, y, creator);
                var endLine = Connect(canvas, splitPin, splitLine, x, y, creator);

                Connect(canvas, endRoot, endLine, start.X + end.X, start.Y + end.Y, creator);

                startLine.SetStartVisible(line.GetStartVisible());
                startLine.SetStartIO(line.GetStartIO());
                endLine.SetEndVisible(line.GetEndVisible());
                endLine.SetEndIO(line.GetEndIO());
            }
            else
            {
                throw new InvalidOperationException("LineEx must have Start and End points.");
            }
        }

        public static void GetLocation(Wire wire1, Wire wire2, out PointEx start, out PointEx end)
        {
            var line1 = wire1.Line as ILine;
            var start1 = wire1.Start;
            var end1 = wire1.End;
            var line2 = wire2.Line as ILine;
            var start2 = wire2.Start;
            var end2 = wire2.End;

            start = null;
            end = null;

            if (start1 != null)
            {
                var margin = line1.GetMargin();
                start = new PointEx(margin.Left, margin.Top);
            }

            if (end1 != null)
                end = new PointEx(line1.GetX2(), line1.GetY2());

            if (start2 != null)
            {
                var margin = line2.GetMargin();
                start = new PointEx(margin.Left,  margin.Top);
            }

            if (end2 != null)
                end = new PointEx(line2.GetX2(), line2.GetY2());
        }

        #endregion

        #region Split

        public static bool Split(ICanvas canvas, ILine line, ILine currentLine, IPoint point, IDiagramCreator creator, bool snap)
        {
            var pin = Insert.Pin(canvas, point, creator, snap);
            double x = pin.GetX();
            double y = pin.GetY();
            var temp = Connect(canvas, pin, currentLine, x, y, creator);

            canvas.Remove(line);

            var connections = ModelEditor.RemoveWireConnections(canvas, line);

            if (connections != null && connections.Count == 2)
                Reconnect(canvas, line, pin, x, y, connections, temp, creator);
            else
                throw new InvalidOperationException("LineEx should have only two connections: Start and End.");

            return true;
        }

        #endregion
    }

    #endregion
}
