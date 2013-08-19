// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region Region

using CanvasDiagramEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#endregion

namespace CanvasDiagramEditor.Util
{
    #region WindowsClipboard

    public class WindowsClipboard : IClipboard
    {
        public bool ContainsText()
        {
            return Clipboard.ContainsText();
        }

        public string GetText()
        {
            return Clipboard.GetText();
        }

        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    } 

    #endregion
}
