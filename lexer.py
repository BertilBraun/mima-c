#!/usr/bin/env python3

from ctoken import TokenType, Token

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
