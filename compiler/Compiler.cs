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
            // Add Call to Main here aswell, 
            // Main call should be the first thing executed, when the mima program gets run
        }

        // AST might have to be replaced with a PreEvaluated Intermediate Program representation
        //   With compile time known Values replaced 
        public Runnable Compile(AST node)
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
                Process p = Process.Start("Mima.exe", '"' + fileName + '"');
                p.WaitForExit();
                return p.ExitCode;
            }
        }

    }
}
