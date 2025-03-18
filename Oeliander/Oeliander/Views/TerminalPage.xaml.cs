using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
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
        ServerExtensions.term = this;
    }

    #region locals
    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<CollectionListing> CollectedTargets { get; } = new();
    #endregion locals

    public void AddResult(string text)
    {
        try
        {
            Dispatcher.Invoke(() => { LogBox.AppendText(text + Environment.NewLine); });
        }
        catch (Exception E)
        {
            Objects.obj.HandleException(E);
        }
    }

    public void SessionResult(string text)
    {
        try
        {
            Dispatcher.Invoke(() => { connectionString.Text = ""; connectionString.Text = text; });
        }
        catch (Exception E)
        {
            Objects.obj.HandleException(E);
            Objects.ShowAlert("Unable to Select Target", E.Message, 1);
        }
    }

    private void FillConnectionList(List<CollectionListing> _collectionList)
    {
        foreach (var item in _collectionList)
        {
            CollectedTargets.Add(item);
        }
    }

    public void StartSSHConnection(string ip, User user)
    {
        Objects.ssh = new SSH(ip, user.Username, user.Password);
        if (Objects.ssh.TryConnect(1))
        {
            Objects.ssh.SendCMD("whoami", 1);
        }
        else
        {
            Dispatcher.Invoke(() => { LogBox.AppendText($"[!] {Objects.GetTime()}: Connection to {ip} failed" + Environment.NewLine); });
        }
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        switch (ConnectButton.Content.ToString().ToLower())
        {
            case "connect":
                var args = connectionString.Text.Trim(); // commandText.Text.Trim();
                Objects.CurrentUser = args.Split('@')[0];
                Objects.CurrentPassword = args.Split(':')[1];
                Objects.CurrentIP = args.Split('@')[1].Split(':')[0];
                User User = new User(Objects.CurrentUser, Objects.CurrentPassword);
                StartSSHConnection(Objects.CurrentIP, User);
                ConnectButton.Content = "Disconnent";
                break;
            case "disconnect":
                ConnectButton.Content = "Connect";
                Objects.ssh.Client.Disconnect();
                break;
        }        
    }

    private void UploadFileToTarget(object sender, RoutedEventArgs e)
    {
        if (Objects.ssh.TryConnect(1))
        {
            var op = new OpenFileDialog();
            op.RestoreDirectory = true;
            op.ShowDialog();
            var localFilePath = op.FileName;
            var result = ServerExtensions.SendFile(Objects.CurrentIP, Objects.CurrentUser, Objects.CurrentPassword, localFilePath);
            AddResult(result);
            var i = 0;
            if (result.Contains("successful")) { i = 0; }
            else { i = 2; }
            Objects.ShowAlert("File Upload", result, i);
        }
        else
        {
            Objects.ShowAlert("No Connection", "Must have a valid connection\nbefore attempting file uploads", 2);
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
            Objects.obj.HandleException(E);
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
            try
            {
                Objects.ssh.Connect(1);
                var output = Objects.ssh.ExecuteCommand(commandText.Text.Trim());
                AddResult(output);
            }
            catch (Exception ex)
            {
                Objects.ShowAlert("Connection Failed", "Connection failed to send command", 1);
                Objects.obj.HandleException(ex);
            }
            //if (Objects.ssh.TryConnect(1))
            //{
            //    Objects.ssh.SendCMD(commandText.Text.Trim(), 1);  //commandText.Text.Trim().Replace("> ", ""), this);

            //    Dispatcher.Invoke(() =>
            //    {
            //        commandText.Text = "";
            //    });
            //}
            //else
            //{
                
            //}      
        }
    }
}
