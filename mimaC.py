#!/usr/bin/env python3

from mimaPreprozessor import Preprozessor
from mimaLexer import Lexer
from mimaParser import AEParser
from mimaInterpreter import Interpreter

if __name__ == "__main__":
    # TODO: Make this a command line utility using argparse
    # TODO: Use standard unix pipes

    input_text = ""
    with open("src/test.c") as file:
        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()

    lexer = Lexer(preprocessed_text)

    print(lexer.tokenStream())
    print()

    parser = AEParser(lexer.tokenStream())
    ast = parser.parse()

    print(ast)

    print()

    interpreter = Interpreter(ast)
    result = interpreter.interpret()

    print("result: " + str(result))
    # ??? compile to what?
