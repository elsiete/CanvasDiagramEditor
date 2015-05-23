// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Core
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
