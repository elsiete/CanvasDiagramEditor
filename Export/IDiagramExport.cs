#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagramEditor.Export
{
    #region IDiagramExport

    public interface IDiagramExport
    {
        void CreateDocument(string fileName, IEnumerable<string> diagrams);
    }

    #endregion
}
