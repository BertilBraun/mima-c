using System;
using System.IO;
using System.Linq;

namespace mima_c
{
    class MimaC
    {
        static void Main(string[] args)
        {
            bool showOuput = !args.Contains("--no_debug");
            string file;
            if (args.Contains("--file"))
                file = args[Array.IndexOf(args, "--file") + 1];
            else
                file = "../../../src/test.c";
            
            string inputText = File.ReadAllText(file);

            string preprozessedText = new Preprozessor(inputText).getProcessedText();
            if (showOuput)
            {
                Console.WriteLine("Preprozessing Done");
                Console.WriteLine(preprozessedText);
            }

            TokenStream tokenStream = new Lexer(preprozessedText).getTokenStream();
            if (showOuput)
            {
                Console.WriteLine("Lexing Done");
                Console.WriteLine(tokenStream);
            }

            AST ast = new AEParser(tokenStream).parse();
            if (showOuput)
            {
                Console.WriteLine("Parsing Done");
                Console.WriteLine("--------------------- :AST: ---------------------");
                Console.WriteLine(ast.ToString());
                Console.WriteLine("-------------------------------------------------");
            }

            int result = new interpreter.Interpreter(ast).interpret();
            if (showOuput)
            {
                Console.WriteLine();
                Console.WriteLine("Interpreting Done");
                Console.WriteLine("Result: " + result.ToString());
            }

            Environment.Exit(result);
        }
    }
}
