using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnTranslate.Model;
using Newtonsoft.Json.Linq;

namespace EnTranslate.utility
{
    public static class QueryDir
    {
        /// <summary>
        /// 获取字典，并且查找其翻译
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static Dictionarys getDir(string word)
        {
            try
            {
                word = word.Trim();
                if (string.IsNullOrEmpty(word) || word.Length < 2)
                {
                    return null;
                }
                string prefix = word.Substring(0, 2);
                string dir = utlis.ReadEmbeddedResource($"EnTranslate.Translates.{prefix.ToLower()}.json");
                if (string.IsNullOrEmpty(dir) || dir is null)
                {
                    return null;
                }
                // 转换为 JSON 对象
                JObject jsonObject = JObject.Parse(dir);

                if (jsonObject.TryGetValue(word, out JToken value))
                {
                    if (value.Type == JTokenType.String)
                    {
                        return new Dictionarys
                        {
                            key = word,
                            t = value.ToString()
                        };
                    }
                    else if (value.Type == JTokenType.Object)
                    {
                        Dictionarys dictionaryValue = value.ToObject<Dictionarys>();
                        if (dictionaryValue != null)
                        {
                            dictionaryValue.key = word;
                            return dictionaryValue;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
