using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using acadEx = Autodesk.AutoCAD.Runtime.Exception;
using OpenMode = Autodesk.AutoCAD.DatabaseServices.OpenMode;
using Exception = System.Exception;
using Autodesk.AutoCAD.GraphicsInterface;

namespace cadwiki.AC.Utilities
{
    /// <summary>
    /// Represents information about a block definition in the drawing
    /// </summary>
    public class BlockDefinitionInfo
    {
        public string Name { get; set; }
        public bool IsDynamic { get; set; }
        public bool HasAttributes { get; set; }
        public int ReferenceCount { get; set; }
        public bool IsLayout { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsSelected { get; set; } = true;
    }

    /// <summary>
    /// Represents information about a block reference to be replaced
    /// </summary>
    public class BlockReplaceInfo
    {
        public ObjectId BlockId { get; set; }
        public string BlockName { get; set; }
        public string EffectiveBlockName { get; set; }
        public Point3d Position { get; set; }
        public double Rotation { get; set; }
        public Scale3d Scale { get; set; }
        public string Layer { get; set; }
        public Dictionary<string, string> AttributeValues { get; set; }
        public bool IsDynamicBlock { get; set; }
        public bool IsReplaceChecked { get; set; } = true;
        public bool IsGridHighlighted { get; set; } = true;
        public bool IsDocumentHighlighted { get; set; } = true;

        public BlockReplaceInfo()
        {
            AttributeValues = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Result of a block replacement operation
    /// </summary>
    public class BlockReplaceResult
    {
        public bool Success { get; set; }
        public string OriginalBlockName { get; set; }
        public string NewBlockName { get; set; }
        public string Message { get; set; }
        public int AttributesTransferred { get; set; }
        public int AttributesNotMatched { get; set; }
        public int AttributeDefinitionsAdded { get; set; }
    }

    /// <summary>
    /// Settings for block replacement
    /// </summary>
    public class BlockReplaceSettings
    {
        public string ExternalDwgPath { get; set; }
        public bool PreserveScale { get; set; } = true;
        public bool PreserveRotation { get; set; } = true;
        public bool PreserveLayer { get; set; } = true;
        public bool TransferAttributes { get; set; } = true;
        public bool AddMissingAttributeDefinitions { get; set; } = true;
        public double? OverrideScaleX { get; set; }
        public double? OverrideScaleY { get; set; }
        public double? OverrideScaleZ { get; set; }
        public double? OverrideRotation { get; set; }
    }

    public class BlockReplacer
    {
        /// <summary>
        /// Gets all block definitions from the current drawing
        /// </summary>
        public static List<BlockDefinitionInfo> GetBlockDefinitionsFromDrawing(Document doc)
        {
            var blockDefs = new List<BlockDefinitionInfo>();
            if (doc == null) return blockDefs;

            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null)
                    {
                        foreach (ObjectId blockId in blockTable)
                        {
                            BlockTableRecord btr = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                            if (btr != null && !btr.IsAnonymous && !btr.IsLayout && !btr.Name.StartsWith("*"))
                            {
                                var info = new BlockDefinitionInfo
                                {
                                    Name = btr.Name,
                                    IsDynamic = btr.IsDynamicBlock,
                                    HasAttributes = HasAttributeDefinitions(btr, tr),
                                    ReferenceCount = btr.GetBlockReferenceIds(true, false).Count,
                                    IsLayout = btr.IsLayout,
                                    IsAnonymous = btr.IsAnonymous
                                };
                                blockDefs.Add(info);
                            }
                        }
                    }
                    tr.Commit();
                }
            }

            return blockDefs.OrderBy(b => b.Name).ToList();
        }

        /// <summary>
        /// Checks if a block table record has attribute definitions
        /// </summary>
        private static bool HasAttributeDefinitions(BlockTableRecord btr, Transaction tr)
        {
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));
            foreach (ObjectId objId in btr)
            {
                if (objId.ObjectClass.IsDerivedFrom(attDefClass))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets block references by block name from the current drawing
        /// </summary>
        public static List<BlockReplaceInfo> GetBlockReferencesByName(Document doc, string blockName)
        {
            var blockRefs = new List<BlockReplaceInfo>();
            if (doc == null || string.IsNullOrEmpty(blockName)) return blockRefs;

            var db = doc.Database;
            var ed = doc.Editor;

            using (var @lock = doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    // Get the block table record for the specified block name
                    BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (!blockTable.Has(blockName))
                    {
                        return blockRefs;
                    }

                    ObjectId btrId = blockTable[blockName];
                    BlockTableRecord btr = tr.GetObject(btrId, OpenMode.ForRead) as BlockTableRecord;

                    // Get all references to this block
                    ObjectIdCollection refIds = btr.GetBlockReferenceIds(true, true);
                    
                    foreach (ObjectId refId in refIds)
                    {
                        BlockReference blockRef = tr.GetObject(refId, OpenMode.ForRead) as BlockReference;
                        if (blockRef != null)
                        {
                            var info = new BlockReplaceInfo
                            {
                                BlockId = refId,
                                Position = blockRef.Position,
                                Rotation = blockRef.Rotation,
                                Scale = blockRef.ScaleFactors,
                                Layer = blockRef.Layer,
                                IsDynamicBlock = blockRef.IsDynamicBlock,
                                BlockName = blockRef.Name,
                                EffectiveBlockName = GetEffectiveBlockName(tr, blockRef),
                                AttributeValues = GetAttributeValues(tr, blockRef)
                            };
                            blockRefs.Add(info);
                        }
                    }
                    tr.Commit();
                }
            }

            return blockRefs;
        }

        /// <summary>
        /// Gets block information from a selection set
        /// </summary>
        public static List<BlockReplaceInfo> GetBlockInfoFromSelection(Document doc, SelectionSet ss)
        {
            var blockInfos = new List<BlockReplaceInfo>();
            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selectedObj in ss)
                    {
                        if (selectedObj != null && selectedObj.ObjectId.ObjectClass.DxfName == "INSERT")
                        {
                            BlockReference blockRef = tr.GetObject(selectedObj.ObjectId, OpenMode.ForRead) as BlockReference;
                            if (blockRef != null)
                            {
                                var info = new BlockReplaceInfo
                                {
                                    BlockId = selectedObj.ObjectId,
                                    Position = blockRef.Position,
                                    Rotation = blockRef.Rotation,
                                    Scale = blockRef.ScaleFactors,
                                    Layer = blockRef.Layer,
                                    IsDynamicBlock = blockRef.IsDynamicBlock,
                                    BlockName = blockRef.Name,
                                    EffectiveBlockName = GetEffectiveBlockName(tr, blockRef),
                                    AttributeValues = GetAttributeValues(tr, blockRef)
                                };
                                blockInfos.Add(info);
                            }
                        }
                    }
                }
            }

            return blockInfos;
        }

        /// <summary>
        /// Gets the effective name of a block (handles dynamic blocks)
        /// </summary>
        private static string GetEffectiveBlockName(Transaction tr, BlockReference blockRef)
        {
            if (blockRef.IsDynamicBlock)
            {
                BlockTableRecord btr = tr.GetObject(blockRef.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                if (btr != null)
                {
                    return btr.Name;
                }
            }
            return blockRef.Name;
        }

        /// <summary>
        /// Gets all attribute values from a block reference
        /// </summary>
        private static Dictionary<string, string> GetAttributeValues(Transaction tr, BlockReference blockRef)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                try
                {
                    if (!attId.IsErased)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (attRef != null && !string.IsNullOrEmpty(attRef.Tag))
                        {
                            values[attRef.Tag] = attRef.TextString;
                        }
                    }
                }
                catch (acadEx ex)
                {
                    DebugLogger.Instance.LogError("Error reading attribute: " + ex.Message, ex);
                }
            }

            return values;
        }

        /// <summary>
        /// Gets all attribute definitions from a block table record
        /// </summary>
        private static Dictionary<string, AttributeDefinition> GetAttributeDefinitions(Transaction tr, ObjectId blockDefId)
        {
            var attDefs = new Dictionary<string, AttributeDefinition>(StringComparer.OrdinalIgnoreCase);
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));

            BlockTableRecord btr = tr.GetObject(blockDefId, OpenMode.ForRead) as BlockTableRecord;
            if (btr == null) return attDefs;

            foreach (ObjectId objId in btr)
            {
                if (objId.ObjectClass.IsDerivedFrom(attDefClass))
                {
                    AttributeDefinition attDef = tr.GetObject(objId, OpenMode.ForRead) as AttributeDefinition;
                    if (attDef != null && !string.IsNullOrEmpty(attDef.Tag))
                    {
                        attDefs[attDef.Tag] = attDef;
                    }
                }
            }

            return attDefs;
        }

        /// <summary>
        /// Adds missing attribute definitions from the old block to the new block definition
        /// </summary>
        private static int AddMissingAttributeDefinitions(Transaction tr, ObjectId oldBlockDefId, ObjectId newBlockDefId, 
            Dictionary<string, string> oldAttributeValues)
        {
            int addedCount = 0;
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));

            // Get attribute definitions from both blocks
            var oldAttDefs = GetAttributeDefinitions(tr, oldBlockDefId);
            var newAttDefs = GetAttributeDefinitions(tr, newBlockDefId);

            // Get the new block definition for writing
            BlockTableRecord newBtr = tr.GetObject(newBlockDefId, OpenMode.ForWrite) as BlockTableRecord;
            if (newBtr == null) return addedCount;

            // Find and add missing attribute definitions
            foreach (var kvp in oldAttDefs)
            {
                string tag = kvp.Key;
                AttributeDefinition oldAttDef = kvp.Value;

                // Check if this attribute definition already exists in new block
                if (!newAttDefs.ContainsKey(tag))
                {
                    // Clone the attribute definition
                    AttributeDefinition newAttDef = oldAttDef.Clone() as AttributeDefinition;
                    if (newAttDef != null)
                    {
                        // Set the default value from the attribute values if available
                        if (oldAttributeValues != null && oldAttributeValues.ContainsKey(tag))
                        {
                            newAttDef.TextString = oldAttributeValues[tag];
                        }

                        // Add to the new block definition
                        newBtr.AppendEntity(newAttDef);
                        tr.AddNewlyCreatedDBObject(newAttDef, true);
                        addedCount++;
                    }
                }
            }

            return addedCount;
        }

        /// <summary>
        /// Synchronizes attributes on all block references after adding new attribute definitions
        /// </summary>
        private static void SynchronizeAttributesOnReferences(Transaction tr, ObjectId blockDefId)
        {
            BlockTableRecord btr = tr.GetObject(blockDefId, OpenMode.ForRead) as BlockTableRecord;
            if (btr == null) return;

            // Get all attribute definitions
            var attDefs = GetAttributeDefinitions(tr, blockDefId);

            // Get all block references
            ObjectIdCollection refIds = btr.GetBlockReferenceIds(true, true);
            foreach (ObjectId refId in refIds)
            {
                BlockReference blockRef = tr.GetObject(refId, OpenMode.ForWrite) as BlockReference;
                if (blockRef == null) continue;

                // Get existing attribute tags
                var existingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (ObjectId attId in blockRef.AttributeCollection)
                {
                    if (!attId.IsErased)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (attRef != null)
                        {
                            existingTags.Add(attRef.Tag);
                        }
                    }
                }

                // Add missing attribute references
                foreach (var kvp in attDefs)
                {
                    string tag = kvp.Key;
                    AttributeDefinition attDef = kvp.Value;

                    if (!existingTags.Contains(tag) && !attDef.Constant)
                    {
                        AttributeReference attRef = new AttributeReference();
                        attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                        attRef.Position = attDef.Position.TransformBy(blockRef.BlockTransform);
                        attRef.TextString = attDef.TextString;
                        blockRef.AttributeCollection.AppendAttribute(attRef);
                        tr.AddNewlyCreatedDBObject(attRef, true);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces a single block with a new block from external file
        /// </summary>
        public static BlockReplaceResult ReplaceBlock(Document doc, BlockReplaceInfo blockInfo, BlockReplaceSettings settings)
        {
            var result = new BlockReplaceResult
            {
                OriginalBlockName = blockInfo.EffectiveBlockName
            };

            try
            {
                var db = doc.Database;
                using (var @lock = doc.LockDocument())
                {
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        // Get the original block reference
                        BlockReference originalBlock = tr.GetObject(blockInfo.BlockId, OpenMode.ForWrite) as BlockReference;
                        if (originalBlock == null)
                        {
                            result.Success = false;
                            result.Message = "Could not open original block reference.";
                            return result;
                        }

                        // Get the old block definition ID before erasing
                        ObjectId oldBlockDefId = originalBlock.BlockTableRecord;

                        // Capture the position and other properties
                        Point3d position = blockInfo.Position;
                        double rotation = settings.OverrideRotation.HasValue ? settings.OverrideRotation.Value :
                            (settings.PreserveRotation ? blockInfo.Rotation : 0);

                        Scale3d scale;
                        if (settings.OverrideScaleX.HasValue || settings.OverrideScaleY.HasValue || settings.OverrideScaleZ.HasValue)
                        {
                            scale = new Scale3d(
                                settings.OverrideScaleX ?? blockInfo.Scale.X,
                                settings.OverrideScaleY ?? blockInfo.Scale.Y,
                                settings.OverrideScaleZ ?? blockInfo.Scale.Z
                            );
                        }
                        else if (settings.PreserveScale)
                        {
                            scale = blockInfo.Scale;
                        }
                        else
                        {
                            scale = new Scale3d(1, 1, 1);
                        }

                        string layer = settings.PreserveLayer ? blockInfo.Layer : "0";

                        // Import the new block definition
                        string newBlockName = SymbolUtilityServices.GetBlockNameFromInsertPathName(settings.ExternalDwgPath);

                        // Import block definition from external file
                        ObjectId newBlockDefId = ImportBlockDefinition(doc, db, tr, settings.ExternalDwgPath, newBlockName);
                        if (newBlockDefId.IsNull)
                        {
                            result.Success = false;
                            result.Message = "Failed to import block definition from external file.";
                            return result;
                        }

                        // Get the actual block name (may have been renamed if there was a conflict)
                        BlockTableRecord importedBtr = tr.GetObject(newBlockDefId, OpenMode.ForRead) as BlockTableRecord;
                        result.NewBlockName = importedBtr?.Name ?? newBlockName;

                        // Note: The imported block definition is the source of truth for geometry and attribute definitions.
                        // We do NOT modify the imported block definition. Only attribute VALUES are transferred to matching tags.

                        // Create new block reference
                        BlockTableRecord currentSpace = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference newBlockRef = new BlockReference(position, newBlockDefId);
                        
                        // Set scale
                        newBlockRef.ScaleFactors = scale;
                        
                        // Set rotation
                        newBlockRef.Rotation = rotation;
                        
                        // Set layer
                        newBlockRef.Layer = layer;
                        
                        // Set normal
                        newBlockRef.Normal = originalBlock.Normal;

                        // Add to database
                        currentSpace.AppendEntity(newBlockRef);
                        tr.AddNewlyCreatedDBObject(newBlockRef, true);

                        // Add attributes from block definition
                        BlockTableRecord newBlockDef = tr.GetObject(newBlockDefId, OpenMode.ForRead) as BlockTableRecord;
                        AddAttributesFromBlockTable(doc, newBlockDef, newBlockRef, tr);

                        // Transfer attribute values
                        if (settings.TransferAttributes && blockInfo.AttributeValues.Count > 0)
                        {
                            TransferAttributeValues(tr, newBlockRef, blockInfo.AttributeValues, ref result);
                        }

                        // Erase the original block
                        originalBlock.Erase(true);

                        tr.Commit();
                        result.Success = true;
                        
                        // Build result message
                        string msg = $"Successfully replaced block '{blockInfo.EffectiveBlockName}' with '{newBlockName}'";
                        if (result.AttributeDefinitionsAdded > 0)
                        {
                            msg += $" (added {result.AttributeDefinitionsAdded} attribute definitions)";
                        }
                        result.Message = msg;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error replacing block: {ex.Message}";
                DebugLogger.Instance.LogError($"Error replacing block: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// Imports a block definition from an external DWG file.
        /// The external DWG is always the source of truth for geometry and attribute definitions.
        /// If a block with the same name exists, it will be renamed to preserve the existing block.
        /// </summary>
        private static ObjectId ImportBlockDefinition(Document doc, Database db, Transaction tr, string filePath, string blockName)
        {
            BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            
            // Generate a unique block name if the target name already exists
            // The external file is the source of truth, so we always import it fresh
            string finalBlockName = blockName;
            int suffix = 1;
            
            while (blockTable.Has(finalBlockName))
            {
                finalBlockName = $"{blockName}_{suffix}";
                suffix++;
            }

            // Try to import from external file with retry logic for file sharing issues
            int maxRetries = 3;
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using (Database xDb = new Database(false, true))
                    {
                        // Try different file open modes
                        FileOpenMode openMode = retry == 0 ? FileOpenMode.OpenForReadAndReadShare : 
                                                 FileOpenMode.OpenForReadAndAllShare;
                        
                        xDb.ReadDwgFile(filePath, openMode, true, null);
                        
                        // Import the external DWG - the entire model space becomes the block definition
                        blockTable.UpgradeOpen();
                        ObjectId blockId = db.Insert(finalBlockName, xDb, true);
                        
                        // Update the result block name if we had to use a different name
                        if (finalBlockName != blockName)
                        {
                            DebugLogger.Instance.Log($"Block '{blockName}' renamed to '{finalBlockName}' to avoid conflict");
                        }
                        
                        return blockId;
                    }
                }
                catch (acadEx ex)
                {
                    DebugLogger.Instance.LogError($"ImportBlockDefinition attempt {retry + 1} failed", ex);
                }
            }

            // If all retries failed due to sharing violation, try copying to temp file
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dwg");
                File.Copy(filePath, tempPath, true);
                
                try
                {
                    using (Database xDb = new Database(false, true))
                    {
                        xDb.ReadDwgFile(tempPath, FileOpenMode.OpenForReadAndAllShare, true, null);
                        
                        blockTable.UpgradeOpen();
                        ObjectId blockId = db.Insert(finalBlockName, xDb, true);
                        
                        // Clean up temp file
                        try { File.Delete(tempPath); } 
                        catch(Exception ex)
                        {
                            DebugLogger.Instance.LogWarning($"Could not delete temp file: {ex.Message}");
                        }
                        
                        return blockId;
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Instance.LogError("ImportBlockDefinition temp file approach failed", ex);
                    // Clean up temp file on failure
                    try { File.Delete(tempPath); }
                    catch (Exception ex2)
                    {
                        DebugLogger.Instance.LogWarning($"Could not delete temp file on failure: {ex2.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogError("ImportBlockDefinition failed to copy to temp file", ex);
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Adds attributes from block table record to a block reference
        /// </summary>
        private static void AddAttributesFromBlockTable(Document doc, BlockTableRecord blockDef, BlockReference blockRef, Transaction tr)
        {
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));

            foreach (ObjectId objId in blockDef)
            {
                if (objId.ObjectClass.IsDerivedFrom(attDefClass))
                {
                    AttributeDefinition attDef = tr.GetObject(objId, OpenMode.ForRead) as AttributeDefinition;
                    if (attDef != null && !attDef.Constant)
                    {
                        // Create attribute reference - do NOT use using statement as the database takes ownership
                        AttributeReference attRef = new AttributeReference();
                        attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                        attRef.Position = attDef.Position.TransformBy(blockRef.BlockTransform);
                        attRef.TextString = attDef.TextString;
                        blockRef.AttributeCollection.AppendAttribute(attRef);
                        tr.AddNewlyCreatedDBObject(attRef, true);
                    }
                }
            }
        }

        /// <summary>
        /// Transfers attribute values from old block to new block
        /// </summary>
        private static void TransferAttributeValues(Transaction tr, BlockReference newBlockRef, 
            Dictionary<string, string> oldAttributeValues, ref BlockReplaceResult result)
        {
            foreach (ObjectId attId in newBlockRef.AttributeCollection)
            {
                try
                {
                    if (!attId.IsErased)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (attRef != null && !string.IsNullOrEmpty(attRef.Tag))
                        {
                            // Look for matching tag (case-insensitive)
                            var matchingKey = oldAttributeValues.Keys.FirstOrDefault(k => 
                                string.Equals(k, attRef.Tag, StringComparison.OrdinalIgnoreCase));
                            
                            if (matchingKey != null)
                            {
                                attRef.TextString = oldAttributeValues[matchingKey];
                                result.AttributesTransferred++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Instance.LogWarning($"Error transferring attribute value: {ex.Message}");
                }
            }

            // Count attributes that didn't match
            foreach (var oldKey in oldAttributeValues.Keys)
            {
                bool found = false;
                foreach (ObjectId attId in newBlockRef.AttributeCollection)
                {
                    try
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (attRef != null && string.Equals(attRef.Tag, oldKey, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Instance.LogWarning($"Error reading attribute for matching: {ex.Message}");
                    }
                }
                if (!found)
                {
                    result.AttributesNotMatched++;
                }
            }
        }

        /// <summary>
        /// Gets all unique block names from a list of block info
        /// </summary>
        public static List<string> GetUniqueBlockNames(List<BlockReplaceInfo> blockInfos)
        {
            return blockInfos.Select(b => b.EffectiveBlockName).Distinct().OrderBy(n => n).ToList();
        }

        /// <summary>
        /// Gets block definitions from an external DWG file
        /// </summary>
        public static List<string> GetBlockNamesFromExternalFile(string filePath)
        {
            var blockNames = new List<string>();
            
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return blockNames;
            }

            try
            {
                using (Database xDb = new Database(false, true))
                {
                    xDb.ReadDwgFile(filePath, FileOpenMode.OpenForReadAndReadShare, true, null);
                    
                    using (Transaction tr = xDb.TransactionManager.StartTransaction())
                    {
                        BlockTable blockTable = tr.GetObject(xDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (blockTable != null)
                        {
                            foreach (ObjectId blockId in blockTable)
                            {
                                BlockTableRecord btr = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                                if (btr != null && !btr.IsAnonymous && !btr.IsLayout && !btr.Name.StartsWith("*"))
                                {
                                    blockNames.Add(btr.Name);
                                }
                            }
                        }
                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogError("GetBlockNamesFromExternalFile failed", ex);
            }

            return blockNames;
        }

        /// <summary>
        /// Exports all block definitions from a drawing to individual DWG files
        /// </summary>
        public static List<string> ExportBlocksToFolder(Document doc, string outputFolder)
        {
            var exportedBlocks = new List<string>();
            if (doc == null || string.IsNullOrEmpty(outputFolder)) return exportedBlocks;

            var db = doc.Database;

            // Ensure output folder exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (var @lock = doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null)
                    {
                        foreach (ObjectId blockId in blockTable)
                        {
                            BlockTableRecord btr = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                            if (btr != null && !btr.IsAnonymous && !btr.IsLayout && !btr.Name.StartsWith("*"))
                            {
                                try
                                {
                                    string safeName = MakeSafeFileName(btr.Name);
                                    string outputPath = Path.Combine(outputFolder, safeName + ".dwg");

                                    // Create a new database containing only this block definition
                                    using (Database newDb = db.Wblock(blockId))
                                    {
                                        newDb.SaveAs(outputPath, DwgVersion.Current);
                                        exportedBlocks.Add(btr.Name);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DebugLogger.Instance.LogError($"Failed to export block '{btr.Name}'", ex);
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }

            return exportedBlocks.OrderBy(n => n).ToList();
        }

        /// <summary>
        /// Collects all ObjectIds from a block definition including sub-entities and nested blocks
        /// </summary>
        private static ObjectIdCollection CollectBlockSubEntityIds(Transaction tr, BlockTableRecord btr, BlockTable blockTable)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            var processedBlocks = new HashSet<ObjectId>();

            // Recursively collect all entity and nested block Ids
            CollectBlockSubEntityIdsRecursive(tr, btr, blockTable, ids, processedBlocks);

            return ids;
        }

        /// <summary>
        /// Recursively collects ObjectIds from a block definition and any nested blocks
        /// </summary>
        private static void CollectBlockSubEntityIdsRecursive(Transaction tr, BlockTableRecord btr, 
            BlockTable blockTable, ObjectIdCollection ids, HashSet<ObjectId> processedBlocks)
        {
            if (btr == null || processedBlocks.Contains(btr.Id)) return;
            
            processedBlocks.Add(btr.Id);

            // Loop over all entities in the block definition
            foreach (ObjectId objId in btr)
            {
                // Add the entity ObjectId to the collection
                if (!ids.Contains(objId))
                {
                    ids.Add(objId);
                }

                // Check if this is a nested block reference
                if (objId.ObjectClass.DxfName == "INSERT")
                {
                    try
                    {
                        BlockReference nestedRef = tr.GetObject(objId, OpenMode.ForRead) as BlockReference;
                        if (nestedRef != null)
                        {
                            // Get the nested block definition
                            ObjectId nestedBtrId = nestedRef.IsDynamicBlock 
                                ? nestedRef.DynamicBlockTableRecord 
                                : nestedRef.BlockTableRecord;

                            if (!nestedBtrId.IsNull && !processedBlocks.Contains(nestedBtrId))
                            {
                                BlockTableRecord nestedBtr = tr.GetObject(nestedBtrId, OpenMode.ForRead) as BlockTableRecord;
                                if (nestedBtr != null && !nestedBtr.IsAnonymous && !nestedBtr.IsLayout)
                                {
                                    // Recursively collect entities from the nested block
                                    CollectBlockSubEntityIdsRecursive(tr, nestedBtr, blockTable, ids, processedBlocks);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Instance.LogWarning($"Error processing nested block: {ex.Message}");
                    }
                }
            }

            // Add the block definition itself
            if (!ids.Contains(btr.Id))
            {
                ids.Add(btr.Id);
            }
        }

        /// <summary>
        /// Creates a safe filename from a block name
        /// </summary>
        private static string MakeSafeFileName(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safeName = name;
            foreach (char c in invalidChars)
            {
                safeName = safeName.Replace(c, '_');
            }
            return safeName;
        }
    }
}