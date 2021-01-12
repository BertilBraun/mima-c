#!/usr/bin/env python3

from preprozessor import Preprozessor
from lexer import Lexer
from parser import Parser

if __name__ == "__main__":
    # TODO: Make this a command line utility using argparse
    # TODO: Use standard unix pipes

    input_text = ""
    with open("src/test.c") as file:
        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()

    lexer = Lexer(preprocessed_text)

    ast =  Parser(lexer.tokenStream()).parse()

    # ??? compile to what?
