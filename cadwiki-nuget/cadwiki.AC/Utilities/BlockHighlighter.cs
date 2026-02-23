using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using OpenMode = Autodesk.AutoCAD.DatabaseServices.OpenMode;

namespace cadwiki.AC.Utilities
{
    /// <summary>
    /// Handles highlighting of blocks in the AutoCAD model and syncing with UI selection
    /// </summary>
    public class BlockHighlighter : IDisposable
    {
        private Document _document;
        private Editor _editor;
        private List<ObjectId> _highlightedObjectIds;
        private bool _isDisposed;

        public BlockHighlighter(Document doc)
        {
            _document = doc;
            _editor = doc.Editor;
            _highlightedObjectIds = new List<ObjectId>();
        }

        /// <summary>
        /// Highlights the specified block references in the model
        /// </summary>
        public void HighlightBlocks(IEnumerable<ObjectId> blockIds)
        {
            ClearHighlights();

            if (blockIds == null) return;

            var idsToHighlight = blockIds.Where(id => !id.IsNull && id.IsValid).ToList();
            if (idsToHighlight.Count == 0) return;

            try
            {
                using (DocumentLock docLock = _document.LockDocument())
                {
                    using (var tr = _document.Database.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId id in idsToHighlight)
                        {
                            try
                            {
                                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                                if (ent != null)
                                {
                                    // Also use built-in highlight
                                    ent.UpgradeOpen();
                                    ent.Highlight();
                                    _highlightedObjectIds.Add(id);
                                }
                            }
                            catch (Exception ex)
                            {
                                DebugLogger.Instance.LogWarning($"Error highlighting block: {ex.Message}");
                            }
                        }
                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogError("Error in HighlightBlocks", ex);
            }
        }

        /// <summary>
        /// Clears all highlights from the model
        /// </summary>
        public void ClearHighlights()
        {
            try
            {
                // Unhighlight entities
                if (_highlightedObjectIds.Count > 0 && _document != null)
                {
                    using (DocumentLock docLock = _document.LockDocument())
                    {
                        using (var tr = _document.Database.TransactionManager.StartTransaction())
                        {
                            foreach (ObjectId id in _highlightedObjectIds)
                            {
                                try
                                {
                                    if (!id.IsNull && !id.IsErased)
                                    {
                                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                                        if (ent != null)
                                        {
                                            ent.UpgradeOpen();
                                            ent.Unhighlight();
                                        }
                                    }
                                }
                                catch { }
                            }
                            tr.Commit();
                        }
                    }
                    _highlightedObjectIds.Clear();
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogWarning($"Error clearing highlights: {ex.Message}");
            }
        }


        /// <summary>
        /// Zooms to the specified block references
        /// </summary>
        public void ZoomToBlocks(IEnumerable<ObjectId> blockIds)
        {
            if (blockIds == null) return;

            var ids = blockIds.Where(id => !id.IsNull && id.IsValid).ToList();
            if (ids.Count == 0) return;

            try
            {
                Extents3d? totalExtents = null;
                using (DocumentLock docLock = _document.LockDocument())
                {
                    using (var tr = _document.Database.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId id in ids)
                        {
                            Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                            if (ent != null && ent.Bounds.HasValue)
                            {
                                var bounds = ent.Bounds.Value;
                                if (totalExtents.HasValue)
                                {
                                    var min = new Point3d(
                                        Math.Min(totalExtents.Value.MinPoint.X, bounds.MinPoint.X),
                                        Math.Min(totalExtents.Value.MinPoint.Y, bounds.MinPoint.Y),
                                        Math.Min(totalExtents.Value.MinPoint.Z, bounds.MinPoint.Z));
                                    var max = new Point3d(
                                        Math.Max(totalExtents.Value.MaxPoint.X, bounds.MaxPoint.X),
                                        Math.Max(totalExtents.Value.MaxPoint.Y, bounds.MaxPoint.Y),
                                        Math.Max(totalExtents.Value.MaxPoint.Z, bounds.MaxPoint.Z));
                                    totalExtents = new Extents3d(min, max);
                                }
                                else
                                {
                                    totalExtents = bounds;
                                }
                            }
                        }
                        tr.Commit();
                    }

                    if (totalExtents.HasValue)
                    {
                        // Add some padding
                        var min = totalExtents.Value.MinPoint;
                        var max = totalExtents.Value.MaxPoint;
                        var padding = Math.Max((max.X - min.X) * 0.1, (max.Y - min.Y) * 0.1);

                        var paddedMin = new Point3d(min.X - padding, min.Y - padding, min.Z);
                        var paddedMax = new Point3d(max.X + padding, max.Y + padding, max.Z);

                        var viewExtents = new Extents3d(paddedMin, paddedMax);

                        // Zoom using command
                        Window(_document, viewExtents.MinPoint, viewExtents.MaxPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogWarning($"Error zooming to blocks: {ex.Message}");
            }
        }

        public static void Window(Document doc, Point3d min, Point3d max)
        {
            using (var @lock = doc.LockDocument())
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {

                    var min2d = new Point2d(min.X, min.Y);
                    var max2d = new Point2d(max.X, max.Y);

                    var view = doc.Editor.GetCurrentView();

                    view.CenterPoint = min2d + (max2d - min2d) / 2.0d;
                    view.Height = max2d.Y - min2d.Y;
                    view.Width = max2d.X - min2d.X;
                    doc.Editor.SetCurrentView(view);
                    tr.Commit();
                }
            }
        }

        private Extents3d? GetEntityBounds(Entity ent, Transaction tr)
        {
            try
            {
                if (ent.Bounds.HasValue)
                {
                    return ent.Bounds.Value;
                }
            }
            catch { }

            return null;
        }


        public void Dispose()
        {
            if (!_isDisposed)
            {
                ClearHighlights();
                _isDisposed = true;
            }
        }
    }

}