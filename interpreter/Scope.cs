using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace mima_c.interpreter
{
    class Scope
    {
        Scope parent;

        Dictionary<Signature, Value> translation;

        public Scope(Scope parent)
        {
            this.parent = parent;
            this.translation = new Dictionary<Signature, Value>();
        }

        public Value translate(Signature symbol)
        {
            if (translation.ContainsKey(symbol))
                return translation[symbol];

            if (parent != null)
                return parent.translate(symbol);

            Debug.Assert(false, "Variable or Function not defined! Signature: " + symbol.ToString());
            return null;
        }

        public bool addSymbol(Signature symbol, Value value)
        {
            if (translation.ContainsKey(symbol))
            {
                Debug.Assert(false, "Variable allready defined! Signature: " + symbol.ToString());
                return false;
            }
            translation.Add(symbol, value);
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Scope:\n");

            foreach (var item in translation)
            {
                builder.Append(string.Format("({0}: {1})", item.Key, item.Value));
                builder.Append("\n");
            }

            return builder.ToString();
        }
    }
}
