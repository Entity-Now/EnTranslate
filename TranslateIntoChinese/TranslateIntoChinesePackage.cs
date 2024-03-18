global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using TranslateIntoChinese.Core;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SettingsToolWindow.Pane),Style = VsDockStyle.Linked, Window = WindowGuids.MainWindow)]
    [Guid(PackageGuids.TranslateIntoChineseString)]
    public sealed class TranslateIntoChinesePackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
            // load config
            Config.Load();
        }
    }
}