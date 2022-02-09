using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public abstract class InicierePropertyEditor
    {
        public abstract void DrawGUI(Rect area, TemplateProperty property);
        public virtual float GetHeight(TemplateProperty property) => EditorGUIUtility.singleLineHeight;

        //public virtual float GetHeight(float? prevHeight, TemplateProperty property) =>
        //    prevHeight is null ?
        //        EditorGUIUtility.singleLineHeight
        //        : (float)prevHeight;
    }
}
