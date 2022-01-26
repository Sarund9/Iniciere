using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class ScriptBuilder : EditorWindow
    {
        Task<TemplateResult> compiling;
        string directoryPath;

        string tmpName;

        public static void CreateScript(TemplateInfo info, string directoryPath)
        {
            var win = CreateInstance<ScriptBuilder>();
            win.directoryPath = directoryPath;
            win.tmpName = info.TmpName;

#if UNITY_2020_1_OR_NEWER
            Rect pos = EditorGUIUtility.GetMainWindowPosition();
#else
            Rect pos = Extensions.GetEditorMainWindowPos2019();
#endif

            Vector2 scale = new Vector2(0.3f, 0.2f);
            Vector2 pivot = new Vector2(pos.width / 2, pos.height / 2);
            win.position = pos.ScaleSizeBy(scale, pivot);

            win.ShowUtility();

            win.compiling = CompileAsync(info);
        }

        private void OnGUI()
        {
            GUILayout.Label($"Generating Script: '{tmpName}' ...");
        }

        private void OnInspectorUpdate()
        {
            if (compiling is null)
            {
                Close();
                return;
            }
            if (compiling.IsFaulted)
            {
                Debug.LogError($"Script Build Canceled, Compilation Error:\n{compiling.Exception.InnerException.Message}");
                compiling = null;
                Close();
            }
            else if (compiling.IsCompleted)
            {
                var r = compiling.Result;
                if (r.result != 0)
                {
                    compiling = null;
                }
                else
                {
                    CreateFile(r.template, directoryPath);
                }
                Close();
            }
        }

        void CreateFile(TemplateOutput template, string directoryPath)
        {
            foreach (var file in template.Files)
            {
                var filepath = $"{directoryPath}/{file.Name}";

                Debug.Log($"CREATING FILE AT {filepath}");

                using var create = File.CreateText(filepath);
                create.Write(file.GetString());
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }


        static async Task<TemplateResult> CompileAsync(TemplateInfo info)
        {
            TemplateOutput template = null;
            int result = await Task.Run(() => Compiler.Compile(info, out template));

            if (result != 0)
            {
                Debug.LogError($"Async Compile of '{info.TmpName}' Failed! error code: {result}");
            }

            return new TemplateResult(result, template);
        }

        struct TemplateResult
        {
            public int result;
            public TemplateOutput template;

            public TemplateResult(int result, TemplateOutput template)
            {
                this.result = result;
                this.template = template;
            }
        }
    }
}


#region OLD_CODE
/*
const int SIZE = 20;
const int MARGIN = 10;

Rect rect = new Rect
{
    x = Screen.width - SIZE,
    y = SIZE,
    width = SIZE,
    height = SIZE,
};

if (GUI.Button(rect, "X"))
{
    Close();
}

*/
#endregion