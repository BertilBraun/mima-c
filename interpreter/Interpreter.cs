using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace mima_c.interpreter
{
    class Interpreter
    {
        public Interpreter(AST ast)
        {
            Ast = ast;
        }

        public AST Ast { get; }

        internal int interpret()
        {
            Console.WriteLine("Not implemented!");
            return 0;
        }
    }
}
