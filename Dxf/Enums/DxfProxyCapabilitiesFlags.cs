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
    #region DxfProxyCapabilitiesFlags

    public enum DxfProxyCapabilitiesFlags : int
    {
        NoOperationsAllowed = 0,
        EraseAllowed = 1,
        TransformAllowed = 2,
        ColorChangeAllowed = 4,
        LayerChangeAllowed = 8,
        LinetypeChangeAllowed = 16,
        LinetypeScaleChangeAllowed = 32,
        VisibilityChangeAllowed = 64,
        AllOperationsExceptCloningAllowed = 127,
        CloningAllowed = 128,
        AllOperationsAllowed  = 255,
        R13FormatProxy = 32768
    }

    #endregion
}
