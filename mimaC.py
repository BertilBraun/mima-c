#!/usr/bin/env python3

from mimaPreprozessor import Preprozessor
from mimaLexer import Lexer
from mimaParser import AEParser
# from mimaInterpreter import Interpreter

if __name__ == "__main__":
    # TODO: Make this a command line utility using argparse
    # TODO: Use standard unix pipes

    input_text = ""
    with open("src/test.c") as file:
        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()
    print(preprocessed_text)

    lexer = Lexer(preprocessed_text)
    print("preprosessing done")

    tokenstream = lexer.tokenStream()
    print("lexing done")

    parser = AEParser(tokenstream)
    ast = parser.parse()

    print()
    print("AST: ================================")
    print(ast)
    print("======================")
    print()

    # interpreter = Interpreter(ast)
    # result = interpreter.interpret()

    print("result: " + str(result))
    # ??? compile to what?
