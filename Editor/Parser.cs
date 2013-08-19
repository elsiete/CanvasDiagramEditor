// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Util;
using CanvasDiagramEditor.Core;
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
    using UndoRedo = Tuple<Stack<string>, Stack<string>>;
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>;
    using TreeProject = Tuple<string, Stack<Stack<string>>>;
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    using TreeSolution = Tuple<string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    using Position = Tuple<double, double>;
    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    using Solution = Tuple<string, IEnumerable<string>>; 

    #endregion

    #region Parser

    public class Parser : IDiagramParser
    {
        #region Parse

        public TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options)
        {
            if (model == null || creator == null || options == null)
                return null;

            double offsetX = options.OffsetX;
            double offsetY = options.OffsetY;
            bool appendIds = options.AppendIds;
            bool updateIds = options.UpdateIds;
            bool select = options.Select;
            bool createElements = options.CreateElements;

            string name = null;
            string root = null;
            var counter = new IdCounter();
            var total = new IdCounter();
            var elements = new List<object>();
            MapWires tuple = null;
            var dict = new Dictionary<string, MapWires>();

            TreeSolution solution = null;
            TreeProjects projects = null;
            TreeProject project = null;
            TreeDiagrams diagrams = null;
            TreeDiagram diagram = null;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var lines = model.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var args = line.Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                int length = args.Length;
                if (length < 2)
                    continue;

                root = args[0];
                name = args[1];

                // root element
                if (StringUtil.Compare(root, ModelConstants.PrefixRoot))
                {
                    // Solution
                    if (StringUtil.StartsWith(name, ModelConstants.TagHeaderSolution) &&
                        (length == 2 || length == 3))
                    {
                        int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                        counter.SolutionCount = Math.Max(counter.SolutionCount, id + 1);

                        total.SolutionCount++;

                        string tagFileName = (length == 3) ? args[2] : null;

                        projects = new TreeProjects();
                        solution = new TreeSolution(name, tagFileName, projects);
                    }

                    // Project
                    else if (StringUtil.StartsWith(name, ModelConstants.TagHeaderProject) &&
                        length == 2)
                    {
                        int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                        counter.ProjectCount = Math.Max(counter.ProjectCount, id + 1);

                        total.ProjectCount++;

                        if (projects != null)
                        {
                            diagrams = new TreeDiagrams();
                            project = new TreeProject(name, diagrams);
                            projects.Push(project);
                        }
                    }

                    // Diagram
                    else if (StringUtil.StartsWith(name, ModelConstants.TagHeaderDiagram) &&
                        length == 13)
                    {
                        int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                        counter.DiagramCount = Math.Max(counter.DiagramCount, id + 1);

                        total.DiagramCount++;

                        if (diagrams != null)
                        {
                            diagram = new TreeDiagram();
                            diagrams.Push(diagram);
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            var prop = new DiagramProperties();

                            prop.PageWidth = int.Parse(args[2]);
                            prop.PageHeight = int.Parse(args[3]);
                            prop.GridOriginX = int.Parse(args[4]);
                            prop.GridOriginY = int.Parse(args[5]);
                            prop.GridWidth = int.Parse(args[6]);
                            prop.GridHeight = int.Parse(args[7]);
                            prop.GridSize = int.Parse(args[8]);
                            prop.SnapX = double.Parse(args[9]);
                            prop.SnapY = double.Parse(args[10]);
                            prop.SnapOffsetX = double.Parse(args[11]);
                            prop.SnapOffsetY = double.Parse(args[12]);

                            creator.CreateDiagram(prop);

                            options.Properties = prop;
                        }
                    }

                    // Pin
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementPin) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.PinCount++;

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.PinCount = Math.Max(counter.PinCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementPin,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Input
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementInput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.InputCount++;

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                            int tagId = (length == 5) ? int.Parse(args[4]) : -1;

                            counter.InputCount = Math.Max(counter.InputCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementInput,
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Output
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOutput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.OutputCount++;

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                            int tagId = (length == 5) ? int.Parse(args[4]) : -1;

                            counter.OutputCount = Math.Max(counter.OutputCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementOutput,
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // AndGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementAndGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.AndGateCount++;

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.AndGateCount = Math.Max(counter.AndGateCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementAndGate,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // OrGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOrGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.OrGateCount++;

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.OrGateCount = Math.Max(counter.OrGateCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementOrGate,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Wire
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        (length == 6 || length == 8 || length == 10))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.WireCount++;

                        if (createElements == true)
                        {
                            double x1 = double.Parse(args[2]);
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);
                            bool startVisible = (length == 8 || length == 10) ? bool.Parse(args[6]) : false;
                            bool endVisible = (length == 8 || length == 10) ? bool.Parse(args[7]) : false;
                            bool startIsIO = (length == 10) ? bool.Parse(args[8]) : false;
                            bool endIsIO = (length == 10) ? bool.Parse(args[9]) : false;
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.WireCount = Math.Max(counter.WireCount, id + 1);

                            var element = creator.CreateElement(ModelConstants.TagElementWire,
                                new object[] 
                                {
                                    x1 + offsetX, y1 + offsetY,
                                    x2 + offsetX, y2 + offsetY,
                                    startVisible, endVisible,
                                    startIsIO, endIsIO,
                                    id
                                },
                                0.0, 0.0, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, tuple);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }
                }

                // child element
                else if (StringUtil.Compare(root, ModelConstants.PrefixChild))
                {
                    if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        length == 3)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        if (createElements == true && tuple != null)
                        {
                            var wires = tuple.Item2;

                            wires.Add(new MapPin(name, args[2]));
                        }
                    }
                }
            }

            if (createElements == true)
            {
                creator.UpdateConnections(dict);

                if (appendIds == true)
                    creator.AppendIds(elements);

                if (updateIds == true)
                    creator.UpdateCounter(options.Counter, counter);

                creator.InsertElements(elements, select, offsetX, offsetY);
            }

            sw.Stop();

            System.Diagnostics.Debug.Print("Parse() in {0}ms", sw.Elapsed.TotalMilliseconds);
            System.Diagnostics.Debug.Print("> Solutions: {0}", total.SolutionCount);
            System.Diagnostics.Debug.Print(">  Projects: {0}", total.ProjectCount);
            System.Diagnostics.Debug.Print(">  Diagrams: {0}", total.DiagramCount);
            System.Diagnostics.Debug.Print(">      Pins: {0}", total.PinCount);
            System.Diagnostics.Debug.Print(">     Wires: {0}", total.WireCount);
            System.Diagnostics.Debug.Print(">    Inputs: {0}", total.InputCount);
            System.Diagnostics.Debug.Print(">   Outputs: {0}", total.OutputCount);
            System.Diagnostics.Debug.Print(">  AndGates: {0}", total.AndGateCount);
            System.Diagnostics.Debug.Print(">   OrGates: {0}", total.OrGateCount);

            return solution;
        }

        #endregion
    } 

    #endregion
}
