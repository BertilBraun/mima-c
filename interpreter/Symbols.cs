using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c.interpreter
{
    class Value
    {

    }
    class Function : Value
    {
        public string returnType { get; }

        public BlockStatements body { get; private set; }
        public List<FuncDecl.Parameter> parameters { get; private set; }

        public Function(string returnType)
        {
            this.returnType = returnType;
        }

        public void Define(BlockStatements body, List<FuncDecl.Parameter> parameters)
        {
            this.body = body;
            this.parameters = parameters;
        }
    }

    class Variable : Value
    {
        public RuntimeType value { get; set; }

        public Variable(RuntimeType.Type type)
        {
            // Set compiler default value based on type?
            this.value = new RuntimeType(type, null);
        }
        public Variable(RuntimeType value)
        {
            // Set compiler default value based on type?
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    class Array : Value
    {
        public RuntimeType[] values { get; set; }

        public Array(RuntimeType.Type type, int size)
        {
            // Set compiler default value based on type?
            List<RuntimeType> values = new List<RuntimeType>(size);
            for (int i = 0; i < size; i++)
                values.Add(new RuntimeType(type, null));
            
            this.values = values.ToArray();
        }
        public Array(RuntimeType[] values)
        {
            this.values = values;
        }

        public override string ToString()
        {
            return "[" + values.ToList().FormatList() + "]";
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
