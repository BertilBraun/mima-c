using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c
{
    using ASTList = List<AST>;

    abstract class Parser
    {
        private TokenStream TokenStream { get; }

        public Parser(TokenStream tokenStream)
        {
            TokenStream = tokenStream;
        }

        public abstract AST parse();

        protected Token eat(TokenType expectedToken)
        {
            return TokenStream.eat(expectedToken);
        }
        protected string eatV(TokenType expectedToken)
        {
            return TokenStream.eat(expectedToken).value;
        }

        protected bool peekEatIf(TokenType expectedToken, int n = 0)
        {
            if (!peekType(expectedToken))
                return false;
            eat(expectedToken);
            return true;
        }

        protected Token peek(int n = 0)
        {
            return TokenStream.peek(n);
        }

        protected bool peekType(TokenType expectedToken, int n = 0)
        {
            return peek(n).tokenType == expectedToken;
        }
    }

    class CParser : Parser
    {
        public CParser(TokenStream tokenStream) : base(tokenStream)
        {
        }

        public override AST parse()
        {
            eat(TokenType.BOS);
            AST node = program();
            eat(TokenType.EOS);
            return node;
        }

        private AST program()
        {
            ASTList statements = new ASTList();

            while (!peekType(TokenType.EOS))
                statements.Add(statement());

            return new Program(statements);
        }

        private AST statement()
        {
            if (peekType(TokenType.IDENTIFIER) && peekType(TokenType.IDENTIFIER, 1))
            {
                if (peekType(TokenType.LPAREN, 2))
                    return funcdecl();
                else
                {
                    AST node = vardecl();
                    eat(TokenType.SEMICOLON);
                    return node;
                }
            }
            // unhandled error condition
            eat(TokenType.UNDEFINED);
            return null;
        }

        private AST funcdecl()
        {
            // IDENTIFIER IDENTIFIER LPAREN (RPAREN | vardecl (COMMA vardecl)*)

            string returnType = eatV(TokenType.IDENTIFIER);
            string identifier = eatV(TokenType.IDENTIFIER);

            eat(TokenType.LPAREN);

            List<FuncDecl.Parameter> parameters = new List<FuncDecl.Parameter>();

            if (!peekType(TokenType.RPAREN))
            {
                string varType = eatV(TokenType.IDENTIFIER);
                string varIdentifier = eatV(TokenType.IDENTIFIER);

                parameters.Add(new FuncDecl.Parameter(varType, varIdentifier));

                while (peekEatIf(TokenType.COMMA))
                {
                    varType = eatV(TokenType.IDENTIFIER);
                    varIdentifier = eatV(TokenType.IDENTIFIER);

                    parameters.Add(new FuncDecl.Parameter(varType, varIdentifier));
                }
            }

            eat(TokenType.RPAREN);

            ASTList statements = new ASTList();
            statements.Add(new FuncDecl(returnType, identifier, parameters));

            if (peekType(TokenType.LBRACE))
                statements.Add(new FuncDef(returnType, identifier, parameters, block()));
            else
                eat(TokenType.SEMICOLON);

            return new Statements(statements);
        }

        private AST vardecl()
        {
            string varType = eatV(TokenType.IDENTIFIER);

            ASTList statements = new ASTList();
            vardeclprime(statements, varType);

            while (peekEatIf(TokenType.COMMA))
                vardeclprime(statements, varType);

            return new Statements(statements);
        }

        private void vardeclprime(ASTList statements, string varType)
        {
            string identifier = eatV(TokenType.IDENTIFIER);

            if (peekEatIf(TokenType.LBRACKET))
            {
                if (peekType(TokenType.RBRACKET))
                    statements.Add(new ArrayDecl(varType + "[]", identifier, null));
                else
                    statements.Add(new ArrayDecl(varType + "[]", identifier, expr()));
                eat(TokenType.RBRACKET);
            }
            else
                statements.Add(new VariableDecl(varType, identifier));

            if (peekEatIf(TokenType.ASSIGN))
                statements.Add(new VariableAssign(identifier, expr()));
        }

        // one extra level of recursion so it's easy to extend expr
        private AST expr()
        {
            return assignment();
        }

        private AST assignment()
        {
            if (peekType(TokenType.IDENTIFIER) && peekType(TokenType.ASSIGN, 1))
            {
                string identifier = eatV(TokenType.IDENTIFIER);
                eat(TokenType.ASSIGN);
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
            if (peekEatIf(TokenType.MINUS))
                return new UnaryArithm(Operator.parse(TokenType.MINUS), value());
            // TODO Expand here in future for Pointers

            return value();
        }

        private AST value()
        {
            if (Literal.tokenToType.ContainsKey(peek().tokenType))
            {
                Token token = eat(peek().tokenType);
                return new Literal(Literal.tokenToType[token.tokenType], token.value);
            }
            if (peekType(TokenType.LBRACE))
            {
                return arrayLiteral();
            }
            if (peekType(TokenType.IDENTIFIER))
            {
                if (peekType(TokenType.LPAREN, 1))
                    return functionCall();
                else if (peekType(TokenType.LBRACKET, 1))
                    return arrayAccess();
                else
                    return new Variable(eatV(TokenType.IDENTIFIER));
            }

            eat(TokenType.LPAREN);
            AST node = expr();
            eat(TokenType.RPAREN);
            return node;
        }

        private AST arrayLiteral()
        {
            eat(TokenType.LBRACE);
            ASTList expressions = readExprList(TokenType.RBRACE);
            eat(TokenType.RBRACE);
            return new ArrayLiteral(expressions);
        }

        private AST functionCall()
        {
            string identifier = eatV(TokenType.IDENTIFIER);

            eat(TokenType.LPAREN);
            ASTList arguments = readExprList(TokenType.RPAREN);
            eat(TokenType.RPAREN);

            return new FuncCall(identifier, arguments);
        }

        private AST arrayAccess()
        {
            string identifier = eatV(TokenType.IDENTIFIER);

            eat(TokenType.LBRACKET);
            AST indexExpr = expr();
            eat(TokenType.RBRACKET);

            return new ArrayAccess(identifier, indexExpr);
        }

        private AST block()
        {
            eat(TokenType.LBRACE);

            ASTList blockStatements = new ASTList();
            while (!peekType(TokenType.RBRACE))
                blockStatements.Add(blockStatement());

            eat(TokenType.RBRACE);

            return new BlockStatements(blockStatements);
        }

        private AST blockStatement()
        {
            // NOTE: ONLY RETURN DIRECTLY IF NO SEMICOLON IS NEEDED

            if (peekType(TokenType.LBRACE))
                return block();
            if (peekType(TokenType.FOR))
                return for_();
            if (peekType(TokenType.WHILE))
                return while_();
            if (peekType(TokenType.IF))
                return if_();

            AST node;
            if (peekType(TokenType.BREAK))
                node = break_();
            else if (peekType(TokenType.CONTINUE))
                node = continue_();
            else if (peekType(TokenType.RETURN))
                node = return_();
            else if (peekType(TokenType.INTRINSIC))
                node = intrinsic();
            else if (peekType(TokenType.IDENTIFIER) && peekType(TokenType.IDENTIFIER, 1))
                node = vardecl();
            else
                node = expr();

            eat(TokenType.SEMICOLON);
            return node;
        }

        private AST for_()
        {
            eat(TokenType.FOR);
            eat(TokenType.LPAREN);

            AST initialization;
            if (peekType(TokenType.IDENTIFIER) && peekType(TokenType.IDENTIFIER, 1))
                initialization = vardecl();
            else
                initialization = expr();
            eat(TokenType.SEMICOLON);
            AST condition = expr();
            eat(TokenType.SEMICOLON);
            AST execution = expr();
            eat(TokenType.RPAREN);

            AST body = blockStatement();

            return new For(initialization, condition, execution, body);
        }

        private AST while_()
        {
            eat(TokenType.WHILE);
            eat(TokenType.LPAREN);
            AST condition = expr();
            eat(TokenType.RPAREN);
            AST body = blockStatement();

            return new While(condition, body);
        }

        private AST if_()
        {
            eat(TokenType.IF);
            eat(TokenType.LPAREN);
            AST condition = expr();
            eat(TokenType.RPAREN);
            AST ifBody = blockStatement();

            AST elseBody;
            if (peekType(TokenType.ELSE))
            {
                eat(TokenType.ELSE);
                elseBody = blockStatement();
            }
            else
                elseBody = new Statements(new ASTList());

            return new If(condition, ifBody, elseBody);
        }

        private AST break_()
        {
            eat(TokenType.BREAK);
            return new Break();
        }

        private AST continue_()
        {
            eat(TokenType.CONTINUE);
            return new Continue();
        }

        private AST return_()
        {
            eat(TokenType.RETURN);
            return new Return(expr());
        }

        private AST intrinsic()
        {
            string type = eatV(TokenType.INTRINSIC);

            eat(TokenType.LPAREN);
            ASTList parameters = readExprList(TokenType.RPAREN);
            eat(TokenType.RPAREN);

            return new Intrinsic(parameters, type);
        }

        private AST parseBinary(Func<AST> nextNodeFunc, params TokenType[] types)
        {
            AST node1 = nextNodeFunc();

            while (types.ToList().Contains(peek().tokenType))
            {
                TokenType type = peek().tokenType;
                eat(type);
                AST node2 = nextNodeFunc();
                node1 = new BinaryArithm(Operator.parse(type), node1, node2);
            }

            return node1;
        }

        private ASTList readExprList(TokenType delimiter)
        {
            ASTList expressions = new ASTList();

            if (!peekType(delimiter))
            {
                expressions.Add(expr());
                while (peekEatIf(TokenType.COMMA))
                    expressions.Add(expr());
            }

            return expressions;
        }

    }
}
