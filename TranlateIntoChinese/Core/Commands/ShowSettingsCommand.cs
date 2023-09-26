namespace TranlateIntoChinese.Core
{
    [Command(PackageIds.ShowSettingsCommand)]
    internal sealed class ShowSettingsCommand : BaseCommand<ShowSettingsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await SettingsToolWindow.ShowAsync();
        }
    }
}
