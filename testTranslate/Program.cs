using MoqDictionary.utility;
using System.Text.RegularExpressions;

namespace testTranslate
{
    internal class Program
    {
        static void Main()
        {
            var word = Console.ReadLine();

            var words = ParseString.getWordArray(word);
            foreach (var item in words)
            {
                var TranslateVal = QueryDir.getDir(item);
                if (TranslateVal != null)
                {
                    Console.WriteLine(TranslateVal.t);
                }
            }
        }
    }
}