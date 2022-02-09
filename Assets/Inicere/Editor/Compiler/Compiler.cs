﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class Compiler
    {
        #region CONST
        const string TEMPLATE_START = @"<#iniciere";
        const string TEMPLATE_DIV = @"\\===//";
        const string TEMPLATE_END = @"#/>";

        #endregion

        
        public static int Precompile(TemplateLocation templateLocation, in TemplateInfo templateInfo)
        {
            var filteredContents = StringUtils.FilterAllComments(templateLocation.GetInfoContents());
            var lines = new List<string>(filteredContents.Split('\n'));

            if (!templateInfo)
            {
                Debug.LogError("TemplateInfo is null");
                return -1;
            }

            try {
                templateInfo.TmpName = TryStartTemplate(lines[0]);

            }
            catch
            {
                Debug.LogError("Template Name is Invalid");
                return -1;
            }
            
            TryEndTemplate(lines[lines.Count - 1]);
            



            //USING STATEMENTS
            List<string> includes = new List<string>();

            Dictionary<string, DecoratorTypeInstance> decorators = new Dictionary<string, DecoratorTypeInstance>();
            
            DecoratorContext decoContext;
            foreach (var item in 
                DecoratorContext.InitializeDecoratorSystem(AppDomain.CurrentDomain, out decoContext))
                    decorators.Add(item.Name, item);

            Queue<DecoratorExecInstance> decoratorQueue = new Queue<DecoratorExecInstance>();

            //REFLECTIONS
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes());

            //Debug.Log($"Starts Compiling: '{templateInfo.Name}'");

            //for (int l = 0; l < lines.Count; l++)
            //{
            //    bool checkForSkip = true;
            //    //FILEEXT KEYWORD
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "fileext", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            var rs = result.Split(
            //                new char[] {' ',',' },
            //                StringSplitOptions.RemoveEmptyEntries);
            //            for (int i = 0; i < rs.Length; i++)
            //                templateInfo.FileExts.Add(rs[i]);

            //            //Debug.Log($"FILE EX: '{result}'");
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //LANGUAGE KEYWORD
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "language", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            var rs = result.Split(
            //                new char[] { ' ', ',' },
            //                StringSplitOptions.RemoveEmptyEntries);
            //            for (int i = 0; i < rs.Length; i++)
            //                templateInfo.Langs.Add(rs[i]);

            //            //Debug.Log($"LANGUAGE: '{result}'");
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //CATEGORY KEYWORD
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "category", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            var rs = result.Split(
            //                new char[] { ' ', ',' },
            //                StringSplitOptions.RemoveEmptyEntries);
            //            for (int i = 0; i < rs.Length; i++)
            //                templateInfo.Categories.Add(rs[i]);

            //            //Debug.Log($"CATEGORY: '{result}'");
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //FLAGS KEYWORD
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "flags", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            var rs = result.Split(
            //                new char[] { ' ', ',' },
            //                StringSplitOptions.RemoveEmptyEntries);
            //            for (int i = 0; i < rs.Length; i++)
            //                templateInfo.Flags.Add(rs[i]);

            //            //Debug.Log($"FLAGS: '{result}'");
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //USING KEYWORD
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "using", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            if (!ResolveNamespace(result, types))
            //            {
            //                Debug.LogError($"Iniciere Error: namespace '{result}' could not be found");
            //                return -1;
            //            }

            //            includes.Add(result);
            //            //Debug.Log($"USING: '{result}'");
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //(OLD)IN KEYWORD (PROPERTIES)
            //    #region ...
            //    //{
            //    //    if (StringUtils.TryHandleProperty(lines, new TextPos(l),
            //    //        out var typename, out var varname, out var end))
            //    //    {
            //    //        if (!TryResolveType(typename, includes, out var type))
            //    //        {
            //    //            Debug.LogError($"TYPE '{typename}' could not be found");
            //    //            foreach (var item in includes)
            //    //            {
            //    //                Debug.LogError($"Including: '{item}'");
            //    //            }
            //    //            return -1;
            //    //        }

            //    //        templateInfo.Properties.Add(new TemplateProperty(varname)
            //    //        {
            //    //            Type = type,
            //    //            Value = null,
            //    //        });
            //    //        //Debug.Log($"PROPERTY: '{varname}' ({typename})");
            //    //        l = end.l;
            //    //        checkForSkip = false;
            //    //        continue;
            //    //    }
            //    //}
            //    #endregion
            //    //DECORATORS
            //    {
            //        if (StringUtils.TryHandleDecorator(lines, new TextPos(l),
            //            decoContext, templateInfo.Properties,
            //            decorators, out DecoratorExecInstance execInstance, out TextPos end))
            //        {
            //            //Debug.Log($"DECORATOR AT [{l}]: {execInstance.Decor.Name}");
            //            decoratorQueue.Enqueue(execInstance);
            //        }

            //    }
            //    //VAR KEYWORD (PROPERTIES)
            //    {
            //        if (StringUtils.TryHandleDynamicProperty(lines, new TextPos(l),
            //            out var varname, out var end))
            //        {
            //            var prop = new TemplateProperty(varname, templateInfo)
            //            {
            //                Value = null,
            //                //LitValue = "null",
            //            };
                        
            //            decoContext.Prepare(prop);

            //            while (decoratorQueue.Count > 0)
            //            {
            //                var exec = decoratorQueue.Dequeue();
                            
            //                if (!exec.Execute(decoContext))
            //                    return -1;
            //            }


            //            templateInfo.Properties.Add(prop);
            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //SDESC KEYWORD (DESCRIPTION)
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "sdesc", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            templateInfo.ShortDescription += result;

            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //LDESC KEYWORD (DESCRIPTION)
            //    {
            //        if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "ldesc", templateInfo.Properties,
            //            out var result, out var end))
            //        {
            //            templateInfo.LongDescription += result;

            //            l = end.l;
            //            checkForSkip = false;
            //            continue;
            //        }
            //    }
            //    //STRING SKIPPING
            //    {
            //        if (checkForSkip && StringUtils.TrySkip(lines, new TextPos(l), out var end))
            //        {
            //            //Debug.Log($"SKIPPING FROM - TO:\n" +
            //            //    $"{l}: {m_Lines[l]}\n" +
            //            //    $"{end.l}: {m_Lines[end.l]}\n");
            //            //Debug.Log($"Skipping {l} to {end.l}");
            //            l = end.l;
            //        }
            //    }
            //}

            //templateInfo.Contents = string.Join(Environment.NewLine, lines);

            Debug.Log($"Template Created {templateInfo.TmpName}");
            EditorUtility.SetDirty(templateInfo);

            return 0;

            // ================ \\
            static string TryStartTemplate(string line)
            {
                if (!line.StartsWith(TEMPLATE_START))
                {
                    throw new Exception($"Template code provided has invalid start: \n {line}");
                }
                return StringUtils.CaptureInBetween(line);
                //template.Name = name;
            }
            static void TryEndTemplate(string line)
            {
                if (!line.StartsWith(TEMPLATE_END))
                {
                    throw new Exception($"Template code provided has invalid end: \n {line}");
                }
            }
            #region OLD
            //bool TryApplyDecorators(TemplateProperty prop, int l)
            //{
            //    while (decoratorQueue.Count > 0)
            //    {
            //        var ex = decoratorQueue.Dequeue();

            //        decoContext.Prepare(prop);

            //        var inputs = new object[ex.Decor.ParamCount + 1];
            //        for (int i = 0; i < ex.Params.Length; i++)
            //        {
            //            if (i == ex.Decor.ParamCount - 1 && ex.Decor.UsesParams)
            //            {
            //                int paramsLenght = ex.Params.Length - ex.Decor.ParamCount + 1;
            //                var paramArgument = new object[paramsLenght];
            //                for (int p = 0; p < paramsLenght; p++)
            //                {
            //                    paramArgument[p] = ex.Params[i + p];
            //                }
            //                inputs[i + 1] = paramArgument;
            //                break;
            //            }
            //            inputs[i + 1] = ex.Params[i];

            //            //bool InRange(int i) => i >= 0 && i < ex.Macro.ParamCount;
            //            //bool Last() => i == ex.Macro.ParamCount - 1;
            //        }
            //        inputs[0] = decoContext;
            //        try
            //        {
            //            //Debug.LogWarning($"EXECUTING");
            //            ex.Decor.Method.Invoke(null, inputs);
            //        }
            //        catch (Exception excep)
            //        {
            //            Debug.LogError(
            //            $"Iniciere Error: decorator '{ex.Decor.Name}' at {l}" +
            //            $"has incorrect Inputs \n{excep.Message}");
            //            return false;
            //        }
            //    }
            //    return true;
            //}
            #endregion
        }

        private static bool ResolveNamespace(string result, IEnumerable<Type> types)
        {
            return types
                .Any(t => t.Namespace == result);
        }

        private static bool TryResolveType(string typename, List<string> includes, out Type type)
        {
            type = ResolveAsPrimitive();

            //TODO: dynamic type
            
            if (type is null)
                type = Type.GetType(typename);

            if (type is object)
            {
                if (type.IsAbstract)
                    throw new Exception($"Cannot create a property of abstract type {typename}");
                
                return true;
            }

            List<Type> possibles = new List<Type>();
            for (int i = 0; i < includes.Count; i++)
            {
                string incl = includes[i];

                var found = Type.GetType($"{incl}.{typename}");
                if (found is object)
                    possibles.Add(found);
            }
            if (possibles.Count == 0)
                return false;

            if (possibles.Count > 0)
            {
                type = possibles[0];
                if (possibles.Count > 1)
                    Debug.LogWarning($"Iniciere Warning: {typename} is an Ambigous reference");
                return true;
            }

            return false;
            // ============== \\
            Type ResolveAsPrimitive() => typename switch
            {
                "bool" => typeof(bool),
                "byte" => typeof(byte),
                "sbyte" => typeof(sbyte),
                "char" => typeof(char),
                "decimal" => typeof(decimal),
                "double" => typeof(double),
                "float" => typeof(float),
                "int" => typeof(int),
                "uint" => typeof(uint),
                "long" => typeof(long),
                "ulong" => typeof(ulong),
                "short" => typeof(short),
                "ushort" => typeof(ushort),

                "string" => typeof(string),
                "object" => typeof(object),

                _ => null,
            };
        }

        public static int Compile(TemplateInfo info, out TemplateOutput output)
        {
            output = new TemplateOutput
            {
                Name = info.TmpName
            };

            Queue<MacroExecutionInstance> macroQueue = new Queue<MacroExecutionInstance>();
            var allMacros =
                MacroContext.InitializeMacroSystem(
                    AppDomain.CurrentDomain, out MacroContext macroContext);

            Dictionary<string, MacroTypeInstance> macros = new Dictionary<string, MacroTypeInstance>();
            foreach (var item in allMacros)
            {
                try
                {
                    macros.Add(item.Name, item);
                }
                catch
                {
                    Debug.LogError($"ERROR, Repeated Macro Name: {item.Name}");
                    return -1;
                }
            }


            var lines = new List<string>(info.GetInfoContents().Split('\n'));

            /*
            for (int l = 0; l < lines.Count; l++)
            {
                bool checkForSkip = true;
                //Debug.Log($"LINE[{l}]");
                //MACROS
                {
                    if (l < lines.Count - 1 && StringUtils.TryHandleMacro(lines, new TextPos(l), info.Properties, macros, macroContext,
                        out MacroExecutionInstance macroExInstance, out TextPos end))
                    {
                        //output.AddFile(name);
                        macroQueue.Enqueue(macroExInstance);
                        //Debug.Log($"MACRO: '{macroExInstance.Macro.Name}'");
                        checkForSkip = false;
                        l = end.l;
                        continue;
                    }
                }

                //FILE KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "file", info.Properties,
                        out var name, out var end))
                    {
                        var build = new StringBuilder(name);
                        if (!TryApplyMacros(build, l))
                            return -1;

                        output.AddFile(build.ToString());
                        //Debug.Log($"FILE: '{name}'");
                        checkForSkip = false;
                        l = end.l;
                        continue;
                    }
                }

                //ADD KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "add", info.Properties,
                        out string body, out TextPos end))
                    {
                        if (output.FileCount == 0)
                        {
                            Debug.LogError("cannot add lines to no file");
                            return -1;
                        }
                        //Debug.Log($"ADD:\n{body}");
                        //TODO: run macros
                        var build = new StringBuilder(body);

                        if (!TryApplyMacros(build, l))
                            return -1;
                        
                        output.LastFile().AddLine(build.ToString());
                        checkForSkip = false;
                        l = end.l;
                        continue;
                    }
                }

                //ADDLN KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "addln", info.Properties,
                        out string body, out TextPos end))
                    {
                        if (output.FileCount == 0)
                        {
                            Debug.LogError("cannot add lines to no file");
                            return -1;
                        }
                        //Debug.Log($"ADDLN:\n{body}");
                        //TODO: run macros
                        var build = new StringBuilder(body);
                        build.Append(Environment.NewLine);
                        if (!TryApplyMacros(build, l))
                            return -1;

                        output.LastFile().AddLine(build.ToString());
                        checkForSkip = false;
                        l = end.l;
                        continue;
                    }
                }

                //STRING SKIPPING
                {
                    if (checkForSkip && StringUtils.TrySkip(lines, new TextPos(l), out var end))
                    {
                        //Debug.Log($"SKIPPING FROM - TO:\n" +
                        //    $"{l}: {lines[l]}\n" +
                        //    $"{end.l}: {lines[end.l]}\n");
                        l = end.l;
                    }
                }
            } //*/

            //POST-COMPILATION
            foreach (var finalFile in output.Files)
            {
                finalFile.CleanStart();
            }


            return 0;

            bool TryApplyMacros(StringBuilder build, int l)
            {
                while (macroQueue.Count > 0)
                {
                    var ex = macroQueue.Dequeue();

                    var inputs = new object[ex.Macro.ParamCount + 2];
                    for (int i = 0; i < ex.Params.Length; i++)
                    {
                        if (i == ex.Macro.ParamCount - 1 && ex.Macro.UsesParams)
                        {
                            int paramsLenght = ex.Params.Length - ex.Macro.ParamCount + 1;
                            var paramArgument = new object[paramsLenght];
                            for (int p = 0; p < paramsLenght; p++)
                            {
                                paramArgument[p] = ex.Params[i + p];
                            }
                            inputs[i + 2] = paramArgument;
                            break;
                        }
                        inputs[i + 2] = ex.Params[i];

                        //bool InRange(int i) => i >= 0 && i < ex.Macro.ParamCount;
                        //bool Last() => i == ex.Macro.ParamCount - 1;
                    }
                    inputs[0] = build;
                    inputs[1] = macroContext;
                    try
                    {
                        //Debug.LogWarning($"EXECUTING");
                        ex.Macro.Method.Invoke(null, inputs);
                    }
                    catch (Exception excep)
                    {
                        Debug.LogError($"Iniciere Error: macro '{ex.Macro.Name}' at {l} has incorrect Inputs \n {excep.Message}");
                        return false;
                    }
                }
                return true;
            }
        }
        #region OLD_CODE
        //while (macroQueue.Count > 0)
        //{
        //    var ex = macroQueue.Dequeue();
        //    var inputs = new object[ex.Macro.ParamCount + 2];
        //    for (int i = 0; i < ex.Params.Length; i++)
        //    {
        //        if (i == ex.Macro.ParamCount - 1 && ex.Macro.UsesParams)
        //        {
        //            int paramsLenght = ex.Params.Length - ex.Macro.ParamCount + 1;
        //            var paramArgument = new object[paramsLenght];
        //            for (int p = 0; p < paramsLenght; p++)
        //            {
        //                paramArgument[p] = ex.Params[i + p];
        //            }
        //            inputs[i + 2] = paramArgument;
        //            break;
        //        }
        //        inputs[i + 2] = ex.Params[i];
        //        //bool InRange(int i) => i >= 0 && i < ex.Macro.ParamCount;
        //        //bool Last() => i == ex.Macro.ParamCount - 1;
        //    }
        //    inputs[0] = build;
        //    inputs[1] = macroContext;
        //    try
        //    {
        //        Debug.LogWarning($"EXECUTING");
        //        ex.Macro.Method.Invoke(null, inputs);
        //    }
        //    catch (Exception excep)
        //    {
        //        Debug.LogError($"Iniciere Error: macro '{ex.Macro.Name}' at {l} has incorrect Inputs \n {excep.Message}");
        //        return -1;
        //    }
        //}
        #endregion

        #region OLD_COMPILER
        //public int Compile(TemplateInfo info, out Template template)
        //{
        //    //result = new List<string>(m_Lines.Count);

        //    bool b_InTemplate = false; //Indicates if we are inside a template
        //    template = new Template(); //If inside a template, the name of it

        //    try {
        //        template.Name = TryStartTemplate(lines[0]);
        //        Debug.LogWarning($"NAME: {template.Name}");
        //    }
        //    catch
        //    {
        //        Debug.LogError("No template Name");
        //        return -1;
        //    } //TEMPLATE NAME

        //    if (!b_InTemplate)
        //    {
        //        Debug.LogError("No template Name");
        //        return -1; //TEMPLATE NOT FOUND
        //    }

        //    Queue<MacroInstance> macroQueue = new Queue<MacroInstance>();
        //    var allMacros =
        //        MacroContext.InitializeMacroSystem(
        //            AppDomain.CurrentDomain, out MacroContext macroContext);



        //    Debug.Log($"Starts Compiling: '{template.Name}'");
        //    for (int l = 0; l < lines.Count; l++)
        //    {
        //        //if (!b_InTemplate)
        //        //{
        //        //    try {
        //        //        TryStartTemplate(m_Lines[l]);
        //        //    }
        //        //    catch { return -1; }
        //        //    continue;
        //        //}

        //        //TODO: detect and add macros to list (and cache for future compilation)


        //        //TODO: detect and add inputs to property system

        //        //FILE KEYWORD
        //        {
        //            if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "file", info.Properties,
        //                out var name, out var end))
        //            {
        //                template.AddFile(name);
        //                Debug.Log($"FILE: '{name}'");
        //                l = end.l;
        //                continue;
        //            }
        //        }

        //        //ADD KEYWORD
        //        {
        //            if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "add", info.Properties,
        //                out var body, out var end))
        //            {
        //                if (template.FileCount == 0)
        //                {
        //                    Debug.LogError("cannot add lines to no file");
        //                    return -1;
        //                }
        //                template.LastFile().AddLine(body);
        //                Debug.Log($"\t ADD: '{body.TrySubstring(0, 20)}'");
        //                //TODO: run macros
        //                l = end.l;
        //                continue;
        //            }
        //        }
        //        //LANGUAGE KEYWORD
        //        //{
        //        //    if (StringUtils.TryHandleKeyword(m_Lines, new TextPos(l), "language",
        //        //        out var name, out var end))
        //        //    {

        //        //    }

        //        //}

        //        //for (int c = 0; c < m_Lines[l].Length; c++)
        //        //{

        //        //    //m_Lines[i][c];
        //        //}
        //    }



        //    return 0;
        //    // =================== \\

        //    string TryStartTemplate(string line)
        //    {
        //        if (line.StartsWith(TEMPLATE_START))
        //        {
        //            b_InTemplate = true;
        //        }
        //        return StringUtils.CaptureInBetween(line);
        //        //template.Name = name;
        //    }
        //}
        #endregion

    }

    public struct LogEntry
    {
        public LogEntry(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }
        public LogLevel Level { get; }
        public string Message { get; }
    }
    public enum LogLevel
    {
        Msg,
        Wrn,
        Err,
    }
    
    public static class NewCompiler
    {
        static List<LogEntry> s_Log = new List<LogEntry>();

        public static void PrintLog(Action<LogEntry> logmethod)
        {
            lock (s_Log)
            {
                for (int i = 0; i < s_Log.Count; i++)
                {
                    logmethod(s_Log[i]);
                }
                s_Log.Clear();
            }
        }
        static void LogMsg(string msg) => s_Log.Add(new LogEntry(LogLevel.Msg, msg));
        static void LogWrn(string msg) => s_Log.Add(new LogEntry(LogLevel.Wrn, msg));
        static void LogErr(string msg) => s_Log.Add(new LogEntry(LogLevel.Err, msg));

        /*
         TODO TemplateInfo values:
        * Name          |
        * Langs         |
        * Categories    |
        * Flags         |
        * FileExts      |
        * Properties
        * FileNameProperty
        * ShortDescription  |
        * LongDescription   |
        TODO:
        * AST
        * Value Resolution
         */

        public static int Precompile(TemplateLocation templateLocation, TemplateInfo r_templateInfo)
        {
            var contents = templateLocation.GetInfoContents(); // TODO: Get IEnumerator<char> from File at Location

            var tokens = new ConcurrentQueue<Token>();
            
            var log = new StringBuilder(" === LOG === ");
            var tklog = new StringBuilder();
            tklog.AppendLine("============ TOKENS ============");
            tklog.AppendLine("================================");
            
            var task = Lexer.ParseAsync(contents, tokens);

            // I dont know why the above function wont run unless we wait for 1 ms
            Thread.Sleep(1);

            var current = new Token();
            
            int linecount = 0;
            bool AwaitDequeue(bool log = false)
            {
                if (log)
                    Debug.Log($"{tokens.Count} < 1 && {!task.IsRunning()}");
                
                if (tokens.Count < 1 && !task.IsRunning())
                {
                    return false;
                }

                while (!tokens.TryDequeue(out current) || current.Type == TokenType.Comment)
                {
                    if (tokens.Count < 1 && !task.IsRunning())
                    {
                        break;
                    }
                    if (task.IsResultFalse())
                    {
                        var d = 0;
                    }
                }
                tklog.AppendLine(current.ToString());

                //if (current.Type == TokenType.OpTemplateSeparate)
                //{
                //    Debug.Log("FOUND");
                //}

                if (current.Type == TokenType.NewLine)
                    linecount++;
                
                if (log)
                    Debug.Log($"Logging {current}");

                if (current.Type == TokenType.Err)
                {
                    Log(current.Value);
                    return false;
                }

                
                return true;
            }

            // TODO: Throw Errors in Each
            if (!AwaitDequeue()) {
                Log($"Template is Empty!");
                return -1;
            }

            //Debug.Log($"SETTING NAME TO {current}");
            if (!StringUtils.TryParse(current.Value, out var tmpName)) {
                Log($"Unable to parse Template Name: {current.Value}");
                return -1;
            }
            r_templateInfo.name = tmpName;
            r_templateInfo.TmpName = tmpName;
            AwaitDequeue(); // -> Template Begins...
           
            Dictionary<string, DecoratorTypeInstance> decorators = new Dictionary<string, DecoratorTypeInstance>();
            DecoratorContext decoContext;
            foreach (var item in
                DecoratorContext.InitializeDecoratorSystem(AppDomain.CurrentDomain, out decoContext))
                decorators.Add(item.Name, item);

            var decoratorQueue = new Queue<DecoratorExecInstance>();


            while (current.Type != TokenType.OpTemplateSeparate &&
                current.Type != TokenType.EoT)
            {
                // Ignore Comments & NewLines
                while (current.Type == TokenType.NewLine)
                {
                    AwaitDequeue();
                    if (current.Type == TokenType.OpTemplateSeparate ||
                        current.Type == TokenType.EoT) 
                    {
                        break;
                    }
                }
                
                // Catch Expressions
                var buffer = new List<Token>();
                do
                {
                    // TODO: Catch '()', Scopes: '{}', Scoped Expressions
                    if (current.Type == TokenType.NewLine ||
                        current.Type == TokenType.Semicolon ||
                        current.Type == TokenType.OpTemplateSeparate ||
                        current.Type == TokenType.EoT)
                    {
                        if (buffer.Count == 0)
                            continue;

                        // END EXPRESSION, PARSE
                        if (TryParseExpression(buffer))
                        {
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    buffer.Add(current);
                } while (AwaitDequeue());

            }

            tklog.AppendLine("================================");

            Debug.Log(tklog);
            Debug.Log(log);

            return 0;

            // ==================================== \\
            
            bool TryParseExpression(List<Token> toks)
            {
                if (toks.Count == 0)
                    return false;

                Log($"PARSING: {toks.AggrToString(", ")}");

                switch (toks[0].Type)
                {
                    // Functions
                    case TokenType.Name:
                        Log($"Function Call: '{toks[0].Value}'");
                        /* TODO:
                         * var Expression
                         * Set Variables
                         * Variable function Call
                        */
                        if (toks[0].Value == "var")
                        {
                            return HandleVarDecl(toks.Skip(1));
                        }

                        return HandleFunctionCall(toks);
                    // Macros and Decorators
                    case TokenType.AtSign:
                        //Log($"Decorator/Macro not Supported");

                        return HandleDecorator(toks.Skip(1));

                    default: break;
                }
                Log($"Exrpession could not be Parsed, Unexpected: '{toks[0].ToPrint()}'");
                return false;
            }

            bool HandleFunctionCall(List<Token> toks)
            {
                string name = toks[0].Value;

                object value;
                switch (name)
                {
                    #region SHORT_DESC
                    case "sdesc":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'sdesc' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null) {
                            return false; // Function above logs the Error
                        }
                        if (value is string) {
                            r_templateInfo.ShortDescription = value as string;
                            return true;
                        }
                        Log($"Function 'sdesc' expected a string, got nothing");
                        return false;
                    #endregion
                    #region LONG_DESC
                    case "ldesc":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'ldesc' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null) {
                            return false; // Function above logs the Error
                        }
                        if (value is string) {
                            r_templateInfo.LongDescription = value as string;
                            return true;
                        }
                        Log($"Function 'ldesc' expected a string, got nothing");
                        return false;
                    #endregion
                    #region LANGUAGE
                    case "language":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'language' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null)
                        {
                            return false; // Function above logs the Error
                        }
                        if (value is string)
                        {
                            r_templateInfo.Langs.AddRange(((string)value).CustomSplit());
                            return true;
                        }
                        Log($"Function 'language' expected a string, got nothing");
                        return false;
                    #endregion
                    #region CATEGORY
                    case "category":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'category' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null)
                        {
                            return false; // Function above logs the Error
                        }
                        if (value is string)
                        {
                            r_templateInfo.Categories.AddRange(((string)value).CustomSplit());
                            return true;
                        }
                        Log($"Function 'category' expected a string, got nothing");
                        return false;
                    #endregion
                    #region FLAGS
                    case "flags":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'flags' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null)
                        {
                            return false; // Function above logs the Error
                        }
                        if (value is string)
                        {
                            r_templateInfo.Flags.AddRange(((string)value).CustomSplit());
                            return true;
                        }
                        Log($"Function 'flags' expected a string, got nothing");
                        return false;
                    #endregion
                    #region FILEEX
                    case "fileext":
                        if (toks.Count < 2)
                        {
                            Log($"Function 'fileext' expected a string, got nothing");
                            return false;
                        }
                        if (!HandleExpression(toks.Skip(1), out value))
                        {
                            Log($"Failed to parse Expression");
                            return false;
                        }
                        if (value is null)
                        {
                            return false; // Function above logs the Error
                        }
                        if (value is string)
                        {
                            r_templateInfo.FileExts.AddRange(((string)value).CustomSplit());
                            return true;
                        }
                        Log($"Function 'fileext' expected a string, got nothing");
                        return false;
                    #endregion
                    default:
                        Log($"Unknown Function: {name}");
                        return false;
                }
            }
            bool HandleVarDecl(IEnumerable<Token> toks)
            {
                var it = toks.GetEnumerator();
                var isIt = it.MoveNext();
                if (!isIt) {
                    Log("Missing arguments in Variable Declaration");
                    return false;
                }

                var current = it.Current;
                if (current.Type != TokenType.Name) {
                    Log($"Expected Name, got '{current.ToSrc()}'");
                    return false;
                }

                var prop = new TemplateProperty(current.Value, r_templateInfo);
                r_templateInfo.Properties.Add(prop);

                // RUN DECORATORS
                if (!ApplyDecorators(prop))
                {
                    Log("Operators could not be Applied");
                    return false;
                }

                return true;
            }
            bool HandleDecorator(IEnumerable<Token> toks)
            {
                var it = toks.GetEnumerator();
                var isIt = it.MoveNext();
                if (!isIt) {
                    Log("Missing arguments in Decoration");
                    return false;
                }
                var current = it.Current;
                bool Advance(bool check = true) {
                    isIt = it.MoveNext();
                    if (check && !isIt) {
                        Log("Missing arguments in Decoration");
                        return false;
                    }
                    current = it.Current;
                    return true;
                } // if (!Advance()) return false;

                if (current.Type != TokenType.Name)
                {
                    Log($"Expected Decorator Name, got '{current.ToSrc()}'");
                    return false;
                }

                if (!decorators.TryGetValue(current.Value, out var decoratorTypeInstance))
                {
                    Log($"Unknown Decorator: '{current.Value}'");
                    return false;
                }

                Advance(false);
                
                // Left Parenthesis
                if (!isIt) {
                    if (decoratorTypeInstance.ParamCount > 0) {
                        Log($"Decorator: '{current.Value}' expected {decoratorTypeInstance.ParamCount} arguments, got None");
                        return false;
                    } else {
                        //Log($"#$$# CALLING {decoratorTypeInstance.Name}");
                        //try {
                        //    decoratorTypeInstance.Method.Invoke(null, new object[] { decoContext });
                        //    return true;
                        //}
                        //catch (Exception c) {
                        //    Log($"{decoratorTypeInstance.Name} raised an Exception\n{c.Message}");
                        //    return false;
                        //}
                        decoratorQueue.Enqueue(new DecoratorExecInstance(
                            decoratorTypeInstance,
                            new object[0]));
                        return false;
                    }
                }

                if (current.Type != TokenType.LeftParen)
                {
                    Log($"Expected '('");
                    return false;
                }

                // Expressions
                var dec_toks = new List<Token>();
                var calling_params = new List<object>();
                //calling_params.Add(decoContext);
                if (!Advance()) return false;

                // ("my str", 0)
                while (current.Type != TokenType.RightParen)
                {
                    while (current.Type != TokenType.RightParen
                        && current.Type != TokenType.Comma)
                    {
                        if (!isIt) {
                            Log("Expected End of ()");
                            return false;
                        }
                        dec_toks.Add(current);
                        if (!Advance()) {
                            Log("Expected )");
                            return false;
                        }
                    }
                    if (dec_toks.Count == 0) {
                        Log("Empty Decorator Parameter");
                        Advance(false);
                        continue;
                    }
                    if (HandleExpression(dec_toks, out var value)) {
                        calling_params.Add(value);
                        dec_toks.Clear();
                        if (current.Type == TokenType.RightParen) {
                            break;
                        }
                    } else {
                        //calling_params.Add(null);
                        Log("Expression could not be Parsed");
                        return false;
                    }
                }

                //string callargsdebug = calling_params.AggrToString(", ");
                //Log($"#$$# CALLING {decoratorTypeInstance.Name} [{callargsdebug}]");
                if (decoratorTypeInstance.ParamCount != calling_params.Count)
                {
                    Debug.Log($"Parameters did not match Function Lenght");
                    return false;
                }

                var execInstance = new DecoratorExecInstance(
                    decoratorTypeInstance,
                    calling_params.ToArray());
                decoratorQueue.Enqueue(execInstance);
                return true;

                #region OLD
                /*
                try {
                    decoContext.Prepare();
                    //decoratorTypeInstance.Method.Invoke(null, );
                    return true;
                } catch (Exception c) {
                    Log($"{decoratorTypeInstance.Name} raised an Exception\n{c.GetExceptionText()}");
                    return false;
                }

                if (current.Type == TokenType.Comma || current.Type == TokenType.RightParen)
                    {
                        // Handle Expression
                        if (dec_toks.Count == 0)
                        {
                            Log("Empty Decorator Parameter");
                            if (current.Type != TokenType.RightParen)
                                Advance();
                            continue;
                        }
                        if (HandleExpression(dec_toks, out var value))
                        {
                            calling_params.Add(value);
                        }
                        else Log("Expresion could not be Handled");

                        dec_toks.Clear();
                        Advance();
                        if (current.Type == TokenType.RightParen)
                            break;

                        continue;
                    }
                    
                    if (current.Type == TokenType.RightParen)
                    {
                        break;
                    }
                */
                #endregion
            }

            bool ApplyDecorators(TemplateProperty property)
            {
                while (decoratorQueue.Count > 0)
                {
                    var curr = decoratorQueue.Dequeue();
                    decoContext.Prepare(property);
                    try {
                        decoContext.Prepare(property);
                        curr.Execute(decoContext);
                    }
                    catch (Exception c) {
                        Log($"{curr.Decor.Name} raised an Exception:\n{c.GetExceptionText()}");
                        return false;
                    }
                }
                return true;
            }

            // TODO: AST
            bool HandleExpression(IEnumerable<Token> toks, out object value)
            {
                int i = 0;
                var sbuild = new StringBuilder();
                StringNode last = null;

                foreach (var item in toks)
                {
                    if (item.Type == TokenType.StringLit)
                    {
                        if (StringNode.TryParse(item.Value, out var newStrNode))
                        {
                            newStrNode.Last = last;
                            last = newStrNode;
                        }
                        else
                        {
                            Log("String Parse error!");
                            value = null;
                            return false;
                        }
                    }
                    else
                    {
                        Log("Non String not supported");
                        value = null;
                        return false;
                    }

                    
                }

                value = last.BuildToRoot();
                return true;
                //Log($"Could not Parse Expression {toks.AggrToString(", ")}");
                //return null;
            }

            
            void Log(string msg)
            {
                log.AppendLine(
                    $"[{templateLocation.Filepath} - " +
                    $"{linecount}]\n{msg}\n");
            }

            #region OLD
            /*
            bool AwaitDequeueAssert(params TokenType[] assert)
            {
                while (!tokens.TryDequeue(out current) || current.Type == TokenType.Comment)
                { }
                foreach (var item in assert)
                    if (item == current.Type)
                        return false;
                return true;
            }
            */
            #endregion
        }

        public static int Compile(TemplateInfo templateInfo, out TemplateOutput templateOutput)
        {
            templateOutput = new TemplateOutput
            {
                Name = templateInfo.TmpName,
            };

            var filecontents = templateInfo.GetInfoContents();
            


            return -1;
        }

        // TODO: Move to Extensions
        static bool IsRunning(this Task task) =>
            !task.IsCompleted && !task.IsFaulted && !task.IsCanceled &&
            task.Status != TaskStatus.WaitingForActivation;

        static bool IsResultFalse(this Task<bool> task)
        {
            if (task.IsCompleted)
                return !task.Result;


            return false;
        }
    }

    public enum IniciereOperator
    {
        Add,            // +
        Sub,            // -
        Mult,           // *
        Div,            // /
        Equals,         // ==
        Greater,        // >
        Less,           // <
        GreaterOrEqual, // >=
        LessOrEqual,    // <=
    }

    class NumberNoder
    {
        
    }
    class BinOpNode
    {

    }
    class StringNode
    {
        StringNode m_Next, m_Last;

        private StringNode(string value)
        {
            Value = value;
        }
        public string Value { get; private set; }
        
        public StringNode Next
        {
            get => m_Next;
            set
            {
                m_Next = value;
                if (value is null || value.m_Last == this)
                    return;

                if (value.m_Last != null)
                    value.m_Last.m_Next = null;

                value.m_Last = this;
            }
        }
        public StringNode Last
        {
            get => m_Last;
            set
            {
                m_Last = value;
                if (value is null || value.m_Next == this)
                    return;

                if (value.m_Next != null)
                    value.m_Next.m_Last = null;

                value.m_Next = this;
            }
        }

        public static bool TryParse(string text, out StringNode node)
        {
            if (StringUtils.TryParse(text, out var str)) {
                node = new StringNode(str);
                return true;
            }
            node = null;
            return false;
        }

        public string BuildToRoot()
        {
            var build = new StringBuilder(Value);
            var curr = Last;
            while (curr is StringNode)
            {
                build.Append(curr.Value);
                curr = curr.Last;
            }
            return build.ToString();
        }
    }

    
}

#region OLD_CODE
/*
public class VariableInstance
{
    public Type type;
    public object value;
}

class ASTNode
{
    private readonly Token m_Token;
    private ASTNode m_Parent;
    private readonly HashSet<ASTNode> m_Children
        = new HashSet<ASTNode>();
    public ASTNode(Token token)
    {
        m_Token = token;
    }
    public TokenType Type => m_Token.Type;
    public string Value => m_Token.Value;
    public ASTNode Parent
    {
        get => m_Parent;
        set
        {
            m_Parent.m_Children.Remove(this);
            m_Parent = value;
            m_Parent.m_Children.Add(this);
        }
    }
    public IEnumerable<ASTNode> Children => m_Children;
}
*/
#endregion
