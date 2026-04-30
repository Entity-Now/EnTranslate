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
        // 引入新模块
        private readonly QuickInfoTextExtractor _extractor;
        private readonly QuickInfoUIBuilder _uiBuilder;
        private readonly LegacyInfoService _legacyService;

        public EnQuickInfoSource(EnQuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            _provider = provider;
            _textBuffer = textBuffer;
            _extractor = new QuickInfoTextExtractor(provider, textBuffer);
            _uiBuilder = new QuickInfoUIBuilder();
            _legacyService = new LegacyInfoService();
        }

        public void Dispose() { }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            if (session.Properties.ContainsProperty("MyTranslatePluginProcessed")) return null;
            session.Properties.AddProperty("MyTranslatePluginProcessed", true);

            try
            {
                // 1. 获取目标文本和范围
                var target = _extractor.GetTargetText(session);
                if (target == null) return default;

                List<ContainerElement> wordElements = new List<ContainerElement>();

                // 2. 处理 Legacy 内容 (原生提示翻译)
                if (Constants.Config.IsRemoteTranslate)
                {
                    var legacyElements = await _legacyService.GetLegacyTranslationsAsync(session);
                    if (legacyElements.Any()) wordElements.AddRange(legacyElements);
                }

                // 3. 处理主文本翻译 (选区或单词)
                var translationElements = await _uiBuilder.BuildTranslationElementsAsync(target.Text);
                wordElements.AddRange(translationElements);

                if (!wordElements.Any()) return default;

                return new QuickInfoItem(target.ApplicableSpan, new ContainerElement(ContainerElementStyle.Stacked, wordElements));
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return default;
            }
        }
    }
}