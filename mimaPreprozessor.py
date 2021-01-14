#!/usr/bin/env python3

import re

class Preprozessor(object):
    def __init__(self, text : str):
        self.text = text;

    def getProcessedText(self) -> str:
        text = self.text

        # NOTE: the following implementation has no performance consideration
        # whatsoverever
        #
        #
        # ISO 5.1.1.2
        #
        # 1. Map multibyte characters to source character set
        # e.g. \r\n -> \n
        #
        # 2. Every instance of '\' followed by '\n' is removed
        text = text.replace("\\\n", "")
        # and the fill will end with a newline
        if text[-1] != '\n':
            text += '\n'
        #
        # 3. Decompose text into ?preprocessing tokens? and whitespace characters (including comments)
        #
        # Replace every comment with one whitespace character
        #
        # 4. Preprocessing directives / macros and _Pragma
        # On include preprocess the file recursively
        #
        # 5. Convert strings and characters to source character set (including escape sequence)
        #
        # 6. Adjacent string literal tokens are concatenated

        # Quick and dirty replacing of comments

        text = re.sub("/\*.*?\*/", "", text, re.MULTILINE)
        text = re.sub("//.*", "", text)

        return text
