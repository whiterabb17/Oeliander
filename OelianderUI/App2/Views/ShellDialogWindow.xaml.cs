using System.Windows;
using System.Windows.Controls;

using OelianderUI.Contracts.Views;

using MahApps.Metro.Controls;

namespace OelianderUI.Views;

public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
{
    public string DialogHeadingString { get; set; }
    public string DialogTextString { get; set; }
    private readonly string ExclamationGlyph = "\uE783";
    private readonly string WarningGlyph = "\uEB56";
    private readonly string SuccessGlyph = "\uE73E";

    public ShellDialogWindow(string heading, string text, int state)
    {
        InitializeComponent();
        DataContext = this;
        DialogHeadingString = heading;
        DialogTextString = text;
        switch (state)
        {
            case 0:
                DialogIcon.Glyph = SuccessGlyph;
                break;
            case 1:
                DialogIcon.Glyph = ExclamationGlyph;
                break;
            case 2:
                DialogIcon.Glyph = WarningGlyph;
                break;
        }
    }

    public Frame GetDialogFrame()
        => dialogFrame;

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
