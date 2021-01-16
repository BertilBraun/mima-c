using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c
{
    using ASTList = List<dynamic>;

    abstract class Parser
    {
        private TokenStream TokenStream { get; }

        public Parser(TokenStream tokenStream)
        {
            TokenStream = tokenStream;
        }

        public abstract AST Parse();

        protected Token Eat(TokenType expectedToken)
        {
            return TokenStream.Eat(expectedToken);
        }
        protected string EatV(TokenType expectedToken)
        {
            return TokenStream.Eat(expectedToken).value;
        }

        protected bool PeekEatIf(TokenType expectedToken, int n = 0)
        {
            if (!PeekType(expectedToken))
                return false;
            Eat(expectedToken);
            return true;
        }

        protected Token Peek(int n = 0)
        {
            return TokenStream.Peek(n);
        }

        protected bool PeekType(TokenType expectedToken, int n = 0)
        {
            return Peek(n).tokenType == expectedToken;
        }
    }

    class CParser : Parser
    {
        public CParser(TokenStream tokenStream) : base(tokenStream)
        {
        }

        public override AST Parse()
        {
            Eat(TokenType.BOS);
            AST node = program();
            Eat(TokenType.EOS);
            return node;
        }

        private AST program()
        {
            ASTList statements = new ASTList();

            while (!PeekType(TokenType.EOS))
                statements.Add(statement());

            return new Program(statements);
        }

        private AST statement()
        {
            if (PeekType(TokenType.IDENTIFIER) && PeekType(TokenType.IDENTIFIER, 1))
            {
                if (PeekType(TokenType.LPAREN, 2))
                    return funcdecl();
                else
                {
                    AST node = vardecl();
                    Eat(TokenType.SEMICOLON);
                    return node;
                }
            }
            // unhandled error condition
            Eat(TokenType.UNDEFINED);
            return null;
        }

        private AST funcdecl()
        {
            // IDENTIFIER IDENTIFIER LPAREN (RPAREN | vardecl (COMMA vardecl)*)

            string returnType = EatV(TokenType.IDENTIFIER);
            string identifier = EatV(TokenType.IDENTIFIER);

            Eat(TokenType.LPAREN);

            List<FuncDecl.Parameter> parameters = new List<FuncDecl.Parameter>();

            if (!PeekType(TokenType.RPAREN))
            {
                string varType = EatV(TokenType.IDENTIFIER);
                string varIdentifier = EatV(TokenType.IDENTIFIER);

                parameters.Add(new FuncDecl.Parameter(varType, varIdentifier));

                while (PeekEatIf(TokenType.COMMA))
                {
                    varType = EatV(TokenType.IDENTIFIER);
                    varIdentifier = EatV(TokenType.IDENTIFIER);

                    parameters.Add(new FuncDecl.Parameter(varType, varIdentifier));
                }
            }

            Eat(TokenType.RPAREN);

            ASTList statements = new ASTList();
            statements.Add(new FuncDecl(returnType, identifier, parameters));

            if (PeekType(TokenType.LBRACE))
                statements.Add(new FuncDef(returnType, identifier, parameters, block() as BlockStatements));
            else
                Eat(TokenType.SEMICOLON);

            return new Statements(statements);
        }

        private AST vardecl()
        {
            string varType = EatV(TokenType.IDENTIFIER);

            ASTList statements = new ASTList();
            vardeclprime(statements, varType);

            while (PeekEatIf(TokenType.COMMA))
                vardeclprime(statements, varType);

            return new Statements(statements);
        }

        private void vardeclprime(ASTList statements, string varType)
        {
            string identifier = EatV(TokenType.IDENTIFIER);

            if (PeekEatIf(TokenType.LBRACKET))
            {
                if (PeekType(TokenType.RBRACKET))
                    statements.Add(new ArrayDecl(varType + "[]", identifier, null));
                else
                    statements.Add(new ArrayDecl(varType + "[]", identifier, expr()));
                Eat(TokenType.RBRACKET);
            }
            else
                statements.Add(new VariableDecl(varType, identifier));

            if (PeekEatIf(TokenType.ASSIGN))
                statements.Add(new VariableAssign(identifier, expr()));
        }

        // one extra level of recursion so it's easy to extend expr
        private AST expr()
        {
            return assignment();
        }

        private AST assignment()
        {
            if (PeekType(TokenType.IDENTIFIER) && PeekType(TokenType.ASSIGN, 1))
            {
                string identifier = EatV(TokenType.IDENTIFIER);
                Eat(TokenType.ASSIGN);
                return new VariableAssign(identifier, assignment());
            }

            return p7();
        }

        private AST p7()
        {
            return parseBinary(p6, TokenType.EQ, TokenType.NEQ);
        }

        private AST p6()
        {
            return parseBinary(p4, TokenType.LT, TokenType.GT, TokenType.LEQ, TokenType.GEQ);
        }

        private AST p4()
        {
            return parseBinary(p3, TokenType.PLUS, TokenType.MINUS);
        }

        private AST p3()
        {
            return parseBinary(unary, TokenType.MULTIPLY, TokenType.DIVIDE, TokenType.MODULO);
        }

        private AST unary()
        {
            if (PeekEatIf(TokenType.MINUS))
                return new UnaryArithm(Operator.Parse(TokenType.MINUS), value());
            // TODO Expand here in future for Pointers

            return value();
        }

        private AST value()
        {
            if (Literal.tokenToType.ContainsKey(Peek().tokenType))
            {
                Token token = Eat(Peek().tokenType);
                return new Literal(Literal.tokenToType[token.tokenType], token.value);
            }
            if (PeekType(TokenType.LBRACE))
            {
                return arrayLiteral();
            }
            if (PeekType(TokenType.IDENTIFIER))
            {
                if (PeekType(TokenType.LPAREN, 1))
                    return functionCall();
                else if (PeekType(TokenType.LBRACKET, 1))
                    return arrayAccess();
                else
                    return new Variable(EatV(TokenType.IDENTIFIER));
            }

            Eat(TokenType.LPAREN);
            AST node = expr();
            Eat(TokenType.RPAREN);
            return node;
        }

        private AST arrayLiteral()
        {
            Eat(TokenType.LBRACE);
            ASTList expressions = readExprList(TokenType.RBRACE);
            Eat(TokenType.RBRACE);
            return new ArrayLiteral(expressions);
        }

        private AST functionCall()
        {
            string identifier = EatV(TokenType.IDENTIFIER);

            Eat(TokenType.LPAREN);
            ASTList arguments = readExprList(TokenType.RPAREN);
            Eat(TokenType.RPAREN);

            return new FuncCall(identifier, arguments);
        }

        private AST arrayAccess()
        {
            string identifier = EatV(TokenType.IDENTIFIER);

            Eat(TokenType.LBRACKET);
            AST indexExpr = expr();
            Eat(TokenType.RBRACKET);

            return new ArrayAccess(identifier, indexExpr);
        }

        private AST block()
        {
            Eat(TokenType.LBRACE);

            ASTList blockStatements = new ASTList();
            while (!PeekType(TokenType.RBRACE))
                blockStatements.Add(blockStatement());

            Eat(TokenType.RBRACE);

            return new BlockStatements(blockStatements);
        }

        private AST blockStatement()
        {
            // NOTE: ONLY RETURN DIRECTLY IF NO SEMICOLON IS NEEDED

            if (PeekType(TokenType.LBRACE))
                return block();
            if (PeekType(TokenType.FOR))
                return for_();
            if (PeekType(TokenType.WHILE))
                return while_();
            if (PeekType(TokenType.IF))
                return if_();

            AST node;
            if (PeekType(TokenType.BREAK))
                node = break_();
            else if (PeekType(TokenType.CONTINUE))
                node = continue_();
            else if (PeekType(TokenType.RETURN))
                node = return_();
            else if (PeekType(TokenType.INTRINSIC))
                node = intrinsic();
            else if (PeekType(TokenType.IDENTIFIER) && PeekType(TokenType.IDENTIFIER, 1))
                node = vardecl();
            else
                node = expr();

            Eat(TokenType.SEMICOLON);
            return node;
        }

        private AST for_()
        {
            Eat(TokenType.FOR);
            Eat(TokenType.LPAREN);

            AST initialization;
            if (PeekType(TokenType.IDENTIFIER) && PeekType(TokenType.IDENTIFIER, 1))
                initialization = vardecl();
            else
                initialization = expr();
            Eat(TokenType.SEMICOLON);
            AST condition = expr();
            Eat(TokenType.SEMICOLON);
            AST execution = expr();
            Eat(TokenType.RPAREN);

            AST body = blockStatement();

            return new For(initialization, condition, execution, body);
        }

        private AST while_()
        {
            Eat(TokenType.WHILE);
            Eat(TokenType.LPAREN);
            AST condition = expr();
            Eat(TokenType.RPAREN);
            AST body = blockStatement();

            return new While(condition, body);
        }

        private AST if_()
        {
            Eat(TokenType.IF);
            Eat(TokenType.LPAREN);
            AST condition = expr();
            Eat(TokenType.RPAREN);
            AST ifBody = blockStatement();

            AST elseBody;
            if (PeekType(TokenType.ELSE))
            {
                Eat(TokenType.ELSE);
                elseBody = blockStatement();
            }
            else
                elseBody = new Statements(new ASTList());

            return new If(condition, ifBody, elseBody);
        }

        private AST break_()
        {
            Eat(TokenType.BREAK);
            return new Break();
        }

        private AST continue_()
        {
            Eat(TokenType.CONTINUE);
            return new Continue();
        }

        private AST return_()
        {
            Eat(TokenType.RETURN);
            return new Return(expr());
        }

        private AST intrinsic()
        {
            string type = EatV(TokenType.INTRINSIC);

            Eat(TokenType.LPAREN);
            ASTList parameters = readExprList(TokenType.RPAREN);
            Eat(TokenType.RPAREN);

            return new Intrinsic(parameters, type);
        }

        private AST parseBinary(Func<AST> nextNodeFunc, params TokenType[] types)
        {
            AST node1 = nextNodeFunc();

            while (types.ToList().Contains(Peek().tokenType))
            {
                TokenType type = Peek().tokenType;
                Eat(type);
                AST node2 = nextNodeFunc();
                node1 = new BinaryArithm(Operator.Parse(type), node1, node2);
            }

            return node1;
        }

        private ASTList readExprList(TokenType delimiter)
        {
            ASTList expressions = new ASTList();

            if (!PeekType(delimiter))
            {
                expressions.Add(expr());
                while (PeekEatIf(TokenType.COMMA))
                    expressions.Add(expr());
            }

            return expressions;
        }

    }
}
