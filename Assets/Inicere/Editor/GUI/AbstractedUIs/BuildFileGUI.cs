using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class BuildFileGUI
    {
        Vector2 m_Scroll;
        string m_FileName;


        public bool Draw(TemplateInfo info, Action closeWindowCallback, bool keepOpenOption = false)
        {
            if (!info) {
                //closeWindowCallback();
                return false;
            }

            PropertyUI(info);

            BottomUI(info, closeWindowCallback, keepOpenOption);

            return true;
        }

        static void TagDisplay(Rect rect, List<string> tags, string title)
        {
            StringBuilder str = new StringBuilder("\n");

            foreach (var tag in tags)
            {
                str.Append($"{tag}, ");
            }
            if (str.Length > 2)
                str.Remove(str.Length - 2, 2);
            else
                str.Append("none");

            float lineHeight = EditorGUIUtility.singleLineHeight;

            var boxstyle = new GUIStyle("HelpBox")
            {
                fontSize = 12,
            };

            GUI.Label(rect, new GUIContent(str.ToString()), boxstyle);

            var titlestyle = new GUIStyle("Toolbar")
            {
                fontSize = 12,
                fixedHeight = lineHeight,
                fontStyle = FontStyle.Bold,
            };

            var titleRect = rect
                .Shrink(0, 0, 0, rect.height - lineHeight);

            GUI.Label(titleRect, title, titlestyle);
        }

        void PropertyUI(TemplateInfo info)
        {

            GUILayout.Label(info.TmpName, "LargeLabel");
            GUILayout.Label(info.LongDescription);

            #region TAGS_UI
            Rect baseRect = GUILayoutUtility.GetRect(10, 36);
            Rect[] rects = baseRect
                .SplitHorizontal(4)
                .Select(r => r.Shrink(3, 3, 0, 0))
                .ToArray();

            TagDisplay(rects[0], info.FileExts, "File Exts");
            TagDisplay(rects[1], info.Langs, "Language");
            TagDisplay(rects[2], info.Categories, "Categories");
            TagDisplay(rects[3], info.Flags, "Flags");
            #endregion

            GUILayout.Space(4f);

            m_Scroll = GUILayout.BeginScrollView(m_Scroll, "Box");

            var list = info.Properties;
            if (list.Count == 0)
            {
                GUILayout.Label("This template has no Properties");
            }

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsFileName)
                    continue;
                if (list[i].HasEditor)
                    HandleProperty(list[i]);
                else // Debug
                    GUILayout.Label(list[i].Name + " - No Editor");

                // ============= \\
                static void HandleProperty(TemplateProperty templateProperty)
                {
                    float height = templateProperty.Editor.GetHeight(templateProperty);

                    var rect = GUILayoutUtility.GetRect(0, height);

                    templateProperty.Editor.DrawGUI(rect, templateProperty);
                }
            }
            //GUILayout.FlexibleSpace();
            GUILayout.Space(Screen.height - 240f);

            GUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(info);
            }
        }

        void BottomUI(TemplateInfo info, Action closeWindowCallback, bool keepOpenOption)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();

            var fileNameProperty = info.FileNameProperty;
            if (fileNameProperty != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("File Name");
                var str = EditorGUILayout.TextField(m_FileName);
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    fileNameProperty.Value = str;
                    info.FileNameProperty.Value = str;
                    m_FileName = str;
                }
            }

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_FileName));

            GUILayout.BeginHorizontal();
            if (keepOpenOption && GUILayout.Button("Create"))
            {
                var path = Extensions.GetPathToProjectWindowFolder();
                ScriptBuilder.CreateScript(info, path);
            }
            if (GUILayout.Button("Create & Close"))
            {
                var path = Extensions.GetPathToProjectWindowFolder();
                ScriptBuilder.CreateScript(info, path);
                closeWindowCallback();
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

    }
}
