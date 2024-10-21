using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic;

namespace cadwiki.AC.Utilities
{

    public class UserInput
    {
        public class PointFromUser
        {
            public Point3d Point;
            public bool Success;
        }

        public static ObjectId GetEntityFromUser(Document doc, PromptEntityOptions promptEntityOptions)
        {
            var entityResult = doc.Editor.GetEntity(promptEntityOptions);
            var id = entityResult.ObjectId;
            if (entityResult.Status == PromptStatus.Cancel)
            {
                return default;
            }
            return id;
        }

        public static PointFromUser GetPointFromUser(Document doc, PromptPointOptions promptPointOptions)
        {
            var promptPointResult = doc.Editor.GetPoint(promptPointOptions);
            var point3d = promptPointResult.Value;
            var pointFromUser = new PointFromUser();
            pointFromUser.Point = point3d;
            pointFromUser.Success = true;
            if (promptPointResult.Status == PromptStatus.Cancel)
            {
                pointFromUser.Success = false;
            }
            return pointFromUser;
        }

        public static SelectionSet GetSelectionFromUser(Document doc, string message)
        {
            doc.Editor.WriteMessage(Environment.NewLine + message + Environment.NewLine);
            var promptSelectionResult = doc.Editor.GetSelection();
            var selectionSet = promptSelectionResult.Value;
            if (promptSelectionResult.Status == PromptStatus.Cancel)
            {
                return null;
            }
            return selectionSet;
        }

        public static SelectionSet GetSelectionFromUser(Document doc, SelectionFilter filter, string message)
        {
            doc.Editor.WriteMessage(Environment.NewLine + message + Environment.NewLine);
            var promptSelectionResult = doc.Editor.GetSelection(filter);
            var selectionSet = promptSelectionResult.Value;
            if (promptSelectionResult.Status == PromptStatus.Cancel)
            {
                return null;
            }
            return selectionSet;
        }


        public static int GetIntegerFromUser(Document doc, PromptDoubleOptions options)
        {
            var promptDoubleResult = doc.Editor.GetDouble(options);
            if (promptDoubleResult.Status == PromptStatus.OK)
            {
                double value = promptDoubleResult.Value;
                int @int = (int)Math.Round(value);
                return @int;
            }
            return default;
        }
    }
}