using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    internal static class QuickActionRunner
    {
        private static readonly Regex FenceStart = new Regex(@"^```[a-zA-Z0-9]*\s*", RegexOptions.Compiled);
        private static readonly Regex FenceEnd = new Regex(@"\s*```$", RegexOptions.Compiled);
        private static readonly Regex Identifier = new Regex(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

        public static async Task RunAsync(ITextView view, SnapshotSpan originalSpan, QuickActionItem action)
        {
            if (view == null || action == null) return;

            var cfg = Constants.Config;
            if (cfg == null || !cfg.UseAiTranslate)
            {
                await VS.StatusBar.ShowMessageAsync("请先在设置中启用 AI 大模型");
                return;
            }

            var snapshot = originalSpan.Snapshot;
            if (snapshot == null) return;

            var tracking = snapshot.CreateTrackingSpan(originalSpan, SpanTrackingMode.EdgeInclusive);
            var source = originalSpan.GetText();
            if (string.IsNullOrWhiteSpace(source)) return;

            await VS.StatusBar.ShowMessageAsync("AI 指令处理中：「" + action.Title + "」");

            string raw;
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    raw = await AiTranslateService.CompleteAsync(source, action.Prompt, cfg, cts.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                await VS.StatusBar.ShowMessageAsync("AI 指令已超时或被取消");
                return;
            }
            catch (Exception ex)
            {
                await VS.StatusBar.ShowMessageAsync("AI 指令失败：" + ex.Message);
                return;
            }

            var replacement = Sanitize(raw, action.IdentifierOutput);
            if (string.IsNullOrEmpty(replacement))
            {
                await VS.StatusBar.ShowMessageAsync("AI 未返回可替换的结果");
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var buffer = view.TextBuffer;
                if (buffer == null || buffer.EditInProgress) return;

                var current = tracking.GetSpan(buffer.CurrentSnapshot);
                if (current.Length == 0 && originalSpan.Length > 0) return;

                using (var edit = buffer.CreateEdit())
                {
                    edit.Replace(current, replacement);
                    edit.Apply();
                }

                await VS.StatusBar.ShowMessageAsync("已应用：「" + action.Title + "」");
            }
            catch (Exception ex)
            {
                await VS.StatusBar.ShowMessageAsync("替换失败：" + ex.Message);
            }
        }

        internal static string Sanitize(string raw, bool identifier)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var text = raw.Trim();
            text = FenceStart.Replace(text, string.Empty);
            text = FenceEnd.Replace(text, string.Empty);
            text = text.Trim().Trim('"', '\'', '“', '”', '`');

            if (text.StartsWith("译文：", StringComparison.Ordinal) ||
                text.StartsWith("译文:", StringComparison.Ordinal))
                text = text.Substring(3).Trim();

            if (!identifier) return string.IsNullOrWhiteSpace(text) ? null : text;

            var compact = text.Replace("\r", " ").Replace("\n", " ").Trim();
            var match = Identifier.Match(compact);
            return match.Success ? match.Value : (string.IsNullOrWhiteSpace(compact) ? null : compact.Replace(" ", ""));
        }
    }
}
