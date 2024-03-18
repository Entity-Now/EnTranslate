using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TranslateIntoChinese.Core
{
    public class SettingsToolWindow : BaseToolWindow<SettingsToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "SettingsToolWindow";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new SettingsToolWindowControl());
        }

        [Guid("1cbc01da-98ca-44f8-b13a-a2e76d90318a")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.Settings;
            }
        }
    }
}
