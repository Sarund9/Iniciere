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

            bool value = EditorGUI.ToggleLeft(area,
                editorName ?? property.Name,
                (bool)property.Value);

            if (EditorGUI.EndChangeCheck())
            {
                property.Value = value;
            }
        }
    }
}
