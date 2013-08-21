// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Util
{
    #region StringUtil

    public static class StringUtil
    {
        public static bool Compare(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool StartsWith(string str, string value)
        {
            return str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    #endregion
}
