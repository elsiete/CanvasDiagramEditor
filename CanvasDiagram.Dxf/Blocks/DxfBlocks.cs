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

namespace CanvasDiagram.Dxf.Blocks
{
    #region DxfBlocks

    public class DxfBlocks : DxfObject<DxfBlocks>
    {
        public DxfBlocks(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlocks Begin()
        {
            Add(0, "SECTION");
            Add(2, "BLOCKS");
            return this;
        }

        public DxfBlocks Add(DxfBlock block)
        {
            Append(block.ToString());
            return this;
        }

        public DxfBlocks Add(IEnumerable<DxfBlock> blocks)
        {
            foreach (var block in blocks)
                Add(block);

            return this;
        }

        public DxfBlocks End()
        {
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}
