﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Entities
{
    #region DxfEntity

    public abstract class DxfEntity
    {
        protected StringBuilder sb = new StringBuilder();

        public override string ToString()
        {
            return this.Build();
        }

        public virtual string Build()
        {
            return this.sb.ToString();
        }

        public virtual DxfEntity Add(string code, string data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data);
            return this;
        }

        public virtual DxfEntity Add(string code, int data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString());
            return this;
        }

        public virtual DxfEntity Add(string code, double data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this;
        }
    } 

    #endregion
}
