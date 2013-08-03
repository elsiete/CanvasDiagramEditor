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

    #region Tags

    public static class Tags
    {
        #region Tags

        public static string Generate(List<object> tags)
        {
            string line = null;

            var sb = new StringBuilder();

            if (tags != null)
            {
                foreach (var tag in tags.Cast<Tag>())
                {
                    line = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                        ModelConstants.ArgumentSeparator,
                        tag.Id,
                        tag.Designation,
                        tag.Signal,
                        tag.Condition,
                        tag.Description);

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        public static List<object> Open(string fileName)
        {
            var tags = new List<object>();

            Import(fileName, tags, false);

            return tags;
        }

        public static void Save(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        public static void Import(string fileName, List<object> tags, bool appedIds)
        {
            int count = 0;
            if (appedIds == true)
            {
                count = tags.Count > 0 ? tags.Cast<Tag>().Max(x => x.Id) + 1 : 0;
            }

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string data = reader.ReadToEnd();

                var lines = data.Split(Environment.NewLine.ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var args = line.Split(new char[] { ModelConstants.ArgumentSeparator, '\t' },
                        StringSplitOptions.RemoveEmptyEntries);

                    int length = args.Length;

                    if (length == 5)
                    {
                        int id = -1;

                        if (appedIds == true)
                        {
                            id = count;
                            count = count + 1;
                        }
                        else
                        {
                            id = int.Parse(args[0]);
                        }

                        var tag = new Tag()
                        {
                            Id = id,
                            Designation = args[1],
                            Signal = args[2],
                            Condition = args[3],
                            Description = args[4]
                        };

                        tags.Add(tag);
                    }
                }
            }
        }

        public static void Export(string fileName, List<object> tags)
        {
            var model = Generate(tags);
            Save(fileName, model);
        }

        #endregion
    }

    #endregion
}
