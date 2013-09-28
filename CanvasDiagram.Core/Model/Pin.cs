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
    #region Pin

    public class Pin
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Pin(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    #endregion
}
