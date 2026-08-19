using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Core
{
    /// <summary>
    /// 按配置调用 LM Studio / OpenAI-Compatible / Ollama / Anthropic-compatible。
    /// </summary>
    public static class AiTranslateService
    {
        private static readonly HttpClient SharedClient;
        private static readonly HttpClient LocalClient;

        static AiTranslateService()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch
            {
                // 部分环境不允许改协议，忽略
            }

            SharedClient = CreateClient(useProxy: true);
            LocalClient = CreateClient(useProxy: false);
        }

        private static HttpClient CreateClient(bool useProxy)
        {
            var handler = new HttpClientHandler
            {
                UseProxy = useProxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public static Task<string> TranslateOneAsync(string text, CancellationToken cancellationToken)
        {
            return TranslateOneAsync(text, Constants.Config, cancellationToken);
        }

        public static async Task<string> TranslateOneAsync(string text, Config config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text) || config == null) return null;

            var items = await TranslateManyAsync(new[] { text }, config, cancellationToken).ConfigureAwait(false);
            return items == null || items.Count == 0 ? null : items[0];
        }

        /// <summary>
        /// 设置页“测试连接”使用：失败时抛出具体原因，而不是吞掉异常。
        /// </summary>
        public static Task<string> TranslateOneStrictAsync(string text, Config config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("没有可翻译的文本。");
            if (config == null) throw new InvalidOperationException("配置未加载。");
            return RequestAsync(text, config, cancellationToken, null);
        }

        /// <summary>
        /// 用指定指令处理文本（划词快捷指令）。失败时抛出异常。
        /// </summary>
        public static async Task<string> CompleteAsync(
            string text,
            string instruction,
            Config config,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("没有可处理的文本。");
            if (config == null) throw new InvalidOperationException("配置未加载。");

            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(30));
                return await RequestAsync(text, config, timeout.Token, instruction).ConfigureAwait(false);
            }
        }

        public static async Task<IList<string>> TranslateManyAsync(
            IList<string> texts,
            Config config,
            CancellationToken cancellationToken)
        {
            if (texts == null || texts.Count == 0 || config == null)
                return Array.Empty<string>();

            var cleaned = texts
                .Select(t => (t ?? string.Empty).Trim())
                .Where(t => t.Length > 0)
                .ToList();
            if (cleaned.Count == 0) return Array.Empty<string>();

            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                try
                {
                    if (cleaned.Count == 1)
                    {
                        var one = await RequestAsync(cleaned[0], config, timeout.Token).ConfigureAwait(false);
                        return string.IsNullOrWhiteSpace(one) ? Array.Empty<string>() : new[] { one.Trim() };
                    }

                    var numbered = new StringBuilder();
                    numbered.AppendLine("请按相同行数和顺序翻译，每行一条译文，不要编号以外的说明：");
                    for (int i = 0; i < cleaned.Count; i++)
                    {
                        numbered.Append(i + 1);
                        numbered.Append(". ");
                        numbered.AppendLine(cleaned[i].Replace("\r\n", " ").Replace("\n", " "));
                    }

                    var raw = await RequestAsync(numbered.ToString(), config, timeout.Token).ConfigureAwait(false);
                    var parsed = ParseNumbered(raw, cleaned.Count);
                    if (parsed.Count == cleaned.Count) return parsed;

                    // 批量解析失败时逐条请求，避免整批丢失
                    var fallback = new List<string>(cleaned.Count);
                    foreach (var item in cleaned)
                    {
                        timeout.Token.ThrowIfCancellationRequested();
                        var one = await RequestAsync(item, config, timeout.Token).ConfigureAwait(false);
                        fallback.Add(string.IsNullOrWhiteSpace(one) ? string.Empty : one.Trim());
                    }
                    return fallback;
                }
                catch (OperationCanceledException)
                {
                    return Array.Empty<string>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AiTranslateService failed: {ex.Message}");
                    return Array.Empty<string>();
                }
            }
        }

        private static async Task<string> RequestAsync(
            string text,
            Config config,
            CancellationToken cancellationToken,
            string promptOverride = null)
        {
            var provider = config.AiProvider;
            var url = BuildChatUrl(provider, config.AiBaseUrl);
            var model = string.IsNullOrWhiteSpace(config.AiModel)
                ? AiProviderDefaults.GetModel(provider)
                : config.AiModel.Trim();
            var prompt = ResolvePrompt(
                string.IsNullOrWhiteSpace(promptOverride) ? config.AiPrompt : promptOverride,
                text);

            if (string.IsNullOrWhiteSpace(model) && provider != AiProviderType.LmStudio)
                throw new InvalidOperationException("请先在设置中填写模型名称。");

            string body;
            if (provider == AiProviderType.Ollama)
            {
                body = BuildOllamaBody(model, prompt.System, prompt.User);
            }
            else if (provider == AiProviderType.Anthropic)
            {
                body = BuildAnthropicBody(model, prompt.System, prompt.User);
            }
            else
            {
                body = BuildOpenAiBody(model, prompt.System, prompt.User);
            }

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                ApplyHeaders(request, provider, config.AiApiKey);

                var client = IsLoopback(url) ? LocalClient : SharedClient;
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
                    .ConfigureAwait(false))
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        var snippet = json != null && json.Length > 240 ? json.Substring(0, 240) : json;
                        throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {snippet}");
                    }
                    return ExtractContent(provider, json);
                }
            }
        }

        internal static string BuildChatUrl(AiProviderType provider, string baseUrl)
        {
            var url = (baseUrl ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(url))
                url = AiProviderDefaults.GetBaseUrl(provider);
            url = url.TrimEnd('/');

            if (provider == AiProviderType.Ollama)
            {
                if (url.EndsWith("/api/chat", StringComparison.OrdinalIgnoreCase)) return url;
                return url + "/api/chat";
            }

            if (provider == AiProviderType.Anthropic)
            {
                if (url.EndsWith("/messages", StringComparison.OrdinalIgnoreCase)) return url;
                if (url.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)) return url + "/messages";
                return url + "/v1/messages";
            }

            if (url.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)) return url;
            if (url.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)) return url + "/chat/completions";
            return url + "/v1/chat/completions";
        }

        private static void ApplyHeaders(HttpRequestMessage request, AiProviderType provider, string apiKey)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var key = (apiKey ?? string.Empty).Trim();

            if (provider == AiProviderType.Anthropic)
            {
                if (!string.IsNullOrEmpty(key))
                    request.Headers.TryAddWithoutValidation("x-api-key", key);
                request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
                return;
            }

            if (provider == AiProviderType.Ollama)
                return;

            if (string.IsNullOrEmpty(key))
                key = provider == AiProviderType.LmStudio ? "lm-studio" : "no-key";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        }

        private static string BuildOpenAiBody(string model, string system, string user)
        {
            var payload = new OpenAiRequest
            {
                model = string.IsNullOrWhiteSpace(model) ? "local-model" : model,
                temperature = 0.2,
                messages = new[]
                {
                    new ChatMessage { role = "system", content = system },
                    new ChatMessage { role = "user", content = user }
                }
            };
            return JsonSerializer.Serialize(payload);
        }

        private static string BuildOllamaBody(string model, string system, string user)
        {
            var payload = new OllamaRequest
            {
                model = model,
                stream = false,
                messages = new[]
                {
                    new ChatMessage { role = "system", content = system },
                    new ChatMessage { role = "user", content = user }
                }
            };
            return JsonSerializer.Serialize(payload);
        }

        private static string BuildAnthropicBody(string model, string system, string user)
        {
            var payload = new AnthropicRequest
            {
                model = model,
                max_tokens = 1024,
                system = system,
                messages = new[]
                {
                    new ChatMessage { role = "user", content = user }
                }
            };
            return JsonSerializer.Serialize(payload);
        }

        private static (string System, string User) ResolvePrompt(string prompt, string text)
        {
            var instruction = string.IsNullOrWhiteSpace(prompt)
                ? AiProviderDefaults.DefaultPrompt
                : prompt.Trim();

            if (instruction.IndexOf("{text}", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var user = Regex.Replace(instruction, @"\{text\}", text ?? string.Empty, RegexOptions.IgnoreCase);
                return (AiProviderDefaults.DefaultPrompt, user);
            }

            return (instruction, text ?? string.Empty);
        }

        private static string ExtractContent(AiProviderType provider, string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;

                if (provider == AiProviderType.Ollama)
                {
                    if (root.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var ollamaContent))
                        return ollamaContent.GetString();
                    if (root.TryGetProperty("response", out var response))
                        return response.GetString();
                }
                else if (provider == AiProviderType.Anthropic)
                {
                    if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var block in content.EnumerateArray())
                        {
                            if (block.TryGetProperty("text", out var text))
                                return text.GetString();
                        }
                    }
                }
                else
                {
                    if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                    {
                        var first = choices[0];
                        if (first.TryGetProperty("message", out var message) &&
                            message.TryGetProperty("content", out var content))
                            return content.GetString();
                        if (first.TryGetProperty("text", out var text))
                            return text.GetString();
                    }
                }
            }

            return null;
        }

        private static List<string> ParseNumbered(string raw, int expected)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var lines = raw
                .Replace("\r\n", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => Regex.Replace(l.Trim(), @"^\d+[\.\)、]\s*", string.Empty))
                .Where(l => l.Length > 0)
                .ToList();

            if (lines.Count == expected) return lines;
            return result;
        }

        private static bool IsLoopback(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            return uri.IsLoopback;
        }

        private class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        private class OpenAiRequest
        {
            public string model { get; set; }
            public double temperature { get; set; }
            public ChatMessage[] messages { get; set; }
        }

        private class OllamaRequest
        {
            public string model { get; set; }
            public bool stream { get; set; }
            public ChatMessage[] messages { get; set; }
        }

        private class AnthropicRequest
        {
            public string model { get; set; }
            public int max_tokens { get; set; }
            public string system { get; set; }
            public ChatMessage[] messages { get; set; }
        }
    }
}
