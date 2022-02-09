using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TextEditor : InicierePropertyEditor
    {
        public override void DrawGUI(Rect area, TemplateProperty property)
        {
            EditorGUI.BeginChangeCheck();

            var str = EditorGUI.TextField(area, property.Name, property.Value.ToString());

            if (EditorGUI.EndChangeCheck())
            {
                property.Value = str;
            }
        }
    }
}
