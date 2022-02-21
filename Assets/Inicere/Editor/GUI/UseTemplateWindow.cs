using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Iniciere
{
    public class UseTemplateWindow : EditorWindow
    {
        TemplateInfo m_Info;

        BuildFileGUI ui = new BuildFileGUI();

        public static void OpenFrom(TemplateInfo info)
        {
            var win = CreateInstance<UseTemplateWindow>();
            win.CenterOnMainWin();
            win.minSize = new Vector2(420, 360);
            win.titleContent = new GUIContent("Create Template");

            win.m_Info = Instantiate(info);
            win.ShowUtility();
        }

        // TODO: Extract UI to Class, for re-use in CreateScriptWindow
        public void OnGUI()
        {
            if (!ui.Draw(m_Info, Close)) {
                Close();
            }
        }

        private void OnDisable()
        {
            DestroyImmediate(m_Info);
        }

    }
}