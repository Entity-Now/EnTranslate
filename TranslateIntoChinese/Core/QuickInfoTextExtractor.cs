using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TranslateIntoChinese.Core
{
    public class QuickInfoTextExtractor
    {
        private readonly EnQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _textBuffer;

        public QuickInfoTextExtractor(EnQuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            _provider = provider;
            _textBuffer = textBuffer;
        }

        public ExtractionResult GetTargetText(IAsyncQuickInfoSession session)
        {
            try
            {
                if (session == null || session.TextView?.Selection == null) return null;

                SnapshotPoint? triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
                if (!triggerPoint.HasValue) return null;

                var snapshot = triggerPoint.Value.Snapshot;
                var selection = session.TextView.Selection.SelectedSpans.FirstOrDefault();

                // 优先处理选区
                if (selection.Length > 0)
                {
                    string text = selection.GetText().Trim();
                    if (IsValidForTranslation(text))
                    {
                        return new ExtractionResult
                        {
                            Text = text,
                            ApplicableSpan = snapshot.CreateTrackingSpan(selection, SpanTrackingMode.EdgeInclusive)
                        };
                    }
                }

                // 其次处理光标下的单词
                if (_provider.NavigatorService == null) return null;
                ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
                if (navigator == null) return null;

                TextExtent extent = navigator.GetExtentOfWord(triggerPoint.Value);
                string wordText = extent.Span.GetText()?.Trim();

                if (IsValidForTranslation(wordText))
                {
                    return new ExtractionResult
                    {
                        Text = wordText,
                        ApplicableSpan = snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTargetText failed: {ex.Message}");
            }

            return null;
        }

        private bool IsValidForTranslation(string text) =>
            !string.IsNullOrWhiteSpace(text) && !Regex.IsMatch(text, @"[\u4e00-\u9fff]");
    }

    public class ExtractionResult
    {
        public string Text { get; set; }
        public ITrackingSpan ApplicableSpan { get; set; }
    }
}
