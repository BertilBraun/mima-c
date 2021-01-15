using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace mima_c
{
    using ASTList = List<ast.AST>;

    namespace ast
    {
        abstract class AST
        {
            protected virtual object _value { get; }
            protected virtual ASTList _children { get; }
            protected virtual string _nodeName => GetType().Name;

            public override string ToString()
            {
                string result;
                if (_value != null)
                    result = string.Format("[{0}] ({1}): ", _nodeName, _value);
                else
                    result = string.Format("[{0}]: ", _nodeName);

                if (_children != null)
                    foreach (var child in _children)
                        result += string.Join("\t|", ("\n->: " + child.ToString().TrimStart()).Split('\n'));

                return result;
            }
        }

        class Literal : AST
        {
            public enum Type
            {
                INTLITERAL,
                STRINGLITERAL,
                CHARLITERAL,
                // FLOATLITERAL,
                // DOUBLELITERAL,
            }

            Type type;
            string value;

            public Literal(Type type, string value)
            {
                this.type = type;
                this.value = value;
            }

            protected override object _value => (type, value);
        }

        class BinaryArithm : AST
        {
            AST leftNode;
            AST rightNode;
            string op;

            public BinaryArithm(string op, AST leftNode, AST rightNode)
            {
                Debug.Assert(op.In("+", "-", "/", "*", "%", "<", ">", "<=", ">=", "==", "!="));
                this.op = op;
                this.leftNode = leftNode;
                this.rightNode = rightNode;
            }

            protected override object _value => op;
            protected override ASTList _children => new ASTList { leftNode, rightNode };
            protected override string _nodeName => op;
        }

        class UnaryArithm : AST
        {
            AST node;
            string op;

            public UnaryArithm(string op, AST node)
            {
                Debug.Assert(op.In("-")); // TODO For Future, op.In("-", "+", "*", "&")
                this.op = op;
                this.node = node;
            }

            protected override object _value => op;
            protected override ASTList _children => new ASTList { node };
            protected override string _nodeName => op;
        }

        class Variable : AST
        {
            string identifier;

            public Variable(string identifier)
            {
                this.identifier = identifier;
            }

            protected override object _value => identifier;
        }

        class VariableDecl : AST
        {
            string type;
            string identifier;

            public VariableDecl(string type, string identifier)
            {
                this.type = type;
                this.identifier = identifier;
            }

            protected override object _value => identifier;
        }

        class ArrayDecl : VariableDecl
        {
            AST countExpr;

            public ArrayDecl(string type, string identifier, AST countExpr) : base(type, identifier)
            {
                this.countExpr = countExpr;
            }

            protected override ASTList _children => new ASTList { countExpr };
        }

        class VariableAssign : AST
        {
            string identifier;
            AST node;

            public VariableAssign(string identifier, AST node)
            {
                this.identifier = identifier;
                this.node = node;
            }

            protected override object _value => identifier;
            protected override ASTList _children => new ASTList { node };
        }

        class FuncCall : AST
        {
            string identifier;
            ASTList arguments;

            public FuncCall(string identifier, ASTList arguments)
            {
                this.identifier = identifier;
                this.arguments = arguments;
            }

            protected override object _value => identifier;
            protected override ASTList _children => arguments;
        }

        class FuncDecl : AST
        {
            public struct Parameter
            {
                public string type { get; }
                public string identifier { get; }

                public Parameter(string type, string identifier)
                {
                    this.type = type;
                    this.identifier = identifier;
                }
            }

            string returnType;
            string identifier;
            List<Parameter> parameters;

            public FuncDecl(string returnType, string identifier, List<Parameter> parameters)
            {
                this.returnType = returnType;
                this.identifier = identifier;
                this.parameters = parameters;
            }

            protected override object _value => (returnType, identifier, parameters);
        }

        class FuncDef : FuncDecl
        {
            AST block;

            public FuncDef(string returnType, string identifier, List<Parameter> parameters, AST block)
                : base(returnType, identifier, parameters)
            {
                this.block = block;
            }

            protected override ASTList _children => new ASTList { block };
        }

        class ArrayAccess : AST
        {
            string identifier;
            AST indexExpr;

            public ArrayAccess(string identifier, AST indexExpr)
            {
                this.identifier = identifier;
                this.indexExpr = indexExpr;
            }

            protected override object _value => identifier;
            protected override ASTList _children => new ASTList { indexExpr };
        }

        class ArrayLiteral : AST
        {
            ASTList valueListExprs;

            public ArrayLiteral(ASTList valueListExprs)
            {
                this.valueListExprs = valueListExprs;
            }

            protected override ASTList _children => valueListExprs;
        }

        class Statements : AST
        {
            ASTList statements;

            public Statements(ASTList statements)
            {
                this.statements = statements;
            }

            protected override ASTList _children => statements;
        }

        class BlockStatements : Statements
        {
            public BlockStatements(ASTList statements) : base(statements)
            {
            }
        }

        class Program : Statements
        {
            public Program(ASTList statements) : base(statements)
            {
            }
        }

        class For : AST
        {
            AST initialization;
            AST condition;
            AST loopExecution;
            AST body;

            public For(AST initialization, AST condition, AST loopExecution, AST body)
            {
                this.initialization = initialization;
                this.condition = condition;
                this.loopExecution = loopExecution;
                this.body = body;
            }

            protected override ASTList _children => new ASTList { initialization, condition, loopExecution, body };
        }

        class While : AST
        {
            AST condition;
            AST body;

            public While(AST condition, AST body)
            {
                this.condition = condition;
                this.body = body;
            }

            protected override ASTList _children => new ASTList { condition, body };
        }

        class If : AST
        {
            AST condition;
            AST ifBody;
            AST elseBody;

            public If(AST condition, AST ifBody, AST elseBody)
            {
                this.condition = condition;
                this.ifBody = ifBody;
                this.elseBody = elseBody;
            }

            protected override ASTList _children => new ASTList { condition, ifBody, elseBody };
        }

        class Break : AST
        {

        }

        class Continue : AST
        {

        }

        class Return : AST
        {
            AST returnExpr;

            public Return(AST returnExpr)
            {
                this.returnExpr = returnExpr;
            }

            protected override ASTList _children => new ASTList { returnExpr };
        }

        class Intrinsic : AST
        {
            ASTList parameters;
            string type;

            public Intrinsic(ASTList parameters, string type)
            {
                this.parameters = parameters;
                this.type = type;
            }

            protected override object _value => type;
            protected override ASTList _children => parameters;
        }
    }
}
