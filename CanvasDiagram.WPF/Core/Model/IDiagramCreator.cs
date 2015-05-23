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
    #region IDiagramCreator

    public interface IDiagramCreator
    {
        void SetCanvas(ICanvas canvas);
        ICanvas GetCanvas();

        object CreateElement(string type, object[] data, double x, double y, bool snap);
        object CreateDiagram(DiagramProperties properties);
        object CreateGrid(double originX, double originY, double width, double height, double size);

        void UpdateConnections(IDictionary<string, Child> dict);
        void UpdateCounter(IdCounter original, IdCounter counter);
        void AppendIds(IEnumerable<object> elements);
        void InsertElements(IEnumerable<object> elements, bool select, double offsetX, double offsetY);
    } 

    #endregion
}
