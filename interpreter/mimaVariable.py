#!/usr/bin/env python3

class Variable:
    # ONLY! uninitialized variables have value None
    def __init__(self, type : str, value=None):
        self.type = type
        self.value = value

    def __repr__(self):
        return repr((self.type, self.value))
