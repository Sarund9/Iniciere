using UnityEditor;
using UnityEngine;
using System;
using System.Text;

namespace Iniciere
{
    public class StringTests : EditorWindow
    {
        //[MenuItem("Tools/Iniciere/String Tests")]
        public static void OpenWindow()
        {
            GetWindow<StringTests>("String Tests");
        }


        string str;
        string args;
        string result;

        private void OnGUI()
        {

            str = EditorGUILayout.TextField("Base", str);
            args = EditorGUILayout.TextField("args", args);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Format"))
            {
                var objs = args.Split(
                    new char[] { ',', ' '},
                    StringSplitOptions.RemoveEmptyEntries
                    );
                var build = new StringBuilder(str);
                StandartMacros.Format(build, null, objs);

                result = build.ToString();
            }
            if (GUILayout.Button("Custom Split"))
            {
                var split = str.CustomSplit();
                var build = new StringBuilder();
                build.AppendLine($"STRING: \n{str}");
                build.AppendLine($"Results in");
                foreach (var item in split)
                {
                    build.AppendLine($"{item}");
                }
                build.AppendLine($"=============");
                result = build.ToString();
            }
            if (GUILayout.Button("Capture ()"))
            {
                result = StringUtils.CaptureAfter(str, '(', ')');
            }
            GUILayout.EndHorizontal();

            if (result != null)
            {
                GUILayout.Label("RESULT");
                EditorGUILayout.TextArea(result);
            }
        }
    }
}
