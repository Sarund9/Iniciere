using System.Text;

namespace Iniciere
{
    public class TextBuilder
    {
        StringBuilder build;
        int index = 0, select = 1;

        public TextBuilder(StringBuilder build)
        {
            this.build = build;
        }

        public char this[int index]
        {
            get => build[index];
            set => build[index] = value;
        }
        public string this[int i, int lenght]
        {
            get => build.ToString().Substring(i, lenght);
        }

        public bool IsFinished => index >= build.Length;

        public int Lenght => build.Length;

        public char CurrentChar => build[index];
        public string Selection => build.ToString().Substring(index, select);

        public int SelectionCount => select;

        
        public void Next(int count = 1, bool resetSelection = true)
        {
            index += count;
            if (resetSelection)
                select = 1;
        }

        public void Cut()
        {
            build.Remove(index, select);
            select = 1;
            index--;
        }
        public void Append(object obj) => build.Append(obj);
        public void Append(string str) => build.Append(str);
        public void Append(string str, int startIndex, int count) => build.Append(str, startIndex, count);
        public void Append(char c) => build.Append(c);
        public void Append(char c, int repeatCount) => build.Append(c, repeatCount);
        public void Append(char[] str) => build.Append(str);
        public void Append(char[] str, int startIndex, int count) => build.Append(str, startIndex, count);
        
        public void AppendLine(string str) => build.AppendLine(str);
        public void AppendLine() => build.AppendLine();

        public void InsertAt(int index, char c) => build.Insert(index, c);
        public void InsertAt(int index, char[] str) => build.Insert(index, str);
        public void InsertAt(int index, object o) => build.Insert(index, o);
        public void InsertAt(int index, string str) => build.Insert(index, str);
        public void InsertAt(int index, string str, int count) => build.Insert(index, str, count);
        public void InsertAt(int index, char[] str, int startIndex, int charCount) => build.Insert(index, str, startIndex, charCount);
        public void Insert(char c) => build.Insert(index, c);
        public void Insert(char[] str) => build.Insert(index, str);
        public void Insert(object o) => build.Insert(index, o);
        public void Insert(string str) => build.Insert(index, str);
        public void Insert(string str, int count) => build.Insert(index, str, count);
        public void Insert(char[] str, int startIndex, int charCount) => build.Insert(index, str, startIndex, charCount);


        public bool TryGoTo(string sequence, SelectionMode selectMode = SelectionMode.Select, bool exit = false)
        {
            int i = index;
            int cur = 0;
            while (i < build.Length)
            {
                if (build[i] == sequence[cur])
                {
                    cur++;
                    if (cur >= sequence.Length)
                    {
                        switch (selectMode)
                        {
                            case SelectionMode.Set1:
                                select = 1;
                                break;
                            case SelectionMode.Select:
                                select = cur;
                                break;
                            default: break;
                        }
                        return true;
                    }
                }
                else
                {
                    cur = 0;
                }
                i++;
            }
            if (exit)
            {
                index = i;
            }
            return false;
        }

        public bool Reads(string str)
        {
            for (int i = index; i < build.Length; i++)
            {
                if (build[i] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        public enum SelectionMode
        {
            /// <summary> Set the selection to One </summary>
            Set1,
            /// <summary> Dont change the Selection Lenght </summary>
            DontChange,
            /// <summary> Dont change the Selection Lenght </summary>
            Select,
        }
    }
}
