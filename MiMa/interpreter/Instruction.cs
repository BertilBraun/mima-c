using System;

namespace CompilerPage.MiMa.interpreter
{
    public interface Instruction
    {
        public void Run(Mima mima);
    }

    struct LDC : Instruction
    {
        Int24 c;
        public LDC(int c) { this.c = (Int24)c; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku = c;
        }
    };

    struct LDV : Instruction
    {
        Int24 a;
        public LDV(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku = mima.M[a];
        }
    };

    struct STV : Instruction
    {
        Int24 a;
        public STV(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.M[a] = mima.Akku;
        }
    };

    struct ADD : Instruction
    {
        Int24 a;
        public ADD(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku += mima.M[a];
        }
    };

    struct AND : Instruction
    {
        Int24 a;
        public AND(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku &= mima.M[a];
        }
    };

    struct OR : Instruction
    {
        Int24 a;
        public OR(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku |= mima.M[a];
        }
    };

    struct XOR : Instruction
    {
        Int24 a;
        public XOR(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku ^= mima.M[a];
        }
    };

    struct EQL : Instruction
    {
        Int24 a;
        public EQL(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            if (mima.Akku == mima.M[a])
                mima.Akku = -1;
            else
                mima.Akku = 0;
        }
    };

    struct JMP : Instruction
    {
        Int24 a;
        public JMP(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.M[Mima.IrAddress] = a;
        }
    };

    struct JMN : Instruction
    {
        Int24 a;
        public JMN(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            if (mima.Akku < 0)
                mima.M[Mima.IrAddress] = a;
        }
    };

    struct HALT : Instruction
    {
        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.M[Mima.IrAddress] = (Int24)mima.Instructions.Count;
        }
    };

    struct NOT : Instruction
    {
        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku = ~mima.Akku;
        }
    };

    struct RAR : Instruction
    {
        // Inherited via instruction
        public void Run(Mima mima)
        {
            int carry = mima.Akku & 1;
            mima.Akku = (Int24)(((mima.Akku >> 1) & ~(3 + 23)) | (carry + 23));
        }
    };

    struct LDIV : Instruction
    {
        Int24 a;
        public LDIV(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.Akku = mima.M[mima.M[a]];
        }
    };

    struct STIV : Instruction
    {
        Int24 a;
        public STIV(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.M[mima.M[a]] = mima.Akku;
        }
    };

    class ReprUtil
    {
        static public string getHexRepr(int v, int lengthInBits)
        {
            return "0x" + v.ToString("X" + (lengthInBits / 4));
        }
        static public string getBinRepr(int v)
        {
            return Convert.ToString(v, 2).PadLeft(24, '0') + "b";
        }
    }

    struct PRINTAKKU : Instruction
    {
        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.WriteLine("Akku:  " + " bin: " + ReprUtil.getBinRepr(mima.Akku) + ": hex: " + ReprUtil.getHexRepr(mima.Akku, 24) + " dez: " + mima.Akku);
        }
    };

    struct PRINT : Instruction
    {
        Int24 a;
        public PRINT(int a) { this.a = (Int24)a; }

        // Inherited via instruction
        public void Run(Mima mima)
        {
            mima.WriteLine(ReprUtil.getHexRepr(a, 20) + " bin: " + ReprUtil.getBinRepr(mima.M[a]) + ": hex: " + ReprUtil.getHexRepr(mima.M[a], 24) + " dez: " + mima.M[a]);
        }
    };
}
