using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    internal class EnQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly EnQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _textBuffer;
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
            try
            {
                if (session == null) return default;

                var cfg = Constants.Config;
                if (cfg == null) return default;
                if (!cfg.EnableHoverTranslate && !TranslateTrigger.IsArmed) return default;

                // 先取出有效文本再抢占会话，避免空投影缓冲把会话标成已处理
                var target = _extractor.GetTargetText(session);
                if (target == null) return default;

                if (!QuickInfoSessionGuard.TryClaim(session)) return default;

                cancellationToken.ThrowIfCancellationRequested();

                var wordElements = new List<ContainerElement>();

                if (TranslationCoordinator.CanTranslateDocuments)
                {
                    var legacyElements = await _legacyService.GetLegacyTranslationsAsync(session, cancellationToken).ConfigureAwait(false);
                    if (legacyElements != null && legacyElements.Count > 0)
                        wordElements.AddRange(legacyElements);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var translationElements = await _uiBuilder.BuildTranslationElementsAsync(target.Text, cancellationToken).ConfigureAwait(false);
                if (translationElements != null && translationElements.Count > 0)
                    wordElements.AddRange(translationElements);

                if (wordElements.Count == 0) return default;

                return new QuickInfoItem(target.ApplicableSpan, new ContainerElement(ContainerElementStyle.Stacked, wordElements));
            }
            catch (OperationCanceledException)
            {
                return default;
            }
            catch (Exception ex)
            {
                try
                {
                    await ex.LogAsync();
                }
                catch
                {
                    // 忽略日志记录本身的异常，防止二次异常导致 VS 崩溃
                }
                return default;
            }
        }
    }
}
