﻿# CanvasDiagramEditor

  Logic diagram editor written in WPF using Canvas panel.

## About

  CanvasDiagramEditor is a small program for making simple logic diagrams
  that are extremely simple and elegant. Be sure to view the `./Examples`.

## Example

  Below you can find sample solution model `solution0.txt` created by editor:

    +;Solution|0;tags0.txt
    +;Project|0
    +;Diagram|0;1260;891;330;31;600;750;30;15;15;0;1
        +;Input|0;30;181;0
            -;Wire|0;Start
        +;Input|1;30;241;1
            -;Wire|2;Start
        +;AndGate|0;420;391
            -;Wire|4;End
            -;Wire|5;Start
        +;Wire|0;315;196;435;196;False;True;True;False
        +;Pin|0;435;196
            -;Wire|0;End
            -;Wire|3;Start
        +;Wire|2;315;256;435;256;False;False;True;False
        +;Pin|1;435;256
            -;Wire|2;End
            -;Wire|3;End
            -;Wire|4;Start
        +;Wire|3;435;196;435;256;False;False;False;False
        +;Wire|4;435;256;435;391;False;False;False;False
        +;Output|0;930;391;3
            -;Wire|5;End
        +;Wire|5;450;406;930;406;False;False;False;True
        +;Input|2;30;481;5
            -;Wire|7;Start
        +;Input|3;30;571;6
            -;Wire|6;Start
        +;OrGate|0;420;571
            -;Wire|6;End
            -;Wire|8;End
            -;Wire|9;Start
        +;Output|1;930;571;8
            -;Wire|9;End
        +;Wire|6;315;586;420;586;False;False;True;False
        +;Wire|7;315;496;435;496;False;False;True;False
        +;Pin|2;435;496
            -;Wire|7;End
            -;Wire|8;Start
        +;Wire|8;435;496;435;571;False;False;False;False
        +;Wire|9;450;586;930;586;True;False;False;True

  in result program produces the following diagram: <img src="http://i43.tinypic.com/nbzsp5.png" border="0">

  Tags are stored in external `tags0.txt` file:

    0;Designation0;Signal0;Condition0;Description0
    1;Designation1;Signal1;Condition1;Description1
    2;Designation2;Signal2;Condition2;Description2
    3;Designation3;Signal3;Condition3;Description3
    4;Designation4;Signal4;Condition4;Description4
    5;Designation5;Signal5;Condition5;Description5
    6;Designation6;Signal6;Condition6;Description6
    7;Designation7;Signal7;Condition7;Description7
    8;Designation8;Signal8;Condition8;Description8
    9;Designation9;Signal9;Condition9;Description9

## Main functions

  Built-in graphical logic diagram editor.
  
  Basic functionality includes:

    Create new solutions
    Create new and delete projects
    Create new and delete diagrams
    Load and save solutions
    Load and save diagrams
    Load and save tags
    Import and export tags
    Drag & drop Tags on canvas to create new Input/Output
    Drag & drop Tags on Input/Output to change associated tag
    Export diagrams to .DXF file format (compatible with CAD applications)
    Print solutions
    Undo and redo any change made (individually for each diagram)
    Cut, copy, paste and delete any element
    Select all and none elements
    Edit and create new tags using built-in Tag Editor
    Use program with Keyboard shortcuts
    Zoom in and out using mouse wheel
    Pan using mouse middle button
    Create new elements using mouse right click (Context Menu)
    Select any element using selection rectable (mouse left click)
    Mode element single element or all selected elements using mouse or arrow keys

  Currently supported logic elements:

    Wire
    AND Gate
    OR Gate
    Input Signal
    Output Signal

  Input and output signals can be associated with external Tags.
  
  Tag contains simple text metadata. You can create tag files in spreadsheet program and 
  import them into editor or create by using built-in Tag Editor.
  
  Data model is based on simple text syntax. You can cut and paste any part of diagram or entire diagram
  from clipboard. You can select part of diagram and generate model from it.

  Keyboard shortcuts:

    Ctrl+O      -> open solution
    Ctrl+S      -> save solution
    Ctrl+N      -> new solution
    Ctrl+T      -> open tags
    Ctrl+I      -> import tags
    Ctrl+R      -> reset tags
    Ctrl+E      -> export to dxf
    Ctrl+P      -> print solution
    Ctrl+Z      -> undo
    Ctrl+Y      -> redo
    Ctrl+X      -> cut elements to clipboard
    Ctrl+C      -> copy elements to clipboard
    Ctrl+V      -> paste elements from clipboard
    Ctrl+A      -> select all elements
    '['         -> select previous element
                   - use control Key to select many element
    ']'         -> select next element
                   - use control Key to select many element
    '|'         -> select connected elements
    Esc         -> deselect all/cancel connection/hide guides
    Del         -> delete selected elements
    Up Arrow    -> move selected elements/line guides up
    Down Arrow  -> move selected elements/line guides down
    Left Arrow  -> move selected elements/line guides left
    Right Arrow -> move selected elements/line guides right
    I           -> insert Input
    O           -> insert Output
    R           -> insert OrGate
    A           -> insert AndGate
    S           -> invert wire start
    E           -> invert wire end
    C           -> connect pins
    G           -> toggle guides
    Ctrl+J      -> insert new solution tree item and paste from clipboard
                   - add new project to selected solution
                   - add new diagram to selected project
                   - add new diagram after selected diagram and select it
    Ctrl+M      -> insert new solution tree item
    '<'         -> select previous solution tree item
    '>'         -> select next solution tree item
    F5          -> show solution explorer tab
    F6          -> show tag editor tab
    F7          -> show table editor tab
    F8          -> show model tab
    F9          -> show options tab
    Ctrl+H      -> show diagram history

## Build

  CanvasDiagramEditor is built with Microsoft Visual Studio Express 2012 for Windows Desktop. 
 
  To run CanvasDiagramEditor you need to have installed Microsoft .NET version 3.5.

## License 

    Copyright (C) Wiesław Šoltés 2013. 
    All Rights Reserved
