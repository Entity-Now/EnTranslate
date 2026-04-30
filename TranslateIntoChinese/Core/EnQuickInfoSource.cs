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
using MoqDictionary.utility;
using System.Windows;
using MoqDictionary.Model;
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

        public void Dispose() { }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            try
            {
                // 检查是否已经有我们的插件处理过这个 session 了
                if (session.Properties.ContainsProperty("MyTranslatePluginProcessed"))
                {
                    return null;
                }

                // 标记为已处理
                session.Properties.AddProperty("MyTranslatePluginProcessed", true);

                SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
                if (!subjectTriggerPoint.HasValue) return default;

                ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
                ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
                TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
                string searchText = extent.Span.GetText().Trim();

                // 排除纯中文
                if (Regex.IsMatch(searchText, @"[\u4e00-\u9fff]")) return default;

                var applicableToSpan = currentSnapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
                List<ContainerElement> wordElements = new List<ContainerElement>();
                HashSet<string> processedTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // --- 1. 处理 Legacy 提示内容 (如代码注释) ---
                if (Constants.Config.IsRemoteTranslate)
                {
                    var legacyElements = await GetLegacyQuickInfoSessionAsync(session);
                    if (legacyElements != null && legacyElements.Count > 0)
                    {
                        wordElements.AddRange(legacyElements);
                    }
                }

                // --- 2. 翻译当前选中的单词 ---
                // 即使上面有内容，我们也执行这一步，但会检查 processedTexts 以防完全重复
                if (!string.IsNullOrWhiteSpace(searchText) && !processedTexts.Contains(searchText))
                {
                    var words = ParseString.getWordArray(searchText);
                    if (words != null && words.Any())
                    {
                        foreach (var item in words)
                        {
                            var translateVal = QueryDir.getDir(item);
                            if (translateVal != null)
                            {
                                wordElements.Add(CreateElement(translateVal));
                            }
                            else if (!string.IsNullOrWhiteSpace(item.Trim()) && Constants.Config.IsRemoteTranslate)
                            {
                                // 调用远程翻译
                                var remoteResults = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, new List<string> { item });
                                if (remoteResults != null && remoteResults.Count > 0)
                                {
                                    wordElements.Add(CreateElement(new Dictionarys { key = item, t = remoteResults[0] }));
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(item.Trim()))
                            {
                                wordElements.Add(CreateNoFoundElement(item));
                            }
                        }
                    }
                }

                if (wordElements.Count == 0) return default;

                // 使用 Stacked 样式组合所有翻译结果
                var finalContainer = new ContainerElement(ContainerElementStyle.Stacked, wordElements);
                return new QuickInfoItem(applicableToSpan, finalContainer);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return default;
            }
        }

        private async Task<List<ContainerElement>> GetLegacyQuickInfoSessionAsync(IAsyncQuickInfoSession session)
        {
            // 查找现有的 QuickInfoSession 实例
            var legacySession = session.Properties.PropertyList
                .Select(it => it.Value)
                .OfType<IQuickInfoSession>()
                .FirstOrDefault();

            if (legacySession == null || legacySession.QuickInfoContent == null)
                return new List<ContainerElement>();

            var containerList = new List<ContainerElement>();

            // 提取所有文本内容
            foreach (var content in legacySession.QuickInfoContent.OfType<ContainerElement>())
            {
                var stringsToTranslate = await HandleContainerElementAsync(content);
                // 过滤掉空字符串和纯数字
                var filteredStrings = stringsToTranslate.Where(s => !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s, "[a-zA-Z]")).Distinct().ToList();

                if (filteredStrings.Count > 0)
                {
                    var translations = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, filteredStrings);
                    if (translations != null && translations.Count > 0)
                    {
                        var textRuns = translations.Select(t => new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.String, $"[译] {t}")));
                        containerList.Add(new ContainerElement(ContainerElementStyle.Stacked, textRuns));
                    }
                }
            }
            return containerList;
        }

        private async Task<List<string>> HandleContainerElementAsync(ContainerElement container)
        {
            var result = new List<string>();
            foreach (var item in container.Elements)
            {
                if (item is ContainerElement sub)
                {
                    result.AddRange(await HandleContainerElementAsync(sub));
                }
                else if (item is ClassifiedTextElement text)
                {
                    var combinedText = string.Join("", text.Runs.Select(r => r.Text));
                    if (!string.IsNullOrWhiteSpace(combinedText)) result.Add(combinedText);
                }
            }
            return result;
        }

        private ContainerElement CreateElement(Dictionarys val)
        {
            var wordHeader = new ContainerElement(
                ContainerElementStyle.Wrapped,
                new ImageElement(KnownMonikers.Play.ToImageId()),
                ClassifiedTextElement.CreateHyperlink("播放", "发音", () => PlayVoice(val.key)),
                new ClassifiedTextElement(
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupAttribute, $" {val.key} ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $" [{val.p}]")
                )
            );

            return new ContainerElement(
                ContainerElementStyle.Stacked,
                wordHeader,
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage, val.t?.Replace(@"\n", "\n") ?? ""))
            );
        }

        private void PlayVoice(string text)
        {
            var sound = Constants.Config.Sound;
            var soundName = Constants.Config.SoundName;
            if (sound == Model.Enums.SoundType.Edge)
            {
                string path = Path.Combine(Constants.AudioPath, $"{text}.mp3");
                if (File.Exists(path)) { Audio.PlayAudioAsync(path); return; }
                var voice = Edge_tts.GetVoice().FirstOrDefault(i => string.IsNullOrEmpty(soundName) ? i.Name.Contains("zh-CN") : i.Name == soundName);
                Edge_tts.PlayText(new PlayOption { Text = text, SavePath = path }, voice);
            }
            else if (sound == Model.Enums.SoundType.YouDao)
            {
                Audio.PlayAudioFromUrlAsync($"https://dict.youdao.com/dictvoice?audio={HttpUtility.UrlEncode(text)}&type={soundName}&le=cn");
            }
            else { SystemHelper.PlayVoice(text); }
        }

        private ContainerElement CreateNoFoundElement(string word)
        {
            return new ContainerElement(
                ContainerElementStyle.Wrapped,
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupAttributeValue, $"{word} (本地未命中): ")),
                ClassifiedTextElement.CreateHyperlink("百度", "跳转", () => SystemHelper.JumpBrowser($"https://fanyi.baidu.com/#en/zh/{word}")),
                ClassifiedTextElement.CreateHyperlink("谷歌", "跳转", () => SystemHelper.JumpBrowser($"https://translate.google.com/?sl=en&tl=zh-CN&text={word}"))
            );
        }
    }
}