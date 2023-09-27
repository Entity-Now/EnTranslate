using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.Threading;
using TranlateIntoChinese.View;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Threading;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Language.StandardClassification;
using EnTranslate.utility;
using System.Windows;
using EnTranslate.Model;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging;
using System.Windows.Controls;
using TranlateIntoChinese.Utility;

namespace TranlateIntoChinese.Core
{
    internal class EnQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly EnQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _textBuffer;
        public EnQuickInfoSource(EnQuickInfoSourceProvider provider, ITextBuffer textBuffer) 
        {
            _provider = provider;
            _textBuffer = textBuffer;
        }
        public void Dispose()
        {

        }
        private ContainerElement createElement(Dictionarys val)
        {
            var Word = new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupAttribute, $"单词：{val.key}", ClassifiedTextRunStyle.Bold);
            var Icon = new ImageElement(KnownMonikers.Play.ToImageId());
            var split = new ClassifiedTextRun(PredefinedClassificationTypeNames.WhiteSpace, "     ");
            var Dimension = new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $"音标：{val.p}");
            return new ContainerElement(
                ContainerElementStyle.Stacked,
                new ContainerElement(ContainerElementStyle.Wrapped,
                    Icon,
                    ClassifiedTextElement.CreateHyperlink("播放", "播放英语发音", () =>
                    {
                        SystemHelper.PlayVoice(val.key);
                    }),
                    new ClassifiedTextElement(Word, split, Dimension)
                ),
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage, val.t.Replace(@"\n","\n")))
            );
        }
        private ContainerElement createNofoundElement(string Word)
        {
            var baiduLink = ClassifiedTextElement.CreateHyperlink(" 百度翻译 ", "跳转到百度翻译", () =>
            {
                SystemHelper.JumpBrowser($"https://fanyi.baidu.com/#en/zh/{Word}");
            });
            var googleLink = ClassifiedTextElement.CreateHyperlink(" 谷歌翻译 ", "跳转到google翻译", () =>
            {
                SystemHelper.JumpBrowser($"https://fanyi.baidu.com/#en/zh/{Word}");
            });
            return new ContainerElement(
                ContainerElementStyle.Wrapped,
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupAttributeValue, $"{Word},本地词库暂无结果，查看：")),
                baiduLink,
                googleLink
                ); ;
        }
        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            ITrackingSpan applicableToSpan = null;
            try
            {
                // 将触发点映射到我们的缓冲区
                SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
                if (!subjectTriggerPoint.HasValue)
                {
                    applicableToSpan = null;
                    return null;
                }
                ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
                SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
                applicableToSpan = currentSnapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);
                // 在范围内查找我们的 QuickInfo 单词的出现
                ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
                TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
                string searchText = extent.Span.GetText().Trim();

                // 判断字符串是否包含中文
                bool containsChinese = Regex.IsMatch(searchText, @"[\u4e00-\u9fff]");
                if (containsChinese)
                {
                    return null;
                }
                List<ContainerElement> wordElement = new List<ContainerElement>();
                // 分割单词
                var words = ParseString.getWordArray(searchText);
                if (words != null && words.Count() > 0)
                {
                    foreach (var item in words)
                    {
                        var TranslateVal = QueryDir.getDir(item);
                        if (TranslateVal != null)
                        {
                            wordElement.Add(createElement(TranslateVal));
                        }
                        else if(!string.IsNullOrWhiteSpace(item.Trim()))
                        {
                            wordElement.Add(createNofoundElement(item));
                        }
                    }
                }
                var translateContainer = new ContainerElement(ContainerElementStyle.Stacked ,wordElement);

                var result = new QuickInfoItem(applicableToSpan, translateContainer);
                return result;
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return null;
            }
        }
    }
}
