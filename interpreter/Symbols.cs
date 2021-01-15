using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace mima_c.interpreter
{
    class Value
    {

    }
    class Function : Value
    {
        string returnType;

        ast.AST body;
        List<ast.FuncDecl.Parameter> parameters;

        public Function(string returnType)
        {
            this.returnType = returnType;
        }

        public void Define(AST body, List<FuncDecl.Parameter> parameters)
        {
            this.body = body;
            this.parameters = parameters;
        }
    }

    class Variable : Value
    {
        string type;
        object value;

        public Variable(string type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", type, value);
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
        string type;
        string name;

        public FunctionParam(string type, string name)
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
