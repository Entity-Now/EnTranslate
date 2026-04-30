using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using MoqDictionary.Model;
using MoqDictionary.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    public class QuickInfoUIBuilder
    {
        public async Task<List<ContainerElement>> BuildTranslationElementsAsync(string text)
        {
            var elements = new List<ContainerElement>();

            // 如果包含空格或长度较长，视为长句/选区翻译
            if (text.Contains(" ") || text.Length > 25)
            {
                if (Constants.Config.IsRemoteTranslate)
                {
                    var remote = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, new List<string> { text });
                    if (remote?.Any() == true)
                        elements.Add(CreateSimpleElement(text, remote[0]));
                }
            }
            else
            {
                // 单词分割逻辑
                var words = ParseString.getWordArray(text);
                foreach (var word in words ?? new string[0])
                {
                    var local = QueryDir.getDir(word);
                    if (local != null)
                    {
                        elements.Add(CreateDictionaryElement(local));
                    }
                    else if (Constants.Config.IsRemoteTranslate)
                    {
                        var remote = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, new List<string> { word });
                        if (remote?.Any() == true)
                            elements.Add(CreateDictionaryElement(new Dictionarys { key = word, t = remote[0] }));
                    }
                }
            }
            return elements;
        }

        private ContainerElement CreateDictionaryElement(Dictionarys val)
        {
            var header = new ContainerElement(
                ContainerElementStyle.Wrapped,
                new ImageElement(KnownMonikers.Play.ToImageId()),
                ClassifiedTextElement.CreateHyperlink("播放", "发音", () => VoiceService.Play(val.key)),
                new ClassifiedTextElement(
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupAttribute, $" {val.key} ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $" [{val.p}]")
                )
            );

            return new ContainerElement(ContainerElementStyle.Stacked, header,
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage, val.t?.Replace(@"\n", "\n") ?? "")));
        }

        private ContainerElement CreateSimpleElement(string title, string content)
        {
            return new ContainerElement(ContainerElementStyle.Stacked,
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupNode, $"[选区翻译]")),
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage, content.Replace(@"\n", "\n"))));
        }
    }
}
