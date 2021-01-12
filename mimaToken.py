#!/usr/bin/env python3

from enum import Enum
import sys

class TokenType(Enum):
    INTLITERAL = 0
    LPAREN     = 1
    RPAREN     = 2
    PLUS       = 3
    MINUS      = 4
    DIVIDE     = 5
    MULTIPLY   = 6
    MODULO     = 7
    IDENTIFIER = 8
    SEMICOLON  = 9
    EQUALS     = 10
    COMMA      = 11

    # end of token stream
    EOS        = -1
    BOS        = -2

class Token(object):
    def __init__(self, token_type : TokenType, value=None):
        self.token_type = token_type
        self.value = value

    def __repr__(self):
        # TODO: add value
        if self.value:
            return "{}({})".format(str(self.token_type), str(self.value))
        else:
            return str(self.token_type)

class TokenStream(object):
    """Adds functionality for eating (asserting) and peeking tokens"""

    # NOTE: for now this is just a static list of tokens
    # TODO: make sure this works with generators as well
    # (Not important until the lexer also supports this)
    def __init__(self, tokens : [Token]):
        # NOTE: this is currently implemented using a list rather then a stack to support
        # some kind of buffering for peeking once we use generators
        self.tokens = tokens

    def eat(self, expected_token_type : TokenType) -> Token:
        next_token = Token(TokenType.EOS) if len(self.tokens) == 0 else self.tokens[0]

        if next_token.token_type != expected_token_type:
            print("Tried to eat: {} but has: {}".format(expected_token_type, next_token.token_type))
            # @HACK: proper error handling
            print("Leftover Tokenstream:")
            print(self.tokens)
            sys.exit()

        self.tokens = self.tokens[1:]
        return next_token

    def peek(self, n = 0)-> Token:
        """Non destuctive reading of the (nth) next token"""
        return Token(TokenType.EOS) if len(self.tokens) <= n else self.tokens[n]

    def __repr__(self):
        if len(self.tokens) == 0:
            return "[]"

        ret_str = "[" + str(self.tokens[0])
        for t in self.tokens[1:]:
            ret_str += ", " + str(t)
        ret_str += "]"
        return ret_str
