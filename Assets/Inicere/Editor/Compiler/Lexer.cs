using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Iniciere
{
    public static class Lexer
    {

        private static readonly HashSet<char> s_Operators
            = new HashSet<char>("{}()[].,:;=+-*/%&|^~<>!?$#@\\");

        private readonly static Dictionary<string, TokenType> s_CompoundOperators
            = new Dictionary<string, TokenType>()
            {
                { "<#", TokenType.OpTemplateStart },
                { "//>", TokenType.OpTemplateEnd },
                { "\\=/", TokenType.OpTemplateSeparate },
            };

        static readonly Dictionary<string, TokenType> s_ValueKeywords =
            new Dictionary<string, TokenType>()
            {
                { "null", TokenType.Err },
                { "true", TokenType.Err },
                { "false", TokenType.Err },
            };


        struct TokAdder
        {
            private readonly Action<Token> addtok;
            public TokAdder(Action<Token> addtok) : this()
            {
                this.addtok = addtok;
            }

            public bool LastWasNewLine { get; set; }
            public bool LastWas2Tick { get; set; }

            public void Add(in Token tok)
            {
                LastWasNewLine = false;
                LastWas2Tick = false;
                addtok(tok);
            }

        }

        public static async Task<bool> ParseAsync(IEnumerable<char> source, ConcurrentQueue<Token> tokens)
        {
            return await Task.Run(() => Parse(source, tokens));
        }

        public static bool Parse(IEnumerable<char> source, ConcurrentQueue<Token> tokens)
        {
            var it = source.GetEnumerator();
            bool isIt = it.MoveNext();
            
            var tkAdd = new TokAdder(x => tokens.Enqueue(x));

            while (isIt)
            {
                while (isIt && (it.Current == ' ' || it.Current == '\t'))
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

                    tkAdd.Add(new Token(TokenType.Name, build.ToString()));
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

                    tkAdd.Add(new Token(TokenType.Number, build.ToString()));
                    continue;
                }

                // String Literal
                if (it.Current == '"')
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!tkAdd.LastWas2Tick && (!isIt || it.Current == '\n' || it.Current == '\r'))
                        {
                            tkAdd.Add(new Token(TokenType.Err, $"String End of Line Error at[{ build }]"));
                            return false;
                        }
                        //throw new Exception("String Error");

                    } while (it.Current != '"');

                    tkAdd.Add(new Token(TokenType.StringLit, build.ToString()));
                    isIt = it.MoveNext();
                    continue;
                }
                if (it.Current == '\'')
                {
                    var build = new StringBuilder();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();

                        if (!tkAdd.LastWas2Tick && (!isIt || it.Current == '\n' || it.Current == '\r'))
                        {
                            tkAdd.Add(new Token(TokenType.Err, $"String End of Line Error at[{build}]"));
                            return false;
                        }
                        //throw new Exception("String Error");

                    } while (it.Current != '\'');

                    tkAdd.Add(new Token(TokenType.StringLit, build.ToString()));
                    if (build.ToString() == "\'")
                    {
                        tkAdd.LastWas2Tick = true;
                    }
                    isIt = it.MoveNext();
                    continue;
                }

                // New Lines
                if (Environment.NewLine.Contains(it.Current))
                {
                    if (!tkAdd.LastWasNewLine)
                    {
                        tkAdd.Add(new Token(TokenType.NewLine, ""));
                    }
                    isIt = it.MoveNext();
                    continue;
                }

                // New Comment Syntax
                if (it.Current == '#')
                {
                    var build = new StringBuilder();
                    isIt = it.MoveNext();
                    do
                    {
                        build.Append(it.Current);
                        isIt = it.MoveNext();
                    } while (isIt && !IsNewLine(it.Current));

                    tkAdd.Add(new Token(TokenType.Comment, build.ToString()));
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

                    // Compound Operator 
                    if (s_CompoundOperators.TryGetValue(build.ToString(), out var type))
                    {
                        tkAdd.Add(new Token(type, ""));
                    }
                    else
                    {
                        //Console.WriteLine($"== ADDING FULL: '{build}'");
                        foreach (var OPC in build.ToString())
                        {
                            tkAdd.Add(new Token(GetCharOperator(OPC), ""));
                        }
                    }

                    continue;
                }
                
                Debug.LogError($"Unknown Char: {it.Current}");
                return false;
            }
            tkAdd.Add(new Token(TokenType.EoT, ""));

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
                '$' => TokenType.DollarSign,
                '#' => TokenType.HashSign,
                '@' => TokenType.AtSign,

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
        static bool IsNewLine(char C) =>
            Environment.NewLine.Contains(C);
    }

    [Serializable]
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

        public string ToSrc()
        {
            return "[Format Not Supported]";
        }

        public override bool Equals(object obj) =>
            obj is Token tok &&
            Type == tok.Type &&
            Value == tok.Value;

        public static bool operator ==(Token a, Token b) =>
            a.Type == b.Type &&
            a.Value == b.Value;

        public static bool operator !=(Token a, Token b) =>
            a.Type != b.Type ||
            a.Value != b.Value;

    }
    public enum TokenType
    {
        Err = 0,
        /// <summary> End of Template (//>) </summary>
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

        // TODO: Keywords
        KwNull, KwFalse, KwTrue,

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
        DollarSign,             // $
        HashSign,               // #
        AtSign,                 // @
        Backslash,              // \

        // Compound Operators
        OpMacroNode,            // ===
        OpNullAssign,           // ---

        OpTemplateStart,        // <#
        OpTemplateEnd,          // //>
        OpTemplateSeparate,     // \=/

    }

}

#region OLD_CODE

//var build_text = build.ToString();
//// Comment, Single Line, TODO: CHANGE THIS, MUST ACCOUNT FOR Ej: @//
//int commentIndex = build_text.FindAll("//").FirstOr(-1);
//if (commentIndex > -1)
//{
//    // 

//    var cbuild = new StringBuilder();
//    // Append extra Operators
//    foreach (var C in build_text.Take(commentIndex))
//        cbuild.Append(C);

//    // Others
//    isIt = it.MoveNext();
//    while (isIt && (it.Current != '\n' && it.Current != '\r'))
//    {
//        cbuild.Append(it.Current);
//        isIt = it.MoveNext();
//    }

//    tkAdd.Add(new Token(TokenType.Comment, cbuild.ToString()));
//    continue;
//}



#endregion
