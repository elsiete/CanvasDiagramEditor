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
    #region IDiagramParser

    public interface IDiagramParser
    {
        TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options);
    } 

    #endregion
}
