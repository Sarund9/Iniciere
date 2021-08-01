using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Iniciere
{
    public class Compiler
    {
        #region CONST
        const string TEMPLATE_START = "<#iniciere";
        const string TEMPLATE_END = "#/>";

        #endregion

        List<string> lines;
        //string s_Raw;

        //[Obsolete]
        //private Compiler(string text)
        //{
        //    var filtered = StringUtils.FilterAllComments(text);
        //    lines = new List<string>(filtered.Split('\n'));
        //    s_Raw = text;
        //}

        //public static int Precompile(TemplateLocation templateLocation)
        //{

        //    throw new NotImplementedException();
        //}

        public static int Precompile(TemplateLocation templateLocation, out TemplateInfo templateInfo)
        {
            var filteredContents = StringUtils.FilterAllComments(templateLocation.GetContents());
            var lines = new List<string>(filteredContents.Split('\n'));

            templateInfo = new TemplateInfo(templateLocation);
            try {
                templateInfo.Name = TryStartTemplate(lines[0]);

            }
            catch
            {
                Debug.LogError("Template Name is Invalid");
                return -1;
            }
            
            TryEndTemplate(lines[lines.Count - 1]);
            



            //USING STATEMENTS
            List<string> includes = new List<string>();

            

            //REFLECTIONS
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes());

            //Debug.Log($"Starts Compiling: '{templateInfo.Name}'");

            for (int l = 0; l < lines.Count; l++)
            {
                bool checkForSkip = true;
                //FILEEXT KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "fileext", templateInfo.Properties,
                        out var result, out var end))
                    {
                        var rs = result.Split(
                            new char[] {' ',',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < rs.Length; i++)
                            templateInfo.FileExts.Add(rs[i]);

                        //Debug.Log($"FILE EX: '{result}'");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //LANGUAGE KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "language", templateInfo.Properties,
                        out var result, out var end))
                    {
                        var rs = result.Split(
                            new char[] { ' ', ',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < rs.Length; i++)
                            templateInfo.Langs.Add(rs[i]);

                        //Debug.Log($"LANGUAGE: '{result}'");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //CATEGORY KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "category", templateInfo.Properties,
                        out var result, out var end))
                    {
                        var rs = result.Split(
                            new char[] { ' ', ',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < rs.Length; i++)
                            templateInfo.Categories.Add(rs[i]);

                        //Debug.Log($"CATEGORY: '{result}'");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //FLAGS KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "flags", templateInfo.Properties,
                        out var result, out var end))
                    {
                        var rs = result.Split(
                            new char[] { ' ', ',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < rs.Length; i++)
                            templateInfo.Flags.Add(rs[i]);

                        //Debug.Log($"FLAGS: '{result}'");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //USING KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "using", templateInfo.Properties,
                        out var result, out var end))
                    {
                        if (!ResolveNamespace(result, types))
                        {
                            Debug.LogError($"Iniciere Error: namespace '{result}' could not be found");
                            return -1;
                        }

                        includes.Add(result);
                        //Debug.Log($"USING: '{result}'");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //IN KEYWORD (PROPERTIES)
                {
                    if (StringUtils.TryHandleProperty(lines, new TextPos(l),
                        out var typename, out var varname, out var end))
                    {
                        if (!TryResolveType(typename, includes, out var type))
                        {
                            Debug.LogError($"TYPE '{typename}' could not be found");
                            foreach (var item in includes)
                            {
                                Debug.LogError($"Including: '{item}'");
                            }
                            return -1;
                        }

                        templateInfo.Properties.Add(new TemplateProperty(varname)
                        {
                            Type = type,
                            Value = null,
                        });
                        //Debug.Log($"PROPERTY: '{varname}' ({typename})");
                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //DESC KEYWORD (DESCRIPTION)
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "desc", templateInfo.Properties,
                        out var result, out var end))
                    {
                        templateInfo.Description += result;

                        l = end.l;
                        checkForSkip = false;
                        continue;
                    }
                }
                //STRING SKIPPING
                {
                    if (checkForSkip && StringUtils.TrySkip(lines, new TextPos(l), out var end))
                    {
                        //Debug.Log($"SKIPPING FROM - TO:\n" +
                        //    $"{l}: {m_Lines[l]}\n" +
                        //    $"{end.l}: {m_Lines[end.l]}\n");
                        //Debug.Log($"Skipping {l} to {end.l}");
                        l = end.l;
                    }
                }
            }

            //templateInfo.Contents = string.Join(Environment.NewLine, lines);

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
                Name = info.Name
            };

            Queue<MacroExecutionInstance> macroQueue = new Queue<MacroExecutionInstance>();
            var allMacros =
                MacroContext.InitializeMacroSystem(
                    AppDomain.CurrentDomain, out MacroContext macroContext);

            Dictionary<string, MacroInstance> macros = new Dictionary<string, MacroInstance>();
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


            var lines = new List<string>(info.GetContents().Split('\n'));


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
                        Debug.Log($"MACRO: '{macroExInstance.Macro.Name}'");
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
                        //Debug.Log($"\t ADD: '{body.TrySubstring(0, 20)}'");
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
            }

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
                        Debug.LogWarning($"EXECUTING");
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
        public int Compile(TemplateInfo info, out Template template)
        {
            //result = new List<string>(m_Lines.Count);

            bool b_InTemplate = false; //Indicates if we are inside a template
            template = new Template(); //If inside a template, the name of it

            try {
                template.Name = TryStartTemplate(lines[0]);
                Debug.LogWarning($"NAME: {template.Name}");
            }
            catch
            {
                Debug.LogError("No template Name");
                return -1;
            } //TEMPLATE NAME

            if (!b_InTemplate)
            {
                Debug.LogError("No template Name");
                return -1; //TEMPLATE NOT FOUND
            }

            Queue<MacroInstance> macroQueue = new Queue<MacroInstance>();
            var allMacros =
                MacroContext.InitializeMacroSystem(
                    AppDomain.CurrentDomain, out MacroContext macroContext);



            Debug.Log($"Starts Compiling: '{template.Name}'");
            for (int l = 0; l < lines.Count; l++)
            {
                //if (!b_InTemplate)
                //{
                //    try {
                //        TryStartTemplate(m_Lines[l]);
                //    }
                //    catch { return -1; }
                //    continue;
                //}

                //TODO: detect and add macros to list (and cache for future compilation)


                //TODO: detect and add inputs to property system

                //FILE KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "file", info.Properties,
                        out var name, out var end))
                    {
                        template.AddFile(name);
                        Debug.Log($"FILE: '{name}'");
                        l = end.l;
                        continue;
                    }
                }

                //ADD KEYWORD
                {
                    if (StringUtils.TryHandleKeyword(lines, new TextPos(l), "add", info.Properties,
                        out var body, out var end))
                    {
                        if (template.FileCount == 0)
                        {
                            Debug.LogError("cannot add lines to no file");
                            return -1;
                        }
                        template.LastFile().AddLine(body);
                        Debug.Log($"\t ADD: '{body.TrySubstring(0, 20)}'");
                        //TODO: run macros
                        l = end.l;
                        continue;
                    }
                }
                //LANGUAGE KEYWORD
                //{
                //    if (StringUtils.TryHandleKeyword(m_Lines, new TextPos(l), "language",
                //        out var name, out var end))
                //    {

                //    }

                //}

                //for (int c = 0; c < m_Lines[l].Length; c++)
                //{

                //    //m_Lines[i][c];
                //}
            }



            return 0;
            // =================== \\

            string TryStartTemplate(string line)
            {
                if (line.StartsWith(TEMPLATE_START))
                {
                    b_InTemplate = true;
                }
                return StringUtils.CaptureInBetween(line);
                //template.Name = name;
            }
        }
        #endregion

    }

    //public class VariableInstance
    //{
    //    public Type type;
    //    public object value;
    //}
}
