﻿using System.Windows;

namespace OelianderUI.Contracts.Services;

public interface IWindowManagerService
{
    Window MainWindow { get; }

    void OpenInNewWindow(Type pageType, object parameter = null);

    bool? OpenInDialog(Type pageType, object parameter = null);

    Window GetWindow(Type pageType);
}
