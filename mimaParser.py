#!/usr/bin/env python3

from mimaLexer import getnext_token
from mimaToken import Token, TokenStream, TokenType
from mimaAst import *

"""
Creates AST from Tokenstream
"""

class Parser(object):
    def __init__(self, tokenstream : TokenStream):
        self.tokenstream = tokenstream

    def parse(self) -> Node:
        pass

    # Wrappers because I'm lazy
    def _eat(self, expected_token : TokenType) -> Token:
        return self.tokenstream.eat(expected_token)

    def _peek(self)-> Token:
        return self.tokenstream.peek()

# multiple classes keeps it a bit cleaner also this is almost like parser combinators
class AEParser(Parser):
    def value(self) -> Node:
        if self._peek().token_type == TokenType.INTLITERAL:
            token = self._eat(TokenType.INTLITERAL)
            return ValueNode(NodeType.INTLITERAL, token.value)
        # We could also parse the empty word here to allow for 0 input
        # That would avoid weird errors
        self._eat(TokenType.LPAREN)
        n1 = self.expr()
        self._eat(TokenType.RPAREN)
        return n1

    def unary(self) -> Node:
        if self._peek().token_type == TokenType.MINUS:
            self._eat(TokenType.MINUS)
            return UnaryNode(NodeType.MINUS, self.value())
        return self.value()

    def mod(self) -> Node:
        n1 = self.unary()
        while self._peek().token_type == TokenType.MODULO:
            self._eat(TokenType.MODULO)
            n2 = self.mod()
            n1 = BinaryNode(NodeType.MODULO, n1, n2)
        return n1

    def factor(self) -> Node:
        n1 = self.mod()

        while (self._peek().token_type in [TokenType.MULTIPLY, TokenType.DIVIDE]):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.mod()
            n1 = BinaryNode(NodeType.MULTIPLY if token_type == TokenType.MULTIPLY else
                            NodeType.DIVIDE, n1, n2)
        return n1

    def expr(self) -> Node:
        n1 = self.factor()

        while (self._peek().token_type in [TokenType.PLUS, TokenType.MINUS]):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.factor()
            n1 = BinaryNode(NodeType.PLUS if token_type == TokenType.PLUS else
                            NodeType.MINUS, n1, n2)
        return n1

    def parse(self) -> Node:
       self._eat(TokenType.BOS)
       node = self.expr()
       self._eat(TokenType.EOS)

       return node

class CParser(Parser):
    pass
