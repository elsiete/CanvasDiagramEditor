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
    #region DxfDictionary

    public class DxfDictionary : DxfObject<DxfDictionary>
    {
        public string OwnerDictionaryHandle { get; set; }
        public bool HardOwnerFlag { get; set; }
        public DxfDuplicateRecordCloningFlags DuplicateRecordCloningFlags { get; set; }
        public Dictionary<string, string> Entries { get; set; }

        public DxfDictionary(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfDictionary Defaults()
        {
            OwnerDictionaryHandle = "0";
            HardOwnerFlag = false;
            DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting;
            Entries = null;
            
            return this;
        }

        public DxfDictionary Create()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Add(0, CodeName.Dictionary);

                Handle(Id);
                Add(330, OwnerDictionaryHandle);
                Subclass(SubclassMarker.Dictionary);
                Add(280, HardOwnerFlag);
                Add(281, (int)DuplicateRecordCloningFlags);

                if (Entries != null)
                {
                    foreach(var entry in Entries)
                    {
                        var entryName = entry.Value;
                        var entryObjectHandle = entry.Key;

                        Add(3, entryName);
                        Add(350, entryObjectHandle);
                    }
                }
            }

            return this;
        }
    }

    #endregion
}
