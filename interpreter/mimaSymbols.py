#!/usr/bin/env python3

class Signature:
    # always make sure that data is hashable and equatable
    @property
    def _data(self):
        return 0

    def __hash__(self):
        return hash(self._data)

    def __eq__(self, other):
        return self._data == other._data

    def __repr__(self):
        return repr(self._data)

class Functionparam(Signature):
    def __init__(self, type : str, signature : str):
        self.type = type
        self.signature = signature

    @property
    def _data(self):
        # make sure what is returned here is hashable and equatable
        return self.type

class FuncSign(Signature):
    def __init__(self, signature, parameters : (Functionparam)):
        self.signature = signature
        self.parameters = parameters

    @property
    def _data(self):
        # make sure what is returned here is hashable
        return (self.signature, self.parameters)

class VariableSign(Signature):
    def __init__(self, signature : str):
        self.signature = signature

    @property
    def _data(self):
        return self.signature

if __name__ == "__main__":
    va = VariableSign('a')
    vb = VariableSign('a')

    # va = 'a'
    # vb = 'a'

    test_dict = {va : "test"}
    print(test_dict[va])
    print(test_dict[vb])
