#/usr/bin/env python3

from enum import Enum

class NodeType(Enum):
    INTLITERAL = 0
    PLUS       = 1
    MINUS      = 2
    DIVIDE     = 3
    MULTIPLY   = 4
    MODULO     = 5

class Node(object):
    def __init__(self, node_type : NodeType):
        self.node_type = node_type

class UnaryNode(Node):
    def __init__(self, node_type : NodeType, node):
        super().__init__(node_type)
        self.node = node

class BinaryNode(Node):
    def __init__(self, node_type : NodeType, left_node, right_node):
        super().__init__(node_type)
        self.left_node = left_node
        self.right_node = right_node

class TenaryNode(Node):
    def __init__(self, node_type : NodeType, left_node, center_node, right_node):
        super().__init__(node_type)
        self.left_node = left_node
        self.right_node = right_node
        self.center_node = center_node

class ValueNode(Node):
    def __init__(self, node_type, value):
        super().__init__(node_type)
        self.value = value


# (+) -> INTLITERAL(5)
#     -> INTLITERAL(3)

# "5" = INTLIERAL(5)

# tree = BinaryNode(NODE_ADDITION
#                   , ValueNode(NODE_INTLITERAL, 5)
#                   , ValueNode(NODE_INTLITERAL, 3))

# tree = ValueNode(NODE_INTLITERAL, 5)
