using mima_c.ast;
using System.Diagnostics;
using System.IO;

namespace mima_c.compiler
{
    class Compiler
    {
        StreamWriter outputFile { get; }
        string fileToCompileTo { get; }

        public Compiler(string fileToCompileTo)
        {
            this.outputFile = File.CreateText(fileToCompileTo);
            this.fileToCompileTo = fileToCompileTo;
        }

        private void AddCommand(string command)
        {
            outputFile.WriteLine(command);
        }

        private void CreateMimaHeader()
        {
            // Add StackPointer, FramePoiter, Registers, Stack etc. to start of File
        }

        // AST might have to be replaced with a PreEvaluated Intermediate Program representation
        //   With compiletime known Values replaced 
        //   With Types connected to Variables
        public Runnable Compile(PreCompiler.PreCompiledAST preCompiled)
        {
            CreateMimaHeader();

            // Compile Here

            // Setup Function jumps
            // Create Statements in Function bodies etc.

            outputFile.Close();
            return new Runnable(fileToCompileTo);
        }

        public class Runnable
        {
            string fileName { get; }

            public Runnable(string fileName)
            {
                this.fileName = fileName;
            }

            public int Run()
            {
                Process p = Process.Start("../../../compiler/Mima.exe", '"' + fileName + '"');
                p.WaitForExit();
                return p.ExitCode;
            }
        }

    }
}
