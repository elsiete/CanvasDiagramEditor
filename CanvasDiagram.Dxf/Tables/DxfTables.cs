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

namespace CanvasDiagram.Dxf.Tables
{
    #region DxfTables

    public class DxfTables : DxfObject<DxfTables>
    {
        public DxfTables(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfTables Begin()
        {
            Add(0, "SECTION");
            Add(2, "TABLES");
            return this;
        }

        public DxfTables Add<T>(T table)
        {
            Append(table.ToString());
            return this;
        }

        public DxfTables Add<T>(IEnumerable<T> tables)
        {
            foreach (var table in tables)
            {
                Add(table);
            }

            return this;
        }

        public DxfTables AddDimstyleTable(IEnumerable<DxfDimstyle> dimstyles, int id)
        {
            BeginDimstyles(dimstyles.Count(), id);
            Add(dimstyles);
            EndDimstyles();
            return this;
        }

        public DxfTables AddAppidTable(IEnumerable<DxfAppid> appids, int id)
        {
            BeginAppids(appids.Count(), id);
            Add(appids);
            EndAppids();
            return this;
        }

        public DxfTables AddBlockRecordTable(IEnumerable<DxfBlockRecord> records, int id)
        {
            BeginBlockRecords(records.Count(), id);
            Add(records);
            EndBlockRecords();
            return this;
        }

        public DxfTables AddLtypeTable(IEnumerable<DxfLtype> ltypes, int id)
        {
            BeginLtypes(ltypes.Count(), id);
            Add(ltypes);
            EndLtypes();
            return this;
        }

        public DxfTables AddLayerTable(IEnumerable<DxfLayer> layers, int id)
        {
            BeginLayers(layers.Count(), id);
            Add(layers);
            EndLayers();
            return this;
        }

        public DxfTables AddStyleTable(IEnumerable<DxfStyle> styles, int id)
        {
            BeginStyles(styles.Count(), id);
            Add(styles);
            EndStyles();
            return this;
        }

        public DxfTables AddUcsTable(IEnumerable<DxfUcs> ucss, int id)
        {
            BeginUcss(ucss.Count(), id);
            Add(ucss);
            EndUcss();
            return this;
        }

        public DxfTables AddViewTable(IEnumerable<DxfView> views, int id)
        {
            BeginViews(views.Count(), id);
            Add(views);
            EndViews();
            return this;
        }

        public DxfTables AddVportTable(IEnumerable<DxfVport> vports, int id)
        {
            BeginVports(vports.Count(), id);
            Add(vports);
            EndVports();
            return this;
        }

        public DxfTables End()
        {
            Add(0, "ENDSEC");
            return this;
        }

        private void BeginDimstyles(int count, int id)
        {
            BeginTable("DIMSTYLE", count, id);

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass(SubclassMarker.DimStyleTable);
                Add(71, count);
            }
        }

        private void EndDimstyles()
        {
            EndTable();
        }

        private void BeginAppids(int count, int id)
        {
            BeginTable("APPID", count, id);
        }

        private void EndAppids()
        {
            EndTable();
        }

        private void BeginBlockRecords(int count, int id)
        {
            BeginTable("BLOCK_RECORD", count, id);
        }

        private void EndBlockRecords()
        {
            EndTable();
        }

        private void BeginLtypes(int count, int id)
        {
            BeginTable("LTYPE", count, id);
        }

        private void EndLtypes()
        {
            EndTable();
        }

        private void BeginLayers(int count, int id)
        {
            BeginTable("LAYER", count, id);
        }

        private void EndLayers()
        {
            EndTable();
        }

        private void BeginStyles(int count, int id)
        {
            BeginTable("STYLE", count, id);
        }

        private void EndStyles()
        {
            EndTable();
        }

        private void BeginUcss(int count, int id)
        {
            BeginTable("UCS", count, id);
        }

        private void EndUcss()
        {
            EndTable();
        }

        private void BeginViews(int count, int id)
        {
            BeginTable("VIEW", count, id);
        }

        private void EndViews()
        {
            EndTable();
        }

        private void BeginVports(int count, int id)
        {
            BeginTable("VPORT", count, id);
        }

        private void EndVports()
        {
            EndTable();
        }

        private void BeginTable(string name, int count, int id)
        {
            Add(0, "TABLE");
            Add(2, name);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTable);
            }

            Add(70, count);
        }

        private void EndTable()
        {
            Add(0, "ENDTAB");
        }
    }

    #endregion
}
