using mima_c.ast;
using System;
using System.Collections.Generic;
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

        internal int interpret()
        {
            // First Node must always be a Program Node
            ((Program)ast).walk(globalScope);

            FuncCall mainCall = new FuncCall("main", new List<AST>());

            return (int)mainCall.walk(globalScope);
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
        public object returnValue { get; }

        public ReturnExc(object returnValue)
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

        /*
class Literal
class BinaryArithm
class UnaryArithm
class Variable
class VariableDecl
class ArrayDecl
class VariableAssign
class FuncCall
class FuncDecl
class FuncDef
class ArrayAccess
class ArrayLiteral
class Statements
class BlockStatements
class Program
class For
class While
class If
class Break
class Continue
class Return
class Intrinsic
        */
        public static object walk(this AST node, Scope scope)
        {
            throw new NotSupportedException(node.GetType().Name + " is not yet Implemented!");
        }
        public static object walk(this Literal node, Scope scope)
        {
            return 42;
        }
        public static object walk(this Program node, Scope scope)
        {
            return 42;
        }
        public static object walk(this FuncCall node, Scope scope)
        {
            return 42;
        }
    }
}
