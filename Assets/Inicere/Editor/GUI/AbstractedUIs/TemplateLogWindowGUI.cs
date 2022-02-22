using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateLogWindowGUI
    {

        Vector2 m_Scroll;
        bool m_ShowTrc = false, m_ShowMsg = true, m_ShowWrn = true, m_ShowErr = true;
        int m_NumTrc, m_NumMsg, m_NumWrn, m_NumErr;
        string m_Search;

        bool m_Dragging;
        float m_FullLogHeight = 70;
        int m_SelectedLogItem = -1;

        int m_MouseDownOn = -1;

        Color c_ItemHover = new Color(.2f, .2f, .2f, .2f),
            c_ItemPress = new Color(.1f, .1f, .1f, .3f),
            c_ItemSelect = new Color(.3f, .4f, .8f, .1f);

        void UpdateLogNums(List<LogEntry> log)
        {
            m_NumTrc = 0; m_NumMsg = 0; m_NumWrn = 0; m_NumErr = 0;
            for (int i = 0; i < log.Count; i++)
            {
                switch (log[i].Level)
                {
                    case LogLevel.Trc:
                        m_NumTrc++;
                        break;
                    case LogLevel.Msg:
                        m_NumMsg++;
                        break;
                    case LogLevel.Wrn:
                        m_NumWrn++;
                        break;
                    case LogLevel.Err:
                        m_NumErr++;
                        break;
                }
            }
        }

        public void Draw(List<LogEntry> log, string tmpname, Rect winPosition)
        {
            UpdateLogNums(log);

            #region TOP Area Layout
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
                .GetRect(130, EditorGUIUtility.singleLineHeight);

            GUILayout.EndHorizontal();

            #endregion

            #region TOP Area Controls

            var toolbarStyle = new GUIStyle("toolbar")
            {

            };

            GUI.Box(fullArea, "", toolbarStyle);

            // TEMPLATE NAME
            GUI.Label(logTitleArea, tmpname, "BoldLabel");

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
                var it = r_Toolbar.SplitHorizontal(4).GetEnumerator();
                it.MoveNext();
                m_ShowTrc = EditorGUI.Toggle(
                    it.Current,
                    m_ShowTrc, toolbarBtnStyle);
                GUI.Label(it.Current, new GUIContent(m_NumTrc.ToString(), "Trace"));

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

            #endregion

            #region LOG

            // LOG
            var boxStyle = new GUIStyle("Box")
            {
                margin = new RectOffset(0, 0, 4, 0),
            };
            m_Scroll = GUILayout.BeginScrollView(
                m_Scroll, boxStyle
                );

            var e = Event.current;

            bool hasSelected = false;
            bool hasBegunSelecting = false;
            for (int i = 0; i < log.Count; i++)
            {
                LogEntry msg = log[i];
                // SKIP UNWANTED MESSAGES
                switch (msg.Level)
                {
                    case LogLevel.Trc:
                        if (!m_ShowTrc) continue;
                        break;
                    case LogLevel.Msg:
                        if (!m_ShowMsg) continue;
                        break;
                    case LogLevel.Wrn:
                        if (!m_ShowWrn) continue;
                        break;
                    case LogLevel.Err:
                        if (!m_ShowErr) continue;
                        break;
                }
                // SKIP SEARCH
                if (!string.IsNullOrEmpty(m_Search) &&
                    !msg.Message.Contains(m_Search))
                {
                    continue;
                }

                var currentArea = GUILayoutUtility.GetRect(10, 36);

                EditorGUI.HelpBox(currentArea, msg.Message, GetType(msg.Level));

                // Selection:
                if (m_SelectedLogItem == i)
                {
                    EditorGUI.DrawRect(currentArea, c_ItemSelect);
                }
                else if (currentArea.Contains(e.mousePosition))
                {
                    if (m_MouseDownOn > -1)
                        EditorGUI.DrawRect(currentArea, c_ItemPress);
                    else
                        EditorGUI.DrawRect(currentArea, c_ItemHover);

                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        m_MouseDownOn = i;
                        hasBegunSelecting = true;
                    }
                    else if (m_MouseDownOn == i &&
                        e.type == EventType.MouseUp && e.button == 0)
                    {
                        m_SelectedLogItem = i;
                        hasSelected = true;
                        m_MouseDownOn = -1;
                    }
                }

                GUILayout.Space(2f);

                static MessageType GetType(LogLevel lvl) => lvl switch
                {
                    LogLevel.Msg => MessageType.Info,
                    LogLevel.Wrn => MessageType.Warning,
                    LogLevel.Err => MessageType.Error,
                    _ => MessageType.None,
                };
            } // For

            if (!hasBegunSelecting
                && e.type == EventType.MouseUp
                && e.button == 0)
            {
                m_MouseDownOn = -1;
            }

            if (!hasSelected
                && e.type == EventType.MouseUp
                && e.button == 0)
            {
                m_SelectedLogItem = -1;
            }

            GUILayout.EndScrollView();

            #endregion

            #region FULL LOG VIEW
            const float DragSize = 8;

            var fullLogView = GUILayoutUtility.GetRect(20, m_FullLogHeight);
            var fullLogDragArea = new Rect(fullLogView)
            {
                y = fullLogView.y - (DragSize / 2),
                height = DragSize,
            };

            var fullLogStyle = new GUIStyle("Label")
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
            };

            if (m_SelectedLogItem >= 0)
            {
                GUI.TextArea(
                    fullLogView.Shrink(0, 0, 5, 0),
                    log[m_SelectedLogItem].Message,
                    fullLogStyle
                );
            }

            // DRAGGING
            var fullWin = new Rect(0, 0, winPosition.width, winPosition.height);
            // Mouse Shape
            EditorGUIUtility.AddCursorRect(fullLogDragArea, MouseCursor.ResizeVertical);

            if (e.type == EventType.MouseDown // Enter Drag
                && e.button == 0
                && fullLogDragArea.Contains(e.mousePosition))
            {
                m_Dragging = true;
            }
            else if (m_Dragging && // Exit Drag
                    (e.type == EventType.MouseUp)
                || (!fullWin.Contains(e.mousePosition)))
            {
                m_Dragging = false;
            }
            else if (m_Dragging) // Move Area
            {
                m_FullLogHeight = (winPosition.height - e.mousePosition.y);
                
            }

            // Clamp Area
            m_FullLogHeight = Mathf.Clamp(m_FullLogHeight,
                70, winPosition.height - 120);

            #endregion
        }
    }
}
