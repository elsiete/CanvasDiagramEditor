// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
