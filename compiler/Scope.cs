using System;
using System.Collections.Generic;
using System.Text;

namespace mima_c.compiler
{
    class Scope
    {
        public class TranslationType
        {
            public int size { get; }
            public int addr { get; }

            public TranslationType(int size, int addr)
            {
                this.size = size;
                this.addr = addr;
            }
        }

        public static int stackPointer = Settings.InitialStackPosition;
        private Scope parent;

        private Dictionary<string, List<dynamic>> customTypes;
        // Dictionary of variable names to location from FramePointer
        private Dictionary<string, TranslationType> translation;
        private int framePointer;

        public Scope(Scope parent = null)
        {
            this.parent = parent;
            this.customTypes = new Dictionary<string, List<dynamic>>();
            this.translation = new Dictionary<string, TranslationType>();
            this.framePointer = stackPointer;
        }

        public void AddVariable(string variableName, int size = 1)
        {
            if (translation.ContainsKey(variableName))
                throw new InvalidOperationException("Variable allready defined! Signature: " + variableName);

            translation[variableName] = new TranslationType(size, stackPointer - framePointer);
        }

        public bool IsDefined(string variableName)
        {
            if (translation.ContainsKey(variableName))
                return true;

            return parent != null && parent.IsDefined(variableName);
        }

        public int GetAddr(string variableName)
        {
            if (!IsDefined(variableName))
                throw new InvalidOperationException("Variable not defined! Signature: " + variableName);

            if (translation.ContainsKey(variableName))
                return translation[variableName].addr;

            return parent.GetAddr(variableName);
        }
        public int GetSize(string variableName)
        {
            if (!IsDefined(variableName))
                throw new InvalidOperationException("Variable not defined! Signature: " + variableName);

            if (translation.ContainsKey(variableName))
                return translation[variableName].size;

            return parent.GetSize(variableName);
        }
    }
}
