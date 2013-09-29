// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#endregion

namespace CanvasDiagram.WPF
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
