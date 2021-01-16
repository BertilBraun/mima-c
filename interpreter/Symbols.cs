using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c.interpreter
{
    class Function : RuntimeType
    {
        public string returnType { get; }

        public BlockStatements body { get; private set; }
        public List<FuncDecl.Parameter> parameters { get; private set; }

        public Function(string returnType) : base(Type.Function, null)
        {
            this.returnType = returnType;
        }

        public void Define(BlockStatements body, List<FuncDecl.Parameter> parameters)
        {
            this.body = body;
            this.parameters = parameters;
        }
    }

    class Variable : RuntimeType
    {
        public Variable(Type type) : base(type, null, true)
        {
            // Set compiler default value based on type?
        }
        public Variable(RuntimeType value) : base(value.type, true)
        {
            // Set compiler default value based on type?
            this.value.Set(value);
        }
    }

    class Array : RuntimeType
    {
        public RuntimeType[] Values
        {
            get { return Get<RuntimeType[]>(); }
            set
            {
                this.value = value;
                foreach (var val in this.Values)
                    val.MakeAssignable();
            }
        }

        public Array(Type type, int size) : base(Type.Array, null, true)
        {
            // Set compiler default value based on type?
            List<RuntimeType> values = new List<RuntimeType>(size);
            for (int i = 0; i < size; i++)
                values.Add(new RuntimeType(type, null, true));

            this.Values = values.ToArray();
        }
        public Array(RuntimeType[] values) : base(Type.Array, null, true)
        {
            this.Values = values;
        }

        public override string ToString()
        {
            return "[" + Values.ToList().FormatList() + "]";
        }
    }

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

        public VariableSignature(string name)
        {
            this.name = name;
        }

        protected override object _data => name;
    }
}
