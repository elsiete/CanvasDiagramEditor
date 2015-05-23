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
