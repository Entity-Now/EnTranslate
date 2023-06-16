using EnTranslate.utility;

namespace testTranslate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var word = Console.ReadLine()!;
            Console.WriteLine(QueryDir.getDir(word));
            Console.WriteLine("Hello, World!");
        }
    }
}