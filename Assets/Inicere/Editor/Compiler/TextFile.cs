using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
        int m_NamespaceCount = 0;

        public void Add(string line)
        {
            if (m_NamespaceCount > 0)
            {
                builder.Append(Tabulate(line));
            }
            else
            {
                builder.Append(line);
            }
        }

        public void AddLine(string line)
        {
            if (m_NamespaceCount > 0)
            {
                builder.Append(Tabulate(line + Environment.NewLine));
            }
            else
            {
                builder.Append(line + Environment.NewLine);
            }
        }

        public static void Test()
        {
            var str = $"{Environment.NewLine}Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3";
            var tab = Tabulate(str);
            Debug.Log($"LINE:{str}\n===\nTABBED:{tab}==={tab.ToDebuggable()}");
        }

        static string Tabulate(string line)
        {
            var build = new StringBuilder(line);
            var it = line.FindNewLines();
            //Debug.Log($"LINE '{line.ToDebuggable()}'");
            int c = 1;
            foreach (var i in it)
            {
                //Debug.Log($"INDEX {i}");
                build.Insert(i + c, '\t');
                c++;
            }
            //
            return build.ToString();
        }

        public void StartNamespace()
        {
            var cfg = IniciereConfig.Instance;
            if (cfg == null)
            {
                Debug.LogError("ERROR: Iniciere Config not Found!");
                return;
            }
            AddLine($"namespace {cfg.projectNamespace}{Environment.NewLine}{{");
            m_NamespaceCount++;
        }

        public void EndNamespace()
        {
            m_NamespaceCount--;
            if (builder[builder.Length - 1] == '\t')
            {
                builder.Remove(builder.Length - 1, 1);
            }
            AddLine("}");
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
