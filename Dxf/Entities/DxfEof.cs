// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfEof

    public class DxfEof : DxfEntity
    {
        public DxfEof()
            : base()
        {
            Add("0", "EOF");
        }
    } 

    #endregion
}
