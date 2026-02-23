using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using cadwiki.AC.PalleteSets;
using cadwiki.AC.Utilities;
using cadwiki.AC.Views;

namespace cadwiki.AC.ViewModels
{
    public static class DataGridSelectedItemsBehavior
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsBehavior),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value)
        {
            element.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject element)
        {
            return (IList)element.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }

        private static void DataGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var boundCollection = GetSelectedItems(dataGrid);
                if (boundCollection == null) return;

                boundCollection.Clear();

                foreach (var item in dataGrid.SelectedItems)
                    boundCollection.Add(item);
            }
        }
    }

    /// <summary>
    /// A simple implementation of ICommand for MVVM
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    public class BlockReplaceViewModel : PaletteSetViewModel
    {
        private Document _currentDocument;
        private bool _isDocumentSwitching;
        private BlockHighlighter _highlighter;
        private DispatcherTimer _selectionSyncTimer;
        private bool _isSyncingSelection;
        private bool _isUpdatingGridFromSelection;
        private readonly Dispatcher _uiDispatcher;

        public BlockReplaceViewModel()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            Title = "Block Replacer";
            FileNamePrefix = "BlockReplacer";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            
            BlockDefinitions = new ObservableCollection<BlockDefinitionInfo>();
            CurrentDocumentBlocks = new ObservableCollection<BlockReplaceInfo>();
            ReplacementResults = new ObservableCollection<BlockReplaceResult>();
            
            // Initialize settings
            Settings = new BlockReplaceSettings();
            
            // Initialize commands
            RefreshBlocksCommand = new RelayCommand(ExecuteRefreshBlocks);
            SelectAllBlocksCommand = new RelayCommand(ExecuteSelectAllBlocks);
            DeselectAllBlocksCommand = new RelayCommand(ExecuteDeselectAllBlocks);
            BrowseExternalFileCommand = new RelayCommand(ExecuteBrowseExternalFile);
            ReplaceCurrentDocumentBlocksCommand = new RelayCommand(ExecuteReplaceCurrentDocumentBlocks, CanReplaceBlocks);
            ClearResultsCommand = new RelayCommand(ExecuteClearResults);
            ExportBlocksCommand = new RelayCommand(ExecuteExportBlocks, CanExportBlocks);
            BrowseExportFolderCommand = new RelayCommand(ExecuteBrowseExportFolder);
            ClearDebugLogCommand = new RelayCommand(ExecuteClearDebugLog);
            
            // Subscribe to debug logger
            DebugLogger.Instance.LogAdded += OnDebugLogAdded;
            
            // Subscribe to document events
            SubscribeToDocumentEvents();
            
            // Load blocks from current document
            LoadBlocksFromCurrentDocument();
            
            // Subscribe to selection events for current document
            SubscribeToSelectionEvents();
        }


        private ObservableCollection<BlockDefinitionInfo> _blockDefinitions;
        public ObservableCollection<BlockDefinitionInfo> BlockDefinitions
        {
            get => _blockDefinitions;
            set
            {
                _blockDefinitions = value;
                NotifyOfPropertyChange(nameof(BlockDefinitions));
            }
        }

        private BlockDefinitionInfo _selectedBlockDefinition;
        public BlockDefinitionInfo SelectedBlockDefinition
        {
            get => _selectedBlockDefinition;
            set
            {
                _selectedBlockDefinition = value;
                NotifyOfPropertyChange(nameof(SelectedBlockDefinition));
                LoadBlockReferences();
            }
        }

        private BlockReplaceInfo _selectedBlockReference;
        public BlockReplaceInfo SelectedBlockReference
        {
            get => _selectedBlockReference;
            set
            {
                _selectedBlockReference = value;
                NotifyOfPropertyChange(nameof(SelectedBlockReference));
                OnSelectedBlockReferenceChanged();
            }
        }

        private ObservableCollection<BlockReplaceInfo> _CurrentDocumentBlocks;
        public ObservableCollection<BlockReplaceInfo> CurrentDocumentBlocks
        {
            get => _CurrentDocumentBlocks;
            set
            {
                _CurrentDocumentBlocks = value;
                NotifyOfPropertyChange(nameof(CurrentDocumentBlocks));
            }
        }

        private ObservableCollection<BlockReplaceInfo> _highlightedBlocks = new ObservableCollection<BlockReplaceInfo>();
        public ObservableCollection<BlockReplaceInfo> HighlightedBlocks
        {
            get => _highlightedBlocks;
            set
            {
                _highlightedBlocks = value;
                NotifyOfPropertyChange(nameof(HighlightedBlocks));
            }
        }

        private string _externalDwgPath;
        public string ExternalDwgPath
        {
            get => _externalDwgPath;
            set
            {
                _externalDwgPath = value;
                Settings.ExternalDwgPath = value;
                NotifyOfPropertyChange(nameof(ExternalDwgPath));
                NotifyOfPropertyChange(nameof(CanReplace));
                NotifyOfPropertyChange(nameof(ExternalFileName));
            }
        }

        public string ExternalFileName
        {
            get
            {
                if (!string.IsNullOrEmpty(ExternalDwgPath) && File.Exists(ExternalDwgPath))
                {
                    return Path.GetFileName(ExternalDwgPath);
                }
                return "No file selected";
            }
        }

        private BlockReplaceSettings _settings;
        public BlockReplaceSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                NotifyOfPropertyChange(nameof(Settings));
            }
        }

        private bool _preserveScale = true;
        public bool PreserveScale
        {
            get => _preserveScale;
            set
            {
                _preserveScale = value;
                Settings.PreserveScale = value;
                NotifyOfPropertyChange(nameof(PreserveScale));
                NotifyOfPropertyChange(nameof(ShowScaleOverride));
            }
        }

        public bool ShowScaleOverride => !PreserveScale;

        private bool _preserveRotation = true;
        public bool PreserveRotation
        {
            get => _preserveRotation;
            set
            {
                _preserveRotation = value;
                Settings.PreserveRotation = value;
                NotifyOfPropertyChange(nameof(PreserveRotation));
                NotifyOfPropertyChange(nameof(ShowRotationOverride));
            }
        }

        public bool ShowRotationOverride => !PreserveRotation;

        private double _overrideScaleX = 1.0;
        public double OverrideScaleX
        {
            get => _overrideScaleX;
            set
            {
                _overrideScaleX = value;
                Settings.OverrideScaleX = value;
                NotifyOfPropertyChange(nameof(OverrideScaleX));
            }
        }

        private double _overrideScaleY = 1.0;
        public double OverrideScaleY
        {
            get => _overrideScaleY;
            set
            {
                _overrideScaleY = value;
                Settings.OverrideScaleY = value;
                NotifyOfPropertyChange(nameof(OverrideScaleY));
            }
        }

        private double _overrideScaleZ = 1.0;
        public double OverrideScaleZ
        {
            get => _overrideScaleZ;
            set
            {
                _overrideScaleZ = value;
                Settings.OverrideScaleZ = value;
                NotifyOfPropertyChange(nameof(OverrideScaleZ));
            }
        }

        private double _overrideRotation = 0.0;
        public double OverrideRotation
        {
            get => _overrideRotation;
            set
            {
                _overrideRotation = value;
                Settings.OverrideRotation = value;
                NotifyOfPropertyChange(nameof(OverrideRotation));
            }
        }

        private ObservableCollection<BlockReplaceResult> _replacementResults;
        public ObservableCollection<BlockReplaceResult> ReplacementResults
        {
            get => _replacementResults;
            set
            {
                _replacementResults = value;
                NotifyOfPropertyChange(nameof(ReplacementResults));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                NotifyOfPropertyChange(nameof(StatusMessage));
            }
        }

        private int _CurrentDocumentBlocksCount;
        public int CurrentDocumentBlocksCount
        {
            get => _CurrentDocumentBlocksCount;
            set
            {
                _CurrentDocumentBlocksCount = value;
                NotifyOfPropertyChange(nameof(CurrentDocumentBlocksCount));
                NotifyOfPropertyChange(nameof(CurrentDocumentBlocksCountText));
                NotifyOfPropertyChange(nameof(CanReplace));
            }
        }

        public string CurrentDocumentBlocksCountText => $"{CurrentDocumentBlocksCount} block references selected for replacement";

        public bool CanReplace => CurrentDocumentBlocksCount > 0 && !string.IsNullOrEmpty(ExternalDwgPath) && !_isDocumentSwitching;

        private string _exportFolderPath;
        public string ExportFolderPath
        {
            get => _exportFolderPath;
            set
            {
                _exportFolderPath = value;
                NotifyOfPropertyChange(nameof(ExportFolderPath));
                NotifyOfPropertyChange(nameof(CanExportBlocks));
            }
        }

        //public bool CanExportBlocks => !string.IsNullOrEmpty(ExportFolderPath) && BlockDefinitions.Count > 0 && !_isDocumentSwitching;

        private string _currentDocumentName;
        public string CurrentDocumentName
        {
            get => _currentDocumentName;
            set
            {
                _currentDocumentName = value;
                NotifyOfPropertyChange(nameof(CurrentDocumentName));
            }
        }

        private StringBuilder _debugLog = new StringBuilder();
        public string DebugLog
        {
            get => _debugLog.ToString();
            set
            {
                _debugLog.Clear();
                _debugLog.Append(value);
                NotifyOfPropertyChange(nameof(DebugLog));
            }
        }


        public ICommand RefreshBlocksCommand { get; }
        public ICommand SelectAllBlocksCommand { get; }
        public ICommand DeselectAllBlocksCommand { get; }
        public ICommand BrowseExternalFileCommand { get; }
        public ICommand ReplaceCurrentDocumentBlocksCommand { get; }
        public ICommand ClearResultsCommand { get; }
        public ICommand ExportBlocksCommand { get; }
        public ICommand BrowseExportFolderCommand { get; }
        public ICommand HighlightCurrentDocumentBlocksCommand { get; }
        public ICommand ZoomToCurrentDocumentBlocksCommand { get; }
        public ICommand SyncSelectionFromModelCommand { get; }
        public ICommand ClearDebugLogCommand { get; }


        private void SubscribeToDocumentEvents()
        {
            var docMgr = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;
            docMgr.DocumentActivated += DocumentManager_DocumentActivated;
            docMgr.DocumentToBeDeactivated += DocumentManager_DocumentToBeDeactivated;
            docMgr.DocumentCreated += DocumentManager_DocumentCreated;
            docMgr.DocumentDestroyed += DocumentManager_DocumentDestroyed;
        }

        private void SubscribeToSelectionEvents()
        {
            if (_currentDocument == null) return;

            var ed = _currentDocument.Editor;
            _currentDocument.ImpliedSelectionChanged += _currentDocument_ImpliedSelectionChanged;
        }

        private void _currentDocument_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            HighlightedBlocks.Clear();

            // Don't process while we're updating the grid or document is switching
            if (_isUpdatingGridFromSelection || _isDocumentSwitching || _isSyncingSelection)
                return;

            try
            {
                if (_currentDocument == null || CurrentDocumentBlocks == null || CurrentDocumentBlocks.Count == 0)
                    return;

                var ed = _currentDocument.Editor;
                
                // Get the implied (pickfirst) selection set
                var ss = ed.SelectImplied();
                if (ss.Status != PromptStatus.OK || ss.Value == null)
                {
                    // No implied selection - deselect all in grid
                    _uiDispatcher.BeginInvoke(new Action(() =>
                    {
                        _isUpdatingGridFromSelection = true;

                        foreach (var block in CurrentDocumentBlocks)
                        {
                            block.IsGridHighlighted = false;
                            block.IsDocumentHighlighted = block.IsGridHighlighted;
                        }

                        UpdateSelectedCount();
                        NotifyOfPropertyChange(nameof(CurrentDocumentBlocks));

                        _isUpdatingGridFromSelection = false;
                    }));
                    return;
                }

                // Get the selected object IDs that are block references
                var selectedIds = new HashSet<ObjectId>();
                foreach (SelectedObject so in ss.Value)
                {
                    if (so != null && so.ObjectId.ObjectClass.DxfName == "INSERT")
                    {
                        selectedIds.Add(so.ObjectId);
                    }
                }

                // Update grid selection on UI thread
                _uiDispatcher.BeginInvoke(new Action(() =>
                {
                    _isUpdatingGridFromSelection = true;

                    // Update selection state for all blocks in the grid
                    foreach (var block in CurrentDocumentBlocks)
                    {
                        var isHighlighted = selectedIds.Contains(block.BlockId);
                        if (isHighlighted)
                        {
                            block.IsGridHighlighted = isHighlighted;
                            block.IsDocumentHighlighted = isHighlighted;
                            HighlightedBlocks.Add(block);
                        }

                    }

                    UpdateSelectedCount();
                    NotifyOfPropertyChange(nameof(CurrentDocumentBlocks));

                    _isUpdatingGridFromSelection = false;
                }));
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogWarning($"Error in ImpliedSelectionChanged handler: {ex.Message}");
            }
        }


        private void UnsubscribeFromSelectionEvents()
        {
            if (_currentDocument == null) return;

            try
            {
                var ed = _currentDocument.Editor;
                _currentDocument.ImpliedSelectionChanged -= _currentDocument_ImpliedSelectionChanged;

            }
            catch (Exception ex)
            {
                DebugLogger.Instance.LogError($"Error in ImpliedSelectionChanged handler: {ex.Message}");
            }
        }

        private void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                _isDocumentSwitching = false;
                
                // Unsubscribe from old document's selection events
                UnsubscribeFromSelectionEvents();
                
                LoadBlocksFromCurrentDocument();
                
                // Subscribe to new document's selection events
                SubscribeToSelectionEvents();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading document: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void DocumentManager_DocumentToBeDeactivated(object sender, DocumentCollectionEventArgs e)
        {
            _isDocumentSwitching = true;
            
            // Unsubscribe from selection events when leaving document
            UnsubscribeFromSelectionEvents();
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                _isDocumentSwitching = false;
                LoadBlocksFromCurrentDocument();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading new document: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            try
            {
                LoadBlocksFromCurrentDocument();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error after document closed: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void LoadBlocksFromCurrentDocument()
        {
            try
            {
                _currentDocument = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                
                if (_currentDocument == null)
                {
                    BlockDefinitions.Clear();
                    CurrentDocumentBlocks.Clear();
                    CurrentDocumentName = "No document";
                    StatusMessage = "No active document.";
                    return;
                }

                CurrentDocumentName = _currentDocument.Name;
                StatusMessage = $"Loading blocks from {Path.GetFileName(_currentDocument.Name)}...";

                var blockDefs = BlockReplacer.GetBlockDefinitionsFromDrawing(_currentDocument);
                
                BlockDefinitions.Clear();
                foreach (var def in blockDefs)
                {
                    BlockDefinitions.Add(def);
                }

                CurrentDocumentBlocks.Clear();
                SelectedBlockDefinition = null;
                CurrentDocumentBlocksCount = 0;

                StatusMessage = $"Loaded {BlockDefinitions.Count} block definitions.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading blocks: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void LoadBlockReferences()
        {
            CurrentDocumentBlocks.Clear();
            CurrentDocumentBlocksCount = 0;

            if (_currentDocument == null)
            {
                return;
            }

            try
            {
                foreach (var blk  in BlockDefinitions)
                {
                    var blockRefs = BlockReplacer.GetBlockReferencesByName(_currentDocument, blk.Name);
                    foreach (var blockRef in blockRefs)
                    {
                        CurrentDocumentBlocks.Add(blockRef);
                    }
                    DebugLogger.Instance.LogWarning($"Loaded {blockRefs.Count} references for '{blk.Name}'.");
                }

                CurrentDocumentBlocksCount = CurrentDocumentBlocks.Count();
                StatusMessage = $"Loaded {CurrentDocumentBlocks.Count} references'.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading block references: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void ExecuteRefreshBlocks(object parameter)
        {
            LoadBlocksFromCurrentDocument();
        }

        private void ExecuteSelectAllBlocks(object parameter)
        {
            if (CurrentDocumentBlocks == null) return;
            
            foreach (var block in CurrentDocumentBlocks)
            {
                block.IsReplaceChecked = true;
            }
            UpdateSelectedCount();
            NotifyOfPropertyChange(nameof(CurrentDocumentBlocks));
        }

        private void ExecuteDeselectAllBlocks(object parameter)
        {
            if (CurrentDocumentBlocks == null) return;
            
            foreach (var block in CurrentDocumentBlocks)
            {
                block.IsReplaceChecked = false;
            }
            UpdateSelectedCount();
            NotifyOfPropertyChange(nameof(CurrentDocumentBlocks));
        }

        private void ExecuteBrowseExternalFile(object parameter)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "AutoCAD Drawing Files (*.dwg)|*.dwg|All Files (*.*)|*.*",
                    Title = "Select External DWG File with Block Definition",
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                if (dialog.ShowDialog() == true)
                {
                    ExternalDwgPath = dialog.FileName;
                    StatusMessage = $"Selected: {ExternalFileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error browsing file: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private bool CanReplaceBlocks(object parameter)
        {
            return CanReplace;
        }

        private void ExecuteReplaceCurrentDocumentBlocks(object parameter)
        {
            try
            {
                _currentDocument = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (_currentDocument == null)
                {
                    StatusMessage = "No active document.";
                    return;
                }

                var blocksToReplace = CurrentDocumentBlocks.Where(b => b.IsReplaceChecked).ToList();
                if (blocksToReplace.Count == 0)
                {
                    StatusMessage = "No blocks selected for replacement.";
                    return;
                }

                if (string.IsNullOrEmpty(ExternalDwgPath) || !File.Exists(ExternalDwgPath))
                {
                    StatusMessage = "Please select a valid external DWG file.";
                    return;
                }

                // Update settings
                Settings.ExternalDwgPath = ExternalDwgPath;
                Settings.PreserveScale = PreserveScale;
                Settings.PreserveRotation = PreserveRotation;
                
                if (!PreserveScale)
                {
                    Settings.OverrideScaleX = OverrideScaleX;
                    Settings.OverrideScaleY = OverrideScaleY;
                    Settings.OverrideScaleZ = OverrideScaleZ;
                }
                else
                {
                    Settings.OverrideScaleX = null;
                    Settings.OverrideScaleY = null;
                    Settings.OverrideScaleZ = null;
                }

                if (!PreserveRotation)
                {
                    Settings.OverrideRotation = OverrideRotation;
                }
                else
                {
                    Settings.OverrideRotation = null;
                }

                // Clear previous results
                ReplacementResults.Clear();

                int successCount = 0;
                int failCount = 0;

                // Replace each selected block
                foreach (var blockInfo in blocksToReplace)
                {
                    var result = BlockReplacer.ReplaceBlock(_currentDocument, blockInfo, Settings);
                    ReplacementResults.Add(result);
                    
                    if (result.Success)
                        successCount++;
                    else
                        failCount++;
                }

                StatusMessage = $"Replacement complete: {successCount} succeeded, {failCount} failed.";
                
                // Reload blocks after replacement
                LoadBlocksFromCurrentDocument();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error replacing blocks: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void ExecuteClearResults(object parameter)
        {
            ReplacementResults.Clear();
            StatusMessage = "Results cleared.";
        }

        private bool CanExportBlocks(object parameter)
        {
            return !string.IsNullOrEmpty(ExportFolderPath) && BlockDefinitions.Count > 0 && !_isDocumentSwitching;
        }

        private void ExecuteBrowseExportFolder(object parameter)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Select Export Folder",
                    FileName = "Select Folder",
                    Filter = "Folder|*.folder",
                    CheckPathExists = true
                };

                if (dialog.ShowDialog() == true)
                {
                    ExportFolderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                    StatusMessage = $"Export folder: {ExportFolderPath}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting folder: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void ExecuteExportBlocks(object parameter)
        {
            try
            {
                _currentDocument = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (_currentDocument == null)
                {
                    StatusMessage = "No active document.";
                    return;
                }

                if (string.IsNullOrEmpty(ExportFolderPath))
                {
                    StatusMessage = "Please select an export folder.";
                    return;
                }

                StatusMessage = "Exporting blocks...";

                var exportedBlocks = BlockReplacer.ExportBlocksToFolder(_currentDocument, ExportFolderPath);
                
                StatusMessage = $"Exported {exportedBlocks.Count} blocks to {ExportFolderPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting blocks: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void UpdateSelectedCount()
        {
            CurrentDocumentBlocksCount = CurrentDocumentBlocks?.Count(b => b.IsReplaceChecked) ?? 0;
        }


        /// <summary>
        /// Called when the selected block reference in the grid changes.
        /// Highlights and zooms to the selected block in the drawing.
        /// </summary>
        private void OnSelectedBlockReferenceChanged()
        {
            try
            {
                // Clear any existing highlights first
                if (_highlighter != null)
                {
                    _highlighter.ClearHighlights();
                }

                if (_currentDocument == null || SelectedBlockReference == null)
                    return;

                // Create new highlighter if needed
                if (_highlighter == null)
                {
                    _highlighter = new BlockHighlighter(_currentDocument);
                }

                // Highlight the selected block
                var blockId = SelectedBlockReference.BlockId;
                _highlighter.HighlightBlocks(new[] { blockId });

                // Zoom to the selected block
                _highlighter.ZoomToBlocks(new[] { blockId });

                StatusMessage = $"Zoomed to block: {SelectedBlockReference.EffectiveBlockName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error highlighting block: {ex.Message}";
                DebugLogger.Instance.LogError(ex);
            }
        }

        private void OnDebugLogAdded(string message)
        {
            // Append the message to the debug log
            _debugLog.AppendLine(message);
            NotifyOfPropertyChange(nameof(DebugLog));
        }

        private void ExecuteClearDebugLog(object parameter)
        {
            _debugLog.Clear();
            NotifyOfPropertyChange(nameof(DebugLog));
        }

    }
}
