// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Dxf.Util
{
    #region DxfBuilder

    public class DxfBuilder
    {
    	private StringBuilder sb = null;

        #region Constructor

    	public DxfBuilder()
    	{
    		this.sb = new StringBuilder();
    	}

        #endregion

        #region Add

        public void AddComment(string comment)
        {
            this.sb.AppendLine("999");
            this.sb.AppendLine(comment);
        }

        public void Add(string code, string data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data);
        }

        public void Add(string code, double data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToDxfString());
        }

        public void Add(string code, int data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString());
        }

        #endregion

        #region Build

        public string Build()
        {
        	return this.sb.ToString();
        }

        #endregion
    }

    #endregion
}
