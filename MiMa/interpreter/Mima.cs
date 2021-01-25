using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CompilerPage.MiMa.interpreter
{
    public class Mima
    {
        public const int IrAddress = 1048500;

        public Int24 Akku;
        public Int24[] M;

        public List<Instruction> Instructions;
        public Action<string> WriteLine { get; }

        public Mima(List<Instruction> instructions, Action<string> WriteLine)
        {
            this.WriteLine = WriteLine;
            Instructions = instructions;
            Akku = new Int24();
            M = new Int24[1 << 20];
        }

        public void Step()
        {
            M[IrAddress]++;
            Instructions[M[IrAddress] - 1].Run(this);
        }

        public bool CanStep()
        {
            return M[IrAddress] < Instructions.Count;
        }

        public static void Run(string filePath)
        {
            Console.WriteLine("Now Running Program " + filePath);

            List<Instruction> instructions = new InstructionParser().Parse(filePath);

            if (instructions == null)
                return;

            Mima mima = new Mima(instructions, Console.WriteLine);

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (mima.CanStep())
                mima.Step();

            Console.WriteLine("Elapsed time: " + stopwatch.Elapsed + "s");
        }
    }
}
