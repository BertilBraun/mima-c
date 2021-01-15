#!/usr/bin/env python3

from enum import Enum
import sys

class TokenType(Enum):
    # end of token stream
    EOS        = 0
    BOS        = 1

    INTLITERAL = 2
    LPAREN     = 3
    RPAREN     = 4
    PLUS       = 5
    MINUS      = 6
    DIVIDE     = 7
    MULTIPLY   = 8
    MODULO     = 9
    IDENTIFIER = 10
    SEMICOLON  = 11
    EQUALS     = 12
    COMMA      = 13
    LBRACE     = 14
    RBRACE     = 15
    FOR        = 16
    WHILE      = 17
    IF         = 18
    ELSE       = 19
    INTRINSIC  = 20
    RETURN     = 21
    STRINGLIT  = 22
    CHARLIT    = 23
    LT         = 24
    GT         = 25
    GEQ        = 26
    LEQ        = 27
    EQUAL      = 28
    NEQ        = 29
    BREAK      = 30
    CONTINUE   = 31
    LBRACKET   = 32
    RBRACKET   = 33

    UNDEFINED = -1

class Pos:
    def __init__(self, line, char):
        self.line = line
        self.char = char

    def __repr__(self):
        return "({}, {})".format(self.line, self.char)

class Token:
    def __init__(self, token_type : TokenType, pos : Pos, value=None):
        self.token_type = token_type
        self.value = value
        self.pos = pos

    def __repr__(self):
        # TODO: add value
        if self.value:
            return "{}({})".format(str(self.token_type).replace("TokenType.", ""), str(self.value))
        else:
            return str(self.token_type)

class TokenStream:
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
            print("{}: Tried to eat: {} but has: {}".format(next_token.pos, expected_token_type, next_token.token_type))
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
        return '[' + ", ".join(str(t) for t in self.tokens) + ']'
