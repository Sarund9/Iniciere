using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Iniciere
{
    [CustomEditor(typeof(TemplateHeader))]
    public class TemplateHeaderEditor : Editor
    {
        TemplateHeader obj;
        Dictionary<string, TemplateGUI> uis
            = new Dictionary<string, TemplateGUI>();
        
        private void OnEnable()
        {
            obj = (TemplateHeader)target;
        }

        private void OnDisable()
        {
            uis.Clear();
        }

        public override void OnInspectorGUI()
        {
            
            foreach (var item in obj.Templates)
            {
                if (string.IsNullOrEmpty(item.TmpName)) {
                    continue;
                }
                if (!uis.TryGetValue(item.TmpName, out var ui)) {
                    ui = new TemplateGUI();
                    uis.Add(item.TmpName, ui);
                }
                ui.Draw(item);
                GUILayout.Space(10f);
            }
        }
        
        
    }
}