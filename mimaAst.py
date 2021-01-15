#/usr/bin/env python3

from enum import Enum

class Node(object):
    # Every Node can potentially store a value and can have children for some common
    # behavior

    @property
    def _value(self):
        return None

    @property
    def _children(self):
        return []

    def __repr__(self):
        if self._value:
            result = "[{}] ({}): ".format(self._nodename, self._value)
        else:
            result = "[{}]: ".format(self._nodename)
        for child in self._children:
            result += '\t|'.join(('\n' + "->: " + str(child).lstrip()).splitlines(True))
        return result

    @property
    def _nodename(self):
        return type(self).__name__.replace("Node", "")

class NodeValue(Node):
    # I left this as a single node because all values are parsed the same(ish)?
    class Type(Enum):
        INTLITERAL = 0
        STRINGLIT   = 1
        CHARLIT    = 2
        # FLOATLITERAL
        # DOUBLELITERAL
        # STRINGLITERAL

    def __init__(self, type, value):
        self.type = type
        self.value = value

    @property
    def _value(self):
        return (self.type, self.value)

class NodeBinaryArithm(Node):
    def __init__(self, op, left_node, right_node):
        # This could also just be an enum
        assert op in ["+", "-", "/", "*", "%", "<", ">", "<=", ">=", "==", "!="]
        self.op = op
        self.left_node = left_node
        self.right_node = right_node

    @property
    def _value(self):
        return self.op

    @property
    def _children(self):
        return [self.left_node, self.right_node]

    @property
    def _nodename(self):
        return self.op

class NodeUnaryArithm(Node):
    def __init__(self, op, node):
        assert op in ["-"]

        self.op = op
        self.node = node

    @property
    def _value(self):
        return self.op

    @property
    def _children(self):
        return [self.node]

    @property
    def _nodename(self):
        return self.op

class NodeVariable(Node):
    def __init__(self, identifier):
        self.identifier = identifier

    @property
    def _value(self):
        return self.identifier

class NodeVariableDecl(Node):
    def __init__(self, type, identifier, count_expr):
        self.type = type
        self.identifier = identifier
        self.count_expr = count_expr

    @property
    def _value(self):
        return (self.type, self.identifier)

    @property
    def _children(self):
        return [self.count_expr] if self.count_expr else []

class NodeVariableAssign(Node):
    def __init__(self, identifier, node):
        self.identifier = identifier
        self.node = node

    @property
    def _value(self):
        return self.identifier

    @property
    def _children(self):
        return [self.node]

class NodeFuncCall(Node):
    def __init__(self, identifier, arguments):
        self.identifier = identifier
        self.arguments = arguments

    @property
    def _value(self):
        return self.identifier

    @property
    def _children(self):
        return self.arguments

class NodeFuncDecl(Node):
    # I'm still not sure if the parameters shouldn't be a list of nodes
    # The way we do it at the moment what parameters are is undefined
    # (variable_type, identifier)
    def __init__(self, return_type, identifier, parameters):
        self.return_type = return_type
        self.identifier = identifier
        self.parameters = parameters

    @property
    def _value(self):
        return (self.return_type, self.identifier, self.parameters)

class NodeFuncDef(Node):
    def __init__(self, return_type, identifier, parameters, block):
        self.return_type = return_type
        self.identifier = identifier
        self.parameters = parameters

        self.block = block

    @property
    def _value(self):
        return (self.return_type, self.identifier, self.parameters)

    @property
    def _children(self):
        return [self.block]

class NodeArrayAccess(Node):
    def __init__(self, identifier : str, index_expr : Node):
        self.identifier = identifier
        self.index_expr = index_expr

    @property
    def _value(self):
        return self.identifier

    @property
    def _children(self):
        return [self.index_expr]

class NodeArrayLit(Node):
    def __init__(self, value_expr_list : [Node]):
        self.value_expr_list = value_expr_list

    @property
    def _children(self):
        return self.value_expr_list

class NodeStatements(Node):
    def __init__(self, statements : [Node]):
        self.statements = statements

    @property
    def _children(self):
        return self.statements

# For dispatching
class NodeBlockStatements(NodeStatements):
    pass

class NodeProgram(NodeStatements):
    pass

class NodeFor(Node):
    def __init__(self, initialization, condition, loop_execution, body):
        self.initialization = initialization
        self.condition = condition
        self.loop_excution = loop_execution
        self.body = body

    @property
    def _children(self):
        return [self.initialization, self.condition, self.loop_excution, self.body]

class NodeWhile(Node):
    def __init__(self, condition, body):
        self.condition = condition
        self.body = body

    @property
    def _children(self):
        return [self.condition, self.body]

class NodeIf(Node):
    def __init__(self, condition, ifbody, elsebody):
        self.condition = condition
        self.ifbody = ifbody
        self.elsebody = elsebody

    @property
    def _children(self):
        return [self.condition, self.ifbody, self.elsebody]

class NodeBreak(Node):
    pass

class NodeContinue(Node):
    pass

class NodeReturn(Node):
    def __init__(self, return_statement):
        self.return_statement = return_statement

    @property
    def _children(self):
        return [self.return_statement]

class NodeIntrinsic(Node):
    def __init__(self, parameters, type):
        self.parameters = parameters
        self.type = type

    @property
    def _value(self):
        return self.type

    @property
    def _children(self):
        return self.parameters
