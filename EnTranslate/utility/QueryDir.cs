using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using EnTranslate.Model;

namespace EnTranslate.utility
{
    public static class QueryDir
    {
        /// <summary>
        /// 获取字典，并且查找其翻译
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string getDir(string word)
        {
            string prefix = word.Substring(0,2);
            string dir = utility.utlis.ReadEmbeddedResource($"EnTranslate.Translates.{prefix}.json");
            // 转换为json
            Dictionary<string, object>? jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(dir);
            if (jsonObject != null && jsonObject.TryGetValue(word, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        return value.ToString();
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        Dictionarys dictionaryValue = JsonSerializer.Deserialize<Dictionarys>(jsonElement.GetRawText());
                        if (dictionaryValue != null)
                        {
                            return dictionaryValue.t;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
