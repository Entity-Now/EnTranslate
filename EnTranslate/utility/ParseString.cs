using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnTranslate.utility
{
    public static class ParseString
    {
        const char SplitValue = '_';
        /// <summary>
        /// 拆分字符串
        /// 例: foor-bar 拆分为 [foo, bar]
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static IEnumerable<string> getWordArray(string character)
        {
            if (string.IsNullOrWhiteSpace(character) && character.Length < 2)
            {
                return null;
            }
            List<string> Words = new List<string>();
            // 判断是否单个单词,并且是小写的
            if (Regex.IsMatch(character, @"^([a-z])+$"))
            {
                Words.Add(character.ToLower());
                return Words;
            }
            // 使用分隔符分割单词
            var new_words = new Regex(@"(?=[A-Z\-_\s])")
                .Split(character)
                .Select(i => MatchLetter(i));
            // 判断是否单词中包含特殊字符导致无法匹配，例如：数字等
            if (new_words.Count() <= 0)
            {
                Words.Add(MatchLetter(character));
            }

            return Words.Concat(new_words).Distinct();
        }
        public static string MatchLetter (string value)
        {
            var regex = new Regex(@"[A-Za-z]{2,}");
            var match = regex.Match(value);
            return match.Value.ToLower();
        }
    }
}
