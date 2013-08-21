// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Core
{
    #region IdCounter

    public class IdCounter
    {
        #region Constructor

        public IdCounter()
        {
            ResetAll();
        }

        #endregion

        #region Reset

        public void ResetDiagram()
        {
            PinCount = 0;
            WireCount = 0;
            InputCount = 0;
            OutputCount = 0;
            AndGateCount = 0;
            OrGateCount = 0;
        }

        public void ResetAll()
        {
            SolutionCount = 0;
            ProjectCount = 0;
            DiagramCount = 0;

            ResetDiagram();
        }

        #endregion

        #region Properties

        public int SolutionCount { get; set; }
        public int ProjectCount { get; set; }
        public int DiagramCount { get; set; }
        public int PinCount { get; set; }
        public int WireCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public int AndGateCount { get; set; }
        public int OrGateCount { get; set; }

        #endregion
    } 

    #endregion
}
