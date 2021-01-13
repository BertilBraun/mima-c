#/usr/bin/env python3

from enum import Enum

class NodeType(Enum):
    INTLITERAL = 0
    PLUS       = 1
    MINUS      = 2
    DIVIDE     = 3
    MULTIPLY   = 4
    MODULO     = 5
    PROGRAM    = 6
    DECL       = 7
    ASSIGN     = 8
    VAR        = 9
    FUNCCALL   = 10
    FUNCDECL   = 11
    FUNCDEF    = 12
    BLOCK      = 13

node_str_repr = {
    NodeType.INTLITERAL : "num",
    NodeType.PLUS       : "+",
    NodeType.MINUS      : "-",
    NodeType.DIVIDE     : "/",
    NodeType.MULTIPLY   : "*",
    NodeType.MODULO     : "%",
    NodeType.PROGRAM    : "prog",
    NodeType.ASSIGN     : "asign",
    NodeType.DECL       : "decl",
    NodeType.VAR        : "var",
    NodeType.FUNCCALL   : "fcall",
    NodeType.FUNCDECL   : "fdecl",
    NodeType.FUNCDEF    : "fdef",
    NodeType.BLOCK      : "block"
}

class Node(object):
    def __init__(self, type : NodeType, value=None, children=None):
        self.type = type
        # an ORDERED list of all children if needed
        self._children = [] if children == None else children
        self.value = value

    def __repr__(self):
        if self.value:
            result = "[{}] ({}): ".format(node_str_repr[self.type], self.value)
        else:
            result = "[{}]: ".format(node_str_repr[self.type])
        for child in self._children:
            result += '\t|'.join(('\n' + "->: " + str(child).lstrip()).splitlines(True))
        return result

class ValueNode(Node):
    def __init__(self, type, value):
        super().__init__(type, value)

class UnaryNode(Node):
    def __init__(self, type : NodeType, node, value = None):
        super().__init__(type, value, [node])
        self.node = node

class BinaryNode(Node):
    def __init__(self, type : NodeType, left_node, right_node, value=None):
        super().__init__(type, value, [left_node, right_node])
        self.left_node = left_node
        self.right_node = right_node

class TenaryNode(Node):
    def __init__(self, type : NodeType, left_node, center_node, right_node, value=None):
        super().__init__(type, value, [left_node, right_node, center_node])
        self.left_node = left_node
        self.right_node = right_node
        self.center_node = center_node

class NaryNode(Node):
    def __init__(self, type, children, value=None):
        super().__init__(type, value, children)

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
