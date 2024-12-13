using EnTranslate.NetworkTranslates.Interfaces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EnTranslate.NetworkTranslates
{
    public class DeepTranslate : ANetworkTranslate
    {
        private class DeepLResponse
        {
            public string Id { get; set; }

            public string Jsonrpc { get; set; }

            public DeepLResult Result { get; set; }
        }

        private class DeepLResult
        {
            public DeepLTextResult[] Texts { get; set; }

            public string Lang { get; set; }
        }

        private class DeepLTextResult
        {
            public string Text { get; set; }
        }


        private static readonly string baseUrl = "https://www2.deepl.com/jsonrpc";

        private static readonly Dictionary<string, string> supportLanguages = new Dictionary<string, string>
        {
            {"zho-CN", "ZH"},
            {"zho-TW", "ZH"},
            {"eng", "EN"},
            {"jpn", "JA"},
            {"kor", "KO"},
            {"fre", "FR"},
            {"spa", "ES"},
            {"rus", "RU"},
            {"ger", "DE"},
            {"ita", "IT"},
            {"tur", "TR"},
            {"por-PT", "PT-PT"},
            {"por", "PT-BR"},
            //{"vie", ""},
            {"ind", "ID"},
            //{"tha", ""},
            //{"msa", ""},
            {"ara", "AR"},
            //{"hin", ""},
        };

        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly Random rndId = new Random(Guid.NewGuid().GetHashCode());

        static DeepTranslate()
        {
            rndId = new Random(Guid.NewGuid().GetHashCode());

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, //, | DecompressionMethods.Brotli,
            };
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("x-app-os-name", "iOS");
            httpClient.DefaultRequestHeaders.Add("x-app-os-version", "16.3.0");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate"); //, br
            httpClient.DefaultRequestHeaders.Add("x-app-device", "iPhone13,2");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DeepL-iOS/2.9.1 iOS 16.3.0 (iPhone13,2)");
            httpClient.DefaultRequestHeaders.Add("x-app-build", "510265");
            httpClient.DefaultRequestHeaders.Add("x-app-version", "2.9.1");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }
        public override Task<string> GetToken()
        {
            return Task.FromResult(string.Empty);
        }
        public override Task<string> Detect(string text, string token = null)
        {
            throw new NotImplementedException();
        }
        public override async Task<List<string>> Translate(List<string> texts, string fromLan, string toLan, string token = null)
        {
            List<string> result = new List<string>();

            long timestamp = this.GetTimestamp(this.GetICount(texts[0]));
            var id = rndId.Next(11111111, 99999999);

            var text = ArrayToString(texts);

            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "LMT_handle_texts",
                @params = new
                {
                    splitting = "newlines",
                    lang = new
                    {
                        source_lang_user_selected = supportLanguages[fromLan],
                        target_lang = supportLanguages[toLan],
                    },
                    //commonJobParams = new
                    //{
                    //    wasSpoken = false,
                    //    transcribe_as = string.Empty,
                    //},
                    texts = new[]
                    {
                        new
                        {
                            text,
                            request_alternatives = 3,
                        },
                    },
                    timestamp,
                },
                id,
            };

            var requestBodyText = JsonConvert.SerializeObject(requestBody);


            if ((id + 5) % 29 == 0 || (id + 3) % 13 == 0)
            {
                requestBodyText = requestBodyText.Replace("\"method\":\"", "\"method\" : \"");
            }
            else
            {
                requestBodyText = requestBodyText.Replace("\"method\":\"", "\"method\": \"");
            }

            var response = await httpClient.PostAsync(baseUrl, new StringContent(requestBodyText, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var transResponse = JsonConvert.DeserializeObject<DeepLResponse>(jsonResponse);

            result = StringToArray(transResponse.Result.Texts[0].Text);

            return result;
        }

        private int GetICount(string translateText)
        {
            return translateText.Count(c => c == 'i');
        }

        private long GetTimestamp(int iCount)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (iCount == 0)
            {
                return timestamp;
            }

            iCount++;
            return timestamp - (timestamp % iCount) + iCount;
        }
    }
}
