using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using OelianderUI.Contracts.Views;
using OelianderUI.Core.Contracts.Services;
using OelianderUI.Core.Models;
using OelianderUI.Helpers;
using Microsoft.Win32;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows;
using ControlzEx.Standard;
using Newtonsoft.Json;
using Renci.SshNet.Messages;
using System.Reflection;
using OelianderUI.Helpers.Exploits;

namespace OelianderUI.Views;

public partial class F5BigIPPage : Page, INotifyPropertyChanged
{
    #region locals

    public ScanHelper helperObject { get; set; }
    internal static bool ShodanScan = false;
    internal static string _targetingString = "";
    public ObservableCollection<ScanResult> Source { get; } = new();
    public List<string> collectedCredentials = new();
    public List<string> rosVersion = new();
    public static Dictionary<Helpers.User, string> _staticList = new();
    public bool SaveShodanOnly = false; 
    private static int _tabulation = 0; 
    internal static Settings settings = new();
    public static List<CollectionListing> _collectionList = new();
    private readonly ReaderWriterLockSlim _readWriteLock = new();

    #endregion locals

    public void AddLog(string text)
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
            Objects.obj.HandleException(E);
        }
    }
    public void AddToLogFile(string text)
    {
        Thread.Sleep(1000);
        _readWriteLock.EnterWriteLock();
        try
        {
            var resultPath = Directory.GetCurrentDirectory() + "\\Oeilander.log";
            using StreamWriter st = File.AppendText(resultPath);
            st.WriteLine(text);
        }
        finally
        {
            _readWriteLock.ExitWriteLock();
        }
    }
    public void AddLog(string text, object obj)
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
            Objects.obj.HandleException(E);
        }
    }
    public F5BigIPPage() //IScanResultService scanResultService)
    {
        //_scanResults = scanResultService;
        InitializeComponent();
        DataContext = this;
        Objects.f5Page = this;
        if (!File.Exists("Oeliander.log")) { File.Create("Oeliander.log"); }
    }
    public void ScanStop()
    {
        Dispatcher.Invoke(() =>
        {
            StartScanButton.Content = "Start";
            AddLog(Environment.NewLine + Objects.GetTime() + ": Scan stopped successfully\n");
            AddToLogFile("\n\n\t[*] End of Scan: " + Objects.GetTime() + "\n\n###############################################################################\n\n");
        });
    }
    
    public void OnNavigatedFrom()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

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

    private void GetLogs(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer", "Oeliander.log");
    }
    private void CheckResults(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer", ".\\Results");
    }
    private void ClearLogs(object sender, RoutedEventArgs e)
    {
        File.WriteAllText("Oeliander.log", "Oeliander Exploit Logs\n--------------------------------\n\n");
    }
    readonly string banner = @"
  ______     _______     ____   ___ ____  _____       _  _    __ _____ _  _ _____ 
 / ___\ \   / / ____|   |___ \ / _ \___ \|___ /      | || |  / /|___  | || |___  |
| |    \ \ / /|  _| _____ __) | | | |__) | |_ \ _____| || |_| '_ \ / /| || |_ / / 
| |___  \ V / | |__|_____/ __/| |_| / __/ ___) |_____|__   _| (_) / / |__   _/ /  
 \____|  \_/  |_____|   |_____|\___/_____|____/         |_|  \___/_/     |_|/_/   
                                                                            
        ";
    private void StartScan(object sender, System.Windows.RoutedEventArgs e)
    {
        if (StartScanButton.Content.ToString() == "Start")
        {
            helperObject.SaveScanTime();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            AddLog(banner);
            Thread tr = new Thread(() => F5Helper.TryExploitF5(TargetTextBox.Text, proxyAddressBox.Text));
            tr.Start();
        }
        else if (StartScanButton.Content.ToString() == "Stop")
        {
            //helperObject.Stop();
            ScanStop();
        }
    }
}
