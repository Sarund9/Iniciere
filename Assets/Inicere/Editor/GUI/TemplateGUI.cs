using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class TemplateGUI
    {
        Color color = Color.HSVToRGB(.7f, .3f, .1f);
        
        public void Draw(TemplateInfo info)
        {
            //var e = Event.current;
            //var area = GUILayoutUtility.GetRect(20, 50);

            //EditorGUI.DrawRect(
            //    new Rect(e.mousePosition, Vector2.one * 5f),
            //    Color.red);

            EditorGUI.EndDisabledGroup();

            GUILayout.Label(info.TmpName, "LargeLabel");
            GUILayout.Label(info.ShortDescription, "Label");

            //if (area.Contains(e.mousePosition))
            //{
            //    color = Color.HSVToRGB(.02f, .6f, .7f);
            //}
            //else
            //{
            //    color = Color.HSVToRGB(.7f, .3f, .1f);
            //}

            if (GUILayout.Button("Use this Template"))
            {
                UseTemplateWindow.OpenFrom(info);
            }

            EditorGUI.BeginDisabledGroup(true);
        }

    }
}
