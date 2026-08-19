using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using MoqDictionary.Model;
using MoqDictionary.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    public class QuickInfoUIBuilder
    {
        public async Task<List<ContainerElement>> BuildTranslationElementsAsync(string text, CancellationToken cancellationToken)
        {
            var elements = new List<ContainerElement>();
            if (string.IsNullOrWhiteSpace(text)) return elements;

            if (EditorTextPicker.LooksLikeSentence(text))
            {
                if (TranslationCoordinator.CanTranslateRemotely)
                {
                    var remote = await TranslationCoordinator.TranslateOneAsync(text, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(remote))
                        elements.Add(CreateSimpleElement("[选区翻译]", remote));
                    else
                        elements.Add(CreateSimpleElement("[选区翻译]", "未返回译文。请检查网络、远程引擎或 AI 的 Base URL / 模型 / Key。"));
                }
                else
                {
                    elements.Add(CreateSimpleElement("[选区翻译]", "长句/选区翻译需要开启「远程翻译」或「AI 大模型」。"));
                }
                return elements;
            }

            var words = ParseString.getWordArray(text);
            foreach (var word in words ?? Array.Empty<string>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var local = QueryDir.getDir(word);
                if (local != null)
                {
                    elements.Add(CreateDictionaryElement(local));
                    continue;
                }

                if (!TranslationCoordinator.CanTranslateRemotely) continue;

                var remote = await TranslationCoordinator.TranslateOneAsync(word, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(remote))
                    elements.Add(CreateDictionaryElement(new Dictionarys { key = word, t = remote }));
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
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupNode, title)),
                new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.NaturalLanguage, content.Replace(@"\n", "\n"))));
        }
    }
}
