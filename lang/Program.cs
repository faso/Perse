using lang.Lexing;
using Lang.REPL;
using System;
using System.Diagnostics;
using System.IO;

namespace lang
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n   ___/-\\___");
            Console.WriteLine("  |---------|");
            Console.WriteLine("   | | | | |");
            Console.WriteLine("   | | | | |");
            Console.WriteLine("   | | | | |");
            Console.WriteLine("   | | | | |");
            Console.WriteLine("   |_______|\n");
            Console.WriteLine("  TrashLang v0.02\n");

            REPL.Start();

            Console.ReadKey(); 
        }
    }
}
