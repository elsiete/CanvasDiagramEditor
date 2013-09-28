// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

#endregion

namespace CanvasDiagram.Core
{
    #region Aliases

    //using MapPin = Tuple<string, string>;
    //using MapWire = Tuple<object, object, object>;
    //using MapWires = Tuple<object, List<Tuple<string, string>>>;
    //using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    //using UndoRedo = Tuple<Stack<string>, Stack<string>>;
    //using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    //using TreeDiagram = Stack<string>;
    //using TreeDiagrams = Stack<Stack<string>>;
    //using TreeProject = Tuple<string, Stack<Stack<string>>>;
    //using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    //using TreeSolution = Tuple<string, string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    //using Position = Tuple<double, double>;
    //using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    //using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    //using Solution = Tuple<string, IEnumerable<string>>;

    #endregion

    #region IDiagramParser

    public interface IDiagramParser
    {
        TreeSolution Parse(string model, IDiagramCreator creator, ParseOptions options);
    } 

    #endregion
}
