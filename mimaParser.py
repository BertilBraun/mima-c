#!/usr/bin/env python3

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

    def _peek(self, n : int = 0)-> Token:
        return self.tokenstream.peek(n)

    def _peekType(self, peek_type : TokenType, n : int = 0):
        return self._peek(n).token_type == peek_type

# multiple classes keeps it a bit cleaner also this is almost like parser combinators
class AEParser(Parser):
    def functioncall(self) -> Node:
        token = self._eat(TokenType.IDENTIFIER)

        self._eat(TokenType.LPAREN)

        arguments = []
        # TODO: all constants should be accepted
        while (self._peekType(TokenType.IDENTIFIER) or 
               self._peekType(TokenType.INTLITERAL)):
            arguments.append(self.value())
            if (self._peekType(TokenType.COMMA)):
                self._eat(TokenType.COMMA)
            else:
                break

        self._eat(TokenType.RPAREN)

        # TODO: how should the arguments be propergated?
        call_data = (token.value, arguments) # like this?
        return ValueNode(NodeType.FUNCCALL, token.value)

    def value(self) -> Node:
        if self._peekType(TokenType.INTLITERAL):
            token = self._eat(TokenType.INTLITERAL)
            return ValueNode(NodeType.INTLITERAL, token.value)
        if self._peekType(TokenType.IDENTIFIER):
            if self._peekType(TokenType.LPAREN, 1):
                return self.functioncall()
            else:
                token = self._eat(TokenType.IDENTIFIER)
                return ValueNode(NodeType.VAR, token.value)
        # We could also parse the empty word here to allow for 0 input
        # That would avoid weird errors
        self._eat(TokenType.LPAREN)
        n1 = self.expr()
        self._eat(TokenType.RPAREN)
        return n1

    def unary(self) -> Node:
        if self._peekType(TokenType.MINUS):
            self._eat(TokenType.MINUS)
            return UnaryNode(NodeType.MINUS, self.value())
        return self.value()

    def mod(self) -> Node:
        n1 = self.unary()
        while self._peekType(TokenType.MODULO):
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

    def add(self) -> Node:
        n1 = self.factor()

        while (self._peek().token_type in [TokenType.PLUS, TokenType.MINUS]):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.factor()
            n1 = BinaryNode(NodeType.PLUS if token_type == TokenType.PLUS else
                            NodeType.MINUS, n1, n2)
        return n1

    def varassign(self) -> Node:
        # while there is somehting like "a ="
        if (self._peekType(TokenType.IDENTIFIER) and self._peekType(TokenType.EQUALS, 1)):
            var_identifier = self._eat(TokenType.IDENTIFIER)
            self._eat(TokenType.EQUALS)
            return UnaryNode(NodeType.ASSIGN, self.varassign(), var_identifier)

        return self.add()

    def expr(self) -> Node:
        return self.varassign()

    def vardecl(self) -> Node:
        var_type = self._eat(TokenType.IDENTIFIER)
        var_identifier = self._eat(TokenType.IDENTIFIER)

        # NOTE: this could be it's own class to make things more clear
        var_data = (var_type, var_identifier)

        # we can write multiple statements in one declaration
        # e.g.: int a, b = 5, c;
        statements = []

        # For now the difference between declaration with and without assignment is the
        # class of the node used
        # ValueNode for declaration
        # UnaryNode for declaration with assignment

        # Duplicate code that could be extraced
        if self._peekType(TokenType.EQUALS):
            self._eat(TokenType.EQUALS)
            statements.append(UnaryNode(NodeType.DECL, self.expr(), var_data))
        else:
            statements.append(ValueNode(NodeType.DECL, var_data))

        while (self._peekType(TokenType.COMMA)):
            self._eat(TokenType.COMMA)
            var_identifier = self._eat(TokenType.IDENTIFIER)
            var_data = (var_type, var_identifier)

            if self._peekType(TokenType.EQUALS):
                self._eat(TokenType.EQUALS)
                statements.append(UnaryNode(NodeType.DECL, self.expr(), var_data))
            else:
                statements.append(ValueNode(NodeType.DECL, var_data))

        # A collection of statements to be executed
        return NaryNode(NodeType.PROGRAM, statements)

    # TODO: differentiate between general statements and blockstatements
    def statement(self) -> Node:
        if self._peekType(TokenType.IDENTIFIER):
            if self._peekType(TokenType.IDENTIFIER, 1):
                node = self.vardecl()
            else:
                node = self.varassign()
        else:
            node = self.expr()
        self._eat(TokenType.SEMICOLON)
        return node

    def block(self) -> Node:
        self._eat(TokenType.LBRACE)
        block_statements = []
        while(not self._peekType(TokenType.RBRACE)):
            block_statements.append(self.statement())
        self._eat(TokenType.RBRACE)
        return NaryNode(NodeType.BLOCK, block_statements)

    def funcdecl(self) -> Node:
        func_return_type = self._eat(TokenType.IDENTIFIER)
        func_identifier = self._eat(TokenType.IDENTIFIER)

        self._eat(TokenType.LPAREN)

        parameters = []
        while (self._peekType(TokenType.IDENTIFIER)):
            parameters.append(self.vardecl())
            if (self._peekType(TokenType.COMMA)):
                self._eat(TokenType.COMMA)
            else:
                break

        self._eat(TokenType.RPAREN)

        # TODO: make this its own class
        func_data = (func_return_type, func_identifier, parameters)

        if self._peekType(TokenType.LBRACE):
            node = self.block()
            return UnaryNode(NodeType.FUNCDECL, node, func_data)
        else:
            self._eat(TokenType.SEMICOLON)
            return ValueNode(NodeType.FUNCDECL, func_data)

    # program -> ([statement, funcdecl])*
    def program(self) -> Node:
        statements = []
        while (not self._peekType(TokenType.EOS)):
            # loads of lookahead
            if (self._peekType(TokenType.IDENTIFIER) and
                self._peekType(TokenType.IDENTIFIER, 1) and
                self._peekType(TokenType.LPAREN, 2)):
                statements.append(self.funcdecl())
            elif self._peekType(TokenType.LBRACE):
                statements.append(self.block())
            else:
                statements.append(self.statement())
        return NaryNode(NodeType.PROGRAM, statements)


    def parse(self) -> Node:
       self._eat(TokenType.BOS)
       node = self.program()
       self._eat(TokenType.EOS)

       return node

class CParser(Parser):
    pass
