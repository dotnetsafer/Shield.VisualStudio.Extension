﻿using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet;
using NuGet.VisualStudio;
using MessageBox = System.Windows.MessageBox;

namespace ShieldVSExtension.Common.Helpers;

[Export(typeof(NugetHelper))]
internal class NugetHelper
{
    private const string PackageId = "Bytehide.Shield.Integration";
    // private static readonly SemanticVersion PackageVersion = new("1.0.0");

    [Import(typeof(IVsPackageInstaller2))] private IVsPackageInstaller2 _packageInstaller;

    [Import(typeof(IVsPackageUninstaller))]
    private IVsPackageUninstaller _packageUninstaller;

    public Task InstallPackageAsync(Project project, SemanticVersion packageVersion)
    {
        try
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var installerServices = componentModel.GetService<IVsPackageInstallerServices>();

            _packageInstaller = componentModel.GetService<IVsPackageInstaller2>();

            if (_packageInstaller == null)
            {
                MessageBox.Show(
                    "Package installer is not available. Please make sure that NuGet is installed and enabled in Visual Studio.",
                    "Error"
                );

                return Task.CompletedTask;
            }

            if (installerServices.IsPackageInstalled(project, PackageId, packageVersion))
            {
                // MessageBox.Show("Package already installed", "Info");
                // ViewModelBase.IsMsbuilderInstalledHandler.Invoke(true);
                return Task.CompletedTask;
            }

            _packageInstaller?.InstallLatestPackage(
                source: null,
                project,
                PackageId,
                includePrerelease: false,
                ignoreDependencies: false
            );

            // MessageBox.Show(@"Package installed successfully");
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error during package installation: {ex.Message}");
        }

        // ViewModelBase.IsMsbuilderInstalledHandler.Invoke(true);
        return Task.CompletedTask;
    }

    public bool IsPackageInstalled(Project project, SemanticVersion packageVersion, string packageId = PackageId)
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        var installerServices = componentModel.GetService<IVsPackageInstallerServices>();

        var installed = installerServices.IsPackageInstalled(project, packageId, packageVersion);
        // ViewModelBase.IsMsbuilderInstalledHandler.Invoke(installed);
        return installed;
    }

    public Task UninstallPackageAsync(Project project)
    {
        try
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var installerServices = componentModel.GetService<IVsPackageInstallerServices>();

            _packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();

            if (!installerServices.IsPackageInstalled(project, PackageId))
            {
                // MessageBox.Show(@"Package not installed");
                // ViewModelBase.IsMsbuilderInstalledHandler.Invoke(false);
                return Task.CompletedTask;
            }

            if (_packageUninstaller == null)
            {
                MessageBox.Show(
                    "Package installer is not available. Please make sure that NuGet is installed and enabled in Visual Studio.",
                    "Error"
                );

                return Task.CompletedTask;
            }

            _packageUninstaller?.UninstallPackage(project, PackageId, false);

            // MessageBox.Show(@"Package uninstalled successfully");
        }
        catch (Exception ex)
        {
            // Console.WriteLine($@"Error during package uninstallation: {ex.Message}");
            Debug.WriteLine($@"Error during package uninstallation: {ex.Message}");
        }

        // ViewModelBase.IsMsbuilderInstalledHandler.Invoke(false);
        return Task.CompletedTask;
    }
}