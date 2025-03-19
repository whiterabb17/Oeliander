using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OelianderUI.Views;
using Renci.SshNet;

namespace OelianderUI.Helpers;

public class SSH
{
    public bool Enabled { get; set; }
    public SshClient Client { get; set; }
    public ShellStream Shell { get; set; }
    public Thread Thread { get; set; }
    public string IP { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public SSH(string ip, string username, string password)
    {
        IP = ip;
        Username = username;
        Password = password;
    }
}

public static class ServerExtensions
{
    #region General Logic
    public static bool TryConnect(this SSH ssh, int window)
    {
        try
        {
            if (ssh.Client == null || ssh.Shell == null || !ssh.Client.IsConnected)
                return ssh.Connect(window);
            else if (ssh.Client.IsConnected)
                return true;
            else
                return false;
        }
        catch { return false; }
    }
    public static bool Connect(this SSH ssh, int window)
    {
        try
        {
            ssh.Client = new SshClient(ssh.IP, 22, ssh.Username, ssh.Password);
            ssh.Client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32("3000"));
            ssh.Client.Connect();
            ssh.Shell = ssh.Client.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);

            Thread thread = new Thread(() => ssh.Receiver(window));
            thread.Start();
            return ssh.Client.IsConnected;
        }
        catch (Exception ex)
        {
            switch (window)
            {
                case 0:
                    Objects.main.AddLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                    break;
                case 1:
                    Objects.term.SessionResult($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                    break;
                case 2:
                    var dialogResult = new ShellDialogWindow("Error", $"{ssh.Username}@{ssh.IP}: {ex.Message}", 1, false);
                    dialogResult.ShowDialog();
                    break;
            }
            return false;
        }
    }

    public static string ExecuteCommand(this SSH ssh, string command)
    {
        try
        {
            ssh.Shell.Write(command);
            ssh.Shell.Flush();
            if (ssh.Shell != null && ssh.Shell.DataAvailable)
            {
                var content = ssh.Shell.Read();
                return content;
            }
            else
            {
                return "";
            }
        }
        catch (Exception ex)
        {
            Objects.obj.HandleException(ex);
            Objects.ShowAlert("Connection Error", ex.Message, 1);
            return ex.Message;
        }
    }

    public static void SendCMD(this SSH ssh, string cmd, int window)
    {
        try
        {
            if (TryConnect(ssh, window))
            {
                ssh.Shell.Write(cmd + "\n");
                ssh.Shell.Flush();
            }
        }
        catch (Exception ex) 
        {
            switch (window)
            {
                case 0:
                    Objects.main.AddLog($"{ssh.Username}@{ssh.IP}: {ex.Message}");
                    break;
                case 1:
                    Objects.term.AddResult($"ERROR: {ssh.Username}@{ssh.IP} {ex.Message}");
                    break;
                case 2:
                    var dialogResult = new ShellDialogWindow("Error", $"{ssh.Username}@{ssh.IP}: {ex.Message}", 1, false);
                    dialogResult.ShowDialog();
                    break;
            }            
        }
    }

    public static void Receiver(this SSH ssh, int window)
    {
        while (true)
        {
            try
            {
                if (ssh.Shell != null && ssh.Shell.DataAvailable)
                {
                    var content = ssh.Shell.Read();
                    switch (window)
                    {
                        case 0:
                            Objects.main.AddLog($"{ssh.Username}@{ssh.IP}: {content}");
                            if (ScanHelper.carryon == 1)
                            {
                                if (content.Contains("error"))
                                    ScanHelper.carryon = 2;
                                else
                                    ScanHelper.carryon = 0;
                            }
                            break;
                        case 1:
                            Objects.term.AddResult($"{ssh.Username}@{ssh.IP}: {content.Replace("\n","")}");
                            break;
                    }                    
                }
            }
            catch { }
            Thread.Sleep(200);
            
        }
    }

    public static void RecieveFile(string hostname, string username, string password, string localFileName, string remoteFilePath)
    {
        var scpClient = new SftpClient(hostname, username, password);
        if (!Directory.Exists("Clients"))
            Directory.CreateDirectory("Clients");
        if (!Directory.Exists($"Clients/{hostname}"))
            Directory.CreateDirectory($"Clients/{hostname}");
        scpClient.Connect();

        if (scpClient.IsConnected)
        {
            using (var fileStream = new FileStream($"Clients/{hostname}/{localFileName}", FileMode.Create))
            {
                scpClient.DownloadFile(remoteFilePath, fileStream);
            }

            Objects.ShowAlert("Success", $"File {localFileName} was downloaded successfully to:\n -> Clients/{hostname}/{localFileName}", 0);
        }
        else
        {
            Objects.ShowAlert("Failed", $"Unable to download {localFileName} from:\n -> {remoteFilePath}", 0);
        }

        scpClient.Disconnect();
    }

    public static string SendFile(string hostname, string username, string password, string localFilePath, string remoteFilePath = "/tmp")
    {
        try
        {
            var scpClient = new ScpClient(hostname, username, password);
            scpClient.Connect();
            // Send the file using SCP
            var fileStream = System.IO.File.OpenRead(localFilePath);
            remoteFilePath += $"/{Path.GetFileName(localFilePath)}";
            scpClient.Upload(fileStream, remoteFilePath);
            return $"File uploaded successfully:\n{remoteFilePath}";            
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    #endregion General Logic
}
