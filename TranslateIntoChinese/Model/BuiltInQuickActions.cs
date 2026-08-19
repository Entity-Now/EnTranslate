using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslateIntoChinese.Model
{
    public static class BuiltInQuickActions
    {
        public const string TranslateReplace = "builtin.translate_replace";
        public const string OptimizeName = "builtin.optimize_name";
        public const string ZhToEnName = "builtin.zh_to_en_name";
        public const string EnToZhComment = "builtin.en_to_zh_comment";

        public static List<QuickActionItem> CreateDefaults()
        {
            return new List<QuickActionItem>
            {
                new QuickActionItem
                {
                    Id = TranslateReplace,
                    Title = "翻译并替换",
                    IsBuiltIn = true,
                    Enabled = true,
                    Prompt =
                        "将选中文本翻译成简洁准确的简体中文，结果将直接替换原文。\n" +
                        "要求：\n" +
                        "1. 只输出可替换原文的译文，不要解释、不要引号、不要 markdown。\n" +
                        "2. 若原文是代码，保留语法、符号、换行和标识符；只翻译注释与自然语言。\n" +
                        "3. 若原文是纯单词/短语（非代码），输出对应中文。\n" +
                        "4. 保持原有空白与换行结构。"
                },
                new QuickActionItem
                {
                    Id = OptimizeName,
                    Title = "变量名优化",
                    IsBuiltIn = true,
                    Enabled = true,
                    IdentifierOutput = true,
                    Prompt =
                        "你是资深程序员。优化选中的变量名/方法名/类型名，使其更清晰、符合常见英文命名习惯。\n" +
                        "要求：\n" +
                        "1. 只输出优化后的一个标识符，不要解释、不要引号、不要代码块。\n" +
                        "2. 保持原命名风格（camelCase / PascalCase / snake_case / 前导下划线）。\n" +
                        "3. 不要匈牙利命名或无意义前缀。\n" +
                        "4. 若已经足够清晰，可做轻微修正或保持原样。"
                },
                new QuickActionItem
                {
                    Id = ZhToEnName,
                    Title = "中文→英文变量名",
                    IsBuiltIn = true,
                    Enabled = true,
                    IdentifierOutput = true,
                    Prompt =
                        "把选中的中文变量名、方法名或中文短语转成地道的英文程序标识符。\n" +
                        "要求：\n" +
                        "1. 只输出一个标识符，不要解释、不要引号、不要拼音。\n" +
                        "2. 默认 camelCase；若原文像类型名/方法名（或本身是 PascalCase），使用 PascalCase。\n" +
                        "3. 只能包含字母、数字、下划线，不能有空格。\n" +
                        "4. 使用国内开发者熟悉的编程英语。"
                },
                new QuickActionItem
                {
                    Id = EnToZhComment,
                    Title = "生成中文注释",
                    IsBuiltIn = true,
                    Enabled = true,
                    Prompt =
                        "为选中的代码或英文注释生成一句简洁的简体中文注释，结果将直接替换选区。\n" +
                        "要求：\n" +
                        "1. 只输出注释文本本身（可带 // 或保留原注释符号风格），不要解释、不要 markdown。\n" +
                        "2. 一到两句说清意图即可，不要复述每一行代码。\n" +
                        "3. 若原文已是注释，将其翻译/润色成中文注释。"
                }
            };
        }

        public static List<QuickActionItem> Merge(IList<QuickActionItem> stored)
        {
            var defaults = CreateDefaults();
            var result = new List<QuickActionItem>();
            var storedList = stored == null
                ? new List<QuickActionItem>()
                : stored.Where(x => x != null).ToList();

            foreach (var builtIn in defaults)
            {
                var existing = storedList.FirstOrDefault(x =>
                    string.Equals(x.Id, builtIn.Id, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    result.Add(builtIn);
                    continue;
                }

                existing.IsBuiltIn = true;
                existing.IdentifierOutput = builtIn.IdentifierOutput;
                if (string.IsNullOrWhiteSpace(existing.Title))
                    existing.Title = builtIn.Title;
                if (string.IsNullOrWhiteSpace(existing.Prompt))
                    existing.Prompt = builtIn.Prompt;
                result.Add(existing);
            }

            foreach (var custom in storedList)
            {
                if (custom.IsBuiltIn) continue;
                if (defaults.Any(d => string.Equals(d.Id, custom.Id, StringComparison.OrdinalIgnoreCase)))
                    continue;
                if (string.IsNullOrWhiteSpace(custom.Title)) continue;
                if (string.IsNullOrWhiteSpace(custom.Id))
                    custom.Id = "custom." + Guid.NewGuid().ToString("N");
                custom.IsBuiltIn = false;
                result.Add(custom);
            }

            return result;
        }

        public static void ResetPrompt(QuickActionItem item)
        {
            if (item == null) return;
            var builtIn = CreateDefaults().FirstOrDefault(x =>
                string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase));
            if (builtIn == null) return;
            item.Title = builtIn.Title;
            item.Prompt = builtIn.Prompt;
            item.IdentifierOutput = builtIn.IdentifierOutput;
        }
    }
}
