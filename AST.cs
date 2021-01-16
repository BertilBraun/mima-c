using mima_c.interpreter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace mima_c
{
    using ASTList = List<dynamic>;

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
                        result += string.Join("\n\t|", ("\n->: " + child.ToString().TrimStart()).Split('\n'));

                return result;
            }
        }

        class Literal : AST
        {
            public static Dictionary<TokenType, Type> tokenToType = new Dictionary<TokenType, Type>
            {
                {TokenType.INTLITERAL, Type.INTLITERAL },
                {TokenType.STRINGLITERAL, Type.STRINGLITERAL },
                {TokenType.CHARLITERAL, Type.CHARLITERAL },
            };
            public enum Type
            {
                INTLITERAL,
                STRINGLITERAL,
                CHARLITERAL,
                // FLOATLITERAL,
                // DOUBLELITERAL,
            }

            public Type type  { get; }
            public string value  { get; }

            public Literal(Type type, string value)
            {
                this.type = type;
                this.value = value;
            }

            protected override object _value => (type, value);
        }

        abstract class Operator
        {
            static Dictionary<TokenType, Code> tokenToOperator = new Dictionary<TokenType, Code>
            {
                {TokenType.PLUS,     Code.PLUS        },
                {TokenType.MINUS,    Code.MINUS         },
                {TokenType.DIVIDE,   Code.DIVIDE          },
                {TokenType.MULTIPLY, Code.MULTIPLY            },
                {TokenType.MODULO,   Code.MODULO          },
                {TokenType.LT,       Code.LT      },
                {TokenType.GT,       Code.GT      },
                {TokenType.GEQ,      Code.GEQ       },
                {TokenType.LEQ,      Code.LEQ       },
                {TokenType.EQ,       Code.EQ      },
                {TokenType.NEQ,      Code.NEQ       },
            };

            public static Code Parse(TokenType type)
            {
                if (!tokenToOperator.ContainsKey(type))
                    throw new ArgumentException("Tried to convert invalid Type to Operator Code: " + type.ToString());
                return tokenToOperator[type];
            }

            public enum Code
            {
                PLUS,
                MINUS,
                DIVIDE,
                MULTIPLY,
                MODULO,
                LT,
                GT,
                GEQ,
                LEQ,
                EQ,
                NEQ,
            }
        }

        class BinaryArithm : AST
        {
            public dynamic leftNode  { get; }
            public dynamic rightNode  { get; }
            public Operator.Code op  { get; }

            public BinaryArithm(Operator.Code op, AST leftNode, AST rightNode)
            {
                this.op = op;
                this.leftNode = leftNode;
                this.rightNode = rightNode;
            }

            protected override ASTList _children => new ASTList { leftNode, rightNode };
            protected override string _nodeName => op.ToString();
        }

        class UnaryArithm : AST
        {
            public dynamic node  { get; }
            public Operator.Code op  { get; }

            public UnaryArithm(Operator.Code op, AST node)
            {
                Debug.Assert(op.In(Operator.Code.MINUS)); // TODO For Future, op.In("-", "+", "*", "&")
                this.op = op;
                this.node = node;
            }

            protected override object _value => op;
            protected override ASTList _children => new ASTList { node };
            protected override string _nodeName => op.ToString();
        }

        class Variable : AST
        {
            public string identifier  { get; }

            public Variable(string identifier)
            {
                this.identifier = identifier;
            }

            protected override object _value => identifier;
        }

        class VariableDecl : AST
        {
            public string type { get; }
            public string identifier  { get; }

            public VariableDecl(string type, string identifier)
            {
                this.type = type;
                this.identifier = identifier;
            }

            protected override object _value => identifier;
        }

        class ArrayDecl : VariableDecl
        {
            public dynamic countExpr  { get; }

            public ArrayDecl(string type, string identifier, AST countExpr) : base(type, identifier)
            {
                this.countExpr = countExpr;
            }

            protected override ASTList _children => new ASTList { countExpr };
        }

        class VariableAssign : AST
        {
            public string identifier  { get; }
            public dynamic node  { get; }

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
            public string identifier  { get; }
            public ASTList arguments  { get; }

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

                public override string ToString()
                {
                    return "{0} {1}".Format(type, identifier);
                }
            }

            public string returnType  { get; }
            public string identifier  { get; }
            public List<Parameter> parameters  { get; }

            public FuncDecl(string returnType, string identifier, List<Parameter> parameters)
            {
                this.returnType = returnType;
                this.identifier = identifier;
                this.parameters = parameters;
            }

            protected override object _value => string.Format("{0} {1}({2})", returnType, identifier, parameters.FormatList());
        }

        class FuncDef : FuncDecl
        {
            public BlockStatements block  { get; }

            public FuncDef(string returnType, string identifier, List<Parameter> parameters, BlockStatements block)
                : base(returnType, identifier, parameters)
            {
                this.block = block;
            }

            protected override ASTList _children => new ASTList { block };
        }

        class ArrayAccess : AST
        {
            public string identifier  { get; }
            public dynamic indexExpr  { get; }

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
            public ASTList valueListExprs  { get; }

            public ArrayLiteral(ASTList valueListExprs)
            {
                this.valueListExprs = valueListExprs;
            }

            protected override ASTList _children => valueListExprs;
        }

        class Statements : AST
        {
            public ASTList statements { get; }

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
            public dynamic initialization  { get; }
            public dynamic condition  { get; }
            public dynamic loopExecution  { get; }
            public dynamic body  { get; }

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
            public dynamic condition  { get; }
            public dynamic body  { get; }

            public While(AST condition, AST body)
            {
                this.condition = condition;
                this.body = body;
            }

            protected override ASTList _children => new ASTList { condition, body };
        }

        class If : AST
        {
            public dynamic condition  { get; }
            public dynamic ifBody  { get; }
            public dynamic elseBody  { get; }

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
            public dynamic returnExpr { get; }

            public Return(AST returnExpr)
            {
                this.returnExpr = returnExpr;
            }

            protected override ASTList _children => new ASTList { returnExpr };
        }

        class Intrinsic : AST
        {
            public ASTList parameters { get; }
            public string type { get; }

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
