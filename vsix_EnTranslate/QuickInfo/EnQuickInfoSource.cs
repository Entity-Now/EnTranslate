using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using EnTranslate.utility;

namespace vsix_EnTranslate.QuickInfo
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
            // 将触发点映射到我们的缓冲区
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                applicableToSpan = null;
                return;
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            // 在范围内查找我们的 QuickInfo 单词的出现
            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            string searchText = extent.Span.GetText();

            var words = ParseString.getWordArray(searchText);
            foreach (var item in words)
            {
                session.QuickInfoContent.Add(item);
                //qiContent.Add(item);
            }
            // 获取光标下的名称
            //string cursorName = GetCursorName(searchText);
            //if (string.IsNullOrEmpty(cursorName))
            //{
            //    var words = ParseString.getWordArray(cursorName);
            //    foreach (var item in words)
            //    {
            //        qiContent.Add(item);
            //    }
            //    applicableToSpan = null;
            //    return;
            //}

            // 对光标名称进行进一步处理
            // ...

            applicableToSpan = null;
        }

        private string GetCursorName(string searchText)
        {
            // 从 searchText 中提取名称的逻辑
            // 根据你的具体需求和语言规范进行修改
            // 这里假设名称是不包含特殊字符的单词，并使用空格、制表符、换行符以及一些常见的特殊字符来分隔单词
            string[] words = searchText.Split(new[] { ' ', '\t', '\n', '\r', '(', ')', '[', ']', '{', '}', '<', '>', '+', '-', '*', '/', '=', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                return words[words.Length - 1];
            }

            return string.Empty;
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
