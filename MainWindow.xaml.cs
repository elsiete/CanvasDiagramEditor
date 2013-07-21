#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#endregion

namespace CanvasDiagramEditor
{
    #region Aliases

    // Maps
    using MapPin = Tuple<string, string>;
    using MapWire = Tuple<object, object, object>; // Item1: LineEx, Item2: start Parent, Item3: end Parent
    using MapWires = Tuple<object, List<Tuple<string, string>>>;

    // TemplatedParent.Tag: Item1: IsSelected, Item2: TagMap
    using Selection = Tuple<bool, List<Tuple<object, object, object>>>;

    // Canvas.Tag => Item1: undoHistory, Item2: redoHistory
    using History = Tuple<Stack<string>, Stack<string>>;

    // Diagram: TreeViewItem.Tag => Item1: model, Item2: History
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;

    // Solution Tree
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>; // Stack<TreeDiagram>
    using TreeProject = Tuple<string, Stack<Stack<string>>>; // Tuple<string, TreeDiagrams>, where string is Project Name
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>; // Stack<Tuple<string, TreeProject>>, 
    using TreeSolution = Tuple<string, Stack<Tuple<string, Stack<Stack<string>>>>>; // Tuple<string, TreeProjects>, where string is Solution Name 

    #endregion

    #region Tuple .NET 3.5

    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        internal Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        internal Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }

    #endregion

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

    #region LineEx
    
    public class LineEx : Shape
    {
        #region Properties

        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(3.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsStartVisible
        {
            get { return (bool)GetValue(IsStartVisibleProperty); }
            set { SetValue(IsStartVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsStartVisibleProperty =
            DependencyProperty.Register("IsStartVisible", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsEndVisible
        {
            get { return (bool)GetValue(IsEndVisibleProperty); }
            set { SetValue(IsEndVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsEndVisibleProperty =
            DependencyProperty.Register("IsEndVisible", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Calculate Size

        public static double CalculateZet(double startX, double startY, double endX, double endY)
        {
            double alpha = Math.Atan2(startY - endY, endX - startX);
            double theta = Math.PI - alpha;
            double zet = theta - Math.PI / 2;
            return zet;
        }

        public static double CalculateSizeX(double radius, double thickness, double zet)
        {
            double sizeX = Math.Sin(zet) * (radius + thickness);
            return sizeX;
        }

        public static double CalculateSizeY(double radius, double thickness, double zet)
        {
            double sizeY = Math.Cos(zet) * (radius + thickness);
            return sizeY;
        }

        #endregion

        #region Get Points

        public static Point GetLineStart(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            Point lineStart;

            if (isStartVisible)
            {
                double lx = startX + (2 * sizeX);
                double ly = startY - (2 * sizeY);

                lineStart = new Point(lx, ly);
            }
            else
            {
                lineStart = new Point(startX, startY);
            }

            return lineStart;
        }

        public static Point GetLineEnd(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            Point lineEnd;

            if (isEndVisible)
            {
                double lx = endX - (2 * sizeX);
                double ly = endY + (2 * sizeY);

                lineEnd = new Point(lx, ly);
            }
            else
            {
                lineEnd = new Point(endX, endY);
            }

            return lineEnd;
        }

        public static Point GetEllipseStartCenter(double startX, double startY, double sizeX, double sizeY, bool isStartVisible)
        {
            Point ellipseStartCenter;

            if (isStartVisible)
            {
                double ex = startX + sizeX;
                double ey = startY - sizeY;

                ellipseStartCenter = new Point(ex, ey);
            }
            else
            {
                ellipseStartCenter = new Point(startX, startY);
            }

            return ellipseStartCenter;
        }

        public static Point GetEllipseEndCenter(double endX, double endY, double sizeX, double sizeY, bool isEndVisible)
        {
            Point ellipseEndCenter;

            if (isEndVisible)
            {
                double ex = endX - sizeX;
                double ey = endY + sizeY;

                ellipseEndCenter = new Point(ex, ey);
            }
            else
            {
                ellipseEndCenter = new Point(endX, endY);
            }

            return ellipseEndCenter;
        }

        #endregion

        #region Get DefiningGeometry

        public double GetThickness()
        {
            return StrokeThickness / 2.0;
        }

        protected virtual Geometry GetDefiningGeometry()
        {
            bool isStartVisible = IsStartVisible;
            bool isEndVisible = IsEndVisible;

            double radius = Radius;
            double thickness = GetThickness();

            double startX = X1;
            double startY = Y1;
            double endX = X2;
            double endY = Y2;

            double zet = CalculateZet(startX, startY, endX, endY);
            double sizeX = CalculateSizeX(radius, thickness, zet);
            double sizeY = CalculateSizeY(radius, thickness, zet);

            Point ellipseStartCenter = GetEllipseStartCenter(startX, startY, sizeX, sizeY, isStartVisible);
            Point ellipseEndCenter = GetEllipseEndCenter(endX, endY, sizeX, sizeY, isEndVisible);
            Point lineStart = GetLineStart(startX, startY, sizeX, sizeY, isStartVisible);
            Point lineEnd = GetLineEnd(endX, endY, sizeX, sizeY, isEndVisible);

            var g = new GeometryGroup() { FillRule = FillRule.Nonzero };

            if (isStartVisible == true)
            {
                var startEllipse = new EllipseGeometry(ellipseStartCenter, radius, radius);
                g.Children.Add(startEllipse);
            }

            if (isEndVisible == true)
            {
                var endEllipse = new EllipseGeometry(ellipseEndCenter, radius, radius);
                g.Children.Add(endEllipse);
            }

            var line = new LineGeometry(lineStart, lineEnd);
            g.Children.Add(line);

            g.Freeze();

            return g;
        }

        #endregion

        #region DefiningGeometry

        protected override Geometry DefiningGeometry
        {
            get
            {
                var g = GetDefiningGeometry();
                return g;
            }
        }

        #endregion
    }

    #endregion

    #region SelectionThumb

    public class SelectionThumb : Thumb
    {
        #region IsSelected Attached Property

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(SelectionThumb),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)); 

        #endregion
    }

    #endregion

    #region SelectionAdorner

    public class SelectionAdorner : Adorner
    {
        #region Properties

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Point SelectionOrigin
        {
            get { return (Point)GetValue(SelectionOriginProperty); }
            set { SetValue(SelectionOriginProperty, value); }
        }

        public static readonly DependencyProperty SelectionOriginProperty =
            DependencyProperty.Register("SelectionOrigin", typeof(Point), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(new Point(),
                FrameworkPropertyMetadataOptions.None));

        public Rect SelectionRect
        {
            get { return (Rect)GetValue(SelectionRectProperty); }
            set { SetValue(SelectionRectProperty, value); }
        }

        public static readonly DependencyProperty SelectionRectProperty =
            DependencyProperty.Register("SelectionRect", typeof(Rect), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(new Rect(),
                FrameworkPropertyMetadataOptions.None));

        #endregion

        #region Fields

        private SolidColorBrush brush = null;
        private Pen pen = null;
        private double defaultThickness = 1.0;

        #endregion

        #region Constructor

        public SelectionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            brush = new SolidColorBrush(Color.FromArgb(0x90, 0x50, 0x50, 0x50));
            pen = new Pen(new SolidColorBrush(Color.FromArgb(0xF0, 0x90, 0x90, 0x90)), defaultThickness);
        }

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = SelectionRect;

            if (rect != null)
            {
                double zoom = Zoom;
                double thickness = defaultThickness / zoom;
                double half = thickness / 2.0;

                pen.Thickness = thickness;

                drawingContext.DrawRectangle(brush, pen, rect);
            }
        } 

        #endregion
    }

    #endregion

    #region Constants

    public static class ModelConstants
    {
        #region Model String Constants

        public const char ArgumentSeparator = ';';
        public const string PrefixRoot = "+";
        public const string PrefixChild = "-";

        public const char TagNameSeparator = '|';

        public const string TagHeaderSolution = "Solution";
        public const string TagHeaderProject = "Project";
        public const string TagHeaderDiagram = "Diagram";

        public const string TagElementPin = "Pin";
        public const string TagElementWire = "Wire";
        public const string TagElementInput = "Input";
        public const string TagElementOutput = "Output";
        public const string TagElementAndGate = "AndGate";
        public const string TagElementOrGate = "OrGate";

        public const string WireStartType = "Start";
        public const string WireEndType = "End";

        #endregion
    }

    public static class ResourceConstants
    {
        #region Resource String Constants

        public const string KeyStrokeThickness = "LogicStrokeThicknessKey";

        public const string KeyTemplatePin = "PinControlTemplateKey";
        public const string KeyTemplateInput = "InputControlTemplateKey";
        public const string KeyTemplateOutput = "OutputControlTemplateKey";
        public const string KeyTemplateAndGate = "AndGateControlTemplateKey";
        public const string KeyTemplateOrGate = "OrGateControlTemplateKey";

        public const string KeySyleRootThumb = "RootThumbStyleKey";
        public const string KeyStyleWireLine = "LineStyleKey";

        public const string StandalonePinName = "MiddlePin";

        #endregion
    }

    #endregion

    #region Parser

    public class ParseOptions
    {
        public ParseOptions()
        {
        }

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool AppendIds { get; set; }
        public bool UpdateIds { get; set; }
        public bool Select { get; set; }
        public bool CreateElements { get; set; }
        public DiagramProperties Properties { get; set; }
        public IdCounter Counter { get; set; }
    }

    public class DiagramProperties
    {
        public DiagramProperties()
        {
        }

        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public int GridOriginX { get; set; }
        public int GridOriginY { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public int GridSize { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public double SnapOffsetX { get; set; }
        public double SnapOffsetY { get; set; }
    }

    public class IdCounter
    {
        public IdCounter()
        {
            ResetAll();
        }

        public void ResetDiagram()
        {
            PinCount = 0;
            WireCount = 0;
            InputCount = 0;
            OutputCount = 0;
            AndGateCount = 0;
            OrGateCount = 0;
        }

        public void ResetAll()
        {
            SolutionCount = 0;
            ProjectCount = 0;
            DiagramCount = 0;

            ResetDiagram();
        }

        public int SolutionCount { get; set; }
        public int ProjectCount { get; set; }
        public int DiagramCount { get; set; }
        public int PinCount { get; set; }
        public int WireCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public int AndGateCount { get; set; }
        public int OrGateCount { get; set; }
    }

    public interface IDiagramCreator
    {
        object CreatePin(double x, double y, int id, bool snap);
        object CreateWire(double x1, double y1, double x2, double y2, bool start, bool end, int id);
        object CreateInput(double x, double y, int id, bool snap);
        object CreateOutput(double x, double y, int id, bool snap);
        object CreateAndGate(double x, double y, int id, bool snap);
        object CreateOrGate(double x, double y, int id, bool snap);
        object CreateDiagram(DiagramProperties properties);

        void UpdateConnections(IDictionary<string, MapWires> dict);
        void UpdateCounter(IdCounter original, IdCounter counter);
        void AppendIds(IEnumerable<object> elements);
        void InsertElements(IEnumerable<object> elements, bool select);
    }

    public interface IDiagramParser
    {
        TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options);
    }

    public class DiagramParser : IDiagramParser
    {
        #region Parse

        public TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options)
        {
            double offsetX = options.OffsetX;
            double offsetY = options.OffsetX;
            bool appendIds = options.AppendIds;
            bool updateIds = options.UpdateIds;
            bool select = options.Select;
            bool createElements = options.CreateElements;

            if (model == null)
                return null;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            string name = null;
            var counter = new IdCounter();
            var elements = new List<object>();
            MapWires tuple = null;
            var dict = new Dictionary<string, MapWires>();

            TreeSolution solution = null;
            TreeProjects projects = null;
            TreeProject project = null;
            TreeDiagrams diagrams = null;
            TreeDiagram diagram = null;

            var lines = model.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            //System.Diagnostics.Debug.Print("Parsing model:");

            foreach (var line in lines)
            {
                var args = line.Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                int length = args.Length;

                //System.Diagnostics.Debug.Print(line);

                if (length < 2)
                    continue;

                name = args[1];

                // root element
                if (StringUtil.Compare(args[0], ModelConstants.PrefixRoot))
                {
                    // Solution
                    if (StringUtil.StartsWith(name, ModelConstants.TagHeaderSolution) &&
                        length == 2)
                    {
                        projects = new TreeProjects();
                        solution = new TreeSolution(name, projects);
                    }

                    // Project
                    else if (StringUtil.StartsWith(name, ModelConstants.TagHeaderProject) &&
                        length == 2)
                    {
                        if (projects != null)
                        {
                            diagrams = new TreeDiagrams();
                            project = new TreeProject(name, diagrams);
                            projects.Push(project);
                        }
                    }

                    // Diagram
                    else if (StringUtil.StartsWith(name, ModelConstants.TagHeaderDiagram) &&
                        length == 13)
                    {
                        if (diagrams != null)
                        {
                            diagram = new TreeDiagram();
                            diagrams.Push(diagram);
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            var prop = new DiagramProperties();

                            prop.PageWidth = int.Parse(args[2]);
                            prop.PageHeight = int.Parse(args[3]);
                            prop.GridOriginX = int.Parse(args[4]);
                            prop.GridOriginY = int.Parse(args[5]);
                            prop.GridWidth = int.Parse(args[6]);
                            prop.GridHeight = int.Parse(args[7]);
                            prop.GridSize = int.Parse(args[8]);
                            prop.SnapX = double.Parse(args[9]);
                            prop.SnapY = double.Parse(args[10]);
                            prop.SnapOffsetX = double.Parse(args[11]);
                            prop.SnapOffsetY = double.Parse(args[12]);

                            creator.CreateDiagram(prop);

                            options.Properties = prop;
                        }
                    }

                    // Pin
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementPin) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.PinCount = Math.Max(counter.PinCount, id + 1);

                            var element = creator.CreatePin(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }

                    // Input
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementInput) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.InputCount = Math.Max(counter.InputCount, id + 1);

                            var element = creator.CreateInput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }

                    // Output
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOutput) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.OutputCount = Math.Max(counter.OutputCount, id + 1);

                            var element = creator.CreateOutput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }

                    // AndGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementAndGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.AndGateCount = Math.Max(counter.AndGateCount, id + 1);

                            var element = creator.CreateAndGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }

                    // OrGate
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementOrGate) &&
                        length == 4)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.OrGateCount = Math.Max(counter.OrGateCount, id + 1);

                            var element = creator.CreateOrGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }

                    // Wire
                    else if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        (length == 6 || length == 8))
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true)
                        {
                            double x1 = double.Parse(args[2]);
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            bool start = false;
                            bool end = false;

                            if (length == 8)
                            {
                                start = bool.Parse(args[6]);
                                end = bool.Parse(args[7]);
                            }

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            counter.WireCount = Math.Max(counter.WireCount, id + 1);

                            var element = creator.CreateWire(x1 + offsetX, y1 + offsetY,
                                x2 + offsetX, y2 + offsetY,
                                start, end,
                                id);

                            elements.Add(element);

                            tuple = new MapWires(element, new List<MapPin>());

                            dict.Add(args[1], tuple);
                        }
                    }
                }

                // child element
                else if (StringUtil.Compare(args[0], ModelConstants.PrefixChild))
                {
                    if (StringUtil.StartsWith(name, ModelConstants.TagElementWire) &&
                        length == 3)
                    {
                        if (diagram != null)
                        {
                            diagram.Push(line);
                        }

                        if (createElements == true && tuple != null)
                        {
                            var wires = tuple.Item2;

                            wires.Add(new MapPin(name, args[2]));
                        }
                    }
                }
            }

            if (createElements == true)
            {
                creator.UpdateConnections(dict);

                if (appendIds == true)
                {
                    creator.AppendIds(elements);
                }

                if (updateIds == true)
                {
                    creator.UpdateCounter(options.Counter, counter);
                }

                creator.InsertElements(elements, select);
            }

            sw.Stop();
            System.Diagnostics.Debug.Print("Parse() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return solution;
        } 
        
        #endregion
    }

    #endregion

    #region DiagramEditorOptions

    public class DiagramEditorOptions
    {
        #region Fields

        public DiagramProperties currentProperties = new DiagramProperties();

        public Canvas currentCanvas = null;
        public Path currentPathGrid = null;

        public bool enableHistory = true;

        public LineEx currentLine = null;
        public FrameworkElement currentRoot = null;

        public IdCounter counter = new IdCounter();

        public Point rightClick;

        public bool enableInsertLast = false;
        public string lastInsert = ModelConstants.TagElementInput;

        public bool enablePageGrid = true;
        public bool enablePageTemplate = true;

        public double defaultGridSize = 30;

        public bool enableSnap = true;
        public bool snapOnRelease = false;

        public bool moveAllSelected = false;

        public double hitTestRadiusX = 6.0;
        public double hitTestRadiusY = 6.0;

        public bool skipContextMenu = false;
        public bool skipLeftClick = false;

        public Point panStart;
        public double previousScrollOffsetX = -1.0;
        public double previousScrollOffsetY = -1.0;

        public double defaultStrokeThickness = 1.0;

        public double zoomInFactor = 0.1;
        public double zoomOutFactor = 0.1;

        public Point zoomPoint;

        public double reversePanDirection = -1.0; // reverse: 1.0, normal: -1.0
        public MouseButton panButton = MouseButton.Middle;

        #endregion
    }

    #endregion

    #region DiagramEditor

    public class DiagramEditor : IDiagramCreator
    {
        #region Fields

        public DiagramEditorOptions options = null;
        private Canvas parserCanvas = null;
        private Path parserPath = null;

        #endregion

        #region Model

        public TreeSolution ParseDiagramModel(string model,
            Canvas canvas,
            Path path,
            double offsetX,
            double offsetY,
            bool appendIds,
            bool updateIds,
            bool select,
            bool createElements)
        {
            var parser = new DiagramParser();

            var parseOptions = new ParseOptions()
            {
                OffsetX = offsetX,
                OffsetY = offsetY,
                AppendIds = appendIds,
                UpdateIds = updateIds,
                Select = select,
                CreateElements = createElements,
                Counter = options.counter,
                Properties = options.currentProperties
            };

            parserCanvas = canvas;
            parserPath = path;

            var result = parser.Parse(model, this, parseOptions);

            options.counter = parseOptions.Counter;
            options.currentProperties = parseOptions.Properties;

            parserCanvas = null;
            parserPath = null;

            return result;
        }

        public string GenerateDiagramModel(Canvas canvas, string uid)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            var elements = canvas != null ? canvas.Children.Cast<FrameworkElement>() : Enumerable.Empty<FrameworkElement>();
            var prop = options.currentProperties;

            string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                uid == null ? ModelConstants.TagHeaderDiagram : uid,
                prop.PageWidth, prop.PageHeight,
                prop.GridOriginX, prop.GridOriginY,
                prop.GridWidth, prop.GridHeight,
                prop.GridSize,
                prop.SnapX, prop.SnapY,
                prop.SnapOffsetX, prop.SnapOffsetY);

            diagram.AppendLine(header);
            //System.Diagnostics.Debug.Print(header);

            GenerateDiagramModel(diagram, elements);

            var result = diagram.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        public void ClearDiagramModel(Canvas canvas)
        {
            canvas.Children.Clear();

            options.counter.ResetDiagram();
        }

        public string GenerateDiagramModelFromSelected(Canvas canvas)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            var elements = GetSelectedElements(canvas);

            GenerateDiagramModel(diagram, elements);

            var result = diagram.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModelFromSelected() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        private static void GenerateDiagramModel(StringBuilder diagram, IEnumerable<FrameworkElement> elements)
        {
            foreach (var element in elements)
            {
                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);

                if (element.Uid.StartsWith(ModelConstants.TagElementWire))
                {
                    var line = element as LineEx;
                    var margin = line.Margin;

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}{5}{7}{5}{8}",
                        element.Uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.X2 + margin.Left, line.Y2 + margin.Top,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        line.IsStartVisible, line.IsEndVisible);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                        element.Uid,
                        x, y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }

                if (element.Tag != null)
                {
                    var selection = element.Tag as Selection;
                    var tuples = selection.Item2;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1 as LineEx;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        if (start != null)
                        {
                            // Start
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.Uid,
                                ModelConstants.WireStartType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            diagram.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.Uid,
                                ModelConstants.WireEndType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            diagram.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }
        }

        private static void AddElementsToCanvas(Canvas canvas, IEnumerable<FrameworkElement> elements, bool select)
        {
            foreach (var element in elements)
            {
                canvas.Children.Add(element);

                if (select == true)
                {
                    SelectionThumb.SetIsSelected(element, true);
                }
            }
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {
            var canvas = parserCanvas;

            AddElementsToCanvas(canvas, elements.Cast<FrameworkElement>(), select);
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            original.PinCount = Math.Max(original.PinCount, counter.PinCount);
            original.WireCount = Math.Max(original.WireCount, counter.WireCount);
            original.InputCount = Math.Max(original.InputCount, counter.InputCount);
            original.OutputCount = Math.Max(original.OutputCount, counter.OutputCount);
            original.AndGateCount = Math.Max(original.AndGateCount, counter.AndGateCount);
            original.OrGateCount = Math.Max(original.OrGateCount, counter.OrGateCount);
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            // update wire to element connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1 as FrameworkElement;
                var wires = item.Value.Item2;

                if (element.Tag == null)
                {
                    element.Tag = new Selection(false, new List<MapWire>());
                }

                if (wires.Count > 0)
                {
                    var selection = element.Tag as Selection;
                    var tuples = selection.Item2;

                    foreach (var wire in wires)
                    {
                        string _name = wire.Item1;
                        string _type = wire.Item2;

                        if (StringUtil.Compare(_type, ModelConstants.WireStartType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, element, null);

                                tuples.Add(mapWire);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire Start: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapTag(line, element, null);
                            //tuples.Add(_tuple);
                        }
                        else if (StringUtil.Compare(_type, ModelConstants.WireEndType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, null, element);

                                tuples.Add(mapWire);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire End: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapWire(line, null, element);
                            //tuples.Add(_tuple);
                        }
                    }
                }
            }
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements.Cast<FrameworkElement>())
            {
                string[] uid = element.Uid.Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = GetUpdatedId(options.counter, type);

                //System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                string appendedUid = string.Concat(type, ModelConstants.TagNameSeparator, appendedId.ToString());
                element.Uid = appendedUid;

                //if (element.Tag != null)
                //{
                //    var _tuples = element.Tag as List<TagMap>;
                //    foreach (var _tuple in _tuples)
                //    {
                //        System.Diagnostics.Debug.Print("  -{0}", _tuple.Item1.Uid);
                //    }
                //}
            }
        }

        private static int GetUpdatedId(IdCounter counter, string type)
        {
            int appendedId = -1;

            switch (type)
            {
                case ModelConstants.TagElementWire:
                    appendedId = counter.WireCount;
                    counter.WireCount += 1;
                    break;
                case ModelConstants.TagElementInput:
                    appendedId = counter.InputCount;
                    counter.InputCount += 1;
                    break;
                case ModelConstants.TagElementOutput:
                    appendedId = counter.OutputCount;
                    counter.OutputCount += 1;
                    break;
                case ModelConstants.TagElementAndGate:
                    appendedId = counter.AndGateCount;
                    counter.AndGateCount += 1;
                    break;
                case ModelConstants.TagElementOrGate:
                    appendedId = counter.OrGateCount;
                    counter.OrGateCount += 1;
                    break;
                case ModelConstants.TagElementPin:
                    appendedId = counter.PinCount;
                    counter.PinCount += 1;
                    break;
                default:
                    throw new Exception("Unknown element type.");
            }

            return appendedId;
        }

        #endregion

        #region Snap

        public double Snap(double original, double snap, double offset)
        {
            return Snap(original - offset, snap) + offset;
        }

        public double Snap(double original, double snap)
        {
            return original + ((Math.Round(original / snap) - original / snap) * snap);
        }

        private double SnapOffsetX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.currentProperties.SnapX, options.currentProperties.SnapOffsetX) : original;
        }

        private double SnapOffsetY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.currentProperties.SnapY, options.currentProperties.SnapOffsetY) : original;
        }

        private double SnapX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.currentProperties.SnapX) : original;
        }

        private double SnapY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.currentProperties.SnapY) : original;
        }

        #endregion

        #region Grid

        public void GenerateGrid(Path path, double originX, double originY, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();

            double startX = size;
            double startY = size;

            // horizontal lines
            for (double y = startY + originY /* originY + size */; y < height + originY; y += size)
            {
                sb.AppendFormat("M{0},{1}", originX, y);
                sb.AppendFormat("L{0},{1}", width + originX, y);
            }

            // vertical lines
            for (double x = startX + originX /* originX + size */; x < width + originX; x += size)
            {
                sb.AppendFormat("M{0},{1}", x, originY);
                sb.AppendFormat("L{0},{1}", x, height + originY);
            }

            string s = sb.ToString();

            path.Data = Geometry.Parse(s);

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateGrid() in {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public void SetDiagramSize(Canvas canvas, double width, double height)
        {
            canvas.Width = width;
            canvas.Height = height;
        }

        #endregion

        #region History

        private History GetHistory(Canvas canvas)
        {
            if (canvas.Tag == null)
            {
                canvas.Tag = new History(new Stack<string>(), new Stack<string>());
            }

            var tuple = canvas.Tag as History;

            return tuple;
        }

        public string AddToHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return null;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            var model = GenerateDiagramModel(canvas, null);

            undoHistory.Push(model);

            redoHistory.Clear();

            return model;
        }

        private void RollbackUndoHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // remove unused history
            undoHistory.Pop();
        }

        private void RollbackRedoHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // remove unused history
            redoHistory.Pop();
        }

        public void ClearHistory(Canvas canvas)
        {
            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            undoHistory.Clear();
            redoHistory.Clear();
        }

        private void Undo(Canvas canvas, Path path, bool pushRedo)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = GenerateDiagramModel(canvas, null);
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        private void Redo(Canvas canvas, Path path, bool pushUndo)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = GenerateDiagramModel(canvas, null);
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();

            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        public void Undo()
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            this.Undo(canvas, path, true);
        }

        public void Redo()
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            this.Redo(canvas, path, true);
        }

        #endregion

        #region Move

        private void SetElementPosition(FrameworkElement element, double left, double top, bool snap)
        {
            Canvas.SetLeft(element, SnapOffsetX(left, snap));
            Canvas.SetTop(element, SnapOffsetY(top, snap));
        }

        private void MoveRoot(FrameworkElement element, double dX, double dY, bool snap)
        {
            double left = Canvas.GetLeft(element) + dX;
            double top = Canvas.GetTop(element) + dY;

            SetElementPosition(element, left, top, snap);

            MoveLines(element, dX, dY, snap);
        }

        private void MoveLines(FrameworkElement element, double dX, double dY, bool snap)
        {
            if (element != null && element.Tag != null)
            {
                var selection = element.Tag as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                {
                    var line = tuple.Item1 as LineEx;
                    var start = tuple.Item2;
                    var end = tuple.Item3;

                    if (start != null)
                    {
                        var margin = line.Margin;
                        double left = margin.Left;
                        double top = margin.Top;
                        double x = 0.0;
                        double y = 0.0;

                        //line.X1 = SnapOffsetX(line.X1 + dX, snap);
                        //line.Y1 = SnapOffsetY(line.Y1 + dY, snap);

                        x = SnapOffsetX(left + dX, snap);
                        y = SnapOffsetY(top + dY, snap);

                        if (left != x || top != y)
                        {
                            line.X2 += left - x;
                            line.Y2 += top - y;
                            line.Margin = new Thickness(x, y, 0, 0);
                        }
                    }
                    
                    if (end != null)
                    {
                        double left = line.X2;
                        double top = line.Y2;
                        double x = 0.0;
                        double y = 0.0;

                        x = SnapX(left + dX, snap);
                        y = SnapY(top + dY, snap);

                        line.X2 = x;
                        line.Y2 = y;  
                    }
                }
            }
        }

        public void MoveSelectedElements(Canvas canvas, double dX, double dY, bool snap)
        {
            // move all selected elements
            var thumbs = canvas.Children.OfType<SelectionThumb>().Where(x => SelectionThumb.GetIsSelected(x));

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        #endregion

        #region Drag

        public void Drag(Canvas canvas, SelectionThumb element, double dX, double dY)
        {
            bool snap = (options.snapOnRelease == true && options.enableSnap == true) ? false : options.enableSnap;

            if (options.moveAllSelected == true)
            {
                MoveSelectedElements(canvas, dX, dY, snap);
            }
            else
            {
                // move only selected element
                MoveRoot(element, dX, dY, snap);
            }
        }

        public void DragStart(Canvas canvas, SelectionThumb element)
        {
            AddToHistory(canvas);

            if (SelectionThumb.GetIsSelected(element) == true)
            {
                options.moveAllSelected = true;
            }
            else
            {
                options.moveAllSelected = false;

                // select
                SelectionThumb.SetIsSelected(element, true);
            }
        }

        public void DragEnd(Canvas canvas, SelectionThumb element)
        {
            if (options.snapOnRelease == true && options.enableSnap == true)
            {
                if (options.moveAllSelected == true)
                {
                    MoveSelectedElements(canvas, 0.0, 0.0, options.enableSnap);
                }
                else
                {
                    // move only selected element

                    // deselect
                    SelectionThumb.SetIsSelected(element, false);

                    MoveRoot(element, 0.0, 0.0, options.enableSnap);
                }
            }
            else
            {
                if (options.moveAllSelected != true)
                {
                    // de-select
                    SelectionThumb.SetIsSelected(element, false);
                }
            }

            options.moveAllSelected = false;
        }

        #endregion

        #region Thumb Events

        private void RootElement_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            Drag(canvas, element, dX, dY);
        }

        private void RootElement_DragStarted(object sender, DragStartedEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            DragStart(canvas, element);
        }

        private void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            DragEnd(canvas, element);
        }

        #endregion

        #region Create

        private void SetThumbEvents(SelectionThumb thumb)
        {
            thumb.DragDelta += this.RootElement_DragDelta;
            thumb.DragStarted += this.RootElement_DragStarted;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

        public object CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementPin + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateWire(double x1, double y1, double x2, double y2, bool start, bool end, int id)
        {
            var line = new LineEx()
            {
                Style = Application.Current.Resources[ResourceConstants.KeyStyleWireLine] as Style,
                X1 = 0, //X1 = x1,
                Y1 = 0, //Y1 = y1,
                Margin = new Thickness(x1, y1, 0, 0),
                X2 = x2 - x1, // X2 = x2,
                Y2 = y2 - y1, // Y2 = y2,
                IsStartVisible = start,
                IsEndVisible = end,
                Uid = ModelConstants.TagElementWire + ModelConstants.TagNameSeparator + id.ToString()
            };

            return line;
        }

        public object CreateInput(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementInput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateOutput(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOutput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementAndGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOrGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            var canvas = parserCanvas;
            var path = parserPath;

            GenerateGrid(path,
                properties.GridOriginX,
                properties.GridOriginY,
                properties.GridWidth, 
                properties.GridHeight,
                properties.GridSize);

            SetDiagramSize(canvas, properties.PageWidth, properties.PageHeight);

            return null;
        }

        private void CreatePinConnection(Canvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            var root =
                (
                    (pin.Parent as FrameworkElement).Parent as FrameworkElement
                ).TemplatedParent as FrameworkElement;

            options.currentRoot = root;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(options.currentRoot);
            double ry = Canvas.GetTop(options.currentRoot);
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);
            double x = rx + px;
            double y = ry + py;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreatePinConnection(canvas, x, y);
        }

        private LineEx CreatePinConnection(Canvas canvas, double x, double y)
        {
            LineEx result = null;

            if (options.currentRoot.Tag == null)
            {
                options.currentRoot.Tag = new Selection(false, new List<MapWire>());
            }

            var selection = options.currentRoot.Tag as Selection;
            var tuples = selection.Item2;

            if (options.currentLine == null)
            {
                var line = CreateWire(x, y, x, y, false, false, options.counter.WireCount) as LineEx;
                options.counter.WireCount += 1;

                options.currentLine = line;

                // update connections
                var tuple = new MapWire(options.currentLine, options.currentRoot, null);
                tuples.Add(tuple);

                canvas.Children.Add(options.currentLine);

                result = line;
            }
            else
            {
                var margin = options.currentLine.Margin;

                options.currentLine.X2 = x - margin.Left;
                options.currentLine.Y2 = y - margin.Top;

                // update connections
                var tuple = new MapWire(options.currentLine, null, options.currentRoot);
                tuples.Add(tuple);

                result = options.currentLine;

                options.currentLine = null;
                options.currentRoot = null;
            }

            return result;
        }

        #endregion

        #region Insert

        public FrameworkElement InsertPin(Canvas canvas, Point point)
        {
            var thumb = CreatePin(point.X, point.Y, options.counter.PinCount, options.enableSnap) as SelectionThumb;
            options.counter.PinCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, options.counter.InputCount, options.enableSnap) as SelectionThumb;
            options.counter.InputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, options.counter.OutputCount, options.enableSnap) as SelectionThumb;
            options.counter.OutputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, options.counter.AndGateCount, options.enableSnap) as SelectionThumb;
            options.counter.AndGateCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, options.counter.OrGateCount, options.enableSnap) as SelectionThumb;
            options.counter.OrGateCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertLast(Canvas canvas, string type, Point point)
        {
            switch (type)
            {
                case ModelConstants.TagElementInput:
                    return InsertInput(canvas, point);
                case ModelConstants.TagElementOutput:
                    return InsertOutput(canvas, point);
                case ModelConstants.TagElementAndGate:
                    return InsertAndGate(canvas, point);
                case ModelConstants.TagElementOrGate:
                    return InsertOrGate(canvas, point);
                case ModelConstants.TagElementPin:
                    return InsertPin(canvas, point);
                default:
                    return null;
            }
        }

        #endregion

        #region Delete

        private void DeleteElement(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point);
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        private void DeleteElement(Canvas canvas, FrameworkElement element)
        {
            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                DeleteWire(canvas, line);
            }
            else
            {
                canvas.Children.Remove(element);
            }
        }

        public FrameworkElement HitTest(Canvas canvas, ref Point point)
        {
            var selectedElements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
            {
                RadiusX = options.hitTestRadiusX,
                RadiusY = options.hitTestRadiusY,
                Center = new Point(point.X, point.Y),
            };

            var hitTestParams = new GeometryHitTestParameters(elippse);

            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    if (VisualTreeHelper.GetParent(element) == canvas)
                    {
                        selectedElements.Add(element);
                    }
                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(canvas, filterCallback, resultCallback, hitTestParams);

            return selectedElements.FirstOrDefault() as FrameworkElement;
        }

        public IEnumerable<FrameworkElement> HitTest(Canvas canvas, ref Rect rect)
        {
            var selectedElements = new List<DependencyObject>();

            var rectangle = new RectangleGeometry(rect, 0.0, 0.0);

            var hitTestParams = new GeometryHitTestParameters(rectangle);

            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    if (VisualTreeHelper.GetParent(element) == canvas)
                    {
                        selectedElements.Add(element);
                    }
                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(canvas, filterCallback, resultCallback, hitTestParams);

            return selectedElements.Cast<FrameworkElement>();
        }

        private static void DeleteWire(Canvas canvas, LineEx line)
        {
            canvas.Children.Remove(line);

            // remove wire connections
            RemoveWireConnections(canvas, line);

            // find empty pins
            var pins = FindEmptyPins(canvas);

            // remove empty pins
            foreach (var pin in pins)
            {
                canvas.Children.Remove(pin);
            }
        }

        private static List<FrameworkElement> FindEmptyPins(Canvas canvas)
        {
            var pins = new List<FrameworkElement>();

            foreach (var child in canvas.Children)
            {
                var _element = child as FrameworkElement;

                string uid = _element.Uid;

                if (uid != null &&
                    StringUtil.StartsWith(uid, ModelConstants.TagElementPin))
                {
                    if (_element.Tag != null)
                    {
                        var selection = _element.Tag as Selection;
                        var tuples = selection.Item2;

                        if (tuples.Count <= 0)
                        {
                            pins.Add(_element);
                        }
                    }
                    else
                    {
                        pins.Add(_element);
                    }
                }
            }

            return pins;
        }

        private static void RemoveWireConnections(Canvas canvas, LineEx line)
        {
            foreach (var child in canvas.Children)
            {
                var _element = child as FrameworkElement;

                if (_element.Tag != null)
                {
                    var selection = _element.Tag as Selection;
                    var tuples = selection.Item2;

                    var remove = new List<MapWire>();

                    foreach (var tuple in tuples)
                    {
                        var _line = tuple.Item1 as LineEx;

                        if (StringUtil.Compare(_line.Uid, line.Uid))
                        {
                            remove.Add(tuple);
                        }
                    }

                    foreach (var tuple in remove)
                    {
                        tuples.Remove(tuple);
                    }
                }
            }
        }

        public void Delete(Canvas canvas, Point point)
        {
            AddToHistory(canvas);

            DeleteElement(canvas, point);

            options.skipLeftClick = false;
        }

        #endregion

        #region Invert

        public LineEx FindLineEx(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point);
            if (element == null)
                return null;

            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("FindLineEx, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                return line;
            }

            return null;
        }

        public void ToggleStart(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas);

                line.IsStartVisible = line.IsStartVisible == true ? false : true;

                options.skipLeftClick = false;
            }
        }

        public void ToggleEnd(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas);

                line.IsEndVisible = line.IsEndVisible == true ? false : true;

                options.skipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save

        private void SaveModel(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        private void SaveDiagram(string fileName, Canvas canvas)
        {
            string model = GenerateDiagramModel(canvas, null);

            SaveModel(fileName, model);
        }

        private void OpenDiagram(string fileName, Canvas canvas, Path path)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                AddToHistory(canvas);

                ClearDiagramModel(canvas);
                ParseDiagramModel(diagram, canvas, path, 0, 0, false, true, false, true);
            }
        }

        private TreeSolution OpenSolution(string fileName)
        {
            TreeSolution solution = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                solution = ParseDiagramModel(diagram, null, null, 0, 0, false, false, false, false);
            }

            return solution;
        }

        public string ImportModel(string fileName)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                return diagram;
            }
        }

        public void OpenDiagram()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;
                var path = options.currentPathGrid;

                this.OpenDiagram(dlg.FileName, canvas, path);
            }
        }

        public TreeSolution OpenSolution()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Solution"
            };

            TreeSolution solution = null;

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;

                ClearDiagramModel(canvas);

                solution = OpenSolution(dlg.FileName);
            }

            return solution;
        }

        public void SaveDiagram()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Diagram",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;

                this.SaveDiagram(dlg.FileName, canvas);
            }
        }

        public void SaveSolution(string model)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Solution",
                FileName = "solution"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                this.SaveModel(dlg.FileName, model);
            }
        }

        public void ExportDiagram()
        {
            //Export(new MsoWordExport(), false);
            Export(new OpenXmlExport(), false);
        }

        public void ExportDiagramHistory()
        {
            //Export(new MsoWordExport(), true);
            Export(new OpenXmlExport(), true);
        }

        private void Export(IDiagramExport export, bool exportHistory)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Word Document (*.docx)|*.docx|All Files (*.*)|*.*",
                Title = "Export to Word Document",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;
                var fileName = dlg.FileName;

                Export(export, exportHistory, canvas, fileName);

                MessageBox.Show("Exported document: " +
                    System.IO.Path.GetFileName(dlg.FileName),
                    "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Export(IDiagramExport export, bool exportHistory, Canvas canvas, string fileName)
        {
            List<string> diagrams = null;

            var currentDiagram = GenerateDiagramModel(canvas, null);

            if (exportHistory == false)
            {
                diagrams = new List<string>();
            }
            else
            {
                var history = GetHistory(canvas);
                var undoHistory = history.Item1;
                var redoHistory = history.Item2;

                diagrams = new List<string>(undoHistory.Reverse());
            }

            diagrams.Add(currentDiagram);

            if (diagrams == null)
                throw new NullReferenceException();

            export.CreateDocument(fileName, diagrams);
        }

        public void Print()
        {
            var model = GenerateDiagramModel(options.currentCanvas, null);

            var canvas = new Canvas()
            {
                Background = Brushes.Black,
                Width = options.currentCanvas.Width,
                Height = options.currentCanvas.Height
            };

            Path path = new Path();

            ParseDiagramModel(model, canvas, path, 0, 0, false, false, false, true);

            Visual visual = canvas;

            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(visual, "diagram");
        }

        public string Import()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var diagram = this.ImportModel(dlg.FileName);

                return diagram;
            }

            return null;
        }

        public void Insert(string diagram, double offsetX, double offsetY)
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            AddToHistory(canvas);

            DeselectAll();
            ParseDiagramModel(diagram, canvas, path, offsetX, offsetY, true, true, true, true);
        }

        public void Clear()
        {
            var canvas = options.currentCanvas;

            AddToHistory(canvas);

            ClearDiagramModel(canvas);
        }

        public string Generate()
        {
            var canvas = options.currentCanvas;

            var diagram = GenerateDiagramModel(canvas, null);

            return diagram;
        }

        public string GenerateFromSelected()
        {
            var canvas = options.currentCanvas;

            var diagram = GenerateDiagramModelFromSelected(canvas);

            return diagram;
        }

        #endregion

        #region Edit

        public void Cut()
        {
            var canvas = options.currentCanvas;

            string model = GenerateDiagramModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = GenerateDiagramModel(canvas, null);

                var elements = GetAllElements(canvas);

                Delete(canvas, elements);
            }
            else
            {
                Delete();
            }

            Clipboard.SetText(model);
        }

        public void Copy()
        {
            var canvas = options.currentCanvas;

            string model = GenerateDiagramModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = GenerateDiagramModel(canvas, null);
            }

            Clipboard.SetText(model);
        }

        public void Paste(Point point)
        {
            var model = Clipboard.GetText();

            if (model != null || model.Length > 0)
            {
                Insert(model, point.X, point.Y);
            }
        }

        public void Delete()
        {
            var canvas = options.currentCanvas;
            var elements = GetSelectedElements(canvas);

            Delete(canvas, elements);
        }

        public void Delete(Canvas canvas, IEnumerable<FrameworkElement> elements)
        {
            AddToHistory(canvas);

            // delete thumbs & lines

            foreach (var element in elements)
            {
                DeleteElement(canvas, element);
            }
        }

        public static IEnumerable<FrameworkElement> GetSelectedElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get selected thumbs
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {
                if (SelectionThumb.GetIsSelected(thumb) == true)
                {
                    elements.Add(thumb);
                }
            }

            // get selected lines
            var lines = canvas.Children.OfType<LineEx>();

            foreach (var line in lines)
            {
                if (SelectionThumb.GetIsSelected(line) == true)
                {
                    elements.Add(line);
                }
            }

            return elements;
        }

        public static IEnumerable<FrameworkElement> GetAllElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get all thumbs
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {

                    elements.Add(thumb);
            }

            // get all lines
            var lines = canvas.Children.OfType<LineEx>();

            foreach (var line in lines)
            {
                    elements.Add(line);
            }

            return elements;
        }

        public static void SetSelectionThumbsSelection(Canvas canvas, bool isSelected)
        {
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                SelectionThumb.SetIsSelected(thumb, isSelected);
            }
        }

        public static void SetLinesSelection(Canvas canvas, bool isSelected)
        {
            var lines = canvas.Children.OfType<LineEx>();

            // deselect all lines
            foreach (var line in lines)
            {
                SelectionThumb.SetIsSelected(line, isSelected);
            }
        }

        public void SelectAll()
        {
            var canvas = options.currentCanvas;

            SetSelectionThumbsSelection(canvas, true);
            SetLinesSelection(canvas, true);
        }

        public void DeselectAll()
        {
            var canvas = options.currentCanvas;

            SetSelectionThumbsSelection(canvas, false);
            SetLinesSelection(canvas, false);
        }

        #endregion

        #region Handlers

        public void HandleLeftDown(Canvas canvas, Point point)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                var root = InsertPin(canvas, point);

                options.currentRoot = root;

                //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(options.currentRoot);
                double ry = Canvas.GetTop(options.currentRoot);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                CreatePinConnection(canvas, x, y);

                options.currentRoot = root;

                CreatePinConnection(canvas, x, y);
            }
            else if (options.enableInsertLast == true)
            {
                AddToHistory(canvas);

                InsertLast(canvas, options.lastInsert, point);
            }
        }

        private static void ToggleLineSelection(FrameworkElement element)
        {
            string uid = element.Uid;

            System.Diagnostics.Debug.Print("ToggleLineSelection: {0}, uid: {1}, parent: {2}",
                element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                // select/deselect line
                bool isSelected = SelectionThumb.GetIsSelected(line);
                SelectionThumb.SetIsSelected(line, isSelected ? false : true);
            }
        }

        public bool HandlePreviewLeftDown(Canvas canvas, Point point, FrameworkElement pin)
        {
            if (options.currentRoot == null && 
                options.currentLine == null &&
                Keyboard.Modifiers != ModifierKeys.Control)
            {
                var element = HitTest(canvas, ref point);
                if (element != null)
                {
                    ToggleLineSelection(element);
                }
                else
                {
                    SetLinesSelection(canvas, false);
                }
            }

            if (pin != null &&
                (!StringUtil.Compare(pin.Name, ResourceConstants.StandalonePinName) || Keyboard.Modifiers == ModifierKeys.Control))
            {
                if (options.currentLine == null)
                    AddToHistory(canvas);

                CreatePinConnection(canvas, pin);

                return true;
            }

            return false;
        }

        public void HandleMove(Canvas canvas, Point point)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                var margin = options.currentLine.Margin;

                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (options.currentLine.X2 != x)
                {
                    //this._line.X2 = SnapX(x);
                    options.currentLine.X2 = x;
                }

                if (options.currentLine.Y2 != y)
                {
                    //this._line.Y2 = SnapY(y);
                    options.currentLine.Y2 = y;
                }
            }
        }

        public bool HandleRightDown(Canvas canvas, Path path)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                if (options.enableHistory == true)
                {
                    Undo(canvas, path, false);
                }
                else
                {
                    var selection = options.currentRoot.Tag as Selection;
                    var tuples = selection.Item2;

                    var last = tuples.LastOrDefault();
                    tuples.Remove(last);

                    canvas.Children.Remove(options.currentLine);
                }

                options.currentLine = null;
                options.currentRoot = null;

                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private DiagramEditor editor = null;

        private Point selectionOrigin;
        private SelectionAdorner adorner = null;

        private double zoomLogBase = 1.8;
        private double zoomExpFactor = 1.3;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeOptions();

            GenerateGrid(false);

            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PanScrollViewer.Focus();
        }

        private void InitializeOptions()
        {
            editor = new DiagramEditor();
            editor.options = new DiagramEditorOptions();

            editor.options.counter.ProjectCount = 1;
            editor.options.counter.DiagramCount = 1;

            UpdateDiagramProperties();

            editor.options.currentCanvas = this.DiagramCanvas;
            editor.options.currentPathGrid = this.PathGrid;

            EnableHistory.IsChecked = editor.options.enableHistory;
            EnableInsertLast.IsChecked = editor.options.enableInsertLast;
            EnablePageGrid.IsChecked = editor.options.enablePageGrid;
            EnablePageTemplate.IsChecked = editor.options.enablePageTemplate;
            EnableSnap.IsChecked = editor.options.enableSnap;
            SnapOnRelease.IsChecked = editor.options.snapOnRelease;
        }

        #endregion

        #region Grid

        private void UpdateDiagramProperties()
        {
            var prop = editor.options.currentProperties;

            prop.PageWidth = int.Parse(TextPageWidth.Text);
            prop.PageHeight = int.Parse(TextPageHeight.Text);

            prop.GridOriginX = int.Parse(TextGridOriginX.Text);
            prop.GridOriginY = int.Parse(TextGridOriginY.Text);
            prop.GridWidth = int.Parse(TextGridWidth.Text);
            prop.GridHeight = int.Parse(TextGridHeight.Text);
            prop.GridSize = int.Parse(TextGridSize.Text);

            prop.SnapX = double.Parse(TextSnapX.Text);
            prop.SnapY = double.Parse(TextSnapY.Text);
            prop.SnapOffsetX = double.Parse(TextSnapOffsetX.Text);
            prop.SnapOffsetY = double.Parse(TextSnapOffsetY.Text);
        }

        private void GenerateGrid(bool undo)
        {
            var canvas = editor.options.currentCanvas;
            var path = this.PathGrid;

            UpdateDiagramProperties();

            if (undo == true)
            {
                editor.AddToHistory(canvas);
            }

            var prop = editor.options.currentProperties;

            editor.GenerateGrid(path, 
                prop.GridOriginX, prop.GridOriginY,
                prop.GridWidth, prop.GridHeight,
                prop.GridSize);

            editor.SetDiagramSize(canvas, prop.PageWidth, prop.PageHeight);
        }

        #endregion

        #region Pan

        private void BeginPan(Point point)
        {
            editor.options.panStart = point;

            editor.options.previousScrollOffsetX = -1.0;
            editor.options.previousScrollOffsetY = -1.0;

            this.Cursor = Cursors.ScrollAll;
            this.PanScrollViewer.CaptureMouse();
        }

        private void EndPan()
        {
            if (PanScrollViewer.IsMouseCaptured == true)
            {
                this.Cursor = Cursors.Arrow;
                this.PanScrollViewer.ReleaseMouseCapture();
            }
        }

        private void PanToPoint(Point point)
        {
            double scrollOffsetX = point.X - editor.options.panStart.X;
            double scrollOffsetY = point.Y - editor.options.panStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * 1.0) * editor.options.reversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * 1.0) * editor.options.reversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != editor.options.previousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                editor.options.previousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != editor.options.previousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                editor.options.previousScrollOffsetY = scrollOffsetY;
            }

            editor.options.panStart = point;
        }

        private void PanToOffset(double offsetX, double offsetY)
        {
            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = Math.Round(horizontalOffset + offsetX, 0);
            double scrollOffsetY = Math.Round(verticalOffset + offsetY, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
            this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
        }

        #endregion

        #region Zoom

        public double CalculateZoom(double x)
        {
            double l = Math.Log(x, zoomLogBase);
            double e = Math.Exp(l / zoomExpFactor);
            double y = x + x * l * e;
            return y;
        }

        private void Zoom(double zoom)
        {
            if (editor == null || editor.options == null)
                return;

            double zoom_fx = CalculateZoom(zoom);

            //System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            var st = GetZoomScaleTransform();

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            Application.Current.Resources[ResourceConstants.KeyStrokeThickness] = editor.options.defaultStrokeThickness / zoom_fx;

            // zoom to point
            ZoomToPoint(zoom_fx, oldZoom);
        }

        private ScaleTransform GetZoomScaleTransform()
        {
            //var tg = RootGrid.RenderTransform as TransformGroup;
            var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is ScaleTransform) as ScaleTransform;

            return st;
        }

        private void ZoomToPoint(double zoom, double oldZoom)
        {
            double offsetX = 0;
            double offsetY = 0;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = this.PanScrollViewer.HorizontalOffset;
            double scrollOffsetY = this.PanScrollViewer.VerticalOffset;

            double oldX = editor.options.zoomPoint.X * oldZoom;
            double oldY = editor.options.zoomPoint.Y * oldZoom;

            double newX = editor.options.zoomPoint.X * zoom;
            double newY = editor.options.zoomPoint.Y * zoom;

            offsetX = newX - oldX;
            offsetY = newY - oldY;

            //System.Diagnostics.Debug.Print("");
            //System.Diagnostics.Debug.Print("zoomPoint: {0},{1}", Math.Round(zoomPoint.X, 0), Math.Round(zoomPoint.Y, 0));
            //System.Diagnostics.Debug.Print("scrollableWidth/Height: {0},{1}", scrollableWidth, scrollableHeight);
            //System.Diagnostics.Debug.Print("scrollOffsetX/Y: {0},{1}", scrollOffsetX, scrollOffsetY);
            //System.Diagnostics.Debug.Print("oldX/Y: {0},{1}", oldX, oldY);
            //System.Diagnostics.Debug.Print("newX/Y: {0},{1}", newX, newY);
            //System.Diagnostics.Debug.Print("offsetX/Y: {0},{1}", offsetX, offsetY);

            if (scrollableWidth <= 0)
                offsetX = 0.0;

            if (scrollableHeight <= 0)
                offsetY = 0.0;

            PanToOffset(offsetX, offsetY);

            if (adorner != null)
            {
                adorner.Zoom = zoom;
            }
        }

        private void ZoomIn()
        {
            double zoom = ZoomSlider.Value;

            zoom += editor.options.zoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= editor.options.zoomOutFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        #endregion

        #region Zoom Events

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoom = ZoomSlider.Value;

            zoom = Math.Round(zoom, 1);

            if (e.OldValue != e.NewValue)
            {
                Zoom(zoom);
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                return;

            var canvas = editor.options.currentCanvas;

            editor.options.zoomPoint = e.GetPosition(canvas);

            if (e.Delta > 0)
            {
                ZoomIn();

                e.Handled = true;
            }
            else if (e.Delta < 0)
            {
                ZoomOut();

                e.Handled = true;
            }
        }

        #endregion

        #region PanScrollViewer Events

        private void PanScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.panButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.panButton)
            {
                EndPan();
            }
        }

        private void PanScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.PanScrollViewer.IsMouseCaptured == true)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                PanToPoint(point);
            }
        }

        #endregion

        #region Selection Adorner

        private void CreateAdorner(Canvas canvas, Point origin, Point point)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            adorner = new SelectionAdorner(canvas);
            adorner.Zoom = GetZoomScaleTransform().ScaleX;
            adorner.SelectionOrigin = new Point(origin.X, origin.Y);

            adorner.SelectionRect = new Rect(origin, point);

            adorner.SnapsToDevicePixels = false;
            RenderOptions.SetEdgeMode(adorner, EdgeMode.Aliased);

            layer.Add(adorner);
            adorner.InvalidateVisual();
        }

        private void RemoveAdorner(Canvas canvas)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            layer.Remove(adorner);

            adorner = null;
        }

        private void UpdateAdorner(Point point)
        {
            var origin = adorner.SelectionOrigin;
            double width = Math.Abs(point.X - origin.X);
            double height = Math.Abs(point.Y - origin.Y);

            adorner.SelectionRect = new Rect(point, origin);
            adorner.InvalidateVisual();
        }

        #endregion

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = e.GetPosition(canvas);

            if (editor.options.currentRoot == null && editor.options.currentLine == null && editor.options.enableInsertLast == false)
            {
                selectionOrigin = point;

                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    editor.DeselectAll();
                }

                canvas.CaptureMouse();
            }
            else
            {
                editor.HandleLeftDown(canvas, point);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            if (canvas.IsMouseCaptured)
            {
                canvas.ReleaseMouseCapture();

                if (adorner != null)
                {
                    var rect = adorner.SelectionRect;
                    var elements = editor.HitTest(canvas, ref rect);

                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            if (SelectionThumb.GetIsSelected(element) == false)
                            {
                                SelectionThumb.SetIsSelected(element, true);
                            }
                            else
                            {
                                SelectionThumb.SetIsSelected(element, false);
                            }
                        }
                    }

                    RemoveAdorner(canvas);
                }
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editor.options.skipLeftClick == true)
            {
                editor.options.skipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = editor.options.currentCanvas;
            var point = e.GetPosition(canvas);
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = editor.HandlePreviewLeftDown(canvas, point, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            var point = e.GetPosition(canvas);

            if (canvas.IsMouseCaptured)
            {
                if (adorner == null)
                {
                    CreateAdorner(canvas, selectionOrigin, point);
                }

                UpdateAdorner(point);
            }
            else
            {
                editor.HandleMove(canvas, point);
            }
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var path = editor.options.currentPathGrid;

            editor.options.rightClick = e.GetPosition(canvas);

            var result = editor.HandleRightDown(canvas, path);
            if (result == true)
            {
                editor.options.skipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (editor.options.skipContextMenu == true)
            {
                editor.options.skipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                editor.options.skipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertPin(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementPin;
            editor.options.skipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertInput(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementInput;
            editor.options.skipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOutput(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementOutput;
            editor.options.skipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertAndGate(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementAndGate;
            editor.options.skipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOrGate(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementOrGate;
            editor.options.skipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = new Point(editor.options.rightClick.X, editor.options.rightClick.Y);

            editor.Delete(canvas, point);
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = new Point(editor.options.rightClick.X, editor.options.rightClick.Y);

            editor.ToggleStart(canvas, point);
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = new Point(editor.options.rightClick.X, editor.options.rightClick.Y);

            editor.ToggleEnd(canvas, point);
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableHistory = EnableHistory.IsChecked == true ? true : false;

            if (editor.options.enableHistory == false)
            {
                var canvas = editor.options.currentCanvas;

                editor.ClearHistory(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            editor.options.snapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        }

        private void EnablePageGrid_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enablePageGrid = EnablePageGrid.IsChecked == true ? true : false;
        }

        private void EnablePageTemplate_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enablePageTemplate = EnablePageTemplate.IsChecked == true ? true : false;
        }

        #endregion

        #region Button Events

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            //var diagram = editor.Generate();
            //this.TextModel.Text = diagram;

            var solution = GenerateSolution();

            this.TextModel.Text = solution;
        }

        private void GenerateModelFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.GenerateFromSelected();

            this.TextModel.Text = diagram;
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            editor.Insert(diagram, offsetX, offsetY);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            GenerateGrid(true);
        }

        #endregion

        #region Main Menu Events

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            NewSolution();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSolution();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            var model = GenerateSolution();

            editor.SaveSolution(model);
        }

        private void FileOpenDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.OpenDiagram();
        }

        private void FileSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.SaveDiagram();
        }

        private void FileImport_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.Import();

            if (diagram != null)
            {
                this.TextModel.Text = diagram;
            }
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportDiagram();
        }

        private void FileExportHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportDiagramHistory();
        }

        private void FilePrint_Click(object sender, RoutedEventArgs e)
        {
            editor.Print();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            editor.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            editor.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            editor.Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            editor.Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            var point = new Point(0.0, 0.0);

            editor.Paste(point);
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            editor.Delete();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.SelectAll();
        }

        private void EditDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.DeselectAll();
        }

        private void EditClear_Click(object sender, RoutedEventArgs e)
        {
            editor.Clear();
        }

        private void EditOptions_Click(object sender, RoutedEventArgs e)
        {
            TabOptions.IsSelected = true;
        }

        #endregion

        #region Move Elements

        private void MoveLeft(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.enableSnap == true)
            {
                editor.MoveSelectedElements(canvas, -editor.options.defaultGridSize, 0.0, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, -1.0, 0.0, false);
            }
        }

        private void MoveRight(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.enableSnap == true)
            {
                editor.MoveSelectedElements(canvas, editor.options.defaultGridSize, 0.0, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 1.0, 0.0, false);
            }
        }

        private void MoveUp(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.enableSnap == true)
            {
                editor.MoveSelectedElements(canvas, 0.0, -editor.options.defaultGridSize, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 0.0, -1.0, false);
            }
        }

        private void MoveDown(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.enableSnap == true)
            {
                editor.MoveSelectedElements(canvas, 0.0, editor.options.defaultGridSize, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 0.0, 1.0, false);
            }
        }

        #endregion

        #region Window Key Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //System.Diagnostics.Debug.Print("PreviewKeyDown sender: {0}, source: {1}", sender.GetType(), e.OriginalSource.GetType());

            if (!(e.OriginalSource is TextBox) && 
                Keyboard.Modifiers != ModifierKeys.Shift)
            {
                var canvas = editor.options.currentCanvas;
                bool isControl = Keyboard.Modifiers == ModifierKeys.Control;

                switch (e.Key)
                {
                    // add new project to selected solution
                    // add new diagram to selected project
                    // add new diagram after selected diagram and select new diagram
                    case Key.M:
                        {
                            if (isControl == true)
                            {
                                var selected = SolutionTree.SelectedItem as TreeViewItem;

                                string uid = selected.Uid;
                                bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
                                bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
                                bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                                if (isSelectedDiagram == true)
                                {
                                    var project = selected.Parent as TreeViewItem;

                                    AddDiagram(project, true);
                                }
                                else if (isSelectedProject == true)
                                {
                                    AddDiagram(selected, false);
                                }
                                else if (isSelectedSolution == true)
                                {
                                    AddProject(selected);
                                }
                            }
                        }
                        break;

                    // open solution
                    case Key.O:
                        {
                            if (isControl == true)
                            {
                                editor.OpenDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // save solution
                    case Key.S:
                        {
                            if (isControl == true)
                            {
                                editor.SaveDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // new solution
                    case Key.N:
                        {
                            if (isControl == true)
                            {
                                NewSolution();
                            }
                        }
                        break;

                    // import
                    case Key.I:
                        {
                            if (isControl == true)
                            {
                                editor.Import();
                                e.Handled = true;
                            }
                        }
                        break;

                    // export
                    case Key.E:
                        {
                            if (isControl == true)
                            {
                                editor.ExportDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // export history
                    case Key.H:
                        {
                            if (isControl == true)
                            {
                                editor.ExportDiagramHistory();
                                e.Handled = true;
                            }
                        }
                        break;

                    //
                    case Key.P:
                        {
                            if (isControl == true)
                            {
                                editor.Print();
                                e.Handled = true;
                            }
                        }
                        break;

                    // undo
                    case Key.Z:
                        {
                            if (isControl == true)
                            {
                                editor.Undo();
                                e.Handled = true;
                            }
                        }
                        break;

                    // redo
                    case Key.Y:
                        {
                            if (isControl == true)
                            {
                                editor.Redo();
                                e.Handled = true;
                            }
                        }
                        break;

                    // cut
                    case Key.X:
                        {
                            if (isControl == true)
                            {
                                editor.Cut();
                                e.Handled = true;
                            }
                        }
                        break;

                    // copy
                    case Key.C:
                        {
                            if (isControl == true)
                            {
                                editor.Copy();
                                e.Handled = true;
                            }
                        }
                        break;

                    // paste
                    case Key.V:
                        {
                            if (isControl == true)
                            {
                                var point = new Point(0.0, 0.0);
                                editor.Paste(point);
                                e.Handled = true;
                            }
                        }
                        break;

                    // select all
                    case Key.A:
                        {
                            if (isControl == true)
                            {
                                editor.SelectAll();
                                e.Handled = true;
                            }
                        }
                        break;

                    // delete
                    case Key.Delete:
                        {
                            editor.Delete();
                            e.Handled = true;
                        }
                        break;

                    // move up
                    case Key.Up:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveUp(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move down
                    case Key.Down:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveDown(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move left
                    case Key.Left:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveLeft(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move right
                    case Key.Right:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveRight(canvas);
                                e.Handled = true;
                            }
                        }
                        break;
                }
            }
        } 

        #endregion

        #region Solution

        private void SwitchItems(Canvas canvas, TreeViewItem oldItem, TreeViewItem newItem)
        {
            if (newItem == null)
                return;

            string oldUid = oldItem == null ? null : oldItem.Uid;
            string newUid = newItem == null ? null : newItem.Uid;

            bool isOldItemDiagram = oldUid == null ? false : StringUtil.StartsWith(oldUid, ModelConstants.TagHeaderDiagram);
            bool isNewItemDiagram = newUid == null ? false : StringUtil.StartsWith(newUid, ModelConstants.TagHeaderDiagram);

            if (isOldItemDiagram == true)
            {
                // save current model
                StoreModel(canvas, oldItem);
            }

            if (isNewItemDiagram == true)
            {
                // load new model
                LoadModel(canvas, newItem);

                EnablePage.IsChecked = true;
            }
            else
            {
                EnablePage.IsChecked = false;
            }

            System.Diagnostics.Debug.Print("Old Uid: {0}, new Uid: {1}", oldUid, newUid);
        }

        private void LoadModel(Canvas canvas, TreeViewItem item)
        {
            var tag = item.Tag;

            editor.ClearDiagramModel(canvas);

            if (tag != null)
            {
                var diagram = tag as Diagram;

                var model = diagram.Item1;
                var history = diagram.Item2;

                canvas.Tag = history;

                editor.ParseDiagramModel(model, canvas, editor.options.currentPathGrid, 0, 0, false, true, false, true);
            }
            else
            {
                canvas.Tag = new History(new Stack<string>(), new Stack<string>());

                GenerateGrid(false);
            }
        }

        private void StoreModel(Canvas canvas, TreeViewItem item)
        {
            var uid = item.Uid;
            var model = editor.GenerateDiagramModel(canvas, uid);

            if (item != null)
            {
                item.Tag = new Diagram(model, canvas != null ? canvas.Tag as History : null);
            }
        }

        private TreeViewItem CreateSolutionItem(string uid)
        {
            var solution = new TreeViewItem();

            solution.Header = "Solution";
            solution.ContextMenu = this.Resources["SolutionContextMenuKey"] as ContextMenu;
            solution.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.counter;
                int id = 0; // there is only one solution allowed

                solution.Uid = ModelConstants.TagHeaderSolution + ModelConstants.TagNameSeparator + id.ToString();
                counter.SolutionCount = id++;
            }
            else
            {
                solution.Uid = uid;
            }

            solution.IsExpanded = true;

            return solution;
        }

        private TreeViewItem CreateProjectItem(string uid)
        {
            var project = new TreeViewItem();

            project.Header = "Project";
            project.ContextMenu = this.Resources["ProjectContextMenuKey"] as ContextMenu;
            project.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.counter;
                int id = counter.ProjectCount;

                project.Uid = ModelConstants.TagHeaderProject + ModelConstants.TagNameSeparator + id.ToString();
                counter.ProjectCount++;
            }
            else
            {
                project.Uid = uid;
            }

            project.IsExpanded = true;

            return project;
        }

        private TreeViewItem CreateDiagramItem(string uid)
        {
            var diagram = new TreeViewItem();

            diagram.Header = "Diagram";
            diagram.ContextMenu = this.Resources["DiagramContextMenuKey"] as ContextMenu;
            diagram.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.counter;
                int id = counter.DiagramCount;

                diagram.Uid = ModelConstants.TagHeaderDiagram + ModelConstants.TagNameSeparator + id.ToString();
                counter.DiagramCount++; 
            }
            else
            {
                diagram.Uid = uid;
            }

            return diagram;
        }

        private void AddProject(TreeViewItem solution)
        {
            var project = CreateProjectItem(null);

            solution.Items.Add(project);

            System.Diagnostics.Debug.Print("Added project: {0} to solution: {1}", project.Uid, solution.Uid);
        }

        private void AddDiagram(TreeViewItem project, bool select)
        {
            var diagram = CreateDiagramItem(null);

            project.Items.Add(diagram);

            StoreModel(null, diagram);

            if (select == true)
            {
                diagram.IsSelected = true;
            }

            System.Diagnostics.Debug.Print("Added diagram: {0} to project: {1}", diagram.Uid, project.Uid);
        }

        private void DeleteSolution(TreeViewItem solution)
        {
            var tree = solution.Parent as TreeView;

            var projects = solution.Items.Cast<TreeViewItem>().ToList();

            foreach (var project in projects)
            {
                var diagrams = project.Items.Cast<TreeViewItem>().ToList();

                foreach (var diagram in diagrams)
                {
                    project.Items.Remove(diagram);
                }

                solution.Items.Remove(project);
            }

            tree.Items.Remove(solution);
        }

        private void DeleteProject(TreeViewItem project)
        {
            var solution = project.Parent as TreeViewItem;

            var diagrams = project.Items.Cast<TreeViewItem>().ToList();

            foreach (var diagram in diagrams)
            {
                project.Items.Remove(diagram);
            }

            solution.Items.Remove(project);
        }

        private void DeleteDiagram(TreeViewItem diagram)
        {
            var project = diagram.Parent as TreeViewItem;

            project.Items.Remove(diagram);
        }

        private void UpdateSelectedDiagramModel()
        {
            var canvas = editor.options.currentCanvas;
            var item = SolutionTree.SelectedItem as TreeViewItem;

            if (item != null)
            {
                string uid = item.Uid;
                bool isDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var model = editor.GenerateDiagramModel(canvas, uid);

                    item.Tag = new Diagram(model, canvas.Tag as History);
                }
            }
        }

        public string GenerateSolution()
        {
            var solution = SolutionTree.Items.Cast<TreeViewItem>().First();
            var projects = solution.Items.Cast<TreeViewItem>();
            string line = null;

            var sb = new StringBuilder();

            // update current diagram
            UpdateSelectedDiagramModel();

            // Solution
            line = string.Format("{0}{1}{2}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                solution.Uid);

            sb.AppendLine(line);

            //System.Diagnostics.Debug.Print(line);

            foreach (var project in projects)
            {
                var diagrams = project.Items.Cast<TreeViewItem>();

                // Project
                line = string.Format("{0}{1}{2}",
                    ModelConstants.PrefixRoot,
                    ModelConstants.ArgumentSeparator,
                    project.Uid);

                sb.AppendLine(line);

                //System.Diagnostics.Debug.Print(line);

                foreach (var diagram in diagrams)
                {
                    // Diagram

                    //line = string.Format("{0}{1}{2}",
                    //    ModelConstants.PrefixRootElement,
                    //    ModelConstants.ArgumentSeparator,
                    //    diagram.Uid);
                    //sb.AppendLine(line);
                    //System.Diagnostics.Debug.Print(line);

                    // Diagram Elements
                    if (diagram.Tag != null)
                    {
                        var _diagram = diagram.Tag as Diagram;

                        var model = _diagram.Item1;
                        var history = _diagram.Item2;

                        sb.Append(model);
                    }
                }
            }

            return sb.ToString();
        }

        public void OpenSolution()
        {
            var solution = editor.OpenSolution();

            if (solution != null)
            {
                var tree = SolutionTree;
                TreeViewItem firstDiagram = null;
                bool haveFirstDiagram = false;

                ClearSolutionTree(tree);

                var counter = editor.options.counter;

                // create solution
                string name = null;
                int id = -1;

                name = solution.Item1;
                var projects = solution.Item2.Reverse();

                //System.Diagnostics.Debug.Print("Solution: {0}", name);

                var solutionItem = CreateSolutionItem(name);
                tree.Items.Add(solutionItem);

                // create projects
                foreach (var project in projects)
                {
                    name = project.Item1;
                    var diagrams = project.Item2.Reverse();

                    //System.Diagnostics.Debug.Print("Project: {0}", name);

                    // create project
                    var projectItem = CreateProjectItem(name);
                    solutionItem.Items.Add(projectItem);

                    // update project count
                    id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                    counter.ProjectCount = Math.Max(counter.ProjectCount, id + 1);

                    // create diagrams
                    foreach (var diagram in diagrams)
                    {
                        var lines = diagram.Reverse();
                        var sb = new StringBuilder();
                        string model = null;

                        var firstLine = lines.First().Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                            StringSplitOptions.RemoveEmptyEntries);

                        name = firstLine.Length >= 1 ? firstLine[1] : null;

                        // create diagram
                        foreach (var line in lines)
                        {
                            sb.AppendLine(line);
                        }

                        model = sb.ToString();

                        //System.Diagnostics.Debug.Print(model);

                        var diagramItem = CreateDiagramItem(name);

                        diagramItem.Tag = new Diagram(model, null);

                        projectItem.Items.Add(diagramItem);

                        // check for first diagram
                        if (haveFirstDiagram == false)
                        {
                            firstDiagram = diagramItem;
                            haveFirstDiagram = true;
                        }

                        // update diagram count
                        id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                        counter.DiagramCount = Math.Max(counter.DiagramCount, id + 1);
                    }
                }

                // select first diagram in tree
                if (haveFirstDiagram == true)
                {
                    firstDiagram.IsSelected = true;
                }
            }
        }

        private void ClearSolutionTree(TreeView tree)
        {
            // clear solution tree
            var items = tree.Items.Cast<TreeViewItem>().ToList();

            foreach (var item in items)
            {
                DeleteSolution(item);
            }

            // reset counter
            editor.options.counter.ResetAll();
        }

        public void NewSolution()
        {
            var canvas = DiagramCanvas;
            var tree = SolutionTree;

            editor.ClearDiagramModel(canvas);

            ClearSolutionTree(tree);

            var solutionItem = CreateSolutionItem(null);
            tree.Items.Add(solutionItem);

            var projectItem = CreateProjectItem(null);
            solutionItem.Items.Add(projectItem);

            var diagramItem = CreateDiagramItem(null);
            projectItem.Items.Add(diagramItem);

            diagramItem.IsSelected = true;
        }

        #endregion

        #region TreeView Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                //item.Focus();
                item.BringIntoView();

                e.Handled = true;
            }
        }

        private void SolutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (editor == null)
                return;

            var canvas = editor.options.currentCanvas;

            var oldItem = e.OldValue as TreeViewItem;
            var newItem = e.NewValue as TreeViewItem;

            SwitchItems(canvas, oldItem, newItem);
        }

        private void SolutionAddProject_Click(object sender, RoutedEventArgs e)
        {
            var solution = SolutionTree.SelectedItem as TreeViewItem;

            AddProject(solution);
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            AddDiagram(project, true);
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            DeleteProject(project);
        }

        private void ProjectDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            var diagram = SolutionTree.SelectedItem as TreeViewItem;

            DeleteDiagram(diagram);
        }

        #endregion
    }

    #endregion
}
