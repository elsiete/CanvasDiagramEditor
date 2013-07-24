#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes; 

#endregion

namespace CanvasDiagramEditor.Editor
{
    #region DiagramEditorOptions

    public class DiagramEditorOptions
    {
        #region Fields

        public DiagramProperties CurrentProperties = new DiagramProperties();

        public ResourceDictionary CurrentResources = null;

        public string TagFileName = null;
        public List<object> Tags = null;

        public TreeView CurrentTree = null;
        public Canvas CurrentCanvas = null;
        public Path CurrentPathGrid = null;

        public bool EnableHistory = true;

        public LineEx CurrentLine = null;
        public FrameworkElement CurrentRoot = null;

        public IdCounter Counter = new IdCounter();

        public Point RightClick;

        public bool EnableInsertLast = false;
        public string LastInsert = ModelConstants.TagElementInput;

        public bool EnablePageGrid = true;
        public bool EnablePageTemplate = true;

        public double DefaultGridSize = 30;

        public bool EnableSnap = true;
        public bool SnapOnRelease = false;

        public bool MoveAllSelected = false;

        public double HitTestRadiusX = 6.0;
        public double HitTestRadiusY = 6.0;

        public bool SkipContextMenu = false;
        public bool SkipLeftClick = false;

        public Point PanStart;
        public double PreviousScrollOffsetX = -1.0;
        public double PreviousScrollOffsetY = -1.0;

        public double DefaultStrokeThickness = 1.0;

        public double ZoomInFactor = 0.1;
        public double ZoomOutFactor = 0.1;

        public double ZoomLogBase = 1.8;
        public double ZoomExpFactor = 1.3;

        public Point ZoomPoint;

        public double ReversePanDirection = -1.0; // reverse: 1.0, normal: -1.0
        public MouseButton PanButton = MouseButton.Middle;

        public Point SelectionOrigin;

        #endregion
    }

    #endregion
}
