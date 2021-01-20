using mima_c.ast;
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

            string preprozessedText = new Preprozessor(inputText).GetProcessedText();
            if (showOuput)
            {
                Console.WriteLine("Preprozessing Done");
                Console.WriteLine(preprozessedText);
            }

            TokenStream tokenStream = new Lexer(preprozessedText).GetTokenStream();
            if (showOuput)
                Console.WriteLine("Lexing Done");

            AST ast = new CParser(tokenStream).Parse();
            if (showOuput)
            {
                Console.WriteLine("Parsing Done");
                Console.WriteLine("--------------------- :AST: ---------------------");
                Console.WriteLine(ast.ToString());
                Console.WriteLine("-------------------------------------------------");
            }

            int result = new interpreter.Interpreter(ast).Interpret();
            if (showOuput)
            {
                Console.WriteLine();
                Console.WriteLine("Interpreting Done");
                Console.WriteLine("Result: " + result.ToString());
            }

            result = new compiler.Compiler("output.mima").Compile(ast).Run();

            Environment.Exit(result);
        }
    }
}
