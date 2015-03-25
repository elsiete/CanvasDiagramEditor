// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Dxf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf
{
    #region DxfInspect

    public class DxfInspect
    {
        #region Fields

        private const string DxfCodeForType = "0";
        private const string DxfCodeForName = "2";

        #endregion

        #region Inspect

        public string GetHtml(string fileName)
        {
            var sb = new StringBuilder();

            string data = GetDxfData(fileName);
            if (data == null)
                return null;

            ParseDxfData(sb, fileName, data);

            return sb.ToString();
        }

        private string GetDxfData(string fileName)
        {
            string data = null;

            try
            {
                using (var reader = new System.IO.StreamReader(fileName))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                System.Diagnostics.Debug.Print(ex.StackTrace);
            }

            return data;
        }

        private void ParseDxfData(StringBuilder sb, string fileName, string data)
        {
            WriteHtmlHeader(sb, fileName);
            WriteBodyHeader(sb, fileName);

            var lines = data.Split("\n".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            var tags = new List<DxfRawTag>();
            DxfRawTag tag = null;

            bool previousName = false;
            bool haveSection = false;
            //bool haveDxfCodeForType = false;

            string[] entity = new string[2] { null, null };
            int lineNumber = 0;

            foreach (var line in lines)
            {
                var str = line.Trim();

                // DxfCodeForType data
                if (tag != null)
                {
                    tag.Data = str;
                    tags.Add(tag);

                    haveSection = WriteTag(sb, tag, haveSection, lineNumber, str);

                    //haveDxfCodeForType = true;

                    tag = null;
                }
                else
                {
                    if (str == DxfCodeForType && entity[0] == null)
                    {
                        tag = new DxfRawTag();
                        tag.Code = str;
                    }
                    else
                    {
                        if (entity[0] == null)
                        {
                            entity[0] = str;
                            entity[1] = null;
                        }
                        else
                        {
                            entity[1] = str;

                            WriteEntity(sb, entity, lineNumber);

                            // entity Name
                            previousName = entity[0] == DxfCodeForName;

                            entity[0] = null;
                            entity[1] = null;

                            //haveDxfCodeForType = false;
                        }
                    }
                }

                lineNumber++;
            }

            WriteBodyFooter(sb, haveSection);
            WriteHtmlFooter(sb);
        }

        private static bool WriteTag(StringBuilder sb, 
            DxfRawTag tag,
            bool haveSection, 
            int lineNumber, 
            string str)
        {
            bool isHeaderSection = str == CodeName.Section;
            string entityClass = isHeaderSection == true ? "section" : "other";

            if (haveSection == true && isHeaderSection == true)
            {
                sb.AppendFormat("</div>");
                haveSection = false;
            }

            if (haveSection == false && isHeaderSection == true)
            {
                haveSection = true;
                sb.AppendFormat("<div class=\"content\">");
                sb.AppendFormat("<dt class=\"header\"><code class=\"lineHeader\">LINE</code><code class=\"codeHeader\">CODE</code><code class=\"dataHeader\">DATA</code></dt>{0}", Environment.NewLine);
            }

            sb.AppendFormat("<dt class=\"{3}\"><code class=\"line\">{4}</code><code class=\"code\">{0}:</code><code class=\"data\">{1}</code></dt>{2}",
                tag.Code,
                tag.Data,
                Environment.NewLine,
                entityClass,
                lineNumber);

            return haveSection;
        }

        private static void WriteEntity(StringBuilder sb, string[] entity, int lineNumber)
        {
            sb.AppendFormat("<dd><code class=\"line\">{3}</code><code class=\"code\">{0}:</code><code class=\"data\">{1}</code></dd>{2}",
                entity[0],
                entity[1],
                Environment.NewLine, lineNumber);
        }

        private static void WriteBodyHeader(StringBuilder sb, string fileName)
        {
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine("<div class=\"header\">");
            sb.AppendFormat("<h1 class=\"header\">{0}</h1></div>{1}",
                System.IO.Path.GetFileName(fileName),
                Environment.NewLine);

            sb.AppendLine("<dl>");
        }

        private static void WriteBodyFooter(StringBuilder sb, bool haveSection)
        {
            if (haveSection == true)
            {
                sb.AppendFormat("</div>");
            }

            sb.AppendLine(@"</dl>");

            sb.AppendLine("<div class=\"footer\">Copyright (C) Wiesław Šoltés 2013. All Rights Reserved</div>");
            sb.AppendLine("</div>");
        }

        private void WriteHtmlHeader(StringBuilder sb, string fileName)
        {
            sb.AppendLine("<html><head>");

            sb.AppendFormat("<title>{0}</title>{1}",
                System.IO.Path.GetFileName(fileName), Environment.NewLine);

            sb.AppendFormat("<meta charset=\"utf-8\"/>");

            sb.AppendLine("<style>");
            sb.AppendLine("body { background-color:rgb(221,221,221); }");
            sb.AppendLine("dl,dt,dd { font-family: Arial; font-size:10pt; width:100%; }");
            sb.AppendLine("dl { font-weight:normal; margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(221,221,221); }");

            sb.AppendLine("dt { font-weight:bold; }");
            sb.AppendLine("dt.header { margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(255,30,102); }");
            sb.AppendLine("dt.section { margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(255,242,102); }");
            sb.AppendLine("dt.other { margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(191,191,191); }");

            sb.AppendLine("dd { font-weight:normal; margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(221,221,221); }");

            sb.AppendLine("code.lineHeader { width:2.0cm; text-align:Left; color:rgb(0,0,0); }");
            sb.AppendLine("code.codeHeader { width:1.2cm; text-align:right; color:rgb(0,0,0); }");
            sb.AppendLine("code.dataHeader { margin:0.0cm 0.0cm 0.0cm 0.5cm; text-align:left; color:rgb(0,0,0); }");

            sb.AppendLine("code.line { width:2.0cm; text-align:Left; color:rgb(84,84,84); }");
            sb.AppendLine("code.code { width:1.2cm; text-align:right; color:rgb(116,116,116); }");
            sb.AppendLine("code.data { margin:0.0cm 0.0cm 0.0cm 0.5cm; text-align:left; color:rgb(0,0,0); }");

            sb.AppendLine("div.footer { font-family: Arial; font-size:10pt; }");

            sb.AppendLine("div.container { clear:both; width:auto; display:inline-block; zoom: 1;*display: inline; height:0.0cm; vertical-align:top; overflow:auto; }");
            sb.AppendLine("div.header { margin:0.2cm; }");
            sb.AppendLine("div.content { margin:0.2cm; width:10.0cm; float:left; }");
            sb.AppendLine("div.footer { margin:0.2cm;clear:both;text-align:center; }");

            sb.AppendLine("h1 { font-family: Arial; font-size:12pt; }");
            sb.AppendLine("h1.header { margin-bottom:0; }");

            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
        }

        private void WriteHtmlFooter(StringBuilder sb)
        {
            sb.AppendLine("</body></html>");
        }

        #endregion
    }

    #endregion
}
