// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.WPF
{
    #region DipUtil

    public static class DipUtil
    {
        #region Dips Calculator Util

        public static double MmToDip(double mm)
        {
            return CmToDip(mm / 10.0);
        }

        public static double CmToDip(double cm)
        {
            return (cm * 96.0 / 2.54);
        }

        public static double InchToDip(double inch)
        {
            return (inch * 96.0);
        }

        public static double PtToDip(double pt)
        {
            return (pt * 96.0 / 72.0);
        }

        public static double DipToCm(double dip)
        {
            return (dip * 2.54 / 96.0);
        }

        public static double DipToMm(double dip)
        {
            return DipToCm(dip) * 10.0;
        }

        #endregion
    }

    #endregion
}
