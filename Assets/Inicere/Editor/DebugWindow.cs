using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class DebugWindow : EditorWindow
    {
        //string input;
        //string output;

        List<TemplateLocation> templates;
        TemplateInfo lastInfo;

        string filename;
        string filepath;

        string s_FullTemplate;

        Vector2 scroll;

        UBox box = new UBox();

        [MenuItem("Tools/Iniciere/DebugWindow")]
        public static void OpenWindow()
        {
            GetWindow<DebugWindow>("Debug Window");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Find Files"))
            {
                var files = InicereScriptFinder.FindFilePaths();
                templates = InicereScriptFinder.FindTemplatesLite(files).ToList();
                //Debug.Log($"FILES {files.Count()}");
                //foreach (var template in templates)
                //{
                //    Debug.Log($"FILE: \n{template}");
                //}
            }
            if (GUILayout.Button("TEST"))
            {
                //if (lastInfo != null) PopulateProperties();
                //Testing(lastInfo);
                //var json = JsonUtility.ToJson("TEST", true);
                //Debug.Log($"'TEST' = '{json}'");
            }

            filepath = EditorGUILayout.TextField("PATH", filepath);

            #region FIND_TEST
            if (GUILayout.Button("FIND_TEST"))
            {
                var it = IniciereFileImporter.GetTemplates(filepath);

                foreach (var item in it)
                {
                    var build = new StringBuilder(item.ToString());
                    build.AppendLine(
                        "\n\n=================================== INFO ===================================");
                    try
                    {
                        build.AppendLine(item.GetInfoContents());
                    }
                    catch
                    {
                        build.AppendLine("INFO CONTENT ERROR");
                    }

                    build.AppendLine(
                        "\n=================================== BODY ===================================");
                    try
                    {
                        build.AppendLine(item.GetBodyContents());
                    }
                    catch (Exception ex)
                    {
                        build.AppendLine("BODY CONTENT ERROR");
                        build.AppendLine(ex.ToString());
                    }
                    build.AppendLine(
                        "\n============================================================================");
                    Debug.Log(build.ToString());
                }
            }
            #endregion

            if (GUILayout.Button("SET_BOX"))
            {
                box.Set(filepath);
            }
            if (GUILayout.Button("GET_BOX"))
            {
                var test = box.Get();
                Debug.Log($"VALUE: '{test}'");
            }

            if (templates is null)
                return;

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (var file in templates)
            {
                //GUILayout.FlexibleSpace();
                var contents = file.GetInfoContents();
                GUILayout.TextArea(contents);

                //GUILayout.BeginHorizontal();
                if (GUILayout.Button("PreCompile", GUILayout.Width(120), GUILayout.Height(25)))
                {
                    //var filtered = StringUtils.FilterAllComments(file);
                    //var lines = new List<string>(filtered.Split('\n'));
                    lastInfo = TemplateInfo.New(file);
                    int result = Compiler.Precompile(file, lastInfo);
                    if (result == 0)
                    {
                        Debug.Log($"Precompiler succeded");
                        PrintTemplateInfo(lastInfo);
                    }
                    else if (result < 0)
                    {
                        Debug.LogError($"Compiler failed with an output of {result}");
                    }
                }
                
                //GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (lastInfo is null)
                return;

            GUILayout.Space(20f);

            filename = EditorGUILayout.TextField("INPUT", filename);

            if (GUILayout.Button($"Compile {lastInfo.TmpName}", GUILayout.Height(24)))
            {
                //Fill string properties with something
                if (lastInfo != null) PopulateProperties();

                TemplateOutput templateOutput = new TemplateOutput();

                int result = Compiler.Compile(lastInfo, x => { }, templateOutput);
                if (result == 0)
                {
                    Debug.Log($"Compiler succeded");
                    PrintTemplateOutput(templateOutput);
                }
                else if (result < 0)
                {
                    Debug.Log($"Compiler failed with an output of {result}");
                }
            }
            //GUILayout.Space(30f);
            //input = EditorGUI.TextArea(GUILayoutUtility.GetRect(20, 60), input);
            //GUILayout.Label(StringUtils.FilterAllComments(input));
            //if (GUILayout.Button("Filter Comments"))
            //{
            //    Debug.Log(StringUtils.FilterAllComments(input));
            //}
        }

        private void PopulateProperties()
        {
            foreach (var prop in lastInfo.Properties)
            {
                if (prop.Type == typeof(string))
                {
                    prop.Value = filename;
                    break;
                }
            }
        }

        private static void Testing(TemplateInfo info)
        {
            StringBuilder build = new StringBuilder("Testing new FindTempaltes");

            var files = InicereScriptFinder.FindFilePaths();

            var templates = InicereScriptFinder.FindTemplatesLite(files);

            foreach (var item in templates)
            {
                build.AppendLine($"TMP in {item.Filepath} at {item.InfoCharPos} and {item.InfoCharPos} returns:");

                string contents = item.GetInfoContents();

                build.AppendLine(contents);

                build.AppendLine("======================");
            }

            build.AppendLine("======================");

            Debug.Log(build.ToString());
        }
        #region OLD_TESTS
        /*
           //START: 1-7
            var str = "$filename, $typename";
        var list = new List<string>
            {
                "",
                "#FORMAT($filename, $typename)",
                "",
            };
        build.AppendLine($"Input: '{str}'");

            StringUtils.TryHandleParamInput(str, info.Properties, out var objs);

            build.AppendLine($"Results in: ");

            if (objs is object)
            {
                foreach (object param in objs)
                {
                    build.AppendLine($"{param}");
                }
            }
            else
            {
                build.AppendLine($"NULL");
            }
        //string str = "  \"A Fox, and a Hound\", River, \"Boo, Lang\"";
        //StringBuilder build = new StringBuilder();
        //build.AppendLine($"Detecting:\n{str}");
        ////for (int i = 0; i < str.Length; i++)
        ////    build.Append(i.ToString());
        ////build.AppendLine();
        //for (int i = 0; i < 5; i++)
        //{
        //    build.AppendLine($"INDEX[{i}] begin with '\"': {str.BeginsAtWithOrWhitespace("\"", i)}");
        //}
        //Debug.Log(build.ToString());
        //string str = "HELLO WORLD, \"A Fox, and a Hound\", River, \"Boo, Lang\"";
        //StringBuilder build = new StringBuilder();
        //build.AppendLine($"Splitting:\n{str}");
        //var split = str.CustomSplit();
        //foreach (var s in split)
        //{
        //    build.AppendLine($"[DES] - {s}");
        //}
        //Debug.Log(build.ToString());
        //string str = "";
        //var lines = new List<string>
        //{
        //    "",
        //    "file $filename",
        //    "",
        //};
        //StringBuilder build = new StringBuilder();
        ////start: 1
        //string str = "      Hello";
        //build.AppendLine("Begins at Test");
        //build.AppendLine(str);
        //build.AppendLine("==== true ====");
        //build.AppendLine("Begins with Hello at [0]: " + str.BeginsAtWith("Hello", 0));
        //build.AppendLine("Begins with Hello at [0]: " + str.BeginsAtWithOrWhitespace("Hello", 0));
        //build.AppendLine("==== false ====");
        //build.AppendLine("Begins with A at [8]: " + str.BeginsAtWith("H", 0));
        //build.AppendLine("Begins with s at [6]: " + str.BeginsAtWithOrWhitespace("H", 0));
        ////var sucess = StringUtils.TryHandleParam<string>(lines, new TextPos(1), info.Properties, '#', out string result, out var end);
        //Debug.Log(build.ToString());//*/
        #endregion

        static void PrintTemplateOutput(TemplateOutput o)
        {
            StringBuilder build = new StringBuilder(1024);

            build.AppendLine($"Template Output for template: {o.Name}");
            build.AppendLine("==========================");

            foreach (var file in o.Files)
            {
                build.AppendLine($"FILE: {file.Name} - Contents:");
                var contents = file.GetString();
                build.AppendLine($"{contents}");
                build.AppendLine("==========================");
            }
            build.AppendLine("==========================");

            Debug.Log(build.ToString());
        }
        static void PrintTemplateInfo(TemplateInfo info)
        {
            StringBuilder build = new StringBuilder(1024);
            build.AppendLine($"Template info for template: {info.TmpName}");
            build.AppendLine("==========================");
            // File Ex
            build.Append($"File extensions: ");
            foreach (var fex in info.FileExts)
            {
                build.Append($"{fex}, ");
            }
            build.AppendLine();
            // Lang
            build.Append($"Languages: ");
            foreach (var lang in info.Langs)
            {
                build.Append($"{lang}, ");
            }
            build.AppendLine();
            // Categories
            build.Append($"Categories: ");
            foreach (var cat in info.Categories)
            {
                build.Append($"{cat}, ");
            }
            build.AppendLine();

            build.Append($"Flags: ");
            foreach (var cat in info.Flags)
            {
                build.Append($"{cat}, ");
            }
            build.AppendLine();

            build.AppendLine($"=== PROPERTIES ===");
            foreach (var prop in info.Properties)
            {
                build.AppendLine(prop.ToString());
            }

            build.AppendLine("=== CONTENTS ===");
            build.Append(info.GetInfoContents());

            Debug.Log(build.ToString());
        }
        static void PrintTemplate(Template template)
        {
            Log("PRINTING TEMPALTE:" + template.Name);
            if (template.FileEx != null)
            {
                Log("File Extensions:");
                foreach (var fileEx in template.FileEx)
                {
                    Log(fileEx);
                }
            }
            
            if (template.Lang != null)
            {
                Log("Languages:");
                foreach (var lang in template.Lang)
                {
                    Log(lang);
                }
            }

            //foreach (var cat in template.Category)
            //{
            //    Log(cat);
            //}
            Log("Text Files:");
            for (int i = 0; i < template.FileCount; i++)
            {
                var file = template[i];
                Log($"File: {file.Name}, Contents:");
                Log(file.GetString());
            }

            // ====================== \\
            static void Log(object msg) => Debug.Log(msg);
        }
    }
}

//var filteered = (string)input.Clone();

//var lines = filteered.Split('\n');
//for (int i = 0; i < lines.Length; i++)
//{
//    StringUtils.FilterComments(ref lines[i]);
//    GUILayout.Label(lines[i]);
//}
//output = EditorGUILayout.TextArea(output);
