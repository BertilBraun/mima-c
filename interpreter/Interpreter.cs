using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mima_c.interpreter
{
    class Interpreter
    {
        public Interpreter(AST ast)
        {
            this.ast = ast;
            this.globalScope = new Scope(null);
            customTypes = new Dictionary<string, List<dynamic>>();
        }

        AST ast { get; }
        Scope globalScope;

        static Dictionary<string, List<dynamic>> customTypes;

        internal int Interpret()
        {
            // First Node must always be a Program Node
            Walk((Program)ast, globalScope);

            FuncCall mainCall = new FuncCall("main", new List<dynamic>());

            try
            {
                return Walk(mainCall, globalScope).Get<int>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
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

        static Dictionary<TokenType, Func<int, int, RuntimeType>> operatorToBinaryFunc = new Dictionary<TokenType, Func<int, int, RuntimeType>>
            {
                {TokenType.PLUS,     (int x, int y) => new RuntimeType(x + y) },
                {TokenType.MINUS,    (int x, int y) => new RuntimeType(x - y) },
                {TokenType.DIVIDE,   (int x, int y) => new RuntimeType(x / y) },
                {TokenType.STAR,     (int x, int y) => new RuntimeType(x * y) },
                {TokenType.MODULO,   (int x, int y) => new RuntimeType(x % y) },
                {TokenType.LT,       (int x, int y) => new RuntimeType((x < y) ? 1 : 0) },
                {TokenType.GT,       (int x, int y) => new RuntimeType((x > y) ? 1 : 0) },
                {TokenType.GEQ,      (int x, int y) => new RuntimeType((x >= y) ? 1 : 0) },
                {TokenType.LEQ,      (int x, int y) => new RuntimeType((x <= y) ? 1 : 0) },
                {TokenType.EQ,       (int x, int y) => new RuntimeType((x == y) ? 1 : 0) },
                {TokenType.NEQ,      (int x, int y) => new RuntimeType((x != y) ? 1 : 0) },

                {TokenType.AND,      (int x, int y) => new RuntimeType((x != 0 && y != 0) ? 1 : 0) },
                {TokenType.OR,       (int x, int y) => new RuntimeType((x != 0 || y != 0) ? 1 : 0) },
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

            return operatorToBinaryFunc[node.operation](leftValue.Get<int>(), rightValue.Get<int>());
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
            var type = RuntimeType.GetTypeFromString(node.type);
            if (type == RuntimeType.Type.Struct)
            {
                Scope structValues = new Scope(null);
                foreach (var decl in customTypes[node.type])
                    Walk(decl, structValues);

                Struct variable = new Struct(structValues);
                VariableSignature variableSignature = new VariableSignature(node.identifier);
                scope.AddSymbol(variableSignature, variable);
                // structValues.MakeScopeToStructVariables(node.identifier);
                // scope.Add(structValues);
            }
            else
            {
                Variable variable = new Variable(type);
                VariableSignature variableSignature = new VariableSignature(node.identifier);
                scope.AddSymbol(variableSignature, variable);
            }

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
                return Walk(node.ifBody, scope);
            else
                return Walk(node.elseBody, scope);
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
            if (customTypes.ContainsKey(node.typeName))
                customTypes[node.alias] = customTypes[node.typeName];
            return RuntimeType.Void;
        }
        public static RuntimeType Walk(Cast node, Scope scope)
        {
            RuntimeType variable = Walk(node.node, scope);
            variable.SetType(RuntimeType.GetTypeFromString(node.castType));
            return variable;
        }
        public static RuntimeType Walk(StructDef node, Scope scope)
        {
            customTypes[node.typeName] = node.program.statements;

            // StructValues now contains all defined variables and default values

            //Struct variable = new Pointer(Walk(node.decl, scope));
            //VariableSignature variableSignature = new VariableSignature(node.identifier);
            //scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void;
        }
        public static RuntimeType Walk(StructAccess node, Scope scope)
        {
            if (node.operation == TokenType.DOT)
            {
                Struct variable = Walk(node.variable, scope) as Struct;
                var data = variable.variables.Where((v) => v.Item1 == node.field).First();
                if (data.Item2 == null)
                    throw new AccessViolationException("Field of name: " + node.field + " does not exist!");
                return data.Item2;
            }
            else if (node.operation == TokenType.ARROW)
            {
                throw new NotImplementedException();
            }
            else
                throw new InvalidOperationException("Operation " + node.operation.ToString() + " for Struct Access not defined!");
        }

        public static RuntimeType Walk(NoOp node, Scope scope)
        {
            return RuntimeType.Void;
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
