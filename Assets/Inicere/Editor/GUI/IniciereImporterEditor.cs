using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Iniciere
{
    //[CustomEditor(typeof(IniciereFileImporter))]
    public class IniciereImporterEditor : Editor
    {
        IniciereFileImporter obj;

        string[] tabs = new string[] { "Import", "Debug" };
        int selected = 1;

        private void OnEnable()
        {
            obj = (IniciereFileImporter)target;
        }

        public override void OnInspectorGUI()
        {
            //selected = GUILayout.Toolbar(selected, tabs);

            //if (selected == 0)
            //{

            //}
            //else
            //{
                
            //}
            base.OnInspectorGUI();
        }
    }
}