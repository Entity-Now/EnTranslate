using EnTranslate.NetworkTranslates;
using EnTranslate.NetworkTranslates.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Utility
{
    public static class TranslateHelper
    {
        static Dictionary<TranslateType, Type> translates = new Dictionary<TranslateType, Type>
        {
            [TranslateType.Bing ] = typeof(EdgeTranslate),
            [TranslateType.Google ] = typeof(GoogleTranslate),
            [TranslateType.Deep ] = typeof(DeepTranslate),
            [TranslateType.Yandex ] = typeof(YandexTranslate),
        };
        public static async Task<List<string>> getTranslateAsync(List<string> text)
        {
            var type = translates[Constants.Config.TranslateType];
            var NetworkTranslate = (ANetworkTranslate)Activator.CreateInstance(type);


            return await NetworkTranslate.Translate(text, "eng", "zho-CN");
        }
    }
}
