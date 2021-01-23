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
            AddCommand("LDC 0");
            AddCommand("STV " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.FramePointerPosition);
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

        public void Push(int addr, int size = 1)
        {
            Scope.stackPointer += size;

            AddCommand("");
            AddCommand("// Push");

            // Store Akku value 
            if (addr == Settings.AkkuPosition)
                AddCommand("STV " + Settings.PushPopPointerPosition);

            // increment stack pointer
            AddCommand("LDC " + size);
            AddCommand("ADD " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.StackPointerPosition);

            // Load value to be stored
            if (addr == Settings.AkkuPosition)
                AddCommand("LDV " + Settings.PushPopPointerPosition);
            else
                AddCommand("LDV " + addr);

            // Set value
            AddCommand("STIV " + Settings.StackPointerPosition);
            AddCommand("");
        }

        public void Pop(int addr, int size = 1)
        {
            AddCommand("");
            AddCommand("// Pop");
            if (addr == Settings.AkkuPosition)
            {
                AddCommand("LDIV " + Settings.StackPointerPosition);
                AddCommand("STV " + Settings.PushPopPointerPosition);
                AddCommand("LDC " + (-size));
                AddCommand("ADD " + Settings.StackPointerPosition);
                AddCommand("STV " + Settings.StackPointerPosition);
                AddCommand("LDV " + Settings.PushPopPointerPosition);
            }
            else
            {
                // store akku value
                AddCommand("STV " + Settings.PushPopPointerPosition);

                // Load value into addr
                AddCommand("LDIV " + Settings.StackPointerPosition);
                AddCommand("STV " + addr);

                // decrement Stack Pointer
                AddCommand("LDC " + (-size));
                AddCommand("ADD " + Settings.StackPointerPosition);
                AddCommand("STV " + Settings.StackPointerPosition);

                // reload akku value
                AddCommand("LDV " + Settings.PushPopPointerPosition);
            }
            AddCommand("");

            Scope.stackPointer -= size;
        }

        void Walk(AST node, Scope scope)
        {
            throw new NotSupportedException(node.GetType().Name + " is not yet Implemented!");
        }
        void Walk(NoOp node, Scope scope)
        {
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
            AddCommand("STV " + Settings.LastAddrPointerPosition);
            AddCommand("LDIV " + Settings.LastAddrPointerPosition);
        }
        // PUSH node.type.size
        // Store location of value in scope, to access addr later
        void Walk(VariableDecl node, Scope scope)
        {
            AddDescription(node);
            // TODO get TypeSize from node.type
            // TODO how are structs supposed to work?
            Push(Settings.AkkuPosition);
            scope.AddVariable(node.identifier);

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

            Push(Settings.RegisterPostions[0]);

            Walk(node.identifier, scope);
            AddCommand("");
            AddCommand("LDV " + Settings.LastAddrPointerPosition);
            AddCommand("STV " + Settings.RegisterPostions[0]);
            Walk(node.node, scope);
            AddCommand("");
            AddCommand("STIV " + Settings.RegisterPostions[0]);

            Pop(Settings.RegisterPostions[0]);
        }
        void Walk(FuncCall node, Scope scope)
        {
            AddDescription(node);
            // return address
            // Offset to return to after JMP... 
            // Arguments could also be expr, therefore more than 6 commands long,
            // then reset addr ist set back to the wrong location
            AddCommand("LDC " + (14 + node.arguments.Count * 6));
            AddCommand("ADD " + Settings.Mima.InstructionPointer);
            Push(Settings.AkkuPosition);

            // old FramePointerPosition
            Push(Settings.FramePointerPosition);

            // set new FramePointerPosition
            AddCommand("LDV " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.FramePointerPosition);

            foreach (var argument in node.arguments)
            {
                Walk(argument, scope);
                Push(Settings.AkkuPosition);
            }

            AddCommand("JMP " + node.identifier);
            Pop(Settings.AkkuPosition);
        }
        void Walk(FuncDecl node, Scope scope)
        {
            // Irrelevant?
        }
        void Walk(FuncDef node, Scope scope)
        {
            AddDescription(node);
            // TODO Make sure, that params are at correct position in funcScope

            Scope funcScope = new Scope(scope);
            foreach (var param in node.parameters)
                funcScope.AddVariable(param.identifier);

            AddCommand(node.identifier + ":");

            // Stack pointer value must be the same before the call as after, right?
            int stackPointerValue = Scope.stackPointer;
            Walk(node.block, funcScope);
            Scope.stackPointer = stackPointerValue;
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
            AddCommand("STV " + Settings.ReturnRegisterPostions[0]);

            // reset StackPointerPosition
            AddCommand("LDV " + Settings.FramePointerPosition);
            AddCommand("STV " + Settings.StackPointerPosition);

            // restore FramePointerPosition
            Pop(Settings.FramePointerPosition);

            // return to call address
            Pop(Settings.ReturnRegisterPostions[1]);

            // push return value to Stack
            Push(Settings.ReturnRegisterPostions[0]);

            AddCommand("LDV " + Settings.ReturnRegisterPostions[1]);
            AddCommand("STV " + Settings.Mima.InstructionPointer);
        }
        void Walk(Intrinsic node, Scope scope)
        {
            if (node.type == "printf")
            {
                AddDescription(node);

                if (node.parameters.Count == 0)
                    AddCommand("PRINTAKKU");

                foreach (var parameter in node.parameters)
                {
                    Walk(parameter, scope);
                    AddCommand("PRINTAKKU");
                }
            }
        }
        void Walk(BinaryArithm node, Scope scope)
        {
            AddDescription(node);

            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?

            Push(Settings.RegisterPostions[0]);

            Walk(node.leftNode, scope);
            AddCommand("");
            AddCommand("STV " + Settings.RegisterPostions[0]);
            Walk(node.rightNode, scope);
            AddCommand("");

            if (node.operation == TokenType.PLUS)
                AddCommand("ADD " + Settings.RegisterPostions[0]);
            else
                throw new InvalidOperationException("Operation " + node.operation + " not Implemented!");

            Pop(Settings.RegisterPostions[0]);
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
                // Console.WriteLine(File.ReadAllText(fileName));

                Process p = Process.Start("../../../compiler/Mima.exe", '"' + fileName + '"');
                p.WaitForExit();
                return p.ExitCode;
            }
        }
    }
}
