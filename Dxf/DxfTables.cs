// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Dxf.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf
{
    #region DxfTables

    public static class DxfTables
    {
        #region DxfTablesBegin

        public static string DxfTablesBegin()
        {
            var sb = new StringBuilder();

            // begin tables section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("TABLES");

            return sb.ToString();
        }

        #endregion

        #region DxfTablesEnd

        public static string DxfTablesEnd()
        {
            var sb = new StringBuilder();

            // end tables section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        }

        #endregion

        #region DxfAppidBegin

        public static string DxfAppidBegin(int count)
        {
            var sb = new StringBuilder();

            // begin table: appid
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("APPID");

            // count
            sb.AppendLine("70");
            sb.AppendLine(count.ToString());

            return sb.ToString();
        }

        #endregion

        #region DxfAppidEnd

        public static string DxfAppidEnd()
        {
            var sb = new StringBuilder();

            // end table: appid
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");

            return sb.ToString();
        }

        #endregion

        #region DxfLayersBegin

        public static string DxfLayersBegin(int count)
        {
            var sb = new StringBuilder();

            // begin table: layers
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("LAYER");

            // count
            sb.AppendLine("70");
            sb.AppendLine(count.ToString());

            return sb.ToString();
        }

        #endregion

        #region DxfLayersEnd

        public static string DxfLayersEnd()
        {
            var sb = new StringBuilder();

            // end table: layers
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");

            return sb.ToString();
        }

        #endregion

        #region DxfLtypesBegin

        public static string DxfLtypesBegin(int count)
        {
            var sb = new StringBuilder();

            // begin table: line styles
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("LTYPE");

            // count
            sb.AppendLine("70");
            sb.AppendLine(count.ToString());

            return sb.ToString();
        }

        #endregion

        #region DxfLtypesEnd

        public static string DxfLtypesEnd()
        {
            var sb = new StringBuilder();

            // end table: line styles
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");

            return sb.ToString();
        }

        #endregion

        #region DxfStylesBegin

        public static string DxfStylesBegin(int count)
        {
            var sb = new StringBuilder();

            // begin table: text styles
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("STYLE");

            // count
            sb.AppendLine("70");
            sb.AppendLine(count.ToString());

            return sb.ToString();
        }

        #endregion

        #region DxfStylesEnd

        public static string DxfStylesEnd()
        {
            var sb = new StringBuilder();

            // end table: text styles
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");

            return sb.ToString();
        }

        #endregion

        #region DxfViewsBegin

        public static string DxfViewsBegin(int count)
        {
            var sb = new StringBuilder();

            // begin table: views
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("VIEW");

            // count
            sb.AppendLine("70");
            sb.AppendLine(count.ToString());

            return sb.ToString();
        }

        #endregion

        #region DxfViewsEnd

        public static string DxfViewsEnd()
        {
            var sb = new StringBuilder();

            // end table: views
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");

            return sb.ToString();
        }

        #endregion

        #region DxfLayer

        public static string DxfLayer(string name,
            DxfLayerFlags flags,
            string color,
            string lineTypeName)
        {
            var sb = new StringBuilder();

            // layer
            sb.AppendLine("0");
            sb.AppendLine("LAYER");

            // layer name
            sb.AppendLine("2");
            sb.AppendLine(name);

            // layer standard flags
            sb.AppendLine("70");
            sb.AppendLine(flags.ToString("d"));

            // color number - if negative, layer is off
            sb.AppendLine("62");
            sb.AppendLine(color);

            // linetype name
            sb.AppendLine("6");
            sb.AppendLine(lineTypeName);

            return sb.ToString();
        }

        #endregion

        #region DxfLtype

        public static string DxfLtype(string name,
            DxfLtypeFlags flags,
            string description,
            double dashLengthItems,
            double totalPatternLenght,
            double [] dashLenghts)
        {
            var sb = new StringBuilder();

            // style
            sb.AppendLine("0");
            sb.AppendLine("LTYPE");

            // linetype name
            sb.AppendLine("2");
            sb.AppendLine(name);

            // linetype description
            sb.AppendLine("3");
            sb.AppendLine(description);

            // alignment code; value is always 65, the ASCII code for A
            sb.AppendLine("72");
            sb.AppendLine("65");

            // style standard flag values
            sb.AppendLine("70");
            sb.AppendLine(flags.ToString("d"));

            // number of dash length items
            sb.AppendLine("73");
            sb.AppendLine(dashLengthItems.ToDxfString());

            // total pattern length
            sb.AppendLine("40");
            sb.AppendLine(totalPatternLenght.ToDxfString());

            if (dashLenghts != null)
            {
                // dash length 1,2...n = dashLengthItems
                foreach (var lenght in dashLenghts)
                {
                    sb.AppendLine("49");
                    sb.AppendLine(lenght.ToDxfString());
                }
            }

            return sb.ToString();
        }

        #endregion

        #region DxfStyle

        public static string DxfStyle(string name,
            DxfStyleFlags flags,
            double fixedTextHeight,
            double widthFactor,
            double obliqueAngle,
            DxfTextGenerationFlags textGenerationFlags,
            double lastHeightUsed,
            string fontFileName,
            string bifFontFileName)
        {
            var sb = new StringBuilder();

            // style
            sb.AppendLine("0");
            sb.AppendLine("STYLE");

            // style name
            sb.AppendLine("2");
            sb.AppendLine(name);

            // style standard flag values
            sb.AppendLine("70");
            sb.AppendLine(flags.ToString("d"));

            // fixed text height - 0 if not fixed
            sb.AppendLine("40");
            sb.AppendLine(fixedTextHeight.ToDxfString());

            // width factor
            sb.AppendLine("41");
            sb.AppendLine(widthFactor.ToDxfString());

            // oblique angle
            sb.AppendLine("50");
            sb.AppendLine(obliqueAngle.ToDxfString());

            // text generation flags
            sb.AppendLine("71");
            sb.AppendLine(textGenerationFlags.ToString("d"));

            // last height used
            sb.AppendLine("42");
            sb.AppendLine(lastHeightUsed.ToDxfString());

            // primary font file name
            sb.AppendLine("3");
            sb.AppendLine(fontFileName);

            // bigfont file name - blank if none
            sb.AppendLine("4");
            sb.AppendLine(bifFontFileName);

            return sb.ToString();
        }

        #endregion
    }

    #endregion
}
