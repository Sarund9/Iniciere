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
        TemplateGUI ui = new TemplateGUI();
        
        private void OnEnable()
        {
            obj = (TemplateHeader)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            foreach (var item in obj.Templates)
            {
                ui.Draw(item);
                GUILayout.Space(10f);
            }
        }
        
        
    }
}