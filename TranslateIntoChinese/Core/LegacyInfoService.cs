using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    public class LegacyInfoService
    {
        private const int MaxFragments = 2;
        private const int MaxCharsPerFragment = 2000;
        private static readonly Regex XmlTag = new Regex(@"</?\w+[^>]*>", RegexOptions.Compiled);
        private static readonly Regex SignaturePrefix = new Regex(
            @"^\s*(public|private|protected|internal|static|virtual|override|async|extern|sealed|partial|class|struct|interface|enum|namespace|using|void)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<List<ContainerElement>> GetLegacyTranslationsAsync(
            IAsyncQuickInfoSession session,
            CancellationToken cancellationToken)
        {
            var containerList = new List<ContainerElement>();
            if (!TranslationCoordinator.CanTranslateDocuments || session == null)
                return containerList;

            try
            {
                if (session == null) return containerList;

                var fragments = await ExtractDocumentFragmentsAsync(session, cancellationToken).ConfigureAwait(false);
                if (fragments.Count == 0) return containerList;

                var translations = await TranslationCoordinator.TranslateManyAsync(fragments, cancellationToken).ConfigureAwait(false);
                if (translations == null || translations.Count == 0) return containerList;

                var textRuns = translations
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.String, $"[文档翻译] {t}")));

                var runs = textRuns.ToList();
                if (runs.Count > 0)
                    containerList.Add(new ContainerElement(ContainerElementStyle.Stacked, runs));
            }
            catch (OperationCanceledException)
            {
                return containerList;
            }
            catch (COMException ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLegacyTranslationsAsync COM: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLegacyTranslationsAsync failed: {ex.Message}");
            }

            return containerList;
        }

        private async Task<List<string>> ExtractDocumentFragmentsAsync(
            IAsyncQuickInfoSession session,
            CancellationToken cancellationToken)
        {
            List<string> collected = null;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            for (int attempt = 0; attempt < 4; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (session == null) return new List<string>();

                collected = TryCollect(session);
                if (collected.Count > 0) break;
                await Task.Delay(40, cancellationToken).ConfigureAwait(true);
            }

            return FilterFragments(collected ?? new List<string>());
        }

        private List<string> TryCollect(IAsyncQuickInfoSession session)
        {
            var result = new List<string>();
            try
            {
                if (session?.Properties?.PropertyList == null) return result;

                var legacySession = session.Properties.PropertyList
                    .Select(it => it.Value)
                    .OfType<IQuickInfoSession>()
                    .FirstOrDefault();

                if (legacySession?.QuickInfoContent == null) return result;

                foreach (var content in legacySession.QuickInfoContent)
                {
                    if (content is ContainerElement container)
                        CollectFromContainer(container, result);
                }
            }
            catch (COMException)
            {
                return new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryCollect failed: {ex.Message}");
            }
            return result;
        }

        private void CollectFromContainer(ContainerElement container, List<string> result)
        {
            if (container?.Elements == null) return;

            try
            {
                foreach (var item in container.Elements)
                {
                    if (item is ContainerElement sub)
                    {
                        CollectFromContainer(sub, result);
                    }
                    else if (item is ClassifiedTextElement text && text.Runs != null)
                    {
                        var combined = string.Join("", text.Runs.Where(r => r?.Text != null).Select(r => r.Text));
                        if (!string.IsNullOrWhiteSpace(combined))
                            result.Add(combined);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CollectFromContainer failed: {ex.Message}");
            }
        }

        internal static List<string> FilterFragments(IEnumerable<string> raw)
        {
            var scored = new List<Tuple<int, string>>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in raw ?? Enumerable.Empty<string>())
            {
                var cleaned = Clean(item);
                if (!IsDocumentComment(cleaned)) continue;
                if (!seen.Add(cleaned)) continue;
                scored.Add(Tuple.Create(Score(cleaned), cleaned));
            }

            return scored
                .OrderByDescending(t => t.Item1)
                .ThenByDescending(t => t.Item2.Length)
                .Take(MaxFragments)
                .Select(t => t.Item2.Length > MaxCharsPerFragment
                    ? t.Item2.Substring(0, MaxCharsPerFragment)
                    : t.Item2)
                .ToList();
        }

        private static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var value = XmlTag.Replace(text, " ");
            value = Regex.Replace(value, @"\s+", " ").Trim();
            return value;
        }

        private static bool IsDocumentComment(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 16) return false;
            if (text.StartsWith("[译]", StringComparison.Ordinal) ||
                text.StartsWith("[文档翻译]", StringComparison.Ordinal) ||
                text.StartsWith("[选区翻译]", StringComparison.Ordinal))
                return false;
            if (!EditorTextPicker.IsValidForTranslation(text)) return false;
            if (LooksLikeSignature(text)) return false;

            int spaces = 0;
            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c)) spaces++;
            }
            return spaces >= 2;
        }

        private static bool LooksLikeSignature(string text)
        {
            if (SignaturePrefix.IsMatch(text)) return true;
            if (text.Length < 80 && text.IndexOf('(') >= 0 && text.IndexOf(')') >= 0 && text.Count(char.IsWhiteSpace) < 3)
                return true;
            if (text.IndexOf("->", StringComparison.Ordinal) >= 0 && text.Length < 60) return true;
            return false;
        }

        private static int Score(string text)
        {
            int score = text.Length;
            if (text.IndexOf('.') >= 0) score += 20;
            if (text.StartsWith("Gets ", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("Sets ", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("Returns ", StringComparison.OrdinalIgnoreCase))
                score += 30;
            return score;
        }
    }
}
