using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateGUI
    {
        TemplateLogGUI m_LogUI = new TemplateLogGUI(); 

        public void Draw(TemplateInfo info)
        {
            EditorGUI.EndDisabledGroup();

            GUILayout.Label(info.TmpName, "LargeLabel");
            GUILayout.Label(info.ShortDescription, "Label");

            m_LogUI.Draw(info.PrecompileLog, info.TmpName);

            // TODO: Log Window
            GUILayout.Space(10f);

            EditorGUI.BeginDisabledGroup(info.IsFailed);

            if (GUILayout.Button("Use this Template"))
            {
                UseTemplateWindow.OpenFrom(info);
            }

            EditorGUI.EndDisabledGroup();

            //// TODO: Recompile One Template (Mutate Import ??)
            //if (GUILayout.Button("Recompile"))
            //{
            //    //UseTemplateWindow.OpenFrom(info);
            //}

            EditorGUI.BeginDisabledGroup(true);
        }

    }
}
