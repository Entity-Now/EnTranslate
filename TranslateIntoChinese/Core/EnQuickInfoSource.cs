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
using TranslateIntoChinese.View;
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
using TranslateIntoChinese.Utility;
using TranslateIntoChinese.Model;
using Edge_tts_sharp;
using System.IO;
using Edge_tts_sharp.Utils;
using Edge_tts_sharp.Model;
using System.Web;
using System.Web.UI.WebControls;

namespace TranslateIntoChinese.Core
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
                        var sound = Constants.Config.Sound;
                        var soundName = Constants.Config.SoundName;
                        if (sound == Model.Enums.SoundType.Edge)
                        {
                            string audioPath = Path.Combine(Constants.AudioPath, $"{val.key}.mp3");
                            if (File.Exists(audioPath))
                            {
                                Audio.PlayAudioAsync(audioPath);
                                return;
                            }
                            var voice = Edge_tts.GetVoice().FirstOrDefault(i => string.IsNullOrEmpty(soundName) ? i.Name.Contains("zh-CN") : i.Name == soundName);
                            Edge_tts.PlayText(new PlayOption { 
                                Text = val.key,
                                SavePath = audioPath,
                            }, voice);
                        }
                        else if(Constants.Config.Sound == Model.Enums.SoundType.YouDao)
                        {
                            Audio.PlayAudioFromUrlAsync
                            (
                                $"https://dict.youdao.com/dictvoice?audio={HttpUtility.UrlEncode(val.key)}&type={soundName}&le=cn"
                            );
                        }
                        else
                        {
                            SystemHelper.PlayVoice(val.key);
                        }
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
                    return default;
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
                    return default;
                }
                List<ContainerElement> wordElement = new List<ContainerElement>();
                // 翻译注释
                if (Constants.Config.IsRemoteTranslate)
                {
                    var remote = await getLegacyQuickInfoSessionAsync(session);
                    wordElement.AddRange(remote);
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
                            wordElement.Add(createElement(TranslateVal));
                        }
                        else if(!string.IsNullOrWhiteSpace(item.Trim()))
                        {
                            if (Constants.Config.IsRemoteTranslate)
                            {
                                Dictionarys _tran = new Dictionarys
                                {
                                    key = item,
                                    t = (await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, new List<string> { item }))?[0]
                                };
                                wordElement.Add(createElement(_tran));
                                continue;
                            }
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
                return default;
            }
        }

        async Task<List<ContainerElement>> getLegacyQuickInfoSessionAsync(IAsyncQuickInfoSession session)
        {
            var res = session.Properties.PropertyList[0].Value as IQuickInfoSession;

            // 使用 LINQ 选择并处理 ContainerElement 类型的元素
            var tasks = res.QuickInfoContent
                .OfType<ContainerElement>()
                .Select(async item => await HandleContainerElementAsync(item))
                .ToList();

            // 等待所有任务完成
            var results = await Task.WhenAll(tasks);

            // 使用 LINQ 处理结果并生成新的 ContainerElement 列表
            var containerElements = results
                .Where(result => result.Count > 1)
                .Select(async result =>
                {
                    var tran = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, result);
                    return new ContainerElement(
                        ContainerElementStyle.Stacked,
                        tran.Select(tran => new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.String, tran)))
                            .ToList()
                    );
                })
                .ToList();

            // 等待所有内部任务完成
            return (await Task.WhenAll(containerElements)).ToList();
        }
        async Task<List<string>> HandleContainerElementAsync(ContainerElement container)
        {
            var result = new List<string>();
            if (container.Elements.Count() <= 0)
            {
                return null;
            }
            foreach (var item in container.Elements)
            {
                if (item.GetType() == typeof(ContainerElement))
                {
                    var sub_res = await HandleContainerElementAsync((ContainerElement)item);
                    result.AddRange(sub_res);
                }
                else if (item.GetType() == typeof(ClassifiedTextElement))
                {
                    result.Add(HandleClassifiedTextElement((ClassifiedTextElement)item));
                }
            }

            return result;
        }
        string HandleClassifiedTextElement(ClassifiedTextElement element)
        {
            StringBuilder temp_res = new StringBuilder();
            foreach (var sub_item in element.Runs)
            {
                temp_res.Append(sub_item.Text);
            }

            return temp_res.ToString();
        }
    }
}
