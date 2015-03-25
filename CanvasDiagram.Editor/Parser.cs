// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Util;
using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanvasDiagram.Core.Model; 

#endregion

namespace CanvasDiagram.Editor
{
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
            Child child = null;
            var dict = new Dictionary<string, Child>();
            TreeSolution solution = null;
            TreeProjects projects = null;
            TreeProject project = null;
            TreeDiagrams diagrams = null;
            TreeDiagram diagram = null;

            var lines = model.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var args = GetArgs(line);

                int length = args.Length;
                if (length < 2)
                    continue;

                root = args[0];
                name = args[1];

                // root element
                if (StringUtil.Compare(root, Constants.PrefixRoot))
                {
                    // Solution
                    if (StringUtil.StartsWith(name, Constants.TagHeaderSolution) &&
                        (length == 2 || length == 3 || length == 4))
                    {
                        int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                        counter.Set(Math.Max(counter.Count, id + 1));

                        total.Next();

                        string tagFileName = (length == 3) ? args[2] : null;
                        string tableFileName = (length == 4) ? args[3] : null;

                        projects = new TreeProjects();
                        solution = new TreeSolution(name, tagFileName, tableFileName, projects);
                    }

                    // Project
                    else if (StringUtil.StartsWith(name, Constants.TagHeaderProject) &&
                        length == 2)
                    {
                        int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                        counter.Set(Math.Max(counter.Count, id + 1));

                        total.Next();

                        if (projects != null)
                        {
                            diagrams = new TreeDiagrams();
                            project = new TreeProject(name, diagrams);
                            projects.Push(project);
                        }
                    }

                    // Diagram
                    else if (StringUtil.StartsWith(name, Constants.TagHeaderDiagram) &&
                        length == 13)
                    {
                        int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                        counter.Set(Math.Max(counter.Count, id + 1));

                        total.Next();

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
                    else if (StringUtil.StartsWith(name, Constants.TagElementPin) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementPin,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Input
                    else if (StringUtil.StartsWith(name, Constants.TagElementInput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                            int tagId = (length == 5) ? int.Parse(args[4]) : -1;

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementInput,
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Output
                    else if (StringUtil.StartsWith(name, Constants.TagElementOutput) &&
                        (length == 4 || length == 5))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                            int tagId = (length == 5) ? int.Parse(args[4]) : -1;

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementOutput,
                                new object[] { id, tagId },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // AndGate
                    else if (StringUtil.StartsWith(name, Constants.TagElementAndGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementAndGate,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // OrGate
                    else if (StringUtil.StartsWith(name, Constants.TagElementOrGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementOrGate,
                                new object[] { id },
                                x + offsetX, y + offsetY, false);
                            elements.Add(element);

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }

                    // Wire
                    else if (StringUtil.StartsWith(name, Constants.TagElementWire) &&
                        (length == 6 || length == 8 || length == 10))
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        total.Next();

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
                            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);

                            counter.Set(Math.Max(counter.Count, id + 1));

                            var element = creator.CreateElement(Constants.TagElementWire,
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

                            child = new Child(element, new List<Pin>());

                            if (dict.ContainsKey(name) == false)
                                dict.Add(name, child);
                            else
                                System.Diagnostics.Debug.Print("Dictionary already contains name key: {0}", name);
                        }
                    }
                }

                // child element
                else if (StringUtil.Compare(root, Constants.PrefixChild))
                {
                    if (StringUtil.StartsWith(name, Constants.TagElementWire) &&
                        length == 3)
                    {
                        if (diagram != null)
                            diagram.Push(line);

                        if (createElements == true && child != null)
                        {
                            var pins = child.Pins;

                            pins.Add(new Pin(name, args[2]));
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

            return solution;
        }

        private static string[] GetArgs(string line)
        {
            return line.Split(new char[] { Constants.ArgumentSeparator, '\t', ' ' },
                StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    } 

    #endregion
}
