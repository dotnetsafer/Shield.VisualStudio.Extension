﻿using System.Linq;
using System.Windows;
using MaterialDesignThemes.Wpf;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Services;
using ShieldVSExtension.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace ShieldVSExtension.UI;

/// <summary>
///   Interaction logic for MainWindowControl.xaml
/// </summary>
public partial class MainWindowControl
{
    // private const string ExtensionConfigurationFile = "ExtensionConfiguration";
    private readonly MainViewModel _vm;
    // private Application _app;

    public MainWindowControl(MainViewModel vm)
    {
        InitializeMaterialDesign();
        InitializeComponent();

        // _app = Application.Current;
        // _app.Exit += OnExit;

        _vm = vm;
        DataContext = vm;

        Loaded += OnLoaded;
        // ViewModelBase.IsMsbuilderInstalledHandler += OnInstalled;
    }

    private void InitializeMaterialDesign()
    {
        _ = new MahApps.Metro.Controls.MetroWindow();
        _ = new Card();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AppNameBox.Text = Globals.AppName;

        ViewModelBase.ProjectChangedHandler.Invoke(_vm.Projects.FirstOrDefault());

        CheckVersionAsync().GetAwaiter();

        var helper = new NugetHelper();
        var isInstalled = helper.IsPackageInstalled(_vm.SelectedProject.Project, NugetHelper.PackageId, null);
        _vm.SelectedProject.Installed = isInstalled;
        ViewModelBase.IsMsbuilderInstalledHandler.Invoke(isInstalled);
    }

    private async Task CheckVersionAsync()
    {
        var vsixVersion = Utils.GetVersionNumber().Get();
        var info = await CoreService.GetVersionInfoAsync(EPackageType.Vsix, vsixVersion);

        if (info.CanUpdate || info.ForceUpdate)
        {
            if (info is { ForceUpdate: true })
            {
                TabControl.IsEnabled = false;
            }

            ViewModelBase.VsixVersionHandler.Invoke(info);
        }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        var window = GetWindow(this);
        window?.Close();
    }
}