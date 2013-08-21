// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region ModelConstants

    public static class ModelConstants
    {
        #region Model String Constants

        public const char ArgumentSeparator = ';';
        public const string PrefixRoot = "+";
        public const string PrefixChild = "-";

        public const char TagNameSeparator = '|';

        public const string TagHeaderSolution = "Solution";
        public const string TagHeaderProject = "Project";
        public const string TagHeaderDiagram = "Diagram";

        public const string TagElementPin = "Pin";
        public const string TagElementWire = "Wire";
        public const string TagElementInput = "Input";
        public const string TagElementOutput = "Output";
        public const string TagElementAndGate = "AndGate";
        public const string TagElementOrGate = "OrGate";

        public const string WireStartType = "Start";
        public const string WireEndType = "End";

        public const string PinStandalone = "MiddlePin";
        public const string PinLeft = "LeftPin";
        public const string PinRight = "RightPin";
        public const string PinTop = "TopPin";
        public const string PinBottom = "BottomPin";
  
        #endregion
    }

    #endregion
}
