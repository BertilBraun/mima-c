using mima_c.ast;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace mima_c.compiler
{
    class Compiler
    {
        private int uniqueID = 0;

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

        public void Push(int addr)
        {
            int size = 1;
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

        public void Pop(int addr)
        {
            int size = 1;

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

            foreach (var argument in node.arguments)
            {
                Walk(argument, scope);
                Push(Settings.AkkuPosition);
            }

            AddCommand("LDC " + 14);
            AddCommand("ADD " + Settings.Mima.InstructionPointer);
            Push(Settings.AkkuPosition);

            // old FramePointerPosition
            Push(Settings.FramePointerPosition);

            // set new FramePointerPosition
            AddCommand("LDV " + Settings.StackPointerPosition);
            AddCommand("STV " + Settings.FramePointerPosition);

            AddCommand("JMP " + node.identifier);

            // simulate pop of framepointer & pop return addr & push return value
            Scope.stackPointer--;

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
            // Stack pointer value must be the same before the call as after, right?
            int stackPointerValue = Scope.stackPointer;

            // Should be dependent on size of each parameter
            Scope.stackPointer -= node.parameters.Count;

            Scope funcScope = new Scope(scope);
            foreach (var param in node.parameters)
            {
                Scope.stackPointer += 1;
                funcScope.AddVariable(param.identifier);
            }

            AddCommand(node.identifier + ":");

            Walk(node.block, funcScope);
            // Precompiler ensures, that a return statement is at the end of the node.block

            Scope.stackPointer = stackPointerValue;
        }
        void Walk(Statements node, Scope scope)
        {
            foreach (var statement in node.statements)
                Walk(statement, scope);
        }
        void Walk(BlockStatements node, Scope scope, Scope copyScope = null)
        {
            Scope blockScope = (copyScope != null) ? copyScope : scope;

            // TODO push framepointer?

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
        //static Dictionary<TokenType, Func<int, int, RuntimeType>> operatorToBinaryFunc = new Dictionary<TokenType, Func<int, int, RuntimeType>>
        //    {
        //        {TokenType.MINUS,    (int x, int y) => new RuntimeType(x - y) },
        //        {TokenType.DIVIDE,   (int x, int y) => new RuntimeType(x / y) },
        //        {TokenType.STAR,     (int x, int y) => new RuntimeType(x * y) },
        //        {TokenType.MODULO,   (int x, int y) => new RuntimeType(x % y) },
        //        {TokenType.LT,       (int x, int y) => new RuntimeType((x < y) ? 1 : 0) },
        //        {TokenType.GT,       (int x, int y) => new RuntimeType((x > y) ? 1 : 0) },
        //        {TokenType.GEQ,      (int x, int y) => new RuntimeType((x >= y) ? 1 : 0) },
        //        {TokenType.LEQ,      (int x, int y) => new RuntimeType((x <= y) ? 1 : 0) },
        //    };
        void Walk(BinaryArithm node, Scope scope)
        {
            AddDescription(node);

            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?

            Push(Settings.RegisterPostions[0]);
            Push(Settings.RegisterPostions[1]);

            Walk(node.leftNode, scope);
            AddCommand("");
            AddCommand("STV " + Settings.RegisterPostions[0]);
            Walk(node.rightNode, scope);
            AddCommand("");

            if (node.operation == TokenType.PLUS)
                AddCommand("ADD " + Settings.RegisterPostions[0]);
            else if (node.operation == TokenType.EQ)
            {
                string isTrue = "trueEQ" + (uniqueID++);
                string end = "endEQ" + (uniqueID++);

                AddCommand("EQL " + Settings.RegisterPostions[0]);
                AddCommand("JMN " + isTrue);
                AddCommand("JMP " + end);
                AddCommand(isTrue + ":");
                AddCommand("LDC 1");
                AddCommand(end + ":");
            }
            else if (node.operation == TokenType.NEQ)
            {
                AddCommand("EQL " + Settings.RegisterPostions[0]);
                AddCommand("STV " + Settings.RegisterPostions[0]);
                AddCommand("LDC 1");
                AddCommand("ADD " + Settings.RegisterPostions[0]);
            }
            else if (node.operation == TokenType.AND)
            {
                // TODO doesnt currently work
                // (int x, int y) => (x != 0 && y != 0) ? 1 : 0

                // - EQL[address] = Compare accumulator with value of memory at address.
                //   Set accumulator to - 1 if same, else 0.
                // - JMN[label] = Jump to label if accumulator is negative

                string isFalse = "falseAND" + (uniqueID++);
                string end = "endAND" + (uniqueID++);

                AddCommand("STV " + Settings.RegisterPostions[1]);

                AddCommand("LDC 0");
                AddCommand("EQL " + Settings.RegisterPostions[0]);
                AddCommand("JMN " + isFalse);
                AddCommand("EQL " + Settings.RegisterPostions[1]);
                AddCommand("JMN " + isFalse);
                AddCommand("LDC 1");
                AddCommand("JMP " + end);
                AddCommand(isFalse + ":");
                AddCommand("LDC 0");
                AddCommand(end + ":");

            }
            else if (node.operation == TokenType.OR)
            {
                // TODO doesnt currently work
                // (int x, int y) => (x != 0 || y != 0) ? 1 : 0

                string firstFalse = "firstFalseOR" + (uniqueID++);
                string isFalse = "falseOR" + (uniqueID++);
                string end = "endOR" + (uniqueID++);

                AddCommand("STV " + Settings.RegisterPostions[1]);

                AddCommand("LDC 0");
                AddCommand("EQL " + Settings.RegisterPostions[0]);
                AddCommand("JMN " + firstFalse);
                // return value
                AddCommand("LDC 1");
                AddCommand("JMP " + end);
                AddCommand(firstFalse + ":");
                AddCommand("LDC 0");
                AddCommand("EQL " + Settings.RegisterPostions[1]);
                AddCommand("JMN " + isFalse);
                // return value
                AddCommand("LDC 1");
                AddCommand("JMP " + end);
                AddCommand(isFalse + ":");
                // return value
                AddCommand("LDC 0");
                AddCommand(end + ":");
            }
            else
                throw new InvalidOperationException("Operation " + node.operation + " not Implemented!");

            Pop(Settings.RegisterPostions[1]);
            Pop(Settings.RegisterPostions[0]);
        }
        //static Dictionary<TokenType, Func<int, int>> operatorToUnaryFunc = new Dictionary<TokenType, Func<int, int>>
        //    {
        //        {TokenType.PLUS,          (int x) => x },
        //        {TokenType.MINUS,         (int x) => -x },
        //
        //        {TokenType.NOT,           (int x) => (x == 0) ? 1 : 0 },
        //        {TokenType.LNOT,          (int x) => ~x },
        //    };
        void Walk(UnaryArithm node, Scope scope)
        {
            if (!node.op.In(TokenType.PLUSPLUS, TokenType.MINUSMINUS))
                throw new InvalidOperationException("Operation " + node.op + " not Implemented!");

            AddDescription(node);

            Push(Settings.RegisterPostions[0]);

            Walk(node.node, scope);
            AddCommand("STV " + Settings.RegisterPostions[0]);
            if (node.op == TokenType.PLUSPLUS)
                AddCommand("LDC 1");
            else
                AddCommand("LDC -1");
            AddCommand("ADD " + Settings.RegisterPostions[0]);
            AddCommand("STIV " + Settings.LastAddrPointerPosition);

            Pop(Settings.RegisterPostions[0]);
        }
        void Walk(For node, Scope scope)
        {
            AddDescription(node);

            scope.CreateCopy();

            Walk(node.initialization, scope);

            Push(Settings.RegisterPostions[0]);

            string start = "for" + (uniqueID++);
            string end = "endFor" + (uniqueID++);

            AddCommand(start + ":");

            Walk(node.condition, scope);

            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDC 0");
            AddCommand("EQL " + Settings.RegisterPostions[0]);

            AddCommand("JMN " + end);

            Walk(node.body, scope);

            Walk(node.loopExecution, scope);

            AddCommand("JMP " + start);

            AddCommand(end + ":");
            Pop(Settings.RegisterPostions[0]);

            scope.ResetToCopy();
        }
        void Walk(While node, Scope scope)
        {
            AddDescription(node);

            scope.CreateCopy();

            Push(Settings.RegisterPostions[0]);

            string start = "while" + (uniqueID++);
            string end = "endWhile" + (uniqueID++);

            AddCommand(start + ":");

            Push(Settings.RegisterPostions[0]);

            Walk(node.condition, scope);

            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDC 0");
            AddCommand("EQL " + Settings.RegisterPostions[0]);

            AddCommand("JMN " + end);
            Walk(node.body, scope);

            AddCommand("JMP " + start);

            AddCommand(end + ":");
            Pop(Settings.RegisterPostions[0]);

            scope.ResetToCopy();
        }
        /*void Walk(Break node, Scope scope)
        {
            throw new BreakExc();
        }
        void Walk(Continue node, Scope scope)
        {
            throw new ContinueExc();
        }*/
        void Walk(If node, Scope scope)
        {
            // if (Walk(node.condition, scope).Get<int>() != 0)
            //     Walk(node.ifBody, scope);
            // else
            //     Walk(node.elseBody, scope);

            AddDescription(node);

            Push(Settings.RegisterPostions[0]);

            Walk(node.condition, scope);

            string endIf = "endif" + (uniqueID++);
            string elseIf = "elseif" + (uniqueID++);

            AddCommand("STV " + Settings.RegisterPostions[0]);
            AddCommand("LDC 0");
            AddCommand("EQL " + Settings.RegisterPostions[0]);

            AddCommand("JMN " + elseIf);
            Walk(node.ifBody, scope);

            AddCommand("JMP " + endIf);
            AddCommand(elseIf + ":");
            Walk(node.elseBody, scope);

            AddCommand(endIf + ":");
            Pop(Settings.RegisterPostions[0]);
        }
        //void Walk(ArrayDecl node, Scope scope)
        //{
        //    int size = 0;
        //    if (node.countExpr != null)
        //        size = Walk(node.countExpr, scope).Get<int>();
        //
        //    Array variable = new Array(RuntimeType.GetTypeFromString(node.type), size);
        //    VariableSignature variableSignature = new VariableSignature(node.identifier);
        //    scope.AddSymbol(variableSignature, variable);
        //
        //    return RuntimeType.Void;
        //}
        //void Walk(ArrayAccess node, Scope scope)
        //{
        //    VariableSignature variableSignature = new VariableSignature(node.identifier);
        //    Array variable = scope.Translate(variableSignature) as Array;
        //
        //    if (variable == null)
        //        throw new TypeAccessException("Variable was not of type Array: " + node.identifier);
        //
        //    // TODO is an assumption, could also be a function
        //    int index = Walk(node.indexExpr, scope).Get<int>();
        //
        //    if (index < 0 || index >= variable.Values.Length)
        //        throw new IndexOutOfRangeException("Array index out of Range: " + index);
        //
        //    return variable.Values[index];
        //}
        //void Walk(ArrayLiteral node, Scope scope)
        //{
        //    List<RuntimeType> values = new List<RuntimeType>();
        //    foreach (var expr in node.valueListExprs)
        //        values.Add(Walk(expr, scope));
        //
        //    return new Array(values.ToArray());
        //}
        //
        //void Walk(PointerDecl node, Scope scope)
        //{
        //    Pointer variable = new Pointer(Walk(node.decl, scope));
        //    VariableSignature variableSignature = new VariableSignature(node.identifier);
        //    scope.AddSymbol(variableSignature, variable);
        //
        //    return RuntimeType.Void;
        //}
        //void Walk(PointerAccess node, Scope scope)
        //{
        //    RuntimeType pointer = Walk(node.node, scope);
        //    return pointer.Get<RuntimeType>();
        //}
        //void Walk(PointerLiteral node, Scope scope)
        //{
        //    return new Pointer(Walk(node.node, scope));
        //}
        void Walk(PostfixArithm node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?

            if (!node.operation.In(TokenType.PLUSPLUS, TokenType.MINUSMINUS))
                throw new InvalidOperationException("Operation " + node.operation + " not Implemented!");

            AddDescription(node);

            Push(Settings.RegisterPostions[0]);

            Walk(node.node, scope);

            AddCommand("STV " + Settings.RegisterPostions[0]);

            if (node.operation == TokenType.PLUSPLUS)
                AddCommand("LDC 1");
            else
                AddCommand("LDC -1");
            AddCommand("ADD " + Settings.RegisterPostions[0]);
            AddCommand("STIV " + Settings.LastAddrPointerPosition);

            // Reload old value, pre operation
            AddCommand("LDV " + Settings.RegisterPostions[0]);

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
