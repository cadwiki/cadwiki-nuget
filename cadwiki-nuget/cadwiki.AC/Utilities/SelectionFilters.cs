using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace cadwiki.AC.Utilities
{

    public class SelectionFilters
    {
        private static TypedValue startOr = new TypedValue((int)DxfCode.Operator, "<or");
        private static TypedValue endOr = new TypedValue((int)DxfCode.Operator, "or>");
        private static TypedValue startAnd = new TypedValue((int)DxfCode.Operator, "<and");
        private static TypedValue endAnd = new TypedValue((int)DxfCode.Operator, "and>");

        public static SelectionFilter GetAllLineEntitiesOnLayers(List<string> layers)
        {

            var typedValues = new List<TypedValue>();

            typedValues.Add(startAnd);
            typedValues.Add(startOr);
            foreach (string layerName in layers)
                typedValues.Add(new TypedValue(8, layerName));
            typedValues.Add(endOr);
            var lines = new TypedValue(0, "LINE");
            typedValues.Add(lines);
            typedValues.Add(endAnd);
            var filter = new SelectionFilter(typedValues.ToArray());
            return filter;
        }

        public static SelectionFilter GetAllLineEntities()
        {
            var lines = new TypedValue(0, "LINE");
            TypedValue[] typedValues = new[] { lines };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }

        public static SelectionFilter GetAllLineEntitiesOnLayer(string layerNameStr)
        {
            var lwPolyLine = new TypedValue(0, "LINE");
            var layerName = new TypedValue(8, layerNameStr);
            TypedValue[] typedValues = new[] { lwPolyLine, layerName };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }

        public static SelectionFilter GetAllLineBasedEntitiesOnLayer(string layerName)
        {
            var startAnd = new TypedValue((int)DxfCode.Operator, "<and");
            var startOr = new TypedValue((int)DxfCode.Operator, "<or");
            var line = new TypedValue(0, "LINE");
            var polyLine = new TypedValue(0, "POLYLINE");
            var arc = new TypedValue(0, "ARC");
            var lwPolyLine = new TypedValue(0, "LWPOLYLINE");
            var endOr = new TypedValue((int)DxfCode.Operator, "or>");
            var layer = new TypedValue(8, layerName);
            var endAnd = new TypedValue((int)DxfCode.Operator, "and>");
            TypedValue[] typedValues = new[] { startAnd, startOr, line, polyLine, arc, lwPolyLine, endOr, layer, endAnd };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }

        public static SelectionFilter GetAllLineBasedEntities()
        {
            var startOr = new TypedValue((int)DxfCode.Operator, "<or");
            var line = new TypedValue(0, "LINE");
            var polyLine = new TypedValue(0, "POLYLINE");
            var arc = new TypedValue(0, "ARC");
            var lwPolyLine = new TypedValue(0, "LWPOLYLINE");
            var endOr = new TypedValue((int)DxfCode.Operator, "or>");
            TypedValue[] typedValues = new[] { startOr, line, polyLine, arc, lwPolyLine, endOr };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }

        public static SelectionFilter LwPoyllinesByLayer(string layerNameStr)
        {
            var lwPolyLine = new TypedValue(0, "LWPOLYLINE");
            var layerName = new TypedValue(8, layerNameStr);

            TypedValue[] typedValues = new[] { lwPolyLine, layerName };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }

        public static SelectionFilter BlocksByName(string blockNameStr)
        {
            var blockInsert = new TypedValue(0, "INSERT");
            var blockName = new TypedValue(2, blockNameStr);

            TypedValue[] typedValues = new[] { blockInsert, blockName };
            var filter = new SelectionFilter(typedValues);
            return filter;
        }
    }
}