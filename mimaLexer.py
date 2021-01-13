#!/usr/bin/env python3

from mimaToken import TokenType, Token, TokenStream, Pos
import re
import sys

"""
Creates Tokenstream from raw input
"""

token_regex = {
    # These need to be prefix free (ordered)
    TokenType.STRINGLIT  : r"\".*?(?<!\\)\"",
    TokenType.CHARLIT    : r"'.*?(?<!\\)'",
    TokenType.INTLITERAL : r"[0-9]+",
    TokenType.LPAREN     : r"\(",
    TokenType.RPAREN     : r"\)",
    TokenType.PLUS       : r"\+",
    TokenType.MINUS      : r"-",
    TokenType.DIVIDE     : r"/",
    TokenType.MULTIPLY   : r"\*",
    TokenType.MODULO     : r"%",
    TokenType.EQUALS     : r"=",
    TokenType.COMMA      : r"\,",
    TokenType.WHILE      : r"while",
    TokenType.FOR        : r"for",
    TokenType.IF         : r"if",
    TokenType.ELSE       : r"else",
    TokenType.INTRINSIC  : r"printf",
    TokenType.RETURN     : r"return",
    TokenType.IDENTIFIER : r"[a-zA-Z_][a-zA-Z0-9_]*",
    TokenType.SEMICOLON  : r";",
    TokenType.LBRACE     : r"{",
    TokenType.RBRACE     : r"}",
    # TokenType.DOUBLE      : r"[0-9]*\.[0-9]+",
    # TokenType.FLOAT     : r"[0-9]*\.[0-9]+f",
}

class Lexer(object):
    def __init__(self, text : str):
        """
        text -- the COMPLETE text to be parsed
        """
        #NOTE: It would be better to have a more dynamic system for input but
        #this semms to be the easiest for now
        self.orig_text = text

    # TODO: count lines and characters

    def calcpos(self, resttext):
        parsedtext = self.orig_text[:self.orig_text.index(resttext)]
        newlines = parsedtext.count('\n')
        char = len(parsedtext.split('\n')[-1])
        return Pos(newlines, char)

    def tokenStream(self) -> TokenStream:
        text = self.orig_text
        tokens = [Token(TokenType.BOS, Pos(0, 0))]
        while(True):
            text = text.lstrip()
            if (len(text) == 0):
                # Done with the parsing
                tokens.append(Token(TokenType.EOS, Pos(-1, -1)))
                break
            for t_type, regex in token_regex.items():
                regex_match = re.match(r"^" + regex, text)
                if (regex_match):
                    # TODO: parse value if applicable
                    tokens.append(Token(t_type, self.calcpos(text)))
                    text = text[len(regex_match.group(0)):]

                    if t_type in [TokenType.INTLITERAL, TokenType.IDENTIFIER, TokenType.INTRINSIC]:
                        tokens[-1].value = regex_match.group(0)
                    elif t_type in [TokenType.STRINGLIT, TokenType.CHARLIT]:
                        tokens[-1].value = regex_match.group(0)[1:-1]

                    break
            else:
                # TODO: check that EOF has been reached?
                print("Can't match anymore")
                sys.exit()

        return TokenStream(tokens)
