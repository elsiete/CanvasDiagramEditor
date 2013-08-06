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

namespace CanvasDiagramEditor.Dxf.Tables
{
    #region DxfAppid

    public class DxfAppid : DxfObject<DxfAppid>
    {
        public DxfAppid(DxfAcadVer version, int id)
            : base(version, id  )
        {
            Add("0", "APPID");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass("AcDbSymbolTableRecord");
                Subclass("AcDbRegAppTableRecord");
            }
        }

        public DxfAppid Application(string name)
        {
            Add("2", name);
            return this;
        }

        public DxfAppid StandardFlags(DxfAppidStandardFlags flags)
        {
            Add("70", (int)flags);
            return this;
        }
    }

    #endregion
}
