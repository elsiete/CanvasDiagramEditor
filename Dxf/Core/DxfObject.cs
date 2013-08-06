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
    #region DxfAcadVer

    // DXF support status: 
    // AC1006 - supported
    // AC1009 - supported
    // AC1012 - not supported
    // AC1014 - not supported
    // AC1015 - ?

    // AutoCAD drawing database version number: 
    // AC1006 = R10
    // AC1009 = R11 and R12, 
    // AC1012 = R13
    // AC1014 = R14
    // AC1015 = AutoCAD 2000

    public enum DxfAcadVer : int
    {
        AC1006 = 0, // R10
        AC1009 = 1, // R11 and R12
        AC1012 = 2, // R13
        AC1014 = 3, // R14
        AC1015 = 4, // AutoCAD 2000
        Default = AC1015
    }

    #endregion

    #region DxfObject

    public abstract class DxfObject<T> 
        where T : DxfObject<T>
    {
        public virtual DxfAcadVer Version { get; private set; }
        public virtual int Id { get; private set; }

        public DxfObject(DxfAcadVer version, int id)
        {
            this.Version = version;
            this.Id = id;
        }

        protected StringBuilder sb = new StringBuilder();

        public override string ToString()
        {
            return this.Build();
        }

        public virtual void Reset()
        {
            this.sb.Length = 0;
        }

        public virtual string Build()
        {
            return this.sb.ToString();
        }

        public virtual T Add(string code, string data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data);
            return this as T;
        }

        public virtual T Add(string code, bool data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data == true ? 1.ToString() : 0.ToString());
            return this as T;
        }

        public virtual T Add(string code, int data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString());
            return this as T;
        }

        public virtual T Add(string code, double data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this as T;
        }

        public virtual T Add(int code, string data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data);
            return this as T;
        }

        public virtual T Add(int code, bool data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data == true ? 1.ToString() : 0.ToString());
            return this as T;
        }

        public virtual T Add(int code, int data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data.ToString());
            return this as T;
        }

        public virtual T Add(int code, double data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this as T;
        }

        protected virtual T Append(string str)
        {
            this.sb.Append(str);
            return this as T;
        }

        public virtual T Comment(string comment)
        {
            Add("999", comment);
            return this as T;
        }

        public virtual T Handle(string handle)
        {
            Add("5", handle);
            return this as T;
        }

        public virtual T Handle(int handle)
        {
            Add("5", handle.ToString("X"));
            return this as T;
        }

        public virtual T Subclass(string subclass)
        {
            Add("100", subclass);
            return this as T;
        }
    } 

    #endregion
}
