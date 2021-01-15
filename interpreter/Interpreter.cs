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
            ((Program)ast).Walk(globalScope);

            FuncCall mainCall = new FuncCall("main", new List<AST>());

            return mainCall.Walk(globalScope).Get<int>();
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

            throw new InvalidCastException();
        }

        public object GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances()
        {
            return value;
        }

        public override string ToString()
        {
            return "({0}, {1})".Format(type.ToString(), value);
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

    static class Extender
    {
        static void Raise(AST node, Scope scope, string error)
        {
            Console.WriteLine("Runtime Error: " + error);
            Console.WriteLine(scope.ToString());
            Environment.Exit(1);
        }
        public static RuntimeType Walk(this AST node, Scope scope)
        {
            throw new NotSupportedException(node.GetType().Name + " is not yet Implemented!");
        }

        public static RuntimeType Walk(this Literal node, Scope scope)
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
                    // BUG: chars can have more then one character BUT this should be checked in the lexer
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
                {Operator.Code.LT,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x < y) ? 1 : 0) },
                {Operator.Code.GT,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x > y) ? 1 : 0) },
                {Operator.Code.GEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x >= y) ? 1 : 0) },
                {Operator.Code.LEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x <= y) ? 1 : 0) },
                {Operator.Code.EQ,       (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x == y) ? 1 : 0) },
                {Operator.Code.NEQ,      (int x, int y) => new RuntimeType(RuntimeType.Type.Bool, (x != y) ? 1 : 0) },
            };
        public static RuntimeType Walk(this BinaryArithm node, Scope scope)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType leftValue = node.leftNode.Walk(scope);
            RuntimeType rightValue = node.rightNode.Walk(scope);

            return operatorToBinaryFunc[node.op](leftValue.Get<int>(), rightValue.Get<int>());
        }
        static Dictionary<Operator.Code, Func<int, int>> operatorToUnaryFunc = new Dictionary<Operator.Code, Func<int, int>>
            {
                {Operator.Code.PLUS,     (int x) => x },
                {Operator.Code.MINUS,    (int x) => -x },
            };
        public static RuntimeType Walk(this UnaryArithm node, Scope scope)
        {
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            RuntimeType value = node.node.Walk(scope);

            return new RuntimeType(operatorToUnaryFunc[node.op](value.Get<int>()));
        }
        public static RuntimeType Walk(this ast.Variable node, Scope scope)
        {
            VariableSignature variableSignature = new VariableSignature(node.identifier);

            // TODO this is an assumption, could also be a function
            Variable variable = scope.Translate(variableSignature) as Variable;
            return variable.value;
        }
        public static RuntimeType Walk(this VariableDecl node, Scope scope)
        {
            Variable variable = new Variable(RuntimeType.GetTypeFromString(node.type));
            VariableSignature variableSignature = new VariableSignature(node.identifier);
            scope.AddSymbol(variableSignature, variable);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this ArrayDecl node, Scope scope)
        {
            throw new NotImplementedException();
        }
        public static RuntimeType Walk(this VariableAssign node, Scope scope)
        {
            // TODO this is an assumption, could also be a function
            Variable variable = scope.Translate(new VariableSignature(node.identifier)) as Variable;
            variable.value = node.node.Walk(scope);
            return variable.value;
        }
        public static RuntimeType Walk(this FuncCall node, Scope scope)
        {
            FunctionSignature signature = new FunctionSignature(node.identifier, node.arguments.Count);

            // TODO this is an assumption, could also be a variable
            Function function = scope.Translate(signature) as Function;

            Scope copyScope = new Scope(scope);
            for (int i = 0; i < node.arguments.Count; i++)
            {
                var parameter = function.parameters[i];
                var argument = node.arguments[i];

                VariableSignature variableSignature = new VariableSignature(parameter.identifier);
                Variable variable = new Variable(argument.Walk(scope));

                Debug.Assert(variable.value.type == RuntimeType.GetTypeFromString(parameter.type),
                    "Type missmatch between: " + variable.value.type.ToString() + " and " + parameter.type);

                copyScope.AddSymbol(variableSignature, variable);
            }

            try
            {
                function.body.Walk(scope, copyScope);
            }
            catch (ReturnExc r)
            {
                return r.returnValue;
            }

            Debug.Assert(function.returnType == "void", "Missing return Statement");
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this FuncDecl node, Scope scope)
        {
            Function function = new Function(node.returnType);

            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(RuntimeType.GetTypeFromString(param.type), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            scope.AddSymbol(signature, function);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this FuncDef node, Scope scope)
        {
            List<FunctionParam> parameteres = new List<FunctionParam>();
            foreach (var param in node.parameters)
                parameteres.Add(new FunctionParam(RuntimeType.GetTypeFromString(param.type), param.identifier));

            FunctionSignature signature = new FunctionSignature(node.identifier, parameteres);

            // TODO this is an assumption, could also be a variable
            Function function = scope.Translate(signature) as Function;
            function.Define(node.block, node.parameters);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this ArrayAccess node, Scope scope)
        {
            throw new NotImplementedException();
        }
        public static RuntimeType Walk(this ArrayLiteral node, Scope scope)
        {
            throw new NotImplementedException();
        }
        public static RuntimeType Walk(this Statements node, Scope scope)
        {
            foreach (var statement in node.statements)
                statement.Walk(scope);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this BlockStatements node, Scope scope, Scope copyScope = null)
        {
            Scope blockScope = (copyScope != null) ? copyScope : scope;

            foreach (var statement in node.statements)
                statement.Walk(blockScope);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this Program node, Scope scope)
        {
            ((Statements)node).Walk(scope);
            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this For node, Scope scope)
        {
            Scope blockScope = new Scope(scope);

            node.initialization.Walk(blockScope);
            while (node.condition.Walk(blockScope).Get<bool>())
            {
                try
                {
                    node.body.Walk(blockScope);
                }
                catch (BreakExc)
                {
                    break;
                }
                catch (ContinueExc)
                {
                }
                node.loopExecution.Walk(blockScope);
            }

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this While node, Scope scope)
        {
            while (node.condition.Walk(scope).Get<bool>())
                try
                {
                    node.body.Walk(scope);
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
        public static RuntimeType Walk(this If node, Scope scope)
        {
            if (node.condition.Walk(scope).Get<bool>())
                node.ifBody.Walk(scope);
            else
                node.elseBody.Walk(scope);

            return RuntimeType.Void();
        }
        public static RuntimeType Walk(this Break node, Scope scope)
        {
            throw new BreakExc();
        }
        public static RuntimeType Walk(this Continue node, Scope scope)
        {
            throw new ContinueExc();
        }
        public static RuntimeType Walk(this Return node, Scope scope)
        {
            throw new ReturnExc(node.returnExpr.Walk(scope));
        }
        public static RuntimeType Walk(this Intrinsic node, Scope scope)
        {
            if (node.type == "printf")
            {
                if (node.parameters.Count == 0)
                    Raise(node, scope, "printf needs at least one parameter");
                //  TODO: implement proper printf
                List<object> parameters = new List<object>();
                foreach (var param in node.parameters)
                    parameters.Add(param.Walk(scope).GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances());

                // TODO this wont work with the passed parameters, C# doesnt have parameter unpacking
                string ouput = parameters[0].ToString().Format(parameters);
                Console.WriteLine("printf: \"" + ouput + "\"");
            }
            throw new NotImplementedException();
        }
    }
}
