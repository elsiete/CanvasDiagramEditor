// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
    #region Tags

    public static class Tags
    {
        #region Tags

        public static string Generate(List<object> tags)
        {
            var sb = new StringBuilder();

            if (tags != null)
            {
                foreach (var tag in tags.Cast<Tag>())
                {
                    sb.Append(tag.Id);
                    sb.Append(Constants.ArgumentSeparator);
                    sb.Append(tag.Designation);
                    sb.Append(Constants.ArgumentSeparator);
                    sb.Append(tag.Signal);
                    sb.Append(Constants.ArgumentSeparator);
                    sb.Append(tag.Condition);
                    sb.Append(Constants.ArgumentSeparator);
                    sb.Append(tag.Description);
                    sb.Append(Environment.NewLine);
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
                count = tags.Count > 0 ? tags.Cast<Tag>().Max(x => x.Id) + 1 : 0;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string data = reader.ReadToEnd();

                var lines = data.Split(Environment.NewLine.ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var args = line.Split(new char[] { Constants.ArgumentSeparator, '\t' },
                        StringSplitOptions.RemoveEmptyEntries);

                    if (args.Length == 5)
                    {
                        bool validId = true;
                        int id = -1;

                        if (appedIds == true)
                        {
                            id = count;
                            count = count + 1;
                        }
                        else
                        {
                            validId = int.TryParse(args[0], out id);
                        }

                        if (validId == true)
                        {
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
