#!/usr/bin/env python3

from mimaAst import *

class Interpreter(object):
    def __init__(self, ast : Node):
        self.ast = ast

    def walkValueNode(self, node : ValueNode) -> int:
        if (node.node_type == NodeType.INTLITERAL):
            return int(node.value)

    def walkUnaryNode(self, node : UnaryNode) -> int:
        if node.node_type == NodeType.MINUS:
            return - self.walkNode(node.node)

    def walkBinaryNode(self, node : BinaryNode) -> int:
        if node.node_type == NodeType.PLUS:
            return self.walkNode(node.left_node) + self.walkNode(node.right_node)
        if node.node_type == NodeType.MINUS:
            return self.walkNode(node.left_node) - self.walkNode(node.right_node)
        if node.node_type == NodeType.MULTIPLY:
            return self.walkNode(node.left_node) * self.walkNode(node.right_node)
        if node.node_type == NodeType.DIVIDE:
            return self.walkNode(node.left_node) / self.walkNode(node.right_node)
        if node.node_type == NodeType.MODULO:
            return self.walkNode(node.left_node) % self.walkNode(node.right_node)

    def walkNode(self, node: Node):
        node_dispatch = {
            ValueNode  : self.walkValueNode,
            UnaryNode  : self.walkUnaryNode,
            BinaryNode : self.walkBinaryNode
        }
        return node_dispatch[type(node)](node)

    def interpret(self):
        return self.walkNode(self.ast)
