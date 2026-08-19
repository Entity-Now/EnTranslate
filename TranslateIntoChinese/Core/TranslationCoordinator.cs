using MoqDictionary.utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    /// <summary>
    /// 统一走本地词库 / 传统在线引擎 / AI。本地词库未命中时才发网络请求。
    /// </summary>
    public static class TranslationCoordinator
    {
        private const int CacheLimit = 256;
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public static bool CanTranslateRemotely
        {
            get
            {
                var cfg = Constants.Config;
                return cfg != null && (cfg.UseAiTranslate || cfg.IsRemoteTranslate);
            }
        }

        public static bool CanTranslateDocuments
        {
            get
            {
                var cfg = Constants.Config;
                return cfg != null && cfg.EnableDocumentTranslate && CanTranslateRemotely;
            }
        }

        public static async Task<string> TranslateOneAsync(string text, CancellationToken cancellationToken)
        {
            var list = await TranslateManyAsync(new[] { text }, cancellationToken).ConfigureAwait(false);
            return list == null || list.Count == 0 ? null : list[0];
        }

        public static async Task<IList<string>> TranslateManyAsync(IList<string> texts, CancellationToken cancellationToken)
        {
            if (texts == null || texts.Count == 0) return Array.Empty<string>();

            var cfg = Constants.Config;
            if (cfg == null || !CanTranslateRemotely) return Array.Empty<string>();

            var pending = new List<string>();
            var pendingIndex = new List<int>();
            var result = new string[texts.Count];

            for (int i = 0; i < texts.Count; i++)
            {
                var text = (texts[i] ?? string.Empty).Trim();
                if (text.Length == 0) continue;

                var key = BuildCacheKey(cfg, text);
                if (Cache.TryGetValue(key, out var cached) && !string.IsNullOrEmpty(cached))
                {
                    result[i] = cached;
                    continue;
                }

                pending.Add(text);
                pendingIndex.Add(i);
            }

            if (pending.Count == 0)
                return result.Where(s => !string.IsNullOrEmpty(s)).ToList();

            IList<string> remote;
            try
            {
                if (cfg.UseAiTranslate)
                {
                    remote = await AiTranslateService.TranslateManyAsync(pending, cfg, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    remote = await TranslateWithTimeout(
                        () => TranslateHelper.getTranslateAsync(cfg.TranslateType, pending.ToList()),
                        TimeSpan.FromSeconds(8),
                        cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TranslateManyAsync failed: {ex.Message}");
                remote = Array.Empty<string>();
            }

            if (remote != null)
            {
                for (int i = 0; i < pending.Count && i < remote.Count; i++)
                {
                    var value = remote[i];
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    result[pendingIndex[i]] = value.Trim();
                    Remember(BuildCacheKey(cfg, pending[i]), value.Trim());
                }
            }

            return result.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private static async Task<IList<string>> TranslateWithTimeout(
            Func<Task<List<string>>> factory,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(timeout);
                var work = factory();
                var completed = await Task.WhenAny(work, Task.Delay(timeout, cts.Token)).ConfigureAwait(false);
                if (completed != work) return Array.Empty<string>();
                return await work.ConfigureAwait(false) ?? (IList<string>)Array.Empty<string>();
            }
        }

        private static string BuildCacheKey(Config cfg, string text)
        {
            if (cfg.UseAiTranslate)
                return $"ai|{cfg.AiProvider}|{cfg.AiModel}|{cfg.AiPrompt?.GetHashCode()}|{text}";
            return $"web|{cfg.TranslateType}|{text}";
        }

        private static void Remember(string key, string value)
        {
            if (Cache.Count >= CacheLimit)
                Cache.Clear();
            Cache[key] = value;
        }
    }

    /// <summary>
    /// 同一 QuickInfo 会话只允许一个缓冲区产出翻译，避免投影/多 ContentType 重复弹层。
    /// </summary>
    internal static class QuickInfoSessionGuard
    {
        private static readonly object Sync = new object();
        private static readonly ConditionalWeakTable<Microsoft.VisualStudio.Language.Intellisense.IAsyncQuickInfoSession, object> Claimed
            = new ConditionalWeakTable<Microsoft.VisualStudio.Language.Intellisense.IAsyncQuickInfoSession, object>();

        public static bool TryClaim(Microsoft.VisualStudio.Language.Intellisense.IAsyncQuickInfoSession session)
        {
            if (session == null) return false;
            try
            {
                lock (Sync)
                {
                    if (Claimed.TryGetValue(session, out _)) return false;
                    Claimed.Add(session, session);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 快捷键触发翻译时短暂放行，即使关闭了悬停翻译。
    /// </summary>
    internal static class TranslateTrigger
    {
        private static int _armedUntilTick;

        public static void Arm(int milliseconds = 3000)
        {
            _armedUntilTick = Environment.TickCount + milliseconds;
        }

        public static bool IsArmed
        {
            get { return unchecked(Environment.TickCount - _armedUntilTick) < 0; }
        }
    }
}
