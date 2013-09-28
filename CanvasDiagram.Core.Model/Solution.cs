// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Core.Model
{
    #region Solution

    public class Solution
    {
        public string Model { get; set; }
        public List<string> Models { get; set; }

        public Solution(string model, List<string> models)
        {
            Model = model;
            Models = models;
        }
    }

    #endregion
}
