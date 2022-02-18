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

        Vector2 m_Scroll;
        bool m_ShowMsg = true, m_ShowWrn = true, m_ShowErr = true;
        int m_NumMsg, m_NumWrn, m_NumErr;
        string m_Search;

        //Rect fullLogView, fullLogDragArea;
        bool m_Dragging;
        float m_FullLogHeight;

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
            GUILayout.BeginHorizontal();

            var logTitleArea = GUILayoutUtility
                .GetRect(50, EditorGUIUtility.singleLineHeight,
                GUILayout.MaxWidth(300));
            var fullArea = new Rect(logTitleArea)
            {
                width = Screen.width,
            };

            GUILayout.FlexibleSpace();
            var r_SearchBar = GUILayoutUtility
                .GetRect(300, EditorGUIUtility.singleLineHeight,
                GUILayout.MinWidth(60),
                GUILayout.ExpandWidth(true));

            var r_Toolbar = GUILayoutUtility
                .GetRect(100, EditorGUIUtility.singleLineHeight);

            GUILayout.EndHorizontal();

            var toolbarStyle = new GUIStyle("toolbar")
            {

            };

            GUI.Box(fullArea, "", toolbarStyle);

            // TEMPLATE NAME
            GUI.Label(logTitleArea, m_TempName, "BoldLabel");

            var toolbarSearchbarStyle = new GUIStyle("ToolbarSeachTextField")
            {
                margin = new RectOffset(12, 12, 12, 12),
                //alignment = TextAnchor.MiddleCenter,
                
            };

            // SEARCHBAR
            m_Search = EditorGUI.TextField(r_SearchBar.Shift(-2.8f, 1.5f),
                m_Search, toolbarSearchbarStyle);

            var toolbarBtnStyle = new GUIStyle("ToolbarButton")
            {

            };

            // TOOLBAR
            {
                var it = r_Toolbar.SplitHorizontal(3).GetEnumerator();
                it.MoveNext();
                m_ShowMsg = EditorGUI.Toggle(
                    it.Current,
                    m_ShowMsg, toolbarBtnStyle);
                GUI.Label(it.Current, new GUIContent(m_NumMsg.ToString(), "Messages"));

                it.MoveNext();
                m_ShowWrn = EditorGUI.Toggle(
                    it.Current,
                    m_ShowWrn, toolbarBtnStyle);
                GUI.Label(it.Current, new GUIContent(m_NumWrn.ToString(), "Warnings"));

                it.MoveNext();
                m_ShowErr = EditorGUI.Toggle(
                    it.Current,
                    m_ShowErr, toolbarBtnStyle);
                GUI.Label(it.Current, new GUIContent(m_NumErr.ToString(), "Errors"));
            }

            // LOG
            var boxStyle = new GUIStyle("Box")
            {
                margin = new RectOffset(0, 0, 4, 0),
            };
            m_Scroll = GUILayout.BeginScrollView(
                m_Scroll, boxStyle
                );

            foreach (var msg in m_Log)
            {
                // SKIP UNWANTED MESSAGES
                switch (msg.Level)
                {
                    case LogLevel.Msg:
                        if (!m_ShowMsg) continue;
                        break;
                    case LogLevel.Wrn:
                        if (!m_ShowWrn) continue;
                        break;
                    case LogLevel.Err:
                        if (!m_ShowErr) continue;
                        break;
                    default: break;
                }
                // SKIP SEARCH
                if (!string.IsNullOrEmpty(m_Search) &&
                    !msg.Message.Contains(m_Search))
                {
                    continue;
                }

                EditorGUILayout.HelpBox(msg.Message, GetType(msg.Level));
                static MessageType GetType(LogLevel lvl) => lvl switch
                {
                    LogLevel.Msg => MessageType.Info,
                    LogLevel.Wrn => MessageType.Warning,
                    LogLevel.Err => MessageType.Error,
                    _ => MessageType.None,
                };
            }

            GUILayout.EndScrollView();

            //if (fullLogView.width < 1)
            const float DragSize = 8;

            var fullLogView = GUILayoutUtility.GetRect(20, m_FullLogHeight)
                .Shrink(0, 0, 0, 0);
            var fullLogDragArea = new Rect(fullLogView)
            {
                y = fullLogView.y - (DragSize / 2),
                height = DragSize,
            };

            var fullLogStyle = new GUIStyle("Label")
            {
                alignment = TextAnchor.UpperLeft,

            };

            GUI.Label(fullLogView, "Full Log Message", fullLogStyle);
            EditorGUI.DrawRect(fullLogDragArea, Color.red * .5f);

            // DRAGGING
            var e = Event.current;
            var fullWin = new Rect(0, 0, position.width, position.height);

            if (e.type == EventType.MouseDown // Enter Drag
                && e.button == 0
                && fullLogDragArea.Contains(e.mousePosition))
            {
                m_Dragging = true;
            }
            else if (m_Dragging && // Exit Drag
                    (e.type == EventType.MouseUp)
                ||  (!fullWin.Contains(e.mousePosition)))
            {
                m_Dragging = false;
            }
            else if (m_Dragging) // Move Area
            {
                m_FullLogHeight = (position.height - e.mousePosition.y) - 20;
                Repaint();
                EditorGUI.DrawRect(
                    new Rect(e.mousePosition - Vector2.one * 3, Vector2.one * 6), Color.green);
            }

            // Clamp Area
            m_FullLogHeight = Mathf.Clamp(m_FullLogHeight,
                70, position.height - 120);

        }
    }
}