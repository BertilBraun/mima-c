#!/usr/bin/env python3

from mimaAst import Node
from interpreter.mimaSymbols import VariableSign

class Function:
    def __init__(self, ret_type : str, body : Node, parameters):
        self.ret_type = ret_type

        # NOTE: BOTH the body and the parameters will only be set on function DEFINITION
        # (thats because functions can be declared with different parameter variable names)
        self.body = Node
        self.parameters = parameters

    def __repr__(self):
        return str((self.ret_type, self.parameters))
