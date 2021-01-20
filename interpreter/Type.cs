using mima_c.ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace mima_c.interpreter
{
    public class RuntimeType
    {
        public enum Type
        {
            Void,

            Int,
            Float,
            Double,

            Char,
            String,

            Struct,
            Array,
            Pointer,
            Function,
        }

        public static RuntimeType Void => new RuntimeType(Type.Void, null);

        public Type type { get; private set; }
        protected dynamic value;

        private bool assignable { get;  set; } = false;

        public RuntimeType(int value) : this(Type.Int, value)
        {
        }

        public RuntimeType(Type type) : this(type, null)
        {
        }
        public RuntimeType(Type type, object value) : this(type, value, false)
        {
        }
        public RuntimeType(Type type, bool assignable) : this(type, null, assignable)
        {
        }

        public RuntimeType(Type type, dynamic value, bool assignable)
        {
            this.type = type;
            this.value = value;
            this.assignable = assignable;
        }

        public void Set(RuntimeType data)
        {
            if (!assignable || data.type != type)
                throw new AccessViolationException();
            value = data.value;
        }

        public T Get<T>()
        {
            if (type == Type.Int && typeof(T) == typeof(int))
                return value;

            if (type == Type.Array)
                return value;
            if (type == Type.Pointer)
                return value;
            if (type == Type.Function)
                return value;

            throw new InvalidCastException();
        }

        public dynamic GetUnderlyingValue_DoNotCallThisMethodUnderAnyCircumstances()
        {
            return value;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", type.ToString(), value);
        }

        public static Type GetTypeFromString(string s)
        {
            if (s == "int")
                return Type.Int;
            if (s == "void")
                return Type.Void;
            if (s.StartsWith('*'))
                return Type.Pointer;

            //throw new TypeAccessException("Type was not defined: " + s);
            return Type.Struct;
        }
        internal void MakeUnAssignable()
        {
            assignable = false;
        }

        internal void MakeAssignable()
        {
            assignable = true;
        }

        internal void SetType(Type type)
        {
            this.type = type;
        }
    }

    class Function : RuntimeType
    {
        public Type returnType { get; }
        public BlockStatements body { get; private set; }
        public List<FuncDecl.Parameter> parameters { get; private set; }

        public Function(Type returnType) : base(Type.Function)
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
        public Variable(Type type) : base(type, true)
        {
            // Set compiler default value based on type?
        }
        public Variable(RuntimeType value) : base(value.type, true)
        {
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

        public Array(Type type, int size) : base(Type.Array, true)
        {
            // Set compiler default value based on type?
            List<RuntimeType> values = new List<RuntimeType>(size);
            for (int i = 0; i < size; i++)
                values.Add(new RuntimeType(type, null, true));

            this.Values = values.ToArray();
        }
        public Array(RuntimeType[] values) : base(Type.Array, true)
        {
            this.Values = values;
        }

        public override string ToString()
        {
            return "[" + Values.ToList().FormatList() + "]";
        }
    }

    class Pointer : RuntimeType
    {
        public int pointerLocation { get; set; } = 0;

        public Pointer(Type type) : this(new RuntimeType(type))
        {
        }
        public Pointer(RuntimeType value) : base(Type.Pointer, true)
        {
            this.Set(new RuntimeType(Type.Pointer, value));
        }
    }
}
