// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Core
{
    #region ILine

    public interface ILine : IElement
    {
        bool GetStartVisible();
        void SetStartVisible(bool visible);

        bool GetEndVisible();
        void SetEndVisible(bool visible);

        bool GetStartIO();
        void SetStartIO(bool flag);

        bool GetEndIO();
        void SetEndIO(bool flag);

        double GetX1();
        void SetX1(double x1);

        double GetY1();
        void SetY1(double y1);

        double GetX2();
        void SetX2(double x2);

        double GetY2();
        void SetY2(double y2);

        IMargin GetMargin();
        void SetMargin(IMargin margin);
    }

    #endregion
}
