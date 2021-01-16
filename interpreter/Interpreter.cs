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
                    Debug.Assert(false, "Not implemented Type of Literal: " + node.type.ToString());
                    return null;
            }
        }

        static Dictionary<Operator.Code, Func<int, int, RuntimeType>> operatorToBinaryFunc = new Dictionary<Operator.Code, Func<int, int, RuntimeType>>
            {
                {Operator.Code.PLUS,     (int x, int y) => new RuntimeType(x + y) },
                {Operator.Code.MINUS,    (int x, int y) => new RuntimeType(x - y) },
                {Operator.Code.DIVIDE,   (int x, int y) => new RuntimeType(x / y) },
                {Operator.Code.MULTIPLY, (int x, int y) => new RuntimeType(x * y) },
                {Operator.Code.MODULO,   (int x, int y) => new RuntimeType(x % y) },
                {Operator.Code.LT,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x < y)) },
                {Operator.Code.GT,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x > y)) },
                {Operator.Code.GEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x >= y)) },
                {Operator.Code.LEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x <= y)) },
                {Operator.Code.EQ,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x == y)) },
                {Operator.Code.NEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x != y)) },
            };
        public static RuntimeType Walk(BinaryArithm node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType leftValue = Walk(node.leftNode, scope);
            RuntimeType rightValue = Walk(node.rightNode, scope);

            return operatorToBinaryFunc[node.op](leftValue.Get<int>(), rightValue.Get<int>());
        }
        static Dictionary<Operator.Code, Func<int, int>> operatorToUnaryFunc = new Dictionary<Operator.Code, Func<int, int>>
            {
                {Operator.Code.PLUS,     (int x) => x },
                {Operator.Code.MINUS,    (int x) => -x },
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
            Variable variable = scope.Translate(variableSignature) as Variable;
            return variable.value;
        }
        public static RuntimeType Walk(VariableDecl node, Scope scope)
        {
            Variable variable = new Variable(RuntimeType.GetTypeFromString(node.type));
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(VariableAssign node, Scope scope)
        {
            // TODO is an assumption, could also be a function
            Variable variable = scope.Translate(new VariableSignature(node.identifier)) as Variable;
            variable.value = Walk(node.node, scope);
            return variable.value;
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

                Debug.Assert(variable.value.type == RuntimeType.GetTypeFromString(parameter.type),
                    "Type missmatch between: " + variable.value.type.ToString() + " and " + parameter.type);

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

            Debug.Assert(function.returnType == "void", "Missing return Statement");
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(FuncDecl node, Scope scope)
        {
            Function function = new Function(node.returnType);

            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(RuntimeType.GetTypeFromString(param.type), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            scope.AddSymbol(signature, function);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(FuncDef node, Scope scope)
        {
            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(RuntimeType.GetTypeFromString(param.type), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            // TODO is an assumption, could also be a variable
            Function function = scope.Translate(signature) as Function;
            function.Define(node.block, node.parameters);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(Statements node, Scope scope)
        {
            foreach (var statement in node.statements)
                Walk(statement, scope);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(BlockStatements node, Scope scope, Scope copyScope = null)
        {
            Scope blockScope = (copyScope != null) ? copyScope : scope;

            foreach (var statement in node.statements)
                Walk(statement, blockScope);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(Program node, Scope scope)
        {
            Walk((Statements)node, scope);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(For node, Scope scope)
        {
            Scope blockScope = new Scope(scope);

            Walk(node.initialization, blockScope);
            while (Walk(node.condition, blockScope).Get<bool>())
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

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(While node, Scope scope)
        {
            while (Walk(node.condition, scope).Get<bool>())
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

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(If node, Scope scope)
        {
            if (Walk(node.condition, scope).Get<bool>())
                Walk(node.ifBody, scope);
            else
                Walk(node.elseBody, scope);

            return RuntimeType.Void();
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
            throw new ReturnExc(Walk(node.returnExpr, scope));
        }
        public static RuntimeType Walk(Intrinsic node, Scope scope)
        {
            if (node.type == "printf")
            {
                if (node.parameters.Count == 0)
                    Raise(node, scope, "printf needs at least one parameter");
                //  TODO: implement proper printf
                List<object> parameters = new List<object>();
                foreach (var param in node.parameters)
                    parameters.Add(Walk(param, scope).GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances());

                string formatString = parameters[0].ToString();
                parameters.RemoveAt(0);

                string ouput = formatString.Format(parameters.ToArray());
                Console.WriteLine("printf: \"" + ouput + "\"");
            }
            return RuntimeType.Void();
        }

        public static RuntimeType Walk(ArrayDecl node, Scope scope)
        {
            throw new NotImplementedException();
        }
        public static RuntimeType Walk(ArrayAccess node, Scope scope)
        {
            throw new NotImplementedException();
        }
        public static RuntimeType Walk(ArrayLiteral node, Scope scope)
        {
            throw new NotImplementedException();
        }
    }

    class RuntimeType
    {
        public enum Type
        {
            Int,
            String,
            Bool,
            Char,
            Float,
            Double,
            Void,
            Function,
            Custom
        }

        public Type type { get; }
        private object value;

        public RuntimeType(int value)
        {
            this.type = Type.Int;
            this.value = value;
        }

        public RuntimeType(Type type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public T Get<T>()
        {
            if (type == Type.Int && typeof(T) == typeof(int))
                return (T)value;

            if (type == Type.Bool && typeof(T) == typeof(bool))
                return (T)value;

            throw new InvalidCastException();
        }

        public object GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances()
        {
            return value;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", type.ToString(), value);
        }

        public static Type GetType<T>()
        {
            return GetTypeFromString(typeof(T).Name);
        }
        public static Type GetTypeFromString(string s)
        {
            if (s == "int")
                return Type.Int;
            if (s == "void")
                return Type.Void;

            return Type.Custom;
        }
        public static RuntimeType Void()
        {
            return new RuntimeType(Type.Void, null);
        }

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
