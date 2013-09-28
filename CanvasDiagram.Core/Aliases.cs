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
    public class Pin
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Pin(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    public class Wire
    {
        public object Line { get; set; }
        public IElement Start { get; set; }
        public IElement End { get; set; }

        public Wire(object line, IElement start, IElement end)
        {
            Line = line;
            Start = start;
            End = end;
        }
    }
    
    public class Child
    {
        public object Element { get; set; }
        public List<Pin> Pins { get; set; }

        public Child(object element, List<Pin> pins)
        {
            Element = element;
            Pins = pins;
        }
    }

    public class Selection
    {
        public bool IsSelected { get; set; }
        public List<Wire> Wires { get; set; }

        public Selection(bool selected, List<Wire> wires)
        {
            IsSelected = selected;
            Wires = wires;
        }
    }

    public class UndoRedo
    {
        public Stack<string> Undo { get; set; }
        public Stack<string> Redo { get; set; }

        public UndoRedo(Stack<string> undo, Stack<string> redo)
        {
            Undo = undo;
            Redo = redo;
        }
    }

    public class Diagram
    {
        public string Model { get; set; }
        public UndoRedo History { get; set; }

        public Diagram(string model, UndoRedo history)
        {
            Model = model;
            History = history;
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
        public string Name { get; set; }
        public TreeDiagrams Diagrams { get; set; }

        public TreeProject(string name, TreeDiagrams diagrams)
        {
            Name = name;
            Diagrams = diagrams;
        }
    }

    public class TreeProjects : Stack<TreeProject>
    {
    }

    public class TreeSolution
    {
        public string Name { get; set; }
        public string TagFileName { get; set; }
        public string TableFileName { get; set; }
        public TreeProjects Projects { get; set; }

        public TreeSolution(string name, string tagFileName, string tableFileName, TreeProjects projects)
        {
            Name = name;
            TagFileName = tagFileName;
            TableFileName = tableFileName;
            Projects = projects;
        }
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Connection
    {
        public IElement Item1 { get; set; }
        public List<Wire> Item2 { get; set; }

        public Connection(IElement element, List<Wire> map)
        {
            Item1 = element;
            Item2 = map;
        }
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
