using System;
using System.Collections.Generic;

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

            public string Representation()
            {
                if (_value != null)
                    return "[{0}] ({1}): ".Format(_nodeName, _value);
                else
                    return "[{0}]: ".Format(_nodeName);
            }
            public override string ToString()
            {
                string result = Representation();

                if (_children != null)
                    foreach (var child in _children)
                        result += string.Join("\n\t|", ("\n->: " + child.ToString().TrimStart()).Split('\n'));

                return result;
            }
        }

        class NoOp : AST
        {
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

            public Type type;
            public string value;

            public Literal(Type type, string value)
            {
                this.type = type;
                this.value = value;
            }

            protected override object _value => (type, value);
        }

        class BinaryArithm : AST
        {
            public dynamic leftNode;
            public dynamic rightNode;
            public TokenType operation;

            public BinaryArithm(TokenType operation, AST leftNode, AST rightNode)
            {
                this.operation = operation;
                this.leftNode = leftNode;
                this.rightNode = rightNode;
            }

            protected override ASTList _children => new ASTList { leftNode, rightNode };
            protected override string _nodeName => operation.ToString();
        }

        class UnaryArithm : AST
        {
            public dynamic node;
            public TokenType op;

            public UnaryArithm(TokenType op, AST node)
            {
                // Debug.Assert(op.In(TokenType.NOT, TokenType.LNOT, TokenType.MINUS, TokenType.PLUS, TokenType.PLUSPLUS, TokenType.MINUSMINUS));
                this.op = op;
                this.node = node;
            }

            protected override object _value => op;
            protected override ASTList _children => new ASTList { node };
            protected override string _nodeName => op.ToString();
        }

        class Variable : AST
        {
            public string identifier;

            public Variable(string identifier)
            {
                this.identifier = identifier;
            }

            protected override object _value => identifier;
        }

        class VariableDecl : AST
        {
            public string type;
            public string identifier;

            public VariableDecl(string type, string identifier)
            {
                this.type = type;
                this.identifier = identifier;
            }

            protected override object _value => "{0} {1}".Format(type, identifier);
        }

        class ArrayDecl : VariableDecl
        {
            public dynamic countExpr;

            public ArrayDecl(string type, string identifier, AST countExpr) : base(type, identifier)
            {
                this.countExpr = countExpr;
            }

            protected override ASTList _children => new ASTList { countExpr };
        }

        class PointerDecl : VariableDecl
        {
            public dynamic decl;

            public PointerDecl(AST decl, string identifier) : base("pointer", identifier)
            {
                this.decl = decl;
            }

            protected override ASTList _children => new ASTList { decl };
        }

        class PointerAccess : AST
        {
            public dynamic node;

            public PointerAccess(AST node)
            {
                this.node = node;
            }

            protected override ASTList _children => new ASTList { node };
        }

        class PointerLiteral : AST
        {
            public dynamic node;

            public PointerLiteral(AST node)
            {
                this.node = node;
            }

            protected override ASTList _children => new ASTList { node };
        }

        class StructAccess : AST
        {
            public TokenType operation;
            public dynamic variable;
            public string field;

            public StructAccess(TokenType operation, AST variable, string field)
            {
                this.operation = operation;
                this.variable = variable;
                this.field = field;
            }

            protected override object _value => "{0} {1}".Format(operation, field);
            protected override ASTList _children => new ASTList { variable };
        }

        class VariableAssign : AST
        {
            public dynamic identifier;
            public dynamic node;

            public VariableAssign(AST identifier, AST node)
            {
                this.identifier = identifier;
                this.node = node;
            }

            protected override object _value => identifier;
            protected override ASTList _children => new ASTList { identifier, node };
        }

        class OperationAssign : AST
        {
            public dynamic identifier;
            public dynamic node;
            public TokenType op;

            public OperationAssign(AST identifier, TokenType op, AST node)
            {
                this.identifier = identifier;
                this.node = node;
                this.op = op;
            }

            protected override object _value => identifier;
            protected override ASTList _children => new ASTList { identifier, node };
            protected override string _nodeName => op.ToString();
        }

        class Ternary : AST
        {
            public dynamic condition;
            public dynamic ifBlock;
            public dynamic elseBlock;

            public Ternary(AST condition, AST ifBlock, AST elseBlock)
            {
                this.condition = condition;
                this.ifBlock = ifBlock;
                this.elseBlock = elseBlock;
            }

            protected override ASTList _children => new ASTList { condition, ifBlock, elseBlock };
        }

        class PostfixArithm : AST
        {
            public TokenType operation;
            public dynamic node;

            public PostfixArithm(TokenType operation, AST node)
            {
                this.operation = operation;
                this.node = node;
            }
            protected override object _value => operation;
            protected override ASTList _children => new ASTList { node };
        }

        class FuncCall : AST
        {
            public string identifier;
            public ASTList arguments;

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
                public string type;
                public string identifier;

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

            public string returnType;
            public string identifier;
            public List<Parameter> parameters;

            public FuncDecl(string returnType, string identifier, List<Parameter> parameters)
            {
                this.returnType = returnType;
                this.identifier = identifier;
                this.parameters = parameters;
            }

            protected override object _value => "{0} {1}({2})".Format(returnType, identifier, parameters.FormatList());
        }

        class FuncDef : FuncDecl
        {
            public BlockStatements block;

            public FuncDef(string returnType, string identifier, List<Parameter> parameters, BlockStatements block)
                : base(returnType, identifier, parameters)
            {
                this.block = block;
            }

            protected override ASTList _children => new ASTList { block };
        }

        class ArrayAccess : AST
        {
            public string identifier;
            public dynamic indexExpr;

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
            public ASTList valueListExprs;

            public ArrayLiteral(ASTList valueListExprs)
            {
                this.valueListExprs = valueListExprs;
            }

            protected override ASTList _children => valueListExprs;
        }

        class Statements : AST
        {
            public ASTList statements;

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
            public dynamic initialization;
            public dynamic condition;
            public dynamic loopExecution;
            public dynamic body;

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
            public dynamic condition;
            public dynamic body;

            public While(AST condition, AST body)
            {
                this.condition = condition;
                this.body = body;
            }

            protected override ASTList _children => new ASTList { condition, body };
        }

        class If : AST
        {
            public dynamic condition;
            public dynamic ifBody;
            public dynamic elseBody;

            public If(AST condition, AST ifBody, AST elseBody)
            {
                this.condition = condition;
                this.ifBody = ifBody;
                this.elseBody = elseBody;
            }

            protected override ASTList _children => new ASTList { condition, ifBody, elseBody };
        }

        class StructDef : AST
        {
            public string typeName;
            public dynamic program;

            public StructDef(string typeName, AST program)
            {
                this.typeName = typeName;
                this.program = program;
            }

            protected override object _value => typeName;
            protected override ASTList _children => new ASTList { program };
        }

        class Typedef : AST
        {
            public string typeName;
            public string alias;

            public Typedef(string typeName, string alias)
            {
                this.typeName = typeName;
                this.alias = alias;
            }

            protected override object _value => (typeName, alias);
        }

        class Break : AST
        {

        }

        class Continue : AST
        {

        }

        class Return : AST
        {
            public dynamic returnExpr;

            public Return(AST returnExpr)
            {
                this.returnExpr = returnExpr;
            }

            protected override ASTList _children => new ASTList { returnExpr };
        }

        class Cast : AST
        {
            public string castType;
            public dynamic node;

            public Cast(string castType, AST node)
            {
                this.castType = castType;
                this.node = node;
            }

            protected override object _value => castType;
            protected override ASTList _children => new ASTList { node };
        }

        class Intrinsic : AST
        {
            public ASTList parameters;
            public string type;

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
