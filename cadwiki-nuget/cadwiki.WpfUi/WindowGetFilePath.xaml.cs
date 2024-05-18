using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.WpfUi
{

    public partial class WindowGetFilePath
    {

        public bool WasOkayClicked;
        public string SelectedFolder = "";

        public WindowGetFilePath()
        {
            // This call is required by the designer.
            this.InitializeComponent();
        }

        public static void LoadXaml(object obj)
        {
            var @type = obj.GetType();
            var assemblyName = type.Assembly.GetName();
            string uristring = string.Format("/{0};v{1};component/{2}.xaml", assemblyName.Name, assemblyName.Version, type.Name);
            var uri = new Uri(uristring, UriKind.Relative);
            System.Windows.Application.LoadComponent(obj, uri);
        }

        public WindowGetFilePath(List<string> filePaths)
        {
            // Might work?
            // https://stackoverflow.com/questions/1453107/how-to-force-wpf-to-use-resource-uris-that-use-assembly-strong-name-argh
            this._contentLoaded = true;
            LoadXaml(this);
            this.InitializeComponent();
            foreach (string filePath in filePaths)
                this.ListBoxFolderPaths.Items.Add(filePath);
        }

        private void ButtonBrowseFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select acad.exe to launch: ";
            dialog.ShowDialog();
            string newPath = dialog.FileName;
            if (System.IO.File.Exists(newPath))
            {
                int index = this.ListBoxFolderPaths.Items.Add(newPath);
                this.ListBoxFolderPaths.SelectedIndex = index;
                this.TextBlockStatus.Text = "File added to list: " + newPath;
            }
            else
            {
                this.TextBlockStatus.Text = "File does not exist: " + newPath;
            }
            SelectedFolder = newPath;
        }

        private void ListBoxFolderPaths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedFolderFromListBox();
        }

        private void UpdateSelectedFolderFromListBox()
        {
            string selectedPath = Conversions.ToString(this.ListBoxFolderPaths.SelectedItem);

            if (selectedPath is not null)
            {
                if (System.IO.File.Exists(selectedPath))
                {
                    this.TextBlockStatus.Text = "File does exist: " + selectedPath;
                    SelectedFolder = selectedPath;
                }
                else
                {
                    this.TextBlockStatus.Text = "File does not exist: " + selectedPath;
                    SelectedFolder = selectedPath;
                }
            }
        }

        private void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WasOkayClicked = true;
            UpdateSelectedFolderFromListBox();
            this.Close();
        }

        private void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelectedFolderFromListBox();
            this.Close();
        }

    }
}