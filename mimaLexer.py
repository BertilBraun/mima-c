#!/usr/bin/env python3

from mimaToken import TokenType, Token, TokenStream
import re
import sys

"""
Creates Tokenstream from raw input
"""

token_regex = {
    # These need to be prefix free (ordered)
    TokenType.INTLITERAL : r"[0-9]+",
    TokenType.LPAREN     : r"\(",
    TokenType.RPAREN     : r"\)",
    TokenType.PLUS       : r"\+",
    TokenType.MINUS      : r"-",
    TokenType.DIVIDE     : r"/",
    TokenType.MULTIPLY   : r"\*",
    TokenType.MODULO     : r"%",
    # TokenType.IDENTIFIER : r"[a-zA-Z_]?[a-zA-Z0-9_]*",
    # TokenType.SEMICOLON  : r";"
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

    def tokenStream(self) -> TokenStream:
        text = self.orig_text
        tokens = [Token(TokenType.BOS)]
        while(True):
            text = text.lstrip()
            if (len(text) == 0):
                # Done with the parsing
                tokens.append(Token(TokenType.EOS))
                break
            for t_type, regex in token_regex.items():
                regex_match = re.match(r"^" + regex, text)
                if (regex_match):
                    # TODO: parse value if applicable
                    tokens.append(Token(t_type))
                    text = text[len(regex_match.group(0)):]

                    if (t_type == TokenType.INTLITERAL):
                        tokens[-1].value = regex_match.group(0)

                    break
            else:
                # TODO: check that EOF has been reached?
                print("Can't match anymore")
                sys.exit()

        return TokenStream(tokens)
