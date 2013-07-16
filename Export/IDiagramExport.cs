
namespace CanvasDiagramEditor.Export
{
    #region References

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text; 

    #endregion

    #region IDiagramExport

    public interface IDiagramExport
    {
        void CreateDocument(string fileName, IEnumerable<string> diagrams);
    }

    #endregion
}
