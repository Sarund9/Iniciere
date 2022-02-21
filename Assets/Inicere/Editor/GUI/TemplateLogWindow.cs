using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Iniciere
{
    public class TemplateLogWindow : EditorWindow
    {
        List<LogEntry> m_Log;
        string m_TempName;

        TemplateLogWindowGUI ui = new TemplateLogWindowGUI();

        public static void OpenFrom(List<LogEntry> log, string tmpname)
        {
            var win = CreateInstance<TemplateLogWindow>();
            win.CenterOnMainWin();
            win.minSize = new Vector2(420, 360);
            win.titleContent = new GUIContent("Template Log");
            win.m_Log = log;
            win.m_TempName = tmpname;
            win.Show();
        }
        
        public void OnGUI()
        {
            ui.Draw(m_Log, m_TempName, position);
        }

        private void Update()
        {
            Repaint();
        }
    }
}