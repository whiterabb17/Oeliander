using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OelianderUI.Views;

namespace OelianderUI.Helpers
{
    internal class Objects
    {
        internal static SSH ssh { get; set; }
        internal static Settings settings { get; set; }
        internal static MainPage main { get; set; }
        internal static TerminalPage term { get; set; }
        internal static DirectoryViewPage dirViewer { get; set; }
        internal static Objects obj = new();
        internal static string CurrentUser { get; set; }
        internal static string CurrentIP { get; set; }
        internal static string CurrentPassword { get; set; }
        private static List<string> errorList { get; set; }

        private readonly ReaderWriterLockSlim _readWriteLock = new();

        internal static void ShowAlert(string title, string message, int state)
        {
            var dialogWindow = new ShellDialogWindow(title, message, state, false);
            dialogWindow.ShowDialog();
        }
        public static string GetTime()
        {
            return "[" + DateTime.Now.ToString("G") + "]";
        }

        public void HandleException(Exception E)
        {
            errorList ??= new List<string>();
            StackTrace trace = new StackTrace(E, true);
            var data = trace.GetFrame(0).GetFileLineNumber().ToString() + ": " + E.Message;
            if (!errorList.Contains(data))
            {
                errorList.Add(data);
                Save(data);
            }
        }

        public string OpenDialogWindowWithResult(string title, string message)
        {
            string returnItem = "";
            var dialogWindow = new ShellDialogWindow(title, message, 0, true);
            dialogWindow.ShowDialog();
            if (dialogWindow.DialogReturnText != "")
            {
                returnItem = dialogWindow.DialogReturnText;
            }
            dialogWindow.Close();
            return returnItem;
        }

        public void Save(string text)
        {
            Thread.Sleep(1000);
            _readWriteLock.EnterWriteLock();
            try
            {
                var resultPath = System.IO.Directory.GetCurrentDirectory() + "\\Oeilander.log";
                using System.IO.StreamWriter st = System.IO.File.AppendText(resultPath);
                st.WriteLine(text);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}
