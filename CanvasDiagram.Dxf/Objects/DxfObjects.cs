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

namespace CanvasDiagram.Dxf.Objects
{
    #region DxfObjects

    public class DxfObjects : DxfObject<DxfObjects>
    {
        public DxfObjects(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfObjects Begin()
        {
            Add(0, "SECTION");
            Add(2, "OBJECTS");
            return this;
        }

        public DxfObjects Add<T>(T obj)
        {
            Append(obj.ToString());
            return this;
        }

        public DxfObjects Add<T>(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Add(obj);
            }

            return this;
        }

        public DxfObjects End()
        {
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}
