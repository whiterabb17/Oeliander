using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OelianderUI.Core.Models;
using OelianderUI.Helpers;

namespace OelianderUI.Views;

public partial class TerminalPage : Page, INotifyPropertyChanged
{
    public TerminalPage()
    {
        InitializeComponent();
        DataContext = this;
        FillConnectionList(MainPage._collectionList);
        //ConnectionString = "admin@127.0.0.1:2222";
        CommandText = "> ";
    }

    #region locals
    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<CollectionListing> CollectedTargets { get; } = new();
    public ScanHelper helperObject { get; set; }
    public List<string> collectedCredentials = new();
    public static Dictionary<User, string> _staticList = new();
    public string CommandText { get; set; }
    private string CurrentUser { get; set; }
    private string CurrentIP { get; set; }
    private string CurrentPassword { get; set; }
    private bool _isConnected = false;

    #endregion locals

    public void AddResult(string text, object obj)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(text + Environment.NewLine);
            });
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }

    public void AddResult(string text)
    {
        try
        {
            Dispatcher.Invoke(() => { LogBox.AppendText(text + Environment.NewLine); });
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }
    public void SessionResult(string text)
    {
        try
        {
            Dispatcher.Invoke(() => { connectionString.Text = ""; connectionString.Text = text; }); //connectionString.Text = ""; connectionString.Text = text; });
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
            ShowAlert("Unable to Select Target", E.Message, 1);
        }
    }

    private void FillConnectionList(List<CollectionListing> _collectionList)
    {
        foreach (var item in _collectionList)
        {
            CollectedTargets.Add(item);
        }
        //Dispatcher.Invoke(() =>
        //{
        //    userGrid.ItemsSource = null;
        //    userGrid.ItemsSource = _collectionList;
        //    userGrid.Items.Refresh();
        //});
    }

    private static SSH ssh;
    public void StartSSHConnection(string ip, User user)
    {
        ssh = new SSH(ip, user.Username, user.Password);
        if (ssh.TryConnect(1))
        {
            ssh.SendCMD("whoami", this);
            _isConnected = true;
        }
        else
        {
            Dispatcher.Invoke(() => { LogBox.AppendText($"[!] {helperObject.GetTime()}: Connection to {ip} failed" + Environment.NewLine); });
        }
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var args = CommandText.Trim(); // commandText.Text.Trim();
        CurrentUser = args.Split('@')[0];
        CurrentPassword = args.Split(':')[1];
        CurrentIP = args.Split('@')[1].Split(':')[0];
        User User = new User(CurrentUser, CurrentPassword);
        StartSSHConnection(CurrentIP, User);
    }

    private void ShowAlert(string title, string message, int state)
    {
        var dialogWindow = new ShellDialogWindow(title, message, state);
        dialogWindow.ShowDialog();
    }

    private void UploadFileToTarget(object sender, RoutedEventArgs e)
    {
        if (!_isConnected)
        {
            ShowAlert("No Connection", "Must have a valid connection\nbefore attempting file uploads", 2);
        }
        else
        {        
            var op = new OpenFileDialog();
            op.RestoreDirectory = true;
            op.ShowDialog();
            var localFilePath = op.FileName;
            var result = ServerExtensions.SendFile(CurrentIP, CurrentUser, CurrentPassword, localFilePath);
            AddResult(result);
            var i = 0;
            if (result.Contains("successful")) { i = 0; }
            else { i = 2; }
            ShowAlert("File Upload", result, i);
        }
    }

    private void logRichTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        try
        {
            LogBox.ScrollToEnd();
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }

    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void userGrid_SelectedCellsChanged_1(object sender, SelectedCellsChangedEventArgs e)
    {
        if (termGrid.CurrentItem != null)
        {
            CollectionListing selectedItem = (CollectionListing)termGrid.CurrentItem;
            if (selectedItem != null)
            {
                Dispatcher.Invoke(() => { connectionString.Text = $"{selectedItem.Username}@{selectedItem.IPAddress}:{selectedItem.Password}"; });
            }
        }        
    }

    private void commandText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            if (ssh.TryConnect(1))
            {
                ssh.SendCMD(CommandText.Trim().Replace("> ", ""), this);  //commandText.Text.Trim().Replace("> ", ""), this);
                Dispatcher.Invoke(() =>
                {
                    CommandText = "> "; // commandText.Text = "> ";
                });
            }
        }
    }
}
