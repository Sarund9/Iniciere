using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class PropertyNamespaceEditor : InicierePropertyEditor
    {
        [SerializeField]
        private bool isEditor;

        int selected = 0;
        bool opened;

        public PropertyNamespaceEditor(bool isEditor)
        {
            this.isEditor = isEditor;
        }

        public override void DrawGUI(Rect area, TemplateProperty property)
        {
            //var cfg = IniciereConfig.Instance;
            //var list = isEditor ? cfg.ProjectEditorNamespaces : cfg.ProjectNamespaces;
            //var title = isEditor ? "EditorNamespace" : "Namespace";

            //var it = area.SplitHorizontal(2).GetEnumerator();
            //it.MoveNext();

            //GUI.Label(it.Current, title);

            //it.MoveNext();
            //if (list.Count == 0)
            //{
            //    GUI.Label(it.Current, $"ERR, No namespaces in config, Script will fail!");
            //    return;
            //}
            
            //EditorGUI.BeginChangeCheck();

            //var style = new GUIStyle("DropdownButton")
            //{

            //};

            //if (GUI.Button(it.Current, list[selected], style))
            //{
            //    ContextDropdown(it.Current, new Vector2(160, 360),
            //        title, list, OnItemSelected,
            //        ref selected, ref opened);
            //};

            //if (EditorGUI.EndChangeCheck())
            //{
            //    //property.Value = value;
            //}
        }

        private void OnItemSelected(int index)
        {
            selected = index;
        }

        public static void ContextDropdown(
            Rect buttonRect, Vector2 windowSize,
            string title, IEnumerable<string> options,
            Action<int> callback,
            ref int selected, ref bool opened)
        {
            if (GUI.Button(buttonRect, title))
            {
                opened = true;
            }
            

            var position = new Rect
            {
                x = buttonRect.xMin,
                y = buttonRect.yMax,
                size = windowSize,
            };

            DoDropdown(position, title, options, callback);

        }

        private static void DoDropdown(Rect position, string title, IEnumerable<string> options, Action<int> callback)
        {
            GUI.Window(0, position, id =>
            {
                var style = new GUIStyle("ToolbarButton")
                {

                };

                int i = 0;
                foreach (var item in options)
                {
                    if (GUILayout.Button(item, style))
                    {
                        callback(i);
                        return;
                    }
                    i++;
                }
            },
            title);
        }

    }
}
