#!/usr/bin/env python3

from mimaAst import *
from interpreter.mimaSymbols import FuncSign, VariableSign
from interpreter.mimaScope import Scope
import interpreter.mimaStrings as mstrings
from interpreter.mimaVariable import Variable
import sys

class Interpreter(object):
    def __init__(self, ast : Node):
        # The complete ast
        self.ast = ast
        self.global_scope = Scope(None)

    def _raise(self, node : Node, scope : Scope,  error : str):
        # TODO: More information (For that the tokens should be added to all nodes)
        # Also error should be a proper class and not just a string
        print("Lexer error exiting. " + error)
        sys.exit()

    def walkNodeValue(self, node : NodeValue, scope : Scope):
        # No need for explicit error handling. If they can't be converted
        # it's a compiler bug
        if node.type == NodeValue.Type.INTLITERAL:
            return int(node.value)
        if node.type == NodeValue.Type.STRINGLIT:
            return mstrings.escape(node.value)
        if node.type == NodeValue.Type.CHARLIT:
            # BUG: chars can have more then one character BUT this should be checked in the lexer
            return mstrings.escape(node.value)

    def walkNodeBinaryArithm(self, node : NodeBinaryArithm, scope : Scope):
        # TODO: typecheck the arguments and maybe dispatch to different functions depending on type?
        op_to_func = {
            "+" : lambda x, y: x + y,
            "-" : lambda x, y: x - y,
            "/" : lambda x, y: x / y,
            "*" : lambda x, y: x * y,
            "%" : lambda x, y: x % y
        }
        return op_to_func[node.op](self.walkNode(node.left_node, scope), \
                                   self.walkNode(node.right_node, scope))

    def walkNodeVariable(self, node : NodeVariable, scope : Scope):
        var_sign = VariableSign(node.identifier)
        var : Variable = scope.translate(var_sign)
        if not var:
            self._raise(node, scope, "Variable not declared")
        return var.value

    def walkNodeVariableDecl(self, node: NodeVariableDecl, scope : Scope):
        var = Variable(node.type)
        # wrapping this wouldn't strictly be neccessary but it keeps things clean
        var_sign = VariableSign(node.identifier)
        scope.addSymbol(var_sign, var)

    def walkNodeVariableAssign(self, node: NodeVariableAssign, scope : Scope):
        var : Variable = scope.translate(VariableSign(node.identifier))
        if not var:
            self._raise(node, scope, "Variable not declared")
        var.value = self.walkNode(node.node, scope)
        # TODO: check type ? or just in the compiler?

    def walkNodeStatements(self, node : NodeStatements, scope : Scope):
        for s in node.statements:
            self.walkNode(s, scope)

    def walkNode(self, node: Node, scope : Scope):
        node_dispatch = {
            NodeValue  : self.walkNodeValue,
            NodeBinaryArithm : self.walkNodeBinaryArithm,
            NodeVariable : self.walkNodeVariable,
            NodeVariableDecl : self.walkNodeVariableDecl,
            NodeVariableAssign : self.walkNodeVariableAssign,
            NodeStatements : self.walkNodeStatements,
            NodeProgram : self.walkNodeStatements,
        }
        return node_dispatch[type(node)](node, scope)

    def interpret(self):
        ret_val = self.walkNode(self.ast, self.global_scope)
        print(self.global_scope)
        return ret_val

    # def walkUnaryNode(self, node : UnaryNode) -> int:
    #     if node.type == NodeType.MINUS:
    #         return - self.walkNode(node.node)
    #     if node.type == NodeType.DECL:
    #         self._variable_types[node.value[1]] = node.value[0]
    #         self._variables[node.value[1]] = self.walkNode(node.node)
    #         return self._variables[node.value[1]] # TODO: correct?
    #     if node.type == NodeType.ASSIGN:
    #         self._variables[node.value] = self.walkNode(node.node)
    #         return self._variables[node.value[1]] # TODO: correct?
    #     if node.type == NodeType.FUNCDECL:
    #         # node.value = func_data = (func_return_type, func_identifier, parameters)
    #         # TODO: parameters
    #         self._functions[node.value[1]] = lambda: self.walkNode(node.node)
    #          # TODO: what should be returned?

    # def walkBinaryNode(self, node : BinaryNode) -> int:
    #     if node.type == NodeType.PLUS:
    #         return self.walkNode(node.left_node) + self.walkNode(node.right_node)
    #     if node.type == NodeType.MINUS:
    #         return self.walkNode(node.left_node) - self.walkNode(node.right_node)
    #     if node.type == NodeType.MULTIPLY:
    #         return self.walkNode(node.left_node) * self.walkNode(node.right_node)
    #     if node.type == NodeType.DIVIDE:
    #         return self.walkNode(node.left_node) / self.walkNode(node.right_node)
    #     if node.type == NodeType.MODULO:
    #         return self.walkNode(node.left_node) % self.walkNode(node.right_node)

    # def walkTenaryNode(self, node : TenaryNode) -> int:
    #     pass

    # def walkNaryNode(self, node : NaryNode) -> int:
    #     if node.type == NodeType.PROGRAM:
    #         # TODO: should this be called in extra scope?
    #         ret = None
    #         for statement in node._children:
    #             ret = self.walkNode(statement)
    #         return ret
    #          # TODO: what should be returned?
    #     if node.type == NodeType.BLOCK:
    #         # TODO: should be called in extra scope
    #         ret = None
    #         for statement in node._children:
    #             ret = self.walkNode(statement)
    #         return ret
    #          # TODO: what should be returned?
