// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region CanvasHistoryChanged

    public class CanvasHistoryChangedEventArgs : EventArgs
    {
        public ICanvas Canvas { get; set; }
        public Stack<string> Undo { get; set; }
        public Stack<string> Redo { get; set; }
    }

    public delegate void CanvasHistoryChangedEventHandler(object sender, CanvasHistoryChangedEventArgs e);

    #endregion

    #region History

    public static class History
    {
        #region CanvasHistoryChanged

        public static event CanvasHistoryChangedEventHandler CanvasHistoryChanged;

        public static void NotifyCanvasHistoryChanged(CanvasHistoryChangedEventArgs e)
        {
            var handler = CanvasHistoryChanged;
            if (handler != null)
                handler(null, e);
        } 

        #endregion

        #region History

        public static UndoRedo Get(ICanvas canvas)
        {
            var canvasTag = canvas.GetTag();
            if (canvasTag == null)
            {
                canvasTag = new UndoRedo(new Stack<string>(), new Stack<string>());
                canvas.SetTag(canvasTag);
            }

            var tuple = canvasTag as UndoRedo;

            return tuple;
        }

        public static string Add(ICanvas canvas)
        {
            var tuple = Get(canvas);
            var undoHistory = tuple.Undo;
            var redoHistory = tuple.Redo;

            var model = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());

            undoHistory.Push(model);
            redoHistory.Clear();

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs() 
            {
                Canvas = canvas, 
                Undo = undoHistory, 
                Redo = redoHistory 
            });

            return model;
        }

        public static void Clear(ICanvas canvas)
        {
            var tuple = Get(canvas);
            var undoHistory = tuple.Undo;
            var redoHistory = tuple.Redo;

            undoHistory.Clear();
            redoHistory.Clear();

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undoHistory,
                Redo = redoHistory
            });
        }

        public static void Undo(ICanvas canvas, IDiagramCreator creator, bool pushRedo)
        {
            var tuple = Get(canvas);
            var undoHistory = tuple.Undo;
            var redoHistory = tuple.Redo;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ModelEditor.Clear(canvas);
            ModelEditor.Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undoHistory,
                Redo = redoHistory
            });
        }

        public static void Redo(ICanvas canvas, IDiagramCreator creator, bool pushUndo)
        {
            var tuple = Get(canvas);
            var undoHistory = tuple.Undo;
            var redoHistory = tuple.Redo;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();

            ModelEditor.Clear(canvas);
            ModelEditor.Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undoHistory,
                Redo = redoHistory
            });
        } 
        
        #endregion
    }

    #endregion
}
