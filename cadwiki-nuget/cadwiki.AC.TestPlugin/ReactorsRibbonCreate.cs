using Autodesk.AutoCAD.ApplicationServices;
using cadwiki.AC.TestPlugin.UiRibbon.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace cadwiki.AC.TestPlugin
{
    public class ReactorsRibbonCreate
    {
        private bool _isSetupComplete = false;
        private int _numberOfTries = 0;
        private int _maxNumberOfTries = 5;

        public void AttachQuiescentReactors(Document doc)
        {
            try
            {
                Application.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
                Application.DocumentManager.DocumentToBeActivated += DocumentManager_DocumentToBeActivated;
                Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
                doc.Editor.EnteringQuiescentState += Editor_EnteringQuiescentState;
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }

        public void DetachAllReactors()
        {
            try
            {
                Application.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
                Application.DocumentManager.DocumentToBeActivated += DocumentManager_DocumentToBeActivated;
                Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.EnteringQuiescentState += Editor_EnteringQuiescentState;
                }
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }

        private void Editor_EnteringQuiescentState(object sender, EventArgs e)
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                _numberOfTries += 1;

                if (_numberOfTries >= _maxNumberOfTries)
                {
                    WriteToEditor(Environment.NewLine + "Max number of ribbon create tries exceeded.");
                    _isSetupComplete = true;
                    DetachAllReactors();
                    return;
                }

                
                if (doc != null && _isSetupComplete == false)
                {
                    var wereAllTabsAdded = TabCreator.AddDevTab(doc);
                    if (wereAllTabsAdded)
                    {
                        _isSetupComplete = true;
                        DetachAllReactors();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }

        private void DocumentManager_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                if (e.Document != null)
                {
                    var ed = e.Document.Editor;
                    ed.EnteringQuiescentState += new EventHandler(Editor_EnteringQuiescentState);
                }
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }
        private void DocumentManager_DocumentToBeActivated(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                if (e.Document != null)
                {
                    var ed = e.Document.Editor;
                    ed.EnteringQuiescentState -= new EventHandler(Editor_EnteringQuiescentState);
                }
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }



        private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                if (e.Document != null)
                {
                    var ed = e.Document.Editor;
                    ed.EnteringQuiescentState -= new EventHandler(Editor_EnteringQuiescentState);
                }
            }
            catch (Exception ex)
            {
                WriteToEditor(ex.Message);
            }
        }

        private void WriteToEditor(string msg)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Max number of ribbon create tries exceeded.");
            }
        }
    }
}
