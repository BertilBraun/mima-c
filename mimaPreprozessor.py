#!/usr/bin/env python3

class Preprozessor(object):
    def __init__(self, text : str):
        self.text = text;

    def getProcessedText(self) -> str:
        return self.text
