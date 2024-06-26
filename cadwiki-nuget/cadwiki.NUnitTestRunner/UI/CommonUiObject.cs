using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using System.Windows.Media;
using cadwiki.NetUtils;
using cadwiki.NUnitTestRunner.Results;
using Microsoft.VisualBasic;
using System;

namespace cadwiki.NUnitTestRunner
{

    public class CommonUiObject
    {
        private readonly BrushConverter converter = new BrushConverter();
        public readonly Brush Green;
        public readonly Brush Red;

        public CommonUiObject()
        {
            Green = (Brush)converter.ConvertFromString("#00FF00");
            Red = (Brush)converter.ConvertFromString("#FF0000");
        }



        public void WinFormsAddTreeViewItemForTestResult(TestResult testResult, System.Windows.Forms.TreeView treeView)
        {
            treeView.BeginUpdate();
            var testNode = treeView.Nodes.Add("testResult.TestName");
            if (testResult.Passed)
            {
                testNode.Nodes.Add("Passed: " + testResult.TestName);
                testNode.BackColor = System.Drawing.Color.Green;
            }
            else
            {
                testNode.Nodes.Add("Failed: " + testResult.TestName);
                testNode.Nodes.Add("Exception: " + testResult.ExceptionMessage);
                string stackTraceString = Lists.StringListToString(testResult.StackTrace, Environment.NewLine);
                testNode.Nodes.Add("Stack trace: " + stackTraceString);
                testNode.BackColor = System.Drawing.Color.Red;
            }
            treeView.EndUpdate();
            Application.DoEvents();
        }

        public void WinFormsAddResultsToTreeView(ObservableTestSuiteResults observableResults, System.Windows.Forms.TreeView treeView)
        {
            var node = WinFormsCreateResultsItem(observableResults);
            treeView.Nodes.Add(node);
        }

        public void WinFormsUpdateResultsToTreeView(ObservableTestSuiteResults observableResults, System.Windows.Forms.TreeView treeView)
        {
            var node = WinFormsCreateResultsItem(observableResults);
            treeView.Nodes.RemoveAt(0);
            treeView.Nodes.Insert(0, node);
        }

        private TreeNode WinFormsCreateResultsItem(ObservableTestSuiteResults observableResults)
        {
            var node = new TreeNode();
            node.Text = "Test Run Results: " + observableResults.TimeElapsed;
            node.Nodes.Add("Total: " + observableResults.TotalTests.ToString());
            node.Nodes.Add("Passed: " + observableResults.PassedTests.ToString());
            node.Nodes.Add("Failed: " + observableResults.FailedTests.ToString());
            node.Nodes.Add("Time Elapsed: " + observableResults.TimeElapsed);
            node.Expand();
            return node;
        }

        public void WpfAddTreeViewItemForTestResult(TestResult testResult, System.Windows.Controls.TreeView treeView)
        {
            var tvi = new TreeViewItem();
            tvi.Header = testResult.TestName;
            if (testResult.Passed)
            {
                tvi.Items.Add("Passed: " + testResult.TestName);
                tvi.Background = Green;
            }
            else
            {
                tvi.Items.Add("Failed: " + testResult.TestName);
                tvi.Background = Red;
                tvi.Items.Add("Exception: " + testResult.ExceptionMessage);
                string stackTraceString = Lists.StringListToString(testResult.StackTrace, Environment.NewLine);
                tvi.Items.Add("Stack trace: " + stackTraceString);
            }
            treeView.Items.Add(tvi);
            treeView.Items.Refresh();
            Application.DoEvents();
        }

        public void WpfAddResultsToTreeView(ObservableTestSuiteResults observableResults, System.Windows.Controls.TreeView treeView)
        {
            var tvi = WpfCreateResultsItem(observableResults);
            treeView.Items.Add(tvi);
        }

        public void WpfUpdateResultsToTreeView(ObservableTestSuiteResults observableResults, System.Windows.Controls.TreeView treeView)
        {
            var tvi = WpfCreateResultsItem(observableResults);
            treeView.Items[0] = tvi;
        }

        private TreeViewItem WpfCreateResultsItem(ObservableTestSuiteResults observableResults)
        {
            var tvi = new TreeViewItem();
            tvi.Header = "Test Run Results: " + observableResults.TimeElapsed;
            tvi.Items.Add("Total: " + observableResults.TotalTests.ToString());
            tvi.Items.Add("Passed: " + observableResults.PassedTests.ToString());
            tvi.Items.Add("Failed: " + observableResults.FailedTests.ToString());
            tvi.Items.Add("Time Elapsed: " + observableResults.TimeElapsed);
            tvi.IsExpanded = true;
            return tvi;
        }

    }
}