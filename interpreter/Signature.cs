using System;
using System.Collections.Generic;
using System.Text;

namespace mima_c.interpreter
{

    class Signature
    {
        protected virtual object _data { get; }

        public override bool Equals(object obj)
        {
            return obj is Signature signature &&
                   EqualityComparer<object>.Default.Equals(_data, signature._data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_data);
        }

        public override string ToString()
        {
            return _data.ToString();
        }
    }

    class FunctionParam : Signature
    {
        RuntimeType.Type type;
        string name;

        public FunctionParam(RuntimeType.Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        protected override object _data => type;
    }
    class FunctionSignature : Signature
    {
        string name;
        List<FunctionParam> parameters;

        // TODO HACK, signature should be based on parameter types, for now only on length of arguments
        public FunctionSignature(string name, int argumentCount)
        {
            this.name = name;
            this.parameters = new List<FunctionParam>(argumentCount);
            for (int i = 0; i < argumentCount; i++)
                parameters.Add(null);
        }
        public FunctionSignature(string name, List<FunctionParam> parameters)
        {
            this.name = name;
            this.parameters = parameters;
        }

        protected override object _data => (name, parameters.Count);
    }
    class VariableSignature : Signature
    {
        string name;
        // TODO HACK, signature should be based on type right?

        public VariableSignature(string name)
        {
            this.name = name;
        }

        protected override object _data => name;
    }
}
