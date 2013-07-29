﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Enums
{
    #region DxfTextGenerationFlags

    // Group code: 71, default = 0
    public enum DxfTextGenerationFlags : int
    {
        Default = 0,
        MirroredInX = 2,
        MirroredInY = 4
    }

    #endregion
}