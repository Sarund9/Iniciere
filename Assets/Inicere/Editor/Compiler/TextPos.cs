namespace Iniciere
{
    public struct TextPos
    {
        public int l, c;

        public TextPos(int l) : this()
        {
            this.l = l;
        }
        public TextPos(int l, int c)
        {
            this.l = l;
            this.c = c;
        }

        public static TextPos operator +(TextPos a, int i)
        {
            return new TextPos(a.l + i, a.c);
        }
        public static TextPos operator >>(TextPos a, int i)
        {
            return new TextPos(a.l, a.c + i);
        }
        public static TextPos operator -(TextPos a, int i)
        {
            return new TextPos(a.l - i, a.c);
        }
        public static TextPos operator <<(TextPos a, int i)
        {
            return new TextPos(a.l, a.c - i);
        }

        public override string ToString()
        {
            return $"Line: {l}, char: {c}";
        }
    }
}
