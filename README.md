# CanvasDiagramEditor

  Logic diagram editor written in WPF using Canvas panel.

## About

  CanvasDiagramEditor is a small program for making logic diagrams
  that are extremely simple and elegant. Be sure to view the `./Examples`.

## Example

  For example below you can find produced by editor sample solution model - all done using GUI:

    +;Solution|0;
    +;Project|0
    +;Diagram|0;1260;891;330;35;600;750;30;15;15;0;5
        +;Input|0;45;275;-1
            -;Wire|0;Start
        +;Input|1;45;395;-1
            -;Wire|2;Start
        +;Output|0;930;395;-1
            -;Wire|3;End
        +;AndGate|0;480;395
            -;Wire|1;End
            -;Wire|2;End
            -;Wire|3;Start
        +;Wire|0;330;290;495;290;False;False;True;False
        +;Pin|0;495;290
            -;Wire|0;End
            -;Wire|1;Start
        +;Wire|1;495;290;495;395;False;False;False;False
        +;Wire|2;330;410;480;410;False;False;True;False
        +;Wire|3;510;410;930;410;False;False;False;True

  in result program produces the following diagram: <a href="http://tinypic.com?ref=11vsdud" target="_blank"><img src="http://i40.tinypic.com/11vsdud.jpg" border="0" alt="Image and video hosting by TinyPic"></a>

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
    Export diagrams to .DXF file format (compatible with CAD applications)
    Print solutions
    Import models
    Undo and redo any change made (individually for each diagram)
    Cut, copy, paste and delete any elements
    Select all and none elements
    Edit and create new tags using built-in Tag Editor
    Use program with Keyboard shortcuts
    Zoom in and out using mouse wheel
    Pan using mouse middle button
    Create new elements using mouse right click (Context Menu)
    Select any element(s) using selection rectable (mouse left click)
    Mode element single element or all selected elements using mouse or arrow keys

  Currently supported logic elements:

    Wire
    AND Gate
    OR Gate
    Input Signal
    Output Signal

  Input and output signals can be associated with external Tags.
  
  Tag contains simple text metadata. You can create tag files in spreadsheet program and 
  import them to editor or using built-in Tag Editor.
  
  Data model is based on very simple text syntax. You can cut & paste form clipboard any parts 
  of diagrams or even entire diagrams. You can select part of diagram and generate model from selection.

## Build

  CanvasDiagramEditor is built with Microsoft Visual Studio Express 2012 for Windows Desktop. 
 
  To run CanvasDiagramEditor you need to have installed Microsoft .NET version 3.5.

## License 

Copyright (C) Wiesław Šoltés 2013. 
All Rights Reserved
