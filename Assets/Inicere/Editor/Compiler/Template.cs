using System.Collections.Generic;
using System.IO;

namespace Iniciere
{
    public class Template
    {
        readonly List<TextFile> m_Files = new List<TextFile>();

        public string Name { get; set; }
        public string[] FileEx { get; set; }
        public string[] Lang { get; set; }
        public string[] Category { get; set; }
        public string[] Flags { get; set; }

        

        public TextFile this[int i]
        {
            get => m_Files[i];
            set => m_Files[i] = value;
        }

        public int FileCount => m_Files.Count;

        public void AddFile(string filename, string firstLine = "")
        {
            m_Files.Add(new TextFile(filename));
        }

        public TextFile LastFile() => m_Files[m_Files.Count - 1];

        //public void AddLine(string line)
        //{
        //    m_Lines.Add(line);
        //}
        //public void InsertLine(int index, string line)
        //{
        //    m_Lines.Insert(index, line);
        //}
        //public void RemoveLine(int index)
        //{
        //    m_Lines.RemoveAt(index);
        //}
    }

    public class TemplateOutput
    {
        public string Name { get; set; }
        public List<TextFile> Files { get; } = new List<TextFile>();

        public int FileCount => Files.Count;
        public void AddFile(string filename, string firstLine = "")
        {
            Files.Add(new TextFile(filename));
        }
        public TextFile LastFile() => Files[Files.Count - 1];
        public void DumpToFile(string folderPath)
        {
            foreach (var file in Files)
            {
                File.WriteAllText($"{folderPath}/{file.Name}", file.GetString());
            }
        }
    }
}
