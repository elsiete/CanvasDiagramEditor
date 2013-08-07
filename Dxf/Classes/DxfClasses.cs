// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Classes
{
    #region DxfClasses

    public class DxfClasses : DxfObject<DxfClasses>
    {
        public DxfClasses(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfClasses Begin()
        {
            Add("0", "SECTION");
            Add("2", "CLASSES");
            return this;
        }

        public DxfClasses Add(DxfClass cls)
        {
            Append(cls.ToString());
            return this;
        }

        public DxfClasses Add(IEnumerable<DxfClass> classes)
        {
            foreach (var cls in classes)
            {
                Add(cls);
            }

            return this;
        }

        public DxfClasses End()
        {
            Add("0", "ENDSEC");
            return this;
        }
    }

    #endregion
}
