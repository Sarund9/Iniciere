using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TextEditor : InicierePropertyEditor
    {
        [SerializeField]
        private string editorName;

        public TextEditor(string editorName)
        {
            this.editorName = editorName;
        }

        public override void DrawGUI(Rect area, TemplateProperty property)
        {
            EditorGUI.BeginChangeCheck();

            var it = area.SplitHorizontal(2).GetEnumerator();
            it.MoveNext();

            GUI.Label(it.Current, editorName ?? property.Name);

            it.MoveNext();
            var str = property.Value is null ? "" : property.Value.ToString();

            str = EditorGUI.TextField(it.Current, str);

            if (EditorGUI.EndChangeCheck())
            {
                property.Value = str;
            }
        }
    }
}
