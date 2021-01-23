using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c.compiler
{
    public class ReplaceWith
    {
        public enum Type
        {
            None,
            Void,

            Int,
            Float,
            Double,

            Char,
            String,

            Struct,
            Array,
            Pointer,
            Function,
        }

        public static ReplaceWith None => new ReplaceWith(Type.None, null);

        public Type type { get; private set; }
        public dynamic value { get; }
        public bool isCompileTimeKnown { get; private set; } = false;

        public ReplaceWith(Type type, dynamic value)
        {
            this.type = type;
            this.value = value;
            this.isCompileTimeKnown = value != null;
        }

        internal static Type GetTypeFromString(string s)
        {
            if (s == "int")
                return Type.Int;
            if (s == "void")
                return Type.Void;
            if (s.StartsWith('*'))
                return Type.Pointer;

            //throw new TypeAccessException("Type was not defined: " + s);
            return Type.Struct;
        }
    }

    class PreCompiler
    {
        public class PreCompiledAST
        {
            // Map from Scope to Map of variable identifiers and compiletime data for these
            //   compiletime data includes:
            //   type (int, struct, pointer, etc.),
            //   compiletime value (int a = 5 -> Map["a"].Value = 5)
            // Dictionary<ast.AST, Dictionary<string, Variable>> scopeVariables;

            public Program Program { get; }
            public List<FuncDef> Functions { get; }

            public PreCompiledAST(Program program, List<FuncDef> functions)
            {
                Program = program;
                Functions = functions;
            }

        }

        public PreCompiler()
        {
        }

        Dictionary<string, List<dynamic>> customTypes;
        List<FuncDef> functions;

        public PreCompiledAST PreComile(Program ast)
        {
            customTypes = new Dictionary<string, List<dynamic>>();
            functions = new List<FuncDef>();

            // add main call as last element to be executed
            ast.statements.Add(new FuncCall("main", new List<dynamic>()));

            dynamic preCompiled = ast;
            Walk(ast, ref preCompiled);

            return new PreCompiledAST(preCompiled, functions);
        }

        void WalkList(List<dynamic> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                dynamic statement = values[i];
                Walk(statement, ref statement);
                values[i] = statement;
            }
        }

        ReplaceWith Walk(AST node, ref dynamic field)
        {
            throw new NotImplementedException();
        }

        ReplaceWith Walk(Literal node, ref dynamic field)
        {
            switch (node.type)
            {
                case Literal.Type.INTLITERAL:
                    return new ReplaceWith(ReplaceWith.Type.Int, int.Parse(node.value));
                case Literal.Type.STRINGLITERAL:
                    return new ReplaceWith(ReplaceWith.Type.String, node.value.Escape());
                case Literal.Type.CHARLITERAL:
                    // BUG: chars can have more then one character BUT should be checked in the lexer
                    return new ReplaceWith(ReplaceWith.Type.Char, node.value.Escape());
                default:
                    throw new NotImplementedException("Not implemented Type of Literal: " + node.type.ToString());
            }
        }

        static Dictionary<TokenType, Func<int, int, int>> operatorToBinaryFunc = new Dictionary<TokenType, Func<int, int, int>>
            {
                {TokenType.PLUS,     (int x, int y) => x + y },
                {TokenType.MINUS,    (int x, int y) => x - y },
                {TokenType.DIVIDE,   (int x, int y) => x / y },
                {TokenType.STAR,     (int x, int y) => x * y },
                {TokenType.MODULO,   (int x, int y) => x % y },
                {TokenType.LT,       (int x, int y) => (x < y) ? 1 : 0 },
                {TokenType.GT,       (int x, int y) => (x > y) ? 1 : 0 },
                {TokenType.GEQ,      (int x, int y) => (x >= y) ? 1 : 0 },
                {TokenType.LEQ,      (int x, int y) => (x <= y) ? 1 : 0 },
                {TokenType.EQ,       (int x, int y) => (x == y) ? 1 : 0 },
                {TokenType.NEQ,      (int x, int y) => (x != y) ? 1 : 0 },

                {TokenType.AND,      (int x, int y) => (x != 0 && y != 0) ? 1 : 0 },
                {TokenType.OR,       (int x, int y) => (x != 0 || y != 0) ? 1 : 0 },
                // TODO string, char with these operators
                // {Operator.Code.EQ,       (int x, int y) => new ReplaceWith((x == y) ? 1 : 0) },
                // {Operator.Code.NEQ,      (int x, int y) => new ReplaceWith((x != y) ? 1 : 0) },
            };
        ReplaceWith Walk(BinaryArithm node, ref dynamic field)
        {
            ReplaceWith leftValue = Walk(node.leftNode, ref node.leftNode);
            ReplaceWith rightValue = Walk(node.rightNode, ref node.rightNode);

            if (!leftValue.isCompileTimeKnown || !rightValue.isCompileTimeKnown)
                return ReplaceWith.None;

            if (leftValue.type != rightValue.type)
                throw new InvalidOperationException("Cant use different types in calculation!");

            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            if (leftValue.type == ReplaceWith.Type.Int)
            {
                int value = operatorToBinaryFunc[node.operation].Invoke(leftValue.value, rightValue.value);
                field = new Literal(Literal.Type.INTLITERAL, value.ToString());
                return new ReplaceWith(ReplaceWith.Type.Int, value);
            }
            else
                throw new NotImplementedException("For now, only integer Arithmetic implemented!");
        }
        static Dictionary<TokenType, Func<int, int>> operatorToUnaryFunc = new Dictionary<TokenType, Func<int, int>>
            {
                {TokenType.PLUS,          (int x) => x },
                {TokenType.MINUS,         (int x) => -x },

                {TokenType.NOT,           (int x) => (x == 0) ? 1 : 0 },
                {TokenType.LNOT,          (int x) => ~x },
            };
        ReplaceWith Walk(UnaryArithm node, ref dynamic field)
        {
            ReplaceWith nodeValue = Walk(node.node, ref node.node);

            if (!nodeValue.isCompileTimeKnown)
                return ReplaceWith.None;

            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            if (nodeValue.type == ReplaceWith.Type.Int)
            {
                int value = operatorToUnaryFunc[node.op].Invoke(nodeValue.value);
                field = new Literal(Literal.Type.INTLITERAL, value.ToString());
                return new ReplaceWith(ReplaceWith.Type.Int, value);
            }
            else
                throw new NotImplementedException("For now, only integer Arithmetic implemented!");
        }
        ReplaceWith Walk(ArrayDecl node, ref dynamic field)
        {
            if (!Walk(node.countExpr, ref node.countExpr).isCompileTimeKnown)
                throw new ArgumentException("Array size declarator must be compile time known! " + node.identifier);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(Cast node, ref dynamic field)
        {
            ReplaceWith.Type type = ReplaceWith.GetTypeFromString(node.castType);
            return new ReplaceWith(type, Walk(node.node, ref node.node).value);
        }


        ReplaceWith Walk(Variable node, ref dynamic field)
        {
            return ReplaceWith.None;
        }
        ReplaceWith Walk(VariableDecl node, ref dynamic field)
        {
            return new ReplaceWith(ReplaceWith.GetTypeFromString(node.type), null);
        }
        ReplaceWith Walk(VariableAssign node, ref dynamic field)
        {
            ReplaceWith val = Walk(node.identifier, ref node.identifier);
            ReplaceWith value = Walk(node.node, ref node.node);

            // TODO would be nice to have :)
            // if (val.type != value.type)
            //     throw new InvalidCastException("Type values do not match: " + val.type + " " + value.type);

            return value;
        }
        ReplaceWith Walk(FuncCall node, ref dynamic field)
        {
            WalkList(node.arguments);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(FuncDecl node, ref dynamic field)
        {
            return ReplaceWith.None;
        }
        ReplaceWith Walk(FuncDef node, ref dynamic field)
        {
            dynamic block = node.block;
            Walk(node.block, ref block);
            node.block = block;

            // removes all code after a return statement
            int index = node.block.statements.FindIndex(statement => statement is Return);
            if (index++ != -1)
                node.block.statements.RemoveRange(index, node.block.statements.Count - index);
            
            if (!(node.block.statements.Last() is Return))
                node.block.statements.Add(new Return(new NoOp()));

            functions.Add(node);
            field = new NoOp();
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Statements node, ref dynamic field)
        {
            WalkList(node.statements);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(BlockStatements node, ref dynamic field)
        {
            WalkList(node.statements);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Program node, ref dynamic field)
        {
            Walk((Statements)node, ref field);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(For node, ref dynamic field)
        {
            Walk(node.initialization, ref node.initialization);

            ReplaceWith condition = Walk(node.condition, ref node.condition);
            if (condition.isCompileTimeKnown && condition.value == 0)
            {
                field = node.initialization;
                return ReplaceWith.None;
            }
            else
            {
                Walk(node.body, ref node.body);
                Walk(node.loopExecution, ref node.loopExecution);
            }

            return ReplaceWith.None;
        }
        ReplaceWith Walk(While node, ref dynamic field)
        {
            ReplaceWith condition = Walk(node.condition, ref node.condition);
            if (condition.isCompileTimeKnown && condition.value == 0)
            {
                field = new NoOp();
                return ReplaceWith.None;
            }
            else
            {
                Walk(node.body, ref node.body);
            }

            return ReplaceWith.None;
        }
        ReplaceWith Walk(If node, ref dynamic field)
        {
            ReplaceWith condition = Walk(node.condition, ref node.condition);
            if (condition.isCompileTimeKnown)
            {
                if (condition.value == 0)
                    field = node.elseBody;
                else
                    field = node.ifBody;
            }
            else
            {
                Walk(node.ifBody, ref node.ifBody);
                Walk(node.elseBody, ref node.elseBody);
            }

            return ReplaceWith.None;
        }
        ReplaceWith Walk(Break node, ref dynamic field)
        {
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Continue node, ref dynamic field)
        {
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Return node, ref dynamic field)
        {
            Walk(node.returnExpr, ref node.returnExpr);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Intrinsic node, ref dynamic field)
        {
            WalkList(node.parameters);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(ArrayAccess node, ref dynamic field)
        {
            Walk(node.indexExpr, ref node.indexExpr);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(ArrayLiteral node, ref dynamic field)
        {
            WalkList(node.valueListExprs);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(OperationAssign node, ref dynamic field)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            ReplaceWith leftValue = Walk(node.identifier, ref node.identifier);
            ReplaceWith rightValue = Walk(node.node, ref node.node);

            // TODO would be nice to have :)
            // if (leftValue.type != rightValue.type)
            //     throw new InvalidCastException("Type values do not match: " + leftValue.type + " " + rightValue.type);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(Ternary node, ref dynamic field)
        {
            ReplaceWith condition = Walk(node.condition, ref node.condition);
            if (condition.isCompileTimeKnown)
            {
                if (condition.value == 0)
                    field = node.elseBlock;
                else
                    field = node.ifBlock;
            }
            else
            {
                Walk(node.ifBlock, ref node.ifBlock);
                Walk(node.elseBlock, ref node.elseBlock);
            }

            return ReplaceWith.None;
        }
        ReplaceWith Walk(PointerDecl node, ref dynamic field)
        {
            Walk(node.decl, ref node.decl);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(PointerAccess node, ref dynamic field)
        {
            Walk(node.node, ref node.node);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(PointerLiteral node, ref dynamic field)
        {
            Walk(node.node, ref node.node);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(PostfixArithm node, ref dynamic field)
        {
            // TODO: At the moment everything is assumed to be a int, BinaryArithms with strings will fail
            // TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
            Walk(node.node, ref node.node);
            return ReplaceWith.None;
        }
        ReplaceWith Walk(Typedef node, ref dynamic field)
        {
            if (customTypes.ContainsKey(node.typeName))
                customTypes[node.alias] = customTypes[node.typeName];
            return ReplaceWith.None;
        }
        ReplaceWith Walk(StructDef node, ref dynamic field)
        {
            WalkList(node.program.statements);

            return ReplaceWith.None;
        }
        ReplaceWith Walk(StructAccess node, ref dynamic field)
        {
            Walk(node.variable, ref node.variable);

            return ReplaceWith.None;
        }

        ReplaceWith Walk(NoOp node, ref dynamic field)
        {
            return ReplaceWith.None;
        }

    }
}
