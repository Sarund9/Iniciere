using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class CreateScriptWindow : EditorWindow
    {


        [MenuItem("Assets/Create/Script", priority = 80)] //80 is C# Script Priority
        public static void Create()
        {
            var win = CreateInstance<CreateScriptWindow>();

            win.minSize = new Vector2(460, 580);
            win.maxSize = new Vector2(2000, 1600);

            //win.position = new Rect(200, 200);
#if UNITY_2020_1_OR_NEWER
            Rect pos = EditorGUIUtility.GetMainWindowPosition();
#else
            Rect pos = Extensions.GetEditorMainWindowPos2019();
#endif

            Vector2 scale = new Vector2(0.4f, 0.7f);
            Vector2 pivot = new Vector2(pos.width / 2, pos.height / 2);
            win.position = pos.ScaleSizeBy(scale, pivot);

            win.titleContent = new GUIContent("Create new Script file", "Create a new Script File");

            //win.ShowModalUtility();
            win.ShowUtility();
        }

        private void OnEnable()
        {
            FindFiles();
        }

        private void OnGUI()
        {
            //GUILayout.Label("Template Window");

            
        }

        private async Task FindFiles()
        {
            IEnumerable<string> filepaths = await Task.Run(() => InicereScriptFinder.FindFilePaths());

            

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