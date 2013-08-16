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
            double offsetX = options.OffsetX;
            double offsetY = options.OffsetY;
            bool appendIds = options.AppendIds;
            bool updateIds = options.UpdateIds;
            bool select = options.Select;
            bool createElements = options.CreateElements;

            if (model == null)
                return null;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            string name = null;
            var counter = new IdCounter();
            var elements = new List<object>();
            MapWires tuple = null;
            var dict = new Dictionary<string, MapWires>();

            TreeSolution solution = null;
            TreeProjects projects = null;
            TreeProject project = null;
            TreeDiagrams diagrams = null;
            TreeDiagram diagram = null;

            var lines = model.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            //System.Diagnostics.Debug.Print("Parsing model:");

            foreach (var line in lines)
            {
                var args = line.Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                int length = args.Length;

                //System.Diagnostics.Debug.Print(line);

                if (length < 2)
                    continue;

                name = args[1];

                // root element
                if (StringUtil.Compare(args[0], ModelConstants.PrefixRoot))
                {
                    // Solution
                    if (StringUtil.StartsWith(name, ModelConstants.TagHeaderSolution) &&
                        (length == 2 || length == 3))
                    {
                        string tagFileName = null;
                        if (length == 3)
                        {
                            tagFileName = args[2];
                        }

                        projects = new TreeProjects();
                        solution = new TreeSolution(name, tagFileName, projects);
                    }

                    // Project
                    else if (StringUtil.StartsWith(name, ModelConstants.TagHeaderProject) &&
                        length == 2)
                    {
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
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.PinCount = Math.Max(counter.PinCount, id + 1);

                            //var element = creator.CreatePin(x + offsetX, y + offsetY, id, false);
                            //elements.Add(element);
                            var element = creator.CreateElement("pin",
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }

                    // Input
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementInput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            int tagId = -1;

                            if (length == 5)
                            {
                                tagId = int.Parse(args[4]);
                            }

                            counter.InputCount = Math.Max(counter.InputCount, id + 1);

                            //var element = creator.CreateInput(x + offsetX, y + offsetY, id, tagId, false);
                            //elements.Add(element);
                            var element = creator.CreateElement("input",
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }

                    // Output
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOutput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            int tagId = -1;

                            if (length == 5)
                            {
                                tagId = int.Parse(args[4]);
                            }

                            counter.OutputCount = Math.Max(counter.OutputCount, id + 1);

                            //var element = creator.CreateOutput(x + offsetX, y + offsetY, id, tagId, false);
                            //elements.Add(element);
                            var element = creator.CreateElement("output",
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }

                    // AndGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementAndGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.AndGateCount = Math.Max(counter.AndGateCount, id + 1);

                            //var element = creator.CreateAndGate(x + offsetX, y + offsetY, id, false);
                            //elements.Add(element);
                            var element = creator.CreateElement("andgate",
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }

                    // OrGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOrGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.OrGateCount = Math.Max(counter.OrGateCount, id + 1);

                            //var element = creator.CreateOrGate(x + offsetX, y + offsetY, id, false);
                            //elements.Add(element);
                            var element = creator.CreateElement("orgate",
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            if (dict.ContainsKey(name) == false)
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }

                    // Wire
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        (length == 6 || length == 8 || length == 10))
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x1 = double.Parse(args[2]);
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            bool startVisible = false;
                            bool endVisible = false;

                            bool startIsIO = false;
                            bool endIsIO = false;

                            if (length == 8 || length == 10)
                            {
                                startVisible = bool.Parse(args[6]);
                                endVisible = bool.Parse(args[7]);
                            }

                            if (length == 10)
                            {
                                startIsIO = bool.Parse(args[8]);
                                endIsIO = bool.Parse(args[9]);
                            }

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.WireCount = Math.Max(counter.WireCount, id + 1);

                            //var element = creator.CreateWire(x1 + offsetX, y1 + offsetY,
                            //    x2 + offsetX, y2 + offsetY,
                            //    startVisible, endVisible,
                            //    startIsIO, endIsIO,
                            //    id);
                            //elements.Add(element);
                            var element = creator.CreateElement("wire",
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
                            {
                                dict.Add(name, tuple);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                            }
                        }
                    }
                }

                // child element
                else if (StringUtil.Compare(args[0], ModelConstants.PrefixChild))
                {
                    if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        length == 3)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

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
                {
                    creator.AppendIds(elements);
                }

                if (updateIds == true)
                {
                    creator.UpdateCounter(options.Counter, counter);
                }

                creator.InsertElements(elements, select, offsetX, offsetY);
            }

            sw.Stop();
            System.Diagnostics.Debug.Print("Parse() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return solution;
        }

        #endregion
    } 

    #endregion
}
