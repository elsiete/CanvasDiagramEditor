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
    #region ITag

    public interface ITag
    {
        object GetTag();
        void SetTag(object tag);
    }

    #endregion
}
