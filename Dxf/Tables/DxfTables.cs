// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Tables
{
    #region DxfTables

    public class DxfTables : DxfObject
    {
        public DxfTables()
            : base()
        {
        }

        public DxfTables Begin()
        {
            Add("0", "SECTION");
            Add("2", "TABLES");
            return this;
        }

        public DxfTables Add(DxfTable table)
        {
            Append(table.ToString());
            return this;
        }

        public DxfTables AddDimstyleTable(IEnumerable<DxfDimstyle> dimstyles)
        {
            BeginDimstyles(dimstyles.Count());
            Add(dimstyles.Cast<DxfObject>());
            EndDimstyles();
            return this;
        }

        public DxfTables AddAppidTable(IEnumerable<DxfAppid> appids)
        {
            BeginAppids(appids.Count());
            Add(appids.Cast<DxfObject>());
            EndAppids();
            return this;
        }

        public DxfTables AddLtypeTable(IEnumerable<DxfLtype> ltypes)
        {
            BeginLtypes(ltypes.Count());
            Add(ltypes.Cast<DxfObject>());
            EndLtypes();
            return this;
        }

        public DxfTables AddLayerTable(IEnumerable<DxfLayer> layers)
        {
            BeginLayers(layers.Count());
            Add(layers.Cast<DxfObject>());
            EndLayers();
            return this;
        }

        public DxfTables AddStyleTable(IEnumerable<DxfStyle> styles)
        {
            BeginStyles(styles.Count());
            Add(styles.Cast<DxfObject>());
            EndStyles();
            return this;
        }

        public DxfTables AddUcsTable(IEnumerable<DxfUcs> ucss)
        {
            BeginUcss(ucss.Count());
            Add(ucss.Cast<DxfObject>());
            EndUcss();
            return this;
        }

        public DxfTables AddViewTable(IEnumerable<DxfView> views)
        {
            BeginViews(views.Count());
            Add(views.Cast<DxfObject>());
            EndViews();
            return this;
        }

        public DxfTables AddVportTable(IEnumerable<DxfVport> vports)
        {
            BeginVports(vports.Count());
            Add(vports.Cast<DxfObject>());
            EndVports();
            return this;
        }

        public DxfTables End()
        {
            Add("0", "ENDSEC");
            return this;
        }

        private void BeginDimstyles(int count)
        {
            BeginTable("DIMSTYLE", count);
        }

        private void EndDimstyles()
        {
            EndTable();
        }

        private void BeginAppids(int count)
        {
            BeginTable("APPID", count);
        }

        private void EndAppids()
        {
            EndTable();
        }

        private void BeginLtypes(int count)
        {
            BeginTable("LTYPE", count);
        }

        private void EndLtypes()
        {
            EndTable();
        }

        private void BeginLayers(int count)
        {
            BeginTable("LAYER", count);
        }

        private void EndLayers()
        {
            EndTable();
        }

        private void BeginStyles(int count)
        {
            BeginTable("STYLE", count);
        }

        private void EndStyles()
        {
            EndTable();
        }

        private void BeginUcss(int count)
        {
            BeginTable("UCS", count);
        }

        private void EndUcss()
        {
            EndTable();
        }

        private void BeginViews(int count)
        {
            BeginTable("VIEW", count);
        }

        private void EndViews()
        {
            EndTable();
        }

        private void BeginVports(int count)
        {
            BeginTable("VPORT", count);
        }

        private void EndVports()
        {
            EndTable();
        }

        private void Add(DxfObject obj)
        {
            Append(obj.ToString());
        }

        private void Add(IEnumerable<DxfObject> objects)
        {
            foreach (var obj in objects)
            {
                Add(obj);
            }
        }

        private void BeginTable(string name, int count)
        {
            Add("0", "TABLE");
            Add("2", name);
            Add("70", count);
        }

        private void EndTable()
        {
            Add("0", "ENDTAB");
        }
    }

    #endregion
}
