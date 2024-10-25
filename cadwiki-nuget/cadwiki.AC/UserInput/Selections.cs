using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.AC.UserInputs
{
    public class Selections
    {
        public static PromptSelectionResult LetUserSelectBlocks(string msg)
        {
            // Define a selection filter for polylines
            TypedValue[] filterList = new TypedValue[]
            {
                    new TypedValue((int)DxfCode.Operator, "<OR"),
                    new TypedValue(0, "INSERT"),
                    new TypedValue((int)DxfCode.Operator, "OR>"),
            };

            var doc = Application.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;

            Database db = doc.Database;
            Editor ed = doc.Editor;
            var ppo = new PromptSelectionOptions();
            ppo.MessageForAdding = msg;
            SelectionFilter filter = new SelectionFilter(filterList);
            PromptSelectionResult selectionResult = ed.GetSelection(ppo, filter);
            return selectionResult;
        }
    }
}
