using System.IO;
using System.Text;

namespace Kernel.Config
{
    public class TextBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private int indent;

        private TextBuilder next;

        public void WriteLine(string text)
        {
            WriteIndents();
            builder.AppendLine(text);
        }

        public void WriteLine(string text, params object[] args)
        {
            WriteIndents();
            builder.AppendFormat(text, args);
            builder.AppendLine();
        }

        public void Write(string text)
        {
            builder.Append(text);
        }

        public void Write(string text, params object[] args)
        {
            builder.AppendFormat(text, args);
        }

        public void Push()
        {
            TextBuilder b = new TextBuilder();

            b.builder = builder;
            b.indent = indent;
            b.next = next;
            next = b;

            builder = new StringBuilder();
            indent = 0;
        }

        public string Pop()
        {
            string result = null;
            TextBuilder b = next;
            if (b != null)
            {
                result = builder.ToString();
                builder = b.builder;
                indent = b.indent;
                next = b.next;
            }
            return result;
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public void LeftPar()
        {
            WriteLine("{");
        }

        public void RightPar()
        {
            WriteLine("}");
        }

        public void NextLine()
        {
            builder.AppendLine();
        }

        public void EmptyLine()
        {
            WriteIndents();
            builder.AppendLine();
        }

        public void Indent()
        {
            indent++;
        }

        public void UnIndent()
        {
            indent--;
        }

        public void WriteToFile(string file)
        {
            File.WriteAllText(file, builder.ToString());
        }

        public void WriteIndents()
        {
            for (int i = 0; i < indent; i++) builder.Append('\t');
        }
    }
}
