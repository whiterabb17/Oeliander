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
        ServerExtensions.term = this;
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
    }

    public string GetTime()
    {
        return "[" + DateTime.Now.ToString("G") + "]";
    }
    public void StartSSHConnection(string ip, User user)
    {
        Objects.ssh = new SSH(ip, user.Username, user.Password);
try 
{
Objects.ssh.Connect();
var output = ssh.CreateCommand("whoami");
var result = ssh.Execute();
AddResult(result);
}
catch (Exception ex)
{
Objects.obj.HandleException(ex);
}
      //  if (Objects.ssh.TryConnect(1))
      //  {
      //      Objects.ssh.SendCMD("whoami", 1);
      //  }
      //  else
      //  {
      //      Dispatcher.Invoke(() => { 
// LogBox.AppendText($"[!] {GetTime()}: Connection to {ip} failed" + Environment.NewLine); });
//        }
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        switch (ConnectButton.Content.ToString().ToLower())
        {
            case "connect":
                var args = connectionString.Text.Trim(); // commandText.Text.Trim();
                CurrentUser = args.Split('@')[0];
                CurrentPassword = args.Split(':')[1];
                CurrentIP = args.Split('@')[1].Split(':')[0];
                User User = new User(CurrentUser, CurrentPassword);
                StartSSHConnection(CurrentIP, User);
                ConnectButton.Content = "Disconnent";
                break;
            case "disconnect":
                ConnectButton.Content = "Connect";
                Objects.ssh.Client.Disconnect();
                break;
        }        
    }

    private void ShowAlert(string title, string message, int state)
    {
        var dialogWindow = new ShellDialogWindow(title, message, state, false);
        dialogWindow.ShowDialog();        
    }

    private void UploadFileToTarget(object sender, RoutedEventArgs e)
    {
        if (Objects.ssh.TryConnect(1))
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
        else
        {
            ShowAlert("No Connection", "Must have a valid connection\nbefore attempting file uploads", 2);
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
            //if (_isConnected)
            //{
            //    Objects.ssh.SendCMD(commandText.Text.Trim().Replace("> ", ""), 1);  //commandText.Text.Trim().Replace("> ", ""), this);

            //    Dispatcher.Invoke(() =>
            //    {
            //        commandText.Text = "> "; // commandText.Text = "> ";
            //        commandText.ScrollToEnd();
            //    });
            //}
            //else 
            //{
                if (Objects.ssh.TryConnect(1))
                {
                    Objects.ssh.SendCMD(commandText.Text.Trim(), 1);  //commandText.Text.Trim().Replace("> ", ""), this);

                    Dispatcher.Invoke(() =>
                    {
                        commandText.Text = "";
                    });
                }
                else
                {
                    ShowAlert("Connection Failed", "Connection failed to send command", 1);
                }
            //}            
        }
    }
}
