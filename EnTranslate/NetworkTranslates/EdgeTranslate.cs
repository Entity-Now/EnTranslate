using EnTranslate.NetworkTranslates.Interfaces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Linq;
using System.Threading;

namespace EnTranslate.NetworkTranslates
{
    /// <summary>
    /// <see cref="https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin/blob/f2eddae3f54970845472abb66366afab23536f45/MultiSupplierMTPlugin/Services/MicrosoftBuiltIn.cs"/>
    /// </summary>
    public class EdgeTranslate : ANetworkTranslate
    {
        private static readonly string tokenUrl = "https://edge.microsoft.com/translate/auth";

        private static readonly string baseUrl = "https://api-edge.cognitive.microsofttranslator.com/translate";
        private static readonly string defectUrl = "https://api-edge.cognitive.microsofttranslator.com/detect";

        private static readonly Dictionary<string, string> supportLanguages = new Dictionary<string, string>
        {
            {"zho-CN", "zh-Hans"},
            {"zho-TW", "zh-Hant"},
            {"eng", "en"},
            {"jpn", "ja"},
            {"kor", "ko"},
            {"fre", "fr"},
            {"spa", "es"},
            {"rus", "ru"},
            {"ger", "de"},
            {"ita", "it"},
            {"tur", "tr"},
            {"por-PT", "pt-pt"},
            {"por", "pt"},
            {"vie", "vi"},
            {"ind", "id"},
            {"tha", "th"},
            {"msa", "ms"},
            {"ara", "ar"},
            {"hin", "hi"},
        };

        private static readonly HttpClient httpClient = new HttpClient();

        private class TranslationRequestItem
        {
            public string Text { get; set; }
        }

        private class TranslationResponseItem
        {
            public List<Translation> Translations { get; set; }
        }

        private class Translation
        {
            public string Text { get; set; }
            public string To { get; set; }
            public SentenceLength SentLen { get; set; }
        }

        private class SentenceLength
        {
            public List<int> SrcSentLen { get; set; }
            public List<int> TransSentLen { get; set; }
        }
        private class LanguageInfo
        {
            [JsonProperty("isTranslationSupported")]
            public bool IsTranslationSupported { get; set; }

            [JsonProperty("isTransliterationSupported")]
            public bool IsTransliterationSupported { get; set; }

            [JsonProperty("language")] public string Language { get; set; } = "";

            [JsonProperty("score")] public double Score { get; set; }
        }
        static EdgeTranslate()
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            httpClient.DefaultRequestHeaders.Add("accept-language", "zh-TW,zh;q=0.9,ja;q=0.8,zh-CN;q=0.7,en-US;q=0.6,en;q=0.5");
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"113\", \"Chromium\";v=\"113\", \"Not-A.Brand\";v=\"24\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://appsumo.com/");
            httpClient.DefaultRequestHeaders.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.42");
        }

        public override async Task<string> GetToken()
        {
            HttpResponseMessage tokenResponse = await httpClient.GetAsync(tokenUrl);
            tokenResponse.EnsureSuccessStatusCode();
            string token = await tokenResponse.Content.ReadAsStringAsync();

            return token;
        }

        public override async Task<string> Detect(string text, string token = null)
        {
            if (string.IsNullOrEmpty(token)) 
            {
                token = await GetToken();
            }
            var data = new[] { new { Text = text } };

            var req = JsonConvert.SerializeObject(data);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, defectUrl + "?api-version=3.0");
            requestMessage.Headers.Add("Authorization", "Bearer "+ token);
            requestMessage.Content = new StringContent(req, Encoding.UTF8, "application/json");

            var httpRes = await httpClient.SendAsync(requestMessage);
            httpRes.EnsureSuccessStatusCode();

            string jsonRes = await httpRes.Content.ReadAsStringAsync();
            var objectRes = JsonConvert.DeserializeObject<List<LanguageInfo>>(jsonRes);

            return objectRes.First().Language;
        }

        public override async Task<List<string>> Translate(List<string> texts, string fromLan, string toLan, string token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken();
            }

            string url = baseUrl + $"?from={supportLanguages[fromLan]}&to={supportLanguages[toLan]}&apiVersion=3.0&includeSentenceLength=true";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Add("Authorization", "Bearer " + token);

            List<TranslationRequestItem> translationRequestItems = new List<TranslationRequestItem>();
            foreach (string text in texts)
            {
                translationRequestItems.Add(new TranslationRequestItem { Text = text });
            }
            string jsonRequest = JsonConvert.SerializeObject(translationRequestItems);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            List<TranslationResponseItem> translationResponseItems = JsonConvert.DeserializeObject<List<TranslationResponseItem>>(jsonResponse);

            List<string> result = new List<string>();
            foreach (TranslationResponseItem translationItem in translationResponseItems)
            {
                foreach (Translation translation in translationItem.Translations)
                {
                    result.Add(translation.Text);
                }
            }

            return result;
        }
    }
}
