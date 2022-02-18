﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateGUI
    {
        TemplateLogGUI m_LogUI = new TemplateLogGUI(); 

        public void Draw(TemplateInfo info)
        {
            EditorGUI.EndDisabledGroup();

            GUILayout.Label(info.TmpName, "LargeLabel");
            GUILayout.Label(info.ShortDescription, "Label");

            m_LogUI.Draw(info.PrecompileLog);

            // TODO: Log Window
            GUILayout.Space(10f);

            EditorGUI.BeginDisabledGroup(info.IsFailed);

            if (GUILayout.Button("Use this Template"))
            {
                UseTemplateWindow.OpenFrom(info);
            }

            EditorGUI.EndDisabledGroup();
            


            EditorGUI.BeginDisabledGroup(true);
        }

    }

    [Serializable]
    public class TemplateLogGUI
    {
        const float LOG_LENGHT = 203;
        
        bool m_Show;
        Vector2 m_Scroll;

        bool m_ShowMsg, m_ShowWrn, m_ShowErr;
        int m_NumMsg, m_NumWrn, m_NumErr;

        void UpdateLogNums(List<LogEntry> log)
        {
            m_NumMsg = 0; m_NumWrn = 0; m_NumErr = 0;
            foreach (var item in log)
            {
                switch (item.Level)
                {
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

        public void Draw(List<LogEntry> log)
        {
            UpdateLogNums(log);

            GUILayout.BeginHorizontal();
            
            var logHeaderArea = GUILayoutUtility
                .GetRect(1, EditorGUIUtility.singleLineHeight,
                GUILayout.MaxWidth(9999));
            var fullArea = new Rect(logHeaderArea)
            {
                width = Screen.width,
            };

            GUILayout.FlexibleSpace();
            var r_Toolbar = GUILayoutUtility
                .GetRect(100, EditorGUIUtility.singleLineHeight);

            var btn_OpenLog = GUILayoutUtility
                .GetRect(80, EditorGUIUtility.singleLineHeight);

            GUILayout.EndHorizontal();
            
            var toolbarStyle = new GUIStyle("toolbar")
            {

            };

            GUI.Box(fullArea, "", toolbarStyle);

            var toolbarDropStyle = new GUIStyle("toolbarDropDown")
            {
                fontStyle = FontStyle.Bold,
                //border = new RectOffset(20, 1, 1, 1),
                margin = new RectOffset(20, 0, 0, 0),
            };

            m_Show = EditorGUI
                .BeginFoldoutHeaderGroup(
                logHeaderArea.Shrink(9, 14, 0, 0), m_Show, "Log", toolbarDropStyle
                );

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
            
            if (GUI.Button(btn_OpenLog, "Open in Log", toolbarBtnStyle))
            {

                return;
            }

            if (m_Show)
            {
                var boxStyle = new GUIStyle("Box")
                {
                    margin = new RectOffset(0, 0, 4, 0),
                };
                m_Scroll = GUILayout.BeginScrollView(
                    m_Scroll, boxStyle, GUILayout.Height(LOG_LENGHT)
                    );

                foreach (var msg in log)
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
            }
            
            EditorGUI.EndFoldoutHeaderGroup();
        }
    }
}
