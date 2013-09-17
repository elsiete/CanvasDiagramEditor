// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region Insert

    public static class Insert
    {
        #region Pin

        public static IElement Pin(ICanvas canvas, IPoint point, IDiagramCreator creator, bool snap)
        {
            var thumb = creator.CreateElement(Constants.TagElementPin,
                new object[] { canvas.GetCounter().Next() },
                point.X, point.Y, snap) as IThumb;

            canvas.Add(thumb);

            return thumb;
        }

        #endregion

        #region Input

        public static IElement Input(ICanvas canvas, IPoint point, IDiagramCreator creator, bool snap)
        {
            var thumb = creator.CreateElement(Constants.TagElementInput,
                new object[] { canvas.GetCounter().Next(), -1 },
                point.X, point.Y, snap) as IThumb;

            canvas.Add(thumb);

            return thumb;
        }

        #endregion

        #region Output

        public static IElement Output(ICanvas canvas, IPoint point, IDiagramCreator creator, bool snap)
        {
            var thumb = creator.CreateElement(Constants.TagElementOutput,
                new object[] { canvas.GetCounter().Next(), -1 },
                point.X, point.Y, snap) as IThumb;

            canvas.Add(thumb);

            return thumb;
        }

        #endregion

        #region AndGate

        public static IElement AndGate(ICanvas canvas, IPoint point, IDiagramCreator creator, bool snap)
        {
            var thumb = creator.CreateElement(Constants.TagElementAndGate,
                new object[] { canvas.GetCounter().Next() },
                point.X, point.Y, snap) as IThumb;

            canvas.Add(thumb);

            return thumb;
        }

        #endregion

        #region OrGate

        public static IElement OrGate(ICanvas canvas, IPoint point, IDiagramCreator creator, bool snap)
        {
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(Constants.TagElementOrGate,
                new object[] { counter.Next() },
                point.X, point.Y, snap) as IThumb;

            canvas.Add(thumb);

            return thumb;
        }

        #endregion
    }

    #endregion
}
