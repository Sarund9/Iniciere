using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class OptTextEditor : InicierePropertyEditor
    {
        public override void DrawGUI(Rect area, TemplateProperty property)
        {
            EditorGUI.BeginChangeCheck();
            //Rect text = area.Shrink(0, 10, 0, 0);
            var str = EditorGUI.TextField(area, property.Name, property.Value.ToString());
            if (EditorGUI.EndChangeCheck())
            {
                property.Value = str;
            }

            EditorGUI.BeginChangeCheck();
            Rect toggle = area.Shrink(area.width - 10, 0, 0, 0);
            toggle.x += area.width - 10;

            if (EditorGUI.EndChangeCheck())
            {
                property.Value = str;
            }
        }
    }
}
