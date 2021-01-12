#/usr/bin/env python3

from enum import Enum

class NodeType(Enum):
    INTLITERAL = 0
    PLUS       = 1
    MINUS      = 2
    DIVIDE     = 3
    MULTIPLY   = 4
    MODULO     = 5

node_str_repr = {
    NodeType.INTLITERAL : "num",
    NodeType.PLUS       : "+",
    NodeType.MINUS      : "-",
    NodeType.DIVIDE     : "/",
    NodeType.MULTIPLY   : "*",
    NodeType.MODULO     : "%"
}

class Node(object):
    def __init__(self, node_type : NodeType):
        self.node_type = node_type
        # an ORDERED list of all children if needed
        self._children = []

    def __repr__(self):
        result = "[{}]: ".format(node_str_repr[self.node_type])
        for child in self._children:
            result += '\t|'.join(('\n' + "->: " + str(child).lstrip()).splitlines(True))
        return result

class UnaryNode(Node):
    def __init__(self, node_type : NodeType, node):
        super().__init__(node_type)
        self.node = node
        self._children = [node]

class BinaryNode(Node):
    def __init__(self, node_type : NodeType, left_node, right_node):
        super().__init__(node_type)
        self.left_node = left_node
        self.right_node = right_node
        self._children = [left_node, right_node]

class TenaryNode(Node):
    def __init__(self, node_type : NodeType, left_node, center_node, right_node):
        super().__init__(node_type)
        self.left_node = left_node
        self.right_node = right_node
        self.center_node = center_node
        self._children = [left_node, right_node, center_node]

class ValueNode(Node):
    def __init__(self, node_type, value):
        super().__init__(node_type)
        self.value = value

    def __repr__(self):
        return "[{}]: ({})".format(node_str_repr[self.node_type], self.value)


if __name__ == "__main__":
    ast =  BinaryNode(NodeType.PLUS,
                        ValueNode(NodeType.INTLITERAL, 0),
                        BinaryNode(NodeType.DIVIDE,
                                    BinaryNode(NodeType.MULTIPLY,
                                            ValueNode(NodeType.INTLITERAL, 1),
                                            ValueNode(NodeType.INTLITERAL, 2)
                                            ),
                                    ValueNode(NodeType.INTLITERAL, 2)
                                    )
                        )
    print(ast)
