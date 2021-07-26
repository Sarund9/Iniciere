﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace Iniciere
{
    public static class StringUtils
    {
        public static string CaptureInBetween(string line, char c = '"')
        {
            bool inside = false;
            string str = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (inside)
                {
                    if (line[i] == c)
                    {
                        return str;
                    }
                    str += line[i];
                }
                else
                {
                    if (line[i] == c)
                        inside = true;
                }

            }
            throw new ArgumentException(
                $"The line does not contain any text encapsulated by '{c}'"
                );
        }

        public static string CaptureAfter(string line, char c = '#', char end = '\n')
        {
            bool inside = false;
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                if (inside)
                {
                    if (line[i] == end)
                    {
                        return str.ToString();
                    }
                    str.Append(line[i]);
                }
                else
                {
                    if (line[i] == c)
                        inside = true;
                }

            }
            throw new ArgumentException(
                $"The line does not contain any text encapsulated by '{c}'"
                );
        }

        public static bool FilterComments(ref string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (BeginsAtWith(line, "//", i))
                {
                    line = line.Substring(0, i);
                    return false;
                }
                if (BeginsAtWith(line, "/*", i))
                {

                    //TODO: Cut if line ends with */
                    int end = line.IndexOf("*/");
                    if (end > 0)
                    {
                        line = line.Substring(0, i) + line.Substring(end);
                        return false;
                    }

                    line = line.Substring(0, i);
                    return true;
                }
            }
            return false;
        }

        public static string FilterAllComments(string text)
        {
            string result = "";
            CommentState state = CommentState.Normal;
            for (int c = 0; c < text.Length; c++)
            {
                switch (state)
                {
                    case CommentState.Normal:
                        Normal(ref c);
                        break;
                    case CommentState.Line:
                        Line(ref c);
                        break;
                    case CommentState.Group:
                        Group(ref c);
                        break;
                    case CommentState.Ignore:
                        Ignore(ref c);
                        break;
                }
            }

            return result;

            // ================= \\
            void Normal(ref int c)
            {
                if (text.BeginsAtWith("//", c))
                {
                    state = CommentState.Line;
                }
                else if (text.BeginsAtWith("/*", c))
                {
                    state = CommentState.Group;
                }
                else if (text[c] == '"')
                {
                    state = CommentState.Ignore;
                    result += text[c];
                }
                else
                {
                    result += text[c];
                }
            }
            void Line(ref int c)
            {
                if (text[c] == '\n')
                {
                    state = CommentState.Normal;
                }
            }
            void Group(ref int c)
            {
                if (text.BeginsAtWith("*/", c))
                {
                    state = CommentState.Normal;
                    c++;
                }
            }
            void Ignore(ref int c)
            {
                if (text[c] == '"')
                {
                    state = CommentState.Normal;
                }
                result += text[c];
            }
        }
        enum CommentState
        {
            Normal,
            Line,
            Group,
            Ignore,
        }
        public static bool BeginsAtWith(this string str, string value, int index)
        {
            int current = 0;
            for (int i = index; i < str.Length; i++)
            {
                if (str[i] == value[current])
                {
                    current++;
                    if (current >= value.Length)
                        return true;
                }
                else return false;
            }
            return false;
        }
        public static bool BeginsAtWithOrWhitespace(this string str, string value, int index)
        {
            int current = 0;
            bool b = true; //TODO: Not allow whitespace between 'value'
            for (int i = index; i < str.Length; i++)
            {
                if (str[i] == value[current])
                {
                    b = false;
                    current++;
                    if (current >= value.Length)
                        return true;
                }
                else if (b && str[i] != ' ')
                {
                    return false;
                }
            }
            return false;
        }
        public static bool BeginsAtWithOrWhitespace(this string str, string value, int index, out int endPos)
        {
            int current = 0;
            bool b = true; //TODO: Not allow whitespace between 'value'
            for (int i = index; i < str.Length; i++)
            {
                if (str[i] == value[current])
                {
                    b = false;
                    current++;
                    if (current >= value.Length)
                    {
                        endPos = i;
                        return true;
                    }
                }
                else if (b && str[i] != ' ')
                {
                    endPos = index;
                    return false;
                }
            }
            endPos = index;
            return false;
        }
        public static bool StartsWithOrWhitespace(this string str, string value) =>
            StartsWithOrWhitespace(str, value, out var _);
        public static bool StartsWithOrWhitespace(this string str, string value, out int endPos)
        {
            int current = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == value[current])
                {
                    current++;
                    if (current >= value.Length)
                    {
                        endPos = i;
                        return true;
                    }
                }
                else if (str[i] != ' ' && str[i] != '\t')
                {
                    endPos = i;
                    return false;
                }
            }
            endPos = 0;
            return false;
        }

        public static bool TryHandleKeyword(
            List<string> lines, TextPos start, string keyword,
            List<TemplateProperty> props,
            out string kw_name, out TextPos end)
        {
            if (!lines[start.l].StartsWithOrWhitespace(keyword, out var endPos))
            {
                kw_name = "";
                end = start;
                return false;
            }

            char nextC = lines[start.l][endPos + 1];
            if (nextC != ' ' && nextC != '"')
            {
                kw_name = "";
                end = start;
                return false;
            }

            //TODO: custom Command Char

            var str = TryHandleParam<string>(lines, start >> (keyword.Length + 1), props, '#', out kw_name, out end);
            //var str = TryHandleString(lines, start >> keyword.Length, '#', out kw_name, out end);
            //ProcessEscapeSequences(ref kw_name, '#');

            Debug.Log($"Keyword '{keyword}' handling resulted in '{kw_name}' "); //at {endPos} char:'{lines[start.l][endPos]}'");
            return str;
        }
        public static bool TryHandleProperty(
            List<string> lines, TextPos start,
            out string typename, out string propname, out TextPos end
            )
        {
            if (!lines[start.l].StartsWithOrWhitespace("in"))
            {
                typename = "";
                propname = "";
                end = start;
                return false;
            }

            typename = "";
            propname = "";
            end = start;

            var words = lines[start.l].Split(
                new char[] { ' ', '\t', '\r' },
                StringSplitOptions.RemoveEmptyEntries
                );

            if (words.Length == 0)
            {
                return false;
            }
            if (words.Length > 0)
            {
                if (words[0] != "in")
                    return false;
                //throw new Exception("Missing type in property declaration");
            }
            if (words.Length < 2)
            {
                throw new Exception("Incomplete In property declaration");
            }
            typename = words[1];
            if (words.Length < 3)
            {
                throw new Exception("Must declare a name after in property");
            }
            if (words[2][0] != '$')
            {
                throw new Exception("in property name specifier must begin with '$'");
            }
            propname = words[2];
            if (words.Length > 3)
            {
                throw new Exception("Syntax Error: expected end of line at property declaration");
            }
            return true;
        }
        #region OBSOLETE
        [Obsolete("This function does nothing")]
        public static bool TryHandleAdd(List<string> lines, int start, out string result, out int skip)
        {
            result = "";
            skip = 0;
            return false;
        }
        [Obsolete("This function does nothing")]
        public static bool TryHandleString(string str, out string result)
        {
            result = "";
            return false;
        }
        [Obsolete("This function does nothing")]
        public static bool TryHandleInput(string line, out Type type, out string nameID)
        {
            type = default;
            nameID = "";
            return false;
        }
        #endregion
        public static bool TryHandleString(
            List<string> lines, TextPos start, char cmdChar,
            out string result, out TextPos end)
        {
            var resultBuild = new StringBuilder();
            bool inside = false;
            bool raw = false;

            int l = start.l, c = start.c;

            while (l < lines.Count)
            {
                while (c < lines[l].Length)
                {
                    if (inside)
                    {
                        //Commands
                        char cmd = lines[l][c];
                        if (!raw && lines[l][c] == cmdChar)
                        {
                            lines[l] = lines[l].Remove(c, 1);
                            cmd = PrcssCmd(lines[l][c]);
                            //c--;
                        }
                        else if (lines[l][c] == '"')
                        {
                            end = new TextPos(l, c);
                            result = resultBuild.ToString();
                            //Debug.LogWarning($"StrUtls: '{lines[start.l]}', {start} to {end}");
                            return true;
                        }
                        resultBuild.Append(cmd);
                    }
                    else
                    {
                        if (lines[l][c] == '"')
                            inside = true;
                        else if (
                            lines[l][c] == '@' &&
                            lines[l].Length > c &&
                            lines[l][c + 1] == '"'
                            )
                        {
                            inside = true;
                            raw = true;
                            c++;
                        }
                    }
                    c++;
                }
                resultBuild.Append('\n');
                c = 0;
                l++;
            }


            //for (l = start.l; l < lines.Count; l++)
            //    for (c = start.c; c < lines[l].Length; c++)
            end = start;
            result = "";
            //Debug.LogWarning($"StrUtls: '{lines[start.l]}', {start} to {end}");
            return false;

            // ================== \\
            static char PrcssCmd(char c)
            {
                return c switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'v' => '\v',
                    '"' => '"',
                    '\'' => '\'',
                    '#' => '#',
                    _ => throw new Exception($"Iniciere: Unknown escape sequence char: '{c}'"),
                };
            }
        }

        static void ProcessEscapeSequences(ref string str, char cmdChar)
        {
            StringBuilder build = new StringBuilder(str);

            for (int i = 0; i < build.Length; i++)
            {
                if (build[i] == cmdChar)
                {
                    if (i == build.Length - 1)
                        throw new Exception("Iniciere: Escape sequence located at end of String");

                    build.Remove(i, 1);
                    build[i] = Prcss(build[i]);
                }
            }

            str = build.ToString();

            static char Prcss(char c)
            {
                switch (c)
                {
                    case 'n':
                        return '\n';
                    case 't':
                        return '\t';
                    case 'v':
                        return '\v';
                    case '"':
                        return '"';
                    case '\'':
                        return '\'';

                    default:
                        throw new Exception($"Iniciere: Unknown escape sequence char: '{c}'");
                }
            }
        }

        public static bool TrySkip(List<string> lines, TextPos start, out TextPos end)
        {
            bool inside = false;
            int l = start.l, c = start.c;

            while (l < lines.Count)
            {
                while (c < lines[l].Length)
                {
                    if (inside)
                    {
                        if (lines[l][c] == '"')
                        {
                            end = new TextPos(l, c);
                            return true;
                        }
                    }
                    else
                    {
                        if (lines[l][c] == '"')
                            inside = true;
                    }
                    c++;
                }
                c = 0;
                l++;
                if (!inside)
                    break;
            }
            end = start;
            return false;
        }

        public static string TrySubstring(this string str, int startIndex, int numChars)
        {
            string result = "";
            for (int i = startIndex; i < startIndex + numChars; i++)
            {
                if (str.Length > i)
                    result += str[i];
                else return result;
            }
            return result;
        }

        public static bool TryHandleMacro(
            List<string> lines, TextPos start, List<TemplateProperty> props,
            Dictionary<string, MacroInstance> macros, MacroContext macroContext,
            out MacroExecutionInstance result, out TextPos end)
        {
            if (!lines[start.l].StartsWithOrWhitespace("#"))
            {
                result = null;
                end = start;
                return false;
            }

            string macroName;
            try {
                macroName = CaptureAfter(lines[start.l], '#', '(');
            } catch {
                throw new Exception($"Iniciere Macro Syntax Error, at {start.l}");
            }

            if (!macros.ContainsKey(macroName))
            {
                throw new Exception($"Iniciere Error: Unknown macro '{macroName}'");
            }

            MacroInstance actualMacro = macros[macroName];

            string inputs = CaptureAfter(lines[start.l], '(', ')');

            //TODO: resolve inputs using TryHandleParams
            var paramInfos = actualMacro.Method.GetParameters();

            TryHandleParamInput(inputs, props, out var objs);
            

            result = new MacroExecutionInstance(actualMacro, objs);
            end = start;
            return true;
        }
        //int i = 2;
        //TextPos pos = start >> (actualMacro.Name.Length + 3);
        //foreach (string inp in inputs)
        //{
        //    if (!paramInfos.InRange(i))
        //    {
        //        throw new Exception($"Parameters for '{actualMacro.Name}' do not fit the input");
        //    }
        //    var type = paramInfos[i].ParameterType;
        //    if (!TryHandleParam(type, lines, pos, props, '#', out object resultValue, out pos))
        //    {
        //        Debug.Log("Failed");
        //        throw new Exception($"Parameters for '{actualMacro.Name}' do not fit the input");
        //    }
        //    i++;
        //}

        public static bool TryHandleParamInput(string input,
            List<TemplateProperty> props, out object[] result)
        {
            string[] split = input.CustomSplit();
            result = new object[split.Length];

            if (split.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].BeginsAtWithOrWhitespace("\"", 0)) // TODO: Raw Strings
                {
                    //Handle as string
                    try
                    {
                        split[i] = CaptureInBetween(split[i]);
                    }
                    catch
                    {
                        throw new Exception($"Error capturing string at \n({input})[{i}]");
                    }
                }
                else
                {
                    //Handle as variable

                    if (TryHandleVariable(split[i], props, out var res))
                    {
                        result[i] = res;
                    }
                    else
                    {
                        throw new Exception($"Variable {split[i]} could not be found");
                    }
                }
            }
            return true;
        }

        public static bool TryHandleParam<T>(
            List<string> lines, TextPos start,
            List<TemplateProperty> props, char cmdChar,
            out T resultValue, out TextPos end)
        {

            if (typeof(T) == typeof(string) && lines[start.l].BeginsAtWithOrWhitespace("\"", start.c)) // TODO: Raw Strings
            {
                //Handle as a string
                bool sucess = TryHandleString(lines, start, cmdChar, out string rstr, out end);
                resultValue = (T)(object)rstr;// Cast<T>(rstr);
                return sucess;
            }



            //TODO: detect expressions (String joins, other variables)

            return TryHandleVariable<T>(lines, start, props, out resultValue, out end);
        }
        public static bool TryHandleParam(Type type,
            List<string> lines, TextPos start,
            List<TemplateProperty> props, char cmdChar,
            out object resultValue, out TextPos end)
        {

            if (type == typeof(string) && lines[start.l].BeginsAtWithOrWhitespace("\"", start.c))
            {
                //Handle as a string
                bool sucess = TryHandleString(lines, start, cmdChar, out string rstr, out end);
                resultValue = rstr;// Cast<T>(rstr);
                return sucess;
            }



            //TODO: detect expressions (String joins, other variables)

            return TryHandleVariable<object>(lines, start, props, out resultValue, out end);
        }

        public static bool TryHandleVariable<T>(
            List<string> lines, TextPos start, List<TemplateProperty> props,
            out T resultValue, out TextPos end )
        {
            //TODO: Compound Objects
            foreach (var prop in props)
            {
                if (lines[start.l].BeginsAtWithOrWhitespace(prop.Name, start.c, out int endPos))
                {
                    try
                    {
                        resultValue = (T)prop.Value;
                        end = new TextPos(start.l, endPos + 1);
                        return true;
                    }
                    catch
                    {
                        throw new Exception($"Iniciere Error: {prop.Type.Name} '{prop.Name}' has no conversion to {typeof(T).Name}");
                    }
                }
            }
            
            resultValue = default;
            end = start;
            return false;
        }

        public static bool TryHandleVariable(string text,
            List<TemplateProperty> props, out object result)
        {
            // TODO: Compound Object
            foreach (var prop in props)
            {
                if (text.BeginsAtWithOrWhitespace(prop.Name, 0))
                {
                    result = prop.Value;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public static string[] CustomSplit(this string str, char split = ',', char halter = '"')
        {
            List<string> result = new List<string>();
            bool halt = false;
            int last = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (halt)
                {
                    if (str[i] == halter)
                        halt = false;
                }
                else
                {
                    if (str[i] == split)
                    {
                        result.Add(str.Substring(last, i - last));
                        i++;
                        last = i;
                    }

                    if (str[i] == halter)
                        halt = true;
                }
            }
            result.Add(str.Substring(last, str.Length - last));

            return result.ToArray();
        }

        public static string RemoveAll(this string str, Predicate<char> predicate)
        {
            var build = new StringBuilder(str);

            int i = 0;
            while (i < build.Length)
            {
                if (predicate(build[i]))
                {
                    build.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }

            return build.ToString();
        }
        public static string RemoveWhitespace(this string str)
        {
            var build = new StringBuilder(str);

            int i = 0;
            while (i < build.Length)
            {
                if (build[i] == ' ')
                {
                    build.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }

            return build.ToString();
        }

        //public static object CastDynamic<T>(this object self)
        //{
        //    var methodInfo = typeof(StringUtils).GetMethod("Cast");
        //    var genericMethod = methodInfo.MakeGenericMethod(typeof(T));
        //    return genericMethod.Invoke(null, new[] { self });
        //}
        //public static object CastDynamic(this object self, Type type)
        //{
        //    var methodInfo = typeof(object).GetMethod("Cast");
        //    var genericMethod = methodInfo.MakeGenericMethod(type);
        //    return genericMethod.Invoke(null, new[] { self });
        //}
        public static object CastDynamic(this object self, Type type)
        {
            var methodInfo = typeof(StringUtils).GetMethod("Cast");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            return genericMethod.Invoke(null, new[] { self });
        }
        public static object CastDynamic<T>(this object self)
        {
            var methodInfo = typeof(StringUtils).GetMethod("Cast");
            var genericMethod = methodInfo.MakeGenericMethod(typeof(T));
            return genericMethod.Invoke(null, new[] { self });
        }
        static T Cast<T>(object o)
        {
            return (T)o;
        }

        public static bool InRange<T>(this T[] array, int i) => i >= 0 && i < array.Length;
        public static bool InRange<T>(this List<T> list, int i) => i >= 0 && i < list.Count;
    }
}