using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using Application = System.Windows.Forms.Application;
using cadwiki.NetUtils;
using cadwiki.NUnitTestRunner.Results;
using Microsoft.VisualBasic;

namespace cadwiki.NUnitTestRunner.UI
{
    public partial class WindowTestRunner
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
            this.RichTextBoxConsole.AppendText(lastItem);
            Application.DoEvents();
        }


        private void TestResults_OnChanged(object sender, EventArgs e)
        {
            ObservableTestSuiteResults suiteResults = (ObservableTestSuiteResults)sender;
            var testResults = suiteResults.TestResults;
            var mostRecentlyAddedTestResult = testResults[testResults.Count - 1];
            _commonUiObject.WpfAddTreeViewItemForTestResult(mostRecentlyAddedTestResult, this.TreeViewResults);
        }



        public WindowTestRunner()
        {
            ObservableResults = new ObservableTestSuiteResults();
            this.InitializeComponent();
            Init();
        }

        public void Init()
        {
            this.RichTextBoxConsole.AppendText(Environment.NewLine + "NunitTestRunner started");
            Bitmap bitMap = cadwiki.FileStore.ResourceIcons._500x500_cadwiki_v1;
            var bitMapImage = Bitmaps.BitMapToBitmapImage(bitMap);
            this.Icon = bitMapImage;
        }

        public WindowTestRunner(ref ObservableTestSuiteResults suiteResults)
        {
            ObservableResults = new ObservableTestSuiteResults();
            ObservableResults = suiteResults;
            this.InitializeComponent();
            Init();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonTestEvidence_Click(object sender, RoutedEventArgs e)
        {
            var creator = new Creators.TestEvidenceCreator();
            string foldername = creator.GetFolderCache();
            Process.Start(foldername);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}