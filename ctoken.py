#!/usr/bin/env python3

from enum import Enum

class TokenType(Enum):
    INTLITERAL = 0
    LPAREN     = 1
    RPAREN     = 2
    PLUS       = 3
    MINUS      = 4
    DIVIDE     = 5
    MULIPLY    = 6
    MODULO     = 7

class Token(object):
    def __init__(self, token_type : TokenType, value=None):
        self.token_type = token_type
        self.value = value

# token = Token(Token.PAREN)
# token = Token(Token.INTLITERAL, 5)
