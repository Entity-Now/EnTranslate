using EnTranslate.NetworkTranslates.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnTranslate.NetworkTranslates
{
    public class GoogleTranslate : ANetworkTranslate
    {
        private static readonly string baseUrl = "https://translate.googleapis.com/translate_a/single";

        private static readonly Dictionary<string, string> supportLanguages = new Dictionary<string, string>
        {
            {"zho-CN", "zh-CN"},
            {"zho-TW", "zh-TW"},
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

        public override async Task<List<string>> Translate(List<string> texts, string fromLan, string toLan)
        {
            string url = baseUrl + $"?client=gtx&dt=t&sl={supportLanguages[fromLan]}&tl={supportLanguages[toLan]}&q={System.Web.HttpUtility.UrlEncode(ArrayToString(texts))}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JArray jsonArray = JArray.Parse(jsonResponse);

            string r = "";
            foreach (JToken jToken in jsonArray[0])
            {
                string t = jToken[0].Value<string>();
                r += t;
            }
            return StringToArray(r);
        }
    }
}
