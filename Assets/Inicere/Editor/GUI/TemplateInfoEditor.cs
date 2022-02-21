using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [CustomEditor(typeof(TemplateInfo))]
    public class TemplateInfoEditor : Editor
    {
        TemplateInfo obj;
        TemplateGUI ui = new TemplateGUI();

        readonly string[] tabs =
            new string[] { "Overview", "Debug" };
        int selected;

        private void OnEnable()
        {
            obj = (TemplateInfo)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.EndDisabledGroup();
            selected = GUILayout.Toolbar(selected, tabs);
            EditorGUI.BeginDisabledGroup(true);

            GUILayout.Space(5);
            if (selected == 0)
            {
                ui.Draw(obj);
            }
            else
            {
                base.OnInspectorGUI();
            }
            
        }
    }
}