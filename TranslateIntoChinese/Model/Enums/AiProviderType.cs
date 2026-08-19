namespace TranslateIntoChinese.Model.Enums
{
    /// <summary>
    /// 大模型接口协议。LM Studio / 多数本地与云端网关走 OpenAI 兼容协议。
    /// </summary>
    public enum AiProviderType
    {
        OpenAICompatible = 0,
        LmStudio = 1,
        Ollama = 2,
        Anthropic = 3
    }
}
