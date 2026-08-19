using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using System.Threading;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    [Command(PackageIds.TranslateSelectionCommand)]
    internal sealed class TranslateSelectionCommand : BaseCommand<TranslateSelectionCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ExecuteFromHotkeyAsync(null);
        }

        internal static async Task ExecuteFromHotkeyAsync(ITextView preferredView)
        {
            if (!TranslateCommandGuard.TryEnter()) return;

            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var view = preferredView ?? await GetActiveViewAsync();
                if (view == null)
                {
                    await VS.StatusBar.ShowMessageAsync("没有活动的文本编辑器");
                    return;
                }

                var text = EditorTextPicker.GetSelectedOrWordOrLine(view);
                if (string.IsNullOrWhiteSpace(text))
                {
                    await VS.StatusBar.ShowMessageAsync("没有可翻译的选区或单词");
                    return;
                }

                TranslateTrigger.Arm();

                var triggered = await TryTriggerQuickInfoAsync(view);
                if (triggered) return;

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                {
                    var translated = await TranslationCoordinator.TranslateOneAsync(text, cts.Token);
                    if (string.IsNullOrWhiteSpace(translated))
                    {
                        await VS.StatusBar.ShowMessageAsync("翻译失败：请检查远程翻译或 AI 配置");
                        return;
                    }

                    var preview = translated.Length > 160 ? translated.Substring(0, 160) + "…" : translated;
                    await VS.StatusBar.ShowMessageAsync("译：" + preview);
                }
            }
            catch (Exception ex)
            {
                try { await ex.LogAsync(); }
                catch { /* ignore */ }
                await VS.StatusBar.ShowMessageAsync("翻译命令执行失败");
            }
        }

        private static async System.Threading.Tasks.Task<ITextView> GetActiveViewAsync()
        {
            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            return doc?.TextView;
        }

        private static async System.Threading.Tasks.Task<bool> TryTriggerQuickInfoAsync(ITextView view)
        {
            try
            {
                var componentModel = await VS.GetServiceAsync<SComponentModel, IComponentModel>();
                var broker = componentModel?.GetService<IAsyncQuickInfoBroker>();
                if (broker == null || view == null) return false;

                var session = await broker.TriggerQuickInfoAsync(view);
                return session != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TriggerQuickInfo failed: {ex.Message}");
                return false;
            }
        }
    }
}
