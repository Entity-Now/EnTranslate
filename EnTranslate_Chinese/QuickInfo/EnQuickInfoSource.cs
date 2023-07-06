using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnTranslate.utility;
using EnTranslate_Chinese.utility;

namespace EnTranslate_Chinese.QuickInfo
{
    internal class EnQuickInfoSource : IQuickInfoSource
    {
        private EnQuickInfoSourceProvider m_provider;
        private ITextBuffer m_subjectBuffer;

        public EnQuickInfoSource(EnQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            m_provider = provider;
            m_subjectBuffer = subjectBuffer;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            try
            {
                // 将触发点映射到我们的缓冲区
                SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
                if (!subjectTriggerPoint.HasValue)
                {
                    applicableToSpan = null;
                    return;
                }
                ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
                SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
                applicableToSpan = currentSnapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);

                // 在范围内查找我们的 QuickInfo 单词的出现
                ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
                TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
                string searchText = extent.Span.GetText();

                // 判断字符串是否包含中文
                bool containsChinese = Regex.IsMatch(searchText, @"[\u4e00-\u9fff]");
                if (string.IsNullOrEmpty(searchText) || searchText.Length < 2 || containsChinese)
                {
                    applicableToSpan = null;
                    return;
                }
                // 分割单词
                var words = ParseString.getWordArray(searchText);
                if (words != null && words.Count() > 0)
                {
                    foreach (var item in words)
                    {
                        var TranslateVal = QueryDir.getDir(item);
                        if (TranslateVal != null)
                        {
                            var Content = applicableToSpan.TranslateInfo(TranslateVal);

                            qiContent.Add(Content);
                        }
                    }
                    return;
                }

                applicableToSpan = null;
            }
            catch (Exception)
            {
                applicableToSpan = null;
            }
        }


        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
