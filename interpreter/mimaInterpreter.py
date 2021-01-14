#!/usr/bin/env python3

from mimaAst import *
from interpreter.mimaSymbols import FuncSign, VariableSign, Functionparam
from interpreter.mimaScope import Scope
import interpreter.mimaStrings as mstrings
from interpreter.mimaVariable import Variable
from interpreter.mimaFunction import Function
import sys

class Break(Exception):
    pass

class Continue(Exception):
    pass

class Return(Exception):
    def __init__(self, ret_value):
        self.ret_value = ret_value


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
            "+"  : lambda x, y: x + y,
            "-"  : lambda x, y: x - y,
            "/"  : lambda x, y: x / y,
            "*"  : lambda x, y: x * y,
            "%"  : lambda x, y: x % y,
            ">"  : lambda x, y: 1 if x > y else 0,
            "<"  : lambda x, y: 1 if x < y else 0,
            "<=" : lambda x, y: 1 if x <= y else 0,
            ">=" : lambda x, y: 1 if x >= y else 0,
            "==" : lambda x, y: 1 if x == y else 0,
            "!=" : lambda x, y: 1 if x != y else 0,
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

    def walkNodeFuncDecl(self, node : NodeFuncDecl, scope : Scope):
        func = Function(node.return_type, None, None)
        # NOTE: This is jank ?MAYBE? just use the the Functionparam class of the interpreter?
        # Also note that the variable identifier is not used to build the signature it's just add. info.
        func_sign = FuncSign(node.identifier, [Functionparam(p[0], p[1]) for p in node.parameters])
        scope.addSymbol(func_sign, func)

    def walkNodeFuncDef(self, node : NodeFuncDef, scope : Scope):
        func_sign = FuncSign(node.identifier, [Functionparam(p[0], p[1]) for p in node.parameters])
        func : Function = scope.translate(func_sign)
        if not func:
            self._raise(node, scope, "Function not declared")
        func.body = node.block
        func.parameters = node.parameters

    def walkNodeFuncCall(self, node : NodeFuncCall, scope : Scope):
        # @HACK: passing node.arguments instead of proper parameter list because we know only the length
        # will be used to generate the function signature
        func_sign = FuncSign(node.identifier, node.arguments)
        func : Function = scope.translate(func_sign)
        if not func:
            self._raise(node, scope, "Function not declared")

        copy_scope = Scope(scope)
        for arg_expr, var in zip(node.arguments, func.parameters):
            # TODO: typecheck type (var[0]) agains type of var_value
            var_sign = VariableSign(var[1])
            var_value = Variable(var[0], self.walkNode(arg_expr, scope))
            copy_scope.addSymbol(var_sign, var_value)
        # Don't dispatch because we need to set the scope
        try:
            self.walkNodeBlockStatements(func.body, scope, copy_scope)
        except Return as e:
            # TODO: check type?
            return e.ret_value

    def walkNodeIf(self, node : NodeIf, scope : Scope):
        if self.walkNode(node.condition, scope):
            self.walkNode(node.ifbody, scope)
        else:
            self.walkNode(node.elsebody, scope)

    def walkNodeWhile(self, node : NodeWhile, scope : Scope):
        while self.walkNode(node.condition, scope):
            try:
                self.walkNode(node.body, scope)
            except Break as b:
                break
            except Continue as c:
                pass

    def walkNodeFor(self, node : NodeFor, scope : Scope):
        block_scope = Scope(scope)

        self.walkNode(node.initialization, block_scope)
        while self.walkNode(node.condition, block_scope):
            try:
                self.walkNode(node.body, block_scope)
            except Break as b:
                break
            except Continue as c:
                pass
            self.walkNode(node.loop_excution, block_scope)

    # copy scope to allow function call to insert parameter variables
    def walkNodeBlockStatements(self, node : NodeBlockStatements, scope : Scope, copy_scope : Scope = None):
        block_scope = copy_scope if copy_scope else Scope(scope)

        for statement in node.statements:
            self.walkNode(statement, block_scope)

    def walkNodeStatements(self, node : NodeStatements, scope : Scope):
        for s in node.statements:
            self.walkNode(s, scope)

    def walkNodeBreak(self, node : NodeBreak, scope : Scope):
        raise Break();

    def walkNodeContinue(self, node : NodeContinue, scope : Scope):
        raise Continue();

    def walkNodeReturn(self, node : NodeReturn, scope : Scope):
        raise Return(self.walkNode(node.return_statement, scope));

    def walkNodeProgram(self, node : NodeProgram, scope : Scope):
        self.walkNodeStatements(node, scope)

    def walkNodeIntrinsic(self, node : NodeIntrinsic, scope : Scope):
        if node.type == "printf":
            if len(node.parameters) == 0:
                self._raise(node, scope, "printf needs at least one parameter")
            # TODO: implement proper printf
            params = [self.walkNode(p, scope) for p in node.parameters]
            printfstring = str(params[0]).format(*params[1:])
            print("printf: \"{}\"".format(printfstring))

    def walkNode(self, node: Node, scope : Scope):
        # method_list = [func for func in dir(Interpreter) if callable(getattr(Interpreter, func))]
        method_name = "walk" + type(node).__name__
        method = getattr(Interpreter, method_name)

        return method(self, node, scope)

    def interpret(self):
        self.walkNode(self.ast, self.global_scope)
        main_call_node = NodeFuncCall("main", [])
        return self.walkNode(main_call_node, self.global_scope)
