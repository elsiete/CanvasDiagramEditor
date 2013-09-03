// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Core
{
    #region IdCounter

    public class IdCounter
    {
        private int count = 0;
        public int Count { get { return count; } }

        public void Reset()
        {
            count = 0;
        }

        public int Next()
        {
            return count++;
        }

        public void Set(int count)
        {
            this.count = count;
        }
    } 

    #endregion
}
