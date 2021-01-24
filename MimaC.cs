using mima_c.ast;
using mima_c.compiler;
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

            string preprozessedText = new PreProzessor(inputText).GetProcessedText();
            if (showOuput)
            {
                Console.WriteLine("Preprozessing Done");
                Console.WriteLine(preprozessedText);
            }

            TokenStream tokenStream = new Lexer(preprozessedText).GetTokenStream();
            if (showOuput)
                Console.WriteLine("Lexing Done");

            Program ast = new CParser(tokenStream).Parse();
            if (showOuput)
            {
                Console.WriteLine("Parsing Done");
                Console.WriteLine("--------------------- :AST: ---------------------");
                Console.WriteLine(ast.ToString());
                Console.WriteLine("-------------------------------------------------");
            }

            // int result = new interpreter.Interpreter(ast).Interpret();
            // if (showOuput)
            // {
            //     Console.WriteLine();
            //     Console.WriteLine("Interpreting Done");
            //     Console.WriteLine("Result: " + result.ToString());
            // }

            PreCompiler.PreCompiledAST preCompiled = new PreCompiler().PreComile(ast);
            if (showOuput)
            {
                Console.WriteLine("Pre Compilation Done");
                Console.WriteLine("--------------------- :AST: ---------------------");
                Console.WriteLine(preCompiled.Program.ToString());
                Console.WriteLine("------------------ :FUNCTIONS: ------------------");
                foreach (var func in preCompiled.Functions)
                    Console.WriteLine(func.ToString());
                Console.WriteLine("-------------------------------------------------");
            }

            Compiler.Runnable compiled = new Compiler("output.mima").Compile(preCompiled);
            if (showOuput)
                Console.WriteLine("Compilation Done");

            int result = compiled.Run();
            if (showOuput)
            {
                Console.WriteLine();
                Console.WriteLine("Running Done");
                Console.WriteLine("Result: " + result.ToString());
            }

            Environment.Exit(result);
        }
    }
}
