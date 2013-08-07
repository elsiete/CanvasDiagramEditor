// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor.Core
{
    #region Tag

    public class Tag
    {
        #region Properties

        public int Id { get; set; }

        public string Designation { get; set; }
        public string Signal { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }

        #endregion
    } 

    #endregion
}
