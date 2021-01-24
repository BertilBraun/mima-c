using System.Text.RegularExpressions;

namespace mima_c
{
    class PreProzessor
    {
        public PreProzessor(string inputText)
        {
            InputText = inputText;
        }

        public string InputText { get; }

        internal string GetProcessedText()
        {
            string text = InputText;

            // NOTE: the following implementation has no performance consideration
            // whatsoverever
            //
            //
            // ISO 5.1.1.2
            //
            // 1. Map multibyte characters to source character set
            // e.g. \r\n -> \n
            //
            // 2. Every instance of '\' followed by '\n' is removed
            text = text.Replace("\\\n", "");
            // and the fill will end with a newline
            if (!text.EndsWith('\n'))
                text += '\n';
            //
            // 3. Decompose text into ?preprocessing tokens? and whitespace characters (including comments)
            //
            // Replace every comment with one whitespace character
            //
            // 4. Preprocessing directives / macros and _Pragma
            // On include preprocess the file recursively
            //
            // 5. Convert strings and characters to source character set (including escape sequence)
            //
            // 6. Adjacent string literal tokens are concatenated

            // Quick and dirty replacing of comments

            text = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"//.*", "");

            return text;
        }
    }
}
