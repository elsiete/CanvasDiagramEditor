// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Core
{
    #region DxfObject

    public abstract class DxfObject
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

        public virtual DxfObject Add(string code, string data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data);
            return this;
        }

        public virtual DxfObject Add(string code, int data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString());
            return this;
        }

        public virtual DxfObject Add(string code, double data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this;
        }

        protected virtual DxfObject Append(string str)
        {
            this.sb.Append(str);
            return this;
        }
    } 

    #endregion
}
