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
    #region IMargin

    public interface IMargin
    {
        double Bottom { get; set; }
        double Left { get; set; }
        double Right { get; set; }
        double Top { get; set; }
    }

    #endregion
}
