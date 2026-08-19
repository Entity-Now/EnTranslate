using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Linq;

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

                // 优先处理选区：单词 / 一行 / 一段
                if (selection.Length > 0)
                {
                    string text = EditorTextPicker.Truncate(EditorTextPicker.Normalize(selection.GetText()));
                    if (EditorTextPicker.IsValidForTranslation(text))
                    {
                        return new ExtractionResult
                        {
                            Text = text,
                            ApplicableSpan = snapshot.CreateTrackingSpan(selection, SpanTrackingMode.EdgeInclusive)
                        };
                    }
                }

                if (_provider.NavigatorService == null) return null;
                ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
                if (navigator == null) return null;

                TextExtent extent = navigator.GetExtentOfWord(triggerPoint.Value);
                string wordText = EditorTextPicker.Normalize(extent.Span.GetText());

                if (EditorTextPicker.IsValidForTranslation(wordText))
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
    }

    public class ExtractionResult
    {
        public string Text { get; set; }
        public ITrackingSpan ApplicableSpan { get; set; }
    }
}
