using System;
using engine;

namespace re2csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Cox's simple example.
            string text = "abbb";
            string pat = "abab|abbb";
            var compiled = RE2.compile(pat);
            var result = compiled.match(text);
            System.Console.WriteLine("Result of pat '" + pat + "' in text '" + text + "'" + result);
        }
    }
}
