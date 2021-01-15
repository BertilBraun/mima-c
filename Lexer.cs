﻿namespace mima_c
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class Lexer
    {
        internal Dictionary<TokenType, string> tokenRegex = new Dictionary<TokenType, string>
        {
            {TokenType.STRINGLIT  , @""".*?(?<!\\)""" },
            {TokenType.CHARLIT    , @"'.*?(?<!\\)'"},
            {TokenType.INTLITERAL , @"[0-9]+"},
            {TokenType.LPAREN     , @"\("},
            {TokenType.RPAREN     , @"\)"},
            {TokenType.PLUS       , @"\+"},
            {TokenType.MINUS      , @"-"},
            {TokenType.DIVIDE     , @"/"},
            {TokenType.MULTIPLY   , @"\*"},
            {TokenType.MODULO     , @"%"},
            {TokenType.GEQ        , @">="},
            {TokenType.LEQ        , @"<="},
            {TokenType.LT         , @"<"},
            {TokenType.GT         , @">"},
            {TokenType.EQUAL      , @"=="},
            {TokenType.NEQ        , @"<="},
            {TokenType.EQUALS     , @"="},
            {TokenType.COMMA      , @"\,"},
            {TokenType.WHILE      , @"while"},
            {TokenType.FOR        , @"for"},
            {TokenType.IF         , @"if"},
            {TokenType.ELSE       , @"else"},
            {TokenType.INTRINSIC  , @"printf"},
            {TokenType.RETURN     , @"return"},
            {TokenType.BREAK      , @"break"},
            {TokenType.CONTINUE   , @"continue"},
            {TokenType.IDENTIFIER , @"[a-zA-Z_][a-zA-Z0-9_]*"},
            {TokenType.SEMICOLON  , @";"},
            {TokenType.LBRACE     , @"{"},
            {TokenType.RBRACE     , @"}"},
            {TokenType.LBRACKET   , @"\["},
            {TokenType.RBRACKET   , @"\]"}
            // TokenType.DOUBLE      : r"[0-9]*\.[0-9]+",
            // TokenType.FLOAT     : r"[0-9]*\.[0-9]+f",
        };

        private string originalText { get; }

        public Lexer(string preprozessedText)
        {
            originalText = preprozessedText;
        }

        internal TokenStream getTokenStream()
        {
            string text = originalText;

            List<Token> tokens = new List<Token>();
            tokens.Add(new Token(TokenType.BOS, new Pos(0, 0)));

            while (true)
            {
                text = text.TrimStart();
                if (text.Length == 0)
                {
                    tokens.Add(new Token(TokenType.EOS, new Pos(-1, -1)));
                    break;
                }

                int tokenCount = tokens.Count;
                foreach (var item in tokenRegex)
                {
                    Match match = Regex.Match(text, "^" + item.Value);
                    if (match.Success)
                    {
                        string matchString = match.Value;

                        tokens.Add(new Token(item.Key, calcPos(text)));
                        text = text.Substring(matchString.Length);

                        if (item.Key.In(TokenType.INTLITERAL, TokenType.IDENTIFIER, TokenType.INTRINSIC))
                            tokens.Last().value = matchString;
                        else if (item.Key.In(TokenType.STRINGLIT, TokenType.CHARLIT))
                            tokens.Last().value = matchString.Substring(1, matchString.Length - 2);

                        break;
                    }
                }
                if (tokenCount == tokens.Count)
                {
                    Console.WriteLine("Can't match anymore");
                    Environment.Exit(1);
                }
            }

            return new TokenStream(tokens);
        }

        internal Pos calcPos(string restText)
        {
            string parsedText = originalText.Substring(0, originalText.Length - restText.Length);
            string[] newLines = parsedText.Split('\n');
            int characters = newLines.LastOrDefault().Length;
            return new Pos(newLines.Length, characters);
        }
    }
}
