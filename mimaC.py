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
    parser.add_argument('--show_debug_output', action='store_true', help='Show all debug output')

    args = parser.parse_args()
    # TODO: uncomment here to always show debug output
    # args.show_debug_output = True

    with open(args.file) as file:
        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()
    if args.show_debug_output:
        print("preprosessing done")
        print(preprocessed_text)

    tokenstream = Lexer(preprocessed_text).tokenStream()
    if args.show_debug_output:
        print("lexing done")

    ast = AEParser(tokenstream).parse()
    if args.show_debug_output:
        print("parsing done")
        print()
        print("AST: ================================")
        print(ast)
        print("======================")
        print()

    result = Interpreter(ast).interpret()
    if args.show_debug_output:
        print()
        print("interpreting done")
        print("result:", result)

    sys.exit(result)
