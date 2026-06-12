using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TranslateIntoChinese.Utility;
using TranslateIntoChinese.Model;
using MoqDictionary.utility;

namespace TranslateIntoChinese.Core
{
    public class LegacyInfoService
    {
        public async Task<List<ContainerElement>> GetLegacyTranslationsAsync(IAsyncQuickInfoSession session)
        {
            try
            {
                if (session?.Properties?.PropertyList == null)
                    return new List<ContainerElement>();

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
                    var filteredStrings = stringsToTranslate
                        .Where(s => !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s, "[a-zA-Z]"))
                        .Distinct()
                        .ToList();

                    if (filteredStrings.Count > 0)
                    {
                        var translations = await TranslateHelper.getTranslateAsync(Constants.Config.TranslateType, filteredStrings);
                        if (translations != null && translations.Count > 0)
                        {
                            var textRuns = translations.Select(t => new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.String, $" [译] {t}")));

                            containerList.Add(new ContainerElement(ContainerElementStyle.Stacked, textRuns));
                        }
                    }
                }
                return containerList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLegacyTranslationsAsync failed: {ex.Message}");
                return new List<ContainerElement>();
            }
        }

        private async Task<List<string>> HandleContainerElementAsync(ContainerElement container)
        {
            var result = new List<string>();
            if (container?.Elements == null) return result;

            try
            {
                foreach (var item in container.Elements)
                {
                    if (item is ContainerElement sub)
                    {
                        result.AddRange(await HandleContainerElementAsync(sub));
                    }
                    else if (item is ClassifiedTextElement text)
                    {
                        if (text.Runs != null)
                        {
                            var combinedText = string.Join("", text.Runs.Where(r => r?.Text != null).Select(r => r.Text));
                            if (!string.IsNullOrWhiteSpace(combinedText)) result.Add(combinedText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HandleContainerElementAsync failed: {ex.Message}");
            }
            return result;
        }
    }
}