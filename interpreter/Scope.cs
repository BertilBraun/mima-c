using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace mima_c.interpreter
{
    class Scope
    {
        Scope parent;

        Dictionary<Signature, RuntimeType> translation;

        public Scope(Scope parent)
        {
            this.parent = parent;
            this.translation = new Dictionary<Signature, RuntimeType>();
        }

        public RuntimeType Translate(Signature symbol)
        {
            if (translation.ContainsKey(symbol))
                return translation[symbol];

            if (parent != null)
                return parent.Translate(symbol);

            Debug.Assert(false, "Variable or Function not defined! Signature: " + symbol.ToString());
            return null;
        }

        public bool AddSymbol(Signature symbol, RuntimeType value)
        {
            // Anonymous struct types must not be added
            if (symbol.ToString() == "")
                return false;

            if (translation.ContainsKey(symbol))
            {
                Debug.Assert(false, "Variable allready defined! Signature: " + symbol.ToString());
                return false;
            }
            translation.Add(symbol, value);
            return true;
        }

        public void Add(Scope scope)
        {
            foreach (var item in scope.translation)
                translation.Add(item.Key, item.Value);
        }

        public void MakeScopeToStructVariables(string name)
        {
            foreach (Signature signature in translation.Keys)
                (signature as VariableSignature).name = name + "." + (signature as VariableSignature).name;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Scope:\n");

            foreach (var item in translation)
            {
                builder.Append("({0}: {1})".Format(item.Key, item.Value));
                builder.Append("\n");
            }

            return builder.ToString();
        }
    }
}
