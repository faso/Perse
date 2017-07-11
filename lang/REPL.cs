using lang.Lexing;
using Lang.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang.REPL
{
    public static class REPL
    {
        public static void Start()
        {
            var env = new Objects.Environment();
            while (true)
            {
                Console.Write(">> ");
                var s = Console.ReadLine();

                if (s == "exit")
                    return;

                if(s.StartsWith("file"))
                {
                    using (StreamReader sr = new StreamReader($"../../tests/{s.Split(' ')[1]}.txt"))
                        s = sr.ReadToEnd();
                }

                var lexer = new Lexer()
                {
                    input = s,
                    position = 0,
                    readPosition = 0
                };

                var AST = new Parser(lexer);
                var ev = new Evaluator();
                var res = AST.ParseProgram();
                if (AST.Errors.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Fucking up there, buddy! Parser errors: ");
                    foreach (var e in AST.Errors)
                        Console.WriteLine($"\t{e}\n");
                }
                else
                {
                    var evl = ev.Eval(res, env);
                    if (evl != null)
                        Console.WriteLine(evl.Inspect());
                }
            }
        }
    }
}
