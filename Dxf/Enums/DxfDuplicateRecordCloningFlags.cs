// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Enums
{
    #region DxfDuplicateRecordCloningFlags

    public enum DxfDuplicateRecordCloningFlags : int
    {
        NotApplicable = 0,
        KeepExisting = 1,
        UseClone = 2,
        XrefPrefixName = 3,
        PrefixName = 4,
        UnmangleName = 5
    }

    #endregion
}
