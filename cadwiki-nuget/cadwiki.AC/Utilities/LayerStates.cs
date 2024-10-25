using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC.Utilities
{

    public class LayerStates
    {
        // CType(LayerStateMasks.Color + LayerStateMasks.LineType, LayerStateMasks)
        public static bool Create(Document doc, string layerStateName, LayerStateMasks layerStateMasks)
        {
            try
            {
                using (var @lock = doc.LockDocument())
                {
                    var layerStateManager = doc.Database.LayerStateManager;
                    string tempLayerStateName = "";
                    int leaveMissingLayersUnchanged = 0;
                    if (layerStateManager.HasLayerState(layerStateName) == true)
                    {
                        tempLayerStateName = CreateFirstAvailableLayerStateName(doc, "tempLayerState", layerStateMasks);
                        layerStateManager.RestoreLayerState(tempLayerStateName, ObjectId.Null, leaveMissingLayersUnchanged, layerStateMasks);
                        layerStateManager.DeleteLayerState(layerStateName);
                    }
                    layerStateManager.SaveLayerState(layerStateName, layerStateMasks, ObjectId.Null);
                    layerStateManager.RestoreLayerState(layerStateName, ObjectId.Null, leaveMissingLayersUnchanged, layerStateMasks);
                    if (!string.IsNullOrEmpty(tempLayerStateName))
                    {
                        layerStateManager.DeleteLayerState(tempLayerStateName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        public static string CreateFirstAvailableLayerStateName(Document doc, string layerStateName, LayerStateMasks layerStateMasks)
        {
            int i = 0;
            string currentLayerStateName = layerStateName;
            try
            {
                using (var @lock = doc.LockDocument())
                {
                    var layerStateManager = doc.Database.LayerStateManager;
                    bool layerStateExists = layerStateManager.HasLayerState(layerStateName);
                    while (layerStateExists)
                    {
                        i = i + 1;
                        currentLayerStateName = layerStateName + "(" + i.ToString() + ")";
                        layerStateExists = layerStateManager.HasLayerState(currentLayerStateName);
                    }
                    layerStateManager.SaveLayerState(layerStateName, layerStateMasks, ObjectId.Null);
                    return currentLayerStateName;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }


        public static string GetLastRestoredLayerState(Document doc)
        {
            int i = 0;
            try
            {
                using (var @lock = doc.LockDocument())
                {
                    var layerStateManager = doc.Database.LayerStateManager;
                    return layerStateManager.LastRestoredLayerState;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static bool Restore(Document doc, string layerState, LayerStateMasks layerStateMasks)
        {
            int i = 0;
            try
            {
                using (var @lock = doc.LockDocument())
                {
                    var layerStateManager = doc.Database.LayerStateManager;
                    if (layerStateManager.HasLayerState(layerState))
                    {
                        int leaveMissingLayersUnchanged = 0;
                        layerStateManager.RestoreLayerState(layerState, ObjectId.Null, leaveMissingLayersUnchanged, layerStateMasks);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
    }
}