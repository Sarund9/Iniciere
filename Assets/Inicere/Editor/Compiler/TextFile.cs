using System;
using System.Text;

namespace Iniciere
{
    public class TextFile
    {
        readonly StringBuilder builder = new StringBuilder();

        public TextFile(string filename)
        {
            Name = filename;
        }
        public string Name { get; set; }
        //private List<string> Lines { get; } = new List<string>();
        //public string Contents { get; set; }

        public void AddLine(string line)
        {
            //Contents += '\n' + line;
            builder.Append('\n' + line);
        }

        public string GetString() => builder.ToString();

        public void CleanStart()
        {
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] != '\n' && builder[i] != ' ' && builder[i] != '\r')
                {
                    builder.Remove(0, i);
                    break;
                }
            }
        }

        //public TextFile(string filename, string firstLine) : this(filename)
        //{
        //    Lines.Add(firstLine);
        //}
        //public TextFile(string filename, params string[] lines) : this(filename)
        //{
        //    Lines.AddRange(lines);
        //}
        //public string this[int i]
        //{
        //    get => Lines[i];
        //    set => Lines[i] = value;
        //}
        //public int LineCount => Lines.Count;

        //public void InsertLine(int index, string line)
        //{
        //    //Lines.Insert(index, line);
        //}
        //public void RemoveLine(int index)
        //{
        //    Lines.RemoveAt(index);
        //}
        //public string[] GetLines() => Contents.Split('\n');

        //public void AddLineRange(string[] lines)
        //{
        //    Lines.AddRange(lines);
        //}
    }
}
