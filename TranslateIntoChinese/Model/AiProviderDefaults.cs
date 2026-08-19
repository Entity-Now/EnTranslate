using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Model
{
    public static class AiProviderDefaults
    {
        public const string DefaultPrompt =
            "你是面向程序员的翻译助手。将用户给出的英文（单词、短语、代码标识符、XML/文档注释或选中的代码文本）翻译成简洁准确的简体中文。\n" +
            "要求：\n" +
            "1. 只输出译文，不要解释、不要前后缀、不要引号。\n" +
            "2. 保留类型名、方法名、参数名等代码标识符；必要时在标识符后用括号补中文释义。\n" +
            "3. 保持原文换行与列表结构。\n" +
            "4. 编程术语使用国内开发者常用译法。";

        public static string GetBaseUrl(AiProviderType provider)
        {
            switch (provider)
            {
                case AiProviderType.LmStudio:
                    return "http://localhost:1234/v1";
                case AiProviderType.Ollama:
                    return "http://localhost:11434";
                case AiProviderType.Anthropic:
                    return "https://api.anthropic.com";
                default:
                    return "https://api.openai.com/v1";
            }
        }

        public static string GetModel(AiProviderType provider)
        {
            switch (provider)
            {
                case AiProviderType.LmStudio:
                    return "";
                case AiProviderType.Ollama:
                    return "qwen2.5";
                case AiProviderType.Anthropic:
                    return "claude-3-5-sonnet-latest";
                default:
                    return "gpt-4o-mini";
            }
        }

        public static string GetDisplayName(AiProviderType provider)
        {
            switch (provider)
            {
                case AiProviderType.LmStudio:
                    return "LM Studio";
                case AiProviderType.Ollama:
                    return "Ollama";
                case AiProviderType.Anthropic:
                    return "Anthropic-compatible";
                default:
                    return "OpenAI-Compatible";
            }
        }

        public static bool IsKnownBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;
            var normalized = url.Trim().TrimEnd('/');
            return normalized == "https://api.openai.com/v1"
                || normalized == "http://localhost:1234/v1"
                || normalized == "http://localhost:1234"
                || normalized == "http://localhost:11434"
                || normalized == "https://api.anthropic.com"
                || normalized == "https://api.anthropic.com/v1"
                || normalized == "https://api.x.ai/v1";
        }

        public static bool IsKnownModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return true;
            switch (model.Trim())
            {
                case "gpt-4o-mini":
                case "qwen2.5":
                case "llama3.2":
                case "claude-3-5-sonnet-latest":
                    return true;
                default:
                    return false;
            }
        }
    }
}
