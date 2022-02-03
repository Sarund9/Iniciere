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

        private void OnEnable()
        {
            obj = (TemplateHeader)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            foreach (var template in obj.Templates)
            {
                TemplateGUI.Draw(template);
            }
        }
        
    }
}