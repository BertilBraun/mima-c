using System;
using System.Collections.Generic;
using System.Linq;

namespace mima_c
{
    enum TokenType
    {
        EOS,
        BOS,

        INTLITERAL,
        LPAREN,
        RPAREN,
        PLUS,
        MINUS,
        DIVIDE,
        MULTIPLY,
        MODULO,
        IDENTIFIER,
        SEMICOLON,
        EQUALS,
        COMMA,
        LBRACE,
        RBRACE,
        FOR,
        WHILE,
        IF,
        ELSE,
        INTRINSIC,
        RETURN,
        STRINGLIT,
        CHARLIT,
        LT,
        GT,
        GEQ,
        LEQ,
        EQUAL,
        NEQ,
        BREAK,
        CONTINUE,
        LBRACKET,
        RBRACKET,

        UNDEFINED
    }

    internal class Pos
    {
        public int line { get; }
        public int character { get; }

        public Pos(int line, int character)
        {
            this.line = line;
            this.character = character;
        }

        public override string ToString()
        {
            return "(" + line + ", " + character + ")";
        }
    }

    class Token
    {
        public Pos pos { get; }
        public TokenType tokenType { get; }
        public object value { get; set; }

        public Token() : this(TokenType.EOS)
        {
        }
        public Token(TokenType tokenType, Pos pos = null, object value = null)
        {
            this.tokenType = tokenType;
            this.pos = pos;
            this.value = value;
        }

        public override string ToString()
        {
            if (value != null)
                return string.Format("{0}({1})", tokenType.ToString().Replace("TokenType.", ""), value.ToString());
            else
                return tokenType.ToString().Replace("TokenType.", "");
        }
    }

    internal class TokenStream
    {
        private List<Token> tokens { get; set; }

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        Token eat(TokenType expectedTokenType)
        {
            Token nextToken = tokens.FirstOrDefault();

            if (nextToken.tokenType != expectedTokenType)
            {
                Console.WriteLine(string.Format("{0}: Tried to eat: {1} but has: {2}", nextToken.pos, expectedTokenType, nextToken.tokenType));
                Console.WriteLine("Leftover Tokenstream:");
                Console.WriteLine(tokens.ToString());
                Environment.Exit(1);
            }

            tokens.RemoveAt(0);
            return nextToken;
        }

        Token peek(int n = 0)
        {
            return tokens.Count <= n ? new Token() : tokens[n];
        }

        public override string ToString()
        {
            return "[" + string.Join(",", tokens) + "]";
        }
    }
}