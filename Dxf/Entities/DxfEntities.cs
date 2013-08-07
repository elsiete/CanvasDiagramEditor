﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfEntities

    public class DxfEntities : DxfObject<DxfEntities>
    {
        public DxfEntities(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfEntities Begin()
        {
            Add("0", "SECTION");
            Add("2", "ENTITIES");
            return this;
        }

        public DxfEntities Add<T>(T entity)
        {
            Append(entity.ToString());
            return this;
        }

        public DxfEntities Add<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }

            return this;
        }

        public DxfEntities End()
        {
            Add("0", "ENDSEC");
            return this;
        }
    }

    #endregion
}
