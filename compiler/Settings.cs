namespace mima_c.compiler
{
    class Settings
    {
        public class Mima
        {
            // Defined in Mima.h
            public static int InstructionPointer = 1048500;
        }

        public static int InitialStackPosition = 0;
        public static int AkkuPosition = -1;
        // Each offset by 4 from InstructionPointer
        public static int StackPointerPosition = 1048504;
        public static int FramePointerPosition = 1048508;
        public static int LastAddrPointerPosition = 1048512;
        public static int PushPopPointerPosition = 1048516;

        public static int[] RegisterPostions = new int[]
        {
            1048520,
            1048524,
            1048528,
            1048532,
            1048536,
            1048540,
            1048544,
            1048548,
        };
        public static int[] ReturnRegisterPostions = new int[]
        {
            1048552,
            1048556,
        };
    }
}
