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
    #region DxfEntities

    public static class DxfEntities
    {
        #region DxfEntitiesBegin

        public static string DxfEntitiesBegin()
        {
            var sb = new StringBuilder();

            // begin entities section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("ENTITIES");

            return sb.ToString();
        } 

        #endregion

        #region DxfEntitiesEnd

        public static string DxfEntitiesEnd()
        {
            var sb = new StringBuilder();

            // end entities section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        } 

        #endregion

        #region DxfText

        public static string DxfText(double thickness,
            DxfPoint3 firstAlignmentPoint,
            DxfPoint3 secondAlignmentPoint,
            DxfPoint3 extrusionDirection,
            string text,
            double textHeight,
            double textRotation,
            double obliqueAngle,
            string textStyle,
            double scaleFactorX,
            DxfTextGenerationFlags textGenerationFlags,
            DxfHorizontalTextJustification horizontalTextJustification,
            DxfVerticalTextJustification verticalTextJustification,
            string layer)
        {
            var sb = new StringBuilder();

            // begin text definition
            sb.AppendLine("0");
            sb.AppendLine("TEXT");

            // layer
            if (layer != null)
            {
                sb.AppendLine("8");
                sb.AppendLine(layer);
            }

            // thickness 
            if (thickness != 0.0)
            {
                sb.AppendLine("39");
                sb.AppendLine(thickness.ToDxfString());
            }

            // default value
            sb.AppendLine("1");
            sb.AppendLine(text);

            // text style
            sb.AppendLine("7");
            sb.AppendLine(textStyle);

            // text height
            sb.AppendLine("40");
            sb.AppendLine(textHeight.ToDxfString());

            if (textRotation != 0.0)
            {
                // text rotation 
                sb.AppendLine("50");
                sb.AppendLine(textRotation.ToDxfString());
            }

            if (obliqueAngle != 0.0)
            {
                // oblique angle
                sb.AppendLine("51");
                sb.AppendLine(obliqueAngle.ToDxfString());
            }

            if (scaleFactorX != 1.0)
            {
                // relative X scale factor
                sb.AppendLine("41");
                sb.AppendLine(scaleFactorX.ToDxfString());
            }

            // first alignment : X
            sb.AppendLine("10");
            sb.AppendLine(firstAlignmentPoint.X.ToDxfString());

            // first alignment : Y
            sb.AppendLine("20");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // first alignment: Z
            sb.AppendLine("30");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // second alignment  point: X
            sb.AppendLine("11");
            sb.AppendLine(secondAlignmentPoint.X.ToDxfString());

            // second alignment  point: Y
            sb.AppendLine("21");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            // second alignment  point: Z
            sb.AppendLine("31");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            if (extrusionDirection != null)
            {
                // extrusion direction: X
                sb.AppendLine("210");
                sb.AppendLine(extrusionDirection.X.ToDxfString());

                // extrusion direction: Y
                sb.AppendLine("220");
                sb.AppendLine(extrusionDirection.Y.ToDxfString());

                // extrusion direction: Z
                sb.AppendLine("230");
                sb.AppendLine(extrusionDirection.Z.ToDxfString());
            }

            if (textGenerationFlags != DxfTextGenerationFlags.Default)
            {
                // text generation flags
                sb.AppendLine("71");
                sb.AppendLine(textGenerationFlags.ToString("d"));
            }

            if (horizontalTextJustification != DxfHorizontalTextJustification.Default)
            {
                // horizontal justification
                sb.AppendLine("72");
                sb.AppendLine(horizontalTextJustification.ToString("d"));
            }

            if (verticalTextJustification != DxfVerticalTextJustification.Default)
            {
                // vertical text justification
                sb.AppendLine("73");
                sb.AppendLine(verticalTextJustification.ToString("d"));
            }

            return sb.ToString();
        }

        #endregion

        #region DxfCircle

        public static string DxfCircle(double thickness,
            DxfPoint3 centerPoint,
            double radius,
            DxfPoint3 extrusionDirection,
            string layer,
            string color)
        {
            var sb = new StringBuilder();

            // line
            sb.AppendLine("0");
            sb.AppendLine("CIRCLE");

            // layer
            if (layer != null)
            {
                sb.AppendLine("8");
                sb.AppendLine(layer);
            }

            // color
            if (color != null)
            {
                sb.AppendLine("62");
                sb.AppendLine(color);
            }

            // thickness 
            if (thickness != 0.0)
            {
                sb.AppendLine("39");
                sb.AppendLine(thickness.ToDxfString());
            }

            // radius 
            sb.AppendLine("40");
            sb.AppendLine(radius.ToDxfString());

            // center point: X
            sb.AppendLine("10");
            sb.AppendLine(centerPoint.X.ToDxfString());

            // center point: Y
            sb.AppendLine("20");
            sb.AppendLine(centerPoint.Y.ToDxfString());

            // center point: Z
            sb.AppendLine("30");
            sb.AppendLine(centerPoint.Y.ToDxfString());

            if (extrusionDirection != null)
            {
                // extrusion direction: X
                sb.AppendLine("210");
                sb.AppendLine(extrusionDirection.X.ToDxfString());

                // extrusion direction: Y
                sb.AppendLine("220");
                sb.AppendLine(extrusionDirection.Y.ToDxfString());

                // extrusion direction: Z
                sb.AppendLine("230");
                sb.AppendLine(extrusionDirection.Z.ToDxfString());
            }

            return sb.ToString();
        }

        #endregion

        #region DxfLine

        public static string DxfLine(double thickness,
            DxfPoint3 startPoint,
            DxfPoint3 endPoint,
            DxfPoint3 extrusionDirection,
            string layer,
            string color)
        {
            var sb = new StringBuilder();

            // line
            sb.AppendLine("0");
            sb.AppendLine("LINE");

            // layer
            if (layer != null)
            {
                sb.AppendLine("8");
                sb.AppendLine(layer);
            }

            // color
            if (color != null)
            {
                sb.AppendLine("62");
                sb.AppendLine(color);
            }

            // thickness 
            if (thickness != 0.0)
            {
                sb.AppendLine("39");
                sb.AppendLine(thickness.ToDxfString());
            }

            // start point: X
            sb.AppendLine("10");
            sb.AppendLine(startPoint.X.ToDxfString());

            // start point: Y
            sb.AppendLine("20");
            sb.AppendLine(startPoint.Y.ToDxfString());

            // start point: Z
            sb.AppendLine("30");
            sb.AppendLine(startPoint.Y.ToDxfString());

            // end point: X
            sb.AppendLine("11");
            sb.AppendLine(endPoint.X.ToDxfString());

            // end point: Y
            sb.AppendLine("21");
            sb.AppendLine(endPoint.Y.ToDxfString());

            // end point: Z
            sb.AppendLine("31");
            sb.AppendLine(endPoint.Y.ToDxfString());

            if (extrusionDirection != null)
            {
                // extrusion direction: X
                sb.AppendLine("210");
                sb.AppendLine(extrusionDirection.X.ToDxfString());

                // extrusion direction: Y
                sb.AppendLine("220");
                sb.AppendLine(extrusionDirection.Y.ToDxfString());

                // extrusion direction: Z
                sb.AppendLine("230");
                sb.Append(extrusionDirection.Z.ToDxfString());
            }

            return sb.ToString();
        }

        #endregion

        #region DxfAttdef

        public static string DxfAttdef(double thickness,
            DxfPoint3 firstAlignmentPoint,
            DxfPoint3 secondAlignmentPoint,
            DxfPoint3 extrusionDirection,
            string tag,
            string defaultValue,
            string prompt,
            DxfAttributeFlags attributeFlags,
            double textHeight,
            double textRotation,
            double obliqueAngle,
            string textStyle,
            double scaleFactorX,
            DxfTextGenerationFlags textGenerationFlags,
            DxfHorizontalTextJustification horizontalTextJustification,
            DxfVerticalTextJustification verticalTextJustification,
            string layer)
        {
            var sb = new StringBuilder();

            // begin attribute definition
            sb.AppendLine("0");
            sb.AppendLine("ATTDEF");

            // layer
            if (layer != null)
            {
                sb.AppendLine("8");
                sb.AppendLine(layer);
            }

            // thickness 
            if (thickness != 0.0)
            {
                sb.AppendLine("39");
                sb.AppendLine(thickness.ToDxfString());
            }

            // default value
            sb.AppendLine("1");
            sb.AppendLine(defaultValue);

            // tag string
            sb.AppendLine("2");
            sb.AppendLine(tag);

            // prompt string
            sb.AppendLine("3");
            sb.AppendLine(prompt);

            // text style
            sb.AppendLine("7");
            sb.AppendLine(textStyle);

            // text height
            sb.AppendLine("40");
            sb.AppendLine(textHeight.ToDxfString());

            if (textRotation != 0.0)
            {
                // text rotation 
                sb.AppendLine("50");
                sb.AppendLine(textRotation.ToDxfString());
            }

            if (obliqueAngle != 0.0)
            {
                // oblique angle
                sb.AppendLine("51");
                sb.AppendLine(obliqueAngle.ToDxfString());
            }

            if (scaleFactorX != 1.0)
            {
                // relative X scale factor
                sb.AppendLine("41");
                sb.AppendLine(scaleFactorX.ToDxfString());
            }

            // first alignment : X
            sb.AppendLine("10");
            sb.AppendLine(firstAlignmentPoint.X.ToDxfString());

            // first alignment : Y
            sb.AppendLine("20");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // first alignment: Z
            sb.AppendLine("30");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // second alignment  point: X
            sb.AppendLine("11");
            sb.AppendLine(secondAlignmentPoint.X.ToDxfString());

            // second alignment  point: Y
            sb.AppendLine("21");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            // second alignment  point: Z
            sb.AppendLine("31");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            if (extrusionDirection != null)
            {
                // extrusion direction: X
                sb.AppendLine("210");
                sb.AppendLine(extrusionDirection.X.ToDxfString());

                // extrusion direction: Y
                sb.AppendLine("220");
                sb.AppendLine(extrusionDirection.Y.ToDxfString());

                // extrusion direction: Z
                sb.AppendLine("230");
                sb.AppendLine(extrusionDirection.Z.ToDxfString());
            }

            // attribute flags
            sb.AppendLine("70");
            sb.AppendLine(attributeFlags.ToString("d"));

            if (textGenerationFlags != DxfTextGenerationFlags.Default)
            {
                // text generation flags
                sb.AppendLine("71");
                sb.AppendLine(textGenerationFlags.ToString("d"));
            }

            if (horizontalTextJustification != DxfHorizontalTextJustification.Default)
            {
                // horizontal justification
                sb.AppendLine("72");
                sb.AppendLine(horizontalTextJustification.ToString("d"));
            }

            if (verticalTextJustification != DxfVerticalTextJustification.Default)
            {
                // vertical text justification
                sb.AppendLine("74");
                sb.AppendLine(verticalTextJustification.ToString("d"));
            }

            return sb.ToString();
        }

        #endregion

        #region DxfAttrib

        public static string DxfAttrib(double thickness,
            DxfPoint3 firstAlignmentPoint,
            DxfPoint3 secondAlignmentPoint,
            DxfPoint3 extrusionDirection,
            string attributeTag,
            string defaultValue,
            DxfAttributeFlags attributeFlags,
            double textHeight,
            double textRotation,
            double obliqueAngle,
            string textStyle,
            double scaleFactorX,
            DxfTextGenerationFlags textGenerationFlags,
            DxfHorizontalTextJustification horizontalTextJustification,
            DxfVerticalTextJustification verticalTextJustification,
            string layer)
        {
            var sb = new StringBuilder();

            // begin attribute definition
            sb.AppendLine("0");
            sb.AppendLine("ATTRIB");

            // layer
            if (layer != null)
            {
                sb.AppendLine("8");
                sb.AppendLine(layer);
            }

            // thickness 
            if (thickness != 0.0)
            {
                sb.AppendLine("39");
                sb.AppendLine(thickness.ToDxfString());
            }

            // default value
            sb.AppendLine("1");
            sb.AppendLine(defaultValue);

            // attribute tag
            sb.AppendLine("2");
            sb.AppendLine(attributeTag);

            // text style
            sb.AppendLine("7");
            sb.AppendLine(textStyle);

            // text height
            sb.AppendLine("40");
            sb.AppendLine(textHeight.ToDxfString());

            if (textRotation != 0.0)
            {
                // text rotation 
                sb.AppendLine("50");
                sb.AppendLine(textRotation.ToDxfString());
            }

            if (obliqueAngle != 0.0)
            {
                // oblique angle
                sb.AppendLine("51");
                sb.AppendLine(obliqueAngle.ToDxfString());
            }

            if (scaleFactorX != 1.0)
            {
                // relative X scale factor
                sb.AppendLine("41");
                sb.AppendLine(scaleFactorX.ToDxfString());
            }

            // first alignment : X
            sb.AppendLine("10");
            sb.AppendLine(firstAlignmentPoint.X.ToDxfString());

            // first alignment : Y
            sb.AppendLine("20");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // first alignment: Z
            sb.AppendLine("30");
            sb.AppendLine(firstAlignmentPoint.Y.ToDxfString());

            // second alignment  point: X
            sb.AppendLine("11");
            sb.AppendLine(secondAlignmentPoint.X.ToDxfString());

            // second alignment  point: Y
            sb.AppendLine("21");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            // second alignment  point: Z
            sb.AppendLine("31");
            sb.AppendLine(secondAlignmentPoint.Y.ToDxfString());

            if (extrusionDirection != null)
            {
                // extrusion direction: X
                sb.AppendLine("210");
                sb.AppendLine(extrusionDirection.X.ToDxfString());

                // extrusion direction: Y
                sb.AppendLine("220");
                sb.AppendLine(extrusionDirection.Y.ToDxfString());

                // extrusion direction: Z
                sb.AppendLine("230");
                sb.AppendLine(extrusionDirection.Z.ToDxfString());
            }

            // attribute flags
            sb.AppendLine("70");
            sb.AppendLine(attributeFlags.ToString("d"));

            if (textGenerationFlags != DxfTextGenerationFlags.Default)
            {
                // text generation flags
                sb.AppendLine("71");
                sb.AppendLine(textGenerationFlags.ToString("d"));
            }

            if (horizontalTextJustification != DxfHorizontalTextJustification.Default)
            {
                // horizontal justification
                sb.AppendLine("72");
                sb.AppendLine(horizontalTextJustification.ToString("d"));
            }

            if (verticalTextJustification != DxfVerticalTextJustification.Default)
            {
                // vertical text justification
                sb.AppendLine("74");
                sb.AppendLine(verticalTextJustification.ToString("d"));
            }

            return sb.ToString();
        }

        #endregion

        #region DxfEof

        public static string DxfEof()
        {
            var sb = new StringBuilder();

            // end if file
            sb.AppendLine("0");
            sb.AppendLine("EOF");

            return sb.ToString();
        }

        #endregion
    }

    #endregion
}
