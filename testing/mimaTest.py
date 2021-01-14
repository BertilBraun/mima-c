from mimaPreprozessor import Preprozessor
from mimaLexer import Lexer
from mimaParser import AEParser

# Workflow:
#  write code in mimaTest.c
#  make sure the output is correct, 
#  run mimaTest.py
#  in the Future, the code will be run as a test before the program starts

def test(input : str, expected : str):
    
    preprocessed_text = Preprozessor(input).getProcessedText()
    tokenstream = Lexer(preprocessed_text).tokenStream()
    ast = AEParser(tokenstream).parse()

    assert str(ast) == expected, "Test Failed: Input failed to be parsed correctly: \n" + input

def genTest(input : str):
    
    preprocessed_text = Preprozessor(input).getProcessedText()
    tokenstream = Lexer(preprocessed_text).tokenStream()
    ast = AEParser(tokenstream).parse()

    expected = str(ast)

    data = '\n\ntest("""' + input + '""", """' + expected + '""") \n'

    with open("mimaTest.py", "a") as file:
        file.write(data)

if __name__ == "__main__":
    
    with open("mimaTest.c", "r") as file:
        input = file.read()
    
    genTest(input)

test("int a = 0;", """[Program]: 
	|->: [Statements]: 
	|	|->: [VariableDecl] (('int', 'a')): 
	|	|->: [VariableAssign] (a): 
	|	|	|->: [Value] ((<Type.INTLITERAL: 0>, '0')): """) 


test("""int a() {
	return 42;
}""", """[Program]: 
	|->: [Statements]: 
	|	|->: [FuncDecl] (('int', 'a', [])): 
	|	|->: [FuncDef] (('int', 'a', [])): 
	|	|	|->: [BlockStatements]: 
	|	|	|	|->: [Return]: 
	|	|	|	|	|->: [Value] ((<Type.INTLITERAL: 0>, '42')): """) 
