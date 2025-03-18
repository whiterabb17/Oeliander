using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OelianderUI.Helpers;

namespace OelianderUI.Views
{
    /// <summary>
    /// Interaction logic for DirectoryView.xaml
    /// </summary>
    public partial class DirectoryViewPage : Page
    {
        public DirectoryViewPage()
        {
            InitializeComponent();
            Objects.dirViewer = this;
            if (Objects.ssh != null)
            {
                if (Objects.ssh.Client.IsConnected)
                    LoadDrives(false);
            }
            else
                LoadDrives();
        }
        public static List<string> _Drives { get;set; }
        public static string CurrentlySelectedDrive { get;set; }
        public static string CurrentlySelectedPath { get;set; }
        internal static bool LocalDrive = true;
        internal static bool IsDirectory = true;
        private void LoadDrives(bool Default = true)
        {
            switch (Default)
            {
                case true:
                    LocalDrive = Default;
                // Load all available drives (e.g., C:, D:, etc.)
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        // Only show drives that are ready and valid
                        if (drive.IsReady)
                        {
                            DriveComboBox.Items.Add(drive.Name);
                        }
                    }
                    break;
                case false:
                    string[] _directories = { "/home", "/" };
                    _Drives.AddRange(_directories);
                    break;
            }
        }

        private string ReturnResult(string command)
        {
            try
            {
                if (Objects.ssh.TryConnect(1))
                {
                    var output = Objects.ssh.ExecuteCommand(command);
                    return output;
                } 
                else
                {
                    return "Unable to connect to remote system";
                }
                
            }
            catch (Exception ex)
            {
                Objects.ShowAlert("Connection Failed", "Connection failed to send command", 1);
                Objects.obj.HandleException(ex);
                return "";
            }
        }

        private void DriveComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DriveComboBox.SelectedItem != null)
            {
                CurrentlySelectedDrive = DriveComboBox.SelectedItem.ToString();
                switch (LocalDrive)
                {
                    case true:
                        LoadDirectories(CurrentlySelectedDrive);
                        break;
                    case false:
                        var result = ReturnResult($"ls {CurrentlySelectedDrive}");
                        IsDirectory = true;
                        SortData(result);
                        break;
                }
            }
        }

        private string[] SanitizeDirectoryPath(string content)
        {
            var _content = content.Split("\n");
            string[] stringsToRemove = { Objects.CurrentUser, "\n" };
            var _path = _content.Where(x => !stringsToRemove.Contains(x)).ToArray();
            File.WriteAllText("Sanitizing.log", $"{content}\n===============================\n{_content}\n===========================\n{_path}");
            return _path[0].Split(" ");
        }

        public void SortData(string content)
        {
            switch (IsDirectory)
            {
                case true:
                    LoadDirectories(content);
                    break;
                case false:
                    LoadFiles(content);
                    break;
            }
        }

        public void LoadDirectories(string response)
        {
            // Clear the existing items in the TreeView
            DirectoryTreeView.Items.Clear();

            try
            {
                // Get directories from the root of the selected drive
                string[] directories;
                switch (LocalDrive)
                {
                    case true:
                        directories = Directory.GetDirectories(response);
                        foreach (var dir in directories)
                        {
                            var directoryInfo = new DirectoryInfo(dir);
                            var treeItem = new System.Windows.Controls.TreeViewItem
                            {
                                Header = directoryInfo.Name,
                                Tag = dir
                            };
                            treeItem.Items.Add(null); // Placeholder for child directories
                            treeItem.Expanded += DirectoryTreeItem_Expanded;
                            DirectoryTreeView.Items.Add(treeItem);
                        }
                        break;
                    case false:
                        directories = SanitizeDirectoryPath(response);
                        foreach (var dir in directories)
                        {
                            //var directoryInfo = new DirectoryInfo(dir);
                            var treeItem = new System.Windows.Controls.TreeViewItem
                            {
                                Header = dir,
                                Tag = $"{CurrentlySelectedDrive}/{dir}"
                            };
                            treeItem.Items.Add(null); // Placeholder for child directories
                            treeItem.Expanded += DirectoryTreeItem_Expanded;
                            DirectoryTreeView.Items.Add(treeItem);
                        }
                        break;
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to the directory.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading directories: {ex.Message}");
            }
        }

        private void DirectoryTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DirectoryTreeView.SelectedItem != null)
            {
                var selectedItem = DirectoryTreeView.SelectedItem as System.Windows.Controls.TreeViewItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {
                    CurrentlySelectedPath = selectedItem.Tag.ToString();
                    switch (LocalDrive)
                    {
                        case true:
                            LoadFiles(CurrentlySelectedPath);
                            break;
                        case false:
                            IsDirectory = false;
                            var result = ReturnResult($"ls {CurrentlySelectedPath}");
                            SortData(result);
                            break;
                    }
                }
            }
        }
        private void DirectoryTreeItem_Expanded(object sender, RoutedEventArgs e)
        {
            var treeItem = sender as System.Windows.Controls.TreeViewItem;
            if (treeItem != null && treeItem.Items.Count == 1 && treeItem.Items[0] == null)
            {
                // Remove the placeholder
                treeItem.Items.Clear();

                string path = treeItem.Tag.ToString();
                string[] directories;
                switch (LocalDrive)
                {
                    case true:
                        directories = Directory.GetDirectories(path);
                        foreach (var dir in directories)
                        {
                            var directoryInfo = new DirectoryInfo(dir);
                            var newItem = new System.Windows.Controls.TreeViewItem
                            {
                                Header = directoryInfo.Name,
                                Tag = dir
                            };
                            newItem.Items.Add(null); // Add placeholder for subdirectories
                            newItem.Expanded += DirectoryTreeItem_Expanded;
                            treeItem.Items.Add(newItem);
                        }
                        break;
                    case false:
                        var result = ReturnResult($"ls {path}");
                        SortData(result);
                        break;
                }
            }
        }

        private void LoadUserPath(object sender, RoutedEventArgs e)
        {
            if (PathTextBox.Text != "" || PathTextBox.Text != null)
            {
                switch (LocalDrive)
                {
                    case true:
                        LoadDirectories(PathTextBox.Text);
                        break;
                    case false:
                        IsDirectory = true;
                        var result = ReturnResult($"ls {PathTextBox.Text}");
                        SortData(result);
                        break;
                }
            }
        }

        private void LoadFiles(string content)
        {
            FileListBox.Items.Clear();

            try
            {
                string[] files;
                switch (LocalDrive)
                {
                    case true: 
                        files = Directory.GetFiles(content);
                        break;
                    case false:
                        files = SanitizeDirectoryPath(content);
                        break;
                }
                foreach (var file in files)
                {
                    FileListBox.Items.Add(file);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to the directory.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}");
            }
        }
    }
}
