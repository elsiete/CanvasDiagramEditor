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
    public class MapPin
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }

        public MapPin(string name, string type)
        {
            Item1 = name;
            Item2 = type;
        }
    }

    public class MapWire
    {
        public object Item1 { get; set; }
        public IElement Item2 { get; set; }
        public IElement Item3 { get; set; }

        public MapWire(object line, IElement start, IElement end)
        {
            Item1 = line;
            Item2 = start;
            Item3 = end;
        }
    }
    
    public class MapWires
    {
        public object Item1 { get; set; }
        public List<MapPin> Item2 { get; set; }

        public MapWires(object element, List<MapPin> map)
        {
            Item1 = element;
            Item2 = map;
        }
    }

    public class Selection
    {
        public bool Item1 { get; set; }
        public List<MapWire> Item2 { get; set; }

        public Selection(bool selected, List<MapWire> map)
        {
            Item1 = selected;
            Item2 = map;
        }
    }

    public class UndoRedo
    {
        public Stack<String> Item1 { get; set; }
        public Stack<String> Item2 { get; set; }

        public UndoRedo(Stack<String> undo, Stack<String> redo)
        {
            Item1 = undo;
            Item2 = redo;
        }
    }

    public class Diagram
    {
        public string Item1 { get; set; }
        public UndoRedo Item2 { get; set; }

        public Diagram(string model, UndoRedo ur)
        {
            Item1 = model;
            Item2 = ur;
        }
    }

    public class TreeDiagram : Stack<string>
    {
    }

    public class TreeDiagrams : Stack<TreeDiagram>
    {
    }

    public class TreeProject
    {
        public string Item1 { get; set; }
        public TreeDiagrams Item2 { get; set; }

        public TreeProject(string name, TreeDiagrams diagrams)
        {
            Item1 = name;
            Item2 = diagrams;
        }
    }

    public class TreeProjects : Stack<TreeProject>
    {
    }

    public class TreeSolution
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public string Item3 { get; set; }
        public TreeProjects Item4 { get; set; }

        public TreeSolution(string name, string tagFileName, string tableFileName, TreeProjects projects)
        {
            Item1 = name;
            Item2 = tagFileName;
            Item3 = tableFileName;
            Item4 = projects;
        }
    }

    public class Position
    {
        public double Item1 { get; set; }
        public double Item2 { get; set; }

        public Position(double x, double y)
        {
            Item1 = x;
            Item2 = y;
        }
    }

    public class Connection
    {
        public IElement Item1 { get; set; }
        public List<MapWire> Item2 { get; set; }

        public Connection(IElement element, List<MapWire> map)
        {
            Item1 = element;
            Item2 = map;
        }
    }

    public class Connections : List<Connection>
    {
    }

    public class Solution
    {
        public string Item1 { get; set; }
        public List<string> Item2 { get; set; }

        public Solution(string model, List<string> models)
        {
            Item1 = model;
            Item2 = models;
        }
    }
}
