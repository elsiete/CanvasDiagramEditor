// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor
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
    using System.Windows;

    #endregion

    public class Dxf : IDiagramCreator
    {
        #region Properties

        public bool ShortenStart { get; set; }
        public bool ShortenEnd { get; set; }

        public DiagramProperties DiagramProperties { get; set; }

        #endregion

        #region Fields

        private StringBuilder DxfString = null;

        //private double PageWidth = 1260.0;
        private double PageHeight = 891.0;

        private string LayerFrame = "FRAME";
        private string LayerGrid = "GRID";
        private string LayerTable = "TABLE";
        private string LayerIO = "IO";
        private string LayerWires = "WIRES";
        private string LayerElements = "ELEMENTS";

        private const double ShortenLineSize = 15.0;

        private double RadiusInvertedEllipse = 4.0;
        private double ThicknessWire = 1.0;

        #endregion

        #region Dxf

        public string DxfHeader(string title)
        {
            var sb = new StringBuilder();

            // header comment
            sb.AppendLine("999");
            sb.AppendLine(title);

            // begin header section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("HEADER");

            sb.AppendLine("9");
            sb.AppendLine("$ACADVER");
            sb.AppendLine("1");
            sb.AppendLine("AC1006");

            sb.AppendLine("9");
            sb.AppendLine("$INSBASE");
            sb.AppendLine("10");
            sb.AppendLine("0.0");
            sb.AppendLine("20");
            sb.AppendLine("0.0");
            sb.AppendLine("30");
            sb.AppendLine("0.0");

            sb.AppendLine("9");
            sb.AppendLine("$EXTMIN");
            sb.AppendLine("10");
            sb.AppendLine("0.0");
            sb.AppendLine("20");
            sb.AppendLine("0.0");

            sb.AppendLine("9");
            sb.AppendLine("$EXTMAX");
            sb.AppendLine("10");
            sb.AppendLine("1260.0");
            sb.AppendLine("20");
            sb.AppendLine("891.0");

            sb.AppendLine("9");
            sb.AppendLine("$LIMMIN");
            sb.AppendLine("10");
            sb.AppendLine("0.0");
            sb.AppendLine("20");
            sb.AppendLine("0.0");

            sb.AppendLine("9");
            sb.AppendLine("$LIMMAX");
            sb.AppendLine("10");
            sb.AppendLine("1260.0");
            sb.AppendLine("20");
            sb.AppendLine("891.0");
            sb.AppendLine("0");

            // end header section
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        }

        public string DxfTables()
        { 
            var sb = new StringBuilder();

            // begin tables section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("TABLES");

            DxfTableLineStyles(sb);

            DxfTableLayers(sb);

            DxfTableTextStyles(sb);

            // end tables section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        }

        private void DxfTableLineStyles(StringBuilder sb)
        {
            // begin table: line styles
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("LTYPE");
            sb.AppendLine("70");
            sb.AppendLine("1");

            // ltype: CONTINUOUS
            sb.AppendLine("0");
            sb.AppendLine("LTYPE");
            sb.AppendLine("2");
            sb.AppendLine("CONTINUOUS");
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("3");
            sb.AppendLine("Solid line");
            sb.AppendLine("72");
            sb.AppendLine("65");
            sb.AppendLine("73");
            sb.AppendLine("0");
            sb.AppendLine("40");
            sb.AppendLine("0.000000");

            // end table: line styles
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");
        }

        private void DxfTableLayers(StringBuilder sb)
        {
            // begin table: layers
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("LAYER");
            sb.AppendLine("70");
            sb.AppendLine("6");

            // layer: FRAME
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerFrame); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("8"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); /// line

            // layer: GRID
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerGrid); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("9"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); // line

            // layer: TABLE
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerTable); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("8"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); // line

            // layer: IO
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerIO); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("6"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); // line

            // layer: WIRES
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerWires); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("5"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); // line

            // layer: ELEMENTS
            sb.AppendLine("0");
            sb.AppendLine("LAYER");
            sb.AppendLine("2");
            sb.AppendLine(LayerElements); // name
            sb.AppendLine("70");
            sb.AppendLine("64");
            sb.AppendLine("62");
            sb.AppendLine("5"); // color
            sb.AppendLine("6");
            sb.AppendLine("CONTINUOUS"); // line

            // end table: layers
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");
        }

        private void DxfTableTextStyles(StringBuilder sb)
        {
            // begin table: text styles
            sb.AppendLine("0");
            sb.AppendLine("TABLE");
            sb.AppendLine("2");
            sb.AppendLine("STYLE");
            sb.AppendLine("70");
            sb.AppendLine("2");

            // style: TEXTGATE
            sb.AppendLine("0");
            sb.AppendLine("STYLE");
            sb.AppendLine("2");
            sb.AppendLine("TEXTGATE");
            sb.AppendLine("70");
            sb.AppendLine("0");
            sb.AppendLine("3");
            sb.AppendLine("ARIAL");
            sb.AppendLine("40");
            sb.AppendLine("0");
            sb.AppendLine("41");
            sb.AppendLine("1");
            sb.AppendLine("50");
            sb.AppendLine("0");
            sb.AppendLine("71");
            sb.AppendLine("0");
            sb.AppendLine("42");
            sb.AppendLine("0");

            // style: TEXTIO
            sb.AppendLine("0");
            sb.AppendLine("STYLE");
            sb.AppendLine("2");
            sb.AppendLine("TEXTIO");
            sb.AppendLine("70");
            sb.AppendLine("0");
            sb.AppendLine("3");
            sb.AppendLine("ARIAL");
            sb.AppendLine("40");
            sb.AppendLine("0");
            sb.AppendLine("41");
            sb.AppendLine("1");
            sb.AppendLine("50");
            sb.AppendLine("0");
            sb.AppendLine("71");
            sb.AppendLine("0");
            sb.AppendLine("42");
            sb.AppendLine("0");

            // end table: text styles
            sb.AppendLine("0");
            sb.AppendLine("ENDTAB");
        }

        public string DxfBeginBlocks()
        {
            var sb = new StringBuilder();

            // begin blocks section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("BLOCKS");

            return sb.ToString();
        }

        public string DxfEndBlocks()
        {
            var sb = new StringBuilder();

            // end blocks section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        }

        public string DxfBeginEntities()
        {
            var sb = new StringBuilder();

            // begin entities section
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("ENTITIES");

            return sb.ToString();
        }

        public string DxfEndEntities()
        {
            var sb = new StringBuilder();

            // end entities section
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");

            return sb.ToString();
        }
                
        public string DxfEof()
        {
            var sb = new StringBuilder();

            // end if file
            sb.AppendLine("0");
            sb.AppendLine("EOF");

            return sb.ToString();
        }

        public string DxfLine(double x1, double y1, 
            double x2, double y2, 
            double offsetX, double offsetY, 
            string layer, int color, 
            double pageOffsetX, double pageOffsetY)
        {
            var sb = new StringBuilder();

            double _x1 = pageOffsetX > 0.0 ? pageOffsetX - x1 + offsetX : x1 + offsetX;
            double _y1 = pageOffsetY > 0.0 ? pageOffsetY - y1 + offsetY : y1 + offsetY;
            double _x2 = pageOffsetX > 0.0 ? pageOffsetX - x2 + offsetX : x2 + offsetX;
            double _y2 = pageOffsetY > 0.0 ? pageOffsetY - y2 + offsetY : y2 + offsetY;

            // line
            sb.AppendLine("0");
            sb.AppendLine("LINE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(layer.ToString());

            // color
            sb.AppendLine("62");
            sb.AppendLine(color.ToString());

            // start point: X
            sb.AppendLine("10");
            sb.AppendLine(_x1.ToString());

            // start point: Y
            sb.AppendLine("20");
            sb.AppendLine(_y1.ToString());

            // start point: Z (not used)
            sb.AppendLine("30");
            sb.AppendLine("0");

            // end point: X
            sb.AppendLine("11");
            sb.AppendLine(_x2.ToString());

            // end point: Y
            sb.AppendLine("21");
            sb.AppendLine(_y2.ToString());

            // end point: Z (not used)
            sb.AppendLine("31");
            sb.Append("0");

            return sb.ToString();
        }

        private string DxfTextGate(string text)
        {
            var sb = new StringBuilder();

            // begin text
            sb.AppendLine("0");
            sb.AppendLine("TEXT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerElements);

            // test string
            sb.AppendLine("1");
            sb.AppendLine(text);

            // test style
            sb.AppendLine("7");
            sb.AppendLine("TEXTGATE");

            // text height
            sb.AppendLine("40");
            sb.AppendLine("10");

            // first alignment  point: X
            sb.AppendLine("10");
            sb.AppendLine("15");

            // first alignment  point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // first alignment  point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // second alignment  point: X
            sb.AppendLine("11");
            sb.AppendLine("15");

            // second alignment  point: Y
            sb.AppendLine("21");
            sb.AppendLine("15");

            // second alignment  point: Z
            sb.AppendLine("31");
            sb.AppendLine("0");

            // horizontal justification
            sb.AppendLine("72");
            sb.AppendLine("1");

            // vertical text justification
            sb.AppendLine("73");
            sb.AppendLine("2");

            return sb.ToString();
        }

        private string DxfTextIO(string text, double x, double y)
        {
            var sb = new StringBuilder();

            // begin text
            sb.AppendLine("0");
            sb.AppendLine("TEXT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerIO);

            // test string
            sb.AppendLine("1");
            sb.AppendLine(text);

            // test style
            sb.AppendLine("7");
            sb.AppendLine("TEXTIO");

            // text height
            sb.AppendLine("40");
            sb.AppendLine("6");

            // first alignment  point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // first alignment  point: Y
            sb.AppendLine("20");
            sb.AppendLine(y.ToString());

            // first alignment  point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // second alignment  point: X
            sb.AppendLine("11");
            sb.AppendLine(x.ToString());

            // second alignment  point: Y
            sb.AppendLine("21");
            sb.AppendLine(y.ToString());

            // second alignment  point: Z
            sb.AppendLine("31");
            sb.AppendLine("0");

            // horizontal justification
            sb.AppendLine("72");
            sb.AppendLine("0");

            // vertical text justification
            sb.AppendLine("73");
            sb.AppendLine("2");

            return sb.ToString();
        }

        public string DxfBlockFrame()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: FRAME
            sb.AppendLine("2");
            sb.AppendLine("FRAME");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            double pageOffsetX = 0.0;
            double pageOffsetY = 891.0;

            // page frame

            // M 0,20 L 600,20 
            str = DxfLine(0.0, 20.0, 600.0, 20.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 600,770 L 0,770 
            str = DxfLine(600.0, 770.0, 0.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 0,770 L 0,0 
            str = DxfLine(0.0, 770.0, 0.0, 0.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 600,0 L 600,770
            str = DxfLine(600.0, 0.0, 600.0, 770.0, 330.0, -15.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 15,15 L 1245,15 
            str = DxfLine(15.0, 15.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1245,816 L 15,816
            str = DxfLine(1245.0, 816.0, 15.0, 816.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 15,876 L 1245,876
            str = DxfLine(15.0, 876.0, 1245.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1245,876 L 1245,15
            str = DxfLine(1245.0, 876.0, 1245.0, 15.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 15,15 L 15,876
            str = DxfLine(15.0, 15.0, 15.0, 876.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1,1 L 1259,1 
            str = DxfLine(1.0, 1.0, 1259.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1259,890 L 1,890 
            str = DxfLine(1259.0, 890.0, 1.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1,890 L 1,1 
            str = DxfLine(1.0, 890.0, 1.0, 1.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 1259,1 L 1259,890
            str = DxfLine(1259.0, 1.0, 1259.0, 890.0, 0.0, 0.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // inputs

            // M 30,0 L 30,750 
            str = DxfLine(30.0, 0.0, 30.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 240,750 L 240,0
            str = DxfLine(240.0, 750.0, 240.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 315,0 L 0,0
            str = DxfLine(315.0, 0.0, 0.0, 0.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 0,750 L 315,750
            str = DxfLine(0.0, 750.0, 315.0, 750.0, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);
         
            // M 0,30 L 315,30 
            // M 315,60 L 0,60 
            // M 0,90 L 315,90 
            // M 315,120 L 0,120 
            // M 0,150 L 315,150 
            // M 315,180 L 0,180 
            // M 0,210 L 315,210 
            // M 315,240 L 0,240
            // M 0,270 L 315,270 
            // M 315,300 L 0,300 
            // M 0,330 L 315,330 
            // M 315,360 L 0,360
            // M 0,390 L 315,390 
            // M 315,420 L 0,420 
            // M 0,450 L 315,450 
            // M 315,480 L 0,480 
            // M 0,510 L 315,510 
            // M 315,540 L 0,540
            // M 0,570 L 315,570 
            // M 315,600 L 0,600 
            // M 0,630 L 315,630 
            // M 315,660 L 0,660 
            // M 0,690 L 315,690
            // M 315,720 L 0,720
            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                str = DxfLine(0.0, y, 315.0, y, 15.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
                sb.AppendLine(str);
            }

            // outputs

            // M 210,0 L 210,750
            str = DxfLine(210.0, 0.0, 210.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 285,750 L 285,0
            str = DxfLine(285.0, 750.0, 285.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 315,0 L 0,0 
            str = DxfLine(315.0, 0.0, 0.0, 0.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 0,750 L 315,750
            str = DxfLine(0.0, 750.0, 315.0, 750.0, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
            sb.AppendLine(str);

            // M 0,30 L 315,30 
            // M 315,60 L 0,60 
            // M 0,90 L 315,90 
            // M 315,120 L 0,120 
            // M 0,150 L 315,150 
            // M 315,180 L 0,180
            // M 0,210 L 315,210 
            // M 315,240 L 0,240 
            // M 0,270 L 315,270 
            // M 315,300 L 0,300 
            // M 0,330 L 315,330 
            // M 315,360 L 0,360 
            // M 0,390 L 315,390 
            // M 315,420 L 0,420 
            // M 0,450 L 315,450 
            // M 315,480 L 0,480 
            // M 0,510 L 315,510 
            // M 315,540 L 0,540 
            // M 0,570 L 315,570 
            // M 315,600 L 0,600 
            // M 0,630 L 315,630 
            // M 315,660 L 0,660 
            // M 0,690 L 315,690 
            // M 315,720 L 0,720
            for (double y = 30.0; y <= 720.0; y += 30.0)
            {
                str = DxfLine(0.0, y, 315.0, y, 930.0, -35.0, LayerFrame, 0, pageOffsetX, pageOffsetY);
                sb.AppendLine(str);
            }




            // TODO: text





            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockTable()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: TABLE
            sb.AppendLine("2");
            sb.AppendLine("TABLE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");


            // TODO: lines


            // TODO: text


            // TODO: attributes




            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockGrid()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: GRID
            sb.AppendLine("2");
            sb.AppendLine("GRID");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");



            // TODO: lines



            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockInput()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: INPUT
            sb.AppendLine("2");
            sb.AppendLine("INPUT");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 285,0 
            str = DxfLine(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 285,30 L 0,30
            str = DxfLine(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,30  L 0,0 
            str = DxfLine(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 210,0 L 210,30 
            str = DxfLine(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 285,30 L 285,0
            str = DxfLine(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // tag text
            str = DxfTextIO("Designation", 3, 23.5);
            sb.Append(str);

            str = DxfTextIO("Description", 3, 7.5);
            sb.Append(str);

            str = DxfTextIO("Signal", 213, 23.5);
            sb.Append(str);

            str = DxfTextIO("Condition", 213, 7.5);
            sb.Append(str);


            // TODO: attributes


            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockOutput()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: OUTPUT
            sb.AppendLine("2");
            sb.AppendLine("OUTPUT");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 285,0 
            str = DxfLine(0.0, 0.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 285,30 L 0,30
            str = DxfLine(285.0, 30.0, 0.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,30  L 0,0 
            str = DxfLine(0.0, 30.0, 0.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 210,0 L 210,30 
            str = DxfLine(210.0, 0.0, 210.0, 30.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 285,30 L 285,0
            str = DxfLine(285.0, 30.0, 285.0, 0.0, 0.0, 0.0, LayerIO, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // tag text
            str = DxfTextIO("Designation", 3, 23.5);
            sb.Append(str);

            str = DxfTextIO("Description", 3, 7.5);
            sb.Append(str);

            str = DxfTextIO("Signal", 213, 23.5);
            sb.Append(str);

            str = DxfTextIO("Condition", 213, 7.5);
            sb.Append(str);

            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockAndGate()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: ANDGATE
            sb.AppendLine("2");
            sb.AppendLine("ANDGATE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 30,0
            str = DxfLine(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,30 L 30,30
            str = DxfLine(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,0 L 0,30
            str = DxfLine(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 30,0 L 30,30
            str = DxfLine(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // gate text
            str = DxfTextGate("&");
            sb.Append(str);

            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        public string DxfBlockOrGate()
        {
            string str = null;
            var sb = new StringBuilder();

            // begin block
            sb.AppendLine("0");
            sb.AppendLine("BLOCK");

            // name: ORGATE
            sb.AppendLine("2");
            sb.AppendLine("ORGATE");

            // block type
            sb.AppendLine("70");
            sb.AppendLine("0");

            // base point: X
            sb.AppendLine("10");
            sb.AppendLine("0");

            // base point: Y
            sb.AppendLine("20");
            sb.AppendLine("0");

            // base point: Z
            sb.AppendLine("30");
            sb.AppendLine("0");

            // M 0,0 L 30,0
            str = DxfLine(0.0, 0.0, 30.0, 0.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,30 L 30,30
            str = DxfLine(0.0, 30.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 0,0 L 0,30
            str = DxfLine(0.0, 0.0, 0.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // M 30,0 L 30,30
            str = DxfLine(30.0, 0.0, 30.0, 30.0, 0.0, 0.0, LayerElements, 0, 0.0, 30.0);
            sb.AppendLine(str);

            // gate text
            str = DxfTextGate("\\U+22651");
            sb.Append(str);

            // end block
            sb.AppendLine("0");
            sb.AppendLine("ENDBLK");

            return sb.ToString();
        }

        #endregion

        #region Insert

        public object InsertPageFrame(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("FRAME");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerFrame);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 891.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object InsertPageTable(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("TABLE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerTable);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 60.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("1");

            // attributes


            // TODO: attributes



            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object InsertPageGrid(double x, double y)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("GRID");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerTable);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 60.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            DxfString.AppendLine(sb.ToString());

            return null;
        }

        #endregion

        #region IDiagramCreator

        public object CreatePin(double x, double y, int id, bool snap)
        {
            return null;
        }

        public object CreateWire(double x1, double y1, double x2, double y2, bool startVisible, bool endVisible, bool startIsIO, bool endIsIO, int id)
        {
            string str = null;
            
            double startX = x1;
            double startY = y1;
            double endX = x2;
            double endY = y2;

            double zet = LineCalc.CalculateZet(startX, startY, endX, endY);
            double sizeX = LineCalc.CalculateSizeX(RadiusInvertedEllipse, ThicknessWire, zet);
            double sizeY = LineCalc.CalculateSizeY(RadiusInvertedEllipse, ThicknessWire, zet);

            bool shortenStart = ShortenStart;
            bool shortenEnd = ShortenEnd;
            bool isStartIO = startIsIO;
            bool isEndIO = endIsIO;

            // shorten start
            if (isStartIO == true && isEndIO == false && shortenStart == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    startX = endX - ShortenLineSize;
                }
            }

            // shorten end
            if (isStartIO == false && isEndIO == true && shortenEnd == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    endX = startX + ShortenLineSize;
                }
            }

            // get start and end ellipse position
            Point ellipseStartCenter = LineCalc.GetEllipseStartCenter(startX, startY, sizeX, sizeY, startVisible);
            Point ellipseEndCenter = LineCalc.GetEllipseEndCenter(endX, endY, sizeX, sizeY, endVisible);

            // get line position
            Point lineStart = LineCalc.GetLineStart(startX, startY, sizeX, sizeY, startVisible);
            Point lineEnd = LineCalc.GetLineEnd(endX, endY, sizeX, sizeY, endVisible);

            if (startVisible == true)
            {
                // TODO:
                // ELLIPSE: ellipseStartCenter, radius, radius
            }

            if (endVisible == true)
            {
                // TODO:
                // ELLIPSE: ellipseEndCenter, radius, radius
            }

            str = DxfLine(lineStart.X, lineStart.Y,
                lineEnd.X, lineEnd.Y,
                0.0, 0.0,
                LayerWires,
                0,
                0.0, 891.0);

            DxfString.AppendLine(str);

            return null;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("INPUT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerIO);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("1");

            // attributes


            // TODO: attributes



            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("OUTPUT");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerIO);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("0");

            // attributes


            // TODO: attributes


            DxfString.AppendLine(sb.ToString());


            return null;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("ANDGATE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerElements);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("0");

            // attributes


            // TODO: attributes


            DxfString.AppendLine(sb.ToString());


            return null;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var sb = new StringBuilder();

            // insert block
            sb.AppendLine("0");
            sb.AppendLine("INSERT");

            // block name
            sb.AppendLine("2");
            sb.AppendLine("ORGATE");

            // layer
            sb.AppendLine("8");
            sb.AppendLine(LayerElements);

            // insertion point: X
            sb.AppendLine("10");
            sb.AppendLine(x.ToString());

            // insertion point: Y
            sb.AppendLine("20");
            sb.AppendLine((PageHeight - 30.0 - y).ToString());

            // insertion point: Z (not used)
            sb.AppendLine("30");
            sb.Append("0");

            // attributes follow: 0 - no, 1 - yes
            //sb.AppendLine("66");
            //sb.AppendLine("0");

            // attributes


            // TODO: attributes


            DxfString.AppendLine(sb.ToString());

            return null;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            return null;
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {

        }

        #endregion

        #region Generate Dxf

        public string GenerateDxfFromModel(string model)
        {
            var parser = new DiagramParser();

            var parseOptions = new ParseOptions()
            {
                OffsetX = 0.0,
                OffsetY = 0.0,
                AppendIds = false,
                UpdateIds = false,
                Select = false,
                CreateElements = true,
                Counter = null,
                Properties = null
            };

            DxfString = new StringBuilder();

            // header
            string header = DxfHeader("Diagram");
            DxfString.Append(header);

            // tables
            string tables = DxfTables();
            DxfString.Append(tables);

            // begin blocks
            string blocksBegin = DxfBeginBlocks();
            DxfString.Append(blocksBegin);

            // blocks
            string blockFrame = DxfBlockFrame();
            DxfString.Append(blockFrame);

            string blockTable = DxfBlockTable();
            DxfString.Append(blockTable);

            string blockGrid = DxfBlockGrid();
            DxfString.Append(blockGrid);

            string blockInput = DxfBlockInput();
            DxfString.Append(blockInput);

            string blockOutput = DxfBlockOutput();
            DxfString.Append(blockOutput);

            string blockAndGate = DxfBlockAndGate();
            DxfString.Append(blockAndGate);

            string blockOrGate = DxfBlockOrGate();
            DxfString.Append(blockOrGate);

            // end blocks
            string blocksEnd = DxfEndBlocks();
            DxfString.Append(blocksEnd);

            // begin entities
            string entitiesBegin = DxfBeginEntities();
            DxfString.Append(entitiesBegin);

            // page frame
            InsertPageFrame(0.0, 0.0);

            // page grid
            InsertPageGrid(0.0, 0.0);

            // page table
            InsertPageTable(0.0, 0.0);

            // entities
            var solution = parser.Parse(model, this, parseOptions);

            // end entities
            string entitiesEnd = DxfEndEntities();
            DxfString.Append(entitiesEnd);

            // eof
            string eof = DxfEof();
            DxfString.Append(eof);

            return DxfString.ToString();
        }

        #endregion
    }
}
