using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class PropertyToggleEditor : InicierePropertyEditor
    {
        [SerializeField]
        private string editorName;

        public PropertyToggleEditor(string editorName)
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
            bool value = EditorGUI.Toggle(it.Current,
                (bool)property.Value);

            if (EditorGUI.EndChangeCheck())
            {
                property.Value = value;
            }
        }
    }
}
