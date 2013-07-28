# CanvasDiagramEditor

  Logic diagram editor written in WPF using Canvas panel.

## About

  CanvasDiagramEditor is a small program for making logic diagrams
  that are extremely simple and elegant. Be sure to view the `./Examples`.

## Example

  For example below you can find produced by editor sample solution (all done in GUI):

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

## Main functions

  Built-in graphical logic diagram editor.
  
  Basic functionality includes:

        Create new solution
        Create new and Delete project
        Create new and Delete diagrams
        Load and Save solutions
        Load and Save diagrams
        Load/Save tags
        Import/Export tags
        Export diagram to .DXF format (compatible with many CAD applications)
        Print solution
        Import model
        Undo and Redo any changes individually for each diagram
        Cut/Copy/Paste and Delete elements
        Select All and None elements
        Edit and Create New tags using built-in Tag Editor
        Keyboard shortcuts
        Zoom In and Out using mouse wheel

  Currently supported logic elements:

        Wire
        AND Gate
        OR Gate
        Input Signal
        Output Signal

  All Input and Output signals can be associated with external Tags.
  Tag contains simple text metadata.
  
  Data model is based on simple text syntax. You can cut & paste form clipboard parts of diagrams
  or you entire diagrams. You can select part of diagram and generate model from selection.

## Build

 CanvasDiagramEditor is built in Microsoft Visual Studio Express 2012 for Windows Desktop. 
 
 To run CanvasDiagramEditor you need to have installed Microsoft .NET version 3.5.

## License 

Copyright (C) Wiesław Šoltés 2013. 
All Rights Reserved
