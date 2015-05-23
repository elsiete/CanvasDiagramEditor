// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Core.Model;
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

    #region HistoryEditor

    public static class HistoryEditor
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
            var history = canvas.GetTag();
            if (history == null)
            {
                history = new UndoRedo(new Stack<string>(), new Stack<string>());
                canvas.SetTag(history);
            }

            return history as UndoRedo;
        }

        public static string Add(ICanvas canvas)
        {
            var history = Get(canvas);
            var undo = history.Undo;
            var redo = history.Redo;

            var model = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());

            undo.Push(model);
            redo.Clear();

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs() 
            {
                Canvas = canvas, 
                Undo = undo, 
                Redo = redo 
            });

            return model;
        }

        public static void Clear(ICanvas canvas)
        {
            var history = Get(canvas);
            var undo = history.Undo;
            var redo = history.Redo;

            undo.Clear();
            redo.Clear();

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undo,
                Redo = redo
            });
        }

        public static void Undo(ICanvas canvas, IDiagramCreator creator, bool pushRedo)
        {
            var history = Get(canvas);
            var undo = history.Undo;
            var redo = history.Redo;

            if (undo.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());
                redo.Push(current);
            }

            // restore previous model
            var model = undo.Pop();

            ModelEditor.Clear(canvas);
            ModelEditor.Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undo,
                Redo = redo
            });
        }

        public static void Redo(ICanvas canvas, IDiagramCreator creator, bool pushUndo)
        {
            var history = Get(canvas);
            var undo = history.Undo;
            var redo = history.Redo;

            if (redo.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());
                undo.Push(current);
            }

            // restore previous model
            var model = redo.Pop();

            ModelEditor.Clear(canvas);
            ModelEditor.Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);

            NotifyCanvasHistoryChanged(new CanvasHistoryChangedEventArgs()
            {
                Canvas = canvas,
                Undo = undo,
                Redo = redo
            });
        } 
        
        #endregion
    }

    #endregion
}
