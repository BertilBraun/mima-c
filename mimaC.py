#!/usr/bin/env python3

import sys

from mimaPreprozessor import Preprozessor
from mimaLexer import Lexer
from mimaParser import AEParser
from interpreter.mimaInterpreter import Interpreter

# This import will run all defined Tests
# TODO: remove this import for faster Compile time and production
import testing.mimaTest

if __name__ == "__main__":
    # TODO: Use standard unix pipes

        input_text = file.read()

    preprocessed_text = Preprozessor(input_text).getProcessedText()

    sys.exit(result)
