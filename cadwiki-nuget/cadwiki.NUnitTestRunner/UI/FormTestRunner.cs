using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Application = System.Windows.Forms.Application;
using cadwiki.NetUtils;
using cadwiki.NUnitTestRunner.Results;
using Microsoft.VisualBasic;

namespace cadwiki.NUnitTestRunner.UI
{
    public partial class FormTestRunner
    {
        private ObservableTestSuiteResults _ObservableResults;

        public virtual ObservableTestSuiteResults ObservableResults
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ObservableResults;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ObservableResults != null)
                {
                    _ObservableResults.MessageAdded -= TestMessages_OnChanged;
                    _ObservableResults.ResultAdded -= TestResults_OnChanged;
                }

                _ObservableResults = value;
                if (_ObservableResults != null)
                {
                    _ObservableResults.MessageAdded += TestMessages_OnChanged;
                    _ObservableResults.ResultAdded += TestResults_OnChanged;
                }
            }
        }

        private CommonUiObject _commonUiObject = new CommonUiObject();

        private void TestMessages_OnChanged(object sender, EventArgs e)
        {
            ObservableTestSuiteResults suiteResults = (ObservableTestSuiteResults)sender;
            var messages = suiteResults.Messages;
            string lastItem = messages[messages.Count - 1];
            RichTextBoxConsole.AppendText(lastItem);
            Application.DoEvents();
        }


        private void TestResults_OnChanged(object sender, EventArgs e)
        {
            ObservableTestSuiteResults suiteResults = (ObservableTestSuiteResults)sender;
            var testResults = suiteResults.TestResults;
            var mostRecentlyAddedTestResult = testResults[testResults.Count - 1];
            _commonUiObject.WinFormsAddTreeViewItemForTestResult(mostRecentlyAddedTestResult, TreeViewResults);
        }



        public FormTestRunner()
        {
            ObservableResults = new ObservableTestSuiteResults();
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            RichTextBoxConsole.AppendText(Environment.NewLine + "NunitTestRunner started");
            Bitmap bitMap = cadwiki.FileStore.ResourceIcons._500x500_cadwiki_v1;
            var icon = Bitmaps.BitmapToIcon(bitMap, true, Color.White);
            Icon = icon;
        }

        public FormTestRunner(ref ObservableTestSuiteResults suiteResults)
        {
            ObservableResults = new ObservableTestSuiteResults();
            ObservableResults = suiteResults;
            InitializeComponent();
            Init();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonOpenTestEvidenceFolder_Click(object sender, EventArgs e)
        {
            var creator = new Creators.TestEvidenceCreator();
            string foldername = creator.GetFolderCache();
            Process.Start(foldername);
        }
    }

}