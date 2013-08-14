﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagramEditor.Util
{
    #region PathUtil

    public static class PathUtil
    {
        #region Path

        public static string MakeRelative(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath, UriKind.RelativeOrAbsolute);
            Uri toUri = new Uri(toPath, UriKind.RelativeOrAbsolute);

            if (fromUri.IsAbsoluteUri == true)
            {
                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            }
            else
            {
                return null;
            }
        }

        public static string GetRelativeFileName(string fromPath, string toPath)
        {
            string relativePath = MakeRelative(toPath, fromPath);
            string onlyFileName = System.IO.Path.GetFileName(toPath);

            if (relativePath != null)
            {
                toPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(relativePath),
                    onlyFileName);
            }

            return toPath;
        }

        #endregion
    }

    #endregion
}
