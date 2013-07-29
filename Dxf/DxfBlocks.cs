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
    #region DxfBlocks

    public static class DxfBlocks
    {
        #region DxfBlocksBegin

        public static string DxfBlocksBegin()
        {
            var sb = new StringBuilder();

            // begin blocks section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("BLOCKS");

            return sb.ToString();
        } 

        #endregion

        #region DxfBlocksEnd

        public static string DxfBlocksEnd()
        {
            var sb = new StringBuilder();

            // end blocks section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        } 

        #endregion

        #region DxfBlockBegin

        public static string DxfBlockBegin()
        {
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            return sb.ToString();
        }

        #endregion

        #region DxfBlockEnd

        public static string DxfBlockEnd()
        {
            var sb = new StringBuilder();

            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        #endregion
    }

    #endregion
}
