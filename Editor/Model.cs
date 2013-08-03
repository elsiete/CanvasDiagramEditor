// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Parser;
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

    #region Model

    public static class Model
    {
        #region Generate

        public static string Generate(IEnumerable<IElement> elements)
        {
            var sb = new StringBuilder();

            foreach (var element in elements)
            {
                double x = element.GetX();
                double y = element.GetY();
                string uid = element.GetUid();

                if (StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
                {
                    var line = element as ILine;
                    var margin = line.GetMargin();

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}{5}{7}{5}{8}{5}{9}{5}{10}",
                        uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.GetX2() + margin.Left, line.GetY2() + margin.Top,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        line.GetStartVisible(), line.GetEndVisible(),
                        line.GetStartIO(), line.GetEndIO());

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else if (StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput))
                {
                    var data = element.GetData();
                    Tag tag = null;

                    if (data != null && data is Tag)
                    {
                        tag = data as Tag;
                    }

                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}{3}{5}",
                        uid,
                        x,
                        y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        tag != null ? tag.Id : -1);

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                        uid,
                        x, y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot);

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }

                var elementTag = element.GetTag();
                if (elementTag != null && !(element is ILine))
                {
                    var selection = elementTag as Selection;
                    var tuples = selection.Item2;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1 as ILine;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        if (start != null)
                        {
                            // Start
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.GetUid(),
                                ModelConstants.WireStartType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            sb.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.GetUid(),
                                ModelConstants.WireEndType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            sb.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static string Generate(ICanvas canvas, string uid, DiagramProperties properties)
        {
            if (canvas == null)
            {
                return null;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();
            var elements = canvas.GetElements();

            string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                uid == null ? ModelConstants.TagHeaderDiagram : uid,
                properties.PageWidth, properties.PageHeight,
                properties.GridOriginX, properties.GridOriginY,
                properties.GridWidth, properties.GridHeight,
                properties.GridSize,
                properties.SnapX, properties.SnapY,
                properties.SnapOffsetX, properties.SnapOffsetY);

            sb.AppendLine(header);
            //System.Diagnostics.Debug.Print(header);

            string model = Generate(elements);

            sb.Append(model);

            var result = sb.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        #endregion

        #region Open

        public static string Open(string fileName)
        {
            string model = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                model = reader.ReadToEnd();
            }

            return model;
        }

        #endregion

        #region Save

        public static void Save(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        #endregion
    }

    #endregion
}
