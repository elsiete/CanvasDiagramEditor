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
    #region IUid

    public interface IUid
    {
        string GetUid();
        void SetUid(string uid);
    }

    #endregion
}
