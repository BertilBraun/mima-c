namespace mima_c
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class Lexer
    {
        static string KEYWORD_END = @"(?=\W)";

        internal Dictionary<TokenType, string> tokenRegex = new Dictionary<TokenType, string>
        {
            {TokenType.STRINGLITERAL  , @""".*?(?<!\\)""" },
            {TokenType.CHARLITERAL    , @"'.*?(?<!\\)'"},
            {TokenType.INTLITERAL     , @"[0-9]+"},
            {TokenType.LPAREN         , @"\("},
            {TokenType.RPAREN         , @"\)"},
            {TokenType.AND            , @"&&"},
            {TokenType.OR             , @"\|\|"},
            {TokenType.DOT            , @"\."},
            {TokenType.ARROW          , @"\-\>"},
            {TokenType.PLUSPLUS       , @"\+\+"},
            {TokenType.MINUSMINUS     , @"--"},
            {TokenType.PLUSEQ         , @"\+="},
            {TokenType.MINUSEQ        , @"-="},
            {TokenType.DIVIDEEQ       , @"/="},
            {TokenType.STAREQ         , @"\*="},
            {TokenType.MODULOEQ       , @"%="},
            {TokenType.PLUS           , @"\+"},
            {TokenType.MINUS          , @"-"},
            {TokenType.DIVIDE         , @"/"},
            {TokenType.STAR           , @"\*"},
            {TokenType.MODULO         , @"%"},
            {TokenType.AMPERSAND      , @"&"},
            {TokenType.GEQ            , @">="},
            {TokenType.LEQ            , @"<="},
            {TokenType.LT             , @"<"},
            {TokenType.GT             , @">"},
            {TokenType.EQ             , @"=="},
            {TokenType.NEQ            , @"!="},
            {TokenType.NOT            , @"!"},
            {TokenType.LNOT           , @"~"},
            {TokenType.ASSIGN         , @"="},
            {TokenType.COMMA          , @"\,"},
            {TokenType.WHILE          , @"while" + KEYWORD_END},
            {TokenType.FOR            , @"for" + KEYWORD_END},
            {TokenType.IF             , @"if" + KEYWORD_END},
            {TokenType.ELSE           , @"else" + KEYWORD_END},
            {TokenType.INTRINSIC      , @"printf" + KEYWORD_END},
            {TokenType.RETURN         , @"return" + KEYWORD_END},
            {TokenType.BREAK          , @"break" + KEYWORD_END},
            {TokenType.CONTINUE       , @"continue" + KEYWORD_END},
            {TokenType.IDENTIFIER     , @"[a-zA-Z_][a-zA-Z0-9_]*"},
            {TokenType.SEMICOLON      , @";"},
            {TokenType.LBRACE         , @"{"},
            {TokenType.RBRACE         , @"}"},
            {TokenType.LBRACKET       , @"\["},
            {TokenType.RBRACKET       , @"\]"},
            {TokenType.QUESTIONMARK   , @"\?"},
            {TokenType.COLON          , @":"},
            // TokenType.DOUBLE      : r"[0-9]*\.[0-9]+",
            // TokenType.FLOAT     : r"[0-9]*\.[0-9]+f",
        };

        private string originalText { get; }

        public Lexer(string preprozessedText)
        {
            originalText = preprozessedText;
        }

        internal TokenStream GetTokenStream()
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

                        tokens.Add(new Token(item.Key, CalcPos(text)));
                        text = text.Substring(matchString.Length);

                        if (item.Key.In(TokenType.INTLITERAL, TokenType.IDENTIFIER, TokenType.INTRINSIC))
                            tokens.Last().value = matchString;
                        else if (item.Key.In(TokenType.STRINGLITERAL, TokenType.CHARLITERAL))
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

        internal Pos CalcPos(string restText)
        {
            string parsedText = originalText.Substring(0, originalText.Length - restText.Length);
            string[] newLines = parsedText.Split('\n');
            int characters = newLines.LastOrDefault().Length;
            return new Pos(newLines.Length, characters);
        }
    }
}
