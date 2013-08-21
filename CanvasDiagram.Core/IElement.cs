// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

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
