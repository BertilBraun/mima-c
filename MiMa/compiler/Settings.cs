namespace mima_c.compiler
{
    class Settings
    {
        public class Mima
        {
            // Defined in Mima.h
            public const int InstructionPointer = 1048500;
        }

        public const int InitialStackPosition = 1;
        public const int AkkuPosition = -1;
        // Each offset by 4 from InstructionPointer
        public const int StackPointerPosition = 1048504;
        public const int FramePointerPosition = 1048508;
        public const int LastAddrPointerPosition = 1048512;
        public const int PushPopPointerPosition = 1048516;

        public static readonly int[] RegisterPostions =
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
        public static readonly int[] ReturnRegisterPostions =
        {
            1048552,
            1048556,
        };
    }
}
