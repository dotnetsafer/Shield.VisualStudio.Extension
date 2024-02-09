﻿using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Common.Validators;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;
using Globals = ShieldVSExtension.Common.Globals;

namespace ShieldVSExtension.UI.Views;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl
{
    public SecureLocalStorage LocalStorage { get; set; }

    // public ObservableCollection<string> SolutionConfigurations { get; set; }
    // private readonly SettingsViewModel _vm;

    public SettingsControl()
    {
        InitializeComponent();
        Initialized += OnInitialized;
        ViewModelBase.ProjectChangedHandler += OnRefresh;
    }

    private void OnInitialized(object sender, EventArgs e)
    {
        // Task.Delay(3000).ConfigureAwait(false).GetAwaiter().OnCompleted(() => LoadDataAsync().GetAwaiter());
        LoadDataAsync().GetAwaiter();
    }

    // private void OnLoaded(object sender, RoutedEventArgs e) => Refresh();

    private void OnRefresh(ProjectViewModel payload)
    {
        if (payload == null) return;

        Payload = payload;
        LoadDataAsync().GetAwaiter();
    }

    // private void OnSelected(ETabType tab)
    // {
    //     if (tab != ETabType.SettingsTab || Payload == null) return;
    // 
    //     Refresh();
    // }

    private async Task LoadDataAsync()
    {
        if (Payload == null) return;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
            .WithDefaultKeyBuilder());

        var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName);

        ProjectNameBox.Text = data?.Name ?? $"Configuration {Payload.Project.Name}";
        ProjectTokenBox.Text = data?.ProjectToken ?? string.Empty;
        SecretBox.Password = data?.ProtectionSecret ?? string.Empty;
        StatusToggle.IsChecked = data?.Enabled ?? true;

        if (data != null)
        {
            var configurationsTable = Payload.Project.ConfigurationManager.ConfigurationRowNames;
            if (configurationsTable is IEnumerable enumerableConfigurations)
            {
                var runConfiguration = data.RunConfiguration;
                var count = 1;

                foreach (var configuration in enumerableConfigurations)
                {
                    if (configuration is not string configName || configName != runConfiguration)
                    {
                        ++count;
                        continue;
                    }

                    ProjectRunCombo.SelectedIndex = count;
                    break;
                }
            }

            var validationRule = new ProjectTokenValidationRule();
            var validationResult = validationRule.Validate(data.ProjectToken, CultureInfo.CurrentCulture);

            SaveButton.IsEnabled = validationResult.IsValid;
        }

        if (ProjectRunCombo.SelectedIndex == -1)
        {
            // ProjectRunCombo.SelectedItem ??= Payload.Project.ConfigurationManager.ActiveConfiguration;
            ProjectRunCombo.SelectedIndex = Payload.Project.ConfigurationManager.Count - 1;
        }

        // var runConfigurations = Payload.Project.ConfigurationManager.Cast<Configuration>().Select(x => x.ConfigurationName);
        // var itemsSource = runConfigurations as string[] ?? runConfigurations.ToArray();
        // 
        // ProjectRunCombo.ItemsSource = itemsSource;
        // ProjectRunCombo.SelectedIndex = itemsSource.ToList().IndexOf(data.RunConfiguration);
    }

    private void SaveButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { IsInitialized: true } control) return;
        if (!control.IsMouseOver) return;
        if (Payload == null) return;

        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
            .WithDefaultKeyBuilder());

        var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ?? new ShieldConfiguration();
        dynamic runConfigurationSelected = ProjectRunCombo.SelectedItem;

        data.Name = $"{ProjectNameBox.Text}";
        data.Preset ??= EPresetType.Optimized.ToFriendlyString();
        data.ProjectToken = ProjectTokenBox.Text;
        data.ProtectionSecret = SecretBox.Password;
        data.Enabled = StatusToggle.IsChecked ?? false;
        data.RunConfiguration = runConfigurationSelected?.ConfigurationName ?? "Release";

        LocalStorage.Set(Payload.Project.UniqueName, data);

        var saved = FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
            JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));

        ViewModelBase.ProjectChangedHandler.Invoke(Payload);

        MessageBox.Show(saved ? $"Saving for {Payload.Name}" : $"Failed to save for {Payload.Name}");
    }

    private void ProjectTokenBoxOnChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox control) return;
        var password = control.Text;

        var validationRule = new ProjectTokenValidationRule();
        var validationResult = validationRule.Validate(password, CultureInfo.CurrentCulture);

        SaveButton.IsEnabled = validationResult.IsValid;
    }

    #region Commands

    public ProjectViewModel Payload
    {
        get => (ProjectViewModel)GetValue(PayloadProperty);
        set => SetValue(PayloadProperty, value);
    }

    public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
        nameof(Payload),
        typeof(ProjectViewModel),
        typeof(SettingsControl),
        new PropertyMetadata(null));

    #endregion
}