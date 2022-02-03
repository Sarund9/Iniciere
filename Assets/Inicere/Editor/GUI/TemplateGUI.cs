using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public static class TemplateGUI
    {

        public static void Draw(TemplateInfo info)
        {
            GUILayout.Label(info.TmpName);

            GUILayout.TextArea(info.ShortDescription);
            GUILayout.TextArea(info.LongDescription);

        }

    }
}