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
    #region ICanvas

    public interface ICanvas : IData, IUid, ITag, ISelected
    {
        IEnumerable<IElement> GetElements();

        void Add(IElement element);
        void Remove(IElement element);
        void Clear();

        double GetWidth();
        void SetWidth(double width);
        double GetHeight();
        void SetHeight(double height);

        List<object> GetTags();
        void SetTags(List<object> tags);

        IEnumerable<IElement> HitTest(IPoint point, double radius);
        IEnumerable<IElement> HitTest(IRect rect);

        IdCounter GetCounter();
        void SetCounter(IdCounter counter);

        DiagramProperties GetProperties();
        void SetProperties(DiagramProperties properties);
    }

    #endregion
}
