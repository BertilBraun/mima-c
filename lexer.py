#!/usr/bin/env python3

from ctoken import TokenType, Token, TokenStream

"""
Creates Tokenstream from raw input
"""

token_regex = {
    TokenType.INTLITERAL : r"[0-9]+",
    TokenType.LPRAEN     : r"(",
    TokenType.RPRAEN     : r")",
    TokenType.PLUS       : r"\+",
    TokenType.MINUS      : r"-",
    TokenType.DIVIDE     : r"/",
    TokenType.MULTIPLY   : r"\*",
    TokenType.MODULO     : r"%"
}

def getnext_token():
    yield Token(TokenType.LPAREN)

class Lexer(object):
    def __init__(self, text : str):
        """
        text -- the COMPLETE text to be parsed
        """
        #NOTE: It would be better to have a more dynamic system for input but
        #this semms to be the easiest for now
        self.text = text;

    def tokenStream(self) -> TokenStream:
        pass
