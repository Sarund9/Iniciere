using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [CustomEditor(typeof(TemplateInfo))]
    public class TemplateInfoEditor : Editor
    {
        TemplateInfo obj;
        TemplateGUI ui = new TemplateGUI();

        private void OnEnable()
        {
            obj = (TemplateInfo)target;
        }

        public override void OnInspectorGUI()
        {
            //EditorGUI.EndDisabledGroup();
            base.OnInspectorGUI();
            //EditorGUI.BeginDisabledGroup(true);

            GUILayout.Space(5);
            ui.Draw(obj);
        }
    }
}