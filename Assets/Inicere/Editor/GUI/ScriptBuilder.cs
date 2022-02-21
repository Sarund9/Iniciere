using System.Collections.Generic;
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

        List<LogEntry> m_Log = new List<LogEntry>();
        Vector2 m_Scroll;

        TemplateLogWindowGUI ui = new TemplateLogWindowGUI();

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
            win.titleContent = new GUIContent($"Building '{info.TmpName}'...");

            Vector2 scale = new Vector2(0.3f, 0.2f);
            Vector2 pivot = new Vector2(pos.width / 2, pos.height / 2);
            win.position = pos.ScaleSizeBy(scale, pivot);

            win.ShowUtility();

            //Debug.Log($"Creating script from {info.name} in\n'{directoryPath}'");

            win.compiling = CompileAsync(info, win);
        }

        private void OnGUI()
        {
            ui.Draw(m_Log, tmpName, position);
        }

        private void Update()
        {
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            if (compiling is null)
            {
                //Close();
                return;
            }
            if (compiling.IsFaulted)
            {
                Debug.LogError($"Script Build Canceled, Compilation Error:\n{compiling.Exception.InnerException.Message}");
                compiling = null;
                //Close();
            }
            else if (compiling.IsCompleted)
            {
                var r = compiling.Result;
                if (r.result == 0)
                {
                    CreateFiles(r.template, directoryPath);
                }
                compiling = null;
                Log(new LogEntry(LogLevel.Msg, "Finished Generating"));
                //Close();
            }
        }

        void CreateFiles(TemplateOutput template, string directoryPath)
        {
            if (template.Files.Count == 0)
            {
                Debug.LogWarning($"No files were created from '{template.Name}'");
                return;
            }
            
            foreach (var file in template.Files)
            {
                var filepath = $"{directoryPath}/{file.Name}";

                //Debug.Log($"CREATING FILE AT {filepath}");

                using var create = File.CreateText(filepath);
                create.Write(file.GetString());
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }


        static async Task<TemplateResult> CompileAsync(TemplateInfo info, ScriptBuilder instance)
        {
            TemplateOutput template = new TemplateOutput();
            int result = await Task.Run(() => Compiler.Compile(info, instance.Log, template));

            if (result != 0)
            {
                Debug.LogError($"Async Compile of '{info.TmpName}' Failed! error code: {result}");
            }

            return new TemplateResult(result, template);
        }

        private void Log(LogEntry obj)
        {
            lock(m_Log)
            {
                m_Log.Add(obj);
                //m_Scroll.y += 400; // TODO: Better go down
            }
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
private string GetStateMessage()
{
    if (compiling is null)
        return $"Finished Generating '{tmpName}'";
    return $"Generating Script: '{tmpName}' ...";
}

GUILayout.Label(GetStateMessage());
m_Scroll = GUILayout.BeginScrollView(m_Scroll, "Box");

try
{
    foreach (var item in m_Log)
    {
        GUILayout.Label($"[{item.Level}] - {item.Message}");
        //static string GetStyle(LogLevel lvl) => lvl switch
        //{
        //    LogLevel.Wrn => "WarningStyle",
        //    LogLevel.Err => "ErrorStyle",
        //    _ => "LogStyle",
        //};
    }
}
catch { }
            
GUILayout.EndScrollView();
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