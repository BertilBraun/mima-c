#!/usr/bin/env python3

import sys
import argparse

from mimaPreprozessor import Preprozessor
from mimaLexer import Lexer
from mimaParser import AEParser
from interpreter.mimaInterpreter import Interpreter

# This import will run all defined Tests
# TODO: remove this import for faster Compile time and production
import testing.mimaTest

if __name__ == "__main__":
    # TODO: Use standard unix pipes
    
    parser = argparse.ArgumentParser(description='C to Mima Compiler')
    
    parser.add_argument('--file', type=str, help='The file which need to be compiled', default="src/test.c")
    parser.add_argument('--no_debug', action='store_true', help='Show all debug output', default=False)

    args = parser.parse_args()

    show_debug = not args.no_debug
    print(show_debug, args)

    with open(args.file) as file:
        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()
    if show_debug:
        print("preprosessing done")
        print(preprocessed_text)

    tokenstream = Lexer(preprocessed_text).tokenStream()
    if show_debug:
        print("lexing done")

    ast = AEParser(tokenstream).parse()
    if show_debug:
        print("parsing done")
        print()
        print("AST: ================================")
        print(ast)
        print("======================")
        print()

    result = Interpreter(ast).interpret()
    if show_debug:
        print()
        print("interpreting done")
        print("result:", result)

    sys.exit(result)
