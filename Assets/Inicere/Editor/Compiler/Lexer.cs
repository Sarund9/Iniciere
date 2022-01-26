using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Iniciere
{
    public static class Lexer
    {

        private static readonly HashSet<char> s_Operators
            = new HashSet<char>("{}()[].,:;=+-*/%&|^~<>!?");

        private readonly static Dictionary<string, TokenType> s_CompoundOperators
            = new Dictionary<string, TokenType>()
            {

            };

        public static bool Parse(IEnumerable<char> source, Action<Token> addtok)
        {
            var it = source.GetEnumerator();
            bool isIt = it.MoveNext();
            bool lastWasNewLine = false;

            while (isIt)
            {
                while (isIt && it.Current == ' ')
                    isIt = it.MoveNext();

                // Names
                if (IsLetter(it.Current))
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    } while (isIt && (IsLetter(it.Current) || IsNumber(it.Current)));

                    lastWasNewLine = false;
                    addtok(new Token(TokenType.Name, build.ToString()));
                    continue;
                }

                // Numbers
                if (IsNumber(it.Current))
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    } while (isIt && (IsLetter(it.Current) || IsNumber(it.Current)));

                    lastWasNewLine = false;
                    addtok(new Token(TokenType.Number, build.ToString()));
                    continue;
                }

                // Comments
                if (it.Current == '#')
                {
                    var build = new StringBuilder();
                    isIt = it.MoveNext();
                    while (isIt && (it.Current != '\n' && it.Current != '\r'))
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    }

                    lastWasNewLine = false;
                    addtok(new Token(TokenType.Comment, build.ToString()));
                    continue;
                }

                // String Literal
                if (it.Current == '"')
                {
                    var build = new StringBuilder();
                    isIt = it.MoveNext();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!isIt || it.Current == '\n' || it.Current == '\r')
                            return false;
                        //throw new Exception("String Error");

                    } while (it.Current != '"');

                    lastWasNewLine = false;
                    addtok(new Token(TokenType.StringLit, build.ToString()));
                    isIt = it.MoveNext();
                    continue;
                }
                if (it.Current == '\'')
                {
                    var build = new StringBuilder();
                    isIt = it.MoveNext();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!isIt || it.Current == '\n' || it.Current == '\r')
                            return false;
                        //throw new Exception("String Error");

                    } while (it.Current == '\'');

                    lastWasNewLine = false;
                    addtok(new Token(TokenType.StringLit, build.ToString()));
                    isIt = it.MoveNext();
                    continue;
                }

                // New Lines
                if (!lastWasNewLine && Environment.NewLine.Contains(it.Current))
                {
                    lastWasNewLine = true;
                    addtok(new Token(TokenType.NewLine, ""));
                    continue;
                }

                // Operators
                if (s_Operators.Contains(it.Current))
                {
                    var build = new StringBuilder();
                    while (isIt && s_Operators.Contains(it.Current))
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    }
                    if (s_CompoundOperators.TryGetValue(build.ToString(), out var type))
                    {
                        lastWasNewLine = false;
                        addtok(new Token(type, ""));
                    }
                    else
                    {
                        //Console.WriteLine($"== ADDING FULL: '{build}'");
                        foreach (var OPC in build.ToString())
                        {
                            lastWasNewLine = false;
                            addtok(new Token(GetCharOperator(OPC), ""));
                        }
                    }

                    continue;
                }

            }

            return true;
        }

        static TokenType GetCharOperator(char C)
        {
            return C switch
            {
                '{' => TokenType.LeftBracket,
                '}' => TokenType.RightBracket,
                '(' => TokenType.LeftParen,
                ')' => TokenType.RightParen,
                '[' => TokenType.LeftSqBracket,
                ']' => TokenType.RightSqBracket,

                '.' => TokenType.Period,
                ',' => TokenType.Comma,
                ':' => TokenType.Colon,
                ';' => TokenType.Semicolon,
                '=' => TokenType.EqualsSign,
                '+' => TokenType.PlusSign,
                '-' => TokenType.MinusSign,
                '*' => TokenType.Asterisk,
                '/' => TokenType.ForwardSlash,
                '&' => TokenType.Ampersand,
                '|' => TokenType.VerticalBar,
                '^' => TokenType.Caret,
                '~' => TokenType.Tilde,
                '<' => TokenType.LessThanSign,
                '>' => TokenType.GreaterThanSign,
                '!' => TokenType.ExclamationSign,
                '?' => TokenType.QuestionSign,

                _ => throw new Exception("Unknown Char"),
            };
        }

        static bool IsLetter(char C)
        {
            var cat = char.GetUnicodeCategory(C);
            return C == '_'
                || cat == UnicodeCategory.UppercaseLetter
                || cat == UnicodeCategory.LowercaseLetter;
        }
        static bool IsNumber(char C)
        {
            return char.IsNumber(C);
        }
    }

    public struct Token
    {
        public TokenType Type;
        public string Value;

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public string ToPrint()
        {
            var build = new StringBuilder();
            build.Append(Type);
            while (build.Length < 20)
                build.Append(' ');
            build.Append($"= {Value}");
            return build.ToString();
        }
        public override string ToString()
        {
            return $"[{Type}|{Value}]";
        }
    }
    public enum TokenType
    {
        Err = 0,
        /// <summary> End of Template (#/>) </summary>
        EoT,
        /// <summary> Name, Value is String </summary>
        Name,
        /// <summary> Number, Value is String </summary>
        Number,
        /// <summary> String, Value is String </summary>
        StringLit,
        /// <summary> Comment, Value is String </summary>
        Comment,
        /// <summary> NewLine, No Value </summary>
        NewLine,

        // Brackets
        LeftBracket,            // {
        RightBracket,           // }
        LeftParen,              // (
        RightParen,             // )
        LeftSqBracket,          // [
        RightSqBracket,         // ]

        // Operator Characters
        Period,                 // .
        Comma,                  // ,
        Colon,                  // :
        Semicolon,              // ;
        EqualsSign,             // =
        PlusSign,               // +
        MinusSign,              // -
        Asterisk,               // *
        ForwardSlash,           // /
        PercentSign,            // %
        Ampersand,              // &
        VerticalBar,            // |
        Caret,                  // ^
        Tilde,                  // ~
        LessThanSign,           // <
        GreaterThanSign,        // >
        ExclamationSign,        // !
        QuestionSign,           // ?

        // Compound Operators
        OpMacroNode,            // ===
        OpNullAssign,           // ---
    }

}
