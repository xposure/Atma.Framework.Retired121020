namespace ecs
{
    using System;
    using System.IO;

    public abstract class OutputWriter : IDisposable
    {
        private int _indent = 0;

        public TextWriter _writer;
        private bool _dispose;

        protected OutputWriter(TextWriter writer, bool dispose)
        {
            _writer = writer;
            _dispose = dispose;
        }

        public void OpenCurly()
        {
            _indent++;
            Write(new string(' ', _indent * 2));
            Write("{{\n");
        }

        public void CloseClury()
        {
            _indent--;
            Write(new string(' ', _indent * 2));
            Write("}\n");
        }

        public void WriteLine(string text)
        {
            Write(new string(' ', _indent * 2));
            Write("}\n");
        }
        private void Write(string text) => _writer.Write(text);

        public void Dispose()
        {
            if (_dispose) _writer.Dispose();
        }
    }

    public class FileWriter : OutputWriter
    {
        public FileWriter(string file) : base(File.CreateText(file), true)
        {

        }
    }

    public class ConsoleWriter : OutputWriter
    {
        public ConsoleWriter() : base(Console.Out, false)
        {

        }
    }
}