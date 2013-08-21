// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Dxf.Core;
using CanvasDiagram.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Dxf.Classes
{
    #region DxfClass

    public class DxfClass : DxfObject<DxfClass>
    {
        public string DxfClassName { get; set; }
        public string CppClassName { get; set; }
        public DxfProxyCapabilitiesFlags ProxyCapabilitiesFlags { get; set; }
        public bool WasAProxyFlag { get; set; }
        public bool IsAnEntityFlag { get; set; }

        public DxfClass(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfClass Defaults()
        {
            DxfClassName = string.Empty;
            CppClassName = string.Empty;
            ProxyCapabilitiesFlags = DxfProxyCapabilitiesFlags.NoOperationsAllowed;
            WasAProxyFlag = false;
            IsAnEntityFlag = false;
            return this;
        }

        public DxfClass Create()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Add(0, CodeName.Class);
                Add(1, DxfClassName);
                Add(2, CppClassName);
                Add(90, (int)ProxyCapabilitiesFlags);
                Add(280, WasAProxyFlag);
                Add(281, IsAnEntityFlag);
            }

            return this;
        }
    }

    #endregion
}
