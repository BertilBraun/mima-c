#!/usr/bin/env python3

from mimaAst import *

class Interpreter(object):
    def __init__(self, ast : Node):
        self.ast = ast
        # TODO: Big TODO what about Scope? / How should that be implemented?
        self._functions = {}
        self._variables = {}
        self._variable_types = {}

    def walkValueNode(self, node : ValueNode) -> int:
        if node.type == NodeType.INTLITERAL:
            return int(node.value)
        if node.type == NodeType.FUNCCALL:
            return self._functions[node.value]()
        if node.type == NodeType.FUNCDECL:
            pass # TODO: not interested for python interpreter probably.. right?
        if node.type == NodeType.VAR:
            return self._variables[node.value]
        if node.type == NodeType.DECL:
            # TODO: remove duplicate code?
            self._variable_types[node.value[1]] = node.value[0]
            self._variables[node.value[1]] = 0 # TODO: proper default value based on var_type
            return self._variables[node.value[1]] # TODO: correct?

    def walkUnaryNode(self, node : UnaryNode) -> int:
        if node.type == NodeType.MINUS:
            return - self.walkNode(node.node)
        if node.type == NodeType.DECL:
            self._variable_types[node.value[1]] = node.value[0]
            self._variables[node.value[1]] = self.walkNode(node.node)
            return self._variables[node.value[1]] # TODO: correct?
        if node.type == NodeType.ASSIGN:
            self._variables[node.value] = self.walkNode(node.node)
            return self._variables[node.value[1]] # TODO: correct?
        if node.type == NodeType.FUNCDECL:
            # node.value = func_data = (func_return_type, func_identifier, parameters)
            # TODO: parameters
            self._functions[node.value[1]] = lambda: self.walkNode(node.node)
             # TODO: what should be returned?

    def walkBinaryNode(self, node : BinaryNode) -> int:
        if node.type == NodeType.PLUS:
            return self.walkNode(node.left_node) + self.walkNode(node.right_node)
        if node.type == NodeType.MINUS:
            return self.walkNode(node.left_node) - self.walkNode(node.right_node)
        if node.type == NodeType.MULTIPLY:
            return self.walkNode(node.left_node) * self.walkNode(node.right_node)
        if node.type == NodeType.DIVIDE:
            return self.walkNode(node.left_node) / self.walkNode(node.right_node)
        if node.type == NodeType.MODULO:
            return self.walkNode(node.left_node) % self.walkNode(node.right_node)
        
    def walkTenaryNode(self, node : TenaryNode) -> int:
        pass
        
    def walkNaryNode(self, node : NaryNode) -> int:
        if node.type == NodeType.PROGRAM:
            # TODO: should this be called in extra scope?
            ret = None
            for statement in node._children:
                ret = self.walkNode(statement)
            return ret
             # TODO: what should be returned?
        if node.type == NodeType.BLOCK:
            # TODO: should be called in extra scope
            ret = None
            for statement in node._children:
                ret = self.walkNode(statement)
            return ret
             # TODO: what should be returned?

    def walkNode(self, node: Node):
        node_dispatch = {
            ValueNode  : self.walkValueNode,
            UnaryNode  : self.walkUnaryNode,
            BinaryNode : self.walkBinaryNode,
            TenaryNode : self.walkTenaryNode,
            NaryNode   : self.walkNaryNode,
        }
        return node_dispatch[type(node)](node)

    def interpret(self):
        return self.walkNode(self.ast)