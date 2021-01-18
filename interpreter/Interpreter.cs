using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace mima_c.interpreter
{
    class Interpreter
    {
        public Interpreter(AST ast)
        {
            this.ast = ast;
            this.globalScope = new Scope(null);
        }

        AST ast { get; }
        Scope globalScope;

        internal int Interpret()
        {
            // First Node must always be a Program Node
            Walk((Program)ast, globalScope);

            FuncCall mainCall = new FuncCall("main", new List<dynamic>());

            return Walk(mainCall, globalScope).Get<int>();
        }

        static void Raise(AST node, Scope scope, string error)
        {
            Console.WriteLine("Runtime Error: " + error);
            Console.WriteLine(scope.ToString());
            Environment.Exit(1);
        }
        public static RuntimeType Walk(AST node, Scope scope)
        {
            throw new NotSupportedException(node.GetType().Name + " is not yet Implemented!");
        }

        public static RuntimeType Walk(Literal node, Scope scope)
        {
            // No need for explicit error handling. If they can't be converted
            // it's a compiler bug
            switch (node.type)
            {
                case Literal.Type.INTLITERAL:
                    return new RuntimeType(int.Parse(node.value));
                case Literal.Type.STRINGLITERAL:
                    return new RuntimeType(RuntimeType.Type.String, node.value.Escape());
                case Literal.Type.CHARLITERAL:
                    // BUG: chars can have more then one character BUT should be checked in the lexer
                    return new RuntimeType(RuntimeType.Type.Char, node.value.Escape());
                default:
                    throw new NotImplementedException("Not implemented Type of Literal: " + node.type.ToString());
            }
        }

        static Dictionary<Operator.Code, Func<int, int, RuntimeType>> operatorToBinaryFunc = new Dictionary<Operator.Code, Func<int, int, RuntimeType>>
            {
                {Operator.Code.PLUS,     (int x, int y) => new RuntimeType(x + y) },
                {Operator.Code.MINUS,    (int x, int y) => new RuntimeType(x - y) },
                {Operator.Code.DIVIDE,   (int x, int y) => new RuntimeType(x / y) },
                {Operator.Code.MULTIPLY, (int x, int y) => new RuntimeType(x * y) },
                {Operator.Code.MODULO,   (int x, int y) => new RuntimeType(x % y) },
                {Operator.Code.LT,       (int x, int y) => new RuntimeType((x < y) ? 1 : 0) },
                {Operator.Code.GT,       (int x, int y) => new RuntimeType((x > y) ? 1 : 0) },
                {Operator.Code.GEQ,      (int x, int y) => new RuntimeType((x >= y) ? 1 : 0) },
                {Operator.Code.LEQ,      (int x, int y) => new RuntimeType((x <= y) ? 1 : 0) },
                {Operator.Code.EQ,       (int x, int y) => new RuntimeType((x == y) ? 1 : 0) },
                {Operator.Code.NEQ,      (int x, int y) => new RuntimeType((x != y) ? 1 : 0) },

                {Operator.Code.AND,      (int x, int y) => new RuntimeType((x != 0 && y != 0) ? 1 : 0) },
                {Operator.Code.OR,       (int x, int y) => new RuntimeType((x != 0 || y != 0) ? 1 : 0) },
                // TODO string, char with these operators
                // {Operator.Code.EQ,       (int x, int y) => new RuntimeType((x == y) ? 1 : 0) },
                // {Operator.Code.NEQ,      (int x, int y) => new RuntimeType((x != y) ? 1 : 0) },
            };
        public static RuntimeType Walk(BinaryArithm node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType leftValue = Walk(node.leftNode, scope);
            RuntimeType rightValue = Walk(node.rightNode, scope);

            return operatorToBinaryFunc[node.op](leftValue.Get<int>(), rightValue.Get<int>());
        }
        static Dictionary<TokenType, Func<int, int>> operatorToUnaryFunc = new Dictionary<TokenType, Func<int, int>>
            {
                {TokenType.PLUS,          (int x) => x },
                {TokenType.MINUS,         (int x) => -x },

                {TokenType.NOT,           (int x) => (x == 0) ? 1 : 0 },
                {TokenType.LNOT,          (int x) => ~x },

                {TokenType.PLUSPLUS,      (int x) => x + 1},
                {TokenType.MINUSMINUS,    (int x) => x - 1 },
            };
        public static RuntimeType Walk(UnaryArithm node, Scope scope)
        {
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType value = Walk(node.node, scope);

            return new RuntimeType(operatorToUnaryFunc[node.op](value.Get<int>()));
        }
        public static RuntimeType Walk(ast.Variable node, Scope scope)
        {
            VariableSignature variableSignature = new VariableSignature(node.identifier);

            // TODO is an assumption, could also be a function
            RuntimeType variable = scope.Translate(variableSignature);
            if (variable.type == RuntimeType.Type.Function)
                throw new ArgumentException("Signature is of type Function: " + node.identifier);

            return variable;
        }
        public static RuntimeType Walk(VariableDecl node, Scope scope)
        {
            Variable variable = new Variable(RuntimeType.GetTypeFromString(node.type));
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(VariableAssign node, Scope scope)
        {
            RuntimeType val = Walk(node.identifier, scope);
            RuntimeType value = Walk(node.node, scope);
            val.Set(value);

            return value;
        }
        public static RuntimeType Walk(FuncCall node, Scope scope)
        {
            FunctionSignature signature = new FunctionSignature(node.identifier, node.arguments.Count);

            // TODO is an assumption, could also be a variable
            Function function = scope.Translate(signature) as Function;

            Scope copyScope = new Scope(scope);
            for (int i = 0; i < node.arguments.Count; i++)
            {
                var parameter = function.parameters[i];
                var argument = node.arguments[i];

                VariableSignature variableSignature = new VariableSignature(parameter.identifier);
                Variable variable = new Variable(Walk(argument, scope));

                if (variable.type != RuntimeType.GetTypeFromString(parameter.type))
                    throw new InvalidCastException("Type missmatch between: " + variable.type.ToString() + " and " + parameter.type);

                copyScope.AddSymbol(variableSignature, variable);
            }

            try
            {
                Walk(function.body, scope, copyScope);
            }
            catch (ReturnExc r)
            {
                return r.returnValue;
            }

            if (function.returnType != RuntimeType.Type.Void)
                throw new InvalidOperationException("Missing return Statement");
            return RuntimeType.Void;
        }
        public static RuntimeType Walk(FuncDecl node, Scope scope)
        {
            Function function = new Function(node.returnType.GetRuntimeType());

            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(param.type.GetRuntimeType(), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            scope.AddSymbol(signature, function);
            return RuntimeType.Void;
        }
        public static RuntimeType Walk(FuncDef node, Scope scope)
        {
            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(param.type.GetRuntimeType(), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            // TODO is an assumption, could also be a variable
            Function function = scope.Translate(signature) as Function;
            function.Define(node.block, node.parameters);
            return RuntimeType.Void;
        }
        public static RuntimeType Walk(Statements node, Scope scope)
        {
            foreach (var statement in node.statements)
                Walk(statement, scope);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(BlockStatements node, Scope scope, Scope copyScope = null)
        {
            Scope blockScope = (copyScope != null) ? copyScope : scope;

            foreach (var statement in node.statements)
                Walk(statement, blockScope);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(Program node, Scope scope)
        {
            Walk((Statements)node, scope);
            return RuntimeType.Void;
        }
        public static RuntimeType Walk(For node, Scope scope)
        {
            Scope blockScope = new Scope(scope);

            Walk(node.initialization, blockScope);
            while (Walk(node.condition, blockScope).Get<int>() != 0)
            {
                try
                {
                    Scope blockBlockScope = new Scope(blockScope);
                    Walk(node.body, blockBlockScope);
                }
                catch (BreakExc)
                {
                    break;
                }
                catch (ContinueExc)
                {
                }
                Walk(node.loopExecution, blockScope);
            }

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(While node, Scope scope)
        {
            while (Walk(node.condition, scope).Get<int>() != 0)
                try
                {
                    Walk(node.body, scope);
                }
                catch (BreakExc)
                {
                    break;
                }
                catch (ContinueExc)
                {
                }

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(If node, Scope scope)
        {
            if (Walk(node.condition, scope).Get<int>() != 0)
                Walk(node.ifBody, scope);
            else
                Walk(node.elseBody, scope);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(Break node, Scope scope)
        {
            throw new BreakExc();
        }
        public static RuntimeType Walk(Continue node, Scope scope)
        {
            throw new ContinueExc();
        }
        public static RuntimeType Walk(Return node, Scope scope)
        {
            RuntimeType ret = Walk(node.returnExpr, scope);
            ret.MakeUnAssignable();
            throw new ReturnExc(ret);
        }
        public static RuntimeType Walk(Intrinsic node, Scope scope)
        {
            if (node.type == "printf")
            {
                if (node.parameters.Count == 0)
                    Raise(node, scope, "printf needs at least one parameter");
                //  TODO: implement proper printf
                List<dynamic> parameters = new List<dynamic>(node.parameters.Count);
                foreach (var param in node.parameters)
                    parameters.Add(Walk(param, scope).GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances());

                string formatString = parameters[0].ToString();
                parameters.RemoveAt(0);

                string ouput = formatString.Format(parameters.ToArray());
                Console.WriteLine("printf: \"" + ouput + "\"");
            }
            return RuntimeType.Void;
        }

        public static RuntimeType Walk(ArrayDecl node, Scope scope)
        {
            int size = 0;
            if (node.countExpr != null)
                size = Walk(node.countExpr, scope).Get<int>();

            Array variable = new Array(RuntimeType.GetTypeFromString(node.type), size);
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(ArrayAccess node, Scope scope)
        {
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            Array variable = scope.Translate(variableSignature) as Array;

            if (variable == null)
                throw new TypeAccessException("Variable was not of type Array: " + node.identifier);

            // TODO is an assumption, could also be a function
            int index = Walk(node.indexExpr, scope).Get<int>();

            if (index < 0 || index >= variable.Values.Length)
                throw new IndexOutOfRangeException("Array index out of Range: " + index);

            return variable.Values[index];
        }
        public static RuntimeType Walk(ArrayLiteral node, Scope scope)
        {
            List<RuntimeType> values = new List<RuntimeType>();
            foreach (var expr in node.valueListExprs)
                values.Add(Walk(expr, scope));

            return new Array(values.ToArray());
        }
        
        static Dictionary<TokenType, Func<int, int, RuntimeType>> operatorToOperationAssign = new Dictionary<TokenType, Func<int, int, RuntimeType>>
            {
                {TokenType.PLUSEQ,     (int x, int y) => new RuntimeType(x + y) },
                {TokenType.MINUSEQ,    (int x, int y) => new RuntimeType(x - y) },
                {TokenType.DIVIDEEQ,   (int x, int y) => new RuntimeType(x / y) },
                {TokenType.STAREQ,     (int x, int y) => new RuntimeType(x * y) },
                {TokenType.MODULOEQ,   (int x, int y) => new RuntimeType(x % y) },
            };
        public static RuntimeType Walk(OperationAssign node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType leftValue = Walk(node.identifier, scope);
            RuntimeType rightValue = Walk(node.node, scope);

            RuntimeType value = operatorToOperationAssign[node.op](leftValue.Get<int>(), rightValue.Get<int>());
            leftValue.Set(value);
            return value;
        }

        public static RuntimeType Walk(Ternary node, Scope scope)
        {
            if (Walk(node.condition, scope).Get<int>() != 0)
                return Walk(node.ifBlock, scope);
            else
                return Walk(node.elseBlock, scope);
        }

        public static RuntimeType Walk(PointerDecl node, Scope scope)
        {
            Pointer variable = new Pointer(Walk(node.decl, scope));
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void;
        }

        public static RuntimeType Walk(PointerAccess node, Scope scope)
        {
            RuntimeType pointer = Walk(node.node, scope);
            return pointer.Get<RuntimeType>();
        }

        public static RuntimeType Walk(PointerLiteral node, Scope scope)
        {
            return new Pointer(Walk(node.node, scope));
        }

        static Dictionary<TokenType, Func<int, RuntimeType>> operatorToPostfixArithm = new Dictionary<TokenType, Func<int, RuntimeType>>
            {
                {TokenType.PLUSPLUS,       (int x) => new RuntimeType(x + 1) },
                {TokenType.MINUSMINUS,     (int x) => new RuntimeType(x - 1) },
            };
        public static RuntimeType Walk(PostfixArithm node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType currentValue = Walk(node.node, scope);
            RuntimeType returnValue = new RuntimeType(currentValue.Get<int>());
            
            currentValue.Set(operatorToPostfixArithm[node.operation](currentValue.Get<int>()));
            return returnValue;
        }

        public static RuntimeType Walk(Typedef node, Scope scope)
        {
            return RuntimeType.Void;
        }

        public static RuntimeType Walk(Cast node, Scope scope)
        {
            throw new NotImplementedException();
        }

        public static RuntimeType Walk(StructDecl node, Scope scope)
        {
            return RuntimeType.Void;
            throw new NotImplementedException();
        }

        public static RuntimeType Walk(StructAccess node, Scope scope)
        {
            throw new NotImplementedException();
        }
        
            
        class BreakExc : Exception
        {
        }

        class ContinueExc : Exception
        {
        }

        class ReturnExc : Exception
        {
            public RuntimeType returnValue { get; }

            public ReturnExc(RuntimeType returnValue)
            {
                this.returnValue = returnValue;
            }
        }
    }
}
