using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompilerPage.MiMa.interpreter
{
    class InstructionParser
    {
        struct JumpLoad : Instruction
        {
            public string element;
            public int idx;
            public string type;

            public JumpLoad(string element, int idx, string type)
            {
                this.element = element;
                this.idx = idx;
                this.type = type;
            }

            // Inherited via instruction
            public void Run(Mima mima) { Console.WriteLine("ERROR"); }
        }

        Dictionary<string, int> jumpLocations = new Dictionary<string, int>();
        List<Instruction> instructions = new List<Instruction>();
        List<JumpLoad> jumpLoads = new List<JumpLoad>();

        public List<Instruction> Parse(string filePath)
        {
            int lineCount = 1;
            List<(string, int)> lines = new List<(string, int)>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                string input = line;
                if (input.Contains("//"))
                    input = input.Substring(0, input.IndexOf("//"));
                input = input.Trim().ToLower();

                if (input != "")
                    lines.Add((input, lineCount));

                lineCount++;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var pair = lines[i];
                ParseLocation(ref pair.Item1, pair.Item2);
                if (pair.Item1 == "")
                    continue;

                Instruction instruction = ParseLine(pair.Item1, pair.Item2);
                if (instruction == null)
                    return null;
                instructions.Add(instruction);
            }

            foreach (JumpLoad jumpLoad in jumpLoads)
            {
                if (!jumpLocations.ContainsKey(jumpLoad.element))
                {
                    Console.WriteLine("Jump location could not be found! " + jumpLoad.element);
                    return null;
                }
                if (jumpLoad.type == "JMP")
                    instructions[jumpLoad.idx] = new JMP(jumpLocations[jumpLoad.element]);
                else if (jumpLoad.type == "JMN")
                    instructions[jumpLoad.idx] = new JMN(jumpLocations[jumpLoad.element]);
            }

            Console.WriteLine("Parsing complete! " + instructions.Count + " instructions");
            return instructions;
        }

        void ParseLocation(ref string line, int lineNumber)
        {
            List<string> elements = line.Split(':').ToList();

            if (elements.Count == 1 && line.Last() == ':')
                line = "";
            else
            {
                line = elements.Last();
                elements.RemoveAt(elements.Count - 1);
            }

            foreach (string element in elements)
            {
                if (jumpLocations.ContainsKey(element))
                {
                    Console.WriteLine("Error on line: " + lineNumber + " Jump loaction was already defined: " + element);
                    return;
                }

                jumpLocations[element] = instructions.Count;
            }
        }

        Instruction ParseLine(string line, int lineNumber)
        {
            Func<string, Instruction> error = (string msg) =>
            {
                Console.WriteLine("Error on line: " + line + " : " + lineNumber + " with message: " + msg);
                return null;
            };

            List<string> elements = line.Split(' ').ToList();
            string instructionCode = elements[0];

            try
            {
                if (instructionCode == "ldc")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'LDC'! 1 expected!");
                    return new LDC(parseInt(elements[1]));
                }
                else if (instructionCode == "ldv")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'LDV'! 1 expected!");
                    return new LDV(parseInt(elements[1]));
                }
                else if (instructionCode == "stv")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'STV'! 1 expected!");
                    return new STV(parseInt(elements[1]));
                }
                else if (instructionCode == "add")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'ADD'! 1 expected!");
                    return new ADD(parseInt(elements[1]));
                }
                else if (instructionCode == "and")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'AND'! 1 expected!");
                    return new AND(parseInt(elements[1]));
                }
                else if (instructionCode == "or")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'OR'! 1 expected!");
                    return new OR(parseInt(elements[1]));
                }
                else if (instructionCode == "xor")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'XOR'! 1 expected!");
                    return new XOR(parseInt(elements[1]));
                }
                else if (instructionCode == "eql")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'EQL'! 1 expected!");
                    return new EQL(parseInt(elements[1]));
                }
                else if (instructionCode == "jmp")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'JMP'! 1 expected!");
                    jumpLoads.Add(new JumpLoad(elements[1], (int)instructions.Count, "JMP"));
                    return jumpLoads.Last();
                }
                else if (instructionCode == "jmn")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'JMN'! 1 expected!");
                    jumpLoads.Add(new JumpLoad(elements[1], (int)instructions.Count, "JMN"));
                    return jumpLoads.Last();
                }
                else if (instructionCode == "halt")
                {
                    if (elements.Count != 1)
                        return error("Not the correct amount of parameters for 'HALT'! 0 expected!");
                    return new HALT();
                }
                else if (instructionCode == "not")
                {
                    if (elements.Count != 1)
                        return error("Not the correct amount of parameters for 'NOT'! 0 expected!");
                    return new NOT();
                }
                else if (instructionCode == "rar")
                {
                    if (elements.Count != 1)
                        return error("Not the correct amount of parameters for 'RAR'! 0 expected!");
                    return new RAR();
                }
                else if (instructionCode == "ldiv")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'LDIV'! 1 expected!");
                    return new LDIV(parseInt(elements[1]));
                }
                else if (instructionCode == "stiv")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'STIV'! 1 expected!");
                    return new STIV(parseInt(elements[1]));
                }
                else if (instructionCode == "printakku")
                {
                    if (elements.Count != 1)
                        return error("Not the correct amount of parameters for 'PRINTAKKU'! 0 expected!");
                    return new PRINTAKKU();
                }
                else if (instructionCode == "print")
                {
                    if (elements.Count != 2)
                        return error("Not the correct amount of parameters for 'PRINT'! 1 expected!");
                    return new PRINT(parseInt(elements[1]));
                }
                else
                    return error("Command not recognized!");
            }
            catch (Exception e)
            {
                return error(e.Message);
            }
        }

        int parseInt(string s)
        {
            try
            {
                if (s.IndexOf("0x") != -1)
                    return Convert.ToInt32(s.Substring(2, s.Length - 2), 16);
                if (s.Last() == 'b')
                    return Convert.ToInt32(s.Substring(0, s.Length - 1), 2);
                else
                    return Convert.ToInt32(s);
            }
            catch (Exception)
            {
                throw new Exception("Can't parse parameter: " + s);
            }
        }
    }
}
