using mima_c.ast;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        private void AddDescription(AST node)
        {
            AddCommand("");
            AddCommand("// " + node.Representation());
        }
        private void AddCommand(string command)
        {
            if (command.Contains(':'))
                outputFile.WriteLine(command);
            else
                outputFile.WriteLine('\t' + command);
        }

        private void CreateMimaHeader()
        {
            // Add StackPointer, FramePoiter, Registers, Stack etc. to start of File
        }

        public Runnable Compile(PreCompiler.PreCompiledAST preCompiled)
        {
            CreateMimaHeader();

            Scope globalScope = new Scope();
            try
            {
                Walk(preCompiled.Program, globalScope);

                AddCommand("");
                AddCommand("HALT // Program End");

                foreach (var function in preCompiled.Functions)
                    Walk(function, globalScope);

            }
            catch (Exception e)
            {
                Console.WriteLine("Compilation Error:");
                Console.WriteLine(e.Message);
            }

            outputFile.Close();
            return new Runnable(fileToCompileTo);
        }

        public void Push(int size = 1)
        {
            Scope.stackPointer += size;

            AddCommand("");
            AddCommand("// Push");
            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDC " + size);
            AddCommand("ADD " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.StackPointerPosition);
            AddCommand("LDV " + Settings.RegisterPostions[0]);
            AddCommand("STIV " + Settings.StackPointerPosition);
            AddCommand("");
        }

        public void Pop(int size = 1)
        {
            AddCommand("");
            AddCommand("// Pop");
            AddCommand("LDIV " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDC " + (-size));
            AddCommand("ADD " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.StackPointerPosition);
            AddCommand("LDV " + Settings.RegisterPostions[0]);
            AddCommand("");

            Scope.stackPointer -= size;
        }

        void Walk(AST node, Scope scope)
        {
            throw new NotSupportedException(node.GetType().Name + " is not yet Implemented!");
        }

        // LOAD value to Akku
        void Walk(Literal node, Scope scope)
        {
            AddDescription(node);
            // No need for explicit error handling. If they can't be converted
            // it's a compiler bug
            switch (node.type)
            {
                case Literal.Type.INTLITERAL:
                    AddCommand("LDC " + node.value);
                    break;
                default:
                    throw new NotImplementedException("Not implemented Type of Literal: " + node.type.ToString());
            }
        }
        // LDC addr
        // ADD FramePointerLocation
        // STV Register
        // LDIV Register
        void Walk(Variable node, Scope scope)
        {
            AddDescription(node);
            int addr = scope.GetAddr(node.identifier);

            AddCommand("LDC " + addr);
            AddCommand("ADD " + Settings.FramePointerPosition);
            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDIV " + Settings.RegisterPostions[0]);
        }
        // PUSH node.type.size
        // Store location of value in scope, to access addr later
        void Walk(VariableDecl node, Scope scope)
        {
            AddDescription(node);
            // TODO get TypeSize from node.type
            // TODO how are structs supposed to work?
            scope.AddVariable(node.identifier);
            Push();

            //if (type == RuntimeType.Type.Struct)
            //    Scope structValues = new Scope(null);
            //    foreach (var decl in customTypes[node.type])
            //        Walk(decl, structValues);
        }
        // LOAD node.identifier
        // STV Register
        // LOAD node.value
        // STIV Register
        void Walk(VariableAssign node, Scope scope)
        {
            AddDescription(node);
            // TODO This doesnt work for sure
            //      This gets the value at node.identifier
            //      Should store addr of node.identifier in akku
            Walk(node.identifier, scope);
            AddCommand("STV " + Settings.RegisterPostions[0]);
            Walk(node.node, scope);
            AddCommand("STIV " + Settings.RegisterPostions[0]);
        }
        void Walk(FuncCall node, Scope scope)
        {
            AddDescription(node);
            // return address
            AddCommand("LDC " + (16 + node.arguments.Count * 6)); // Offset to return to after JMP 
            AddCommand("ADD " + Settings.Mima.InstructionPointer);
            Push();
            // old FramePointerPosition
            AddCommand("LDV " + Settings.FramePointerPosition);
            Push();

            // set new FramePointerPosition
            AddCommand("LDV " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.FramePointerPosition);

            foreach (var argument in node.arguments)
            {
                Walk(argument, scope);
                Push();
            }

            AddCommand("JMP " + node.identifier);
        }
        // Irrelevant?
        void Walk(FuncDecl node, Scope scope)
        {
            //Function function = new Function(node.returnType.GetRuntimeType());

            //List<FunctionParam> parameteres = new List<FunctionParam>();
            //foreach (var param in node.parameters)
            //    parameteres.Add(new FunctionParam(param.type.GetRuntimeType(), param.identifier));

            //FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            //scope.AddSymbol(signature, function);
        }
        void Walk(FuncDef node, Scope scope)
        {
            AddDescription(node);
            // TODO Make sure, that params are at correct position in funcScope

            Scope funcScope = new Scope(scope);
            foreach (var param in node.parameters)
                funcScope.AddVariable(param.identifier);

            AddCommand(node.identifier + ":");
            Walk(node.block, funcScope);
            // Precompiler ensures, that a return statement is at the end of the node.block
        }
        void Walk(Statements node, Scope scope)
        {
            foreach (var statement in node.statements)
                Walk(statement, scope);
        }
        void Walk(BlockStatements node, Scope scope, Scope copyScope = null)
        {
            Scope blockScope = (copyScope != null) ? copyScope : scope;

            Walk((Statements)node, blockScope);
        }
        void Walk(Program node, Scope scope)
        {
            Walk((Statements)node, scope);
        }
        void Walk(Return node, Scope scope)
        {
            AddDescription(node);
            Walk(node.returnExpr, scope);
            AddCommand("");

            // store return value
            AddCommand("STV " + Settings.RegisterPostions[0]);

            // reset StackPointerPosition
            AddCommand("LDV " + Settings.FramePointerPosition);
            AddCommand("STV " + Settings.StackPointerPosition);

            // restore FramePointerPosition
            Pop();
            AddCommand("STV " + Settings.FramePointerPosition);

            // return to call address
            Pop();
            AddCommand("STV " + Settings.RegisterPostions[1]);

            // push return value to Stack
            AddCommand("LDV " + Settings.RegisterPostions[0]);
            Push();

            AddCommand("LDV " + Settings.RegisterPostions[1]);
            AddCommand("STV " + Settings.Mima.InstructionPointer);
        }
        void Walk(Intrinsic node, Scope scope)
        {
            if (node.type == "printf")
            {
                AddDescription(node);
                if (node.parameters.Count == 1)
                    Walk(node.parameters.First(), scope);
                AddCommand("PRINTAKKU");
                // if (node.parameters.Count == 0)
                //     Raise(node, scope, "printf needs at least one parameter");
                // //  TODO: implement proper printf
                // List<dynamic> parameters = new List<dynamic>(node.parameters.Count);
                // foreach (var param in node.parameters)
                //     parameters.Add(Walk(param, scope).GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances());
                // 
                // string formatString = parameters[0].ToString();
                // parameters.RemoveAt(0);
                // 
                // string ouput = formatString.Format(parameters.ToArray());
                // Console.WriteLine("printf: \"" + ouput + "\"");
            }
        }

        void Walk(NoOp node, Scope scope)
        {
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
                Console.WriteLine(File.ReadAllText(fileName));

                Process p = Process.Start("../../../compiler/Mima.exe", '"' + fileName + '"');
                p.WaitForExit();
                return p.ExitCode;
            }
        }
    }
}
