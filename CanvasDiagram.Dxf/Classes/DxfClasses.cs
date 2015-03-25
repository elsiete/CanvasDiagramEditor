// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
    #region DxfClasses

    public class DxfClasses : DxfObject<DxfClasses>
    {
        public DxfClasses(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfClasses Begin()
        {
            Add(0, "SECTION");
            Add(2, "CLASSES");
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
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}
