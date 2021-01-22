namespace mima_c.compiler
{
    class Settings
    {
        public class Mima
        {
            public static int InstructionPointer = 1048500;
        }

        public static int InitialStackPosition = 0;
        public static int StackPointerPosition = 1048504;
        public static int FramePointerPosition = 1048508;
        public static int[] RegisterPostions = new int[]
        {
            1048512,
            1048516,
            1048520,
            1048524,
            1048528,
            1048532,
            1048536,
            1048540,
        };
    }
}
