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
    #region IDiagramParser

    public interface IDiagramParser
    {
        TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options);
    } 

    #endregion
}
