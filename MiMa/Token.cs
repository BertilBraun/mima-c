using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace mima_c
{
    enum TokenType
    {
        UNDEFINED,

        EOS,
        BOS,

        INTLITERAL,
        LPAREN,
        RPAREN,
        PLUS,
        MINUS,
        DIVIDE,
        STAR,
        MODULO,
        IDENTIFIER,
        SEMICOLON,
        ASSIGN,
        COMMA,
        LBRACE,
        RBRACE,
        FOR,
        WHILE,
        IF,
        ELSE,
        INTRINSIC,
        RETURN,
        STRINGLITERAL,
        CHARLITERAL,
        LT,
        GT,
        GEQ,
        LEQ,
        EQ,
        NEQ,
        BREAK,
        CONTINUE,
        LBRACKET,
        RBRACKET,
        AMPERSAND,
        PLUSPLUS,
        MINUSMINUS,
        QUESTIONMARK,
        COLON,
        AND,
        OR,
        DOT,
        ARROW,
        NOT,
        LNOT,
        DIVIDEEQ,
        MODULOEQ,
        STAREQ,
        MINUSEQ,
        PLUSEQ,
        STRUCT,
        TYPEDEF,
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
        public string value { get; set; }

        public Token() : this(TokenType.EOS)
        {
        }
        public Token(TokenType tokenType, Pos pos = null, string value = null)
        {
            this.tokenType = tokenType;
            this.pos = pos;
            this.value = value;
        }

        public override string ToString()
        {
            if (value != null)
                return "{0}({1})".Format(tokenType.ToString().Replace("TokenType.", ""), value.ToString());
            else
                return tokenType.ToString().Replace("TokenType.", "");
        }
    }

    internal class TokenStream
    {
        public List<Token> tokens { get; set; }

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Token Eat(TokenType expectedTokenType)
        {
            Token nextToken = tokens.FirstOrDefault();

            if (nextToken.tokenType != expectedTokenType)
            {
                Console.WriteLine("{0}: Tried to eat: {1} but has: {2}".Format(nextToken.pos, expectedTokenType, nextToken.tokenType));
                Console.WriteLine("Leftover Tokenstream:");
                Console.WriteLine(ToString());
                Debug.Assert(false);
                Environment.Exit(1);
            }

            tokens.RemoveAt(0);
            return nextToken;
        }

        public Token Peek(int n = 0)
        {
            return tokens.Count <= n ? new Token() : tokens[n];
        }

        public override string ToString()
        {
            return "[" + tokens.FormatList() + "]";
        }
    }
}