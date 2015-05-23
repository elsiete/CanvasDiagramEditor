// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Enums
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
