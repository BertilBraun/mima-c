using System;
using System.Collections.Generic;
using System.Text;

namespace mima_c
{
    class AEParser
    {
        public AEParser(TokenStream tokenStream)
        {
            TokenStream = tokenStream;
        }

        public TokenStream TokenStream { get; }

        internal AST parse()
        {
            throw new NotImplementedException();
        }
    }
}
