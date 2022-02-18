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
        [SerializeField]
        string m_Path;
        
        Vector2 m_Scroll;
        string m_FileName;

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
            if (!m_Info) {
                Close();
                return;
            }

            PropertyUI();
            
            BottomUI();
        }

        private void OnDisable()
        {
            DestroyImmediate(m_Info);
        }

        void PropertyUI()
        {
            
            GUILayout.Label(m_Info.TmpName, "LargeLabel");
            GUILayout.Label(m_Info.LongDescription);

            #region TAGS_UI
            Rect baseRect = GUILayoutUtility.GetRect(10, 36);
            Rect[] rects = baseRect
                .SplitHorizontal(4)
                .Select(r => r.Shrink(3, 3, 0, 0))
                .ToArray();

            TagDisplay(rects[0], m_Info.FileExts, "File Exts");
            TagDisplay(rects[1], m_Info.Langs, "Language");
            TagDisplay(rects[2], m_Info.Categories, "Categories");
            TagDisplay(rects[3], m_Info.Flags, "Flags");
            #endregion

            GUILayout.Space(4f);

            m_Scroll = GUILayout.BeginScrollView(m_Scroll, "Box");

            var list = m_Info.Properties;
            if (list.Count == 0)
            {
                GUILayout.Label("This template has no Properties");
            }

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

            
        }

        void BottomUI()
        {
            GUILayout.FlexibleSpace();
            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();

            var fileNameProperty = m_Info.FileNameProperty;
            if (fileNameProperty != null)
            {
                var str = EditorGUILayout.TextField("File Name", m_FileName);

                if (EditorGUI.EndChangeCheck())
                {
                    fileNameProperty.Value = str;
                    m_Info.FileNameProperty.Value = str;
                    m_FileName = str;
                }
            }

            //if (GUILayout.Button("TEST"))
            //{
            //    Debug.Log($"{m_Info.FileNameProperty.Value}");
            //}

            using (var _ = new GUILayout.HorizontalScope())
            {
                //m_Path = GUILayout.TextArea(m_Path, GUILayout.Height(18));
                //if (GUILayout.Button("...", GUILayout.Width(26)))
                //{
                //    var path = EditorUtility.SaveFilePanel(
                //        "Create Script",
                //        null,
                //        Path.GetFileNameWithoutExtension(m_Path),
                //        "");
                //}
            }
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_FileName));

            if (GUILayout.Button("Create"))
            {
                var path = Extensions.GetPathToProjectWindowFolder();
                ScriptBuilder.CreateScript(m_Info, path);
                Close();
            }
            EditorGUI.EndDisabledGroup();
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
            int numItems = tags.Count;

            float height = lineHeight * numItems;

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
    }
}