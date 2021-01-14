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
        identifier = self._eat(TokenType.IDENTIFIER).value

        self._eat(TokenType.LPAREN)

        # a list of nodes
        arguments = []

        # LPAREN (RPAREN | expr (COMMA expr)* RPAREN)

        # Check for function arguments
        if not self._peekType(TokenType.RPAREN):
            arguments.append(self.expr())

            # function arguments can be arbitrary expressions
            while self._peekType(TokenType.COMMA):
                self._eat(TokenType.COMMA)
                arguments.append(self.expr())

        self._eat(TokenType.RPAREN)

        return NodeFuncCall(identifier, arguments)

    def value(self) -> Node:
        token_to_node_type = {
            TokenType.INTLITERAL : NodeValue.Type.INTLITERAL,
            TokenType.STRINGLIT  : NodeValue.Type.STRINGLIT,
            TokenType.CHARLIT    : NodeValue.Type.CHARLIT
        }
        if self._peek().token_type in token_to_node_type.keys():
            token = self._eat(self._peek().token_type)
            return NodeValue(token_to_node_type[token.token_type], token.value)
        if self._peekType(TokenType.IDENTIFIER):
            if self._peekType(TokenType.LPAREN, 1):
                return self.functioncall()
            else:
                token = self._eat(TokenType.IDENTIFIER)
                return NodeVariable(token.value)
        # We could also parse the empty word here to allow for 0 input
        # That would avoid weird errors
        self._eat(TokenType.LPAREN)
        n1 = self.expr()
        self._eat(TokenType.RPAREN)
        return n1

    def unary(self) -> Node:
        if self._peekType(TokenType.MINUS):
            self._eat(TokenType.MINUS)
            return NodeUnaryArithm("-", self.value())
        return self.value()

    def p3(self) -> Node:
        n1 = self.unary()

        token_to_op = {
            TokenType.MULTIPLY : "*",
            TokenType.DIVIDE : "/",
            TokenType.MODULO   : "%"
        }

        while self._peek().token_type in token_to_op.keys():
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.unary()
            n1 = NodeBinaryArithm(token_to_op[token_type], n1, n2)
        return n1

    def p4(self) -> Node:
        n1 = self.p3()

        token_to_op = {
            TokenType.PLUS : "+",
            TokenType.MINUS : "-"
        }

        while (self._peek().token_type in token_to_op.keys()):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.p3()
            n1 = NodeBinaryArithm(token_to_op[token_type], n1, n2)
        return n1

    def p6(self) -> Node:
        n1 = self.p4()

        token_to_op = {
            TokenType.LT  : "<",
            TokenType.GT  : ">",
            TokenType.GEQ : ">=",
            TokenType.LEQ : "<="
        }

        while (self._peek().token_type in token_to_op.keys()):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.p4()
            n1 = NodeBinaryArithm(token_to_op[token_type], n1, n2)

        return n1

    def p7(self):
        n1 = self.p6()

        token_to_op = {
            TokenType.EQUAL : "==",
            TokenType.NEQ   : "!="
        }

        while (self._peek().token_type in token_to_op.keys()):
            token_type = self._peek().token_type
            self._eat(token_type)
            n2 = self.p6()
            n1 = NodeBinaryArithm(token_to_op[token_type], n1, n2)

        return n1

    def assignment(self) -> Node:
        # while there is somehting like "a ="
        if (self._peekType(TokenType.IDENTIFIER) and self._peekType(TokenType.EQUALS, 1)):
            var_identifier = self._eat(TokenType.IDENTIFIER).value
            self._eat(TokenType.EQUALS)
            return NodeVariableAssign(var_identifier, self.assignment())

        return self.p7()

    # one extra level of recursion so it's easy to extend expr
    def expr(self) -> Node:
        return self.assignment()

    def vardecl(self) -> Node:
        var_type = self._eat(TokenType.IDENTIFIER).value
        var_identifier = self._eat(TokenType.IDENTIFIER).value

        # we can write multiple statements in one declaration
        # e.g.: int a, b = 5, c;
        statements = []

        # For now the difference between declaration with and without assignment is the
        # class of the node used
        # ValueNode for declaration
        # UnaryNode for declaration with assignment

        statements.append(NodeVariableDecl(var_type, var_identifier))

        # Duplicate code that could be extraced
        if self._peekType(TokenType.EQUALS):
            self._eat(TokenType.EQUALS)
            statements.append(NodeVariableAssign(var_identifier, self.expr()))

        while (self._peekType(TokenType.COMMA)):
            self._eat(TokenType.COMMA)
            var_identifier = self._eat(TokenType.IDENTIFIER).value
            var_data = (var_type, var_identifier)

            statements.append(NodeVariableDecl(var_type, var_identifier))
            if self._peekType(TokenType.EQUALS):
                self._eat(TokenType.EQUALS)
                statements.append(NodeVariableAssign(var_identifier, self.expr()))

        return NodeStatements(statements)

    # TODO: differentiate between general statements and blockstatements
    def statement(self) -> Node:
        if self._peekType(TokenType.IDENTIFIER) and \
           self._peekType(TokenType.IDENTIFIER, 1):
            if self._peekType(TokenType.LPAREN, 2):
                return self.funcdecl()
            else:
                node = self.vardecl()
                self._eat(TokenType.SEMICOLON)
                return node
        # unhandled error condition
        self._eat(TokenType.UNDEFINED)

    def block(self) -> Node:
        self._eat(TokenType.LBRACE)
        block_statements = []
        while not self._peekType(TokenType.RBRACE):
            block_statements.append(self.blockstatement())
        self._eat(TokenType.RBRACE)
        return NodeBlockStatements(block_statements)

    def for_(self):

        # Possible way to return [blockstatments] rather then dedicated ForloopNode
        #
        # {
        #     initialization
        #     while (cond) {
        #           body
        #           loop execution
        #     }
        # }

        self._eat(TokenType.FOR)
        self._eat(TokenType.LPAREN)

        if self._peekType(TokenType.IDENTIFIER) and \
           self._peekType(TokenType.IDENTIFIER, 1):
            initialization = self.vardecl()
        else:
            initialization = self.expr()
        self._eat(TokenType.SEMICOLON)
        condition = self.expr()
        self._eat(TokenType.SEMICOLON)
        execution = self.expr()
        self._eat(TokenType.RPAREN)

        body = self.blockstatement()

        return NodeFor(initialization, condition, execution, body)

    def while_(self):
        self._eat(TokenType.WHILE)
        self._eat(TokenType.LPAREN)
        condition = self.expr()
        self._eat(TokenType.RPAREN)
        body = self.blockstatement()

        return NodeWhile(condition, body)

    def if_(self):
        self._eat(TokenType.IF)
        self._eat(TokenType.LPAREN)
        condition = self.expr()
        self._eat(TokenType.RPAREN)
        ifbody = self.blockstatement()
        if self._peekType(TokenType.ELSE):
            self._eat(TokenType.ELSE)
            elsebody = self.blockstatement()
        else:
            elsebody = NodeStatements([])
        return NodeIf(condition, ifbody, elsebody)

    def return_(self) -> Node:
        self._eat(TokenType.RETURN)
        n1 = NodeReturn(self.expr())
        return n1

    def intrinsic(self) -> Node:
        token = self._eat(TokenType.INTRINSIC)
        self._eat(TokenType.LPAREN)

        parameters = []
        if not self._peekType(TokenType.RPAREN):
            parameters.append(self.expr())
            while self._peekType(TokenType.COMMA):
                self._eat(TokenType.COMMA)
                parameters.append(self.expr())
        self._eat(TokenType.RPAREN)

        return NodeIntrinsic(parameters, token.value)

    def blockstatement(self) -> Node:
        # NOTE: ONLY RETURN DIRECTLY IF NO SEMICOLON IS NEEDED

        if self._peekType(TokenType.LBRACE):
            return self.block()
        if self._peekType(TokenType.FOR):
            return self.for_()
        if self._peekType(TokenType.WHILE):
            return self.while_()
        if self._peekType(TokenType.IF):
            return self.if_()

        if self._peekType(TokenType.RETURN):
            node = self.return_()
        elif self._peekType(TokenType.INTRINSIC):
            node = self.intrinsic()
        elif self._peekType(TokenType.IDENTIFIER) and \
           self._peekType(TokenType.IDENTIFIER, 1):
            node = self.vardecl()
        else:
            node = self.expr()
        self._eat(TokenType.SEMICOLON)
        return node

    def funcdecl(self) -> Node:
        func_return_type = self._eat(TokenType.IDENTIFIER).value
        func_identifier = self._eat(TokenType.IDENTIFIER).value

        self._eat(TokenType.LPAREN)

        # IDENTIFIER IDENTIFIER LPAREN (RPAREN | vardecl (COMMA vardecl)*)

        parameters = []

        if (not self._peekType(TokenType.RPAREN)):
            var_type = self._eat(TokenType.IDENTIFIER).value
            var_identifier = self._eat(TokenType.IDENTIFIER).value
            var_data = (var_type, var_identifier)
            parameters.append(var_data)

            while (self._peekType(TokenType.COMMA)):
                self._eat(TokenType.COMMA)

                var_type = self._eat(TokenType.IDENTIFIER).value
                var_identifier = self._eat(TokenType.IDENTIFIER).value
                var_data = (var_type, var_identifier)
                parameters.append(var_data)

        self._eat(TokenType.RPAREN)

        # NOTE: we split a combined funciton declaration and definiiton
        # into two statements
        #
        # 1. Function declaration
        # There are no collisions becaues function can be declared multiple times
        statements = [NodeFuncDecl(func_return_type, func_identifier, parameters)]

        if self._peekType(TokenType.LBRACE):
            # 2. if applicable function definition
            statements.append(NodeFuncDef(func_return_type, func_identifier, parameters, self.block()))
        else:
            self._eat(TokenType.SEMICOLON)

        return NodeStatements(statements)

    # program -> ([statement, funcdecl])*
    def program(self) -> Node:
        statements = []
        while (not self._peekType(TokenType.EOS)):
            statements.append(self.statement())
        return NodeProgram(statements)


    def parse(self) -> Node:
       self._eat(TokenType.BOS)
       node = self.program()
       self._eat(TokenType.EOS)

       return node

class CParser(Parser):
    pass
