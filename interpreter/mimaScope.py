#!/usr/bin/env python3

# TODO: add human readable representation of scope
class Scope:
    def __init__(self, parent_scope: 'Scope' ):
        # Parents cope can be None for the global scope
        self._parent_scope : Scope = parent_scope
        self._translation = {}

    def translate(self, symbol):
        if self.hasSymbol(symbol):
            return self._translation[symbol]

        if self._parent_scope:
            return self._parent_scope.translat(symbol)

        # TODO Error? Variable not defined?
        return None

    def addSymbol(self, symbol, value) -> bool:
        # No redeclaration of variables IN THIS scope
        # shadowing variables is allowed
        if self.hasSymbol(symbol):
            return False
        self._translation[symbol] = value
        return True

    def hasSymbol(self, symbol):
        """Returns true if THIS scope knows the symbol"""
        return symbol in self._stranslation

    def __repr__(self):
        return "Scope:\n" + "\n".join(["({}: {})".format(k, v) for k, v in self._translation.items()])
