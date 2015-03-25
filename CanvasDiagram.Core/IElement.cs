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
    #region IElement

    public interface IElement : IData, IUid, ITag, ISelected
    {
        double GetX();
        double GetY();
        void SetX(double x);
        void SetY(double y);

        object GetParent();
    }

    #endregion
}
